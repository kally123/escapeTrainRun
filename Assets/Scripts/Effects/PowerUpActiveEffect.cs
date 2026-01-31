using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Visual effect displayed while a power-up is active on the player.
    /// </summary>
    public class PowerUpActiveEffect : MonoBehaviour
    {
        [Header("Power-Up Type")]
        [SerializeField] private PowerUpType powerUpType;

        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem mainParticles;
        [SerializeField] private ParticleSystem trailParticles;
        [SerializeField] private ParticleSystem auraParticles;

        [Header("Light")]
        [SerializeField] private Light effectLight;
        [SerializeField] private float lightIntensity = 2f;
        [SerializeField] private float lightPulseSpeed = 2f;

        [Header("Rotation")]
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Header("Scale Pulse")]
        [SerializeField] private bool scalePulse = true;
        [SerializeField] private float pulseAmount = 0.1f;
        [SerializeField] private float pulseSpeed = 3f;

        [Header("Warning")]
        [SerializeField] private bool enableWarning = true;
        [SerializeField] private float warningTime = 3f;
        [SerializeField] private float warningFlashSpeed = 5f;
        [SerializeField] private Color warningColor = Color.red;

        // State
        private float duration;
        private float elapsedTime;
        private bool isWarning;
        private Vector3 baseScale;
        private Color baseColor;
        private Color baseLightColor;

        private void Awake()
        {
            baseScale = transform.localScale;

            if (effectLight != null)
            {
                baseLightColor = effectLight.color;
            }

            if (mainParticles != null)
            {
                var main = mainParticles.main;
                baseColor = main.startColor.color;
            }
        }

        /// <summary>
        /// Initializes and starts the effect.
        /// </summary>
        public void Initialize(PowerUpType type, float powerUpDuration)
        {
            powerUpType = type;
            duration = powerUpDuration;
            elapsedTime = 0f;
            isWarning = false;

            // Configure based on power-up type
            ConfigureForPowerUp(type);

            // Start particles
            PlayAllParticles();

            // Enable light
            if (effectLight != null)
            {
                effectLight.enabled = true;
            }
        }

        private void ConfigureForPowerUp(PowerUpType type)
        {
            Color color = type switch
            {
                PowerUpType.Magnet => Color.magenta,
                PowerUpType.Shield => Color.cyan,
                PowerUpType.SpeedBoost => Color.blue,
                PowerUpType.StarPower => Color.yellow,
                PowerUpType.Multiplier => Color.green,
                _ => Color.white
            };

            SetColor(color);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            // Check for warning phase
            if (enableWarning && !isWarning && duration - elapsedTime <= warningTime)
            {
                StartWarning();
            }

            // Rotation
            if (rotate)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            }

            // Scale pulse
            if (scalePulse)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                transform.localScale = baseScale * pulse;
            }

            // Light pulse
            if (effectLight != null)
            {
                float lightPulse = 0.5f + Mathf.Sin(Time.time * lightPulseSpeed) * 0.5f;
                effectLight.intensity = lightIntensity * lightPulse;

                if (isWarning)
                {
                    // Flash between base color and warning color
                    float flash = Mathf.Sin(Time.time * warningFlashSpeed);
                    effectLight.color = Color.Lerp(baseLightColor, warningColor, (flash + 1f) / 2f);
                }
            }

            // Warning flash for particles
            if (isWarning && mainParticles != null)
            {
                float flash = (Mathf.Sin(Time.time * warningFlashSpeed) + 1f) / 2f;
                var main = mainParticles.main;
                main.startColor = Color.Lerp(baseColor, warningColor, flash);
            }
        }

        private void StartWarning()
        {
            isWarning = true;

            // Could trigger audio warning here
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlayPowerUpWarning();
            }
        }

        private void PlayAllParticles()
        {
            if (mainParticles != null) mainParticles.Play();
            if (trailParticles != null) trailParticles.Play();
            if (auraParticles != null) auraParticles.Play();
        }

        private void StopAllParticles()
        {
            if (mainParticles != null) mainParticles.Stop();
            if (trailParticles != null) trailParticles.Stop();
            if (auraParticles != null) auraParticles.Stop();
        }

        /// <summary>
        /// Sets the effect color.
        /// </summary>
        public void SetColor(Color color)
        {
            baseColor = color;

            if (mainParticles != null)
            {
                var main = mainParticles.main;
                main.startColor = color;
            }

            if (trailParticles != null)
            {
                var main = trailParticles.main;
                main.startColor = color;
            }

            if (auraParticles != null)
            {
                var main = auraParticles.main;
                Color auraColor = color;
                auraColor.a = 0.3f;
                main.startColor = auraColor;
            }

            if (effectLight != null)
            {
                effectLight.color = color;
                baseLightColor = color;
            }
        }

        /// <summary>
        /// Stops the effect.
        /// </summary>
        public void Stop()
        {
            StopAllParticles();

            if (effectLight != null)
            {
                effectLight.enabled = false;
            }

            // Could play end effect here
        }

        private void OnDisable()
        {
            Stop();
            transform.localScale = baseScale;
        }
    }
}
