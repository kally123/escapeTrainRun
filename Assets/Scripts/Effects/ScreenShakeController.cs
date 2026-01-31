using UnityEngine;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Controls camera shake effects for impacts, crashes, and power-ups.
    /// </summary>
    public class ScreenShakeController : MonoBehaviour
    {
        public static ScreenShakeController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        [Header("Shake Presets")]
        [SerializeField] private ShakePreset lightShake;
        [SerializeField] private ShakePreset mediumShake;
        [SerializeField] private ShakePreset heavyShake;
        [SerializeField] private ShakePreset crashShake;

        [Header("Settings")]
        [SerializeField] private bool shakeEnabled = true;
        [SerializeField] private float shakeMultiplier = 1f;
        [SerializeField] private float maxShakeOffset = 0.5f;

        // State
        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;
        private float currentTrauma;
        private float traumaDecay = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            if (cameraTransform != null)
            {
                originalPosition = cameraTransform.localPosition;
            }

            InitializePresets();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnPlayerJumped += OnPlayerJumped;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnPlayerJumped -= OnPlayerJumped;
        }

        private void InitializePresets()
        {
            if (lightShake == null)
            {
                lightShake = new ShakePreset { intensity = 0.1f, duration = 0.1f, frequency = 25f };
            }
            if (mediumShake == null)
            {
                mediumShake = new ShakePreset { intensity = 0.2f, duration = 0.2f, frequency = 20f };
            }
            if (heavyShake == null)
            {
                heavyShake = new ShakePreset { intensity = 0.4f, duration = 0.3f, frequency = 15f };
            }
            if (crashShake == null)
            {
                crashShake = new ShakePreset { intensity = 0.6f, duration = 0.5f, frequency = 12f };
            }
        }

        private void Update()
        {
            if (!shakeEnabled || cameraTransform == null) return;

            // Decay trauma over time
            if (currentTrauma > 0)
            {
                currentTrauma = Mathf.Max(0, currentTrauma - traumaDecay * Time.deltaTime);
                ApplyTraumaShake();
            }
        }

        #region Public API

        /// <summary>
        /// Triggers a light shake effect.
        /// </summary>
        public void ShakeLight()
        {
            Shake(lightShake);
        }

        /// <summary>
        /// Triggers a medium shake effect.
        /// </summary>
        public void ShakeMedium()
        {
            Shake(mediumShake);
        }

        /// <summary>
        /// Triggers a heavy shake effect.
        /// </summary>
        public void ShakeHeavy()
        {
            Shake(heavyShake);
        }

        /// <summary>
        /// Triggers a crash shake effect.
        /// </summary>
        public void ShakeCrash()
        {
            Shake(crashShake);
        }

        /// <summary>
        /// Triggers a custom shake effect.
        /// </summary>
        public void Shake(float intensity, float duration, float frequency = 20f)
        {
            Shake(new ShakePreset { intensity = intensity, duration = duration, frequency = frequency });
        }

        /// <summary>
        /// Triggers a shake using a preset.
        /// </summary>
        public void Shake(ShakePreset preset)
        {
            if (!shakeEnabled || preset == null) return;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(preset));
        }

        /// <summary>
        /// Adds trauma for continuous shake effect.
        /// </summary>
        public void AddTrauma(float amount)
        {
            currentTrauma = Mathf.Clamp01(currentTrauma + amount);
        }

        /// <summary>
        /// Stops all shake effects.
        /// </summary>
        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }

            currentTrauma = 0f;

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalPosition;
            }
        }

        /// <summary>
        /// Enables or disables screen shake.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            shakeEnabled = enabled;
            if (!enabled)
            {
                StopShake();
            }
        }

        /// <summary>
        /// Sets the shake intensity multiplier.
        /// </summary>
        public void SetMultiplier(float multiplier)
        {
            shakeMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        }

        #endregion

        #region Shake Implementation

        private IEnumerator ShakeCoroutine(ShakePreset preset)
        {
            float elapsed = 0f;
            float intensity = preset.intensity * shakeMultiplier;

            while (elapsed < preset.duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / preset.duration;
                float damper = 1f - progress; // Decrease intensity over time

                float offsetX = Mathf.PerlinNoise(Time.time * preset.frequency, 0f) * 2f - 1f;
                float offsetY = Mathf.PerlinNoise(0f, Time.time * preset.frequency) * 2f - 1f;

                Vector3 offset = new Vector3(offsetX, offsetY, 0f) * intensity * damper;
                offset = Vector3.ClampMagnitude(offset, maxShakeOffset);

                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = originalPosition + offset;
                }

                yield return null;
            }

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalPosition;
            }

            shakeCoroutine = null;
        }

        private void ApplyTraumaShake()
        {
            if (cameraTransform == null) return;

            // Use squared trauma for more dramatic falloff
            float shake = currentTrauma * currentTrauma;

            float offsetX = Mathf.PerlinNoise(Time.time * 20f, 0f) * 2f - 1f;
            float offsetY = Mathf.PerlinNoise(0f, Time.time * 20f) * 2f - 1f;

            Vector3 offset = new Vector3(offsetX, offsetY, 0f) * shake * shakeMultiplier * 0.3f;
            offset = Vector3.ClampMagnitude(offset, maxShakeOffset);

            cameraTransform.localPosition = originalPosition + offset;
        }

        #endregion

        #region Event Handlers

        private void OnPlayerCrashed()
        {
            ShakeCrash();
        }

        private void OnPlayerJumped()
        {
            // Small shake on landing (would be called on land, not jump)
        }

        #endregion

        /// <summary>
        /// Called when player lands after jump.
        /// </summary>
        public void OnPlayerLanded()
        {
            ShakeLight();
        }

        /// <summary>
        /// Called for near-miss with obstacle.
        /// </summary>
        public void OnNearMiss()
        {
            ShakeLight();
        }

        private void LateUpdate()
        {
            // Ensure original position is updated if camera moves
            if (cameraTransform != null && shakeCoroutine == null && currentTrauma <= 0)
            {
                originalPosition = cameraTransform.localPosition;
            }
        }
    }

    /// <summary>
    /// Preset configuration for a shake effect.
    /// </summary>
    [System.Serializable]
    public class ShakePreset
    {
        [Tooltip("Maximum offset distance")]
        public float intensity = 0.2f;

        [Tooltip("Duration of the shake in seconds")]
        public float duration = 0.2f;

        [Tooltip("Speed of the shake oscillation")]
        public float frequency = 20f;
    }
}
