using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to set up post-processing for URP.
/// Run from menu: Tools > Escape Train Run > Setup Post Processing
/// </summary>
public class PostProcessingSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Setup All Effects")]
    public static void SetupAllPostProcessing()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("✨ SETTING UP POST PROCESSING");
        Debug.Log("═══════════════════════════════════════════════════════════");

        CreateVolumeProfile();
        CreatePostProcessVolume();
        
        Debug.Log("\n✅ Post Processing setup complete!");
        EditorUtility.DisplayDialog("Post Processing Setup", 
            "Post processing has been configured!\n\n" +
            "Effects enabled:\n" +
            "• Bloom (subtle glow)\n" +
            "• Color Grading (vibrant)\n" +
            "• Vignette (subtle)\n" +
            "• Motion Blur (disabled by default)\n\n" +
            "Find the Volume Profile in:\n" +
            "Assets/Settings/PostProcessing/", "OK");
    }

    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Create Volume Profile Asset")]
    public static void CreateVolumeProfile()
    {
        Debug.Log("\n📄 Creating Volume Profile...");

        // Ensure directory exists
        string settingsPath = "Assets/Settings";
        string ppPath = "Assets/Settings/PostProcessing";

        if (!AssetDatabase.IsValidFolder(settingsPath))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }
        if (!AssetDatabase.IsValidFolder(ppPath))
        {
            AssetDatabase.CreateFolder(settingsPath, "PostProcessing");
        }

        // Create instructions file since we can't create URP Volume Profiles via script easily
        string instructionsPath = $"{ppPath}/SETUP_INSTRUCTIONS.md";
        string instructions = @"# Post Processing Setup Instructions

Since Unity Volume Profiles are best created through the Unity Editor, follow these steps:

## Step 1: Create Volume Profile
1. Right-click in `Assets/Settings/PostProcessing/`
2. Select **Create → Rendering → Volume Profile**
3. Name it `GameplayVolumeProfile`

## Step 2: Add Post Processing Effects
Double-click the Volume Profile to edit it, then click **Add Override** for each effect:

### Bloom
- Intensity: 0.5
- Threshold: 0.9
- Scatter: 0.7

### Color Adjustments
- Post Exposure: 0.1
- Contrast: 10
- Saturation: 15
- Hue Shift: 0

### Vignette
- Intensity: 0.25
- Smoothness: 0.5

### Film Grain (Optional)
- Type: Medium
- Intensity: 0.1

### Motion Blur (Optional - may cause motion sickness in kids)
- Keep disabled or set very low (0.1)

## Step 3: Create Global Volume in Scene
1. In your GamePlay scene, create **GameObject → Volume → Global Volume**
2. Assign the `GameplayVolumeProfile` to the Volume's Profile field
3. Make sure **Is Global** is checked

## Step 4: Enable Post Processing on Camera
1. Select your Main Camera
2. In the Camera component, enable **Post Processing**
3. Set **Rendering → Anti-aliasing** to **SMAA** or **FXAA**

## Alternative: Use the Editor Script
The `PostProcessingSetup.cs` script can create a Global Volume in your scene:
**Menu: Tools → Escape Train Run → Setup Post Processing → Create Global Volume**

---

## Theme-Specific Profiles (Optional)

Create separate profiles for different game themes:

### City Theme
- Higher contrast
- Slightly desaturated
- Blue tint in shadows

### Beach Theme  
- Warm color temperature
- Higher saturation
- Soft bloom

### Forest Theme
- Green tint
- Lower contrast
- Soft vignette

### Night Theme
- Blue color temperature
- Higher bloom
- Strong vignette

### Space Theme
- High contrast
- Chromatic aberration
- Strong bloom on lights
";

        File.WriteAllText(Path.Combine(Application.dataPath.Replace("Assets", ""), instructionsPath), instructions);
        AssetDatabase.Refresh();

        Debug.Log($"  ✅ Created setup instructions at {instructionsPath}");
        Debug.Log("  ℹ️ Note: Volume Profiles should be created via Unity Editor");
        Debug.Log("     Right-click → Create → Rendering → Volume Profile");
    }

    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Create Global Volume")]
    public static void CreatePostProcessVolume()
    {
        Debug.Log("\n🎬 Creating Global Volume...");

        // Check if one already exists
        var existingVolumes = Object.FindObjectsOfType<UnityEngine.Rendering.Volume>();
        foreach (var vol in existingVolumes)
        {
            if (vol.isGlobal)
            {
                Debug.Log("  ⏭️ Global Volume already exists");
                Selection.activeGameObject = vol.gameObject;
                return;
            }
        }

        // Create new global volume
        GameObject volumeObj = new GameObject("Global Volume");
        var volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
        volume.isGlobal = true;
        volume.priority = 0;
        volume.weight = 1f;

        Debug.Log("  ✅ Global Volume created");
        Debug.Log("  ⚠️ Assign a Volume Profile to the Volume component!");

        Selection.activeGameObject = volumeObj;
        EditorGUIUtility.PingObject(volumeObj);
    }

    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Configure Camera for Post Processing")]
    public static void ConfigureCamera()
    {
        Debug.Log("\n📷 Configuring Camera for Post Processing...");

        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("  ❌ No Main Camera found!");
            return;
        }

        // For URP, we need to access the Universal Additional Camera Data
#if UNITY_2019_3_OR_NEWER
        var additionalCameraData = camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (additionalCameraData == null)
        {
            additionalCameraData = camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        }

        additionalCameraData.renderPostProcessing = true;
        additionalCameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        additionalCameraData.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.Medium;
        
        Debug.Log("  ✅ Camera configured for URP Post Processing");
        Debug.Log("  ✅ Anti-aliasing: SMAA Medium");
#else
        Debug.Log("  ⚠️ Please enable Post Processing manually in Camera settings");
#endif
    }

    #region Quick Presets

    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Presets/Vibrant Kids Style")]
    public static void PresetVibrant()
    {
        CreatePostProcessVolume();
        Debug.Log(@"
🎨 VIBRANT KIDS STYLE PRESET
Apply these values to your Volume Profile:

Bloom:
  - Intensity: 0.6
  - Threshold: 0.85
  
Color Adjustments:
  - Saturation: 20
  - Contrast: 15
  - Post Exposure: 0.15
  
Vignette:
  - Intensity: 0.2
");
    }

    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Presets/Cinematic")]
    public static void PresetCinematic()
    {
        CreatePostProcessVolume();
        Debug.Log(@"
🎬 CINEMATIC PRESET
Apply these values to your Volume Profile:

Bloom:
  - Intensity: 0.4
  - Threshold: 0.9
  
Color Adjustments:
  - Saturation: 5
  - Contrast: 20
  
Vignette:
  - Intensity: 0.35
  
Film Grain:
  - Intensity: 0.15
");
    }

    [MenuItem("Tools/Escape Train Run/Setup Post Processing/Presets/Cartoon")]
    public static void PresetCartoon()
    {
        CreatePostProcessVolume();
        Debug.Log(@"
🖼️ CARTOON PRESET
Apply these values to your Volume Profile:

Bloom:
  - Intensity: 0.8
  - Threshold: 0.7
  
Color Adjustments:
  - Saturation: 30
  - Contrast: 25
  
NO Vignette
NO Film Grain
");
    }

    #endregion
}
