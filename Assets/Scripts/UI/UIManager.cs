using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Central manager for all UI screens and transitions.
    /// Handles showing/hiding UI panels and screen flow.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Screens")]
        [SerializeField] private MainMenuUI mainMenu;
        [SerializeField] private GameHUD gameHUD;
        [SerializeField] private PauseMenuUI pauseMenu;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private SettingsUI settingsUI;
        [SerializeField] private CharacterSelectionUI characterSelection;
        [SerializeField] private ThemeSelectionUI themeSelection;
        [SerializeField] private LoadingScreenUI loadingScreen;

        [Header("Transitions")]
        [SerializeField] private CanvasGroup transitionOverlay;
        [SerializeField] private float transitionDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;

        // State
        private UIScreen currentScreen;
        private bool isTransitioning;

        public UIScreen CurrentScreen => currentScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGamePaused += OnGamePaused;
            GameEvents.OnGameResumed += OnGameResumed;
            GameEvents.OnGameOver += OnGameOver;
            GameEvents.OnShowPanel += ShowPanel;
            GameEvents.OnHidePanel += HidePanel;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGamePaused -= OnGamePaused;
            GameEvents.OnGameResumed -= OnGameResumed;
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnShowPanel -= ShowPanel;
            GameEvents.OnHidePanel -= HidePanel;
        }

        private void Start()
        {
            InitializeScreens();
            ShowMainMenu();
        }

        private void InitializeScreens()
        {
            // Hide all screens initially
            HideAllScreens();
        }

        #region Screen Management

        /// <summary>
        /// Shows the main menu screen.
        /// </summary>
        public void ShowMainMenu()
        {
            TransitionTo(UIScreen.MainMenu, () =>
            {
                HideAllScreens();
                if (mainMenu != null) mainMenu.Show();
                currentScreen = UIScreen.MainMenu;
            });
        }

        /// <summary>
        /// Shows the game HUD.
        /// </summary>
        public void ShowGameHUD()
        {
            HideAllScreens();
            if (gameHUD != null) gameHUD.Show();
            currentScreen = UIScreen.GameHUD;
        }

        /// <summary>
        /// Shows the pause menu.
        /// </summary>
        public void ShowPauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.Show();
                PlaySound(panelOpenSound);
            }
            currentScreen = UIScreen.PauseMenu;
        }

        /// <summary>
        /// Hides the pause menu.
        /// </summary>
        public void HidePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.Hide();
                PlaySound(panelCloseSound);
            }
            currentScreen = UIScreen.GameHUD;
        }

        /// <summary>
        /// Shows the game over screen.
        /// </summary>
        public void ShowGameOver(GameOverData data)
        {
            if (gameOverUI != null)
            {
                gameOverUI.Show(data);
            }
            currentScreen = UIScreen.GameOver;
        }

        /// <summary>
        /// Shows the settings screen.
        /// </summary>
        public void ShowSettings()
        {
            if (settingsUI != null)
            {
                settingsUI.Show();
                PlaySound(panelOpenSound);
            }
            currentScreen = UIScreen.Settings;
        }

        /// <summary>
        /// Hides the settings screen.
        /// </summary>
        public void HideSettings()
        {
            if (settingsUI != null)
            {
                settingsUI.Hide();
                PlaySound(panelCloseSound);
            }
        }

        /// <summary>
        /// Shows the character selection screen.
        /// </summary>
        public void ShowCharacterSelection()
        {
            TransitionTo(UIScreen.CharacterSelect, () =>
            {
                HideAllScreens();
                if (characterSelection != null) characterSelection.gameObject.SetActive(true);
                currentScreen = UIScreen.CharacterSelect;
            });
        }

        /// <summary>
        /// Shows the theme/mode selection screen.
        /// </summary>
        public void ShowThemeSelection()
        {
            TransitionTo(UIScreen.ThemeSelect, () =>
            {
                HideAllScreens();
                if (themeSelection != null) themeSelection.Show();
                currentScreen = UIScreen.ThemeSelect;
            });
        }

        /// <summary>
        /// Shows the loading screen.
        /// </summary>
        public void ShowLoading(string message = "Loading...")
        {
            if (loadingScreen != null)
            {
                loadingScreen.Show(message);
            }
        }

        /// <summary>
        /// Hides the loading screen.
        /// </summary>
        public void HideLoading()
        {
            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }
        }

        private void HideAllScreens()
        {
            if (mainMenu != null) mainMenu.Hide();
            if (gameHUD != null) gameHUD.Hide();
            if (pauseMenu != null) pauseMenu.Hide();
            if (gameOverUI != null) gameOverUI.Hide();
            if (settingsUI != null) settingsUI.Hide();
            if (characterSelection != null) characterSelection.gameObject.SetActive(false);
            if (themeSelection != null) themeSelection.Hide();
        }

        #endregion

        #region Event Handlers

        private void OnGameStarted()
        {
            ShowGameHUD();
        }

        private void OnGamePaused()
        {
            ShowPauseMenu();
        }

        private void OnGameResumed()
        {
            HidePauseMenu();
        }

        private void OnGameOver(GameOverData data)
        {
            ShowGameOver(data);
        }

        private void ShowPanel(string panelName)
        {
            switch (panelName.ToLower())
            {
                case "settings":
                    ShowSettings();
                    break;
                case "pause":
                    ShowPauseMenu();
                    break;
                case "characters":
                    ShowCharacterSelection();
                    break;
                case "themes":
                    ShowThemeSelection();
                    break;
            }
        }

        private void HidePanel(string panelName)
        {
            switch (panelName.ToLower())
            {
                case "settings":
                    HideSettings();
                    break;
                case "pause":
                    HidePauseMenu();
                    break;
            }
        }

        #endregion

        #region Transitions

        private void TransitionTo(UIScreen targetScreen, System.Action onComplete)
        {
            if (isTransitioning) return;

            if (transitionOverlay != null)
            {
                StartCoroutine(TransitionCoroutine(onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private System.Collections.IEnumerator TransitionCoroutine(System.Action onComplete)
        {
            isTransitioning = true;

            // Fade in overlay
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                transitionOverlay.alpha = elapsed / transitionDuration;
                yield return null;
            }
            transitionOverlay.alpha = 1f;

            // Execute callback
            onComplete?.Invoke();

            // Small delay
            yield return new WaitForSecondsRealtime(0.1f);

            // Fade out overlay
            elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                transitionOverlay.alpha = 1f - (elapsed / transitionDuration);
                yield return null;
            }
            transitionOverlay.alpha = 0f;

            isTransitioning = false;
        }

        #endregion

        #region Audio

        /// <summary>
        /// Plays the button click sound.
        /// </summary>
        public void PlayButtonClick()
        {
            PlaySound(buttonClickSound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(clip);
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<UIManager>();
        }
    }

    /// <summary>
    /// Enumeration of UI screens.
    /// </summary>
    public enum UIScreen
    {
        None,
        MainMenu,
        GameHUD,
        PauseMenu,
        GameOver,
        Settings,
        CharacterSelect,
        ThemeSelect,
        Loading
    }
}
