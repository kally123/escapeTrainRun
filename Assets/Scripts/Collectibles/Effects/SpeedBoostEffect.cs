using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Speed boost effect - temporarily increases player speed and grants invincibility.
    /// </summary>
    public class SpeedBoostEffect : MonoBehaviour, IPowerUpEffect
    {
        [Header("Speed Settings")]
        [SerializeField] private float speedMultiplier = Constants.SpeedBoostMultiplier;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem speedParticles;
        [SerializeField] private ParticleSystem trailParticles;
        [SerializeField] private GameObject speedLinesVisual;

        [Header("Camera Effects")]
        [SerializeField] private float fovIncrease = 10f;
        [SerializeField] private float fovTransitionSpeed = 5f;

        [Header("Post Processing")]
        [SerializeField] private float motionBlurIntensity = 0.5f;

        private PlayerController player;
        private PlayerCamera playerCamera;
        private bool isActive;
        private float originalSpeed;

        public bool IsActive => isActive;
        public float SpeedMultiplier => speedMultiplier;

        public void Activate(PlayerController player)
        {
            this.player = player;
            isActive = true;

            // Apply speed boost
            if (player != null)
            {
                player.SetSpeedMultiplier(speedMultiplier);
                player.SetInvincible(true); // Invincible during speed boost
            }

            // Get camera for effects
            playerCamera = FindObjectOfType<PlayerCamera>();

            // Visual effects
            StartVisualEffects();

            Debug.Log($"[SpeedBoostEffect] Activated with {speedMultiplier}x speed");
        }

        public void Deactivate()
        {
            isActive = false;

            // Reset speed (will naturally decrease)
            if (player != null)
            {
                player.SetInvincible(false);
                // Speed will return to normal gradually
            }

            // Stop visual effects
            StopVisualEffects();

            Debug.Log("[SpeedBoostEffect] Deactivated");
        }

        public void Update()
        {
            if (!isActive) return;

            UpdateVisualEffects();
        }

        private void StartVisualEffects()
        {
            if (speedParticles != null)
            {
                speedParticles.Play();
            }

            if (trailParticles != null)
            {
                trailParticles.Play();
            }

            if (speedLinesVisual != null)
            {
                speedLinesVisual.SetActive(true);
            }

            // Could apply camera FOV increase here
            // Could apply post-processing motion blur
        }

        private void StopVisualEffects()
        {
            if (speedParticles != null)
            {
                speedParticles.Stop();
            }

            if (trailParticles != null)
            {
                trailParticles.Stop();
            }

            if (speedLinesVisual != null)
            {
                speedLinesVisual.SetActive(false);
            }
        }

        private void UpdateVisualEffects()
        {
            // Could update speed lines intensity based on current speed
            // Could update trail length/intensity
        }
    }
}
