using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// In-game heads-up display showing score, coins, distance, and power-ups.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text scoreMultiplierText;
        [SerializeField] private Animator scoreAnimator;

        [Header("Coins Display")]
        [SerializeField] private Text coinText;
        [SerializeField] private Image coinIcon;
        [SerializeField] private Animator coinAnimator;

        [Header("Distance Display")]
        [SerializeField] private Text distanceText;
        [SerializeField] private string distanceFormat = "{0:F0}m";

        [Header("Power-Up Display")]
        [SerializeField] private Transform powerUpContainer;
        [SerializeField] private PowerUpDisplay powerUpDisplay;

        [Header("Pause Button")]
        [SerializeField] private Button pauseButton;

        [Header("Combo Display")]
        [SerializeField] private Text comboText;
        [SerializeField] private Animator comboAnimator;
        [SerializeField] private float comboDisplayDuration = 1.5f;

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDuration = 0.3f;

        // State
        private int displayedScore;
        private int targetScore;
        private float scoreAnimationSpeed = 5f;
        private int currentCoins;
        private float comboTimer;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnScoreChanged += OnScoreChanged;
            GameEvents.OnCoinsCollected += OnCoinsCollected;
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnScoreChanged -= OnScoreChanged;
            GameEvents.OnCoinsCollected -= OnCoinsCollected;
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
        }

        private void Update()
        {
            UpdateScoreAnimation();
            UpdateDistance();
            UpdateComboTimer();
        }

        /// <summary>
        /// Shows the HUD.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ResetDisplay();

            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        /// <summary>
        /// Hides the HUD.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ResetDisplay()
        {
            displayedScore = 0;
            targetScore = 0;
            currentCoins = 0;

            UpdateScoreDisplay(0);
            UpdateCoinDisplay(0);
            UpdateDistanceDisplay(0);
            HideCombo();
            HideMultiplier();
        }

        private System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        #region Score Display

        private void OnScoreChanged(int newScore)
        {
            targetScore = newScore;

            // Trigger animation for big score gains
            if (newScore - displayedScore > 100 && scoreAnimator != null)
            {
                scoreAnimator.SetTrigger("Pulse");
            }
        }

        private void UpdateScoreAnimation()
        {
            if (displayedScore != targetScore)
            {
                displayedScore = (int)Mathf.MoveTowards(displayedScore, targetScore, 
                    scoreAnimationSpeed * Time.deltaTime * Mathf.Max(100, targetScore - displayedScore));
                UpdateScoreDisplay(displayedScore);
            }
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString("N0");
            }
        }

        /// <summary>
        /// Shows the score multiplier indicator.
        /// </summary>
        public void ShowMultiplier(int multiplier)
        {
            if (scoreMultiplierText != null)
            {
                scoreMultiplierText.gameObject.SetActive(true);
                scoreMultiplierText.text = $"x{multiplier}";
            }
        }

        private void HideMultiplier()
        {
            if (scoreMultiplierText != null)
            {
                scoreMultiplierText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Coins Display

        private void OnCoinsCollected(int amount)
        {
            currentCoins += amount;
            UpdateCoinDisplay(currentCoins);

            // Trigger animation
            if (coinAnimator != null)
            {
                coinAnimator.SetTrigger("Collect");
            }

            // Show combo
            ShowCombo(amount);
        }

        private void UpdateCoinDisplay(int coins)
        {
            if (coinText != null)
            {
                coinText.text = coins.ToString();
            }
        }

        #endregion

        #region Distance Display

        private void UpdateDistance()
        {
            if (distanceText == null) return;

            if (ServiceLocator.TryGet<ScoreManager>(out var scoreManager))
            {
                UpdateDistanceDisplay(scoreManager.DistanceTraveled);
            }
        }

        private void UpdateDistanceDisplay(float distance)
        {
            if (distanceText != null)
            {
                if (distance < 1000)
                {
                    distanceText.text = string.Format(distanceFormat, distance);
                }
                else
                {
                    distanceText.text = $"{distance / 1000f:F2}km";
                }
            }
        }

        #endregion

        #region Combo Display

        private void ShowCombo(int amount)
        {
            if (comboText == null) return;

            comboText.gameObject.SetActive(true);
            comboText.text = $"+{amount}";
            comboTimer = comboDisplayDuration;

            if (comboAnimator != null)
            {
                comboAnimator.SetTrigger("Show");
            }
        }

        private void HideCombo()
        {
            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }
        }

        private void UpdateComboTimer()
        {
            if (comboTimer > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0)
                {
                    HideCombo();
                }
            }
        }

        #endregion

        #region Power-Up Display

        private void OnPowerUpActivated(PowerUpType type)
        {
            if (type == PowerUpType.Multiplier)
            {
                ShowMultiplier(Utils.Constants.MultiplierValue);
            }

            // Power-up display is handled by PowerUpDisplay component
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            if (type == PowerUpType.Multiplier)
            {
                HideMultiplier();
            }
        }

        #endregion

        #region Button Handlers

        private void OnPauseClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            if (ServiceLocator.TryGet<GameManager>(out var gameManager))
            {
                gameManager.PauseGame();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows a floating text popup (for special events).
        /// </summary>
        public void ShowFloatingText(string text, Vector3 worldPosition)
        {
            // Could spawn floating text prefab
            Debug.Log($"[GameHUD] Floating text: {text}");
        }

        /// <summary>
        /// Flashes the screen (for damage, power-ups, etc.)
        /// </summary>
        public void FlashScreen(Color color, float duration = 0.2f)
        {
            StartCoroutine(FlashCoroutine(color, duration));
        }

        private System.Collections.IEnumerator FlashCoroutine(Color color, float duration)
        {
            // Could have a flash overlay image
            yield return new WaitForSeconds(duration);
        }

        #endregion
    }
}
