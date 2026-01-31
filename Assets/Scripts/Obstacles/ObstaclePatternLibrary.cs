using UnityEngine;
using System.Collections.Generic;
using EscapeTrainRun.Core;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// ScriptableObject containing a library of obstacle patterns.
    /// </summary>
    [CreateAssetMenu(fileName = "ObstaclePatternLibrary", menuName = "EscapeTrainRun/Obstacles/Pattern Library")]
    public class ObstaclePatternLibrary : ScriptableObject
    {
        [Header("Patterns")]
        public ObstaclePattern[] easyPatterns;
        public ObstaclePattern[] mediumPatterns;
        public ObstaclePattern[] hardPatterns;
        public ObstaclePattern[] expertPatterns;

        [Header("Selection Weights")]
        [Range(0f, 1f)]
        public float easyWeight = 0.4f;
        [Range(0f, 1f)]
        public float mediumWeight = 0.3f;
        [Range(0f, 1f)]
        public float hardWeight = 0.2f;
        [Range(0f, 1f)]
        public float expertWeight = 0.1f;

        /// <summary>
        /// Gets a random pattern based on current difficulty.
        /// </summary>
        public ObstaclePattern GetRandomPattern(float difficulty, ThemeType theme)
        {
            // Adjust weights based on difficulty
            float adjustedEasyWeight = easyWeight * (1f - difficulty);
            float adjustedMediumWeight = mediumWeight;
            float adjustedHardWeight = hardWeight * difficulty;
            float adjustedExpertWeight = expertWeight * difficulty * difficulty;

            float totalWeight = adjustedEasyWeight + adjustedMediumWeight + adjustedHardWeight + adjustedExpertWeight;
            float roll = Random.value * totalWeight;

            ObstaclePattern[] selectedArray;

            if (roll < adjustedEasyWeight)
            {
                selectedArray = easyPatterns;
            }
            else if (roll < adjustedEasyWeight + adjustedMediumWeight)
            {
                selectedArray = mediumPatterns;
            }
            else if (roll < adjustedEasyWeight + adjustedMediumWeight + adjustedHardWeight)
            {
                selectedArray = hardPatterns;
            }
            else
            {
                selectedArray = expertPatterns;
            }

            // Find valid patterns for theme
            var validPatterns = GetValidPatterns(selectedArray, theme, difficulty);

            if (validPatterns.Count == 0)
            {
                // Fallback to easy patterns
                validPatterns = GetValidPatterns(easyPatterns, theme, difficulty);
            }

            if (validPatterns.Count > 0)
            {
                return validPatterns[Random.Range(0, validPatterns.Count)];
            }

            return null;
        }

        /// <summary>
        /// Gets all patterns for a specific difficulty.
        /// </summary>
        public ObstaclePattern[] GetPatternsByDifficulty(PatternDifficulty difficulty)
        {
            return difficulty switch
            {
                PatternDifficulty.Easy => easyPatterns,
                PatternDifficulty.Medium => mediumPatterns,
                PatternDifficulty.Hard => hardPatterns,
                PatternDifficulty.Expert => expertPatterns,
                _ => easyPatterns
            };
        }

        /// <summary>
        /// Gets all patterns valid for a theme.
        /// </summary>
        public List<ObstaclePattern> GetPatternsForTheme(ThemeType theme)
        {
            var result = new List<ObstaclePattern>();

            AddValidPatterns(result, easyPatterns, theme);
            AddValidPatterns(result, mediumPatterns, theme);
            AddValidPatterns(result, hardPatterns, theme);
            AddValidPatterns(result, expertPatterns, theme);

            return result;
        }

        private List<ObstaclePattern> GetValidPatterns(ObstaclePattern[] patterns, ThemeType theme, float difficulty)
        {
            var valid = new List<ObstaclePattern>();

            if (patterns == null) return valid;

            foreach (var pattern in patterns)
            {
                if (pattern != null && 
                    pattern.IsValidForTheme(theme) && 
                    pattern.IsValidForDifficulty(difficulty))
                {
                    valid.Add(pattern);
                }
            }

            return valid;
        }

        private void AddValidPatterns(List<ObstaclePattern> list, ObstaclePattern[] patterns, ThemeType theme)
        {
            if (patterns == null) return;

            foreach (var pattern in patterns)
            {
                if (pattern != null && pattern.IsValidForTheme(theme))
                {
                    list.Add(pattern);
                }
            }
        }
    }
}
