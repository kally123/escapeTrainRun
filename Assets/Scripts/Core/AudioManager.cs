using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Centralized audio management system.
    /// Handles music, sound effects, ambient audio, and 3D spatial audio.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource uiSource;

        [Header("3D Audio Pool")]
        [SerializeField] private int spatialAudioPoolSize = 10;
        [SerializeField] private GameObject spatialAudioPrefab;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip trainMusic;
        [SerializeField] private AudioClip busMusic;
        [SerializeField] private AudioClip groundMusic;
        [SerializeField] private AudioClip gameOverJingle;
        [SerializeField] private AudioClip highScoreJingle;

        [Header("Ambient Sounds")]
        [SerializeField] private AudioClip trainAmbient;
        [SerializeField] private AudioClip busAmbient;
        [SerializeField] private AudioClip groundAmbient;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip slideSound;
        [SerializeField] private AudioClip laneChangeSound;
        [SerializeField] private AudioClip coinCollectSound;
        [SerializeField] private AudioClip powerUpSound;
        [SerializeField] private AudioClip crashSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip achievementSound;

        [Header("Power-Up Sounds")]
        [SerializeField] private AudioClip magnetSound;
        [SerializeField] private AudioClip shieldSound;
        [SerializeField] private AudioClip shieldBreakSound;
        [SerializeField] private AudioClip speedBoostSound;
        [SerializeField] private AudioClip starPowerSound;
        [SerializeField] private AudioClip multiplierSound;
        [SerializeField] private AudioClip powerUpWarningSound;
        [SerializeField] private AudioClip powerUpEndSound;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip countdownSound;
        [SerializeField] private AudioClip scoreTickSound;

        [Header("Gameplay Sounds")]
        [SerializeField] private AudioClip nearMissSound;
        [SerializeField] private AudioClip obstaclePassSound;
        [SerializeField] private AudioClip comboSound;
        [SerializeField] private AudioClip milestoneSound;
        [SerializeField] private AudioClip warningSound;

        [Header("Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ambientVolume = 0.5f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float pitchVariation = 0.1f;

        // Audio state
        private bool isMuted;
        private bool isMusicEnabled = true;
        private bool isSfxEnabled = true;
        private Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();
        private List<AudioSource> spatialAudioPool = new List<AudioSource>();
        private int spatialAudioIndex;
        private Coroutine musicFadeCoroutine;
        private ThemeType currentTheme;

        private void Awake()
        {
            InitializeSingleton();
            LoadAudioSettings();
            RegisterServices();
            InitializeSpatialAudioPool();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        private void InitializeSpatialAudioPool()
        {
            for (int i = 0; i < spatialAudioPoolSize; i++)
            {
                GameObject audioObj;
                if (spatialAudioPrefab != null)
                {
                    audioObj = Instantiate(spatialAudioPrefab, transform);
                }
                else
                {
                    audioObj = new GameObject($"SpatialAudio_{i}");
                    audioObj.transform.SetParent(transform);
                    var source = audioObj.AddComponent<AudioSource>();
                    source.spatialBlend = 1f; // Full 3D
                    source.rolloffMode = AudioRolloffMode.Linear;
                    source.minDistance = 1f;
                    source.maxDistance = 30f;
                    source.playOnAwake = false;
                }
                
                var audioSource = audioObj.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    spatialAudioPool.Add(audioSource);
                }
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnPlayerJumped += PlayJumpSound;
            GameEvents.OnPlayerSlide += PlaySlideSound;
            GameEvents.OnLaneChanged += (_) => PlayLaneChangeSound();
            GameEvents.OnCoinsCollected += OnCoinsCollected;
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
            GameEvents.OnPlayerCrashed += PlayCrashSound;
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGameOver += OnGameOver;
            GameEvents.OnThemeChanged += OnThemeChanged;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnPlayerJumped -= PlayJumpSound;
            GameEvents.OnPlayerSlide -= PlaySlideSound;
            GameEvents.OnLaneChanged -= (_) => PlayLaneChangeSound();
            GameEvents.OnCoinsCollected -= OnCoinsCollected;
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
            GameEvents.OnPlayerCrashed -= PlayCrashSound;
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnThemeChanged -= OnThemeChanged;
        }

        private void OnCoinsCollected(int amount)
        {
            // Pitch variation based on combo/amount
            float pitch = 1f + (amount - 1) * 0.05f;
            PlaySFXWithPitch(coinCollectSound, Mathf.Clamp(pitch, 0.8f, 1.5f));
        }

        private void OnPowerUpActivated(Collectibles.PowerUpType type)
        {
            AudioClip clip = GetPowerUpSound(type);
            PlaySFX(clip ?? powerUpSound);
        }

        private void OnPowerUpDeactivated(Collectibles.PowerUpType type)
        {
            PlaySFX(powerUpEndSound);
        }

        private AudioClip GetPowerUpSound(Collectibles.PowerUpType type)
        {
            return type switch
            {
                Collectibles.PowerUpType.Magnet => magnetSound,
                Collectibles.PowerUpType.Shield => shieldSound,
                Collectibles.PowerUpType.SpeedBoost => speedBoostSound,
                Collectibles.PowerUpType.StarPower => starPowerSound,
                Collectibles.PowerUpType.Multiplier => multiplierSound,
                _ => powerUpSound
            };
        }

        #region Music Control

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!isMusicEnabled || clip == null) return;

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        /// <summary>
        /// Crossfades to a new music track.
        /// </summary>
        public void CrossfadeMusic(AudioClip newClip, float duration = 1f, bool loop = true)
        {
            if (!isMusicEnabled || newClip == null) return;

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            musicFadeCoroutine = StartCoroutine(CrossfadeMusicCoroutine(newClip, duration, loop));
        }

        private IEnumerator CrossfadeMusicCoroutine(AudioClip newClip, float duration, bool loop)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            // Fade out
            while (elapsed < duration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
                yield return null;
            }

            // Switch clip
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in
            elapsed = 0f;
            float targetVolume = musicVolume * masterVolume;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration / 2f));
                yield return null;
            }

            musicSource.volume = targetVolume;
            musicFadeCoroutine = null;
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void FadeOutMusic(float duration = 1f)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeOutMusicCoroutine(duration));
        }

        private IEnumerator FadeOutMusicCoroutine(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = musicVolume * masterVolume;
            musicFadeCoroutine = null;
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (isMusicEnabled)
            {
                musicSource.UnPause();
            }
        }

        public void PlayMenuMusic()
        {
            CrossfadeMusic(menuMusic);
        }

        private void OnGameStarted()
        {
            // Start ambient sound
            PlayThemeAmbient(currentTheme);
        }

        private void OnGameOver(GameOverData data)
        {
            StopAmbient();
            
            if (data.IsHighScore && highScoreJingle != null)
            {
                PlayMusic(highScoreJingle, false);
            }
            else
            {
                PlayMusic(gameOverJingle, false);
            }
        }

        private void OnThemeChanged(ThemeType theme)
        {
            currentTheme = theme;
            AudioClip themeMusic = theme switch
            {
                ThemeType.Train => trainMusic,
                ThemeType.Bus => busMusic,
                ThemeType.Ground => groundMusic,
                _ => trainMusic
            };
            CrossfadeMusic(themeMusic);
        }

        #endregion

        #region Ambient Audio

        /// <summary>
        /// Plays theme-specific ambient sounds.
        /// </summary>
        public void PlayThemeAmbient(ThemeType theme)
        {
            AudioClip ambient = theme switch
            {
                ThemeType.Train => trainAmbient,
                ThemeType.Bus => busAmbient,
                ThemeType.Ground => groundAmbient,
                _ => trainAmbient
            };

            PlayAmbient(ambient);
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (ambientSource == null || clip == null) return;

            ambientSource.clip = clip;
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
            {
                ambientSource.volume = ambientVolume * masterVolume;
            }
        }

        #endregion

        #region Sound Effects

        public void PlaySFX(AudioClip clip)
        {
            if (!isSfxEnabled || clip == null || isMuted) return;
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        /// <summary>
        /// Plays a sound effect with custom pitch.
        /// </summary>
        public void PlaySFXWithPitch(AudioClip clip, float pitch)
        {
            if (!isSfxEnabled || clip == null || isMuted) return;
            
            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
            sfxSource.pitch = originalPitch;
        }

        /// <summary>
        /// Plays a sound effect with random pitch variation.
        /// </summary>
        public void PlaySFXVaried(AudioClip clip)
        {
            if (!isSfxEnabled || clip == null || isMuted) return;
            
            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            PlaySFXWithPitch(clip, pitch);
        }

        /// <summary>
        /// Plays a 3D spatial sound at the specified position.
        /// </summary>
        public void PlaySpatialSFX(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (!isSfxEnabled || clip == null || isMuted) return;
            if (spatialAudioPool.Count == 0) return;

            var source = spatialAudioPool[spatialAudioIndex];
            spatialAudioIndex = (spatialAudioIndex + 1) % spatialAudioPool.Count;

            source.transform.position = position;
            source.volume = volume * sfxVolume * masterVolume;
            source.clip = clip;
            source.Play();
        }

        /// <summary>
        /// Plays a UI sound effect (unaffected by game pause).
        /// </summary>
        public void PlayUISound(AudioClip clip)
        {
            if (!isSfxEnabled || clip == null || isMuted) return;
            
            if (uiSource != null)
            {
                uiSource.PlayOneShot(clip, sfxVolume * masterVolume);
            }
            else
            {
                PlaySFX(clip);
            }
        }

        public void PlayJumpSound()
        {
            PlaySFXVaried(jumpSound);
        }

        public void PlaySlideSound()
        {
            PlaySFX(slideSound);
        }

        public void PlayLaneChangeSound()
        {
            PlaySFXVaried(laneChangeSound);
        }

        public void PlayCoinSound()
        {
            PlaySFXVaried(coinCollectSound);
        }

        public void PlayPowerUpSound()
        {
            PlaySFX(powerUpSound);
        }

        public void PlayCrashSound()
        {
            PlaySFX(crashSound);
        }

        public void PlayButtonClick()
        {
            PlayUISound(buttonClickSound);
        }

        public void PlayAchievementSound()
        {
            PlaySFX(achievementSound);
        }

        // Additional sound methods
        public void PlayPanelOpen() => PlayUISound(panelOpenSound);
        public void PlayPanelClose() => PlayUISound(panelCloseSound);
        public void PlayPurchase() => PlayUISound(purchaseSound);
        public void PlayError() => PlayUISound(errorSound);
        public void PlayUnlock() => PlayUISound(unlockSound);
        public void PlayCountdown() => PlayUISound(countdownSound);
        public void PlayScoreTick() => PlayUISound(scoreTickSound);
        public void PlayNearMiss() => PlaySFX(nearMissSound);
        public void PlayObstaclePass() => PlaySFXVaried(obstaclePassSound);
        public void PlayCombo() => PlaySFX(comboSound);
        public void PlayMilestone() => PlaySFX(milestoneSound);
        public void PlayWarning() => PlaySFX(warningSound);
        public void PlayShieldBreak() => PlaySFX(shieldBreakSound);
        public void PlayPowerUpWarning() => PlaySFX(powerUpWarningSound);
        public void PlayObstacleDestroy() => PlaySFXVaried(crashSound); // Use crash sound variant for destruction

        #endregion

        #region Settings

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveAudioSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume;
            SaveAudioSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveAudioSettings();
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            AudioListener.volume = muted ? 0f : 1f;
            SaveAudioSettings();
        }

        public void SetMusicEnabled(bool enabled)
        {
            isMusicEnabled = enabled;
            if (!enabled)
            {
                musicSource.Pause();
            }
            else
            {
                musicSource.UnPause();
            }
            SaveAudioSettings();
        }

        public void SetSFXEnabled(bool enabled)
        {
            isSfxEnabled = enabled;
            SaveAudioSettings();
        }

        private void ApplyVolumeSettings()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;
        }

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;
        public float AmbientVolume => ambientVolume;
        public bool IsMuted => isMuted;
        public bool IsMusicEnabled => isMusicEnabled;
        public bool IsSFXEnabled => isSfxEnabled;

        private void LoadAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat(Constants.SettingsMusicKey, 0.7f);
            sfxVolume = PlayerPrefs.GetFloat(Constants.SettingsSfxKey, 1f);
            ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
            isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
            isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            isSfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

            ApplyVolumeSettings();
            AudioListener.volume = isMuted ? 0f : 1f;
        }

        private void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat(Constants.SettingsMusicKey, musicVolume);
            PlayerPrefs.SetFloat(Constants.SettingsSfxKey, sfxVolume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
            PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("SFXEnabled", isSfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<AudioManager>();
        }
    }
}
