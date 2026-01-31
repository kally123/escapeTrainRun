using UnityEngine;
using EscapeTrainRun.Characters;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Displays a 3D preview of the selected character.
    /// Handles rotation, animation preview, and visual effects.
    /// </summary>
    public class CharacterPreviewUI : MonoBehaviour
    {
        [Header("Preview Setup")]
        [SerializeField] private Transform modelContainer;
        [SerializeField] private Camera previewCamera;
        [SerializeField] private RenderTexture previewRenderTexture;

        [Header("Rotation")]
        [SerializeField] private bool allowRotation = true;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float autoRotateSpeed = 20f;
        [SerializeField] private bool autoRotate = true;

        [Header("Animation")]
        [SerializeField] private bool playIdleAnimation = true;
        [SerializeField] private float animationPreviewDuration = 3f;

        [Header("Lighting")]
        [SerializeField] private Light previewLight;
        [SerializeField] private Color defaultLightColor = Color.white;

        // State
        private GameObject currentModel;
        private Animator currentAnimator;
        private CharacterData currentCharacter;
        private bool isDragging;
        private float dragStartX;
        private float modelStartRotation;

        private void Update()
        {
            HandleRotationInput();

            if (autoRotate && !isDragging && currentModel != null)
            {
                currentModel.transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Shows a character in the preview.
        /// </summary>
        public void ShowCharacter(CharacterData character)
        {
            if (character == null) return;

            currentCharacter = character;

            // Clear existing model
            ClearPreview();

            // Spawn new model
            if (character.ModelPrefab != null && modelContainer != null)
            {
                currentModel = Instantiate(character.ModelPrefab, modelContainer);
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.identity;
                currentModel.transform.localScale = Vector3.one;

                // Set layer for preview camera
                SetLayerRecursively(currentModel, gameObject.layer);

                // Get animator and play idle
                currentAnimator = currentModel.GetComponent<Animator>();
                if (currentAnimator != null && character.AnimatorController != null)
                {
                    currentAnimator.runtimeAnimatorController = character.AnimatorController;

                    if (playIdleAnimation)
                    {
                        currentAnimator.Play("Idle");
                    }
                }

                // Update lighting to match character theme
                UpdateLighting(character);
            }
        }

        /// <summary>
        /// Clears the current preview.
        /// </summary>
        public void ClearPreview()
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
                currentModel = null;
                currentAnimator = null;
            }
        }

        /// <summary>
        /// Plays an animation on the preview model.
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            if (currentAnimator != null)
            {
                currentAnimator.Play(animationName);
                
                // Return to idle after duration
                if (playIdleAnimation)
                {
                    StartCoroutine(ReturnToIdleCoroutine());
                }
            }
        }

        private System.Collections.IEnumerator ReturnToIdleCoroutine()
        {
            yield return new WaitForSeconds(animationPreviewDuration);

            if (currentAnimator != null)
            {
                currentAnimator.Play("Idle");
            }
        }

        #region Rotation Handling

        private void HandleRotationInput()
        {
            if (!allowRotation || currentModel == null) return;

            // Mouse/touch drag rotation
            if (Input.GetMouseButtonDown(0))
            {
                // Check if over preview area (simplified - full implementation would use EventSystem)
                isDragging = true;
                dragStartX = Input.mousePosition.x;
                modelStartRotation = currentModel.transform.eulerAngles.y;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                float delta = Input.mousePosition.x - dragStartX;
                float newRotation = modelStartRotation - delta * rotationSpeed * 0.01f;
                currentModel.transform.rotation = Quaternion.Euler(0f, newRotation, 0f);
            }
        }

        /// <summary>
        /// Resets the model rotation.
        /// </summary>
        public void ResetRotation()
        {
            if (currentModel != null)
            {
                currentModel.transform.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Sets whether auto-rotation is enabled.
        /// </summary>
        public void SetAutoRotate(bool enabled)
        {
            autoRotate = enabled;
        }

        #endregion

        #region Utility

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private void UpdateLighting(CharacterData character)
        {
            if (previewLight == null) return;

            // Use character's theme color for accent lighting
            Color lightColor = Color.Lerp(defaultLightColor, character.ThemeColor, 0.3f);
            previewLight.color = lightColor;
        }

        #endregion

        private void OnDisable()
        {
            ClearPreview();
        }
    }
}
