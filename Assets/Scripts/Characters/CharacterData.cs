using UnityEngine;

namespace EscapeTrainRun.Characters
{
    /// <summary>
    /// ScriptableObject containing all data for a playable character.
    /// Each character has unique stats, abilities, and visual properties.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Escape Train Run/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string characterId;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private CharacterRarity rarity = CharacterRarity.Common;

        [Header("Visuals")]
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject modelPrefab;
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private Color themeColor = Color.white;

        [Header("Unlock Requirements")]
        [SerializeField] private bool isUnlockedByDefault;
        [SerializeField] private int unlockPrice = 1000;
        [SerializeField] private UnlockType unlockType = UnlockType.Coins;

        [Header("Base Stats")]
        [SerializeField] private float speedModifier = 1f;
        [SerializeField] private float coinMultiplier = 1f;
        [SerializeField] private float scoreMultiplier = 1f;

        [Header("Abilities")]
        [SerializeField] private CharacterAbility primaryAbility;
        [SerializeField] private float abilityStrength = 1f;
        [SerializeField] private string abilityDescription;

        [Header("Power-Up Modifiers")]
        [SerializeField] private float magnetRangeBonus = 0f;
        [SerializeField] private float shieldDurationBonus = 0f;
        [SerializeField] private float speedBoostBonus = 0f;
        [SerializeField] private float starPowerBonus = 0f;
        [SerializeField] private float multiplierBonus = 0f;

        [Header("Audio")]
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip slideSound;
        [SerializeField] private AudioClip hurtSound;

        #region Properties - Identity

        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public string Description => description;
        public CharacterRarity Rarity => rarity;

        #endregion

        #region Properties - Visuals

        public Sprite Portrait => portrait;
        public Sprite Icon => icon;
        public GameObject ModelPrefab => modelPrefab;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public Color ThemeColor => themeColor;

        #endregion

        #region Properties - Unlock

        public bool IsUnlockedByDefault => isUnlockedByDefault;
        public int UnlockPrice => unlockPrice;
        public UnlockType UnlockRequirement => unlockType;

        #endregion

        #region Properties - Stats

        public float SpeedModifier => speedModifier;
        public float CoinMultiplier => coinMultiplier;
        public float ScoreMultiplier => scoreMultiplier;

        #endregion

        #region Properties - Abilities

        public CharacterAbility PrimaryAbility => primaryAbility;
        public float AbilityStrength => abilityStrength;
        public string AbilityDescription => abilityDescription;

        #endregion

        #region Properties - Power-Up Bonuses

        public float MagnetRangeBonus => magnetRangeBonus;
        public float ShieldDurationBonus => shieldDurationBonus;
        public float SpeedBoostBonus => speedBoostBonus;
        public float StarPowerBonus => starPowerBonus;
        public float MultiplierBonus => multiplierBonus;

        #endregion

        #region Properties - Audio

        public AudioClip SelectSound => selectSound;
        public AudioClip JumpSound => jumpSound;
        public AudioClip SlideSound => slideSound;
        public AudioClip HurtSound => hurtSound;

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the total power-up duration modifier for a specific type.
        /// </summary>
        public float GetPowerUpDurationBonus(Collectibles.PowerUpType type)
        {
            return type switch
            {
                Collectibles.PowerUpType.Magnet => magnetRangeBonus,
                Collectibles.PowerUpType.Shield => shieldDurationBonus,
                Collectibles.PowerUpType.SpeedBoost => speedBoostBonus,
                Collectibles.PowerUpType.StarPower => starPowerBonus,
                Collectibles.PowerUpType.Multiplier => multiplierBonus,
                _ => 0f
            };
        }

        /// <summary>
        /// Gets the rarity color for UI display.
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                CharacterRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                CharacterRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                CharacterRarity.Rare => new Color(0.2f, 0.5f, 1f),
                CharacterRarity.Epic => new Color(0.7f, 0.3f, 0.9f),
                CharacterRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };
        }

        /// <summary>
        /// Gets a formatted string of all stat bonuses.
        /// </summary>
        public string GetStatsSummary()
        {
            var stats = new System.Text.StringBuilder();

            if (speedModifier != 1f)
                stats.AppendLine($"Speed: {(speedModifier > 1 ? "+" : "")}{(speedModifier - 1) * 100:F0}%");

            if (coinMultiplier != 1f)
                stats.AppendLine($"Coins: {(coinMultiplier > 1 ? "+" : "")}{(coinMultiplier - 1) * 100:F0}%");

            if (scoreMultiplier != 1f)
                stats.AppendLine($"Score: {(scoreMultiplier > 1 ? "+" : "")}{(scoreMultiplier - 1) * 100:F0}%");

            return stats.ToString().TrimEnd();
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(characterId))
            {
                characterId = name.ToLower().Replace(" ", "_");
            }

            // Clamp modifiers
            speedModifier = Mathf.Clamp(speedModifier, 0.5f, 2f);
            coinMultiplier = Mathf.Clamp(coinMultiplier, 0.5f, 3f);
            scoreMultiplier = Mathf.Clamp(scoreMultiplier, 0.5f, 3f);
            abilityStrength = Mathf.Clamp(abilityStrength, 0f, 2f);
        }
#endif
    }

    /// <summary>
    /// Character rarity levels affecting unlock difficulty and stats.
    /// </summary>
    public enum CharacterRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Types of unlock requirements for characters.
    /// </summary>
    public enum UnlockType
    {
        /// <summary>Unlock with in-game coins.</summary>
        Coins,
        /// <summary>Unlock by reaching a distance milestone.</summary>
        Distance,
        /// <summary>Unlock by collecting total coins.</summary>
        TotalCoins,
        /// <summary>Unlock by playing a number of games.</summary>
        GamesPlayed,
        /// <summary>Unlock by achieving a high score.</summary>
        HighScore,
        /// <summary>Special event unlock.</summary>
        Special
    }

    /// <summary>
    /// Special abilities that characters can have.
    /// </summary>
    public enum CharacterAbility
    {
        /// <summary>No special ability.</summary>
        None,

        /// <summary>Start with a shield (Super Sara).</summary>
        StartWithShield,

        /// <summary>Longer power-up durations (Luna).</summary>
        ExtendedPowerUps,

        /// <summary>Double jump capability (Max).</summary>
        DoubleJump,

        /// <summary>Coin magnet range increased (Robo-Kid).</summary>
        EnhancedMagnet,

        /// <summary>Score multiplier bonus (Princess Penny).</summary>
        ScoreBoost,

        /// <summary>Slower speed increase for easier gameplay (Timmy).</summary>
        BeginnerFriendly,

        /// <summary>Longer slide duration (Dino Dan).</summary>
        PowerSlide,

        /// <summary>Brief invincibility after near-miss (Ninja Nick).</summary>
        NinjaDodge
    }
}
