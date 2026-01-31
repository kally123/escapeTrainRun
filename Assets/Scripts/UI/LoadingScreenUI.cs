using UnityEngine;
using UnityEngine.UI;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Loading screen with progress bar and tips.
    /// </summary>
    public class LoadingScreenUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Display")]
        [SerializeField] private Text loadingText;
        [SerializeField] private Text tipText;
        [SerializeField] private Image progressBar;
        [SerializeField] private Image loadingIcon;

        [Header("Animation")]
        [SerializeField] private float iconRotationSpeed = 180f;
        [SerializeField] private float tipChangeInterval = 3f;

        [Header("Tips")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "Swipe left or right to change lanes!",
            "Swipe up to jump over obstacles!",
            "Swipe down to slide under barriers!",
            "Collect coins to unlock new characters!",
            "Power-ups can save you in tough spots!",
            "The shield protects you from one hit!",
            "Star power lets you fly above everything!",
            "Try different characters for unique abilities!",
            "Each theme has different obstacles!",
            "Practice makes perfect!"
        };

        // State
        private float progress;
        private int currentTipIndex;
        private float tipTimer;

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            AnimateIcon();
            UpdateTipTimer();
        }

        /// <summary>
        /// Shows the loading screen.
        /// </summary>
        public void Show(string message = "Loading...")
        {
            gameObject.SetActive(true);

            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }

            if (loadingText != null)
            {
                loadingText.text = message;
            }

            SetProgress(0f);
            ShowRandomTip();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Hides the loading screen.
        /// </summary>
        public void Hide()
        {
            StartCoroutine(FadeOut());
        }

        private System.Collections.IEnumerator FadeOut()
        {
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                float duration = 0.3f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = 1f - (elapsed / duration);
                    yield return null;
                }
            }

            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the loading progress (0-1).
        /// </summary>
        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);

            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }
        }

        /// <summary>
        /// Sets the loading message.
        /// </summary>
        public void SetMessage(string message)
        {
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }

        private void AnimateIcon()
        {
            if (loadingIcon != null)
            {
                loadingIcon.transform.Rotate(Vector3.forward, -iconRotationSpeed * Time.unscaledDeltaTime);
            }
        }

        private void UpdateTipTimer()
        {
            tipTimer -= Time.unscaledDeltaTime;

            if (tipTimer <= 0f)
            {
                ShowNextTip();
                tipTimer = tipChangeInterval;
            }
        }

        private void ShowRandomTip()
        {
            if (loadingTips == null || loadingTips.Length == 0) return;

            currentTipIndex = Random.Range(0, loadingTips.Length);
            UpdateTipDisplay();
            tipTimer = tipChangeInterval;
        }

        private void ShowNextTip()
        {
            if (loadingTips == null || loadingTips.Length == 0) return;

            currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
            UpdateTipDisplay();
        }

        private void UpdateTipDisplay()
        {
            if (tipText != null && loadingTips != null && loadingTips.Length > 0)
            {
                tipText.text = $"TIP: {loadingTips[currentTipIndex]}";
            }
        }
    }
}
