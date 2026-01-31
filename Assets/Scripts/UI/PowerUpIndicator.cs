using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// UI indicator for active power-ups.
    /// Shows icon, remaining time, and warning state.
    /// </summary>
    public class PowerUpIndicator : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text timerText;

        [Header("Icons")]
        [SerializeField] private Sprite magnetIcon;
        [SerializeField] private Sprite shieldIcon;
        [SerializeField] private Sprite speedIcon;
        [SerializeField] private Sprite starIcon;
        [SerializeField] private Sprite multiplierIcon;
        [SerializeField] private Sprite mysteryIcon;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.red;
        [SerializeField] private Color magnetColor = Color.blue;
        [SerializeField] private Color shieldColor = Color.cyan;
        [SerializeField] private Color speedColor = Color.red;
        [SerializeField] private Color starColor = Color.yellow;
        [SerializeField] private Color multiplierColor = new Color(1f, 0.5f, 0f);

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseScale = 0.1f;
        [SerializeField] private float warningPulseSpeed = 8f;

        // State
        private PowerUpType powerUpType;
        private float duration;
        private float remainingTime;
        private bool isWarning;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        /// <summary>
        /// Initializes the indicator for a power-up type.
        /// </summary>
        public void Initialize(PowerUpType type, float duration)
        {
            this.powerUpType = type;
            this.duration = duration;
            this.remainingTime = duration;
            this.isWarning = false;

            // Set icon
            SetIcon(type);

            // Set color
            SetColor(type);

            // Reset fill
            if (fillImage != null)
            {
                fillImage.fillAmount = 1f;
            }

            // Show timer
            UpdateTimerText();
        }

        private void Update()
        {
            // Update remaining time
            remainingTime -= Time.deltaTime;
            remainingTime = Mathf.Max(0, remainingTime);

            // Update fill
            if (fillImage != null)
            {
                fillImage.fillAmount = remainingTime / duration;
            }

            // Update timer text
            UpdateTimerText();

            // Animate
            AnimateIndicator();
        }

        private void SetIcon(PowerUpType type)
        {
            if (iconImage == null) return;

            Sprite icon = type switch
            {
                PowerUpType.Magnet => magnetIcon,
                PowerUpType.Shield => shieldIcon,
                PowerUpType.SpeedBoost => speedIcon,
                PowerUpType.StarPower => starIcon,
                PowerUpType.Multiplier => multiplierIcon,
                PowerUpType.MysteryBox => mysteryIcon,
                _ => null
            };

            if (icon != null)
            {
                iconImage.sprite = icon;
            }
        }

        private void SetColor(PowerUpType type)
        {
            Color color = type switch
            {
                PowerUpType.Magnet => magnetColor,
                PowerUpType.Shield => shieldColor,
                PowerUpType.SpeedBoost => speedColor,
                PowerUpType.StarPower => starColor,
                PowerUpType.Multiplier => multiplierColor,
                _ => normalColor
            };

            if (fillImage != null)
            {
                fillImage.color = color;
            }

            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }

        private void UpdateTimerText()
        {
            if (timerText == null) return;

            if (remainingTime > 0)
            {
                timerText.text = Mathf.CeilToInt(remainingTime).ToString();
            }
            else
            {
                timerText.text = "";
            }
        }

        private void AnimateIndicator()
        {
            float speed = isWarning ? warningPulseSpeed : pulseSpeed;
            float scale = 1f + Mathf.Sin(Time.time * speed) * pulseScale;

            transform.localScale = originalScale * scale;

            // Color pulse during warning
            if (isWarning && iconImage != null)
            {
                float alpha = 0.5f + Mathf.Sin(Time.time * warningPulseSpeed) * 0.5f;
                Color color = iconImage.color;
                color = Color.Lerp(color, warningColor, alpha);
                iconImage.color = color;
            }
        }

        /// <summary>
        /// Sets the warning state (power-up about to expire).
        /// </summary>
        public void SetWarning(bool warning)
        {
            isWarning = warning;

            if (warning && backgroundImage != null)
            {
                backgroundImage.color = warningColor * 0.3f;
            }
        }

        /// <summary>
        /// Updates the remaining time display.
        /// </summary>
        public void UpdateTime(float normalizedTime)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = normalizedTime;
            }

            remainingTime = normalizedTime * duration;
        }
    }
}
