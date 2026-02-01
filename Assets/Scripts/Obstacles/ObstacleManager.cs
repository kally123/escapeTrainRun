using UnityEngine;
using System.Collections.Generic;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Central manager for obstacle spawning, pooling, and difficulty scaling.
    /// </summary>
    public class ObstacleManager : MonoBehaviour
    {
        public static ObstacleManager Instance { get; private set; }

        [Header("Obstacle Prefabs")]
        [SerializeField] private ObstaclePrefabSet trainObstacles;
        [SerializeField] private ObstaclePrefabSet busObstacles;
        [SerializeField] private ObstaclePrefabSet groundObstacles;

        [Header("Spawn Settings")]
        [SerializeField] private float baseSpawnDistance = 50f;
        [SerializeField] private float minSpawnInterval = 0.5f;
        [SerializeField] private float maxSpawnInterval = 2f;
        [SerializeField] private float difficultyRampTime = 300f; // 5 minutes to max difficulty

        [Header("Lane Settings")]
        [SerializeField] private float laneWidth = 2.5f;
        [SerializeField] private int laneCount = 3;

        [Header("Pool Settings")]
        [SerializeField] private int poolSizePerType = 10;
        [SerializeField] private Transform poolContainer;

        // Pools
        private Dictionary<ObstacleType, Queue<Obstacle>> obstaclePools = new Dictionary<ObstacleType, Queue<Obstacle>>();
        private List<Obstacle> activeObstacles = new List<Obstacle>();

        // State
        private ThemeType currentTheme = ThemeType.Train;
        private float nextSpawnTime;
        private float gameTime;
        private float currentDifficulty;
        private bool isSpawning;
        private Transform playerTransform;

        // Pattern spawning
        private ObstaclePattern currentPattern;
        private int patternIndex;
        private float patternSpawnTime;

        public float CurrentDifficulty => currentDifficulty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ServiceLocator.Register(this);
            InitializePools();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGameOver += OnGameOver;
            GameEvents.OnGamePaused += OnGamePaused;
            GameEvents.OnGameResumed += OnGameResumed;
            GameEvents.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnGamePaused -= OnGamePaused;
            GameEvents.OnGameResumed -= OnGameResumed;
            GameEvents.OnThemeChanged -= OnThemeChanged;
        }

        private void Update()
        {
            if (!isSpawning) return;

            gameTime += Time.deltaTime;
            UpdateDifficulty();

            if (Time.time >= nextSpawnTime)
            {
                SpawnObstacle();
                ScheduleNextSpawn();
            }

            UpdateActiveObstacles();
        }

        private void InitializePools()
        {
            if (poolContainer == null)
            {
                var containerObj = new GameObject("Obstacle_Pool");
                containerObj.transform.SetParent(transform);
                poolContainer = containerObj.transform;
            }

            // Initialize pools for each obstacle type
            foreach (ObstacleType type in System.Enum.GetValues(typeof(ObstacleType)))
            {
                obstaclePools[type] = new Queue<Obstacle>();
            }
        }

        private void PopulatePool(ObstacleType type, GameObject prefab, int count)
        {
            if (prefab == null) return;

            for (int i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab, poolContainer);
                var obstacle = instance.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.Initialize(currentTheme);
                    instance.SetActive(false);
                    obstaclePools[type].Enqueue(obstacle);
                }
            }
        }

        #region Public API

        /// <summary>
        /// Spawns an obstacle of the specified type at the given position.
        /// </summary>
        public Obstacle SpawnObstacle(ObstacleType type, Vector3 position, int lane)
        {
            var prefabSet = GetCurrentPrefabSet();
            if (prefabSet == null) return null;

            Obstacle obstacle = GetFromPool(type);
            if (obstacle == null)
            {
                // Create new if pool is empty
                var prefab = prefabSet.GetPrefab(type);
                if (prefab != null)
                {
                    var instance = Instantiate(prefab, poolContainer);
                    obstacle = instance.GetComponent<Obstacle>();
                    obstacle?.Initialize(currentTheme);
                }
            }

            if (obstacle != null)
            {
                obstacle.transform.position = position;
                obstacle.SetLane(lane);
                obstacle.Activate();
                activeObstacles.Add(obstacle);
            }

            return obstacle;
        }

        /// <summary>
        /// Returns an obstacle to the pool.
        /// </summary>
        public void ReturnToPool(Obstacle obstacle)
        {
            if (obstacle == null) return;

            obstacle.Deactivate();
            activeObstacles.Remove(obstacle);

            if (obstaclePools.TryGetValue(obstacle.Type, out var pool))
            {
                pool.Enqueue(obstacle);
            }
        }

        /// <summary>
        /// Clears all active obstacles.
        /// </summary>
        public void ClearAllObstacles()
        {
            foreach (var obstacle in activeObstacles.ToArray())
            {
                ReturnToPool(obstacle);
            }
            activeObstacles.Clear();
        }

        /// <summary>
        /// Sets the current game theme.
        /// </summary>
        public void SetTheme(ThemeType theme)
        {
            currentTheme = theme;
        }

        /// <summary>
        /// Starts a specific obstacle pattern.
        /// </summary>
        public void StartPattern(ObstaclePattern pattern)
        {
            currentPattern = pattern;
            patternIndex = 0;
            patternSpawnTime = Time.time;
        }

        /// <summary>
        /// Gets the lane position for a given lane index.
        /// </summary>
        public float GetLanePosition(int lane)
        {
            int centerLane = laneCount / 2;
            return (lane - centerLane) * laneWidth;
        }

        #endregion

        #region Spawning Logic

        private void SpawnObstacle()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                playerTransform = player?.transform;
                if (playerTransform == null) return;
            }

            // Check if we're in a pattern
            if (currentPattern != null)
            {
                SpawnPatternObstacle();
                return;
            }

            // Random obstacle spawn
            SpawnRandomObstacle();
        }

        private void SpawnRandomObstacle()
        {
            var prefabSet = GetCurrentPrefabSet();
            if (prefabSet == null) return;

            // Select obstacle type based on difficulty
            ObstacleType type = SelectObstacleType();

            // Select lane(s)
            int lane = SelectLane(type);

            // Calculate spawn position
            Vector3 spawnPos = playerTransform.position + Vector3.forward * baseSpawnDistance;
            spawnPos.x = GetLanePosition(lane);
            spawnPos.y = GetSpawnHeight(type);

            SpawnObstacle(type, spawnPos, lane);

            // Chance to spawn pattern at higher difficulties
            if (currentDifficulty > 0.3f && Random.value < currentDifficulty * 0.2f)
            {
                var pattern = SelectRandomPattern();
                if (pattern != null)
                {
                    StartPattern(pattern);
                }
            }
        }

        private void SpawnPatternObstacle()
        {
            if (currentPattern == null || patternIndex >= currentPattern.obstacles.Length)
            {
                currentPattern = null;
                return;
            }

            var patternData = currentPattern.obstacles[patternIndex];
            float timeSincePatternStart = Time.time - patternSpawnTime;

            if (timeSincePatternStart >= patternData.delay)
            {
                Vector3 spawnPos = playerTransform.position + Vector3.forward * baseSpawnDistance;
                spawnPos.x = GetLanePosition(patternData.lane);
                spawnPos.y = GetSpawnHeight(patternData.type);

                SpawnObstacle(patternData.type, spawnPos, patternData.lane);
                patternIndex++;

                if (patternIndex >= currentPattern.obstacles.Length)
                {
                    currentPattern = null;
                }
            }
        }

        private ObstacleType SelectObstacleType()
        {
            float roll = Random.value;

            // At low difficulty, mostly simple obstacles
            // At high difficulty, more complex obstacles
            if (roll < 0.4f - currentDifficulty * 0.2f)
            {
                return ObstacleType.Static;
            }
            else if (roll < 0.6f)
            {
                return ObstacleType.Low;
            }
            else if (roll < 0.75f)
            {
                return ObstacleType.High;
            }
            else if (roll < 0.85f && currentDifficulty > 0.3f)
            {
                return ObstacleType.Moving;
            }
            else if (currentDifficulty > 0.5f)
            {
                return ObstacleType.Combined;
            }

            return ObstacleType.Static;
        }

        private int SelectLane(ObstacleType type)
        {
            // Wide obstacles might span multiple lanes
            if (type == ObstacleType.Wide)
            {
                return 1; // Center lane for wide obstacles
            }

            return Random.Range(0, laneCount);
        }

        private float GetSpawnHeight(ObstacleType type)
        {
            return type switch
            {
                ObstacleType.Low => 0f,
                ObstacleType.High => 2f, // Overhead obstacles
                ObstacleType.Flying => 1.5f,
                _ => 0f
            };
        }

        private void ScheduleNextSpawn()
        {
            float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, currentDifficulty);
            interval *= Random.Range(0.8f, 1.2f); // Add some randomness
            nextSpawnTime = Time.time + interval;
        }

        private ObstaclePattern SelectRandomPattern()
        {
            // This would be loaded from data
            // For now, return null (patterns defined elsewhere)
            return null;
        }

        #endregion

        #region Updates

        private void UpdateDifficulty()
        {
            currentDifficulty = Mathf.Clamp01(gameTime / difficultyRampTime);
        }

        private void UpdateActiveObstacles()
        {
            // Remove obstacles that are too far behind the player
            float despawnDistance = -20f;

            for (int i = activeObstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = activeObstacles[i];
                if (obstacle == null) continue;

                float distanceToPlayer = obstacle.transform.position.z - playerTransform.position.z;
                if (distanceToPlayer < despawnDistance)
                {
                    // Player passed this obstacle successfully
                    GameEvents.RaiseObstaclePassed();
                    ReturnToPool(obstacle);
                }
            }
        }

        #endregion

        #region Helpers

        private Obstacle GetFromPool(ObstacleType type)
        {
            if (obstaclePools.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                return pool.Dequeue();
            }
            return null;
        }

        private ObstaclePrefabSet GetCurrentPrefabSet()
        {
            return currentTheme switch
            {
                ThemeType.Train => trainObstacles,
                ThemeType.Bus => busObstacles,
                ThemeType.Park => groundObstacles,
                _ => trainObstacles
            };
        }

        #endregion

        #region Event Handlers

        private void OnGameStarted()
        {
            ClearAllObstacles();
            gameTime = 0f;
            currentDifficulty = 0f;
            isSpawning = true;
            ScheduleNextSpawn();
        }

        private void OnGameOver(GameOverData data)
        {
            isSpawning = false;
        }

        private void OnGamePaused()
        {
            isSpawning = false;
        }

        private void OnGameResumed()
        {
            isSpawning = true;
        }

        private void OnThemeChanged(ThemeType theme)
        {
            currentTheme = theme;
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<ObstacleManager>();
        }
    }
}
