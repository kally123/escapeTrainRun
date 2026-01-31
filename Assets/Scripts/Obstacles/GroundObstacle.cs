using UnityEngine;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Ground/Park-themed obstacles: Benches, Branches, Sprinklers, Playful Dogs, etc.
    /// </summary>
    public class GroundObstacle : Obstacle
    {
        [Header("Ground Obstacle Settings")]
        [SerializeField] private GroundObstacleType groundType = GroundObstacleType.ParkBench;

        [Header("Visual Variations")]
        [SerializeField] private GameObject[] visualVariations;
        [SerializeField] private Material[] seasonalMaterials;

        [Header("Animated Elements")]
        [SerializeField] private Animator obstacleAnimator;
        [SerializeField] private ParticleSystem particleEffect;

        public GroundObstacleType GroundType => groundType;

        public override void Initialize(ThemeType theme)
        {
            base.Initialize(theme);

            ApplyRandomVariation();
            StartAnimations();
        }

        private void ApplyRandomVariation()
        {
            // Activate random visual variant
            if (visualVariations != null && visualVariations.Length > 0)
            {
                int index = Random.Range(0, visualVariations.Length);
                for (int i = 0; i < visualVariations.Length; i++)
                {
                    if (visualVariations[i] != null)
                    {
                        visualVariations[i].SetActive(i == index);
                    }
                }
            }

            // Apply seasonal material variation
            if (seasonalMaterials != null && seasonalMaterials.Length > 0)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                Material mat = seasonalMaterials[Random.Range(0, seasonalMaterials.Length)];
                foreach (var renderer in renderers)
                {
                    renderer.material = mat;
                }
            }
        }

        private void StartAnimations()
        {
            // Start any animated elements
            if (obstacleAnimator != null)
            {
                obstacleAnimator.SetTrigger("Idle");
            }

            // Start particle effects (e.g., sprinkler water)
            if (particleEffect != null && groundType == GroundObstacleType.Sprinkler)
            {
                particleEffect.Play();
            }
        }

        protected override void OnPlayerHit(Collider playerCollider)
        {
            base.OnPlayerHit(playerCollider);

            // Specific effects for ground obstacles
            switch (groundType)
            {
                case GroundObstacleType.PlayfulDog:
                    // Dog could bark or react
                    if (obstacleAnimator != null)
                    {
                        obstacleAnimator.SetTrigger("React");
                    }
                    break;

                case GroundObstacleType.Sprinkler:
                    // Player gets splashed
                    break;

                case GroundObstacleType.Leaves:
                    // Leaves scatter
                    if (particleEffect != null)
                    {
                        particleEffect.Play();
                    }
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            // Reset animations
            if (obstacleAnimator != null)
            {
                obstacleAnimator.SetTrigger("Idle");
            }

            // Reset particle effects
            if (particleEffect != null)
            {
                particleEffect.Stop();
                if (groundType == GroundObstacleType.Sprinkler)
                {
                    particleEffect.Play();
                }
            }
        }
    }

    /// <summary>
    /// Specific ground/park obstacle types.
    /// </summary>
    public enum GroundObstacleType
    {
        /// <summary>Park bench - jump over.</summary>
        ParkBench,
        /// <summary>Fallen tree branch - jump over.</summary>
        FallenBranch,
        /// <summary>Active sprinkler - slide through.</summary>
        Sprinkler,
        /// <summary>Playful dog that moves - avoid by changing lanes.</summary>
        PlayfulDog,
        /// <summary>Pile of leaves - visual effect on hit.</summary>
        Leaves,
        /// <summary>Low fence - jump over.</summary>
        LowFence,
        /// <summary>Trash can - blocks lane.</summary>
        TrashCan,
        /// <summary>Bicycle rack - blocks lane.</summary>
        BicycleRack
    }
}
