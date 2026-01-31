using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Shield effect - protects player from one obstacle collision.
    /// </summary>
    public class ShieldEffect : MonoBehaviour, IPowerUpEffect
    {
        [Header("Visual Effects")]
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private ParticleSystem shieldParticles;
        [SerializeField] private Material shieldMaterial;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.2f;
        [SerializeField] private float rotationSpeed = 30f;

        private PlayerController player;
        private bool isActive;
        private Transform shieldTransform;
        private float pulseTimer;

        public bool IsActive => isActive;

        private void Update()
        {
            if (!isActive || shieldTransform == null) return;

            AnimateShield();
        }

        public void Activate(PlayerController player)
        {
            this.player = player;
            isActive = true;

            // Set player invincibility
            if (player != null)
            {
                player.SetInvincible(true);
            }

            // Create/show shield visual
            CreateShieldVisual();

            // Start particles
            if (shieldParticles != null)
            {
                shieldParticles.Play();
            }

            Debug.Log("[ShieldEffect] Activated - Player is now protected");
        }

        public void Deactivate()
        {
            isActive = false;

            // Remove player invincibility
            if (player != null)
            {
                player.SetInvincible(false);
            }

            // Hide shield visual
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            // Stop particles
            if (shieldParticles != null)
            {
                shieldParticles.Stop();
            }

            Debug.Log("[ShieldEffect] Deactivated - Shield removed");
        }

        void IPowerUpEffect.Update()
        {
            // Animation handled in MonoBehaviour Update
        }

        private void CreateShieldVisual()
        {
            if (player == null) return;

            if (shieldVisual == null)
            {
                // Create default shield visual
                shieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shieldVisual.name = "ShieldBubble";
                shieldVisual.transform.SetParent(player.transform);
                shieldVisual.transform.localPosition = Vector3.zero;
                shieldVisual.transform.localScale = Vector3.one * 2f;

                // Remove collider
                var collider = shieldVisual.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                // Apply material
                var renderer = shieldVisual.GetComponent<Renderer>();
                if (renderer != null && shieldMaterial != null)
                {
                    renderer.material = shieldMaterial;
                }
                else if (renderer != null)
                {
                    // Create transparent blue material
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.3f, 0.7f, 1f, 0.3f);
                    mat.SetFloat("_Mode", 3); // Transparent
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.renderQueue = 3000;
                    renderer.material = mat;
                }
            }

            shieldVisual.SetActive(true);
            shieldTransform = shieldVisual.transform;
        }

        private void AnimateShield()
        {
            if (shieldTransform == null) return;

            // Rotation
            shieldTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Pulsing scale
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = 2f + Mathf.Sin(pulseTimer) * pulseIntensity;
            shieldTransform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Called when shield absorbs a hit.
        /// </summary>
        public void OnHitAbsorbed()
        {
            // Could trigger special effect when shield blocks something
            if (shieldParticles != null)
            {
                shieldParticles.Emit(20);
            }

            Debug.Log("[ShieldEffect] Absorbed a hit!");
        }
    }
}
