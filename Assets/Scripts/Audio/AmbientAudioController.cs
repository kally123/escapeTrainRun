using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Handles ambient audio that changes based on theme and gameplay.
    /// Includes environmental sounds, background noise, and atmosphere.
    /// </summary>
    public class AmbientAudioController : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource primaryAmbient;
        [SerializeField] private AudioSource secondaryAmbient;
        [SerializeField] private AudioSource weatherAmbient;

        [Header("Train Theme Sounds")]
        [SerializeField] private AudioClip trainEngineLoop;
        [SerializeField] private AudioClip trainWheelsLoop;
        [SerializeField] private AudioClip trainWhistle;
        [SerializeField] private AudioClip stationAnnouncement;

        [Header("Bus Theme Sounds")]
        [SerializeField] private AudioClip busEngineLoop;
        [SerializeField] private AudioClip cityTrafficLoop;
        [SerializeField] private AudioClip busHornSound;
        [SerializeField] private AudioClip busStopBell;

        [Header("Ground Theme Sounds")]
        [SerializeField] private AudioClip outdoorAmbienceLoop;
        [SerializeField] private AudioClip birdsLoop;
        [SerializeField] private AudioClip windLoop;
        [SerializeField] private AudioClip crowdMurmur;

        [Header("Weather Sounds")]
        [SerializeField] private AudioClip rainLoop;
        [SerializeField] private AudioClip thunderSound;
        [SerializeField] private AudioClip windStormLoop;

        [Header("Settings")]
        [SerializeField] private float crossfadeDuration = 1.5f;
        [SerializeField] private float baseVolume = 0.5f;
        [SerializeField] private float randomSoundInterval = 10f;
        [SerializeField] private float randomSoundChance = 0.3f;

        // State
        private ThemeType currentTheme;
        private float randomSoundTimer;
        private bool isPlaying;

        private void OnEnable()
        {
            Events.GameEvents.OnThemeChanged += OnThemeChanged;
            Events.GameEvents.OnGameStarted += OnGameStarted;
            Events.GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            Events.GameEvents.OnThemeChanged -= OnThemeChanged;
            Events.GameEvents.OnGameStarted -= OnGameStarted;
            Events.GameEvents.OnGameOver -= OnGameOver;
        }

        private void Update()
        {
            if (isPlaying)
            {
                UpdateRandomSounds();
            }
        }

        private void OnThemeChanged(ThemeType theme)
        {
            currentTheme = theme;
            CrossfadeToTheme(theme);
        }

        private void OnGameStarted()
        {
            isPlaying = true;
            StartAmbientAudio();
        }

        private void OnGameOver(GameOverData data)
        {
            isPlaying = false;
            FadeOutAmbient();
        }

        /// <summary>
        /// Starts ambient audio for the current theme.
        /// </summary>
        public void StartAmbientAudio()
        {
            PlayThemeAmbient(currentTheme);
        }

        /// <summary>
        /// Stops all ambient audio.
        /// </summary>
        public void StopAmbientAudio()
        {
            if (primaryAmbient != null) primaryAmbient.Stop();
            if (secondaryAmbient != null) secondaryAmbient.Stop();
            if (weatherAmbient != null) weatherAmbient.Stop();
        }

        private void PlayThemeAmbient(ThemeType theme)
        {
            AudioClip primary = null;
            AudioClip secondary = null;

            switch (theme)
            {
                case ThemeType.Train:
                    primary = trainEngineLoop;
                    secondary = trainWheelsLoop;
                    break;
                case ThemeType.Bus:
                    primary = busEngineLoop;
                    secondary = cityTrafficLoop;
                    break;
                case ThemeType.Ground:
                    primary = outdoorAmbienceLoop;
                    secondary = birdsLoop;
                    break;
            }

            PlayLoopingAudio(primaryAmbient, primary, baseVolume);
            PlayLoopingAudio(secondaryAmbient, secondary, baseVolume * 0.5f);
        }

        private void PlayLoopingAudio(AudioSource source, AudioClip clip, float volume)
        {
            if (source == null || clip == null) return;

            source.clip = clip;
            source.volume = volume;
            source.loop = true;
            source.Play();
        }

        private void CrossfadeToTheme(ThemeType theme)
        {
            StartCoroutine(CrossfadeCoroutine(theme));
        }

        private System.Collections.IEnumerator CrossfadeCoroutine(ThemeType theme)
        {
            // Fade out current
            float elapsed = 0f;
            float startVolume = primaryAmbient != null ? primaryAmbient.volume : 0f;

            while (elapsed < crossfadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (crossfadeDuration / 2f);

                if (primaryAmbient != null)
                    primaryAmbient.volume = Mathf.Lerp(startVolume, 0f, t);
                if (secondaryAmbient != null)
                    secondaryAmbient.volume = Mathf.Lerp(startVolume * 0.5f, 0f, t);

                yield return null;
            }

            // Switch clips
            PlayThemeAmbient(theme);

            // Fade in new
            elapsed = 0f;
            while (elapsed < crossfadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (crossfadeDuration / 2f);

                if (primaryAmbient != null)
                    primaryAmbient.volume = Mathf.Lerp(0f, baseVolume, t);
                if (secondaryAmbient != null)
                    secondaryAmbient.volume = Mathf.Lerp(0f, baseVolume * 0.5f, t);

                yield return null;
            }
        }

        private void FadeOutAmbient()
        {
            StartCoroutine(FadeOutCoroutine());
        }

        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            float elapsed = 0f;
            float duration = 1f;
            float startPrimary = primaryAmbient != null ? primaryAmbient.volume : 0f;
            float startSecondary = secondaryAmbient != null ? secondaryAmbient.volume : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                if (primaryAmbient != null)
                    primaryAmbient.volume = Mathf.Lerp(startPrimary, 0f, t);
                if (secondaryAmbient != null)
                    secondaryAmbient.volume = Mathf.Lerp(startSecondary, 0f, t);

                yield return null;
            }

            StopAmbientAudio();
        }

        private void UpdateRandomSounds()
        {
            randomSoundTimer -= Time.deltaTime;

            if (randomSoundTimer <= 0f)
            {
                randomSoundTimer = randomSoundInterval;

                if (Random.value < randomSoundChance)
                {
                    PlayRandomThemeSound();
                }
            }
        }

        private void PlayRandomThemeSound()
        {
            AudioClip randomClip = null;

            switch (currentTheme)
            {
                case ThemeType.Train:
                    randomClip = Random.value > 0.5f ? trainWhistle : stationAnnouncement;
                    break;
                case ThemeType.Bus:
                    randomClip = Random.value > 0.5f ? busHornSound : busStopBell;
                    break;
                case ThemeType.Ground:
                    randomClip = Random.value > 0.5f ? windLoop : crowdMurmur;
                    break;
            }

            if (randomClip != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(randomClip);
            }
        }

        /// <summary>
        /// Starts weather effects audio.
        /// </summary>
        public void StartWeatherAudio(WeatherType weather)
        {
            if (weatherAmbient == null) return;

            AudioClip weatherClip = weather switch
            {
                WeatherType.Rain => rainLoop,
                WeatherType.Storm => windStormLoop,
                _ => null
            };

            if (weatherClip != null)
            {
                weatherAmbient.clip = weatherClip;
                weatherAmbient.loop = true;
                weatherAmbient.volume = baseVolume * 0.7f;
                weatherAmbient.Play();
            }
        }

        /// <summary>
        /// Stops weather effects audio.
        /// </summary>
        public void StopWeatherAudio()
        {
            if (weatherAmbient != null)
            {
                weatherAmbient.Stop();
            }
        }

        /// <summary>
        /// Plays thunder sound effect.
        /// </summary>
        public void PlayThunder()
        {
            if (thunderSound != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(thunderSound);
            }
        }
    }

    /// <summary>
    /// Weather types for ambient audio.
    /// </summary>
    public enum WeatherType
    {
        Clear,
        Rain,
        Storm
    }
}
