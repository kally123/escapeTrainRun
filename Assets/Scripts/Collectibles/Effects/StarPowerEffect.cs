using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Core;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Star Power effect - player flies above obstacles, collecting everything.
    /// Ultimate power-up that makes player completely invincible and attracts all collectibles.
    /// </summary>
    public class StarPowerEffect : MonoBehaviour, IPowerUpEffect
    {
        [Header("Flight Settings")]
        [SerializeField] private float flyHeight = 3f;
        [SerializeField] private float transitionSpeed = 5f;
        [SerializeField] private float collectRange = 8f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem starParticles;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private ParticleSystem trailParticles;
        [SerializeField] private Light playerGlow;
        [SerializeField] private Color glowColor = Color.yellow;
        [SerializeField] private float glowIntensity = 3f;

        [Header("Audio")]
        [SerializeField] private AudioClip starPowerMusic;

        private PlayerController player;
        private CoinManager coinManager;
        private bool isActive;
        private float originalY;
        private float currentHeight;

        public bool IsActive => isActive;
        public float FlyHeight => flyHeight;

        public void Activate(PlayerController player)
        {
            this.player = player;
            isActive = true;

            if (player != null)
            {
                originalY = player.transform.position.y;
                player.SetInvincible(true);
            }

            // Get coin manager for mega-magnet effect
            coinManager = CoinManager.Instance;
            if (coinManager != null && player != null)
            {
                coinManager.EnableMagnet(player.transform, collectRange);
            }

            // Start visual effects
            StartVisualEffects();

            // Start special music
            PlayStarMusic();

            Debug.Log("[StarPowerEffect] Activated - SUPER STAR MODE!");
        }

        public void Deactivate()
        {
            isActive = false;

            if (player != null)
            {
                player.SetInvincible(false);
            }

            // Disable mega-magnet
            if (coinManager != null)
            {
                coinManager.DisableMagnet();
            }

            // Stop visual effects
            StopVisualEffects();

            // Stop special music
            StopStarMusic();

            Debug.Log("[StarPowerEffect] Deactivated - Returning to normal");
        }

        public void Update()
        {
            if (!isActive || player == null) return;

            // Smoothly transition to fly height
            UpdateFlyHeight();

            // Rotate/animate player during star power
            AnimatePlayer();
        }

        private void UpdateFlyHeight()
        {
            float targetHeight = originalY + flyHeight;
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, transitionSpeed * Time.deltaTime);

            // Note: Actual height change would need to be applied to player transform
            // This is handled by PlayerMovement or a special fly mode
        }

        private void AnimatePlayer()
        {
            // Could add spinning or floating animation
            // Player model could rotate or have special pose
        }

        private void StartVisualEffects()
        {
            if (starParticles != null)
            {
                starParticles.Play();
            }

            if (glowParticles != null)
            {
                glowParticles.Play();
            }

            if (trailParticles != null)
            {
                trailParticles.Play();
            }

            // Create/enable glow light
            if (playerGlow == null && player != null)
            {
                var lightObj = new GameObject("StarGlow");
                lightObj.transform.SetParent(player.transform);
                lightObj.transform.localPosition = Vector3.zero;
                playerGlow = lightObj.AddComponent<Light>();
                playerGlow.type = LightType.Point;
                playerGlow.range = 10f;
            }

            if (playerGlow != null)
            {
                playerGlow.enabled = true;
                playerGlow.color = glowColor;
                playerGlow.intensity = glowIntensity;
            }

            // Flash effect on environment
            if (EnvironmentManager.Instance != null)
            {
                EnvironmentManager.Instance.TriggerFlash(glowColor, 0.3f);
            }
        }

        private void StopVisualEffects()
        {
            if (starParticles != null)
            {
                starParticles.Stop();
            }

            if (glowParticles != null)
            {
                glowParticles.Stop();
            }

            if (trailParticles != null)
            {
                trailParticles.Stop();
            }

            if (playerGlow != null)
            {
                playerGlow.enabled = false;
            }
        }

        private void PlayStarMusic()
        {
            if (starPowerMusic != null && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                // Could play special star power music
                // audio.PlayMusic(starPowerMusic);
            }
        }

        private void StopStarMusic()
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                // Return to normal music
                // audio.PlayThemeMusic(currentTheme);
            }
        }
    }
}
