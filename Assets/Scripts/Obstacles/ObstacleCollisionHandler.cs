using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Handles collision detection and response between player and obstacles.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ObstacleCollisionHandler : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;

        [Header("Collision Settings")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float invincibilityDuration = 2f;
        [SerializeField] private int maxLives = 1;

        [Header("Power-Up Protection")]
        [SerializeField] private bool shieldBlocksCollision = true;
        [SerializeField] private bool starPowerDestroysObstacles = true;

        [Header("Effects")]
        [SerializeField] private GameObject crashEffect;
        [SerializeField] private GameObject shieldBreakEffect;

        // State
        private int currentLives;
        private bool isInvincible;
        private float invincibilityEndTime;
        private bool hasShield;
        private bool hasStarPower;

        public int CurrentLives => currentLives;
        public bool IsInvincible => isInvincible;

        private void Start()
        {
            currentLives = maxLives;

            if (playerTransform == null)
            {
                playerTransform = transform;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
            GameEvents.OnGameStarted += OnGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
            GameEvents.OnGameStarted -= OnGameStarted;
        }

        private void Update()
        {
            // Check invincibility timer
            if (isInvincible && Time.time >= invincibilityEndTime)
            {
                EndInvincibility();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.collider);
        }

        private void HandleCollision(Collider other)
        {
            // Check if it's an obstacle
            var obstacle = other.GetComponent<Obstacle>();
            if (obstacle == null) return;

            // Check for star power
            if (hasStarPower && starPowerDestroysObstacles)
            {
                if (obstacle.IsDestructible)
                {
                    obstacle.DestroyObstacle();
                    AwardDestroyPoints(obstacle);
                }
                return; // Star power ignores collision
            }

            // Check for shield
            if (hasShield && shieldBlocksCollision)
            {
                BreakShield();
                return;
            }

            // Check invincibility
            if (isInvincible)
            {
                return;
            }

            // Handle the crash
            OnCrash(obstacle);
        }

        #region Collision Response

        private void OnCrash(Obstacle obstacle)
        {
            currentLives--;

            // Spawn crash effect
            if (crashEffect != null)
            {
                Instantiate(crashEffect, playerTransform.position, Quaternion.identity);
            }

            // Notify obstacle
            obstacle.OnPlayerCollision(gameObject);

            // Raise crash event
            GameEvents.RaisePlayerCrashed();

            // Check for game over
            if (currentLives <= 0)
            {
                OnDeath();
            }
            else
            {
                // Start invincibility
                StartInvincibility();
            }
        }

        private void OnDeath()
        {
            // Game over
            var gameOverData = new GameOverData(
                score: 0,
                coins: 0,
                distance: 0f,
                highScore: false,
                mode: Environment.ThemeType.Train
            );
            GameEvents.RaiseGameOver(gameOverData);
        }

        private void BreakShield()
        {
            hasShield = false;

            // Spawn shield break effect
            if (shieldBreakEffect != null)
            {
                Instantiate(shieldBreakEffect, playerTransform.position, Quaternion.identity);
            }

            // Notify about shield break
            GameEvents.RaiseShieldBroken();

            // Play sound
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlayShieldBreak();
            }

            // Brief invincibility after shield break
            StartInvincibility(1f);
        }

        private void AwardDestroyPoints(Obstacle obstacle)
        {
            // Award points for destroying obstacles
            GameEvents.RaiseScoreChanged(100);

            // Play destroy sound
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlayObstacleDestroy();
            }
        }

        #endregion

        #region Invincibility

        private void StartInvincibility(float? duration = null)
        {
            isInvincible = true;
            invincibilityEndTime = Time.time + (duration ?? invincibilityDuration);

            // Visual feedback (handled by other systems)
        }

        private void EndInvincibility()
        {
            isInvincible = false;
        }

        #endregion

        #region Event Handlers

        private void OnPowerUpActivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    hasShield = true;
                    break;
                case PowerUpType.StarPower:
                    hasStarPower = true;
                    isInvincible = true;
                    break;
            }
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    hasShield = false;
                    break;
                case PowerUpType.StarPower:
                    hasStarPower = false;
                    isInvincible = false;
                    break;
            }
        }

        private void OnGameStarted()
        {
            currentLives = maxLives;
            isInvincible = false;
            hasShield = false;
            hasStarPower = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Adds a life.
        /// </summary>
        public void AddLife()
        {
            currentLives++;
        }

        /// <summary>
        /// Sets the player invincible for a duration.
        /// </summary>
        public void SetInvincible(float duration)
        {
            StartInvincibility(duration);
        }

        /// <summary>
        /// Grants shield protection.
        /// </summary>
        public void GrantShield()
        {
            hasShield = true;
        }

        #endregion
    }
}
