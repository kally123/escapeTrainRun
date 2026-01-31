using System.Collections.Generic;
using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Procedural level generator that spawns track segments, obstacles, and collectibles.
    /// Implements infinite runner pattern with object pooling for performance.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; private set; }

        [Header("Track Settings")]
        [SerializeField] private float segmentLength = Constants.SegmentLength;
        [SerializeField] private int initialSegments = Constants.InitialSegments;
        [SerializeField] private int maxActiveSegments = Constants.MaxActiveSegments;
        [SerializeField] private float despawnDistance = Constants.DespawnDistance;

        [Header("Segment Prefabs")]
        [SerializeField] private TrackSegment[] trainSegmentPrefabs;
        [SerializeField] private TrackSegment[] busSegmentPrefabs;
        [SerializeField] private TrackSegment[] groundSegmentPrefabs;

        [Header("Spawners")]
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private CollectibleSpawner collectibleSpawner;

        [Header("Difficulty Settings")]
        [SerializeField] private float difficultyIncreaseRate = 0.01f;
        [SerializeField] private float maxDifficulty = 1f;
        [SerializeField] private float baseDifficulty = 0.2f;

        // Active segments
        private Queue<TrackSegment> activeSegments = new Queue<TrackSegment>();
        private float nextSpawnZ;
        private float currentDifficulty;

        // Theme
        private ThemeType currentTheme = ThemeType.Train;
        private TrackSegment[] currentPrefabs;

        // Player reference
        private Transform playerTransform;

        // State
        private bool isGenerating;
        private int segmentIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnThemeSelected += OnThemeSelected;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnThemeSelected -= OnThemeSelected;
        }

        private void Start()
        {
            InitializeSpawners();
            SetThemePrefabs(currentTheme);
        }

        private void Update()
        {
            if (!isGenerating || playerTransform == null) return;

            UpdateDifficulty();
            CheckSpawnNewSegments();
            CheckDespawnOldSegments();
        }

        #region Initialization

        private void InitializeSpawners()
        {
            if (obstacleSpawner == null)
            {
                obstacleSpawner = GetComponentInChildren<ObstacleSpawner>();
            }

            if (collectibleSpawner == null)
            {
                collectibleSpawner = GetComponentInChildren<CollectibleSpawner>();
            }
        }

        private void OnGameStarted()
        {
            // Find player
            if (ServiceLocator.TryGet<Player.PlayerController>(out var player))
            {
                playerTransform = player.transform;
            }
            else
            {
                var playerObj = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }

            StartGeneration();
        }

        private void OnThemeSelected(ThemeType theme)
        {
            currentTheme = theme;
            SetThemePrefabs(theme);
        }

        private void SetThemePrefabs(ThemeType theme)
        {
            currentPrefabs = theme switch
            {
                ThemeType.Train => trainSegmentPrefabs,
                ThemeType.Bus => busSegmentPrefabs,
                ThemeType.Ground => groundSegmentPrefabs,
                _ => trainSegmentPrefabs
            };

            // Notify spawners of theme change
            if (obstacleSpawner != null)
            {
                obstacleSpawner.SetTheme(theme);
            }

            if (collectibleSpawner != null)
            {
                collectibleSpawner.SetTheme(theme);
            }

            Debug.Log($"[LevelGenerator] Theme set to: {theme}");
        }

        #endregion

        #region Generation Control

        /// <summary>
        /// Starts level generation.
        /// </summary>
        public void StartGeneration()
        {
            ClearAllSegments();
            
            nextSpawnZ = 0f;
            currentDifficulty = baseDifficulty;
            segmentIndex = 0;
            isGenerating = true;

            // Spawn initial segments
            for (int i = 0; i < initialSegments; i++)
            {
                SpawnNextSegment(i == 0); // First segment has no obstacles
            }

            Debug.Log($"[LevelGenerator] Generation started with {initialSegments} segments");
        }

        /// <summary>
        /// Stops level generation.
        /// </summary>
        public void StopGeneration()
        {
            isGenerating = false;
        }

        /// <summary>
        /// Clears all active segments.
        /// </summary>
        public void ClearAllSegments()
        {
            while (activeSegments.Count > 0)
            {
                var segment = activeSegments.Dequeue();
                if (segment != null)
                {
                    DespawnSegment(segment);
                }
            }

            // Clear spawners
            if (obstacleSpawner != null)
            {
                obstacleSpawner.ClearAll();
            }

            if (collectibleSpawner != null)
            {
                collectibleSpawner.ClearAll();
            }
        }

        #endregion

        #region Segment Management

        private void SpawnNextSegment(bool isEmpty = false)
        {
            if (currentPrefabs == null || currentPrefabs.Length == 0)
            {
                Debug.LogWarning("[LevelGenerator] No segment prefabs available!");
                CreateDefaultSegment();
                return;
            }

            // Select random segment prefab
            int prefabIndex = Random.Range(0, currentPrefabs.Length);
            TrackSegment prefab = currentPrefabs[prefabIndex];

            // Instantiate or get from pool
            TrackSegment segment = Instantiate(prefab);
            segment.transform.position = new Vector3(0, 0, nextSpawnZ);
            segment.Initialize(currentTheme, segmentIndex);

            // Add to active list
            activeSegments.Enqueue(segment);

            // Spawn obstacles and collectibles (skip for first segment)
            if (!isEmpty)
            {
                SpawnSegmentContent(segment);
            }

            // Update spawn position
            nextSpawnZ += segmentLength;
            segmentIndex++;

            GameEvents.TriggerSegmentSpawned(segment);
        }

        private void CreateDefaultSegment()
        {
            // Create a simple default segment if no prefabs assigned
            GameObject segmentObj = new GameObject($"Segment_{segmentIndex}");
            segmentObj.transform.position = new Vector3(0, 0, nextSpawnZ);

            // Add ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.SetParent(segmentObj.transform);
            ground.transform.localPosition = new Vector3(0, -0.5f, segmentLength / 2);
            ground.transform.localScale = new Vector3(Constants.LaneWidth * 3, 1f, segmentLength);
            ground.tag = Constants.GroundTag;

            var segment = segmentObj.AddComponent<TrackSegment>();
            segment.Initialize(currentTheme, segmentIndex);

            activeSegments.Enqueue(segment);

            nextSpawnZ += segmentLength;
            segmentIndex++;
        }

        private void SpawnSegmentContent(TrackSegment segment)
        {
            // Get spawn points from segment
            var obstaclePoints = segment.GetObstacleSpawnPoints();
            var coinPoints = segment.GetCoinSpawnPoints();
            var powerUpPoints = segment.GetPowerUpSpawnPoints();

            // Spawn obstacles
            if (obstacleSpawner != null && obstaclePoints != null)
            {
                obstacleSpawner.SpawnObstacles(obstaclePoints, currentDifficulty);
            }

            // Spawn collectibles
            if (collectibleSpawner != null)
            {
                if (coinPoints != null)
                {
                    collectibleSpawner.SpawnCoins(coinPoints);
                }

                if (powerUpPoints != null)
                {
                    collectibleSpawner.SpawnPowerUp(powerUpPoints, currentDifficulty);
                }
            }
        }

        private void DespawnSegment(TrackSegment segment)
        {
            if (segment == null) return;

            // Could return to pool instead of destroying
            Destroy(segment.gameObject);
        }

        private void CheckSpawnNewSegments()
        {
            if (playerTransform == null) return;

            float playerZ = playerTransform.position.z;
            float spawnThreshold = nextSpawnZ - (segmentLength * (initialSegments - 1));

            // Spawn new segment when player approaches
            while (playerZ > spawnThreshold && activeSegments.Count < maxActiveSegments)
            {
                SpawnNextSegment();
                spawnThreshold = nextSpawnZ - (segmentLength * (initialSegments - 1));
            }
        }

        private void CheckDespawnOldSegments()
        {
            if (playerTransform == null || activeSegments.Count == 0) return;

            float playerZ = playerTransform.position.z;

            // Despawn segments that are too far behind
            while (activeSegments.Count > 0)
            {
                TrackSegment oldest = activeSegments.Peek();
                if (oldest == null)
                {
                    activeSegments.Dequeue();
                    continue;
                }

                float segmentEndZ = oldest.transform.position.z + segmentLength;
                if (playerZ - segmentEndZ > despawnDistance)
                {
                    activeSegments.Dequeue();
                    DespawnSegment(oldest);
                    GameEvents.TriggerSegmentDespawned();
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Difficulty

        private void UpdateDifficulty()
        {
            if (playerTransform == null) return;

            // Increase difficulty based on distance traveled
            float distance = playerTransform.position.z;
            currentDifficulty = Mathf.Min(
                baseDifficulty + (distance * difficultyIncreaseRate / 100f),
                maxDifficulty
            );
        }

        /// <summary>
        /// Gets the current difficulty level (0-1).
        /// </summary>
        public float GetCurrentDifficulty()
        {
            return currentDifficulty;
        }

        /// <summary>
        /// Sets the base difficulty level.
        /// </summary>
        public void SetBaseDifficulty(float difficulty)
        {
            baseDifficulty = Mathf.Clamp01(difficulty);
            currentDifficulty = baseDifficulty;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the current theme.
        /// </summary>
        public ThemeType GetCurrentTheme()
        {
            return currentTheme;
        }

        /// <summary>
        /// Gets the number of active segments.
        /// </summary>
        public int GetActiveSegmentCount()
        {
            return activeSegments.Count;
        }

        /// <summary>
        /// Forces a theme change mid-game.
        /// </summary>
        public void ChangeTheme(ThemeType newTheme)
        {
            OnThemeSelected(newTheme);
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<LevelGenerator>();
        }

        #region Debug

        private void OnDrawGizmosSelected()
        {
            // Draw spawn threshold
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                new Vector3(0, 1, nextSpawnZ),
                new Vector3(Constants.LaneWidth * 3, 2, 0.5f)
            );

            // Draw despawn line
            if (playerTransform != null)
            {
                Gizmos.color = Color.red;
                float despawnZ = playerTransform.position.z - despawnDistance;
                Gizmos.DrawWireCube(
                    new Vector3(0, 1, despawnZ),
                    new Vector3(Constants.LaneWidth * 3, 2, 0.5f)
                );
            }
        }

        #endregion
    }
}
