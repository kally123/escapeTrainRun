using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

/// <summary>
/// Editor script to set up lighting and post-processing for the game.
/// Run from menu: Tools > Escape Train Run > Setup Lighting
/// </summary>
public class LightingSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup Lighting/Setup All Lighting")]
    public static void SetupAllLighting()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("💡 SETTING UP LIGHTING");
        Debug.Log("═══════════════════════════════════════════════════════════");

        SetupDirectionalLight();
        SetupAmbientLighting();
        SetupFog();
        SetupReflectionProbe();

        Debug.Log("✅ Lighting setup complete!");
        EditorUtility.DisplayDialog("Lighting Setup", 
            "Scene lighting has been configured.\n\n" +
            "• Directional Light (Sun)\n" +
            "• Ambient Lighting\n" +
            "• Distance Fog\n" +
            "• Reflection Probe", "OK");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Create Directional Light")]
    public static void SetupDirectionalLight()
    {
        Debug.Log("\n☀️ Setting up Directional Light...");

        // Find or create directional light
        Light sunLight = null;
        var lights = Object.FindObjectsOfType<Light>();
        
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                sunLight = light;
                break;
            }
        }

        if (sunLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            sunLight = lightObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            Debug.Log("  Created new Directional Light");
        }
        else
        {
            Debug.Log("  Using existing Directional Light");
        }

        // Configure light settings
        sunLight.gameObject.name = "Directional Light (Sun)";
        sunLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        // Warm sunlight color
        sunLight.color = new Color(1f, 0.96f, 0.88f);
        sunLight.intensity = 1.2f;
        
        // Shadow settings
        sunLight.shadows = LightShadows.Soft;
        sunLight.shadowStrength = 0.7f;
        sunLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        sunLight.shadowBias = 0.05f;
        sunLight.shadowNormalBias = 0.4f;
        sunLight.shadowNearPlane = 0.2f;

        // Culling mask - render everything
        sunLight.cullingMask = -1;

        Debug.Log("  ✅ Directional Light configured");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Configure Ambient")]
    public static void SetupAmbientLighting()
    {
        Debug.Log("\n🌤️ Setting up Ambient Lighting...");

        // Use gradient ambient
        RenderSettings.ambientMode = AmbientMode.Trilight;
        
        // Sky color (top)
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
        
        // Equator color (horizon)
        RenderSettings.ambientEquatorColor = new Color(0.8f, 0.85f, 0.9f);
        
        // Ground color (bottom)
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.3f, 0.35f);

        // Ambient intensity
        RenderSettings.ambientIntensity = 1.0f;

        // Reflection settings
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        RenderSettings.reflectionIntensity = 0.5f;

        Debug.Log("  ✅ Ambient lighting configured");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Configure Fog")]
    public static void SetupFog()
    {
        Debug.Log("\n🌫️ Setting up Distance Fog...");

        // Enable fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        
        // Fog color - slightly blue atmospheric
        RenderSettings.fogColor = new Color(0.7f, 0.8f, 0.9f);
        
        // Fog distances - start far, end at horizon
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 200f;

        Debug.Log("  ✅ Fog configured (Linear, 50-200m)");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Add Reflection Probe")]
    public static void SetupReflectionProbe()
    {
        Debug.Log("\n🔮 Setting up Reflection Probe...");

        // Check if one already exists
        var existingProbe = Object.FindObjectOfType<ReflectionProbe>();
        if (existingProbe != null)
        {
            Debug.Log("  ⏭️ Reflection Probe already exists");
            return;
        }

        GameObject probeObj = new GameObject("Reflection Probe");
        var probe = probeObj.AddComponent<ReflectionProbe>();
        
        // Position above player area
        probeObj.transform.position = new Vector3(0, 5, 20);
        
        // Configure probe
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
        probe.resolution = 128;
        probe.size = new Vector3(100, 50, 200);
        probe.boxProjection = true;
        probe.importance = 1;

        Debug.Log("  ✅ Reflection Probe created");
    }

    #region Theme Presets

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Presets/Day - Clear")]
    public static void PresetDayClear()
    {
        SetupDirectionalLight();
        
        var sun = Object.FindObjectOfType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun.color = new Color(1f, 0.98f, 0.9f);
            sun.intensity = 1.3f;
        }

        RenderSettings.ambientSkyColor = new Color(0.53f, 0.81f, 0.98f);
        RenderSettings.ambientEquatorColor = new Color(0.9f, 0.9f, 0.95f);
        RenderSettings.fogColor = new Color(0.8f, 0.9f, 1f);

        Debug.Log("☀️ Applied Day - Clear preset");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Presets/Sunset")]
    public static void PresetSunset()
    {
        SetupDirectionalLight();
        
        var sun = Object.FindObjectOfType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(15f, -60f, 0f);
            sun.color = new Color(1f, 0.6f, 0.3f);
            sun.intensity = 1.0f;
        }

        RenderSettings.ambientSkyColor = new Color(0.4f, 0.3f, 0.5f);
        RenderSettings.ambientEquatorColor = new Color(1f, 0.6f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.15f, 0.2f);
        RenderSettings.fogColor = new Color(1f, 0.7f, 0.5f);

        Debug.Log("🌅 Applied Sunset preset");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Presets/Night")]
    public static void PresetNight()
    {
        SetupDirectionalLight();
        
        var sun = Object.FindObjectOfType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(30f, -45f, 0f);
            sun.color = new Color(0.5f, 0.6f, 0.9f);
            sun.intensity = 0.3f;
        }

        RenderSettings.ambientSkyColor = new Color(0.1f, 0.1f, 0.2f);
        RenderSettings.ambientEquatorColor = new Color(0.15f, 0.15f, 0.25f);
        RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.2f);

        Debug.Log("🌙 Applied Night preset");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Presets/Space")]
    public static void PresetSpace()
    {
        SetupDirectionalLight();
        
        var sun = Object.FindObjectOfType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            sun.color = new Color(0.9f, 0.95f, 1f);
            sun.intensity = 1.5f;
            sun.shadows = LightShadows.Hard;
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.fog = false;

        Debug.Log("🚀 Applied Space preset");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Presets/Beach")]
    public static void PresetBeach()
    {
        SetupDirectionalLight();
        
        var sun = Object.FindObjectOfType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(60f, -20f, 0f);
            sun.color = new Color(1f, 0.98f, 0.85f);
            sun.intensity = 1.4f;
        }

        RenderSettings.ambientSkyColor = new Color(0.4f, 0.75f, 0.95f);
        RenderSettings.ambientEquatorColor = new Color(0.9f, 0.95f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.9f, 0.85f, 0.7f);
        RenderSettings.fogColor = new Color(0.8f, 0.9f, 0.95f);
        RenderSettings.fogStartDistance = 80f;
        RenderSettings.fogEndDistance = 300f;

        Debug.Log("🏖️ Applied Beach preset");
    }

    [MenuItem("Tools/Escape Train Run/Setup Lighting/Presets/Forest")]
    public static void PresetForest()
    {
        SetupDirectionalLight();
        
        var sun = Object.FindObjectOfType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(55f, -40f, 0f);
            sun.color = new Color(1f, 0.95f, 0.8f);
            sun.intensity = 0.9f;
        }

        RenderSettings.ambientSkyColor = new Color(0.3f, 0.5f, 0.3f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.6f, 0.3f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.25f, 0.15f);
        RenderSettings.fogColor = new Color(0.5f, 0.6f, 0.5f);
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 150f;

        Debug.Log("🌲 Applied Forest preset");
    }

    #endregion
}
