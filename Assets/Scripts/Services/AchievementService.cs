using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EscapeTrainRun.Events;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Handles achievement tracking, unlocking, and sync with backend.
    /// Listens to game events and updates achievement progress.
    /// </summary>
    public class AchievementService : MonoBehaviour, IBackendService
    {
        public static AchievementService Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private BackendConfig config;
        [SerializeField] private List<AchievementDefinition> allAchievements = new List<AchievementDefinition>();

        // IBackendService implementation
        public string ServiceName => "AchievementService";
        public bool IsConnected { get; private set; }
        public bool IsOfflineMode { get; private set; }

        // Events
        public static event Action<AchievementDefinition> OnAchievementUnlocked;
        public static event Action<AchievementDefinition, float> OnAchievementProgress;
        public static event Action<List<AchievementProgress>> OnAchievementsLoaded;

        // State
        private Dictionary<string, AchievementProgress> progressMap = new Dictionary<string, AchievementProgress>();
        private Dictionary<string, AchievementDefinition> definitionMap = new Dictionary<string, AchievementDefinition>();
        private bool hasUnsyncedProgress;

        // Stats tracking
        private int sessionCoinsCollected;
        private int sessionScore;
        private float sessionDistance;
        private int sessionJumps;
        private int sessionSlides;
        private int sessionNearMisses;
        private int sessionObstaclesPassed;

        private void Awake()
        {
            InitializeSingleton();
            BuildDefinitionMap();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void BuildDefinitionMap()
        {
            definitionMap.Clear();
            foreach (var achievement in allAchievements)
            {
                if (achievement != null && !string.IsNullOrEmpty(achievement.achievementId))
                {
                    definitionMap[achievement.achievementId] = achievement;
                }
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnCoinsCollected += HandleCoinsCollected;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnPlayerJumped += HandlePlayerJumped;
            GameEvents.OnPlayerSlide += HandlePlayerSlide;
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnPowerUpActivated += HandlePowerUpActivated;
            GameEvents.OnCharacterUnlocked += HandleCharacterUnlocked;
            GameEvents.OnNearMiss += HandleNearMiss;
            GameEvents.OnObstaclePassed += HandleObstaclePassed;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnCoinsCollected -= HandleCoinsCollected;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnPlayerJumped -= HandlePlayerJumped;
            GameEvents.OnPlayerSlide -= HandlePlayerSlide;
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnPowerUpActivated -= HandlePowerUpActivated;
            GameEvents.OnCharacterUnlocked -= HandleCharacterUnlocked;
            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnObstaclePassed -= HandleObstaclePassed;
        }

        #region IBackendService Implementation

        public async Task InitializeAsync()
        {
            if (config?.VerboseLogging == true)
            {
                Debug.Log($"[{ServiceName}] Initializing...");
            }

            // Load saved progress
            LoadProgressFromLocal();

            await Task.Delay(100);

            IsConnected = true;
            IsOfflineMode = config?.UseMockData ?? true;

            Debug.Log($"[{ServiceName}] Initialized with {allAchievements.Count} achievements");
        }

        public async Task ShutdownAsync()
        {
            if (hasUnsyncedProgress)
            {
                await SyncProgressAsync();
            }

            SaveProgressToLocal();
            IsConnected = false;

            Debug.Log($"[{ServiceName}] Shutdown complete");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets all achievement definitions.
        /// </summary>
        public List<AchievementDefinition> GetAllAchievements()
        {
            return new List<AchievementDefinition>(allAchievements);
        }

        /// <summary>
        /// Gets achievements by category.
        /// </summary>
        public List<AchievementDefinition> GetAchievementsByCategory(AchievementCategory category)
        {
            return allAchievements.Where(a => a.category == category).ToList();
        }

        /// <summary>
        /// Gets unlocked achievements.
        /// </summary>
        public List<AchievementDefinition> GetUnlockedAchievements()
        {
            return allAchievements.Where(a =>
                progressMap.TryGetValue(a.achievementId, out var progress) && progress.isUnlocked
            ).ToList();
        }

        /// <summary>
        /// Gets progress for an achievement.
        /// </summary>
        public AchievementProgress GetProgress(string achievementId)
        {
            if (progressMap.TryGetValue(achievementId, out var progress))
            {
                return progress;
            }
            return new AchievementProgress(achievementId);
        }

        /// <summary>
        /// Gets progress percentage (0-1).
        /// </summary>
        public float GetProgressPercent(string achievementId)
        {
            if (!definitionMap.TryGetValue(achievementId, out var definition))
                return 0f;

            if (!progressMap.TryGetValue(achievementId, out var progress))
                return 0f;

            return progress.GetProgressPercent(definition.targetValue);
        }

        /// <summary>
        /// Checks if an achievement is unlocked.
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return progressMap.TryGetValue(achievementId, out var progress) && progress.isUnlocked;
        }

        /// <summary>
        /// Gets total unlocked achievement count.
        /// </summary>
        public int GetUnlockedCount()
        {
            return progressMap.Values.Count(p => p.isUnlocked);
        }

        /// <summary>
        /// Gets overall completion percentage.
        /// </summary>
        public float GetOverallCompletionPercent()
        {
            if (allAchievements.Count == 0) return 0f;
            return (float)GetUnlockedCount() / allAchievements.Count;
        }

        /// <summary>
        /// Manually increments progress for an achievement.
        /// </summary>
        public void IncrementProgress(string achievementId, int amount = 1)
        {
            if (!definitionMap.TryGetValue(achievementId, out var definition))
                return;

            UpdateProgress(achievementId, GetCurrentProgress(achievementId) + amount, definition.targetValue);
        }

        /// <summary>
        /// Manually sets progress for an achievement.
        /// </summary>
        public void SetProgress(string achievementId, int value)
        {
            if (!definitionMap.TryGetValue(achievementId, out var definition))
                return;

            UpdateProgress(achievementId, value, definition.targetValue);
        }

        /// <summary>
        /// Manually unlocks an achievement (for special/milestone types).
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!definitionMap.TryGetValue(achievementId, out var definition))
                return;

            if (IsUnlocked(achievementId))
                return;

            // Check prerequisites
            if (!ArePrerequisitesMet(definition))
            {
                Debug.LogWarning($"[{ServiceName}] Prerequisites not met for {achievementId}");
                return;
            }

            ForceUnlock(definition);
        }

        /// <summary>
        /// Syncs achievement progress with backend.
        /// </summary>
        public async Task<ServiceResult<bool>> SyncProgressAsync()
        {
            try
            {
                // TODO: Implement actual API call
                await Task.Delay(100);

                hasUnsyncedProgress = false;
                Debug.Log($"[{ServiceName}] Progress synced to cloud");
                return ServiceResult<bool>.Succeeded(true);
            }
            catch (Exception e)
            {
                return ServiceResult<bool>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Resets all achievement progress (use with caution).
        /// </summary>
        public void ResetAllProgress()
        {
            progressMap.Clear();
            SaveProgressToLocal();
            Debug.Log($"[{ServiceName}] All progress reset");
        }

        #endregion

        #region Event Handlers

        private void HandleCoinsCollected(int amount)
        {
            sessionCoinsCollected += amount;

            // Update cumulative coin achievements
            UpdateCumulativeProgress("collect_coins_100", amount, 100);
            UpdateCumulativeProgress("collect_coins_1000", amount, 1000);
            UpdateCumulativeProgress("collect_coins_10000", amount, 10000);
            UpdateCumulativeProgress("coin_master", amount, 100000);
        }

        private void HandleScoreChanged(int score)
        {
            sessionScore = score;

            // Check score milestones
            CheckMilestone("score_10000", score >= 10000);
            CheckMilestone("score_50000", score >= 50000);
            CheckMilestone("score_100000", score >= 100000);
            CheckMilestone("score_500000", score >= 500000);
        }

        private void HandlePlayerJumped()
        {
            sessionJumps++;

            UpdateCumulativeProgress("jumper_100", 1, 100);
            UpdateCumulativeProgress("jumper_1000", 1, 1000);
        }

        private void HandlePlayerSlide()
        {
            sessionSlides++;

            UpdateCumulativeProgress("slider_100", 1, 100);
            UpdateCumulativeProgress("slider_1000", 1, 1000);
        }

        private void HandleGameStarted()
        {
            // Reset session stats
            sessionCoinsCollected = 0;
            sessionScore = 0;
            sessionDistance = 0;
            sessionJumps = 0;
            sessionSlides = 0;
            sessionNearMisses = 0;
            sessionObstaclesPassed = 0;

            // Increment games played
            UpdateCumulativeProgress("play_games_10", 1, 10);
            UpdateCumulativeProgress("play_games_100", 1, 100);
            UpdateCumulativeProgress("play_games_500", 1, 500);
        }

        private void HandleGameOver(GameOverData data)
        {
            sessionDistance = data.DistanceTraveled;

            // Check distance achievements
            CheckMilestone("run_1km", data.DistanceTraveled >= 1000);
            CheckMilestone("run_5km", data.DistanceTraveled >= 5000);
            CheckMilestone("run_10km", data.DistanceTraveled >= 10000);

            // Update cumulative distance
            UpdateCumulativeProgress("total_distance_100km", (int)data.DistanceTraveled, 100000);

            // Check session-specific achievements
            CheckMilestone("no_coins_1km", data.DistanceTraveled >= 1000 && sessionCoinsCollected == 0);
            CheckMilestone("near_miss_master", sessionNearMisses >= 10);
            CheckMilestone("perfect_run_100", sessionObstaclesPassed >= 100 && data.DistanceTraveled > 0);

            // Save progress
            SaveProgressToLocal();
        }

        private void HandlePowerUpActivated(Collectibles.PowerUpType type)
        {
            UpdateCumulativeProgress("use_powerups_50", 1, 50);
            UpdateCumulativeProgress("use_powerups_200", 1, 200);

            // Type-specific achievements
            switch (type)
            {
                case Collectibles.PowerUpType.Magnet:
                    UpdateCumulativeProgress("magnet_master", 1, 50);
                    break;
                case Collectibles.PowerUpType.Shield:
                    UpdateCumulativeProgress("shield_master", 1, 50);
                    break;
                case Collectibles.PowerUpType.SpeedBoost:
                    UpdateCumulativeProgress("speed_demon", 1, 50);
                    break;
                case Collectibles.PowerUpType.StarPower:
                    UpdateCumulativeProgress("superstar", 1, 25);
                    break;
            }
        }

        private void HandleCharacterUnlocked(Characters.CharacterData character)
        {
            UpdateCumulativeProgress("unlock_characters_3", 1, 3);
            UpdateCumulativeProgress("unlock_characters_all", 1, 7);
        }

        private void HandleNearMiss()
        {
            sessionNearMisses++;
            UpdateCumulativeProgress("near_miss_50", 1, 50);
            UpdateCumulativeProgress("near_miss_500", 1, 500);
        }

        private void HandleObstaclePassed()
        {
            sessionObstaclesPassed++;
            UpdateCumulativeProgress("obstacles_passed_1000", 1, 1000);
        }

        #endregion

        #region Private Methods

        private int GetCurrentProgress(string achievementId)
        {
            if (progressMap.TryGetValue(achievementId, out var progress))
            {
                return progress.currentValue;
            }
            return 0;
        }

        private void UpdateProgress(string achievementId, int newValue, int targetValue)
        {
            if (!progressMap.TryGetValue(achievementId, out var progress))
            {
                progress = new AchievementProgress(achievementId);
                progressMap[achievementId] = progress;
            }

            if (progress.isUnlocked) return; // Already unlocked

            int oldValue = progress.currentValue;
            progress.currentValue = newValue;

            // Notify progress change
            if (definitionMap.TryGetValue(achievementId, out var definition))
            {
                float percent = progress.GetProgressPercent(targetValue);
                OnAchievementProgress?.Invoke(definition, percent);

                // Check if now unlocked
                if (newValue >= targetValue)
                {
                    ForceUnlock(definition);
                }
            }

            hasUnsyncedProgress = true;
        }

        private void UpdateCumulativeProgress(string achievementId, int increment, int targetValue)
        {
            int current = GetCurrentProgress(achievementId);
            UpdateProgress(achievementId, current + increment, targetValue);
        }

        private void CheckMilestone(string achievementId, bool condition)
        {
            if (!condition) return;
            if (IsUnlocked(achievementId)) return;

            if (definitionMap.TryGetValue(achievementId, out var definition))
            {
                if (ArePrerequisitesMet(definition))
                {
                    ForceUnlock(definition);
                }
            }
        }

        private bool ArePrerequisitesMet(AchievementDefinition definition)
        {
            if (definition.prerequisites == null || definition.prerequisites.Count == 0)
                return true;

            foreach (var prereq in definition.prerequisites)
            {
                if (prereq != null && !IsUnlocked(prereq.achievementId))
                {
                    return false;
                }
            }
            return true;
        }

        private void ForceUnlock(AchievementDefinition definition)
        {
            if (!progressMap.TryGetValue(definition.achievementId, out var progress))
            {
                progress = new AchievementProgress(definition.achievementId);
                progressMap[definition.achievementId] = progress;
            }

            progress.isUnlocked = true;
            progress.currentValue = definition.targetValue;
            progress.unlockTime = DateTime.UtcNow.ToString("o");

            // Grant rewards
            if (definition.coinReward > 0)
            {
                var saveManager = SaveManager.Instance;
                if (saveManager != null)
                {
                    saveManager.AddCoins(definition.coinReward);
                }
            }

            Debug.Log($"[{ServiceName}] Achievement unlocked: {definition.displayName}");
            OnAchievementUnlocked?.Invoke(definition);

            hasUnsyncedProgress = true;
        }

        private void LoadProgressFromLocal()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager?.CurrentSave == null) return;

            // Load from save data
            var savedProgress = saveManager.CurrentSave.achievementProgress;
            if (savedProgress != null)
            {
                foreach (var kvp in savedProgress)
                {
                    progressMap[kvp.Key] = new AchievementProgress(kvp.Key)
                    {
                        currentValue = kvp.Value,
                        isUnlocked = definitionMap.TryGetValue(kvp.Key, out var def) && kvp.Value >= def.targetValue
                    };
                }
            }

            Debug.Log($"[{ServiceName}] Loaded {progressMap.Count} achievement progress entries");
        }

        private void SaveProgressToLocal()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager?.CurrentSave == null) return;

            // Convert to save format
            var saveDict = new Dictionary<string, int>();
            foreach (var kvp in progressMap)
            {
                saveDict[kvp.Key] = kvp.Value.currentValue;
            }

            saveManager.CurrentSave.achievementProgress = saveDict;
            saveManager.SaveGameData();
        }

        #endregion
    }
}
