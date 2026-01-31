using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Theme/mode selection screen before starting the game.
    /// Allows player to choose Train, Bus, or Ground mode.
    /// </summary>
    public class ThemeSelectionUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Theme Cards")]
        [SerializeField] private ThemeCard trainCard;
        [SerializeField] private ThemeCard busCard;
        [SerializeField] private ThemeCard groundCard;

        [Header("Selected Theme Display")]
        [SerializeField] private Image selectedThemeImage;
        [SerializeField] private Text selectedThemeName;
        [SerializeField] private Text selectedThemeDescription;
        [SerializeField] private Text highScoreText;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button randomButton;

        [Header("Theme Data")]
        [SerializeField] private ThemeData trainThemeData;
        [SerializeField] private ThemeData busThemeData;
        [SerializeField] private ThemeData groundThemeData;

        // State
        private ThemeType selectedTheme = ThemeType.Train;

        private void Awake()
        {
            SetupButtons();
            SetupCards();
        }

        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (randomButton != null)
            {
                randomButton.onClick.AddListener(OnRandomClicked);
            }
        }

        private void SetupCards()
        {
            if (trainCard != null)
            {
                trainCard.Initialize(ThemeType.Train, trainThemeData, OnThemeSelected);
            }

            if (busCard != null)
            {
                busCard.Initialize(ThemeType.Bus, busThemeData, OnThemeSelected);
            }

            if (groundCard != null)
            {
                groundCard.Initialize(ThemeType.Ground, groundThemeData, OnThemeSelected);
            }
        }

        /// <summary>
        /// Shows the theme selection screen.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }

            // Select last played theme or default
            string lastTheme = PlayerPrefs.GetString("LastTheme", "Train");
            if (System.Enum.TryParse(lastTheme, out ThemeType theme))
            {
                SelectTheme(theme);
            }
            else
            {
                SelectTheme(ThemeType.Train);
            }

            PlayShowAnimation();
        }

        /// <summary>
        /// Hides the theme selection screen.
        /// </summary>
        public void Hide()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void PlayShowAnimation()
        {
            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        private System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / duration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        #region Theme Selection

        private void OnThemeSelected(ThemeType theme)
        {
            UIManager.Instance?.PlayButtonClick();
            SelectTheme(theme);
        }

        private void SelectTheme(ThemeType theme)
        {
            selectedTheme = theme;

            // Update card selection states
            if (trainCard != null) trainCard.SetSelected(theme == ThemeType.Train);
            if (busCard != null) busCard.SetSelected(theme == ThemeType.Bus);
            if (groundCard != null) groundCard.SetSelected(theme == ThemeType.Ground);

            // Update display
            UpdateThemeDisplay(theme);
        }

        private void UpdateThemeDisplay(ThemeType theme)
        {
            ThemeData data = GetThemeData(theme);
            if (data == null) return;

            if (selectedThemeImage != null && data.PreviewImage != null)
            {
                selectedThemeImage.sprite = data.PreviewImage;
            }

            if (selectedThemeName != null)
            {
                selectedThemeName.text = data.DisplayName;
            }

            if (selectedThemeDescription != null)
            {
                selectedThemeDescription.text = data.Description;
            }

            // High score for this theme
            if (highScoreText != null && ServiceLocator.TryGet<SaveManager>(out var saveManager))
            {
                int highScore = saveManager.GetHighScore(theme);
                highScoreText.text = $"Best: {highScore:N0}";
            }
        }

        private ThemeData GetThemeData(ThemeType theme)
        {
            return theme switch
            {
                ThemeType.Train => trainThemeData,
                ThemeType.Bus => busThemeData,
                ThemeType.Ground => groundThemeData,
                _ => trainThemeData
            };
        }

        #endregion

        #region Button Handlers

        private void OnPlayClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            // Save selected theme
            PlayerPrefs.SetString("LastTheme", selectedTheme.ToString());
            PlayerPrefs.Save();

            // Fire theme selected event
            GameEvents.TriggerThemeSelected(selectedTheme);

            // Start the game
            if (ServiceLocator.TryGet<GameManager>(out var gameManager))
            {
                gameManager.StartGame(selectedTheme);
            }
        }

        private void OnBackClicked()
        {
            UIManager.Instance?.PlayButtonClick();
            UIManager.Instance?.ShowMainMenu();
        }

        private void OnRandomClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            // Select random theme
            ThemeType[] themes = { ThemeType.Train, ThemeType.Bus, ThemeType.Ground };
            ThemeType randomTheme = themes[Random.Range(0, themes.Length)];
            SelectTheme(randomTheme);
        }

        #endregion
    }

    /// <summary>
    /// Data container for theme display information.
    /// </summary>
    [System.Serializable]
    public class ThemeData
    {
        public string DisplayName;
        public string Description;
        public Sprite PreviewImage;
        public Sprite IconImage;
        public Color ThemeColor;
    }
}
