using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create all ScriptableObject assets.
/// Run from menu: Tools > Escape Train Run > Create All ScriptableObjects
/// </summary>
public class ScriptableObjectCreator : Editor
{
    [MenuItem("Tools/Escape Train Run/Create All ScriptableObjects")]
    public static void CreateAllScriptableObjects()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("📋 CREATING SCRIPTABLE OBJECTS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        EnsureDirectories();
        CreateCharacterData();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("\n✅ All ScriptableObjects created!");
        EditorUtility.DisplayDialog("ScriptableObjects Created", 
            "All game data assets have been created in Assets/ScriptableObjects/", "OK");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            "Assets/ScriptableObjects",
            "Assets/ScriptableObjects/Characters",
            "Assets/ScriptableObjects/Config",
            "Assets/ScriptableObjects/Achievements",
            "Assets/ScriptableObjects/PowerUps"
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

    [MenuItem("Tools/Escape Train Run/ScriptableObjects/Create Characters")]
    public static void CreateCharacterData()
    {
        Debug.Log("\n🏃 Creating Character Data...");

        CreateCharacter("default_runner", "Default Runner", 0, true);
        CreateCharacter("speed_runner", "Speed Runner", 500, false);
        CreateCharacter("ninja", "Shadow Ninja", 1000, false);
        CreateCharacter("robot", "Robo Runner", 1500, false);
        CreateCharacter("astronaut", "Space Walker", 2000, false);
        CreateCharacter("pirate", "Captain Swift", 2500, false);
    }

    private static void CreateCharacter(string id, string displayName, int price, bool unlocked)
    {
        string path = $"Assets/ScriptableObjects/Characters/{id}.asset";
        
        if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {id}");
            return;
        }

        var character = ScriptableObject.CreateInstance<EscapeTrainRun.Characters.CharacterData>();
        
        // Use SerializedObject to set properties
        AssetDatabase.CreateAsset(character, path);
        
        var so = new SerializedObject(character);
        
        var nameProp = so.FindProperty("characterName");
        if (nameProp != null) nameProp.stringValue = displayName;
        
        var idProp = so.FindProperty("characterId");
        if (idProp != null) idProp.stringValue = id;
        
        var priceProp = so.FindProperty("price");
        if (priceProp != null) priceProp.intValue = price;
        
        var unlockedProp = so.FindProperty("isUnlockedByDefault");
        if (unlockedProp != null) unlockedProp.boolValue = unlocked;
        
        so.ApplyModifiedPropertiesWithoutUndo();
        
        EditorUtility.SetDirty(character);
        
        Debug.Log($"  ✅ Created: {displayName}");
    }
}
