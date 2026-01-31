using UnityEngine;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Obstacle that moves between lanes or in a pattern.
    /// </summary>
    public class MovingObstacle : Obstacle
    {
        [Header("Movement Settings")]
        [SerializeField] private MovementType movementType = MovementType.Horizontal;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float moveRange = 2.5f;
        [SerializeField] private bool pingPong = true;

        [Header("Lane Movement")]
        [SerializeField] private int startLane = 0;
        [SerializeField] private int endLane = 2;
        [SerializeField] private float laneSwitchTime = 1f;

        [Header("Circular Movement")]
        [SerializeField] private float circleRadius = 2f;
        [SerializeField] private float circleSpeed = 2f;

        [Header("Warning")]
        [SerializeField] private float warningDistance = 30f;
        [SerializeField] private GameObject moveIndicator;

        // State
        private Vector3 startPosition;
        private float moveProgress;
        private int currentDirection = 1;
        private float circleAngle;
        private bool hasShownWarning;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Activate()
        {
            base.Activate();
            startPosition = transform.position;
            moveProgress = 0f;
            currentDirection = 1;
            circleAngle = 0f;
            hasShownWarning = false;

            if (moveIndicator != null)
            {
                moveIndicator.SetActive(true);
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (moveIndicator != null)
            {
                moveIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            UpdateMovement();
            UpdateWarning();
        }

        private void UpdateMovement()
        {
            switch (movementType)
            {
                case MovementType.Horizontal:
                    UpdateHorizontalMovement();
                    break;
                case MovementType.Vertical:
                    UpdateVerticalMovement();
                    break;
                case MovementType.LaneToLane:
                    UpdateLaneMovement();
                    break;
                case MovementType.Circular:
                    UpdateCircularMovement();
                    break;
                case MovementType.Sine:
                    UpdateSineMovement();
                    break;
            }
        }

        private void UpdateHorizontalMovement()
        {
            moveProgress += moveSpeed * Time.deltaTime * currentDirection;

            if (pingPong)
            {
                if (Mathf.Abs(moveProgress) >= moveRange)
                {
                    currentDirection *= -1;
                    moveProgress = Mathf.Clamp(moveProgress, -moveRange, moveRange);
                }
            }
            else
            {
                if (moveProgress >= moveRange)
                {
                    moveProgress = -moveRange;
                }
            }

            Vector3 pos = startPosition;
            pos.x += moveProgress;
            transform.position = pos;
        }

        private void UpdateVerticalMovement()
        {
            moveProgress += moveSpeed * Time.deltaTime * currentDirection;

            if (pingPong)
            {
                if (Mathf.Abs(moveProgress) >= moveRange)
                {
                    currentDirection *= -1;
                    moveProgress = Mathf.Clamp(moveProgress, -moveRange, moveRange);
                }
            }

            Vector3 pos = startPosition;
            pos.y += moveProgress;
            transform.position = pos;
        }

        private void UpdateLaneMovement()
        {
            moveProgress += Time.deltaTime / laneSwitchTime;

            if (moveProgress >= 1f)
            {
                if (pingPong)
                {
                    // Swap start and end lanes
                    (startLane, endLane) = (endLane, startLane);
                    moveProgress = 0f;
                }
                else
                {
                    moveProgress = 0f;
                }
            }

            float startX = ObstacleManager.Instance != null ? 
                ObstacleManager.Instance.GetLanePosition(startLane) : 
                (startLane - 1) * 2.5f;
            float endX = ObstacleManager.Instance != null ? 
                ObstacleManager.Instance.GetLanePosition(endLane) : 
                (endLane - 1) * 2.5f;

            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startX, endX, EaseInOutSine(moveProgress));
            transform.position = pos;
        }

        private void UpdateCircularMovement()
        {
            circleAngle += circleSpeed * Time.deltaTime;

            Vector3 pos = startPosition;
            pos.x += Mathf.Cos(circleAngle) * circleRadius;
            pos.y += Mathf.Sin(circleAngle) * circleRadius;
            transform.position = pos;
        }

        private void UpdateSineMovement()
        {
            moveProgress += moveSpeed * Time.deltaTime;

            Vector3 pos = startPosition;
            pos.x += Mathf.Sin(moveProgress) * moveRange;
            transform.position = pos;
        }

        private void UpdateWarning()
        {
            if (hasShownWarning) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float distance = transform.position.z - player.transform.position.z;
            if (distance <= warningDistance && distance > 0)
            {
                hasShownWarning = true;
                ShowWarning();

                // Show movement indicator
                if (moveIndicator != null)
                {
                    moveIndicator.SetActive(true);
                }
            }
        }

        private float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        }
    }

    /// <summary>
    /// Types of movement for moving obstacles.
    /// </summary>
    public enum MovementType
    {
        Horizontal,     // Left-right movement
        Vertical,       // Up-down movement
        LaneToLane,     // Moves between specific lanes
        Circular,       // Circular pattern
        Sine            // Sine wave pattern
    }
}
