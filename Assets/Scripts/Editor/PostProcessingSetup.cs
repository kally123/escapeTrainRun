using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to setup post-processing effects.
/// Run from menu: Tools > Escape Train Run > Setup Post Processing
/// </summary>
public class PostProcessingSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup Post Processing")]
    public static void SetupPostProcessing()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("🎬 SETTING UP POST PROCESSING");
        Debug.Log("═══════════════════════════════════════════════════════════");

        CheckURPPostProcessing();
        CreateVolumeProfile();

        Debug.Log("\n✅ Post-processing setup complete!");
        EditorUtility.DisplayDialog("Post Processing Setup", 
            "Post-processing has been configured.\n\nNote: Make sure URP renderer has post-processing enabled.", "OK");
    }

    private static void CheckURPPostProcessing()
    {
        Debug.Log("\n📋 Checking URP Post Processing...");
        Debug.Log("  ℹ️ Make sure your URP Renderer Asset has 'Post-processing' checkbox enabled");
        Debug.Log("  ℹ️ Camera should have 'Post Processing' enabled in its settings");
    }

    private static void CreateVolumeProfile()
    {
        Debug.Log("\n📦 Creating Global Volume...");

        // Find or create global volume
        var existingVolume = Object.FindFirstObjectByType<UnityEngine.Rendering.Volume>();
        
        if (existingVolume != null)
        {
            Debug.Log("  ⏭️ Global Volume already exists");
            return;
        }

        var volumeObj = new GameObject("Global Volume");
        var volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;

        // Create volume profile
        EnsureDirectory("Assets/Settings");
        
        var profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
        AssetDatabase.CreateAsset(profile, "Assets/Settings/GlobalVolumeProfile.asset");
        
        volume.profile = profile;

        Debug.Log("  ✅ Created Global Volume");
        Debug.Log("  ℹ️ Add effects like Bloom, Vignette, Color Adjustments through the Volume component");
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
