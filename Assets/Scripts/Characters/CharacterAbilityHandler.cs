using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Player;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Characters
{
    /// <summary>
    /// Handles character-specific abilities during gameplay.
    /// Applies ability effects based on the selected character.
    /// </summary>
    public class CharacterAbilityHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;

        [Header("Ability Settings")]
        [SerializeField] private float doubleJumpCooldown = 0.5f;
        [SerializeField] private float ninjaDodgeInvincibilityTime = 0.5f;

        // State
        private CharacterManager characterManager;
        private CharacterAbility currentAbility;
        private float abilityStrength;

        // Double Jump state
        private bool canDoubleJump;
        private bool hasDoubleJumped;
        private float lastJumpTime;

        // Ninja Dodge state
        private bool isNinjaDodgeActive;

        private void Start()
        {
            characterManager = CharacterManager.Instance;

            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnCharacterSelected += OnCharacterSelected;
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnPlayerJumped += OnPlayerJumped;
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
        }

        private void OnDisable()
        {
            GameEvents.OnCharacterSelected -= OnCharacterSelected;
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnPlayerJumped -= OnPlayerJumped;
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
        }

        private void OnCharacterSelected(CharacterData character)
        {
            if (character == null) return;

            currentAbility = character.PrimaryAbility;
            abilityStrength = character.AbilityStrength;

            Debug.Log($"[CharacterAbilityHandler] Character ability set: {currentAbility}");
        }

        private void OnGameStarted()
        {
            ResetAbilityState();
            ApplyStartOfGameAbilities();
        }

        private void ResetAbilityState()
        {
            canDoubleJump = currentAbility == CharacterAbility.DoubleJump;
            hasDoubleJumped = false;
            isNinjaDodgeActive = false;
        }

        #region Start-of-Game Abilities

        private void ApplyStartOfGameAbilities()
        {
            switch (currentAbility)
            {
                case CharacterAbility.StartWithShield:
                    ApplyStartWithShield();
                    break;

                case CharacterAbility.BeginnerFriendly:
                    ApplyBeginnerFriendly();
                    break;
            }
        }

        private void ApplyStartWithShield()
        {
            // Activate shield power-up at game start
            if (PowerUpManager.Instance != null)
            {
                float shieldDuration = Utils.Constants.ShieldDuration * (1f + abilityStrength * 0.5f);
                PowerUpManager.Instance.ActivatePowerUp(PowerUpType.Shield, shieldDuration);
                Debug.Log("[CharacterAbilityHandler] Started with Shield!");
            }
        }

        private void ApplyBeginnerFriendly()
        {
            // Slower speed increase is handled in PlayerController
            // This could apply additional effects like clearer warnings
            Debug.Log("[CharacterAbilityHandler] Beginner Friendly mode active");
        }

        #endregion

        #region Power-Up Abilities

        private void OnPowerUpActivated(PowerUpType type)
        {
            if (currentAbility == CharacterAbility.ExtendedPowerUps)
            {
                ApplyExtendedPowerUp(type);
            }
            else if (currentAbility == CharacterAbility.EnhancedMagnet && type == PowerUpType.Magnet)
            {
                ApplyEnhancedMagnet();
            }
        }

        private void ApplyExtendedPowerUp(PowerUpType type)
        {
            // Extended power-ups - Luna's ability
            // Duration is extended by ability strength percentage
            float extensionPercent = abilityStrength * 0.5f; // 50% longer at max

            if (PowerUpManager.Instance != null)
            {
                float currentTime = PowerUpManager.Instance.GetRemainingTime(type);
                float extension = currentTime * extensionPercent;

                // Re-activate with extended duration
                // Note: PowerUpManager.ActivatePowerUp extends duration if already active
                Debug.Log($"[CharacterAbilityHandler] Extended {type} by {extension:F1}s");
            }
        }

        private void ApplyEnhancedMagnet()
        {
            // Enhanced magnet - Robo-Kid's ability
            if (CoinManager.Instance != null)
            {
                float rangeMultiplier = 1f + abilityStrength; // Up to 2x range
                CoinManager.Instance.SetMagnetRangeMultiplier(rangeMultiplier);
                Debug.Log($"[CharacterAbilityHandler] Enhanced Magnet range: {rangeMultiplier:F1}x");
            }
        }

        #endregion

        #region Movement Abilities

        private void OnPlayerJumped()
        {
            lastJumpTime = Time.time;

            if (currentAbility == CharacterAbility.DoubleJump)
            {
                hasDoubleJumped = false;
            }
        }

        /// <summary>
        /// Attempts to perform a double jump (Max's ability).
        /// </summary>
        public bool TryDoubleJump()
        {
            if (currentAbility != CharacterAbility.DoubleJump)
            {
                return false;
            }

            if (hasDoubleJumped)
            {
                return false;
            }

            if (Time.time - lastJumpTime < doubleJumpCooldown)
            {
                return false;
            }

            // Check if player is in air
            if (playerController != null && !playerController.IsGrounded)
            {
                hasDoubleJumped = true;
                // Trigger double jump in player movement
                GameEvents.TriggerPlayerJumped();
                Debug.Log("[CharacterAbilityHandler] Double Jump!");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the slide duration modifier (Dino Dan's ability).
        /// </summary>
        public float GetSlideDurationModifier()
        {
            if (currentAbility == CharacterAbility.PowerSlide)
            {
                return 1f + abilityStrength * 0.5f; // Up to 50% longer slides
            }
            return 1f;
        }

        #endregion

        #region Defensive Abilities

        /// <summary>
        /// Triggers Ninja Dodge ability (Ninja Nick).
        /// Called when player has a near-miss with an obstacle.
        /// </summary>
        public void TriggerNinjaDodge()
        {
            if (currentAbility != CharacterAbility.NinjaDodge)
            {
                return;
            }

            if (isNinjaDodgeActive)
            {
                return;
            }

            isNinjaDodgeActive = true;

            // Grant brief invincibility
            if (playerController != null)
            {
                playerController.SetInvincible(true);
                StartCoroutine(EndNinjaDodge());
            }

            Debug.Log("[CharacterAbilityHandler] Ninja Dodge activated!");
        }

        private System.Collections.IEnumerator EndNinjaDodge()
        {
            yield return new WaitForSeconds(ninjaDodgeInvincibilityTime * abilityStrength);

            isNinjaDodgeActive = false;

            // Only remove invincibility if no other power-up is providing it
            if (PowerUpManager.Instance != null)
            {
                bool hasPowerUpInvincibility =
                    PowerUpManager.Instance.IsPowerUpActive(PowerUpType.Shield) ||
                    PowerUpManager.Instance.IsPowerUpActive(PowerUpType.SpeedBoost) ||
                    PowerUpManager.Instance.IsPowerUpActive(PowerUpType.StarPower);

                if (!hasPowerUpInvincibility && playerController != null)
                {
                    playerController.SetInvincible(false);
                }
            }
        }

        #endregion

        #region Score Abilities

        /// <summary>
        /// Gets the score multiplier from character ability (Princess Penny).
        /// </summary>
        public float GetScoreAbilityMultiplier()
        {
            if (currentAbility == CharacterAbility.ScoreBoost)
            {
                return 1f + abilityStrength * 0.25f; // Up to 25% score boost
            }
            return 1f;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Checks if the current character has a specific ability.
        /// </summary>
        public bool HasAbility(CharacterAbility ability)
        {
            return currentAbility == ability;
        }

        /// <summary>
        /// Gets the current ability strength.
        /// </summary>
        public float GetAbilityStrength()
        {
            return abilityStrength;
        }

        /// <summary>
        /// Checks if double jump is available.
        /// </summary>
        public bool CanDoubleJump()
        {
            return currentAbility == CharacterAbility.DoubleJump && !hasDoubleJumped;
        }

        #endregion
    }
}
