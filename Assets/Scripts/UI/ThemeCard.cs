using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// UI card for a single theme in the selection screen.
    /// </summary>
    public class ThemeCard : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image previewImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Image borderImage;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Colors")]
        [SerializeField] private Color normalBorderColor = Color.white;
        [SerializeField] private Color selectedBorderColor = Color.yellow;
        [SerializeField] private float selectedScale = 1.1f;

        [Header("Button")]
        [SerializeField] private Button cardButton;

        // State
        private ThemeType themeType;
        private ThemeData themeData;
        private bool isSelected;
        private bool isLocked;
        private System.Action<ThemeType> onClickCallback;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;

            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnCardClicked);
            }
        }

        /// <summary>
        /// Initializes the card with theme data.
        /// </summary>
        public void Initialize(ThemeType type, ThemeData data, System.Action<ThemeType> onClick)
        {
            themeType = type;
            themeData = data;
            onClickCallback = onClick;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (themeData == null)
            {
                // Use defaults based on theme type
                if (nameText != null)
                {
                    nameText.text = themeType.ToString();
                }
                return;
            }

            // Name
            if (nameText != null)
            {
                nameText.text = themeData.DisplayName;
            }

            // Icon
            if (iconImage != null && themeData.IconImage != null)
            {
                iconImage.sprite = themeData.IconImage;
            }

            // Preview
            if (previewImage != null && themeData.PreviewImage != null)
            {
                previewImage.sprite = themeData.PreviewImage;
            }

            // Background color
            if (backgroundImage != null)
            {
                backgroundImage.color = themeData.ThemeColor;
            }

            // Locked state
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(isLocked);
            }
        }

        private void OnCardClicked()
        {
            if (isLocked) return;

            onClickCallback?.Invoke(themeType);
        }

        /// <summary>
        /// Sets the selected state of the card.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            // Update visual state
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(selected);
            }

            if (borderImage != null)
            {
                borderImage.color = selected ? selectedBorderColor : normalBorderColor;
            }

            // Scale animation
            transform.localScale = selected ? originalScale * selectedScale : originalScale;
        }

        /// <summary>
        /// Sets the locked state of the card.
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(locked);
            }

            if (cardButton != null)
            {
                cardButton.interactable = !locked;
            }
        }
    }
}
