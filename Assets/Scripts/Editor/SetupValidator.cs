using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Editor script to validate game setup and find missing references.
/// Run from menu: Tools > Escape Train Run > Validate Setup
/// </summary>
public class SetupValidator : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ValidationResult> results = new List<ValidationResult>();

    private class ValidationResult
    {
        public string category;
        public string message;
        public ValidationType type;
        public Object context;
    }

    private enum ValidationType
    {
        Pass,
        Warning,
        Error
    }

    [MenuItem("Tools/Escape Train Run/Validate Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<SetupValidator>("Setup Validator");
        window.minSize = new Vector2(400, 300);
        window.RunValidation();
    }

    private void OnGUI()
    {
        GUILayout.Label("🔍 Setup Validation Results", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run Validation"))
        {
            RunValidation();
        }

        GUILayout.Space(10);

        // Summary
        int passes = 0, warnings = 0, errors = 0;
        foreach (var r in results)
        {
            switch (r.type)
            {
                case ValidationType.Pass: passes++; break;
                case ValidationType.Warning: warnings++; break;
                case ValidationType.Error: errors++; break;
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        GUILayout.Label($"✅ {passes} Passed");
        GUI.color = Color.yellow;
        GUILayout.Label($"⚠️ {warnings} Warnings");
        GUI.color = Color.red;
        GUILayout.Label($"❌ {errors} Errors");
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        string currentCategory = "";
        foreach (var result in results)
        {
            if (result.category != currentCategory)
            {
                currentCategory = result.category;
                GUILayout.Space(10);
                GUILayout.Label(currentCategory, EditorStyles.boldLabel);
            }

            EditorGUILayout.BeginHorizontal();
            
            switch (result.type)
            {
                case ValidationType.Pass:
                    GUI.color = Color.green;
                    GUILayout.Label("✓", GUILayout.Width(20));
                    break;
                case ValidationType.Warning:
                    GUI.color = Color.yellow;
                    GUILayout.Label("⚠", GUILayout.Width(20));
                    break;
                case ValidationType.Error:
                    GUI.color = Color.red;
                    GUILayout.Label("✗", GUILayout.Width(20));
                    break;
            }

            GUI.color = Color.white;
            GUILayout.Label(result.message);

            if (result.context != null)
            {
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = result.context;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void RunValidation()
    {
        results.Clear();

        ValidateTags();
        ValidateLayers();
        ValidatePrefabs();
        ValidateScenes();
        ValidateScriptableObjects();
        ValidateAudioSetup();

        Repaint();
    }

    private void ValidateTags()
    {
        string[] requiredTags = { "Player", "Obstacle", "Coin", "PowerUp" };

        foreach (var tag in requiredTags)
        {
            try
            {
                // Try to use the tag - will throw if doesn't exist
                var testObj = new GameObject();
                testObj.tag = tag;
                Object.DestroyImmediate(testObj);
                
                results.Add(new ValidationResult
                {
                    category = "📌 Tags",
                    message = $"Tag '{tag}' exists",
                    type = ValidationType.Pass
                });
            }
            catch
            {
                results.Add(new ValidationResult
                {
                    category = "📌 Tags",
                    message = $"Tag '{tag}' is missing - create it in Project Settings > Tags and Layers",
                    type = ValidationType.Error
                });
            }
        }
    }

    private void ValidateLayers()
    {
        string[] requiredLayers = { "Player", "Ground", "Obstacle", "Collectible" };

        foreach (var layer in requiredLayers)
        {
            int layerIndex = LayerMask.NameToLayer(layer);
            
            if (layerIndex >= 0)
            {
                results.Add(new ValidationResult
                {
                    category = "📂 Layers",
                    message = $"Layer '{layer}' exists (index: {layerIndex})",
                    type = ValidationType.Pass
                });
            }
            else
            {
                results.Add(new ValidationResult
                {
                    category = "📂 Layers",
                    message = $"Layer '{layer}' is missing - create it in Project Settings > Tags and Layers",
                    type = ValidationType.Warning
                });
            }
        }
    }

    private void ValidatePrefabs()
    {
        string[] requiredPrefabs = {
            "Assets/Prefabs/Player/Player.prefab",
            "Assets/Prefabs/Environment/TrackSegment.prefab",
            "Assets/Prefabs/Collectibles/Coin.prefab"
        };

        foreach (var path in requiredPrefabs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                results.Add(new ValidationResult
                {
                    category = "📦 Prefabs",
                    message = $"Found: {System.IO.Path.GetFileName(path)}",
                    type = ValidationType.Pass,
                    context = prefab
                });
            }
            else
            {
                results.Add(new ValidationResult
                {
                    category = "📦 Prefabs",
                    message = $"Missing: {System.IO.Path.GetFileName(path)} - Run 'Create All Prefabs' from Tools menu",
                    type = ValidationType.Warning
                });
            }
        }
    }

    private void ValidateScenes()
    {
        var buildScenes = EditorBuildSettings.scenes;
        
        if (buildScenes.Length == 0)
        {
            results.Add(new ValidationResult
            {
                category = "🎬 Scenes",
                message = "No scenes in Build Settings - Add scenes via File > Build Settings",
                type = ValidationType.Warning
            });
        }
        else
        {
            foreach (var scene in buildScenes)
            {
                if (scene.enabled)
                {
                    results.Add(new ValidationResult
                    {
                        category = "🎬 Scenes",
                        message = $"Scene in build: {System.IO.Path.GetFileName(scene.path)}",
                        type = ValidationType.Pass
                    });
                }
            }
        }
    }

    private void ValidateScriptableObjects()
    {
        // Check for character data
        var characters = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects/Characters" });
        
        if (characters.Length > 0)
        {
            results.Add(new ValidationResult
            {
                category = "📋 Data Assets",
                message = $"Found {characters.Length} character data assets",
                type = ValidationType.Pass
            });
        }
        else
        {
            results.Add(new ValidationResult
            {
                category = "📋 Data Assets",
                message = "No character data found - Run 'Create All ScriptableObjects' from Tools menu",
                type = ValidationType.Warning
            });
        }
    }

    private void ValidateAudioSetup()
    {
        // Check for AudioListener
        var listener = Object.FindFirstObjectByType<AudioListener>();
        
        if (listener != null)
        {
            results.Add(new ValidationResult
            {
                category = "🔊 Audio",
                message = "AudioListener found in scene",
                type = ValidationType.Pass,
                context = listener
            });
        }
        else
        {
            results.Add(new ValidationResult
            {
                category = "🔊 Audio",
                message = "No AudioListener in scene - usually attached to Main Camera",
                type = ValidationType.Warning
            });
        }
    }
}
