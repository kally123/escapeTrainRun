using System.Collections.Generic;
using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Player;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Manages all active power-ups, their durations, and effects.
    /// Central hub for power-up activation, deactivation, and stacking rules.
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Power-Up Effects")]
        [SerializeField] private MagnetEffect magnetEffect;
        [SerializeField] private ShieldEffect shieldEffect;
        [SerializeField] private SpeedBoostEffect speedBoostEffect;
        [SerializeField] private StarPowerEffect starPowerEffect;
        [SerializeField] private MultiplierEffect multiplierEffect;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject powerUpIndicatorPrefab;
        [SerializeField] private Transform indicatorContainer;

        [Header("Audio")]
        [SerializeField] private AudioClip activateSound;
        [SerializeField] private AudioClip deactivateSound;
        [SerializeField] private AudioClip warningSound;

        // Active power-ups tracking
        private Dictionary<PowerUpType, ActivePowerUp> activePowerUps = new Dictionary<PowerUpType, ActivePowerUp>();
        private Dictionary<PowerUpType, PowerUpIndicator> indicators = new Dictionary<PowerUpType, PowerUpIndicator>();

        // Player reference
        private PlayerController playerController;

        // Events
        public event System.Action<PowerUpType, float> OnPowerUpTimeUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ServiceLocator.Register(this);
            InitializeEffects();
        }

        private void OnEnable()
        {
            GameEvents.OnPowerUpActivated += OnPowerUpCollected;
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnPowerUpActivated -= OnPowerUpCollected;
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void Start()
        {
            FindPlayerController();
        }

        private void Update()
        {
            UpdateActivePowerUps();
        }

        #region Initialization

        private void InitializeEffects()
        {
            // Create effect components if not assigned
            if (magnetEffect == null)
            {
                magnetEffect = gameObject.AddComponent<MagnetEffect>();
            }
            if (shieldEffect == null)
            {
                shieldEffect = gameObject.AddComponent<ShieldEffect>();
            }
            if (speedBoostEffect == null)
            {
                speedBoostEffect = gameObject.AddComponent<SpeedBoostEffect>();
            }
            if (starPowerEffect == null)
            {
                starPowerEffect = gameObject.AddComponent<StarPowerEffect>();
            }
            if (multiplierEffect == null)
            {
                multiplierEffect = gameObject.AddComponent<MultiplierEffect>();
            }
        }

        private void FindPlayerController()
        {
            if (ServiceLocator.TryGet<PlayerController>(out var player))
            {
                playerController = player;
            }
        }

        private void OnGameStarted()
        {
            FindPlayerController();
            DeactivateAllPowerUps();
        }

        private void OnGameOver(GameOverData data)
        {
            DeactivateAllPowerUps();
        }

        #endregion

        #region Power-Up Collection

        private void OnPowerUpCollected(PowerUpType type)
        {
            // Handle mystery box specially
            if (type == PowerUpType.MysteryBox)
            {
                type = GetRandomPowerUp();
                Debug.Log($"[PowerUpManager] Mystery box revealed: {type}");
            }

            ActivatePowerUp(type);
        }

        private PowerUpType GetRandomPowerUp()
        {
            // Weighted random selection (excluding mystery box)
            float[] weights = { 0.25f, 0.20f, 0.20f, 0.15f, 0.20f };
            PowerUpType[] types = { 
                PowerUpType.Magnet, 
                PowerUpType.Shield, 
                PowerUpType.SpeedBoost, 
                PowerUpType.StarPower, 
                PowerUpType.Multiplier 
            };

            float total = 0f;
            foreach (float w in weights) total += w;

            float roll = Random.value * total;
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return types[i];
                }
            }

            return PowerUpType.Magnet;
        }

        #endregion

        #region Power-Up Activation

        /// <summary>
        /// Activates a power-up with its default duration.
        /// </summary>
        public void ActivatePowerUp(PowerUpType type)
        {
            float duration = GetPowerUpDuration(type);
            ActivatePowerUp(type, duration);
        }

        /// <summary>
        /// Activates a power-up with a custom duration.
        /// </summary>
        public void ActivatePowerUp(PowerUpType type, float duration)
        {
            // Check if already active - extend duration
            if (activePowerUps.TryGetValue(type, out var existing))
            {
                existing.RemainingTime = Mathf.Max(existing.RemainingTime, duration);
                Debug.Log($"[PowerUpManager] Extended {type} duration to {existing.RemainingTime:F1}s");
                return;
            }

            // Create new active power-up
            var activePowerUp = new ActivePowerUp
            {
                Type = type,
                Duration = duration,
                RemainingTime = duration,
                Effect = GetEffect(type)
            };

            activePowerUps[type] = activePowerUp;

            // Activate effect
            activePowerUp.Effect?.Activate(playerController);

            // Create UI indicator
            CreateIndicator(type, duration);

            // Play sound
            PlayActivateSound();

            Debug.Log($"[PowerUpManager] Activated {type} for {duration}s");
        }

        /// <summary>
        /// Deactivates a specific power-up.
        /// </summary>
        public void DeactivatePowerUp(PowerUpType type)
        {
            if (!activePowerUps.TryGetValue(type, out var activePowerUp))
            {
                return;
            }

            // Deactivate effect
            activePowerUp.Effect?.Deactivate();

            // Remove from tracking
            activePowerUps.Remove(type);

            // Remove UI indicator
            RemoveIndicator(type);

            // Fire event
            GameEvents.TriggerPowerUpDeactivated(type);

            // Play sound
            PlayDeactivateSound();

            Debug.Log($"[PowerUpManager] Deactivated {type}");
        }

        /// <summary>
        /// Deactivates all active power-ups.
        /// </summary>
        public void DeactivateAllPowerUps()
        {
            var types = new List<PowerUpType>(activePowerUps.Keys);
            foreach (var type in types)
            {
                DeactivatePowerUp(type);
            }
        }

        #endregion

        #region Power-Up Updates

        private void UpdateActivePowerUps()
        {
            if (activePowerUps.Count == 0) return;

            var expiredPowerUps = new List<PowerUpType>();

            foreach (var kvp in activePowerUps)
            {
                var powerUp = kvp.Value;
                powerUp.RemainingTime -= Time.deltaTime;

                // Update UI
                OnPowerUpTimeUpdated?.Invoke(kvp.Key, powerUp.RemainingTime / powerUp.Duration);

                // Check for warning (last 3 seconds)
                if (powerUp.RemainingTime <= 3f && !powerUp.WarningPlayed)
                {
                    powerUp.WarningPlayed = true;
                    PlayWarningSound();
                    UpdateIndicatorWarning(kvp.Key, true);
                }

                // Check expiration
                if (powerUp.RemainingTime <= 0f)
                {
                    expiredPowerUps.Add(kvp.Key);
                }
            }

            // Deactivate expired power-ups
            foreach (var type in expiredPowerUps)
            {
                DeactivatePowerUp(type);
            }
        }

        #endregion

        #region Effect Management

        private IPowerUpEffect GetEffect(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => magnetEffect,
                PowerUpType.Shield => shieldEffect,
                PowerUpType.SpeedBoost => speedBoostEffect,
                PowerUpType.StarPower => starPowerEffect,
                PowerUpType.Multiplier => multiplierEffect,
                _ => null
            };
        }

        private float GetPowerUpDuration(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => Constants.MagnetDuration,
                PowerUpType.Shield => Constants.ShieldDuration,
                PowerUpType.SpeedBoost => Constants.SpeedBoostDuration,
                PowerUpType.StarPower => Constants.StarPowerDuration,
                PowerUpType.Multiplier => Constants.MultiplierDuration,
                PowerUpType.MysteryBox => 0f, // Mystery box is instant
                _ => 10f
            };
        }

        #endregion

        #region UI Indicators

        private void CreateIndicator(PowerUpType type, float duration)
        {
            if (powerUpIndicatorPrefab == null || indicatorContainer == null)
            {
                return;
            }

            var indicatorObj = Instantiate(powerUpIndicatorPrefab, indicatorContainer);
            var indicator = indicatorObj.GetComponent<PowerUpIndicator>();

            if (indicator != null)
            {
                indicator.Initialize(type, duration);
                indicators[type] = indicator;
            }
        }

        private void RemoveIndicator(PowerUpType type)
        {
            if (indicators.TryGetValue(type, out var indicator))
            {
                if (indicator != null)
                {
                    Destroy(indicator.gameObject);
                }
                indicators.Remove(type);
            }
        }

        private void UpdateIndicatorWarning(PowerUpType type, bool warning)
        {
            if (indicators.TryGetValue(type, out var indicator))
            {
                indicator?.SetWarning(warning);
            }
        }

        #endregion

        #region Audio

        private void PlayActivateSound()
        {
            if (activateSound != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(activateSound);
            }
        }

        private void PlayDeactivateSound()
        {
            if (deactivateSound != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(deactivateSound);
            }
        }

        private void PlayWarningSound()
        {
            if (warningSound != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(warningSound);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Checks if a power-up is currently active.
        /// </summary>
        public bool IsPowerUpActive(PowerUpType type)
        {
            return activePowerUps.ContainsKey(type);
        }

        /// <summary>
        /// Gets the remaining time for a power-up.
        /// </summary>
        public float GetRemainingTime(PowerUpType type)
        {
            if (activePowerUps.TryGetValue(type, out var powerUp))
            {
                return powerUp.RemainingTime;
            }
            return 0f;
        }

        /// <summary>
        /// Gets all currently active power-up types.
        /// </summary>
        public PowerUpType[] GetActivePowerUps()
        {
            var types = new PowerUpType[activePowerUps.Count];
            activePowerUps.Keys.CopyTo(types, 0);
            return types;
        }

        /// <summary>
        /// Gets the count of active power-ups.
        /// </summary>
        public int GetActivePowerUpCount()
        {
            return activePowerUps.Count;
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<PowerUpManager>();
        }
    }

    /// <summary>
    /// Represents an active power-up with its state.
    /// </summary>
    public class ActivePowerUp
    {
        public PowerUpType Type;
        public float Duration;
        public float RemainingTime;
        public IPowerUpEffect Effect;
        public bool WarningPlayed;
    }

    /// <summary>
    /// Interface for power-up effects.
    /// </summary>
    public interface IPowerUpEffect
    {
        void Activate(PlayerController player);
        void Deactivate();
        void Update();
    }
}
