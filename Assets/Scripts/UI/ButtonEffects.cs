using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Adds visual effects to UI buttons (scale, color, sound).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Effects")]
        [SerializeField] private bool useScaleEffect = true;
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float scaleSpeed = 10f;

        [Header("Color Effects")]
        [SerializeField] private bool useColorEffect = false;
        [SerializeField] private Color hoverColor = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f);

        [Header("Sound Effects")]
        [SerializeField] private bool useSoundEffects = true;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        // State
        private Button button;
        private Image buttonImage;
        private Color originalColor;
        private Vector3 originalScale;
        private float targetScale = 1f;
        private bool isHovering;
        private bool isPressed;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();

            if (buttonImage != null)
            {
                originalColor = buttonImage.color;
            }

            originalScale = transform.localScale;
        }

        private void Update()
        {
            if (useScaleEffect)
            {
                UpdateScale();
            }
        }

        private void UpdateScale()
        {
            float scale = isPressed ? pressedScale : (isHovering ? hoverScale : 1f);
            targetScale = Mathf.Lerp(targetScale, scale, scaleSpeed * Time.unscaledDeltaTime);
            transform.localScale = originalScale * targetScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isHovering = true;

            if (useColorEffect && buttonImage != null)
            {
                buttonImage.color = hoverColor;
            }

            if (useSoundEffects && hoverSound != null)
            {
                PlaySound(hoverSound);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            isPressed = false;

            if (useColorEffect && buttonImage != null)
            {
                buttonImage.color = originalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPressed = true;

            if (useColorEffect && buttonImage != null)
            {
                buttonImage.color = pressedColor;
            }

            if (useSoundEffects && clickSound != null)
            {
                PlaySound(clickSound);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;

            if (useColorEffect && buttonImage != null)
            {
                buttonImage.color = isHovering ? hoverColor : originalColor;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (Core.ServiceLocator.TryGet<Core.AudioManager>(out var audio))
            {
                audio.PlaySFX(clip);
            }
        }

        private void OnDisable()
        {
            // Reset state
            isHovering = false;
            isPressed = false;
            targetScale = 1f;
            transform.localScale = originalScale;

            if (buttonImage != null)
            {
                buttonImage.color = originalColor;
            }
        }
    }
}
