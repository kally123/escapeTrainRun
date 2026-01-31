using UnityEngine;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Audio mixer controller for dynamic audio mixing.
    /// Handles ducking, filtering, and audio effects.
    /// </summary>
    public class AudioMixerController : MonoBehaviour
    {
        [Header("Mixer Groups")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup masterGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup musicGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup ambientGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup uiGroup;

        [Header("Ducking Settings")]
        [SerializeField] private float duckVolume = -10f;
        [SerializeField] private float duckDuration = 0.3f;
        [SerializeField] private float normalVolume = 0f;

        [Header("Low Pass Filter")]
        [SerializeField] private float pausedCutoff = 500f;
        [SerializeField] private float normalCutoff = 22000f;

        // Parameter names
        private const string MASTER_VOLUME = "MasterVolume";
        private const string MUSIC_VOLUME = "MusicVolume";
        private const string SFX_VOLUME = "SFXVolume";
        private const string AMBIENT_VOLUME = "AmbientVolume";
        private const string LOWPASS_CUTOFF = "LowpassCutoff";

        private UnityEngine.Audio.AudioMixer mixer;
        private Coroutine duckingCoroutine;

        private void Awake()
        {
            if (masterGroup != null)
            {
                mixer = masterGroup.audioMixer;
            }
        }

        private void OnEnable()
        {
            Events.GameEvents.OnGamePaused += OnGamePaused;
            Events.GameEvents.OnGameResumed += OnGameResumed;
        }

        private void OnDisable()
        {
            Events.GameEvents.OnGamePaused -= OnGamePaused;
            Events.GameEvents.OnGameResumed -= OnGameResumed;
        }

        private void OnGamePaused()
        {
            ApplyPausedEffect();
        }

        private void OnGameResumed()
        {
            RemovePausedEffect();
        }

        /// <summary>
        /// Sets the master volume on the mixer.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            if (mixer == null) return;
            float dbVolume = LinearToDecibels(volume);
            mixer.SetFloat(MASTER_VOLUME, dbVolume);
        }

        /// <summary>
        /// Sets the music volume on the mixer.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (mixer == null) return;
            float dbVolume = LinearToDecibels(volume);
            mixer.SetFloat(MUSIC_VOLUME, dbVolume);
        }

        /// <summary>
        /// Sets the SFX volume on the mixer.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (mixer == null) return;
            float dbVolume = LinearToDecibels(volume);
            mixer.SetFloat(SFX_VOLUME, dbVolume);
        }

        /// <summary>
        /// Sets the ambient volume on the mixer.
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            if (mixer == null) return;
            float dbVolume = LinearToDecibels(volume);
            mixer.SetFloat(AMBIENT_VOLUME, dbVolume);
        }

        /// <summary>
        /// Ducks the music temporarily (for important sounds).
        /// </summary>
        public void DuckMusic(float duration = 1f)
        {
            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }
            duckingCoroutine = StartCoroutine(DuckingCoroutine(duration));
        }

        private System.Collections.IEnumerator DuckingCoroutine(float duration)
        {
            // Fade down
            yield return StartCoroutine(FadeMixerVolume(MUSIC_VOLUME, duckVolume, duckDuration));

            // Hold
            yield return new WaitForSeconds(duration);

            // Fade back up
            yield return StartCoroutine(FadeMixerVolume(MUSIC_VOLUME, normalVolume, duckDuration));

            duckingCoroutine = null;
        }

        private System.Collections.IEnumerator FadeMixerVolume(string parameter, float targetVolume, float duration)
        {
            if (mixer == null) yield break;

            mixer.GetFloat(parameter, out float currentVolume);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float newVolume = Mathf.Lerp(currentVolume, targetVolume, t);
                mixer.SetFloat(parameter, newVolume);
                yield return null;
            }

            mixer.SetFloat(parameter, targetVolume);
        }

        /// <summary>
        /// Applies a low-pass filter effect (muffled sound for pause).
        /// </summary>
        public void ApplyPausedEffect()
        {
            if (mixer == null) return;
            StartCoroutine(TransitionLowPass(pausedCutoff, 0.3f));
        }

        /// <summary>
        /// Removes the low-pass filter effect.
        /// </summary>
        public void RemovePausedEffect()
        {
            if (mixer == null) return;
            StartCoroutine(TransitionLowPass(normalCutoff, 0.3f));
        }

        private System.Collections.IEnumerator TransitionLowPass(float targetCutoff, float duration)
        {
            mixer.GetFloat(LOWPASS_CUTOFF, out float currentCutoff);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float newCutoff = Mathf.Lerp(currentCutoff, targetCutoff, t);
                mixer.SetFloat(LOWPASS_CUTOFF, newCutoff);
                yield return null;
            }

            mixer.SetFloat(LOWPASS_CUTOFF, targetCutoff);
        }

        /// <summary>
        /// Converts linear volume (0-1) to decibels.
        /// </summary>
        private float LinearToDecibels(float linear)
        {
            if (linear <= 0f) return -80f;
            return Mathf.Log10(linear) * 20f;
        }

        /// <summary>
        /// Converts decibels to linear volume (0-1).
        /// </summary>
        private float DecibelsToLinear(float decibels)
        {
            return Mathf.Pow(10f, decibels / 20f);
        }
    }
}
