using System.Collections.Generic;
using UnityEngine;
using EscapeTrainRun.Collectibles;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Spawns coins and power-ups in various patterns.
    /// Creates engaging collectible layouts for players.
    /// </summary>
    public class CollectibleSpawner : MonoBehaviour
    {
        [Header("Coin Prefabs")]
        [SerializeField] private Coin regularCoinPrefab;
        [SerializeField] private Coin specialCoinPrefab;

        [Header("Power-Up Prefabs")]
        [SerializeField] private PowerUp magnetPrefab;
        [SerializeField] private PowerUp shieldPrefab;
        [SerializeField] private PowerUp speedBoostPrefab;
        [SerializeField] private PowerUp starPowerPrefab;
        [SerializeField] private PowerUp multiplierPrefab;
        [SerializeField] private PowerUp mysteryBoxPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float coinHeight = 1f;
        [SerializeField] private float powerUpHeight = 1.5f;
        [SerializeField] private float coinSpacing = 2f;
        [SerializeField] private int maxCoinsPerSegment = 20;

        [Header("Pattern Settings")]
        [SerializeField] private float patternChance = 0.4f;
        [SerializeField] private float arcPatternChance = 0.3f;
        [SerializeField] private float zigzagPatternChance = 0.3f;

        [Header("Power-Up Settings")]
        [SerializeField] private float powerUpSpawnChance = Constants.PowerUpSpawnChance;
        [SerializeField] private float minPowerUpDistance = Constants.MinPowerUpSpacing;

        // State
        private ThemeType currentTheme;
        private float lastPowerUpZ;
        private List<Coin> activeCoins = new List<Coin>();
        private List<PowerUp> activePowerUps = new List<PowerUp>();

        // Coin manager reference
        private CoinManager coinManager;

        private void Start()
        {
            coinManager = CoinManager.Instance;
        }

        /// <summary>
        /// Sets the current theme.
        /// </summary>
        public void SetTheme(ThemeType theme)
        {
            currentTheme = theme;
        }

        #region Coin Spawning

        /// <summary>
        /// Spawns coins at the given spawn points.
        /// </summary>
        public void SpawnCoins(Transform[] spawnPoints)
        {
            if (regularCoinPrefab == null)
            {
                Debug.LogWarning("[CollectibleSpawner] No coin prefab assigned!");
                return;
            }

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                SpawnCoinsAtPoints(spawnPoints);
            }
            else
            {
                SpawnCoinPattern();
            }
        }

        private void SpawnCoinsAtPoints(Transform[] points)
        {
            int spawnCount = 0;

            foreach (var point in points)
            {
                if (spawnCount >= maxCoinsPerSegment) break;

                SpawnCoinAt(point.position);
                spawnCount++;
            }
        }

        /// <summary>
        /// Spawns coins in a procedural pattern.
        /// </summary>
        private void SpawnCoinPattern()
        {
            // Decide on a pattern
            float roll = Random.value;

            if (roll < patternChance)
            {
                if (Random.value < arcPatternChance)
                {
                    SpawnArcPattern();
                }
                else if (Random.value < zigzagPatternChance)
                {
                    SpawnZigzagPattern();
                }
                else
                {
                    SpawnLinePattern();
                }
            }
            else
            {
                SpawnRandomCoins();
            }
        }

        /// <summary>
        /// Spawns coins in a straight line pattern.
        /// </summary>
        private void SpawnLinePattern()
        {
            int lane = Random.Range(0, Constants.LaneCount);
            float x = MathHelpers.LaneToWorldX(lane, Constants.LaneWidth);
            float startZ = GetNextCoinZ();
            int coinCount = Random.Range(5, 10);

            for (int i = 0; i < coinCount; i++)
            {
                Vector3 position = new Vector3(x, coinHeight, startZ + (i * coinSpacing));
                SpawnCoinAt(position);
            }
        }

        /// <summary>
        /// Spawns coins in an arc pattern (jump collection).
        /// </summary>
        private void SpawnArcPattern()
        {
            int lane = Random.Range(0, Constants.LaneCount);
            float x = MathHelpers.LaneToWorldX(lane, Constants.LaneWidth);
            float startZ = GetNextCoinZ();
            int coinCount = 7;

            for (int i = 0; i < coinCount; i++)
            {
                // Parabolic arc
                float t = (float)i / (coinCount - 1);
                float arcHeight = MathHelpers.ParabolicJump(t, Constants.JumpHeight * 0.8f);
                
                Vector3 position = new Vector3(x, coinHeight + arcHeight, startZ + (i * coinSpacing));
                SpawnCoinAt(position);
            }
        }

        /// <summary>
        /// Spawns coins in a zigzag pattern across lanes.
        /// </summary>
        private void SpawnZigzagPattern()
        {
            float startZ = GetNextCoinZ();
            int coinCount = Random.Range(8, 12);
            int currentLane = Random.Range(0, Constants.LaneCount);
            int direction = Random.value > 0.5f ? 1 : -1;

            for (int i = 0; i < coinCount; i++)
            {
                float x = MathHelpers.LaneToWorldX(currentLane, Constants.LaneWidth);
                Vector3 position = new Vector3(x, coinHeight, startZ + (i * coinSpacing));
                SpawnCoinAt(position);

                // Zigzag every 2-3 coins
                if (i % 2 == 0)
                {
                    currentLane += direction;
                    if (currentLane < 0 || currentLane >= Constants.LaneCount)
                    {
                        direction *= -1;
                        currentLane += direction * 2;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns coins randomly across lanes.
        /// </summary>
        private void SpawnRandomCoins()
        {
            float startZ = GetNextCoinZ();
            int coinCount = Random.Range(3, 8);

            for (int i = 0; i < coinCount; i++)
            {
                int lane = Random.Range(0, Constants.LaneCount);
                float x = MathHelpers.LaneToWorldX(lane, Constants.LaneWidth);
                Vector3 position = new Vector3(x, coinHeight, startZ + (i * coinSpacing * 1.5f));
                SpawnCoinAt(position);
            }
        }

        /// <summary>
        /// Spawns a single coin at the specified position.
        /// </summary>
        private void SpawnCoinAt(Vector3 position, bool isSpecial = false)
        {
            Coin prefab = isSpecial && specialCoinPrefab != null ? specialCoinPrefab : regularCoinPrefab;
            if (prefab == null) return;

            Coin coin = Instantiate(prefab);
            coin.SetPosition(position);
            activeCoins.Add(coin);

            // Register with coin manager for magnet
            if (coinManager != null)
            {
                coinManager.RegisterCoin(coin);
            }
        }

        private float GetNextCoinZ()
        {
            // Find the furthest active coin
            float maxZ = 0f;
            foreach (var coin in activeCoins)
            {
                if (coin != null && coin.transform.position.z > maxZ)
                {
                    maxZ = coin.transform.position.z;
                }
            }
            return maxZ + coinSpacing * 3f;
        }

        #endregion

        #region Power-Up Spawning

        /// <summary>
        /// Attempts to spawn a power-up at the given points.
        /// </summary>
        public void SpawnPowerUp(Transform[] spawnPoints, float difficulty)
        {
            // Check spawn chance
            if (Random.value > powerUpSpawnChance)
            {
                return;
            }

            // Check minimum distance
            float currentZ = spawnPoints != null && spawnPoints.Length > 0 
                ? spawnPoints[0].position.z 
                : GetNextCoinZ();

            if (currentZ - lastPowerUpZ < minPowerUpDistance)
            {
                return;
            }

            // Select random power-up type
            PowerUpType type = SelectPowerUpType(difficulty);
            PowerUp prefab = GetPowerUpPrefab(type);

            if (prefab == null)
            {
                Debug.LogWarning($"[CollectibleSpawner] No prefab for power-up type: {type}");
                return;
            }

            // Select spawn position
            Vector3 position;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int index = Random.Range(0, spawnPoints.Length);
                position = spawnPoints[index].position;
                position.y = powerUpHeight;
            }
            else
            {
                int lane = Random.Range(0, Constants.LaneCount);
                float x = MathHelpers.LaneToWorldX(lane, Constants.LaneWidth);
                position = new Vector3(x, powerUpHeight, currentZ);
            }

            // Spawn power-up
            SpawnPowerUpAt(position, prefab);
            lastPowerUpZ = currentZ;
        }

        /// <summary>
        /// Selects a power-up type based on difficulty and rarity.
        /// </summary>
        private PowerUpType SelectPowerUpType(float difficulty)
        {
            // Weight probabilities (adjust based on game balance)
            float[] weights = new float[]
            {
                0.25f, // Magnet
                0.20f, // Shield
                0.15f, // SpeedBoost
                0.10f * difficulty, // StarPower (rare, more common at high difficulty)
                0.20f, // Multiplier
                0.10f  // MysteryBox
            };

            // Normalize weights
            float totalWeight = 0f;
            foreach (float w in weights) totalWeight += w;

            float roll = Random.value * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return (PowerUpType)i;
                }
            }

            return PowerUpType.Magnet; // Default
        }

        /// <summary>
        /// Gets the prefab for a power-up type.
        /// </summary>
        private PowerUp GetPowerUpPrefab(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => magnetPrefab,
                PowerUpType.Shield => shieldPrefab,
                PowerUpType.SpeedBoost => speedBoostPrefab,
                PowerUpType.StarPower => starPowerPrefab,
                PowerUpType.Multiplier => multiplierPrefab,
                PowerUpType.MysteryBox => mysteryBoxPrefab,
                _ => magnetPrefab
            };
        }

        /// <summary>
        /// Spawns a power-up at the specified position.
        /// </summary>
        private void SpawnPowerUpAt(Vector3 position, PowerUp prefab)
        {
            PowerUp powerUp = Instantiate(prefab);
            powerUp.SetPosition(position);
            activePowerUps.Add(powerUp);

            Debug.Log($"[CollectibleSpawner] Spawned {powerUp.Type} at {position}");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clears all active collectibles.
        /// </summary>
        public void ClearAll()
        {
            foreach (var coin in activeCoins)
            {
                if (coin != null)
                {
                    Destroy(coin.gameObject);
                }
            }
            activeCoins.Clear();

            foreach (var powerUp in activePowerUps)
            {
                if (powerUp != null)
                {
                    Destroy(powerUp.gameObject);
                }
            }
            activePowerUps.Clear();

            lastPowerUpZ = 0f;
        }

        /// <summary>
        /// Removes collectibles behind a certain Z position.
        /// </summary>
        public void CleanupBehind(float zPosition)
        {
            activeCoins.RemoveAll(coin =>
            {
                if (coin == null) return true;
                if (coin.transform.position.z < zPosition)
                {
                    Destroy(coin.gameObject);
                    return true;
                }
                return false;
            });

            activePowerUps.RemoveAll(powerUp =>
            {
                if (powerUp == null) return true;
                if (powerUp.transform.position.z < zPosition)
                {
                    Destroy(powerUp.gameObject);
                    return true;
                }
                return false;
            });
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets count of active coins.
        /// </summary>
        public int GetActiveCoinCount()
        {
            activeCoins.RemoveAll(c => c == null || c.IsCollected);
            return activeCoins.Count;
        }

        /// <summary>
        /// Gets count of active power-ups.
        /// </summary>
        public int GetActivePowerUpCount()
        {
            activePowerUps.RemoveAll(p => p == null || p.IsCollected);
            return activePowerUps.Count;
        }

        #endregion
    }
}
