using UnityEngine;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Creates parallax scrolling effect for background layers.
    /// Adds depth and motion to the game environment.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Layers")]
        [SerializeField] private ParallaxLayer[] layers;

        [Header("Settings")]
        [SerializeField] private bool autoScroll = true;
        [SerializeField] private float baseScrollSpeed = 5f;
        [SerializeField] private bool usePlayerSpeed = true;

        // Player reference
        private Transform playerTransform;
        private float lastPlayerZ;

        private void Start()
        {
            FindPlayer();
            InitializeLayers();
        }

        private void FindPlayer()
        {
            if (ServiceLocator.TryGet<Player.PlayerController>(out var player))
            {
                playerTransform = player.transform;
                lastPlayerZ = playerTransform.position.z;
            }
        }

        private void InitializeLayers()
        {
            foreach (var layer in layers)
            {
                if (layer.renderer != null)
                {
                    layer.startOffset = layer.renderer.material.mainTextureOffset;
                }
            }
        }

        private void Update()
        {
            if (!autoScroll) return;

            float scrollSpeed = baseScrollSpeed;

            // Use player movement speed if available
            if (usePlayerSpeed && playerTransform != null)
            {
                float playerDelta = playerTransform.position.z - lastPlayerZ;
                scrollSpeed = playerDelta / Time.deltaTime;
                lastPlayerZ = playerTransform.position.z;
            }

            UpdateLayers(scrollSpeed);
        }

        private void UpdateLayers(float speed)
        {
            foreach (var layer in layers)
            {
                if (layer.renderer == null) continue;

                // Calculate scroll amount based on parallax factor
                float scrollAmount = speed * layer.parallaxFactor * Time.deltaTime * 0.1f;

                // Update texture offset
                Vector2 offset = layer.renderer.material.mainTextureOffset;
                offset.x += scrollAmount * layer.scrollDirection.x;
                offset.y += scrollAmount * layer.scrollDirection.y;

                layer.renderer.material.mainTextureOffset = offset;
            }
        }

        /// <summary>
        /// Sets the scroll speed multiplier.
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            baseScrollSpeed *= multiplier;
        }

        /// <summary>
        /// Resets all layers to starting position.
        /// </summary>
        public void ResetLayers()
        {
            foreach (var layer in layers)
            {
                if (layer.renderer != null)
                {
                    layer.renderer.material.mainTextureOffset = layer.startOffset;
                }
            }
        }

        /// <summary>
        /// Enables or disables auto-scrolling.
        /// </summary>
        public void SetAutoScroll(bool enabled)
        {
            autoScroll = enabled;
        }
    }

    /// <summary>
    /// Represents a single parallax layer.
    /// </summary>
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("The renderer for this layer")]
        public Renderer renderer;

        [Tooltip("Parallax factor: 0 = no movement, 1 = full speed")]
        [Range(0f, 1f)]
        public float parallaxFactor = 0.5f;

        [Tooltip("Direction of scroll (normalized)")]
        public Vector2 scrollDirection = Vector2.right;

        [HideInInspector]
        public Vector2 startOffset;
    }
}
