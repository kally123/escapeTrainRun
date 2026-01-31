using UnityEngine;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Handles player animation states and transitions.
    /// Uses Animator component for character animations.
    /// </summary>
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform characterTransform;

        [Header("Animation Settings")]
        [SerializeField] private float laneChangeTilt = 15f;
        [SerializeField] private float tiltSpeed = 10f;
        [SerializeField] private float jumpSquashStretch = 0.1f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem runDustParticles;
        [SerializeField] private ParticleSystem jumpParticles;
        [SerializeField] private ParticleSystem slideParticles;
        [SerializeField] private ParticleSystem crashParticles;
        [SerializeField] private ParticleSystem starPowerParticles;

        // Animation parameter hashes (for performance)
        private int hashIsRunning;
        private int hashIsJumping;
        private int hashIsSliding;
        private int hashCrashed;
        private int hashSpeed;
        private int hashLaneChange;

        // State
        private float currentTilt;
        private float targetTilt;
        private bool isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (characterTransform == null)
            {
                characterTransform = transform;
            }

            CacheAnimatorHashes();
            isInitialized = true;
        }

        private void CacheAnimatorHashes()
        {
            hashIsRunning = Animator.StringToHash(Constants.AnimRunning);
            hashIsJumping = Animator.StringToHash(Constants.AnimJumping);
            hashIsSliding = Animator.StringToHash(Constants.AnimSliding);
            hashCrashed = Animator.StringToHash(Constants.AnimCrashed);
            hashSpeed = Animator.StringToHash(Constants.AnimSpeed);
            hashLaneChange = Animator.StringToHash(Constants.AnimLaneChange);
        }

        private void Update()
        {
            UpdateTilt();
        }

        #region Tilt Animation (Lane Changes)

        private void UpdateTilt()
        {
            if (characterTransform == null) return;

            // Smoothly interpolate to target tilt
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

            // Apply tilt rotation
            Vector3 euler = characterTransform.localEulerAngles;
            euler.z = -currentTilt; // Negative for correct direction
            characterTransform.localEulerAngles = euler;

            // Reset tilt over time
            targetTilt = Mathf.Lerp(targetTilt, 0f, tiltSpeed * 0.5f * Time.deltaTime);
        }

        #endregion

        #region Animation Triggers

        /// <summary>
        /// Plays the run animation.
        /// </summary>
        public void PlayRun()
        {
            if (!isInitialized) return;

            SetBool(hashIsRunning, true);
            SetBool(hashIsJumping, false);
            SetBool(hashIsSliding, false);
            SetBool(hashCrashed, false);

            PlayParticles(runDustParticles, true);
        }

        /// <summary>
        /// Plays the jump animation.
        /// </summary>
        public void PlayJump()
        {
            if (!isInitialized) return;

            SetBool(hashIsJumping, true);
            SetBool(hashIsSliding, false);

            PlayParticles(runDustParticles, false);
            PlayParticlesOnce(jumpParticles);

            // Squash and stretch effect
            StartCoroutine(JumpSquashStretch());
        }

        /// <summary>
        /// Plays the slide animation.
        /// </summary>
        public void PlaySlide()
        {
            if (!isInitialized) return;

            SetBool(hashIsSliding, true);
            SetBool(hashIsJumping, false);

            PlayParticles(runDustParticles, false);
            PlayParticles(slideParticles, true);
        }

        /// <summary>
        /// Plays the lane change animation with tilt.
        /// </summary>
        /// <param name="direction">-1 for left, 1 for right.</param>
        public void PlayLaneChange(int direction)
        {
            if (!isInitialized) return;

            targetTilt = direction * laneChangeTilt;

            if (animator != null)
            {
                animator.SetInteger(hashLaneChange, direction);
            }
        }

        /// <summary>
        /// Plays the crash animation.
        /// </summary>
        public void PlayCrash()
        {
            if (!isInitialized) return;

            SetBool(hashCrashed, true);
            SetBool(hashIsRunning, false);
            SetBool(hashIsJumping, false);
            SetBool(hashIsSliding, false);

            StopAllParticles();
            PlayParticlesOnce(crashParticles);

            // Stumble effect
            StartCoroutine(CrashEffect());
        }

        /// <summary>
        /// Plays the star power animation (flying/invincible).
        /// </summary>
        public void PlayStarPower()
        {
            if (!isInitialized) return;

            PlayParticles(starPowerParticles, true);
        }

        /// <summary>
        /// Sets the animation speed multiplier.
        /// </summary>
        /// <param name="speedRatio">0-1 ratio of current speed to max speed.</param>
        public void SetSpeed(float speedRatio)
        {
            if (animator != null)
            {
                animator.SetFloat(hashSpeed, 0.5f + speedRatio * 0.5f);
            }
        }

        /// <summary>
        /// Resets all animations to idle state.
        /// </summary>
        public void ResetAnimations()
        {
            if (!isInitialized) return;

            SetBool(hashIsRunning, false);
            SetBool(hashIsJumping, false);
            SetBool(hashIsSliding, false);
            SetBool(hashCrashed, false);

            currentTilt = 0f;
            targetTilt = 0f;

            if (characterTransform != null)
            {
                characterTransform.localEulerAngles = Vector3.zero;
                characterTransform.localScale = Vector3.one;
            }

            StopAllParticles();
        }

        #endregion

        #region Particle Effects

        private void PlayParticles(ParticleSystem particles, bool play)
        {
            if (particles == null) return;

            if (play)
            {
                if (!particles.isPlaying)
                {
                    particles.Play();
                }
            }
            else
            {
                particles.Stop();
            }
        }

        private void PlayParticlesOnce(ParticleSystem particles)
        {
            if (particles == null) return;

            particles.Stop();
            particles.Play();
        }

        private void StopAllParticles()
        {
            PlayParticles(runDustParticles, false);
            PlayParticles(slideParticles, false);
            PlayParticles(starPowerParticles, false);
        }

        #endregion

        #region Animation Coroutines

        private System.Collections.IEnumerator JumpSquashStretch()
        {
            if (characterTransform == null) yield break;

            // Squash on takeoff
            Vector3 originalScale = Vector3.one;
            Vector3 squashScale = new Vector3(
                1f + jumpSquashStretch,
                1f - jumpSquashStretch,
                1f + jumpSquashStretch
            );

            float duration = 0.1f;
            float elapsed = 0f;

            // Squash
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                characterTransform.localScale = Vector3.Lerp(originalScale, squashScale, t);
                yield return null;
            }

            // Stretch
            Vector3 stretchScale = new Vector3(
                1f - jumpSquashStretch * 0.5f,
                1f + jumpSquashStretch,
                1f - jumpSquashStretch * 0.5f
            );

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                characterTransform.localScale = Vector3.Lerp(squashScale, stretchScale, t);
                yield return null;
            }

            // Return to normal
            elapsed = 0f;
            while (elapsed < duration * 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 2f);
                characterTransform.localScale = Vector3.Lerp(stretchScale, originalScale, t);
                yield return null;
            }

            characterTransform.localScale = originalScale;
        }

        private System.Collections.IEnumerator CrashEffect()
        {
            if (characterTransform == null) yield break;

            // Tumble forward
            Vector3 originalRotation = characterTransform.localEulerAngles;
            float tumbleAngle = 30f;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = MathHelpers.EaseOutQuad(elapsed / duration);
                
                Vector3 euler = originalRotation;
                euler.x = t * tumbleAngle;
                characterTransform.localEulerAngles = euler;
                
                yield return null;
            }
        }

        #endregion

        #region Helper Methods

        private void SetBool(int hash, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(hash, value);
            }
        }

        /// <summary>
        /// Sets the animator reference (for character swapping).
        /// </summary>
        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
            if (animator != null)
            {
                CacheAnimatorHashes();
            }
        }

        /// <summary>
        /// Gets the current animator.
        /// </summary>
        public Animator GetAnimator()
        {
            return animator;
        }

        #endregion
    }
}
