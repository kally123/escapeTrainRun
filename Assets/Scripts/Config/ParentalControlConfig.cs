using UnityEngine;

namespace EscapeTrainRun.Config
{
    /// <summary>
    /// Configuration for parental control features.
    /// Ensures COPPA compliance and parental oversight.
    /// </summary>
    [CreateAssetMenu(fileName = "ParentalControlConfig", menuName = "Escape Train Run/Config/Parental Controls")]
    public class ParentalControlConfig : ScriptableObject
    {
        [Header("Parental Gate")]
        [Tooltip("Enable parental gate for sensitive actions")]
        public bool enableParentalGate = true;

        [Tooltip("Type of parental gate challenge")]
        public ParentalGateType gateType = ParentalGateType.MathProblem;

        [Tooltip("Difficulty of math problems (for MathProblem type)")]
        public MathDifficulty mathDifficulty = MathDifficulty.Medium;

        [Header("Restricted Features")]
        [Tooltip("Require parental gate for external links")]
        public bool gateExternalLinks = true;

        [Tooltip("Require parental gate for in-app purchases")]
        public bool gateInAppPurchases = true;

        [Tooltip("Require parental gate for social features")]
        public bool gateSocialFeatures = true;

        [Tooltip("Require parental gate for settings")]
        public bool gateSettings = false;

        [Header("Limits")]
        [Tooltip("Enable play time limits")]
        public bool enablePlayTimeLimits = false;

        [Tooltip("Maximum daily play time in minutes (0 = unlimited)")]
        [Range(0, 480)]
        public int maxDailyPlayTimeMinutes = 0;

        [Tooltip("Enable break reminders")]
        public bool enableBreakReminders = true;

        [Tooltip("Remind to take a break after this many minutes")]
        [Range(15, 120)]
        public int breakReminderMinutes = 30;

        [Header("Content Settings")]
        [Tooltip("Enable sound effects")]
        public bool soundEffectsEnabled = true;

        [Tooltip("Enable music")]
        public bool musicEnabled = true;

        [Tooltip("Enable vibration (mobile)")]
        public bool vibrationEnabled = true;

        [Header("Network")]
        [Tooltip("Allow cloud save synchronization")]
        public bool allowCloudSave = true;

        [Tooltip("Allow leaderboard access")]
        public bool allowLeaderboards = true;

        [Tooltip("Allow analytics collection")]
        public bool allowAnalytics = true;

        /// <summary>
        /// Generates a random math problem for parental gate.
        /// </summary>
        public ParentalGateChallenge GenerateMathChallenge()
        {
            int num1, num2, answer;
            string operation;

            switch (mathDifficulty)
            {
                case MathDifficulty.Easy:
                    num1 = Random.Range(1, 10);
                    num2 = Random.Range(1, 10);
                    answer = num1 + num2;
                    operation = "+";
                    break;

                case MathDifficulty.Medium:
                    if (Random.value > 0.5f)
                    {
                        num1 = Random.Range(10, 30);
                        num2 = Random.Range(5, 20);
                        answer = num1 + num2;
                        operation = "+";
                    }
                    else
                    {
                        num1 = Random.Range(15, 30);
                        num2 = Random.Range(5, 15);
                        answer = num1 - num2;
                        operation = "-";
                    }
                    break;

                case MathDifficulty.Hard:
                default:
                    int type = Random.Range(0, 3);
                    if (type == 0)
                    {
                        num1 = Random.Range(20, 50);
                        num2 = Random.Range(15, 35);
                        answer = num1 + num2;
                        operation = "+";
                    }
                    else if (type == 1)
                    {
                        num1 = Random.Range(30, 60);
                        num2 = Random.Range(10, 30);
                        answer = num1 - num2;
                        operation = "-";
                    }
                    else
                    {
                        num1 = Random.Range(3, 12);
                        num2 = Random.Range(3, 9);
                        answer = num1 * num2;
                        operation = "×";
                    }
                    break;
            }

            return new ParentalGateChallenge
            {
                Question = $"What is {num1} {operation} {num2}?",
                Answer = answer
            };
        }
    }

    /// <summary>
    /// Types of parental gate challenges.
    /// </summary>
    public enum ParentalGateType
    {
        MathProblem,
        YearOfBirth,
        TextEntry
    }

    /// <summary>
    /// Difficulty levels for math problems.
    /// </summary>
    public enum MathDifficulty
    {
        Easy,   // Simple addition 1-10
        Medium, // Addition/subtraction up to 50
        Hard    // Includes multiplication
    }

    /// <summary>
    /// A parental gate challenge.
    /// </summary>
    [System.Serializable]
    public struct ParentalGateChallenge
    {
        public string Question;
        public int Answer;
    }
}
