using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Core;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Score multiplier effect - doubles all score gains for the duration.
    /// </summary>
    public class MultiplierEffect : MonoBehaviour, IPowerUpEffect
    {
        [Header("Multiplier Settings")]
        [SerializeField] private int multiplierValue = Constants.MultiplierValue;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem multiplierParticles;
        [SerializeField] private GameObject multiplierVisual;
        [SerializeField] private Color multiplierColor = new Color(1f, 0.5f, 0f);

        private PlayerController player;
        private ScoreManager scoreManager;
        private bool isActive;

        public bool IsActive => isActive;
        public int MultiplierValue => multiplierValue;

        public void Activate(PlayerController player)
        {
            this.player = player;
            isActive = true;

            // Set multiplier on score manager
            if (ServiceLocator.TryGet<ScoreManager>(out scoreManager))
            {
                scoreManager.SetScoreMultiplier(multiplierValue);
            }

            // Visual effects
            StartVisualEffects();

            Debug.Log($"[MultiplierEffect] Activated - {multiplierValue}x score multiplier!");
        }

        public void Deactivate()
        {
            isActive = false;

            // Reset multiplier
            if (scoreManager != null)
            {
                scoreManager.SetScoreMultiplier(1);
            }

            // Stop visual effects
            StopVisualEffects();

            Debug.Log("[MultiplierEffect] Deactivated - Score multiplier reset");
        }

        public void Update()
        {
            if (!isActive) return;

            UpdateVisualEffects();
        }

        private void StartVisualEffects()
        {
            if (multiplierParticles != null)
            {
                var main = multiplierParticles.main;
                main.startColor = multiplierColor;
                multiplierParticles.Play();
            }

            if (multiplierVisual != null)
            {
                multiplierVisual.SetActive(true);
            }
        }

        private void StopVisualEffects()
        {
            if (multiplierParticles != null)
            {
                multiplierParticles.Stop();
            }

            if (multiplierVisual != null)
            {
                multiplierVisual.SetActive(false);
            }
        }

        private void UpdateVisualEffects()
        {
            // Could animate multiplier display
            // Could pulse color intensity
        }
    }
}
