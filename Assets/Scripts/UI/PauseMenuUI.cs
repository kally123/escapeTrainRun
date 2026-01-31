using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Pause menu with resume, restart, settings, and quit options.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundOverlay;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Display")]
        [SerializeField] private Text pauseTitleText;
        [SerializeField] private Text currentScoreText;
        [SerializeField] private Text currentDistanceText;
        [SerializeField] private Text currentCoinsText;

        [Header("Settings")]
        [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private float animationDuration = 0.2f;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private Text confirmationText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        // State
        private System.Action pendingConfirmAction;

        private void Awake()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            }

            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(OnConfirmNo);
            }
        }

        /// <summary>
        /// Shows the pause menu.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }

            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }

            RefreshDisplay();
            PlayShowAnimation();
        }

        /// <summary>
        /// Hides the pause menu.
        /// </summary>
        public void Hide()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void RefreshDisplay()
        {
            if (ServiceLocator.TryGet<ScoreManager>(out var scoreManager))
            {
                if (currentScoreText != null)
                {
                    currentScoreText.text = $"Score: {scoreManager.CurrentScore:N0}";
                }

                if (currentDistanceText != null)
                {
                    currentDistanceText.text = $"Distance: {scoreManager.GetFormattedDistance()}";
                }

                if (currentCoinsText != null)
                {
                    currentCoinsText.text = $"Coins: {scoreManager.SessionCoins}";
                }
            }
        }

        private void PlayShowAnimation()
        {
            if (canvasGroup != null)
            {
                StartCoroutine(AnimateShow());
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.color = overlayColor;
            }
        }

        private System.Collections.IEnumerator AnimateShow()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / animationDuration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        #region Button Handlers

        private void OnResumeClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            if (ServiceLocator.TryGet<GameManager>(out var gameManager))
            {
                gameManager.ResumeGame();
            }
        }

        private void OnRestartClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            ShowConfirmation("Restart game?", () =>
            {
                if (ServiceLocator.TryGet<GameManager>(out var gameManager))
                {
                    gameManager.RestartGame();
                }
            });
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            UIManager.Instance?.ShowSettings();
        }

        private void OnMainMenuClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            ShowConfirmation("Return to main menu?\nCurrent progress will be lost.", () =>
            {
                if (ServiceLocator.TryGet<GameManager>(out var gameManager))
                {
                    gameManager.ReturnToMenu();
                }
            });
        }

        #endregion

        #region Confirmation Dialog

        private void ShowConfirmation(string message, System.Action onConfirm)
        {
            if (confirmationDialog == null)
            {
                // No dialog, just confirm
                onConfirm?.Invoke();
                return;
            }

            pendingConfirmAction = onConfirm;

            if (confirmationText != null)
            {
                confirmationText.text = message;
            }

            confirmationDialog.SetActive(true);
        }

        private void HideConfirmation()
        {
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }
            pendingConfirmAction = null;
        }

        private void OnConfirmYes()
        {
            UIManager.Instance?.PlayButtonClick();
            var action = pendingConfirmAction;
            HideConfirmation();
            action?.Invoke();
        }

        private void OnConfirmNo()
        {
            UIManager.Instance?.PlayButtonClick();
            HideConfirmation();
        }

        #endregion
    }
}
