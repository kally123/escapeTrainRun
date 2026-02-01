using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create all game materials.
/// Run from menu: Tools > Escape Train Run > Create All Materials
/// </summary>
public class MaterialCreator : Editor
{
    private static string materialsPath = "Assets/Materials";

    [MenuItem("Tools/Escape Train Run/Create All Materials")]
    public static void CreateAllMaterials()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("🎨 CREATING ALL MATERIALS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        EnsureDirectories();
        CreateTrackMaterials();
        CreateObstacleMaterials();
        CreateCollectibleMaterials();
        CreateCharacterMaterials();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("\n✅ All materials created!");
        EditorUtility.DisplayDialog("Materials Created", 
            "All game materials have been created in Assets/Materials/", "OK");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            "Assets/Materials",
            "Assets/Materials/Track",
            "Assets/Materials/Obstacles",
            "Assets/Materials/Collectibles",
            "Assets/Materials/Characters"
        };

        foreach (var dir in dirs)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                var name = Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static void CreateTrackMaterials()
    {
        Debug.Log("\n🛤️ Creating Track Materials...");
        CreateMaterial("Track/Track_Floor", new Color(0.5f, 0.5f, 0.55f), 0f, 0.3f);
        CreateMaterial("Track/Track_Rail", new Color(0.3f, 0.3f, 0.35f), 0.7f, 0.6f);
    }

    private static void CreateObstacleMaterials()
    {
        Debug.Log("\n🚧 Creating Obstacle Materials...");
        CreateMaterial("Obstacles/Obstacle_Warning", new Color(1f, 0.8f, 0.2f), 0f, 0.5f);
        CreateMaterial("Obstacles/Obstacle_Danger", new Color(0.9f, 0.2f, 0.2f), 0f, 0.5f);
        CreateMaterial("Obstacles/Obstacle_Block", new Color(0.3f, 0.3f, 0.4f), 0.3f, 0.5f);
    }

    private static void CreateCollectibleMaterials()
    {
        Debug.Log("\n💰 Creating Collectible Materials...");
        CreateMaterial("Collectibles/Coin_Gold", new Color(1f, 0.84f, 0f), 1f, 0.8f);
        CreateMaterial("Collectibles/PowerUp_Blue", new Color(0.2f, 0.6f, 1f), 0.5f, 0.7f);
        CreateMaterial("Collectibles/PowerUp_Purple", new Color(0.7f, 0.2f, 1f), 0.5f, 0.7f);
        CreateMaterial("Collectibles/PowerUp_Orange", new Color(1f, 0.5f, 0.1f), 0.5f, 0.7f);
    }

    private static void CreateCharacterMaterials()
    {
        Debug.Log("\n🏃 Creating Character Materials...");
        CreateMaterial("Characters/Char_Default", new Color(0.2f, 0.5f, 0.9f), 0f, 0.5f);
        CreateMaterial("Characters/Char_Speed", new Color(0.9f, 0.4f, 0.1f), 0f, 0.5f);
        CreateMaterial("Characters/Char_Ninja", new Color(0.1f, 0.1f, 0.15f), 0f, 0.6f);
    }

    private static void CreateMaterial(string path, Color color, float metallic, float smoothness)
    {
        string fullPath = $"{materialsPath}/{path}.mat";
        
        if (AssetDatabase.LoadAssetAtPath<Material>(fullPath) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {Path.GetFileName(path)}");
            return;
        }

        // Ensure directory exists
        var dir = Path.GetDirectoryName(fullPath).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(dir))
        {
            var parentDir = Path.GetDirectoryName(dir).Replace("\\", "/");
            var folderName = Path.GetFileName(dir);
            AssetDatabase.CreateFolder(parentDir, folderName);
        }

        // Try URP shader first, fallback to Standard
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material mat = new Material(shader);
        
        // Set properties for both URP and Standard
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Glossiness", smoothness);

        AssetDatabase.CreateAsset(mat, fullPath);
        Debug.Log($"  ✅ Created: {Path.GetFileName(path)}");
    }
}
