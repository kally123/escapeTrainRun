using UnityEngine;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Provides control methods for particle systems.
    /// Attach to objects with ParticleSystem components.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private bool autoDestroy = false;
        [SerializeField] private float autoDestroyDelay = 0f;

        [Header("Scaling")]
        [SerializeField] private bool scaleWithSpeed;
        [SerializeField] private float minEmission = 10f;
        [SerializeField] private float maxEmission = 100f;
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float maxSpeed = 25f;

        // Components
        private ParticleSystem ps;
        private ParticleSystem.EmissionModule emission;
        private ParticleSystem.MainModule mainModule;

        // State
        private float baseEmissionRate;
        private float baseStartSpeed;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            emission = ps.emission;
            mainModule = ps.main;

            baseEmissionRate = emission.rateOverTime.constant;
            baseStartSpeed = mainModule.startSpeed.constant;
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }

            if (autoDestroy)
            {
                float duration = mainModule.duration + mainModule.startLifetime.constantMax + autoDestroyDelay;
                Destroy(gameObject, duration);
            }
        }

        #region Public API

        /// <summary>
        /// Plays the particle system.
        /// </summary>
        public void Play()
        {
            if (ps != null)
            {
                ps.Play();
            }
        }

        /// <summary>
        /// Stops the particle system.
        /// </summary>
        public void Stop()
        {
            if (ps != null)
            {
                ps.Stop();
            }
        }

        /// <summary>
        /// Pauses the particle system.
        /// </summary>
        public void Pause()
        {
            if (ps != null)
            {
                ps.Pause();
            }
        }

        /// <summary>
        /// Clears all particles.
        /// </summary>
        public void Clear()
        {
            if (ps != null)
            {
                ps.Clear();
            }
        }

        /// <summary>
        /// Emits a burst of particles.
        /// </summary>
        public void Emit(int count)
        {
            if (ps != null)
            {
                ps.Emit(count);
            }
        }

        /// <summary>
        /// Sets the emission rate.
        /// </summary>
        public void SetEmissionRate(float rate)
        {
            if (ps != null)
            {
                var em = ps.emission;
                em.rateOverTime = rate;
            }
        }

        /// <summary>
        /// Sets the start color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }
        }

        /// <summary>
        /// Sets the start size.
        /// </summary>
        public void SetSize(float size)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startSize = size;
            }
        }

        /// <summary>
        /// Sets the start speed.
        /// </summary>
        public void SetSpeed(float speed)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startSpeed = speed;
            }
        }

        /// <summary>
        /// Sets the simulation speed.
        /// </summary>
        public void SetSimulationSpeed(float speed)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpeed = speed;
            }
        }

        /// <summary>
        /// Updates particle system based on player speed.
        /// </summary>
        public void UpdateWithSpeed(float playerSpeed)
        {
            if (!scaleWithSpeed || ps == null) return;

            float normalizedSpeed = Mathf.InverseLerp(minSpeed, maxSpeed, playerSpeed);

            // Scale emission rate
            float newEmission = Mathf.Lerp(minEmission, maxEmission, normalizedSpeed);
            var em = ps.emission;
            em.rateOverTime = newEmission;

            // Scale particle speed
            var main = ps.main;
            main.startSpeed = baseStartSpeed * (1f + normalizedSpeed);
        }

        /// <summary>
        /// Enables or disables emission.
        /// </summary>
        public void SetEmitting(bool emitting)
        {
            if (ps != null)
            {
                var em = ps.emission;
                em.enabled = emitting;
            }
        }

        /// <summary>
        /// Checks if the particle system is playing.
        /// </summary>
        public bool IsPlaying()
        {
            return ps != null && ps.isPlaying;
        }

        /// <summary>
        /// Gets the current particle count.
        /// </summary>
        public int GetParticleCount()
        {
            return ps != null ? ps.particleCount : 0;
        }

        #endregion

        #region Presets

        /// <summary>
        /// Applies a burst preset.
        /// </summary>
        public void ApplyBurstPreset(int count, float speed, Color color)
        {
            if (ps == null) return;

            var main = ps.main;
            main.startSpeed = speed;
            main.startColor = color;

            ps.Emit(count);
        }

        /// <summary>
        /// Applies a continuous preset.
        /// </summary>
        public void ApplyContinuousPreset(float rate, float speed, Color color)
        {
            if (ps == null) return;

            var main = ps.main;
            main.startSpeed = speed;
            main.startColor = color;

            var em = ps.emission;
            em.rateOverTime = rate;

            Play();
        }

        #endregion
    }
}
