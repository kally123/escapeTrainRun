using UnityEngine;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Base class for power-up pickups.
    /// Handles collection and activation.
    /// </summary>
    public class PowerUp : MonoBehaviour
    {
        [Header("Power-Up Settings")]
        [SerializeField] private PowerUpType powerUpType = PowerUpType.Magnet;

        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private float bobSpeed = 1.5f;
        [SerializeField] private float pulseScale = 0.1f;
        [SerializeField] private float pulseSpeed = 2f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem idleParticles;
        [SerializeField] private ParticleSystem collectEffect;
        [SerializeField] private Light glowLight;

        // State
        private bool isCollected;
        private Vector3 startPosition;
        private Vector3 originalScale;
        private float bobOffset;

        public PowerUpType Type => powerUpType;
        public bool IsCollected => isCollected;

        private void Start()
        {
            startPosition = transform.position;
            originalScale = transform.localScale;
            bobOffset = Random.Range(0f, Mathf.PI * 2f);

            if (idleParticles != null)
            {
                idleParticles.Play();
            }
        }

        private void Update()
        {
            if (isCollected) return;

            AnimatePowerUp();
        }

        #region Animation

        private void AnimatePowerUp()
        {
            // Rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Bobbing
            float bobY = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
            transform.position = startPosition + new Vector3(0, bobY, 0);

            // Pulsing scale
            float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = originalScale * (1f + scaleOffset);

            // Light pulsing
            if (glowLight != null)
            {
                glowLight.intensity = 1f + scaleOffset * 2f;
            }
        }

        #endregion

        #region Collection

        /// <summary>
        /// Collects this power-up and returns its type.
        /// </summary>
        public PowerUpType Collect()
        {
            if (isCollected) return powerUpType;

            isCollected = true;

            // Play effects
            PlayCollectEffects();

            // Stop idle effects
            if (idleParticles != null)
            {
                idleParticles.Stop();
            }

            // Disable visuals
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            if (glowLight != null)
            {
                glowLight.enabled = false;
            }

            // Delay destroy to allow particles
            if (collectEffect != null)
            {
                collectEffect.transform.SetParent(null);
                Destroy(collectEffect.gameObject, 2f);
            }

            Destroy(gameObject, 0.1f);

            return powerUpType;
        }

        private void PlayCollectEffects()
        {
            if (collectEffect != null)
            {
                collectEffect.Play();
            }
        }

        #endregion

        #region Pool Support

        /// <summary>
        /// Resets the power-up for reuse from object pool.
        /// </summary>
        public void Reset()
        {
            isCollected = false;

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }

            if (glowLight != null)
            {
                glowLight.enabled = true;
            }

            if (idleParticles != null)
            {
                idleParticles.Play();
            }
        }

        /// <summary>
        /// Sets the power-up type.
        /// </summary>
        public void SetType(PowerUpType type)
        {
            powerUpType = type;
            UpdateVisuals();
        }

        /// <summary>
        /// Sets the power-up's world position.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            startPosition = position;
        }

        private void UpdateVisuals()
        {
            // Update visuals based on type
            // This would change colors, particles, etc. based on the power-up type
            Color typeColor = GetTypeColor();

            if (glowLight != null)
            {
                glowLight.color = typeColor;
            }

            // Update material color
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = typeColor;
                }
            }
        }

        private Color GetTypeColor()
        {
            return powerUpType switch
            {
                PowerUpType.Magnet => Color.blue,
                PowerUpType.Shield => Color.cyan,
                PowerUpType.SpeedBoost => Color.red,
                PowerUpType.StarPower => Color.yellow,
                PowerUpType.Multiplier => new Color(1f, 0.5f, 0f), // Orange
                PowerUpType.MysteryBox => Color.magenta,
                _ => Color.white
            };
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GetTypeColor();
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        #endregion
    }
}
