using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;
using EscapeTrainRun.Characters;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Main menu screen with play, character selection, settings, etc.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button charactersButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button dailyRewardButton;
        [SerializeField] private Button shopButton;

        [Header("Display")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text coinCountText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Image currentCharacterImage;
        [SerializeField] private Text currentCharacterName;

        [Header("Animation")]
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private float showAnimationDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip menuMusic;

        // References
        private SaveManager saveManager;
        private CharacterManager characterManager;

        private void Awake()
        {
            SetupButtons();
        }

        private void Start()
        {
            saveManager = SaveManager.Instance;
            characterManager = CharacterManager.Instance;
        }

        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (charactersButton != null)
            {
                charactersButton.onClick.AddListener(OnCharactersClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (dailyRewardButton != null)
            {
                dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
            }

            if (shopButton != null)
            {
                shopButton.onClick.AddListener(OnShopClicked);
            }
        }

        /// <summary>
        /// Shows the main menu.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
            }

            RefreshDisplay();
            PlayShowAnimation();
            PlayMenuMusic();
        }

        /// <summary>
        /// Hides the main menu.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);

            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }
        }

        private void RefreshDisplay()
        {
            // Coins
            if (coinCountText != null && saveManager != null)
            {
                coinCountText.text = saveManager.TotalCoins.ToString("N0");
            }

            // High score
            if (highScoreText != null && saveManager != null)
            {
                highScoreText.text = $"Best: {saveManager.GetHighScore():N0}";
            }

            // Current character
            if (characterManager != null && characterManager.CurrentCharacter != null)
            {
                var character = characterManager.CurrentCharacter;

                if (currentCharacterImage != null && character.Portrait != null)
                {
                    currentCharacterImage.sprite = character.Portrait;
                }

                if (currentCharacterName != null)
                {
                    currentCharacterName.text = character.DisplayName;
                }
            }

            // Daily reward button visibility
            if (dailyRewardButton != null && saveManager != null)
            {
                dailyRewardButton.interactable = saveManager.CanClaimDailyReward();
            }
        }

        private void PlayShowAnimation()
        {
            if (menuAnimator != null)
            {
                menuAnimator.SetTrigger("Show");
            }
            else if (canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        private System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < showAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / showAnimationDuration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private void PlayMenuMusic()
        {
            if (menuMusic != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlayMusic(menuMusic);
            }
        }

        #region Button Handlers

        private void OnPlayClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            // Go to theme selection or start game directly
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowThemeSelection();
            }
            else
            {
                // Direct start
                if (ServiceLocator.TryGet<GameManager>(out var gameManager))
                {
                    gameManager.StartGame();
                }
            }
        }

        private void OnCharactersClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            UIManager.Instance?.ShowCharacterSelection();
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            UIManager.Instance?.ShowSettings();
        }

        private void OnQuitClicked()
        {
            UIManager.Instance?.PlayButtonClick();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDailyRewardClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            if (saveManager != null && saveManager.CanClaimDailyReward())
            {
                int reward = saveManager.ClaimDailyReward();
                Debug.Log($"[MainMenuUI] Claimed daily reward: {reward} coins");

                // Show reward popup
                RefreshDisplay();
            }
        }

        private void OnShopClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            // Show shop - not implemented in this phase
            Debug.Log("[MainMenuUI] Shop clicked - not implemented yet");
        }

        #endregion
    }
}
