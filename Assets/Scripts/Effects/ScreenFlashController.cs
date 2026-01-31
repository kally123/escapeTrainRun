using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Controls screen flash effects for impacts, power-ups, and feedback.
    /// </summary>
    public class ScreenFlashController : MonoBehaviour
    {
        public static ScreenFlashController Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image flashOverlay;
        [SerializeField] private Image vignetteOverlay;
        [SerializeField] private CanvasGroup flashCanvasGroup;

        [Header("Flash Colors")]
        [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.5f);
        [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color powerUpFlashColor = new Color(1f, 1f, 0f, 0.4f);
        [SerializeField] private Color starPowerFlashColor = new Color(1f, 0.8f, 0f, 0.6f);
        [SerializeField] private Color speedFlashColor = new Color(0f, 0.5f, 1f, 0.3f);
        [SerializeField] private Color whiteFlashColor = new Color(1f, 1f, 1f, 0.8f);

        [Header("Vignette Settings")]
        [SerializeField] private Color dangerVignetteColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private float vignetteIntensity = 0.5f;
        [SerializeField] private float vignettePulseSpeed = 2f;

        [Header("Settings")]
        [SerializeField] private float defaultFlashDuration = 0.2f;
        [SerializeField] private bool effectsEnabled = true;

        // State
        private Coroutine flashCoroutine;
        private Coroutine vignetteCoroutine;
        private bool isDangerVignetteActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeOverlays();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
        }

        private void InitializeOverlays()
        {
            if (flashOverlay != null)
            {
                flashOverlay.color = Color.clear;
                flashOverlay.raycastTarget = false;
            }

            if (vignetteOverlay != null)
            {
                vignetteOverlay.color = Color.clear;
                vignetteOverlay.raycastTarget = false;
            }
        }

        #region Public API

        /// <summary>
        /// Flashes the screen with damage color.
        /// </summary>
        public void FlashDamage()
        {
            Flash(damageFlashColor, defaultFlashDuration);
        }

        /// <summary>
        /// Flashes the screen with heal color.
        /// </summary>
        public void FlashHeal()
        {
            Flash(healFlashColor, defaultFlashDuration);
        }

        /// <summary>
        /// Flashes the screen with power-up color.
        /// </summary>
        public void FlashPowerUp()
        {
            Flash(powerUpFlashColor, defaultFlashDuration * 0.5f);
        }

        /// <summary>
        /// Flashes the screen with star power color.
        /// </summary>
        public void FlashStarPower()
        {
            Flash(starPowerFlashColor, defaultFlashDuration);
        }

        /// <summary>
        /// Flashes the screen with speed boost color.
        /// </summary>
        public void FlashSpeed()
        {
            Flash(speedFlashColor, defaultFlashDuration);
        }

        /// <summary>
        /// Flashes the screen white (for transitions, etc.)
        /// </summary>
        public void FlashWhite(float duration = 0.3f)
        {
            Flash(whiteFlashColor, duration);
        }

        /// <summary>
        /// Flashes the screen with a custom color.
        /// </summary>
        public void Flash(Color color, float duration)
        {
            if (!effectsEnabled || flashOverlay == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
        }

        /// <summary>
        /// Starts the danger vignette pulsing effect.
        /// </summary>
        public void StartDangerVignette()
        {
            if (!effectsEnabled || vignetteOverlay == null) return;

            isDangerVignetteActive = true;

            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
            }

            vignetteCoroutine = StartCoroutine(DangerVignetteCoroutine());
        }

        /// <summary>
        /// Stops the danger vignette effect.
        /// </summary>
        public void StopDangerVignette()
        {
            isDangerVignetteActive = false;

            if (vignetteCoroutine != null)
            {
                StopCoroutine(vignetteCoroutine);
                vignetteCoroutine = null;
            }

            if (vignetteOverlay != null)
            {
                vignetteOverlay.color = Color.clear;
            }
        }

        /// <summary>
        /// Shows a fade-in effect.
        /// </summary>
        public void FadeIn(float duration = 0.5f, Color? color = null)
        {
            StartCoroutine(FadeCoroutine(color ?? Color.black, 1f, 0f, duration));
        }

        /// <summary>
        /// Shows a fade-out effect.
        /// </summary>
        public void FadeOut(float duration = 0.5f, Color? color = null)
        {
            StartCoroutine(FadeCoroutine(color ?? Color.black, 0f, 1f, duration));
        }

        /// <summary>
        /// Enables or disables screen effects.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            effectsEnabled = enabled;
            if (!enabled)
            {
                StopAllEffects();
            }
        }

        /// <summary>
        /// Stops all screen effects.
        /// </summary>
        public void StopAllEffects()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            StopDangerVignette();

            if (flashOverlay != null)
            {
                flashOverlay.color = Color.clear;
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator FlashCoroutine(Color color, float duration)
        {
            flashOverlay.color = color;

            float elapsed = 0f;
            Color startColor = color;
            Color endColor = Color.clear;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                t = EaseOutQuad(t);
                flashOverlay.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            flashOverlay.color = Color.clear;
            flashCoroutine = null;
        }

        private IEnumerator DangerVignetteCoroutine()
        {
            while (isDangerVignetteActive)
            {
                // Pulse effect
                float pulse = (Mathf.Sin(Time.time * vignettePulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
                float alpha = pulse * vignetteIntensity;

                Color vignetteColor = dangerVignetteColor;
                vignetteColor.a = alpha;
                vignetteOverlay.color = vignetteColor;

                yield return null;
            }

            vignetteOverlay.color = Color.clear;
        }

        private IEnumerator FadeCoroutine(Color color, float startAlpha, float endAlpha, float duration)
        {
            Color startColor = color;
            startColor.a = startAlpha;
            Color endColor = color;
            endColor.a = endAlpha;

            flashOverlay.color = startColor;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                flashOverlay.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            flashOverlay.color = endColor;
        }

        #endregion

        #region Event Handlers

        private void OnPlayerCrashed()
        {
            FlashDamage();
        }

        private void OnPowerUpActivated(Collectibles.PowerUpType type)
        {
            switch (type)
            {
                case Collectibles.PowerUpType.StarPower:
                    FlashStarPower();
                    break;
                case Collectibles.PowerUpType.SpeedBoost:
                    FlashSpeed();
                    break;
                default:
                    FlashPowerUp();
                    break;
            }
        }

        #endregion

        #region Helpers

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        #endregion
    }
}
