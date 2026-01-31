using UnityEngine;
using System;
using System.Collections.Generic;
using EscapeTrainRun.Events;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Tracks game statistics and triggers achievement checks.
    /// Bridges gameplay events to achievement service.
    /// </summary>
    public class AchievementTracker : MonoBehaviour
    {
        [Header("UI Integration")]
        [SerializeField] private bool showUnlockNotifications = true;
        [SerializeField] private float notificationDuration = 3f;

        // Session stats for special achievements
        private int consecutiveJumps;
        private int consecutiveSlides;
        private int coinsWithoutMissing;
        private float maxComboMultiplier;
        private bool hasCrashedThisRun;
        private float runStartTime;

        // Events for UI
        public static event Action<AchievementDefinition> OnShowAchievementPopup;

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
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnPlayerJumped += HandleJump;
            GameEvents.OnPlayerSlide += HandleSlide;
            GameEvents.OnPlayerCrashed += HandleCrash;
            GameEvents.OnLaneChanged += HandleLaneChange;
            AchievementService.OnAchievementUnlocked += HandleAchievementUnlocked;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnPlayerJumped -= HandleJump;
            GameEvents.OnPlayerSlide -= HandleSlide;
            GameEvents.OnPlayerCrashed -= HandleCrash;
            GameEvents.OnLaneChanged -= HandleLaneChange;
            AchievementService.OnAchievementUnlocked -= HandleAchievementUnlocked;
        }

        #region Event Handlers

        private void HandleGameStarted()
        {
            // Reset session tracking
            consecutiveJumps = 0;
            consecutiveSlides = 0;
            coinsWithoutMissing = 0;
            maxComboMultiplier = 1f;
            hasCrashedThisRun = false;
            runStartTime = Time.time;
        }

        private void HandleGameOver(GameOverData data)
        {
            var achievementService = AchievementService.Instance;
            if (achievementService == null) return;

            float runDuration = Time.time - runStartTime;

            // Check special achievements based on session data
            if (!hasCrashedThisRun && data.distance >= 500)
            {
                achievementService.UnlockAchievement("flawless_500m");
            }

            if (!hasCrashedThisRun && data.distance >= 1000)
            {
                achievementService.UnlockAchievement("flawless_1km");
            }

            // Speed run achievement
            if (runDuration < 60 && data.score >= 10000)
            {
                achievementService.UnlockAchievement("speed_scorer");
            }

            // Multiplier achievements
            if (maxComboMultiplier >= 5)
            {
                achievementService.UnlockAchievement("combo_king_5x");
            }
            if (maxComboMultiplier >= 10)
            {
                achievementService.UnlockAchievement("combo_king_10x");
            }

            // First game achievement
            achievementService.UnlockAchievement("first_run");
        }

        private void HandleJump()
        {
            consecutiveJumps++;
            consecutiveSlides = 0;

            var achievementService = AchievementService.Instance;
            if (achievementService == null) return;

            // Consecutive jumps achievement
            if (consecutiveJumps >= 5)
            {
                achievementService.UnlockAchievement("jump_streak_5");
            }
            if (consecutiveJumps >= 10)
            {
                achievementService.UnlockAchievement("jump_streak_10");
            }
        }

        private void HandleSlide()
        {
            consecutiveSlides++;
            consecutiveJumps = 0;

            var achievementService = AchievementService.Instance;
            if (achievementService == null) return;

            // Consecutive slides achievement
            if (consecutiveSlides >= 5)
            {
                achievementService.UnlockAchievement("slide_streak_5");
            }
            if (consecutiveSlides >= 10)
            {
                achievementService.UnlockAchievement("slide_streak_10");
            }
        }

        private void HandleCrash()
        {
            hasCrashedThisRun = true;
            consecutiveJumps = 0;
            consecutiveSlides = 0;
        }

        private void HandleLaneChange(int lane)
        {
            // Reset streaks on lane change
            consecutiveJumps = 0;
            consecutiveSlides = 0;
        }

        private void HandleAchievementUnlocked(AchievementDefinition achievement)
        {
            if (showUnlockNotifications)
            {
                OnShowAchievementPopup?.Invoke(achievement);
            }

            Debug.Log($"[AchievementTracker] Achievement unlocked: {achievement.displayName}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the max combo multiplier for tracking.
        /// </summary>
        public void UpdateComboMultiplier(float multiplier)
        {
            maxComboMultiplier = Mathf.Max(maxComboMultiplier, multiplier);
        }

        /// <summary>
        /// Reports coin collection for streak tracking.
        /// </summary>
        public void ReportCoinCollected()
        {
            coinsWithoutMissing++;

            var achievementService = AchievementService.Instance;
            if (achievementService == null) return;

            // Coin streak achievements
            if (coinsWithoutMissing >= 50)
            {
                achievementService.UnlockAchievement("coin_streak_50");
            }
            if (coinsWithoutMissing >= 100)
            {
                achievementService.UnlockAchievement("coin_streak_100");
            }
        }

        /// <summary>
        /// Reports missing a coin (resets streak).
        /// </summary>
        public void ReportCoinMissed()
        {
            coinsWithoutMissing = 0;
        }

        #endregion
    }
}
