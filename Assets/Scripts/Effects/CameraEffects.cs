using UnityEngine;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Camera effects including FOV changes, zoom, and tilt.
    /// </summary>
    public class CameraEffects : MonoBehaviour
    {
        [Header("Camera Reference")]
        [SerializeField] private Camera targetCamera;

        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float speedBoostFOV = 75f;
        [SerializeField] private float starPowerFOV = 80f;
        [SerializeField] private float fovTransitionSpeed = 5f;

        [Header("Zoom Settings")]
        [SerializeField] private float crashZoomFOV = 50f;
        [SerializeField] private float crashZoomDuration = 0.3f;

        [Header("Tilt Settings")]
        [SerializeField] private float laneChangeTilt = 5f;
        [SerializeField] private float tiltSpeed = 10f;
        [SerializeField] private float tiltReturnSpeed = 5f;

        [Header("Punch Zoom")]
        [SerializeField] private float punchZoomAmount = 5f;
        [SerializeField] private float punchZoomDuration = 0.15f;

        // State
        private float targetFOV;
        private float currentTilt;
        private float targetTilt;
        private Coroutine fovCoroutine;
        private Vector3 originalRotation;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                normalFOV = targetCamera.fieldOfView;
                targetFOV = normalFOV;
                originalRotation = targetCamera.transform.localEulerAngles;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnLaneChanged += OnLaneChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnLaneChanged -= OnLaneChanged;
        }

        private void Update()
        {
            if (targetCamera == null) return;

            UpdateFOV();
            UpdateTilt();
        }

        #region Public API

        /// <summary>
        /// Sets the target FOV.
        /// </summary>
        public void SetTargetFOV(float fov)
        {
            targetFOV = fov;
        }

        /// <summary>
        /// Resets FOV to normal.
        /// </summary>
        public void ResetFOV()
        {
            targetFOV = normalFOV;
        }

        /// <summary>
        /// Triggers a punch zoom effect.
        /// </summary>
        public void PunchZoom()
        {
            if (fovCoroutine != null)
            {
                StopCoroutine(fovCoroutine);
            }
            fovCoroutine = StartCoroutine(PunchZoomCoroutine());
        }

        /// <summary>
        /// Triggers a crash zoom effect.
        /// </summary>
        public void CrashZoom()
        {
            if (fovCoroutine != null)
            {
                StopCoroutine(fovCoroutine);
            }
            fovCoroutine = StartCoroutine(CrashZoomCoroutine());
        }

        /// <summary>
        /// Tilts the camera for lane changes.
        /// </summary>
        public void TiltCamera(int direction)
        {
            targetTilt = -direction * laneChangeTilt;
        }

        /// <summary>
        /// Resets camera tilt.
        /// </summary>
        public void ResetTilt()
        {
            targetTilt = 0f;
        }

        /// <summary>
        /// Sets camera to speed boost effects.
        /// </summary>
        public void EnableSpeedBoostEffects()
        {
            targetFOV = speedBoostFOV;
        }

        /// <summary>
        /// Sets camera to star power effects.
        /// </summary>
        public void EnableStarPowerEffects()
        {
            targetFOV = starPowerFOV;
        }

        /// <summary>
        /// Disables all special effects.
        /// </summary>
        public void DisableAllEffects()
        {
            targetFOV = normalFOV;
            targetTilt = 0f;
        }

        #endregion

        #region Updates

        private void UpdateFOV()
        {
            if (Mathf.Abs(targetCamera.fieldOfView - targetFOV) > 0.01f)
            {
                targetCamera.fieldOfView = Mathf.Lerp(
                    targetCamera.fieldOfView,
                    targetFOV,
                    fovTransitionSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateTilt()
        {
            // Smoothly transition to target tilt
            if (Mathf.Abs(currentTilt - targetTilt) > 0.01f)
            {
                float speed = targetTilt == 0f ? tiltReturnSpeed : tiltSpeed;
                currentTilt = Mathf.Lerp(currentTilt, targetTilt, speed * Time.deltaTime);
            }

            // Apply tilt
            Vector3 rotation = originalRotation;
            rotation.z = currentTilt;
            targetCamera.transform.localEulerAngles = rotation;

            // Auto-reset tilt
            if (targetTilt != 0f)
            {
                targetTilt = Mathf.Lerp(targetTilt, 0f, tiltReturnSpeed * Time.deltaTime * 0.5f);
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator PunchZoomCoroutine()
        {
            float startFOV = targetCamera.fieldOfView;
            float punchFOV = startFOV - punchZoomAmount;

            // Zoom in
            float elapsed = 0f;
            while (elapsed < punchZoomDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (punchZoomDuration / 2f);
                targetCamera.fieldOfView = Mathf.Lerp(startFOV, punchFOV, t);
                yield return null;
            }

            // Zoom out
            elapsed = 0f;
            while (elapsed < punchZoomDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (punchZoomDuration / 2f);
                targetCamera.fieldOfView = Mathf.Lerp(punchFOV, targetFOV, t);
                yield return null;
            }

            fovCoroutine = null;
        }

        private IEnumerator CrashZoomCoroutine()
        {
            float startFOV = targetCamera.fieldOfView;

            // Zoom to crash FOV
            float elapsed = 0f;
            while (elapsed < crashZoomDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / crashZoomDuration;
                t = EaseOutCubic(t);
                targetCamera.fieldOfView = Mathf.Lerp(startFOV, crashZoomFOV, t);
                yield return null;
            }

            fovCoroutine = null;
        }

        #endregion

        #region Event Handlers

        private void OnPowerUpActivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.SpeedBoost:
                    EnableSpeedBoostEffects();
                    break;
                case PowerUpType.StarPower:
                    EnableStarPowerEffects();
                    break;
                default:
                    PunchZoom();
                    break;
            }
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            if (type == PowerUpType.SpeedBoost || type == PowerUpType.StarPower)
            {
                // Check if other effects are still active
                // For now, just reset to normal
                ResetFOV();
            }
        }

        private void OnPlayerCrashed()
        {
            CrashZoom();
        }

        private void OnLaneChanged(int lane)
        {
            // Determine direction from previous lane
            // For now, just use lane value as direction hint
            TiltCamera(lane);
        }

        #endregion

        #region Helpers

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        #endregion
    }
}
