using UnityEngine;

namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Represents a single segment of the procedurally generated track.
    /// Contains spawn points for obstacles and collectibles.
    /// </summary>
    public class TrackSegment : MonoBehaviour
    {
        [Header("Segment Settings")]
        [SerializeField] private float length = 30f;
        [SerializeField] private ThemeType theme;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] obstacleSpawnPoints;
        [SerializeField] private Transform[] coinSpawnPoints;
        [SerializeField] private Transform[] powerUpSpawnPoints;

        [Header("Visual Elements")]
        [SerializeField] private GameObject[] themeDecorations;

        // Properties
        public float Length => length;
        public ThemeType Theme => theme;
        public Transform[] ObstacleSpawnPoints => obstacleSpawnPoints;
        public Transform[] CoinSpawnPoints => coinSpawnPoints;
        public Transform[] PowerUpSpawnPoints => powerUpSpawnPoints;

        /// <summary>
        /// End position of this segment (for spawning next segment).
        /// </summary>
        public Vector3 EndPosition => transform.position + Vector3.forward * length;

        /// <summary>
        /// Initializes the segment for a specific theme.
        /// </summary>
        /// <param name="themeType">The theme to apply.</param>
        public void Initialize(ThemeType themeType)
        {
            theme = themeType;
            ApplyTheme();
        }

        /// <summary>
        /// Applies theme-specific visuals to the segment.
        /// </summary>
        private void ApplyTheme()
        {
            // Activate decorations based on theme
            // This would be expanded based on your theme prefabs
            foreach (var decoration in themeDecorations)
            {
                if (decoration != null)
                {
                    decoration.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Cleans up the segment when it's recycled.
        /// </summary>
        public void Cleanup()
        {
            // Reset any spawned objects or state
            // This is called before the segment goes back to the pool
        }

        /// <summary>
        /// Gets a random obstacle spawn point.
        /// </summary>
        public Transform GetRandomObstacleSpawnPoint()
        {
            if (obstacleSpawnPoints == null || obstacleSpawnPoints.Length == 0)
            {
                return null;
            }
            return obstacleSpawnPoints[Random.Range(0, obstacleSpawnPoints.Length)];
        }

        /// <summary>
        /// Gets all coin spawn points for this segment.
        /// </summary>
        public Transform[] GetCoinSpawnPoints()
        {
            return coinSpawnPoints;
        }

        /// <summary>
        /// Gets a power-up spawn point if available.
        /// </summary>
        public Transform GetPowerUpSpawnPoint()
        {
            if (powerUpSpawnPoints == null || powerUpSpawnPoints.Length == 0)
            {
                return null;
            }
            return powerUpSpawnPoints[Random.Range(0, powerUpSpawnPoints.Length)];
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize segment bounds
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                transform.position + Vector3.forward * (length / 2f),
                new Vector3(7.5f, 3f, length)
            );

            // Visualize spawn points
            if (obstacleSpawnPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var point in obstacleSpawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.position, 0.5f);
                    }
                }
            }

            if (coinSpawnPoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var point in coinSpawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.position, 0.3f);
                    }
                }
            }

            if (powerUpSpawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var point in powerUpSpawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.position, 0.4f);
                    }
                }
            }
        }
    }
}
