using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Manages environment visuals, lighting, and atmosphere for each theme.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance { get; private set; }

        [Header("Skyboxes")]
        [SerializeField] private Material trainSkybox;
        [SerializeField] private Material busSkybox;
        [SerializeField] private Material groundSkybox;

        [Header("Directional Light")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Color trainLightColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] private Color busLightColor = new Color(1f, 1f, 1f);
        [SerializeField] private Color groundLightColor = new Color(1f, 0.98f, 0.9f);

        [Header("Fog Settings")]
        [SerializeField] private bool useFog = true;
        [SerializeField] private Color trainFogColor = new Color(0.3f, 0.3f, 0.35f);
        [SerializeField] private Color busFogColor = new Color(0.5f, 0.5f, 0.55f);
        [SerializeField] private Color groundFogColor = new Color(0.6f, 0.7f, 0.6f);
        [SerializeField] private float fogDensity = 0.02f;

        [Header("Ambient Settings")]
        [SerializeField] private Color trainAmbient = new Color(0.4f, 0.35f, 0.3f);
        [SerializeField] private Color busAmbient = new Color(0.45f, 0.45f, 0.5f);
        [SerializeField] private Color groundAmbient = new Color(0.5f, 0.55f, 0.5f);

        [Header("Particles")]
        [SerializeField] private ParticleSystem trainParticles; // Dust/steam
        [SerializeField] private ParticleSystem busParticles;   // City dust
        [SerializeField] private ParticleSystem groundParticles; // Leaves/birds

        // Current state
        private ThemeType currentTheme;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (directionalLight == null)
            {
                directionalLight = FindObjectOfType<Light>();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnThemeSelected += OnThemeSelected;
        }

        private void OnDisable()
        {
            GameEvents.OnThemeSelected -= OnThemeSelected;
        }

        private void Start()
        {
            // Apply default theme
            ApplyTheme(ThemeType.Train);
        }

        private void OnThemeSelected(ThemeType theme)
        {
            ApplyTheme(theme);
        }

        /// <summary>
        /// Applies all environment settings for a theme.
        /// </summary>
        public void ApplyTheme(ThemeType theme)
        {
            currentTheme = theme;

            ApplySkybox(theme);
            ApplyLighting(theme);
            ApplyFog(theme);
            ApplyAmbient(theme);
            ApplyParticles(theme);

            Debug.Log($"[EnvironmentManager] Theme applied: {theme}");
        }

        #region Theme Application

        private void ApplySkybox(ThemeType theme)
        {
            Material skybox = theme switch
            {
                ThemeType.Train => trainSkybox,
                ThemeType.Bus => busSkybox,
                ThemeType.Ground => groundSkybox,
                _ => trainSkybox
            };

            if (skybox != null)
            {
                RenderSettings.skybox = skybox;
            }
        }

        private void ApplyLighting(ThemeType theme)
        {
            if (directionalLight == null) return;

            Color lightColor = theme switch
            {
                ThemeType.Train => trainLightColor,
                ThemeType.Bus => busLightColor,
                ThemeType.Ground => groundLightColor,
                _ => Color.white
            };

            directionalLight.color = lightColor;

            // Adjust intensity and angle based on theme
            switch (theme)
            {
                case ThemeType.Train:
                    directionalLight.intensity = 0.8f;
                    directionalLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
                    break;
                case ThemeType.Bus:
                    directionalLight.intensity = 1f;
                    directionalLight.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
                    break;
                case ThemeType.Ground:
                    directionalLight.intensity = 1.2f;
                    directionalLight.transform.rotation = Quaternion.Euler(60f, 30f, 0f);
                    break;
            }
        }

        private void ApplyFog(ThemeType theme)
        {
            RenderSettings.fog = useFog;

            if (!useFog) return;

            Color fogColor = theme switch
            {
                ThemeType.Train => trainFogColor,
                ThemeType.Bus => busFogColor,
                ThemeType.Ground => groundFogColor,
                _ => Color.gray
            };

            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = fogDensity;

            // Adjust fog density by theme
            RenderSettings.fogDensity = theme switch
            {
                ThemeType.Train => fogDensity * 1.5f,
                ThemeType.Bus => fogDensity,
                ThemeType.Ground => fogDensity * 0.5f,
                _ => fogDensity
            };
        }

        private void ApplyAmbient(ThemeType theme)
        {
            Color ambient = theme switch
            {
                ThemeType.Train => trainAmbient,
                ThemeType.Bus => busAmbient,
                ThemeType.Ground => groundAmbient,
                _ => Color.gray
            };

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
        }

        private void ApplyParticles(ThemeType theme)
        {
            // Stop all particles first
            if (trainParticles != null) trainParticles.Stop();
            if (busParticles != null) busParticles.Stop();
            if (groundParticles != null) groundParticles.Stop();

            // Start appropriate particles
            switch (theme)
            {
                case ThemeType.Train:
                    if (trainParticles != null) trainParticles.Play();
                    break;
                case ThemeType.Bus:
                    if (busParticles != null) busParticles.Play();
                    break;
                case ThemeType.Ground:
                    if (groundParticles != null) groundParticles.Play();
                    break;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the current theme.
        /// </summary>
        public ThemeType GetCurrentTheme()
        {
            return currentTheme;
        }

        /// <summary>
        /// Temporarily changes lighting (for power-up effects).
        /// </summary>
        public void SetTemporaryLighting(Color color, float intensity, float duration)
        {
            StartCoroutine(TemporaryLightingCoroutine(color, intensity, duration));
        }

        private System.Collections.IEnumerator TemporaryLightingCoroutine(Color color, float intensity, float duration)
        {
            if (directionalLight == null) yield break;

            Color originalColor = directionalLight.color;
            float originalIntensity = directionalLight.intensity;

            directionalLight.color = color;
            directionalLight.intensity = intensity;

            yield return new WaitForSeconds(duration);

            // Restore original lighting
            ApplyLighting(currentTheme);
        }

        /// <summary>
        /// Triggers a flash effect (for power-up activation).
        /// </summary>
        public void TriggerFlash(Color color, float duration = 0.2f)
        {
            StartCoroutine(FlashCoroutine(color, duration));
        }

        private System.Collections.IEnumerator FlashCoroutine(Color color, float duration)
        {
            // Could flash post-processing or UI overlay
            yield return new WaitForSeconds(duration);
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
