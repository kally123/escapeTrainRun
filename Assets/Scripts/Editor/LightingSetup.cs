using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to setup scene lighting.
/// Run from menu: Tools > Escape Train Run > Setup Lighting
/// </summary>
public class LightingSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup Lighting")]
    public static void SetupSceneLighting()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("💡 SETTING UP SCENE LIGHTING");
        Debug.Log("═══════════════════════════════════════════════════════════");

        SetupDirectionalLight();
        SetupAmbientLighting();
        SetupFog();

        Debug.Log("\n✅ Lighting setup complete!");
        EditorUtility.DisplayDialog("Lighting Setup", 
            "Scene lighting has been configured for the game.", "OK");
    }

    private static void SetupDirectionalLight()
    {
        Debug.Log("\n☀️ Setting up Directional Light...");

        // Find or create main directional light
        Light mainLight = null;
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.type == LightType.Directional)
            {
                mainLight = light;
                break;
            }
        }

        if (mainLight == null)
        {
            var lightObj = new GameObject("Directional Light");
            mainLight = lightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
        }

        mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        mainLight.color = new Color(1f, 0.96f, 0.9f);
        mainLight.intensity = 1.2f;
        mainLight.shadows = LightShadows.Soft;
        mainLight.shadowStrength = 0.6f;

        Debug.Log($"  ✅ Directional light configured");
    }

    private static void SetupAmbientLighting()
    {
        Debug.Log("\n🌤️ Setting up Ambient Lighting...");

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.55f);
        RenderSettings.ambientIntensity = 1f;

        // Skybox
        RenderSettings.skybox = null; // Use solid color for mobile performance

        Debug.Log($"  ✅ Ambient lighting configured");
    }

    private static void SetupFog()
    {
        Debug.Log("\n🌫️ Setting up Fog...");

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.75f, 0.8f, 0.9f);
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 200f;

        Debug.Log($"  ✅ Fog configured (fade distance: 50-200m)");
    }
}
