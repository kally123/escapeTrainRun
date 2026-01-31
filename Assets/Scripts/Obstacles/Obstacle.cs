using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Base class for all obstacles in the game.
    /// Implements IObstacle interface for collision detection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Obstacle : MonoBehaviour, IObstacle
    {
        [Header("Obstacle Settings")]
        [SerializeField] private ObstacleType obstacleType = ObstacleType.Static;
        [SerializeField] private ObstacleAction requiredAction = ObstacleAction.Jump;
        [SerializeField] private int lanesOccupied = 1;

        [Header("Dimensions")]
        [SerializeField] private float height = 1f;
        [SerializeField] private float width = 2f;
        [SerializeField] private float depth = 1f;

        [Header("Animation")]
        [SerializeField] private bool animateIdle = false;
        [SerializeField] private float bobHeight = 0.1f;
        [SerializeField] private float bobSpeed = 1f;
        [SerializeField] private float rotateSpeed = 0f;

        [Header("Moving Obstacle")]
        [SerializeField] private bool isMoving = false;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 2f;
        [SerializeField] private Vector3 moveDirection = Vector3.right;

        // State
        private bool isActive = true;
        private Vector3 startPosition;
        private float moveTimer;
        private ThemeType currentTheme;
        private Collider obstacleCollider;

        // IObstacle implementation
        public ObstacleAction RequiredAction => requiredAction;
        public bool IsActive => isActive;
        public ObstacleType Type => obstacleType;
        public int LanesOccupied => lanesOccupied;
        public float Height => height;

        private void Awake()
        {
            obstacleCollider = GetComponent<Collider>();
            startPosition = transform.position;
            gameObject.tag = Constants.ObstacleTag;
        }

        /// <summary>
        /// Initializes the obstacle for the current theme.
        /// </summary>
        public virtual void Initialize(ThemeType theme)
        {
            currentTheme = theme;
            startPosition = transform.position;
            isActive = true;
            moveTimer = Random.Range(0f, Mathf.PI * 2f);

            // Ensure collider is set correctly
            if (obstacleCollider != null)
            {
                obstacleCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (!isActive) return;

            if (animateIdle)
            {
                AnimateIdle();
            }

            if (isMoving)
            {
                AnimateMoving();
            }
        }

        #region Animation

        private void AnimateIdle()
        {
            // Bobbing
            if (bobHeight > 0)
            {
                float bobOffset = Mathf.Sin(Time.time * bobSpeed + moveTimer) * bobHeight;
                Vector3 pos = startPosition;
                pos.y += bobOffset;
                transform.position = pos;
            }

            // Rotation
            if (rotateSpeed > 0)
            {
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
        }

        private void AnimateMoving()
        {
            moveTimer += Time.deltaTime * moveSpeed;
            float offset = Mathf.Sin(moveTimer) * moveRange;
            transform.position = startPosition + moveDirection.normalized * offset;
        }

        #endregion

        #region Collision

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;

            // Check if it's the player
            if (other.CompareTag(Constants.PlayerTag) || other.GetComponent<PlayerController>() != null)
            {
                OnPlayerHit(other);
            }
        }

        /// <summary>
        /// Called when player collides with this obstacle.
        /// </summary>
        protected virtual void OnPlayerHit(Collider playerCollider)
        {
            // Base implementation does nothing
            // PlayerCollision handles the actual collision response
            Debug.Log($"[Obstacle] Player hit obstacle: {gameObject.name}");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enables or disables this obstacle.
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            if (obstacleCollider != null)
            {
                obstacleCollider.enabled = active;
            }

            // Could also disable renderers for visual feedback
        }

        /// <summary>
        /// Resets the obstacle for reuse from object pool.
        /// </summary>
        public virtual void Reset()
        {
            isActive = true;
            transform.position = startPosition;
            moveTimer = Random.Range(0f, Mathf.PI * 2f);

            if (obstacleCollider != null)
            {
                obstacleCollider.enabled = true;
            }
        }

        /// <summary>
        /// Sets the obstacle position.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            startPosition = position;
        }

        /// <summary>
        /// Checks if this obstacle requires jumping to avoid.
        /// </summary>
        public bool RequiresJump()
        {
            return requiredAction == ObstacleAction.Jump;
        }

        /// <summary>
        /// Checks if this obstacle requires sliding to avoid.
        /// </summary>
        public bool RequiresSlide()
        {
            return requiredAction == ObstacleAction.Slide;
        }

        /// <summary>
        /// Checks if this obstacle requires lane change to avoid.
        /// </summary>
        public bool RequiresLaneChange()
        {
            return requiredAction == ObstacleAction.ChangeLane;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            // Draw obstacle bounds
            Color color = requiredAction switch
            {
                ObstacleAction.Jump => Color.red,
                ObstacleAction.Slide => Color.yellow,
                ObstacleAction.ChangeLane => Color.blue,
                _ => Color.white
            };

            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position + Vector3.up * (height / 2), new Vector3(width, height, depth));

            // Draw movement path for moving obstacles
            if (isMoving)
            {
                Gizmos.color = Color.cyan;
                Vector3 start = transform.position - moveDirection.normalized * moveRange;
                Vector3 end = transform.position + moveDirection.normalized * moveRange;
                Gizmos.DrawLine(start, end);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw lanes occupied
            Gizmos.color = Color.red * 0.5f;
            for (int i = 0; i < lanesOccupied; i++)
            {
                float x = transform.position.x + (i - lanesOccupied / 2f) * Constants.LaneWidth;
                Gizmos.DrawWireCube(
                    new Vector3(x, 0.5f, transform.position.z),
                    new Vector3(Constants.LaneWidth * 0.9f, 1f, depth)
                );
            }
        }

        #endregion
    }

    /// <summary>
    /// Types of obstacles based on behavior.
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>Stationary obstacle.</summary>
        Static,
        /// <summary>Moving obstacle (side to side).</summary>
        Moving,
        /// <summary>Barrier that spans multiple lanes.</summary>
        Barrier,
        /// <summary>Low obstacle requiring jump.</summary>
        LowBarrier,
        /// <summary>Overhead obstacle requiring slide.</summary>
        Overhead
    }
}
