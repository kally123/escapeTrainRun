using UnityEngine;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Controls trail renderer effects for player and power-ups.
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class TrailEffect : MonoBehaviour
    {
        [Header("Trail Settings")]
        [SerializeField] private float normalWidth = 0.1f;
        [SerializeField] private float boostedWidth = 0.3f;
        [SerializeField] private float transitionSpeed = 5f;

        [Header("Colors")]
        [SerializeField] private Gradient normalGradient;
        [SerializeField] private Gradient speedBoostGradient;
        [SerializeField] private Gradient starPowerGradient;

        [Header("Speed Based")]
        [SerializeField] private bool scaleWithSpeed;
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float maxSpeed = 25f;
        [SerializeField] private float minWidth = 0.05f;
        [SerializeField] private float maxWidth = 0.2f;

        // Components
        private TrailRenderer trailRenderer;

        // State
        private float targetWidth;
        private Gradient targetGradient;
        private bool isActive = true;

        private void Awake()
        {
            trailRenderer = GetComponent<TrailRenderer>();
            targetWidth = normalWidth;
            targetGradient = normalGradient;

            InitializeDefaultGradients();
        }

        private void Update()
        {
            if (trailRenderer == null) return;

            // Smooth width transition
            float currentWidth = trailRenderer.startWidth;
            if (Mathf.Abs(currentWidth - targetWidth) > 0.001f)
            {
                float newWidth = Mathf.Lerp(currentWidth, targetWidth, transitionSpeed * Time.deltaTime);
                trailRenderer.startWidth = newWidth;
                trailRenderer.endWidth = newWidth * 0.1f;
            }
        }

        private void InitializeDefaultGradients()
        {
            if (normalGradient == null || normalGradient.colorKeys.Length == 0)
            {
                normalGradient = new Gradient();
                normalGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
                );
            }

            if (speedBoostGradient == null || speedBoostGradient.colorKeys.Length == 0)
            {
                speedBoostGradient = new Gradient();
                speedBoostGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.blue, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
                );
            }

            if (starPowerGradient == null || starPowerGradient.colorKeys.Length == 0)
            {
                starPowerGradient = new Gradient();
                starPowerGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
            }
        }

        #region Public API

        /// <summary>
        /// Sets normal trail effect.
        /// </summary>
        public void SetNormal()
        {
            targetWidth = normalWidth;
            SetGradient(normalGradient);
        }

        /// <summary>
        /// Sets speed boost trail effect.
        /// </summary>
        public void SetSpeedBoost()
        {
            targetWidth = boostedWidth;
            SetGradient(speedBoostGradient);
        }

        /// <summary>
        /// Sets star power trail effect.
        /// </summary>
        public void SetStarPower()
        {
            targetWidth = boostedWidth * 1.5f;
            SetGradient(starPowerGradient);
        }

        /// <summary>
        /// Updates trail based on speed.
        /// </summary>
        public void UpdateSpeed(float speed)
        {
            if (!scaleWithSpeed || trailRenderer == null) return;

            float normalizedSpeed = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
            targetWidth = Mathf.Lerp(minWidth, maxWidth, normalizedSpeed);
        }

        /// <summary>
        /// Enables the trail effect.
        /// </summary>
        public void Enable()
        {
            isActive = true;
            if (trailRenderer != null)
            {
                trailRenderer.emitting = true;
            }
        }

        /// <summary>
        /// Disables the trail effect.
        /// </summary>
        public void Disable()
        {
            isActive = false;
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }
        }

        /// <summary>
        /// Clears the current trail.
        /// </summary>
        public void ClearTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        /// <summary>
        /// Sets a custom gradient.
        /// </summary>
        public void SetGradient(Gradient gradient)
        {
            if (trailRenderer != null && gradient != null)
            {
                trailRenderer.colorGradient = gradient;
            }
        }

        /// <summary>
        /// Sets trail width.
        /// </summary>
        public void SetWidth(float width)
        {
            targetWidth = width;
        }

        /// <summary>
        /// Sets trail time (length).
        /// </summary>
        public void SetTime(float time)
        {
            if (trailRenderer != null)
            {
                trailRenderer.time = time;
            }
        }

        #endregion
    }
}
