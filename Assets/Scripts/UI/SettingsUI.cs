using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Settings screen for audio, graphics, and control options.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private Text masterVolumeText;
        [SerializeField] private Text musicVolumeText;
        [SerializeField] private Text sfxVolumeText;

        [Header("Graphics Settings")]
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Toggle particlesToggle;
        [SerializeField] private Toggle screenShakeToggle;

        [Header("Control Settings")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Toggle invertControlsToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Text sensitivityText;

        [Header("Account Settings")]
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsButton;
        [SerializeField] private Text versionText;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetDefaultsButton;

        [Header("Tabs")]
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button graphicsTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject graphicsPanel;
        [SerializeField] private GameObject controlsPanel;

        // Settings keys
        private const string KEY_MASTER_VOLUME = "MasterVolume";
        private const string KEY_MUSIC_VOLUME = "MusicVolume";
        private const string KEY_SFX_VOLUME = "SFXVolume";
        private const string KEY_MUTED = "Muted";
        private const string KEY_QUALITY = "Quality";
        private const string KEY_VSYNC = "VSync";
        private const string KEY_PARTICLES = "Particles";
        private const string KEY_SCREEN_SHAKE = "ScreenShake";
        private const string KEY_SENSITIVITY = "Sensitivity";
        private const string KEY_INVERT_CONTROLS = "InvertControls";
        private const string KEY_VIBRATION = "Vibration";

        private void Awake()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // Close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (resetDefaultsButton != null)
            {
                resetDefaultsButton.onClick.AddListener(OnResetDefaultsClicked);
            }

            // Audio sliders
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteChanged);
            }

            // Graphics
            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
                PopulateQualityDropdown();
            }

            if (vsyncToggle != null)
            {
                vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }

            if (particlesToggle != null)
            {
                particlesToggle.onValueChanged.AddListener(OnParticlesChanged);
            }

            if (screenShakeToggle != null)
            {
                screenShakeToggle.onValueChanged.AddListener(OnScreenShakeChanged);
            }

            // Controls
            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }

            if (invertControlsToggle != null)
            {
                invertControlsToggle.onValueChanged.AddListener(OnInvertControlsChanged);
            }

            if (vibrationToggle != null)
            {
                vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
            }

            // Account
            if (resetProgressButton != null)
            {
                resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            }

            // Tabs
            if (audioTabButton != null)
            {
                audioTabButton.onClick.AddListener(() => ShowTab(0));
            }

            if (graphicsTabButton != null)
            {
                graphicsTabButton.onClick.AddListener(() => ShowTab(1));
            }

            if (controlsTabButton != null)
            {
                controlsTabButton.onClick.AddListener(() => ShowTab(2));
            }

            // Version text
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }
        }

        /// <summary>
        /// Shows the settings panel.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }

            LoadSettings();
            ShowTab(0); // Start on audio tab
        }

        /// <summary>
        /// Hides the settings panel.
        /// </summary>
        public void Hide()
        {
            SaveSettings();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void ShowTab(int tabIndex)
        {
            UIManager.Instance?.PlayButtonClick();

            if (audioPanel != null) audioPanel.SetActive(tabIndex == 0);
            if (graphicsPanel != null) graphicsPanel.SetActive(tabIndex == 1);
            if (controlsPanel != null) controlsPanel.SetActive(tabIndex == 2);
        }

        #region Settings Load/Save

        private void LoadSettings()
        {
            // Audio
            float masterVol = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, 1f);
            float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 0.8f);
            float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
            bool muted = PlayerPrefs.GetInt(KEY_MUTED, 0) == 1;

            if (masterVolumeSlider != null) masterVolumeSlider.value = masterVol;
            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
            if (muteToggle != null) muteToggle.isOn = muted;

            // Graphics
            int quality = PlayerPrefs.GetInt(KEY_QUALITY, QualitySettings.GetQualityLevel());
            bool vsync = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
            bool particles = PlayerPrefs.GetInt(KEY_PARTICLES, 1) == 1;
            bool screenShake = PlayerPrefs.GetInt(KEY_SCREEN_SHAKE, 1) == 1;

            if (qualityDropdown != null) qualityDropdown.value = quality;
            if (vsyncToggle != null) vsyncToggle.isOn = vsync;
            if (particlesToggle != null) particlesToggle.isOn = particles;
            if (screenShakeToggle != null) screenShakeToggle.isOn = screenShake;

            // Controls
            float sensitivity = PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1f);
            bool invert = PlayerPrefs.GetInt(KEY_INVERT_CONTROLS, 0) == 1;
            bool vibration = PlayerPrefs.GetInt(KEY_VIBRATION, 1) == 1;

            if (sensitivitySlider != null) sensitivitySlider.value = sensitivity;
            if (invertControlsToggle != null) invertControlsToggle.isOn = invert;
            if (vibrationToggle != null) vibrationToggle.isOn = vibration;

            // Update text displays
            UpdateVolumeTexts();
            UpdateSensitivityText();
        }

        private void SaveSettings()
        {
            // Audio
            if (masterVolumeSlider != null) PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, masterVolumeSlider.value);
            if (musicVolumeSlider != null) PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, musicVolumeSlider.value);
            if (sfxVolumeSlider != null) PlayerPrefs.SetFloat(KEY_SFX_VOLUME, sfxVolumeSlider.value);
            if (muteToggle != null) PlayerPrefs.SetInt(KEY_MUTED, muteToggle.isOn ? 1 : 0);

            // Graphics
            if (qualityDropdown != null) PlayerPrefs.SetInt(KEY_QUALITY, qualityDropdown.value);
            if (vsyncToggle != null) PlayerPrefs.SetInt(KEY_VSYNC, vsyncToggle.isOn ? 1 : 0);
            if (particlesToggle != null) PlayerPrefs.SetInt(KEY_PARTICLES, particlesToggle.isOn ? 1 : 0);
            if (screenShakeToggle != null) PlayerPrefs.SetInt(KEY_SCREEN_SHAKE, screenShakeToggle.isOn ? 1 : 0);

            // Controls
            if (sensitivitySlider != null) PlayerPrefs.SetFloat(KEY_SENSITIVITY, sensitivitySlider.value);
            if (invertControlsToggle != null) PlayerPrefs.SetInt(KEY_INVERT_CONTROLS, invertControlsToggle.isOn ? 1 : 0);
            if (vibrationToggle != null) PlayerPrefs.SetInt(KEY_VIBRATION, vibrationToggle.isOn ? 1 : 0);

            PlayerPrefs.Save();
        }

        #endregion

        #region Audio Handlers

        private void OnMasterVolumeChanged(float value)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.SetMasterVolume(value);
            }
            UpdateVolumeTexts();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.SetMusicVolume(value);
            }
            UpdateVolumeTexts();
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.SetSFXVolume(value);
            }
            UpdateVolumeTexts();
        }

        private void OnMuteChanged(bool muted)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.SetMuted(muted);
            }
        }

        private void UpdateVolumeTexts()
        {
            if (masterVolumeText != null && masterVolumeSlider != null)
            {
                masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
            }

            if (musicVolumeText != null && musicVolumeSlider != null)
            {
                musicVolumeText.text = $"{Mathf.RoundToInt(musicVolumeSlider.value * 100)}%";
            }

            if (sfxVolumeText != null && sfxVolumeSlider != null)
            {
                sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value * 100)}%";
            }
        }

        #endregion

        #region Graphics Handlers

        private void PopulateQualityDropdown()
        {
            if (qualityDropdown == null) return;

            qualityDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>(QualitySettings.names);
            qualityDropdown.AddOptions(options);
        }

        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index);
        }

        private void OnVSyncChanged(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
        }

        private void OnParticlesChanged(bool enabled)
        {
            // Could disable particle systems globally
            Debug.Log($"[SettingsUI] Particles: {enabled}");
        }

        private void OnScreenShakeChanged(bool enabled)
        {
            // Could disable camera shake effects
            Debug.Log($"[SettingsUI] Screen shake: {enabled}");
        }

        #endregion

        #region Control Handlers

        private void OnSensitivityChanged(float value)
        {
            // Apply to swipe detector
            UpdateSensitivityText();
        }

        private void OnInvertControlsChanged(bool inverted)
        {
            // Apply to controls
            Debug.Log($"[SettingsUI] Invert controls: {inverted}");
        }

        private void OnVibrationChanged(bool enabled)
        {
            // Enable/disable haptic feedback
            Debug.Log($"[SettingsUI] Vibration: {enabled}");
        }

        private void UpdateSensitivityText()
        {
            if (sensitivityText != null && sensitivitySlider != null)
            {
                sensitivityText.text = $"{sensitivitySlider.value:F1}x";
            }
        }

        #endregion

        #region Button Handlers

        private void OnCloseClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            UIManager.Instance?.HideSettings();
        }

        private void OnResetDefaultsClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            // Reset to defaults
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (musicVolumeSlider != null) musicVolumeSlider.value = 0.8f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (muteToggle != null) muteToggle.isOn = false;
            if (qualityDropdown != null) qualityDropdown.value = QualitySettings.names.Length / 2;
            if (vsyncToggle != null) vsyncToggle.isOn = true;
            if (particlesToggle != null) particlesToggle.isOn = true;
            if (screenShakeToggle != null) screenShakeToggle.isOn = true;
            if (sensitivitySlider != null) sensitivitySlider.value = 1f;
            if (invertControlsToggle != null) invertControlsToggle.isOn = false;
            if (vibrationToggle != null) vibrationToggle.isOn = true;

            SaveSettings();
        }

        private void OnResetProgressClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            // Show confirmation dialog before resetting
            Debug.Log("[SettingsUI] Reset progress clicked - needs confirmation");
        }

        #endregion
    }
}
