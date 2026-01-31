using UnityEngine;
using System.Collections;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Flashes a material color for damage, invincibility, or other effects.
    /// </summary>
    public class MaterialFlasher : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private int materialIndex = 0;

        [Header("Flash Settings")]
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private int flashCount = 3;

        [Header("Invincibility Flash")]
        [SerializeField] private float invincibilityFlashSpeed = 10f;
        [SerializeField] private Color invincibilityColor = new Color(1f, 1f, 1f, 0.5f);

        [Header("Damage Flash")]
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float damageFlashDuration = 0.2f;

        // State
        private Material material;
        private Color originalColor;
        private Coroutine flashCoroutine;
        private bool isInvincible;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }

            if (targetRenderer != null && targetRenderer.materials.Length > materialIndex)
            {
                material = targetRenderer.materials[materialIndex];
                originalColor = material.color;
            }
        }

        #region Public API

        /// <summary>
        /// Flashes the material with the default color.
        /// </summary>
        public void Flash()
        {
            Flash(flashColor, flashDuration, flashCount);
        }

        /// <summary>
        /// Flashes the material with a custom color.
        /// </summary>
        public void Flash(Color color, float duration, int count = 1)
        {
            if (material == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(FlashCoroutine(color, duration, count));
        }

        /// <summary>
        /// Flashes for damage effect.
        /// </summary>
        public void FlashDamage()
        {
            Flash(damageColor, damageFlashDuration, 2);
        }

        /// <summary>
        /// Starts invincibility flashing.
        /// </summary>
        public void StartInvincibilityFlash()
        {
            isInvincible = true;
            StartCoroutine(InvincibilityFlashCoroutine());
        }

        /// <summary>
        /// Stops invincibility flashing.
        /// </summary>
        public void StopInvincibilityFlash()
        {
            isInvincible = false;
            ResetColor();
        }

        /// <summary>
        /// Sets the material to a solid color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (material != null)
            {
                material.color = color;
            }
        }

        /// <summary>
        /// Resets to the original color.
        /// </summary>
        public void ResetColor()
        {
            if (material != null)
            {
                material.color = originalColor;
            }
        }

        /// <summary>
        /// Sets the emission color (for glowing effects).
        /// </summary>
        public void SetEmission(Color color, float intensity = 1f)
        {
            if (material != null && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
            }
        }

        /// <summary>
        /// Disables emission.
        /// </summary>
        public void DisableEmission()
        {
            if (material != null && material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator FlashCoroutine(Color color, float duration, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Flash on
                material.color = color;
                yield return new WaitForSeconds(duration / 2f);

                // Flash off
                material.color = originalColor;
                yield return new WaitForSeconds(duration / 2f);
            }

            flashCoroutine = null;
        }

        private IEnumerator InvincibilityFlashCoroutine()
        {
            while (isInvincible)
            {
                float t = (Mathf.Sin(Time.time * invincibilityFlashSpeed) + 1f) / 2f;
                material.color = Color.Lerp(originalColor, invincibilityColor, t);
                yield return null;
            }

            material.color = originalColor;
        }

        #endregion

        private void OnDisable()
        {
            isInvincible = false;
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
            ResetColor();
        }

        private void OnDestroy()
        {
            // Clean up instantiated material
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
