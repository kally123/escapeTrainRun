using UnityEngine;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Controls slow motion effects for dramatic moments.
    /// </summary>
    public class SlowMotionController : MonoBehaviour
    {
        public static SlowMotionController Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float slowMotionScale = 0.3f;
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private bool affectAudio = true;

        [Header("Presets")]
        [SerializeField] private float crashSlowMo = 0.2f;
        [SerializeField] private float crashDuration = 1f;
        [SerializeField] private float nearMissSlowMo = 0.5f;
        [SerializeField] private float nearMissDuration = 0.3f;

        // State
        private float originalTimeScale = 1f;
        private float originalFixedDeltaTime;
        private Coroutine slowMoCoroutine;
        private bool isSlowMotion;

        public bool IsSlowMotion => isSlowMotion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnGameResumed += OnGameResumed;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnGameResumed -= OnGameResumed;
        }

        #region Public API

        /// <summary>
        /// Triggers slow motion effect.
        /// </summary>
        public void TriggerSlowMotion(float timeScale, float duration)
        {
            if (slowMoCoroutine != null)
            {
                StopCoroutine(slowMoCoroutine);
            }

            slowMoCoroutine = StartCoroutine(SlowMotionCoroutine(timeScale, duration));
        }

        /// <summary>
        /// Triggers a crash slow motion effect.
        /// </summary>
        public void TriggerCrashSlowMo()
        {
            TriggerSlowMotion(crashSlowMo, crashDuration);
        }

        /// <summary>
        /// Triggers a near miss slow motion effect.
        /// </summary>
        public void TriggerNearMissSlowMo()
        {
            TriggerSlowMotion(nearMissSlowMo, nearMissDuration);
        }

        /// <summary>
        /// Immediately resets time to normal.
        /// </summary>
        public void ResetTimeScale()
        {
            if (slowMoCoroutine != null)
            {
                StopCoroutine(slowMoCoroutine);
                slowMoCoroutine = null;
            }

            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            isSlowMotion = false;

            ResetAudioPitch();
        }

        /// <summary>
        /// Sets the slow motion scale.
        /// </summary>
        public void SetSlowMotionScale(float scale)
        {
            slowMotionScale = Mathf.Clamp(scale, 0.1f, 1f);
        }

        /// <summary>
        /// Pauses time completely (for menus, etc.)
        /// </summary>
        public void PauseTime()
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Resumes time from pause.
        /// </summary>
        public void ResumeTime()
        {
            Time.timeScale = originalTimeScale;
        }

        #endregion

        #region Coroutines

        private IEnumerator SlowMotionCoroutine(float targetTimeScale, float duration)
        {
            isSlowMotion = true;
            originalTimeScale = Time.timeScale;

            // Transition into slow motion
            yield return StartCoroutine(TransitionTimeScale(targetTimeScale, transitionDuration));

            // Hold slow motion
            // Use unscaled time for consistent duration
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Transition back to normal
            yield return StartCoroutine(TransitionTimeScale(1f, transitionDuration));

            isSlowMotion = false;
            slowMoCoroutine = null;
        }

        private IEnumerator TransitionTimeScale(float targetScale, float duration)
        {
            float startScale = Time.timeScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                t = EaseInOutQuad(t);

                float newScale = Mathf.Lerp(startScale, targetScale, t);
                SetTimeScale(newScale);

                yield return null;
            }

            SetTimeScale(targetScale);
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = originalFixedDeltaTime * scale;

            if (affectAudio)
            {
                SetAudioPitch(scale);
            }
        }

        #endregion

        #region Audio

        private void SetAudioPitch(float pitch)
        {
            // Clamp pitch to avoid extreme values
            pitch = Mathf.Clamp(pitch, 0.5f, 1f);

            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                // Could adjust music pitch here
                // audio.SetMusicPitch(pitch);
            }
        }

        private void ResetAudioPitch()
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                // audio.SetMusicPitch(1f);
            }
        }

        #endregion

        #region Event Handlers

        private void OnPlayerCrashed()
        {
            TriggerCrashSlowMo();
        }

        private void OnGameResumed()
        {
            ResetTimeScale();
        }

        #endregion

        #region Helpers

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        #endregion

        private void OnDestroy()
        {
            // Ensure time is reset
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
    }
}
