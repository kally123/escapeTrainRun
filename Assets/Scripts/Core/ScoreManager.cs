using UnityEngine;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Manages score calculation and persistence.
    /// Follows event-driven pattern for score updates.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Score Settings")]
        [SerializeField] private int pointsPerMeter = 1;
        [SerializeField] private int pointsPerCoin = 10;
        [SerializeField] private int baseCoinsPerRun = 5;

        // Current game session data
        private int currentScore;
        private int sessionCoins;
        private float distanceTraveled;
        private int scoreMultiplier = 1;

        private Transform playerTransform;
        private float lastPlayerZ;
        private bool isTracking;

        // Properties
        public int CurrentScore => currentScore;
        public int SessionCoins => sessionCoins;
        public float DistanceTraveled => distanceTraveled;
        public int ScoreMultiplier => scoreMultiplier;

        private void Awake()
        {
            InitializeSingleton();
            RegisterServices();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnCoinsCollected += AddCoins;
            GameEvents.OnPowerUpActivated += HandlePowerUpActivated;
            GameEvents.OnPowerUpDeactivated += HandlePowerUpDeactivated;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnCoinsCollected -= AddCoins;
            GameEvents.OnPowerUpActivated -= HandlePowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= HandlePowerUpDeactivated;
        }

        private void Update()
        {
            if (isTracking && playerTransform != null)
            {
                UpdateDistanceScore();
            }
        }

        private void OnGameStarted()
        {
            ResetSession();
            isTracking = true;

            // Try to find player if not set
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    lastPlayerZ = playerTransform.position.z;
                }
            }
        }

        /// <summary>
        /// Resets all session data for a new game.
        /// </summary>
        public void ResetSession()
        {
            currentScore = 0;
            sessionCoins = 0;
            distanceTraveled = 0;
            scoreMultiplier = 1;
            lastPlayerZ = 0;

            GameEvents.TriggerScoreChanged(currentScore);
            Debug.Log("[ScoreManager] Session reset");
        }

        /// <summary>
        /// Sets the player transform to track distance.
        /// </summary>
        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
            if (player != null)
            {
                lastPlayerZ = player.position.z;
            }
        }

        private void UpdateDistanceScore()
        {
            float currentZ = playerTransform.position.z;
            float deltaDistance = currentZ - lastPlayerZ;

            if (deltaDistance > 0)
            {
                distanceTraveled += deltaDistance;
                int pointsEarned = Mathf.FloorToInt(deltaDistance * pointsPerMeter * scoreMultiplier);
                
                if (pointsEarned > 0)
                {
                    AddScore(pointsEarned);
                }
                
                lastPlayerZ = currentZ;
            }
        }

        /// <summary>
        /// Adds points to the current score.
        /// </summary>
        public void AddScore(int points)
        {
            if (points <= 0) return;

            currentScore += points;
            GameEvents.TriggerScoreChanged(currentScore);
        }

        /// <summary>
        /// Adds coins collected during gameplay.
        /// </summary>
        private void AddCoins(int amount)
        {
            if (amount <= 0) return;

            sessionCoins += amount;
            int scoreFromCoins = amount * pointsPerCoin * scoreMultiplier;
            AddScore(scoreFromCoins);
        }

        /// <summary>
        /// Handles power-up activation effects on scoring.
        /// </summary>
        private void HandlePowerUpActivated(Collectibles.PowerUpType type)
        {
            if (type == Collectibles.PowerUpType.Multiplier)
            {
                scoreMultiplier = 2;
                Debug.Log("[ScoreManager] Score multiplier activated: x2");
            }
        }

        /// <summary>
        /// Handles power-up deactivation effects on scoring.
        /// </summary>
        private void HandlePowerUpDeactivated(Collectibles.PowerUpType type)
        {
            if (type == Collectibles.PowerUpType.Multiplier)
            {
                scoreMultiplier = 1;
                Debug.Log("[ScoreManager] Score multiplier deactivated");
            }
        }

        /// <summary>
        /// Sets the score multiplier value directly.
        /// Used by PowerUpManager for more control over multiplier effects.
        /// </summary>
        public void SetScoreMultiplier(int multiplier)
        {
            scoreMultiplier = Mathf.Max(1, multiplier);
            Debug.Log($"[ScoreManager] Score multiplier set to: {scoreMultiplier}x");
        }

        /// <summary>
        /// Creates game over data for the current session.
        /// </summary>
        public GameOverData CreateGameOverData(ThemeType theme)
        {
            isTracking = false;

            // Check for high score
            var saveManager = ServiceLocator.Get<SaveManager>();
            int previousHighScore = saveManager.GetHighScore();
            bool isHighScore = currentScore > previousHighScore;

            // Save new high score if achieved
            if (isHighScore)
            {
                saveManager.UpdateHighScore(currentScore, theme);
            }

            // Add coins to player's total
            int totalSessionCoins = sessionCoins + baseCoinsPerRun;
            saveManager.AddCoins(totalSessionCoins);

            // Update statistics
            saveManager.IncrementGamesPlayed();
            saveManager.AddDistanceRun(distanceTraveled);

            var gameManager = ServiceLocator.Get<GameManager>();
            float gameDuration = gameManager != null ? gameManager.GameTime : 0f;

            return new GameOverData(
                currentScore,
                totalSessionCoins,
                distanceTraveled,
                isHighScore,
                theme,
                gameDuration
            );
        }

        /// <summary>
        /// Gets the distance traveled formatted as a string.
        /// </summary>
        public string GetFormattedDistance()
        {
            if (distanceTraveled < 1000)
            {
                return $"{distanceTraveled:F0}m";
            }
            else
            {
                return $"{distanceTraveled / 1000f:F2}km";
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
