using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Game over screen showing final score, stats, and options.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Title")]
        [SerializeField] private Text titleText;
        [SerializeField] private GameObject newHighScoreBanner;
        [SerializeField] private ParticleSystem celebrationParticles;

        [Header("Score Display")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text scoreLabel;

        [Header("Stats Display")]
        [SerializeField] private Text distanceText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text gameTimeText;

        [Header("Rewards")]
        [SerializeField] private GameObject rewardsPanel;
        [SerializeField] private Text rewardCoinsText;
        [SerializeField] private Text bonusText;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private Button watchAdButton;

        [Header("Animation")]
        [SerializeField] private float showDelay = 0.5f;
        [SerializeField] private float scoreCountDuration = 1.5f;
        [SerializeField] private float statRevealDelay = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip highScoreSound;
        [SerializeField] private AudioClip scoreCountSound;

        // State
        private GameOverData currentData;
        private bool isAnimating;

        private void Awake()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (shareButton != null)
            {
                shareButton.onClick.AddListener(OnShareClicked);
            }

            if (watchAdButton != null)
            {
                watchAdButton.onClick.AddListener(OnWatchAdClicked);
            }
        }

        /// <summary>
        /// Shows the game over screen with the final data.
        /// </summary>
        public void Show(GameOverData data)
        {
            currentData = data;
            gameObject.SetActive(true);

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // Hide elements initially
            HideAllElements();

            // Start animation sequence
            StartCoroutine(ShowSequence(data));

            // Play sound
            PlayGameOverSound(data.IsHighScore);
        }

        /// <summary>
        /// Hides the game over screen.
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines();
            isAnimating = false;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void HideAllElements()
        {
            if (newHighScoreBanner != null) newHighScoreBanner.SetActive(false);
            if (rewardsPanel != null) rewardsPanel.SetActive(false);
            if (scoreText != null) scoreText.text = "0";
            if (distanceText != null) distanceText.text = "0m";
            if (coinsText != null) coinsText.text = "0";
            if (gameTimeText != null) gameTimeText.text = "0:00";

            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private System.Collections.IEnumerator ShowSequence(GameOverData data)
        {
            isAnimating = true;

            // Wait before showing
            yield return new WaitForSecondsRealtime(showDelay);

            // Fade in panel
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < 0.3f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = elapsed / 0.3f;
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = data.IsHighScore ? "NEW HIGH SCORE!" : "GAME OVER";
            }

            // Show high score banner
            if (data.IsHighScore && newHighScoreBanner != null)
            {
                newHighScoreBanner.SetActive(true);
                if (celebrationParticles != null)
                {
                    celebrationParticles.Play();
                }
            }

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Animate score counting
            yield return StartCoroutine(AnimateScoreCount(data.FinalScore));

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Show high score
            if (highScoreText != null)
            {
                int highScore = data.IsHighScore ? data.FinalScore : GetHighScore();
                highScoreText.text = $"Best: {highScore:N0}";
            }

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Show stats
            yield return StartCoroutine(RevealStats(data));

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Show rewards
            if (rewardsPanel != null)
            {
                rewardsPanel.SetActive(true);
                if (rewardCoinsText != null)
                {
                    rewardCoinsText.text = $"+{data.CoinsCollected}";
                }
            }

            isAnimating = false;
        }

        private System.Collections.IEnumerator AnimateScoreCount(int finalScore)
        {
            if (scoreText == null) yield break;

            int displayScore = 0;
            float elapsed = 0f;

            // Play counting sound
            // Could loop score count sound here

            while (elapsed < scoreCountDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scoreCountDuration;
                t = EaseOutCubic(t);

                displayScore = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, t));
                scoreText.text = displayScore.ToString("N0");

                yield return null;
            }

            scoreText.text = finalScore.ToString("N0");
        }

        private System.Collections.IEnumerator RevealStats(GameOverData data)
        {
            // Distance
            if (distanceText != null)
            {
                if (data.DistanceTraveled < 1000)
                {
                    distanceText.text = $"{data.DistanceTraveled:F0}m";
                }
                else
                {
                    distanceText.text = $"{data.DistanceTraveled / 1000f:F2}km";
                }
            }

            yield return new WaitForSecondsRealtime(statRevealDelay * 0.5f);

            // Coins
            if (coinsText != null)
            {
                coinsText.text = data.CoinsCollected.ToString();
            }

            yield return new WaitForSecondsRealtime(statRevealDelay * 0.5f);

            // Time
            if (gameTimeText != null)
            {
                int minutes = Mathf.FloorToInt(data.GameDuration / 60f);
                int seconds = Mathf.FloorToInt(data.GameDuration % 60f);
                gameTimeText.text = $"{minutes}:{seconds:D2}";
            }
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private int GetHighScore()
        {
            if (ServiceLocator.TryGet<SaveManager>(out var saveManager))
            {
                return saveManager.GetHighScore();
            }
            return 0;
        }

        private void PlayGameOverSound(bool isHighScore)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                if (isHighScore && highScoreSound != null)
                {
                    audio.PlaySFX(highScoreSound);
                }
                else if (gameOverSound != null)
                {
                    audio.PlaySFX(gameOverSound);
                }
            }
        }

        #region Button Handlers

        private void OnPlayAgainClicked()
        {
            if (isAnimating) return;

            UIManager.Instance?.PlayButtonClick();

            if (ServiceLocator.TryGet<GameManager>(out var gameManager))
            {
                gameManager.RestartGame();
            }
        }

        private void OnMainMenuClicked()
        {
            if (isAnimating) return;

            UIManager.Instance?.PlayButtonClick();

            if (ServiceLocator.TryGet<GameManager>(out var gameManager))
            {
                gameManager.ReturnToMenu();
            }
        }

        private void OnShareClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            // Share functionality
            string shareText = $"I scored {currentData.FinalScore:N0} points in Escape Train Run! Can you beat my score?";
            Debug.Log($"[GameOverUI] Share: {shareText}");

            // Could use native share on mobile
#if UNITY_ANDROID || UNITY_IOS
            // Native share implementation
#endif
        }

        private void OnWatchAdClicked()
        {
            UIManager.Instance?.PlayButtonClick();

            // Watch ad for continue or bonus
            Debug.Log("[GameOverUI] Watch ad - not implemented");
        }

        #endregion
    }
}
