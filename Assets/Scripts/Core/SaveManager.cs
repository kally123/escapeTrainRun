using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Handles all save/load operations for game progress.
    /// Uses JSON serialization for local saves with optional cloud sync.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 60f;

        private GameSaveData currentSaveData;
        private string saveFilePath;
        private float lastAutoSaveTime;

        public GameSaveData CurrentSave => currentSaveData;

        private void Awake()
        {
            InitializeSingleton();
            RegisterServices();
            InitializeSavePath();
            LoadGameData();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        private void InitializeSavePath()
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            Debug.Log($"[SaveManager] Save path: {saveFilePath}");
        }

        private void Update()
        {
            if (autoSave && Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                SaveGameData();
                lastAutoSaveTime = Time.time;
            }
        }

        #region Save/Load Operations

        /// <summary>
        /// Loads game data from local storage.
        /// </summary>
        public void LoadGameData()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    string json = File.ReadAllText(saveFilePath);
                    currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                    Debug.Log("[SaveManager] Game data loaded successfully");
                }
                else
                {
                    currentSaveData = CreateNewSaveData();
                    Debug.Log("[SaveManager] No save file found, created new save data");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game data: {e.Message}");
                currentSaveData = CreateNewSaveData();
            }
        }

        /// <summary>
        /// Saves current game data to local storage.
        /// </summary>
        public void SaveGameData()
        {
            try
            {
                currentSaveData.lastSaveTime = DateTime.Now.ToString("o");
                string json = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log("[SaveManager] Game data saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game data: {e.Message}");
            }
        }

        /// <summary>
        /// Creates a new save data with default values.
        /// </summary>
        private GameSaveData CreateNewSaveData()
        {
            return new GameSaveData
            {
                playerId = Guid.NewGuid().ToString(),
                playerName = "Player",
                totalCoins = 0,
                totalCoinsCollected = 0,
                highScore = 0,
                highScoreTrain = 0,
                highScoreBus = 0,
                highScoreGround = 0,
                totalGamesPlayed = 0,
                totalDistanceRun = 0,
                currentCharacter = "Timmy",
                unlockedCharacters = new List<string> { "Timmy" },
                unlockedOutfits = new List<string>(),
                achievements = new List<string>(),
                dailyRewardStreak = 0,
                lastDailyRewardDate = "",
                lastSaveTime = DateTime.Now.ToString("o")
            };
        }

        #endregion

        #region Coins

        public void AddCoins(int amount)
        {
            currentSaveData.totalCoins += amount;
            currentSaveData.totalCoinsCollected += amount;
            SaveGameData();
        }

        public bool SpendCoins(int amount)
        {
            if (currentSaveData.totalCoins >= amount)
            {
                currentSaveData.totalCoins -= amount;
                SaveGameData();
                return true;
            }
            return false;
        }

        public int GetTotalCoins()
        {
            return currentSaveData.totalCoins;
        }

        #endregion

        #region High Scores

        public void UpdateHighScore(int score, Environment.ThemeType theme)
        {
            bool isNewHighScore = false;

            if (score > currentSaveData.highScore)
            {
                currentSaveData.highScore = score;
                isNewHighScore = true;
            }

            switch (theme)
            {
                case Environment.ThemeType.Train:
                    if (score > currentSaveData.highScoreTrain)
                    {
                        currentSaveData.highScoreTrain = score;
                        isNewHighScore = true;
                    }
                    break;
                case Environment.ThemeType.Bus:
                    if (score > currentSaveData.highScoreBus)
                    {
                        currentSaveData.highScoreBus = score;
                        isNewHighScore = true;
                    }
                    break;
                case Environment.ThemeType.Ground:
                    if (score > currentSaveData.highScoreGround)
                    {
                        currentSaveData.highScoreGround = score;
                        isNewHighScore = true;
                    }
                    break;
            }

            if (isNewHighScore)
            {
                SaveGameData();
            }
        }

        public int GetHighScore()
        {
            return currentSaveData.highScore;
        }

        public int GetHighScore(Environment.ThemeType theme)
        {
            return theme switch
            {
                Environment.ThemeType.Train => currentSaveData.highScoreTrain,
                Environment.ThemeType.Bus => currentSaveData.highScoreBus,
                Environment.ThemeType.Ground => currentSaveData.highScoreGround,
                _ => currentSaveData.highScore
            };
        }

        #endregion

        #region Characters

        public bool UnlockCharacter(string characterId)
        {
            if (!currentSaveData.unlockedCharacters.Contains(characterId))
            {
                currentSaveData.unlockedCharacters.Add(characterId);
                SaveGameData();
                return true;
            }
            return false;
        }

        public bool IsCharacterUnlocked(string characterId)
        {
            return currentSaveData.unlockedCharacters.Contains(characterId);
        }

        public void SetCurrentCharacter(string characterId)
        {
            if (IsCharacterUnlocked(characterId))
            {
                currentSaveData.currentCharacter = characterId;
                SaveGameData();
            }
        }

        public string GetCurrentCharacter()
        {
            return currentSaveData.currentCharacter;
        }

        public List<string> GetUnlockedCharacters()
        {
            return new List<string>(currentSaveData.unlockedCharacters);
        }

        #endregion

        #region Statistics

        /// <summary>Gets total coins currently available.</summary>
        public int TotalCoins => currentSaveData.totalCoins;

        /// <summary>Gets total coins ever collected.</summary>
        public int TotalCoinsCollected => currentSaveData.totalCoinsCollected;

        /// <summary>Gets total distance ever run.</summary>
        public float TotalDistanceRun => currentSaveData.totalDistanceRun;

        /// <summary>Gets total games played.</summary>
        public int GamesPlayed => currentSaveData.totalGamesPlayed;

        public void IncrementGamesPlayed()
        {
            currentSaveData.totalGamesPlayed++;
            SaveGameData();
        }

        public void AddDistanceRun(float distance)
        {
            currentSaveData.totalDistanceRun += distance;
            SaveGameData();
        }

        #endregion

        #region Daily Rewards

        public bool CanClaimDailyReward()
        {
            if (string.IsNullOrEmpty(currentSaveData.lastDailyRewardDate))
            {
                return true;
            }

            if (DateTime.TryParse(currentSaveData.lastDailyRewardDate, out DateTime lastClaim))
            {
                return DateTime.Now.Date > lastClaim.Date;
            }

            return true;
        }

        public int ClaimDailyReward()
        {
            if (!CanClaimDailyReward())
            {
                return 0;
            }

            // Check if streak is maintained (claimed yesterday)
            if (!string.IsNullOrEmpty(currentSaveData.lastDailyRewardDate))
            {
                if (DateTime.TryParse(currentSaveData.lastDailyRewardDate, out DateTime lastClaim))
                {
                    if ((DateTime.Now.Date - lastClaim.Date).Days > 1)
                    {
                        // Streak broken - reset
                        currentSaveData.dailyRewardStreak = 0;
                    }
                }
            }

            currentSaveData.dailyRewardStreak++;
            if (currentSaveData.dailyRewardStreak > 7)
            {
                currentSaveData.dailyRewardStreak = 1;
            }

            currentSaveData.lastDailyRewardDate = DateTime.Now.Date.ToString("o");

            // Calculate reward based on streak day
            int reward = currentSaveData.dailyRewardStreak switch
            {
                1 => 100,
                2 => 200,
                3 => 300,
                4 => 300,
                5 => 400,
                6 => 400,
                7 => 1000,
                _ => 100
            };

            AddCoins(reward);
            return reward;
        }

        public int GetDailyRewardStreak()
        {
            return currentSaveData.dailyRewardStreak;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets all save data to defaults. Use with caution!
        /// </summary>
        public void ResetAllData()
        {
            currentSaveData = CreateNewSaveData();
            SaveGameData();
            Debug.Log("[SaveManager] All data reset to defaults");
        }

        #endregion

        private void OnApplicationQuit()
        {
            SaveGameData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGameData();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// Serializable game save data structure.
    /// Contains all persistent player data.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public string playerId;
        public string playerName;
        public int totalCoins;
        public int totalCoinsCollected;
        public int highScore;
        public int highScoreTrain;
        public int highScoreBus;
        public int highScoreGround;
        public int totalGamesPlayed;
        public float totalDistanceRun;
        public string currentCharacter;
        public List<string> unlockedCharacters;
        public List<string> unlockedOutfits;
        public List<string> achievements;
        public int dailyRewardStreak;
        public string lastDailyRewardDate;
        public string lastSaveTime;
    }
}
