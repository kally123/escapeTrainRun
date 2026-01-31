using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Player;

namespace EscapeTrainRun.Characters
{
    /// <summary>
    /// Controls the visual representation of the selected character.
    /// Handles model swapping, animations, and visual effects.
    /// </summary>
    public class CharacterModelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform modelContainer;
        [SerializeField] private PlayerAnimation playerAnimation;

        [Header("Effects")]
        [SerializeField] private ParticleSystem swapEffect;
        [SerializeField] private AudioClip swapSound;

        // State
        private GameObject currentModel;
        private CharacterData currentCharacter;
        private Animator currentAnimator;

        private void OnEnable()
        {
            GameEvents.OnCharacterSelected += OnCharacterSelected;
        }

        private void OnDisable()
        {
            GameEvents.OnCharacterSelected -= OnCharacterSelected;
        }

        private void Start()
        {
            // Load initial character
            if (CharacterManager.Instance != null && CharacterManager.Instance.CurrentCharacter != null)
            {
                OnCharacterSelected(CharacterManager.Instance.CurrentCharacter);
            }
        }

        private void OnCharacterSelected(CharacterData character)
        {
            if (character == null) return;

            // Don't swap if same character
            if (currentCharacter != null && currentCharacter.CharacterId == character.CharacterId)
            {
                return;
            }

            currentCharacter = character;
            SwapModel(character);
        }

        /// <summary>
        /// Swaps the current character model.
        /// </summary>
        private void SwapModel(CharacterData character)
        {
            // Play swap effect
            if (swapEffect != null)
            {
                swapEffect.Play();
            }

            // Play swap sound
            if (swapSound != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(swapSound);
            }

            // Destroy old model
            if (currentModel != null)
            {
                Destroy(currentModel);
            }

            // Spawn new model
            if (character.ModelPrefab != null && modelContainer != null)
            {
                currentModel = Instantiate(character.ModelPrefab, modelContainer);
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.identity;
                currentModel.transform.localScale = Vector3.one;

                // Get animator
                currentAnimator = currentModel.GetComponent<Animator>();

                // Apply animator controller if specified
                if (currentAnimator != null && character.AnimatorController != null)
                {
                    currentAnimator.runtimeAnimatorController = character.AnimatorController;
                }

                // Update player animation reference
                if (playerAnimation != null)
                {
                    playerAnimation.SetAnimator(currentAnimator);
                }
            }

            Debug.Log($"[CharacterModelController] Swapped to: {character.DisplayName}");
        }

        /// <summary>
        /// Gets the current model's animator.
        /// </summary>
        public Animator GetAnimator()
        {
            return currentAnimator;
        }

        /// <summary>
        /// Gets the current model GameObject.
        /// </summary>
        public GameObject GetModel()
        {
            return currentModel;
        }

        /// <summary>
        /// Applies a temporary color tint to the model.
        /// </summary>
        public void ApplyColorTint(Color color, float duration)
        {
            if (currentModel == null) return;

            StartCoroutine(ColorTintCoroutine(color, duration));
        }

        private System.Collections.IEnumerator ColorTintCoroutine(Color color, float duration)
        {
            var renderers = currentModel.GetComponentsInChildren<Renderer>();
            var originalColors = new Color[renderers.Length];

            // Store original colors and apply tint
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = renderers[i].material.color;
                    renderers[i].material.color = color;
                }
            }

            yield return new WaitForSeconds(duration);

            // Restore original colors
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }

        /// <summary>
        /// Makes the model flash (for damage, invincibility, etc.)
        /// </summary>
        public void FlashModel(int flashCount, float flashInterval)
        {
            if (currentModel == null) return;

            StartCoroutine(FlashCoroutine(flashCount, flashInterval));
        }

        private System.Collections.IEnumerator FlashCoroutine(int count, float interval)
        {
            var renderers = currentModel.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < count; i++)
            {
                // Hide
                foreach (var r in renderers)
                {
                    if (r != null) r.enabled = false;
                }

                yield return new WaitForSeconds(interval);

                // Show
                foreach (var r in renderers)
                {
                    if (r != null) r.enabled = true;
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }
}
