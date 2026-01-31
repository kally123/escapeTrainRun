using UnityEngine;
using System;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// ScriptableObject defining a pattern of obstacles to spawn together.
    /// </summary>
    [CreateAssetMenu(fileName = "ObstaclePattern", menuName = "EscapeTrainRun/Obstacles/Pattern")]
    public class ObstaclePattern : ScriptableObject
    {
        [Header("Pattern Info")]
        public string patternName;
        public string description;
        public PatternDifficulty difficulty = PatternDifficulty.Medium;

        [Header("Requirements")]
        [Tooltip("Minimum difficulty level (0-1) for this pattern to appear")]
        public float minDifficultyLevel = 0f;
        [Tooltip("Themes where this pattern can appear")]
        public ThemeType[] allowedThemes;

        [Header("Obstacles")]
        public PatternObstacle[] obstacles;

        [Header("Timing")]
        [Tooltip("Time between this pattern and the next spawn")]
        public float cooldownAfterPattern = 2f;

        /// <summary>
        /// Gets the total duration of the pattern.
        /// </summary>
        public float TotalDuration
        {
            get
            {
                if (obstacles == null || obstacles.Length == 0) return 0f;

                float maxDelay = 0f;
                foreach (var obs in obstacles)
                {
                    maxDelay = Mathf.Max(maxDelay, obs.delay);
                }
                return maxDelay;
            }
        }

        /// <summary>
        /// Checks if this pattern is valid for the given theme.
        /// </summary>
        public bool IsValidForTheme(ThemeType theme)
        {
            if (allowedThemes == null || allowedThemes.Length == 0) return true;

            foreach (var t in allowedThemes)
            {
                if (t == theme) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if this pattern is valid for the current difficulty.
        /// </summary>
        public bool IsValidForDifficulty(float currentDifficulty)
        {
            return currentDifficulty >= minDifficultyLevel;
        }
    }

    /// <summary>
    /// Defines a single obstacle within a pattern.
    /// </summary>
    [Serializable]
    public class PatternObstacle
    {
        [Tooltip("Type of obstacle to spawn")]
        public ObstacleType type = ObstacleType.Static;

        [Tooltip("Lane to spawn in (0 = left, 1 = center, 2 = right)")]
        [Range(0, 2)]
        public int lane = 1;

        [Tooltip("Delay from pattern start to spawn this obstacle")]
        public float delay = 0f;

        [Tooltip("Offset from standard spawn distance")]
        public float distanceOffset = 0f;

        [Tooltip("Height offset for this obstacle")]
        public float heightOffset = 0f;
    }

    /// <summary>
    /// Difficulty rating for patterns.
    /// </summary>
    public enum PatternDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
}
