using UnityEngine;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Visual effect for coin collection with animation and particles.
    /// </summary>
    public class CoinCollectEffect : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float scaleDuration = 0.15f;
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private float maxScale = 1.5f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Movement")]
        [SerializeField] private bool moveToUI = true;
        [SerializeField] private Vector3 uiTargetOffset = new Vector3(0f, 5f, 0f);
        [SerializeField] private float arcHeight = 2f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem collectParticles;
        [SerializeField] private int particleCount = 10;

        [Header("Trail")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private Gradient trailGradient;

        // State
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float animationTime;
        private bool isAnimating;

        /// <summary>
        /// Plays the collection effect.
        /// </summary>
        public void Play(Vector3 position, Transform uiTarget = null)
        {
            transform.position = position;
            startPosition = position;

            if (uiTarget != null && moveToUI)
            {
                targetPosition = uiTarget.position;
            }
            else
            {
                targetPosition = position + uiTargetOffset;
            }

            // Emit particles
            if (collectParticles != null)
            {
                collectParticles.transform.position = position;
                collectParticles.Emit(particleCount);
            }

            // Start animation
            animationTime = 0f;
            isAnimating = true;
            gameObject.SetActive(true);

            // Enable trail
            if (trail != null)
            {
                trail.Clear();
                trail.emitting = true;
            }
        }

        private void Update()
        {
            if (!isAnimating) return;

            animationTime += Time.deltaTime;

            // Scale animation phase
            if (animationTime < scaleDuration)
            {
                float t = animationTime / scaleDuration;
                float scale = scaleCurve.Evaluate(t) * maxScale;
                transform.localScale = Vector3.one * (1f + scale * 0.5f);
            }
            // Move animation phase
            else if (animationTime < scaleDuration + moveDuration)
            {
                float t = (animationTime - scaleDuration) / moveDuration;
                float curveT = moveCurve.Evaluate(t);

                // Arc movement
                Vector3 linearPos = Vector3.Lerp(startPosition, targetPosition, curveT);
                float arc = Mathf.Sin(curveT * Mathf.PI) * arcHeight;
                transform.position = linearPos + Vector3.up * arc;

                // Scale down
                transform.localScale = Vector3.Lerp(Vector3.one * maxScale, Vector3.zero, t);
            }
            else
            {
                // Animation complete
                isAnimating = false;

                if (trail != null)
                {
                    trail.emitting = false;
                }

                // Return to pool or destroy
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the coin color for variety.
        /// </summary>
        public void SetColor(Color color)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = color;
            }

            if (collectParticles != null)
            {
                var main = collectParticles.main;
                main.startColor = color;
            }
        }

        private void OnDisable()
        {
            isAnimating = false;
            transform.localScale = Vector3.one;
        }
    }
}
