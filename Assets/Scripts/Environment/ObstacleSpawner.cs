using System.Collections.Generic;
using UnityEngine;
using EscapeTrainRun.Obstacles;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Spawns obstacles based on theme and difficulty.
    /// Uses object pooling for performance optimization.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Obstacle Prefabs - Train Theme")]
        [SerializeField] private Obstacle[] trainObstaclePrefabs;

        [Header("Obstacle Prefabs - Bus Theme")]
        [SerializeField] private Obstacle[] busObstaclePrefabs;

        [Header("Obstacle Prefabs - Ground Theme")]
        [SerializeField] private Obstacle[] groundObstaclePrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float minSpacing = Constants.MinObstacleSpacing;
        [SerializeField] private float maxSpacing = Constants.MaxObstacleSpacing;
        [SerializeField] private int maxObstaclesPerSegment = 5;
        [SerializeField] private float spawnHeightOffset = 0f;

        [Header("Difficulty Scaling")]
        [SerializeField] private AnimationCurve spawnProbabilityCurve;
        [SerializeField] private AnimationCurve multiLaneObstacleCurve;

        // Current theme prefabs
        private Obstacle[] currentPrefabs;
        private ThemeType currentTheme;

        // Object pool
        private Dictionary<string, ObjectPool<Obstacle>> obstaclePools = 
            new Dictionary<string, ObjectPool<Obstacle>>();

        // Active obstacles
        private List<Obstacle> activeObstacles = new List<Obstacle>();

        // Spawn tracking
        private float lastObstacleZ;

        private void Awake()
        {
            InitializeSpawnCurves();
        }

        private void InitializeSpawnCurves()
        {
            // Default spawn probability curve (increases with difficulty)
            if (spawnProbabilityCurve == null || spawnProbabilityCurve.length == 0)
            {
                spawnProbabilityCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 0.8f);
            }

            // Multi-lane obstacle curve (more likely at higher difficulty)
            if (multiLaneObstacleCurve == null || multiLaneObstacleCurve.length == 0)
            {
                multiLaneObstacleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.3f);
            }
        }

        /// <summary>
        /// Sets the current theme for obstacle selection.
        /// </summary>
        public void SetTheme(ThemeType theme)
        {
            currentTheme = theme;
            currentPrefabs = theme switch
            {
                ThemeType.Train => trainObstaclePrefabs,
                ThemeType.Bus => busObstaclePrefabs,
                ThemeType.Ground => groundObstaclePrefabs,
                _ => trainObstaclePrefabs
            };

            Debug.Log($"[ObstacleSpawner] Theme set to: {theme}, {currentPrefabs?.Length ?? 0} prefabs available");
        }

        /// <summary>
        /// Spawns obstacles at the given spawn points based on difficulty.
        /// </summary>
        public void SpawnObstacles(Transform[] spawnPoints, float difficulty)
        {
            if (currentPrefabs == null || currentPrefabs.Length == 0)
            {
                Debug.LogWarning("[ObstacleSpawner] No obstacle prefabs assigned for current theme!");
                return;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                // Use procedural spawning if no spawn points
                SpawnProceduralObstacles(difficulty);
                return;
            }

            int spawnCount = 0;
            float spawnProbability = spawnProbabilityCurve.Evaluate(difficulty);

            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnCount >= maxObstaclesPerSegment) break;

                // Random chance to spawn based on difficulty
                if (Random.value > spawnProbability) continue;

                // Check spacing
                if (Mathf.Abs(spawnPoint.position.z - lastObstacleZ) < minSpacing) continue;

                SpawnObstacleAt(spawnPoint.position, difficulty);
                lastObstacleZ = spawnPoint.position.z;
                spawnCount++;
            }
        }

        /// <summary>
        /// Spawns obstacles procedurally when no spawn points are defined.
        /// </summary>
        private void SpawnProceduralObstacles(float difficulty)
        {
            float segmentStartZ = lastObstacleZ + minSpacing;
            float segmentEndZ = segmentStartZ + Constants.SegmentLength;
            float currentZ = segmentStartZ;
            int spawnCount = 0;

            while (currentZ < segmentEndZ && spawnCount < maxObstaclesPerSegment)
            {
                float spawnProbability = spawnProbabilityCurve.Evaluate(difficulty);

                if (Random.value < spawnProbability)
                {
                    // Select random lane(s)
                    int lane = SelectObstacleLane(difficulty);
                    float x = MathHelpers.LaneToWorldX(lane, Constants.LaneWidth);
                    Vector3 position = new Vector3(x, spawnHeightOffset, currentZ);

                    SpawnObstacleAt(position, difficulty);
                    lastObstacleZ = currentZ;
                    spawnCount++;
                }

                // Move to next potential spawn position
                currentZ += Random.Range(minSpacing, maxSpacing);
            }
        }

        /// <summary>
        /// Spawns a single obstacle at the specified position.
        /// </summary>
        private void SpawnObstacleAt(Vector3 position, float difficulty)
        {
            // Select obstacle type based on difficulty
            Obstacle prefab = SelectObstaclePrefab(difficulty);
            if (prefab == null) return;

            // Spawn obstacle
            Obstacle obstacle = InstantiateObstacle(prefab);
            obstacle.transform.position = position;
            obstacle.Initialize(currentTheme);

            activeObstacles.Add(obstacle);
        }

        /// <summary>
        /// Selects an obstacle prefab based on difficulty.
        /// </summary>
        private Obstacle SelectObstaclePrefab(float difficulty)
        {
            if (currentPrefabs == null || currentPrefabs.Length == 0)
            {
                return null;
            }

            // Weight selection by difficulty
            // Higher difficulty = more complex obstacles (assuming sorted by complexity)
            float weightedIndex = difficulty * (currentPrefabs.Length - 1);
            int minIndex = Mathf.FloorToInt(weightedIndex * 0.5f);
            int maxIndex = Mathf.Min(currentPrefabs.Length - 1, Mathf.CeilToInt(weightedIndex) + 1);

            int selectedIndex = Random.Range(minIndex, maxIndex + 1);
            return currentPrefabs[selectedIndex];
        }

        /// <summary>
        /// Selects which lane(s) to place obstacles.
        /// </summary>
        private int SelectObstacleLane(float difficulty)
        {
            // At higher difficulty, more likely to spawn in center (harder to avoid)
            float centerBias = difficulty * 0.3f;

            float roll = Random.value;
            if (roll < 0.33f - centerBias)
            {
                return 0; // Left
            }
            else if (roll < 0.66f + centerBias)
            {
                return 1; // Center
            }
            else
            {
                return 2; // Right
            }
        }

        /// <summary>
        /// Instantiates an obstacle (with pooling support).
        /// </summary>
        private Obstacle InstantiateObstacle(Obstacle prefab)
        {
            // Simple instantiation for now
            // TODO: Implement object pooling for better performance
            return Instantiate(prefab);
        }

        /// <summary>
        /// Clears all active obstacles.
        /// </summary>
        public void ClearAll()
        {
            foreach (var obstacle in activeObstacles)
            {
                if (obstacle != null)
                {
                    Destroy(obstacle.gameObject);
                }
            }
            activeObstacles.Clear();
            lastObstacleZ = 0f;
        }

        /// <summary>
        /// Removes obstacles behind a certain Z position.
        /// </summary>
        public void CleanupBehind(float zPosition)
        {
            activeObstacles.RemoveAll(obstacle =>
            {
                if (obstacle == null) return true;

                if (obstacle.transform.position.z < zPosition)
                {
                    Destroy(obstacle.gameObject);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Gets count of active obstacles.
        /// </summary>
        public int GetActiveCount()
        {
            // Clean up null references
            activeObstacles.RemoveAll(o => o == null);
            return activeObstacles.Count;
        }

        #region Debug

        private void OnDrawGizmosSelected()
        {
            // Draw spawn range
            Gizmos.color = Color.red;
            Vector3 center = transform.position + Vector3.forward * (minSpacing + maxSpacing) / 2;
            Gizmos.DrawWireCube(center, new Vector3(Constants.LaneWidth * 3, 1, maxSpacing - minSpacing));
        }

        #endregion
    }
}
