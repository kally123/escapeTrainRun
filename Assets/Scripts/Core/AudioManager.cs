using UnityEngine;
using System.Collections.Generic;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Centralized audio management system.
    /// Handles music, sound effects, and audio settings.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip trainMusic;
        [SerializeField] private AudioClip busMusic;
        [SerializeField] private AudioClip groundMusic;
        [SerializeField] private AudioClip gameOverJingle;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip slideSound;
        [SerializeField] private AudioClip laneChangeSound;
        [SerializeField] private AudioClip coinCollectSound;
        [SerializeField] private AudioClip powerUpSound;
        [SerializeField] private AudioClip crashSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip achievementSound;

        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float fadeDuration = 1f;

        // Audio state
        private bool isMusicEnabled = true;
        private bool isSfxEnabled = true;
        private Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            InitializeSingleton();
            LoadAudioSettings();
            RegisterServices();
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
            GameEvents.OnCoinsCollected += (_) => PlayCoinSound();
            GameEvents.OnPowerUpActivated += (_) => PlayPowerUpSound();
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
            GameEvents.OnCoinsCollected -= (_) => PlayCoinSound();
            GameEvents.OnPowerUpActivated -= (_) => PlayPowerUpSound();
            GameEvents.OnPlayerCrashed -= PlayCrashSound;
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnThemeChanged -= OnThemeChanged;
        }

        #region Music Control

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!isMusicEnabled || clip == null) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
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
            PlayMusic(menuMusic);
        }

        private void OnGameStarted()
        {
            // Music will be set by theme change event
        }

        private void OnGameOver(GameOverData data)
        {
            PlayMusic(gameOverJingle, false);
        }

        private void OnThemeChanged(ThemeType theme)
        {
            AudioClip themeMusic = theme switch
            {
                ThemeType.Train => trainMusic,
                ThemeType.Bus => busMusic,
                ThemeType.Ground => groundMusic,
                _ => trainMusic
            };
            PlayMusic(themeMusic);
        }

        #endregion

        #region Sound Effects

        public void PlaySFX(AudioClip clip)
        {
            if (!isSfxEnabled || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlayJumpSound()
        {
            PlaySFX(jumpSound);
        }

        public void PlaySlideSound()
        {
            PlaySFX(slideSound);
        }

        public void PlayLaneChangeSound()
        {
            PlaySFX(laneChangeSound);
        }

        public void PlayCoinSound()
        {
            PlaySFX(coinCollectSound);
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
            PlaySFX(buttonClickSound);
        }

        public void PlayAchievementSound()
        {
            PlaySFX(achievementSound);
        }

        #endregion

        #region Settings

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
            SaveAudioSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
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

        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;
        public bool IsMusicEnabled => isMusicEnabled;
        public bool IsSFXEnabled => isSfxEnabled;

        private void LoadAudioSettings()
        {
            musicVolume = PlayerPrefs.GetFloat(Constants.SettingsMusicKey, 0.7f);
            sfxVolume = PlayerPrefs.GetFloat(Constants.SettingsSfxKey, 1f);
            isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            isSfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

            musicSource.volume = musicVolume;
        }

        private void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat(Constants.SettingsMusicKey, musicVolume);
            PlayerPrefs.SetFloat(Constants.SettingsSfxKey, sfxVolume);
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
        }
    }
}
