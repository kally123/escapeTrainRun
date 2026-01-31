using UnityEngine;
using System.Collections.Generic;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Manages coins in the game world.
    /// Handles magnet power-up attraction.
    /// </summary>
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }

        [Header("Magnet Settings")]
        [SerializeField] private float magnetRange = 5f;
        [SerializeField] private float magnetUpdateInterval = 0.1f;

        // Magnet state
        private bool isMagnetActive;
        private Transform magnetTarget;
        private float currentMagnetRange;
        private float lastMagnetUpdate;

        // Tracked coins
        private List<Coin> activeCoins = new List<Coin>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
        }

        private void OnDisable()
        {
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
        }

        private void Update()
        {
            if (isMagnetActive)
            {
                UpdateMagnet();
            }
        }

        #region Coin Registration

        /// <summary>
        /// Registers a coin for tracking.
        /// </summary>
        public void RegisterCoin(Coin coin)
        {
            if (!activeCoins.Contains(coin))
            {
                activeCoins.Add(coin);
            }
        }

        /// <summary>
        /// Unregisters a coin from tracking.
        /// </summary>
        public void UnregisterCoin(Coin coin)
        {
            activeCoins.Remove(coin);
        }

        /// <summary>
        /// Cleans up collected coins from the list.
        /// </summary>
        private void CleanupCollectedCoins()
        {
            activeCoins.RemoveAll(coin => coin == null || coin.IsCollected);
        }

        #endregion

        #region Magnet

        private void OnPowerUpActivated(PowerUpType type)
        {
            if (type == PowerUpType.Magnet)
            {
                // Get player transform
                if (ServiceLocator.TryGet<Player.PlayerController>(out var player))
                {
                    EnableMagnet(player.transform, magnetRange);
                }
            }
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            if (type == PowerUpType.Magnet)
            {
                DisableMagnet();
            }
        }

        /// <summary>
        /// Enables the coin magnet effect.
        /// </summary>
        public void EnableMagnet(Transform target, float range)
        {
            isMagnetActive = true;
            magnetTarget = target;
            currentMagnetRange = range;

            Debug.Log($"[CoinManager] Magnet enabled - Range: {range}");
        }

        /// <summary>
        /// Disables the coin magnet effect.
        /// </summary>
        public void DisableMagnet()
        {
            isMagnetActive = false;

            // Stop all coins from being magneted
            foreach (var coin in activeCoins)
            {
                if (coin != null && coin.IsBeingAttracted)
                {
                    coin.StopMagnet();
                }
            }

            Debug.Log("[CoinManager] Magnet disabled");
        }

        private void UpdateMagnet()
        {
            if (magnetTarget == null)
            {
                DisableMagnet();
                return;
            }

            // Throttle updates for performance
            if (Time.time - lastMagnetUpdate < magnetUpdateInterval)
            {
                return;
            }
            lastMagnetUpdate = Time.time;

            // Clean up list
            CleanupCollectedCoins();

            // Check each coin
            Vector3 playerPos = magnetTarget.position;

            foreach (var coin in activeCoins)
            {
                if (coin == null || coin.IsCollected) continue;

                float distance = Vector3.Distance(coin.transform.position, playerPos);

                if (distance <= currentMagnetRange)
                {
                    if (!coin.IsBeingAttracted)
                    {
                        coin.StartMagnet(magnetTarget);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the magnet range multiplier (for character abilities).
        /// </summary>
        public void SetMagnetRangeMultiplier(float multiplier)
        {
            currentMagnetRange = magnetRange * multiplier;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Gets the count of active (uncollected) coins.
        /// </summary>
        public int GetActiveCoinCount()
        {
            CleanupCollectedCoins();
            return activeCoins.Count;
        }

        /// <summary>
        /// Gets all coins within a range of a position.
        /// </summary>
        public List<Coin> GetCoinsInRange(Vector3 position, float range)
        {
            var result = new List<Coin>();

            foreach (var coin in activeCoins)
            {
                if (coin == null || coin.IsCollected) continue;

                float distance = Vector3.Distance(coin.transform.position, position);
                if (distance <= range)
                {
                    result.Add(coin);
                }
            }

            return result;
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
