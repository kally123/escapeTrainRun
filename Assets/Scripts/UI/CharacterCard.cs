using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Characters;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// UI card representing a single character in the selection grid.
    /// </summary>
    public class CharacterCard : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image portrait;
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private Image rarityIndicator;
        [SerializeField] private Text nameText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Image lockIcon;
        [SerializeField] private GameObject selectedIndicator;

        [Header("Colors")]
        [SerializeField] private Color unlockedBackground = Color.white;
        [SerializeField] private Color lockedBackground = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color selectedBorderColor = Color.yellow;
        [SerializeField] private Color normalBorderColor = Color.white;

        [Header("Button")]
        [SerializeField] private Button cardButton;

        // State
        private CharacterData character;
        private bool isUnlocked;
        private bool isSelected;
        private System.Action<CharacterData> onClickCallback;

        public CharacterData Character => character;

        private void Awake()
        {
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnCardClicked);
            }
        }

        /// <summary>
        /// Initializes the card with character data.
        /// </summary>
        public void Initialize(CharacterData character, bool isUnlocked, System.Action<CharacterData> onClick)
        {
            this.character = character;
            this.isUnlocked = isUnlocked;
            this.onClickCallback = onClick;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (character == null) return;

            // Portrait
            if (portrait != null)
            {
                if (character.Icon != null)
                {
                    portrait.sprite = character.Icon;
                }
                else if (character.Portrait != null)
                {
                    portrait.sprite = character.Portrait;
                }

                // Grayscale if locked
                portrait.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // Name
            if (nameText != null)
            {
                nameText.text = character.DisplayName;
                nameText.color = isUnlocked ? character.GetRarityColor() : Color.gray;
            }

            // Background
            if (background != null)
            {
                background.color = isUnlocked ? unlockedBackground : lockedBackground;
            }

            // Rarity indicator
            if (rarityIndicator != null)
            {
                rarityIndicator.color = character.GetRarityColor();
            }

            // Locked overlay
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!isUnlocked);
            }

            // Selected indicator (off by default)
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(false);
            }

            // Border
            if (border != null)
            {
                border.color = normalBorderColor;
            }
        }

        private void OnCardClicked()
        {
            onClickCallback?.Invoke(character);
        }

        /// <summary>
        /// Sets the selected state of the card.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(selected);
            }

            if (border != null)
            {
                border.color = selected ? selectedBorderColor : normalBorderColor;
            }
        }

        /// <summary>
        /// Sets the unlocked state of the card.
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            UpdateDisplay();
        }

        /// <summary>
        /// Plays an unlock animation.
        /// </summary>
        public void PlayUnlockAnimation()
        {
            // Could animate the card when character is unlocked
            StartCoroutine(UnlockAnimationCoroutine());
        }

        private System.Collections.IEnumerator UnlockAnimationCoroutine()
        {
            // Scale up
            Vector3 originalScale = transform.localScale;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
                transform.localScale = originalScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;

            // Update display
            SetUnlocked(true);
        }
    }
}
