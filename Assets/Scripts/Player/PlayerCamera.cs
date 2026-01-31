using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Camera controller that follows the player with smooth movement.
    /// Supports different camera modes and effects.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayer = true;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -8f);
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private float lookAheadDistance = 5f;

        [Header("Lane Following")]
        [SerializeField] private bool followLanePosition = true;
        [SerializeField] private float laneFollowSpeed = 5f;
        [SerializeField] private float maxLaneOffset = 1f;

        [Header("Effects")]
        [SerializeField] private float speedZoomFactor = 0.1f;
        [SerializeField] private float crashShakeIntensity = 0.5f;
        [SerializeField] private float crashShakeDuration = 0.5f;

        // Camera state
        private Vector3 currentOffset;
        private Vector3 velocity;
        private float currentLaneOffset;
        private Camera mainCamera;
        private float originalFOV;

        // Shake effect
        private bool isShaking;
        private float shakeTimer;
        private float shakeIntensity;

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera != null)
            {
                originalFOV = mainCamera.fieldOfView;
            }
            currentOffset = offset;
        }

        private void Start()
        {
            if (target == null && autoFindPlayer)
            {
                FindPlayer();
            }

            // Set initial position
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnLaneChanged += OnLaneChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnLaneChanged -= OnLaneChanged;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (autoFindPlayer)
                {
                    FindPlayer();
                }
                return;
            }

            UpdateCameraPosition();
            UpdateShakeEffect();
            UpdateSpeedZoom();
        }

        #region Camera Movement

        private void UpdateCameraPosition()
        {
            // Calculate target position
            Vector3 targetPosition = target.position + currentOffset;

            // Add look-ahead
            targetPosition += Vector3.forward * lookAheadDistance;

            // Add lane offset for smoother lane following
            if (followLanePosition)
            {
                targetPosition.x += currentLaneOffset;
            }

            // Smooth follow
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                1f / smoothSpeed
            );

            // Look at player
            Vector3 lookTarget = target.position + Vector3.forward * lookAheadDistance;
            transform.LookAt(lookTarget);
        }

        private void OnLaneChanged(int newLane)
        {
            if (!followLanePosition) return;

            // Calculate lane offset for camera
            float targetLaneOffset = (newLane - 1) * maxLaneOffset;
            
            // Smoothly interpolate to new lane offset
            StartCoroutine(SmoothLaneOffset(targetLaneOffset));
        }

        private System.Collections.IEnumerator SmoothLaneOffset(float targetOffset)
        {
            float startOffset = currentLaneOffset;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                currentLaneOffset = Mathf.Lerp(startOffset, targetOffset, t);
                yield return null;
            }

            currentLaneOffset = targetOffset;
        }

        #endregion

        #region Effects

        private void UpdateShakeEffect()
        {
            if (!isShaking) return;

            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0)
            {
                isShaking = false;
                return;
            }

            // Calculate shake offset
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            // Apply shake (added to position in UpdateCameraPosition)
            transform.position += new Vector3(x, y, 0);
        }

        private void OnPlayerCrashed()
        {
            StartShake(crashShakeIntensity, crashShakeDuration);
        }

        /// <summary>
        /// Starts a camera shake effect.
        /// </summary>
        public void StartShake(float intensity, float duration)
        {
            isShaking = true;
            shakeIntensity = intensity;
            shakeTimer = duration;
        }

        private void UpdateSpeedZoom()
        {
            if (mainCamera == null) return;

            // Get player speed
            if (ServiceLocator.TryGet<PlayerController>(out var player))
            {
                float speedRatio = player.CurrentSpeed / Utils.Constants.MaxRunSpeed;
                float targetFOV = originalFOV + (speedRatio * speedZoomFactor * 10f);
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * 2f);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the camera target.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Sets the camera offset.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
            currentOffset = newOffset;
        }

        /// <summary>
        /// Resets camera to default state.
        /// </summary>
        public void ResetCamera()
        {
            currentOffset = offset;
            currentLaneOffset = 0f;
            isShaking = false;

            if (mainCamera != null)
            {
                mainCamera.fieldOfView = originalFOV;
            }

            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        /// <summary>
        /// Finds the player in the scene.
        /// </summary>
        public void FindPlayer()
        {
            if (ServiceLocator.TryGet<PlayerController>(out var player))
            {
                target = player.transform;
            }
            else
            {
                var playerObj = GameObject.FindGameObjectWithTag(Utils.Constants.PlayerTag);
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (target == null) return;

            // Draw camera target position
            Gizmos.color = Color.cyan;
            Vector3 targetPos = target.position + offset;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            Gizmos.DrawLine(transform.position, targetPos);

            // Draw look target
            Gizmos.color = Color.yellow;
            Vector3 lookPos = target.position + Vector3.forward * lookAheadDistance;
            Gizmos.DrawSphere(lookPos, 0.3f);
            Gizmos.DrawLine(transform.position, lookPos);
        }

        #endregion
    }
}
