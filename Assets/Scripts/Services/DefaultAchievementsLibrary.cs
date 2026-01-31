using UnityEngine;
using System.Collections.Generic;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Default achievement definitions library.
    /// ScriptableObject containing all game achievements.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultAchievements", menuName = "EscapeTrainRun/Default Achievements Library")]
    public class DefaultAchievementsLibrary : ScriptableObject
    {
        [Header("All Achievements")]
        public List<AchievementDefinition> achievements = new List<AchievementDefinition>();

        /// <summary>
        /// Creates default achievement definitions programmatically.
        /// Use in editor or for testing.
        /// </summary>
        public static List<AchievementData> GetDefaultAchievements()
        {
            return new List<AchievementData>
            {
                // Gameplay - Basic
                new AchievementData("first_run", "First Steps", "Complete your first run", AchievementCategory.Gameplay, AchievementRarity.Common, 1, 0),
                new AchievementData("play_games_10", "Getting Started", "Play 10 games", AchievementCategory.Gameplay, AchievementRarity.Common, 10, 100),
                new AchievementData("play_games_100", "Dedicated Runner", "Play 100 games", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 100, 500),
                new AchievementData("play_games_500", "Marathon Runner", "Play 500 games", AchievementCategory.Gameplay, AchievementRarity.Rare, 500, 2000),

                // Score
                new AchievementData("score_10000", "Score Seeker", "Score 10,000 points in one run", AchievementCategory.Score, AchievementRarity.Common, 10000, 100),
                new AchievementData("score_50000", "High Scorer", "Score 50,000 points in one run", AchievementCategory.Score, AchievementRarity.Uncommon, 50000, 300),
                new AchievementData("score_100000", "Score Master", "Score 100,000 points in one run", AchievementCategory.Score, AchievementRarity.Rare, 100000, 500),
                new AchievementData("score_500000", "Score Legend", "Score 500,000 points in one run", AchievementCategory.Score, AchievementRarity.Epic, 500000, 2000),

                // Distance
                new AchievementData("run_1km", "Short Sprint", "Run 1 kilometer in one run", AchievementCategory.Distance, AchievementRarity.Common, 1000, 100),
                new AchievementData("run_5km", "Distance Runner", "Run 5 kilometers in one run", AchievementCategory.Distance, AchievementRarity.Uncommon, 5000, 300),
                new AchievementData("run_10km", "Ultra Runner", "Run 10 kilometers in one run", AchievementCategory.Distance, AchievementRarity.Rare, 10000, 1000),
                new AchievementData("total_distance_100km", "World Traveler", "Run 100 kilometers total", AchievementCategory.Distance, AchievementRarity.Epic, 100000, 5000),

                // Collection - Coins
                new AchievementData("collect_coins_100", "Coin Collector", "Collect 100 coins", AchievementCategory.Collection, AchievementRarity.Common, 100, 50),
                new AchievementData("collect_coins_1000", "Coin Hunter", "Collect 1,000 coins", AchievementCategory.Collection, AchievementRarity.Uncommon, 1000, 200),
                new AchievementData("collect_coins_10000", "Coin Master", "Collect 10,000 coins", AchievementCategory.Collection, AchievementRarity.Rare, 10000, 500),
                new AchievementData("coin_master", "Golden Touch", "Collect 100,000 coins total", AchievementCategory.Collection, AchievementRarity.Legendary, 100000, 5000),

                // Actions - Jump
                new AchievementData("jumper_100", "Hopper", "Jump 100 times", AchievementCategory.Gameplay, AchievementRarity.Common, 100, 50),
                new AchievementData("jumper_1000", "Spring Legs", "Jump 1,000 times", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 1000, 300),
                new AchievementData("jump_streak_5", "Jump Combo", "Jump 5 times consecutively", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 5, 100),
                new AchievementData("jump_streak_10", "Air Time", "Jump 10 times consecutively", AchievementCategory.Gameplay, AchievementRarity.Rare, 10, 300),

                // Actions - Slide
                new AchievementData("slider_100", "Slider", "Slide 100 times", AchievementCategory.Gameplay, AchievementRarity.Common, 100, 50),
                new AchievementData("slider_1000", "Ground Hugger", "Slide 1,000 times", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 1000, 300),
                new AchievementData("slide_streak_5", "Slide Combo", "Slide 5 times consecutively", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 5, 100),
                new AchievementData("slide_streak_10", "Low Rider", "Slide 10 times consecutively", AchievementCategory.Gameplay, AchievementRarity.Rare, 10, 300),

                // Power-ups
                new AchievementData("use_powerups_50", "Power User", "Use 50 power-ups", AchievementCategory.PowerUp, AchievementRarity.Common, 50, 100),
                new AchievementData("use_powerups_200", "Power Master", "Use 200 power-ups", AchievementCategory.PowerUp, AchievementRarity.Uncommon, 200, 500),
                new AchievementData("magnet_master", "Magnetic Personality", "Use magnet 50 times", AchievementCategory.PowerUp, AchievementRarity.Uncommon, 50, 200),
                new AchievementData("shield_master", "Shield Bearer", "Use shield 50 times", AchievementCategory.PowerUp, AchievementRarity.Uncommon, 50, 200),
                new AchievementData("speed_demon", "Speed Demon", "Use speed boost 50 times", AchievementCategory.PowerUp, AchievementRarity.Uncommon, 50, 200),
                new AchievementData("superstar", "Superstar", "Use star power 25 times", AchievementCategory.PowerUp, AchievementRarity.Rare, 25, 300),

                // Near Miss
                new AchievementData("near_miss_50", "Daredevil", "Get 50 near misses", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 50, 200),
                new AchievementData("near_miss_500", "Risk Taker", "Get 500 near misses", AchievementCategory.Gameplay, AchievementRarity.Rare, 500, 1000),
                new AchievementData("near_miss_master", "Close Call Master", "Get 10 near misses in one run", AchievementCategory.Gameplay, AchievementRarity.Rare, 10, 500),

                // Obstacles
                new AchievementData("obstacles_passed_1000", "Obstacle Course", "Pass 1,000 obstacles", AchievementCategory.Gameplay, AchievementRarity.Uncommon, 1000, 300),

                // Characters
                new AchievementData("unlock_characters_3", "Team Builder", "Unlock 3 characters", AchievementCategory.Character, AchievementRarity.Uncommon, 3, 300),
                new AchievementData("unlock_characters_all", "Collector", "Unlock all characters", AchievementCategory.Character, AchievementRarity.Legendary, 7, 5000),

                // Special/Challenge
                new AchievementData("flawless_500m", "Flawless Start", "Run 500m without crashing", AchievementCategory.Special, AchievementRarity.Uncommon, 1, 200),
                new AchievementData("flawless_1km", "Flawless Run", "Run 1km without crashing", AchievementCategory.Special, AchievementRarity.Rare, 1, 500),
                new AchievementData("no_coins_1km", "Minimalist", "Run 1km without collecting coins", AchievementCategory.Special, AchievementRarity.Rare, 1, 500),
                new AchievementData("speed_scorer", "Quick Score", "Score 10,000 in under 60 seconds", AchievementCategory.Special, AchievementRarity.Rare, 1, 500),
                new AchievementData("perfect_run_100", "Perfect Dodger", "Pass 100 obstacles in one run", AchievementCategory.Special, AchievementRarity.Rare, 1, 1000),

                // Combo
                new AchievementData("coin_streak_50", "Coin Chain", "Collect 50 coins without missing", AchievementCategory.Collection, AchievementRarity.Uncommon, 50, 200),
                new AchievementData("coin_streak_100", "Coin Streak", "Collect 100 coins without missing", AchievementCategory.Collection, AchievementRarity.Rare, 100, 500),
                new AchievementData("combo_king_5x", "Combo King", "Reach 5x score multiplier", AchievementCategory.Special, AchievementRarity.Rare, 1, 500),
                new AchievementData("combo_king_10x", "Combo Legend", "Reach 10x score multiplier", AchievementCategory.Special, AchievementRarity.Epic, 1, 1000)
            };
        }
    }

    /// <summary>
    /// Simple data class for default achievement creation.
    /// </summary>
    public class AchievementData
    {
        public string id;
        public string name;
        public string description;
        public AchievementCategory category;
        public AchievementRarity rarity;
        public int targetValue;
        public int coinReward;

        public AchievementData(string id, string name, string description,
            AchievementCategory category, AchievementRarity rarity,
            int targetValue, int coinReward)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.category = category;
            this.rarity = rarity;
            this.targetValue = targetValue;
            this.coinReward = coinReward;
        }
    }
}
