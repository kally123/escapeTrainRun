using UnityEngine;

namespace EscapeTrainRun.Characters
{
    /// <summary>
    /// Static class containing default character configurations.
    /// Used for documentation and as reference for ScriptableObject creation.
    /// </summary>
    public static class DefaultCharacters
    {
        /// <summary>
        /// Character definitions for the 8 default characters.
        /// These are used as templates for creating CharacterData ScriptableObjects.
        /// </summary>
        public static readonly CharacterDefinition[] Characters = new CharacterDefinition[]
        {
            // Timmy - The Default Kid (Free starter character)
            new CharacterDefinition
            {
                Id = "timmy",
                Name = "Timmy",
                Description = "A cheerful kid who loves adventure! Perfect for beginners.",
                Rarity = CharacterRarity.Common,
                IsUnlockedByDefault = true,
                UnlockPrice = 0,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.0f,
                CoinMultiplier = 1.0f,
                ScoreMultiplier = 1.0f,
                Ability = CharacterAbility.BeginnerFriendly,
                AbilityStrength = 0.3f,
                AbilityDescription = "Speed increases more slowly, giving you more time to react.",
                ThemeColor = new Color(0.2f, 0.6f, 1f) // Blue
            },

            // Luna - The Explorer
            new CharacterDefinition
            {
                Id = "luna",
                Name = "Luna",
                Description = "A curious explorer who never runs out of energy!",
                Rarity = CharacterRarity.Uncommon,
                IsUnlockedByDefault = false,
                UnlockPrice = 500,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.0f,
                CoinMultiplier = 1.0f,
                ScoreMultiplier = 1.0f,
                Ability = CharacterAbility.ExtendedPowerUps,
                AbilityStrength = 0.5f,
                AbilityDescription = "Power-ups last 50% longer!",
                ThemeColor = new Color(0.9f, 0.5f, 0.9f) // Purple
            },

            // Max - The Athlete
            new CharacterDefinition
            {
                Id = "max",
                Name = "Max",
                Description = "The school's best athlete. Can jump higher than anyone!",
                Rarity = CharacterRarity.Uncommon,
                IsUnlockedByDefault = false,
                UnlockPrice = 750,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.1f,
                CoinMultiplier = 1.0f,
                ScoreMultiplier = 1.0f,
                Ability = CharacterAbility.DoubleJump,
                AbilityStrength = 1.0f,
                AbilityDescription = "Tap jump again in mid-air for a double jump!",
                ThemeColor = new Color(1f, 0.5f, 0.2f) // Orange
            },

            // Robo-Kid - The Tech Genius
            new CharacterDefinition
            {
                Id = "robo_kid",
                Name = "Robo-Kid",
                Description = "Half kid, half robot! Built-in magnet technology.",
                Rarity = CharacterRarity.Rare,
                IsUnlockedByDefault = false,
                UnlockPrice = 1500,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.0f,
                CoinMultiplier = 1.2f,
                ScoreMultiplier = 1.0f,
                Ability = CharacterAbility.EnhancedMagnet,
                AbilityStrength = 1.0f,
                AbilityDescription = "Coin magnet has double the range!",
                ThemeColor = new Color(0.3f, 0.8f, 0.8f) // Cyan
            },

            // Super Sara - The Superhero
            new CharacterDefinition
            {
                Id = "super_sara",
                Name = "Super Sara",
                Description = "She's got superpowers! Starts every run protected.",
                Rarity = CharacterRarity.Rare,
                IsUnlockedByDefault = false,
                UnlockPrice = 2000,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.0f,
                CoinMultiplier = 1.0f,
                ScoreMultiplier = 1.0f,
                Ability = CharacterAbility.StartWithShield,
                AbilityStrength = 1.0f,
                AbilityDescription = "Start every game with a free shield!",
                ThemeColor = new Color(1f, 0.2f, 0.3f) // Red
            },

            // Princess Penny - The Royal
            new CharacterDefinition
            {
                Id = "princess_penny",
                Name = "Princess Penny",
                Description = "A princess who loves scoring points and collecting treasure!",
                Rarity = CharacterRarity.Epic,
                IsUnlockedByDefault = false,
                UnlockPrice = 3000,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 0.95f,
                CoinMultiplier = 1.3f,
                ScoreMultiplier = 1.25f,
                Ability = CharacterAbility.ScoreBoost,
                AbilityStrength = 0.25f,
                AbilityDescription = "Earn 25% more score and 30% more coins!",
                ThemeColor = new Color(1f, 0.8f, 0.9f) // Pink
            },

            // Dino Dan - The Prehistoric Kid
            new CharacterDefinition
            {
                Id = "dino_dan",
                Name = "Dino Dan",
                Description = "Raised by dinosaurs! Has a powerful sliding attack.",
                Rarity = CharacterRarity.Epic,
                IsUnlockedByDefault = false,
                UnlockPrice = 3500,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.15f,
                CoinMultiplier = 1.0f,
                ScoreMultiplier = 1.1f,
                Ability = CharacterAbility.PowerSlide,
                AbilityStrength = 0.5f,
                AbilityDescription = "Slides last 50% longer and destroy obstacles!",
                ThemeColor = new Color(0.4f, 0.8f, 0.2f) // Green
            },

            // Ninja Nick - The Shadow Master
            new CharacterDefinition
            {
                Id = "ninja_nick",
                Name = "Ninja Nick",
                Description = "Master of stealth and reflexes! Nearly impossible to hit.",
                Rarity = CharacterRarity.Legendary,
                IsUnlockedByDefault = false,
                UnlockPrice = 5000,
                UnlockType = UnlockType.Coins,
                SpeedModifier = 1.2f,
                CoinMultiplier = 1.0f,
                ScoreMultiplier = 1.0f,
                Ability = CharacterAbility.NinjaDodge,
                AbilityStrength = 1.0f,
                AbilityDescription = "Near-misses grant brief invincibility!",
                ThemeColor = new Color(0.2f, 0.2f, 0.3f) // Dark Blue
            }
        };

        /// <summary>
        /// Gets the rarity color for display.
        /// </summary>
        public static Color GetRarityColor(CharacterRarity rarity)
        {
            return rarity switch
            {
                CharacterRarity.Common => new Color(0.7f, 0.7f, 0.7f),     // Gray
                CharacterRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),   // Green
                CharacterRarity.Rare => new Color(0.2f, 0.5f, 1f),         // Blue
                CharacterRarity.Epic => new Color(0.7f, 0.3f, 0.9f),       // Purple
                CharacterRarity.Legendary => new Color(1f, 0.8f, 0.2f),    // Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// Gets a human-readable name for a rarity.
        /// </summary>
        public static string GetRarityName(CharacterRarity rarity)
        {
            return rarity switch
            {
                CharacterRarity.Common => "Common",
                CharacterRarity.Uncommon => "Uncommon",
                CharacterRarity.Rare => "Rare",
                CharacterRarity.Epic => "Epic",
                CharacterRarity.Legendary => "Legendary",
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Definition structure for creating characters.
    /// </summary>
    [System.Serializable]
    public struct CharacterDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public CharacterRarity Rarity;
        public bool IsUnlockedByDefault;
        public int UnlockPrice;
        public UnlockType UnlockType;
        public float SpeedModifier;
        public float CoinMultiplier;
        public float ScoreMultiplier;
        public CharacterAbility Ability;
        public float AbilityStrength;
        public string AbilityDescription;
        public Color ThemeColor;
    }
}
