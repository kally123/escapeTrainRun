using UnityEngine;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Bus-themed obstacles: Backpacks, Straps, Sports Equipment, Books, etc.
    /// </summary>
    public class BusObstacle : Obstacle
    {
        [Header("Bus Obstacle Settings")]
        [SerializeField] private BusObstacleType busType = BusObstacleType.Backpack;

        [Header("Visual Variations")]
        [SerializeField] private GameObject[] visualVariations;
        [SerializeField] private Color[] colorVariations;

        public BusObstacleType BusType => busType;

        public override void Initialize(ThemeType theme)
        {
            base.Initialize(theme);

            ApplyRandomVariation();
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

            // Apply random color
            if (colorVariations != null && colorVariations.Length > 0)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                Color color = colorVariations[Random.Range(0, colorVariations.Length)];
                foreach (var renderer in renderers)
                {
                    if (renderer.material != null)
                    {
                        renderer.material.color = color;
                    }
                }
            }
        }

        protected override void OnPlayerHit(Collider playerCollider)
        {
            base.OnPlayerHit(playerCollider);

            // Specific effects for bus obstacles
            // e.g., books scattering, backpack bouncing
        }
    }

    /// <summary>
    /// Specific bus obstacle types.
    /// </summary>
    public enum BusObstacleType
    {
        /// <summary>School backpack on the floor.</summary>
        Backpack,
        /// <summary>Hanging strap - overhead, slide under.</summary>
        HangingStrap,
        /// <summary>Sports equipment (balls, bats).</summary>
        SportsEquipment,
        /// <summary>Stack of books - low, jump over.</summary>
        StackedBooks,
        /// <summary>Musical instrument case.</summary>
        InstrumentCase,
        /// <summary>Lunchbox on seat.</summary>
        Lunchbox
    }
}
