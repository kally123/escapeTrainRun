using UnityEngine;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Controls post-processing effects for visual feedback.
    /// Works with Unity's Post Processing Stack or URP Post Processing.
    /// </summary>
    public class PostProcessingController : MonoBehaviour
    {
        public static PostProcessingController Instance { get; private set; }

        [Header("Effect References")]
        [SerializeField] private Material chromaticAberrationMat;
        [SerializeField] private Material vignettemat;
        [SerializeField] private Material radialBlurMat;
        [SerializeField] private Material colorGradingMat;

        [Header("Speed Lines")]
        [SerializeField] private GameObject speedLinesEffect;
        [SerializeField] private float speedLinesThreshold = 15f;

        [Header("Chromatic Aberration")]
        [SerializeField] private float maxChromaticAberration = 0.1f;
        [SerializeField] private float aberrationDecaySpeed = 2f;

        [Header("Vignette")]
        [SerializeField] private float normalVignette = 0.3f;
        [SerializeField] private float damageVignette = 0.6f;
        [SerializeField] private float starPowerVignette = 0.1f;

        [Header("Motion Blur")]
        [SerializeField] private float speedBoostBlur = 0.3f;
        [SerializeField] private float normalBlur = 0f;

        [Header("Color Grading")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color starPowerColor = new Color(1f, 0.9f, 0.7f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.8f, 0.8f);

        [Header("Settings")]
        [SerializeField] private bool effectsEnabled = true;
        [SerializeField] private float transitionSpeed = 3f;

        // State
        private float currentAberration;
        private float targetVignette;
        private float targetBlur;
        private Color targetColor;
        private bool isStarPowerActive;
        private bool isSpeedBoostActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            targetVignette = normalVignette;
            targetColor = normalColor;
        }

        private void OnEnable()
        {
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void Update()
        {
            if (!effectsEnabled) return;

            UpdateChromaticAberration();
            UpdateSpeedLines();
        }

        #region Public API

        /// <summary>
        /// Triggers a chromatic aberration pulse.
        /// </summary>
        public void PulseAberration(float intensity = 1f)
        {
            currentAberration = maxChromaticAberration * intensity;
        }

        /// <summary>
        /// Sets the vignette intensity.
        /// </summary>
        public void SetVignette(float intensity)
        {
            targetVignette = intensity;
        }

        /// <summary>
        /// Sets the motion blur amount.
        /// </summary>
        public void SetMotionBlur(float amount)
        {
            targetBlur = amount;
        }

        /// <summary>
        /// Sets the color grading tint.
        /// </summary>
        public void SetColorGrading(Color color)
        {
            targetColor = color;
        }

        /// <summary>
        /// Applies star power visual effects.
        /// </summary>
        public void EnableStarPowerEffects()
        {
            isStarPowerActive = true;
            targetVignette = starPowerVignette;
            targetColor = starPowerColor;

            if (speedLinesEffect != null)
            {
                speedLinesEffect.SetActive(true);
            }
        }

        /// <summary>
        /// Removes star power visual effects.
        /// </summary>
        public void DisableStarPowerEffects()
        {
            isStarPowerActive = false;
            ResetToNormal();
        }

        /// <summary>
        /// Applies speed boost visual effects.
        /// </summary>
        public void EnableSpeedBoostEffects()
        {
            isSpeedBoostActive = true;
            targetBlur = speedBoostBlur;

            if (speedLinesEffect != null)
            {
                speedLinesEffect.SetActive(true);
            }
        }

        /// <summary>
        /// Removes speed boost visual effects.
        /// </summary>
        public void DisableSpeedBoostEffects()
        {
            isSpeedBoostActive = false;
            targetBlur = normalBlur;

            if (!isStarPowerActive && speedLinesEffect != null)
            {
                speedLinesEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Applies damage visual effects.
        /// </summary>
        public void ApplyDamageEffects()
        {
            targetVignette = damageVignette;
            targetColor = dangerColor;
            PulseAberration(1.5f);

            // Reset after delay
            StartCoroutine(ResetDamageEffects(0.5f));
        }

        /// <summary>
        /// Resets all effects to normal.
        /// </summary>
        public void ResetToNormal()
        {
            if (!isStarPowerActive)
            {
                targetVignette = normalVignette;
                targetColor = normalColor;
            }

            if (!isSpeedBoostActive && !isStarPowerActive)
            {
                targetBlur = normalBlur;
                if (speedLinesEffect != null)
                {
                    speedLinesEffect.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Enables or disables post-processing effects.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            effectsEnabled = enabled;
            if (!enabled)
            {
                ResetAllEffects();
            }
        }

        /// <summary>
        /// Updates speed lines based on current speed.
        /// </summary>
        public void UpdateSpeedBasedEffects(float speed)
        {
            if (!effectsEnabled) return;

            // Speed lines appear at high speeds
            if (speedLinesEffect != null && !isStarPowerActive && !isSpeedBoostActive)
            {
                speedLinesEffect.SetActive(speed > speedLinesThreshold);
            }

            // Slight aberration at high speeds
            if (speed > speedLinesThreshold)
            {
                float speedFactor = Mathf.InverseLerp(speedLinesThreshold, speedLinesThreshold * 2f, speed);
                currentAberration = Mathf.Max(currentAberration, maxChromaticAberration * 0.3f * speedFactor);
            }
        }

        #endregion

        #region Effect Updates

        private void UpdateChromaticAberration()
        {
            if (currentAberration > 0)
            {
                currentAberration -= aberrationDecaySpeed * Time.deltaTime;
                currentAberration = Mathf.Max(0, currentAberration);

                // Apply to shader if available
                if (chromaticAberrationMat != null)
                {
                    chromaticAberrationMat.SetFloat("_Intensity", currentAberration);
                }
            }
        }

        private void UpdateSpeedLines()
        {
            // Speed lines intensity could be updated here
        }

        private IEnumerator ResetDamageEffects(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!isStarPowerActive)
            {
                targetVignette = normalVignette;
                targetColor = normalColor;
            }
        }

        private void ResetAllEffects()
        {
            currentAberration = 0f;
            targetVignette = normalVignette;
            targetBlur = normalBlur;
            targetColor = normalColor;
            isStarPowerActive = false;
            isSpeedBoostActive = false;

            if (speedLinesEffect != null)
            {
                speedLinesEffect.SetActive(false);
            }
        }

        #endregion

        #region Event Handlers

        private void OnPowerUpActivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.StarPower:
                    EnableStarPowerEffects();
                    break;
                case PowerUpType.SpeedBoost:
                    EnableSpeedBoostEffects();
                    break;
                default:
                    PulseAberration(0.5f);
                    break;
            }
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.StarPower:
                    DisableStarPowerEffects();
                    break;
                case PowerUpType.SpeedBoost:
                    DisableSpeedBoostEffects();
                    break;
            }
        }

        private void OnPlayerCrashed()
        {
            ApplyDamageEffects();
        }

        private void OnGameStarted()
        {
            ResetToNormal();
        }

        private void OnGameOver(GameOverData data)
        {
            // Slow motion / desaturate effect could go here
        }

        #endregion
    }
}
