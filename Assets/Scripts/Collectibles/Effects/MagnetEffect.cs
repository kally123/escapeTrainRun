using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Magnet effect - attracts nearby coins to the player.
    /// </summary>
    public class MagnetEffect : MonoBehaviour, IPowerUpEffect
    {
        [Header("Magnet Settings")]
        [SerializeField] private float magnetRange = Constants.MagnetRange;
        [SerializeField] private float rangeMultiplier = 1f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem magnetParticles;
        [SerializeField] private GameObject magnetVisual;

        private PlayerController player;
        private CoinManager coinManager;
        private bool isActive;

        public bool IsActive => isActive;
        public float CurrentRange => magnetRange * rangeMultiplier;

        public void Activate(PlayerController player)
        {
            this.player = player;
            isActive = true;

            // Get coin manager
            coinManager = CoinManager.Instance;

            // Enable magnet on coin manager
            if (coinManager != null && player != null)
            {
                coinManager.EnableMagnet(player.transform, CurrentRange);
            }

            // Visual effects
            if (magnetParticles != null)
            {
                magnetParticles.Play();
            }

            if (magnetVisual != null)
            {
                magnetVisual.SetActive(true);
            }

            Debug.Log($"[MagnetEffect] Activated with range {CurrentRange}");
        }

        public void Deactivate()
        {
            isActive = false;

            // Disable magnet on coin manager
            if (coinManager != null)
            {
                coinManager.DisableMagnet();
            }

            // Stop visual effects
            if (magnetParticles != null)
            {
                magnetParticles.Stop();
            }

            if (magnetVisual != null)
            {
                magnetVisual.SetActive(false);
            }

            Debug.Log("[MagnetEffect] Deactivated");
        }

        public void Update()
        {
            // Magnet effect is handled by CoinManager
        }

        /// <summary>
        /// Sets the range multiplier (for character abilities).
        /// </summary>
        public void SetRangeMultiplier(float multiplier)
        {
            rangeMultiplier = multiplier;

            if (isActive && coinManager != null && player != null)
            {
                coinManager.EnableMagnet(player.transform, CurrentRange);
            }
        }
    }
}
