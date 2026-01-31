using UnityEngine;
using System;
using System.Collections.Generic;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Achievement category for organization.
    /// </summary>
    public enum AchievementCategory
    {
        Gameplay,
        Collection,
        Distance,
        Score,
        Character,
        PowerUp,
        Special
    }

    /// <summary>
    /// Achievement rarity tier.
    /// </summary>
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Achievement unlock type.
    /// </summary>
    public enum AchievementType
    {
        Progress,    // Track progress toward goal
        Milestone,   // Single action triggers
        Cumulative,  // Track total across sessions
        Challenge    // Special requirements
    }

    /// <summary>
    /// Achievement definition - ScriptableObject for editor configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "Achievement", menuName = "EscapeTrainRun/Achievement")]
    public class AchievementDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string achievementId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Classification")]
        public AchievementCategory category;
        public AchievementRarity rarity;
        public AchievementType type;

        [Header("Requirements")]
        [Tooltip("Target value to unlock (e.g., collect 1000 coins)")]
        public int targetValue = 1;

        [Tooltip("Whether progress persists across sessions")]
        public bool persistProgress = true;

        [Tooltip("Hidden until unlocked")]
        public bool isSecret = false;

        [Header("Rewards")]
        public int coinReward = 0;
        public string characterUnlockId;
        public string specialRewardId;

        [Header("Prerequisites")]
        [Tooltip("Achievements that must be unlocked first")]
        public List<AchievementDefinition> prerequisites = new List<AchievementDefinition>();
    }

    /// <summary>
    /// Runtime achievement progress data.
    /// </summary>
    [Serializable]
    public class AchievementProgress
    {
        public string achievementId;
        public int currentValue;
        public bool isUnlocked;
        public string unlockTime;

        public AchievementProgress(string id)
        {
            achievementId = id;
            currentValue = 0;
            isUnlocked = false;
        }

        public float GetProgressPercent(int targetValue)
        {
            if (targetValue <= 0) return isUnlocked ? 1f : 0f;
            return Mathf.Clamp01((float)currentValue / targetValue);
        }
    }

    /// <summary>
    /// Achievement unlock notification data.
    /// </summary>
    public class AchievementUnlockedEventArgs : EventArgs
    {
        public AchievementDefinition Achievement { get; set; }
        public int CoinReward { get; set; }
        public bool IsFirstTimeUnlock { get; set; }
    }
}
