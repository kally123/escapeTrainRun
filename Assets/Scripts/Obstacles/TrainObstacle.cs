using UnityEngine;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Train-themed obstacles: Luggage, Gift Boxes, Trolleys, etc.
    /// </summary>
    public class TrainObstacle : Obstacle
    {
        [Header("Train Obstacle Settings")]
        [SerializeField] private TrainObstacleType trainType = TrainObstacleType.Luggage;

        [Header("Visual Variations")]
        [SerializeField] private GameObject[] visualVariations;
        [SerializeField] private Material[] materialVariations;

        public TrainObstacleType TrainType => trainType;

        public override void Initialize(ThemeType theme)
        {
            base.Initialize(theme);

            // Apply random visual variation
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

            // Apply random material
            if (materialVariations != null && materialVariations.Length > 0)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                Material mat = materialVariations[Random.Range(0, materialVariations.Length)];
                foreach (var renderer in renderers)
                {
                    renderer.material = mat;
                }
            }
        }

        protected override void OnPlayerHit(Collider playerCollider)
        {
            base.OnPlayerHit(playerCollider);

            // Could add specific effects for train obstacles
            // e.g., luggage scattering, gift box popping
        }
    }

    /// <summary>
    /// Specific train obstacle types.
    /// </summary>
    public enum TrainObstacleType
    {
        /// <summary>Suitcase or luggage on the track.</summary>
        Luggage,
        /// <summary>Gift box (holiday themed).</summary>
        GiftBox,
        /// <summary>Luggage trolley - low, jump over.</summary>
        LuggageTrolley,
        /// <summary>Hanging bag - overhead, slide under.</summary>
        HangingBag,
        /// <summary>Food cart - blocks lane.</summary>
        FoodCart,
        /// <summary>Stacked boxes - barrier.</summary>
        StackedBoxes
    }
}
