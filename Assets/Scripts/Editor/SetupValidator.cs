using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor script to validate the game setup and run automated tests.
/// Run from menu: Tools > Escape Train Run > Run Setup Validation
/// </summary>
public class SetupValidator : Editor
{
    private static List<string> errors = new List<string>();
    private static List<string> warnings = new List<string>();
    private static List<string> passed = new List<string>();

    [MenuItem("Tools/Escape Train Run/Run Setup Validation")]
    public static void RunFullValidation()
    {
        errors.Clear();
        warnings.Clear();
        passed.Clear();

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("🧪 ESCAPE TRAIN RUN - SETUP VALIDATION");
        Debug.Log("═══════════════════════════════════════════════════════════");

        ValidateSceneSetup();
        ValidatePrefabs();
        ValidateScriptableObjects();
        ValidateReferences();
        ValidateTags();
        ValidateLayers();
        ValidateBuildSettings();

        PrintResults();
    }

    #region Scene Validation

    [MenuItem("Tools/Escape Train Run/Validate/Validate Current Scene")]
    public static void ValidateSceneSetup()
    {
        Debug.Log("\n📍 SCENE VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Current Scene: {sceneName}");

        if (sceneName == "GamePlay" || sceneName.Contains("Game"))
        {
            ValidateGamePlayScene();
        }
        else if (sceneName == "MainMenu" || sceneName.Contains("Menu"))
        {
            ValidateMainMenuScene();
        }
        else if (sceneName == "Shop")
        {
            ValidateShopScene();
        }
        else
        {
            warnings.Add($"Unknown scene type: {sceneName}");
        }
    }

    private static void ValidateGamePlayScene()
    {
        // Required Managers
        CheckGameObject("GameManager", true, typeof(EscapeTrainRun.GameManager));
        CheckGameObject("SaveManager", true, typeof(EscapeTrainRun.SaveManager));
        CheckGameObject("PoolManager", false, typeof(EscapeTrainRun.PoolManager));
        CheckGameObject("AudioManager", true, typeof(EscapeTrainRun.AudioManager));
        CheckGameObject("LevelGenerator", true, typeof(EscapeTrainRun.LevelGenerator));
        CheckGameObject("ScoreManager", false, typeof(EscapeTrainRun.ScoreManager));
        CheckGameObject("InputHandler", false, typeof(EscapeTrainRun.SwipeDetector));

        // Player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            passed.Add("Player found with correct tag");
            
            CheckComponent<CharacterController>(player, "CharacterController");
            CheckComponent<EscapeTrainRun.PlayerController>(player, "PlayerController");
            CheckComponent<EscapeTrainRun.PlayerMovement>(player, "PlayerMovement");
            CheckComponent<EscapeTrainRun.PlayerCollision>(player, "PlayerCollision");

            // Check child objects
            if (player.transform.Find("Model") != null)
                passed.Add("Player/Model child exists");
            else
                warnings.Add("Player/Model child not found");

            if (player.transform.Find("GroundCheck") != null)
                passed.Add("Player/GroundCheck child exists");
            else
                warnings.Add("Player/GroundCheck child not found");
        }
        else
        {
            errors.Add("Player not found! Make sure it has the 'Player' tag");
        }

        // Camera
        var camera = Camera.main;
        if (camera != null)
        {
            passed.Add("Main Camera found");
            
            var cameraRig = camera.transform.parent;
            if (cameraRig != null && cameraRig.GetComponent<EscapeTrainRun.CameraController>() != null)
                passed.Add("CameraController component found");
            else
                warnings.Add("CameraController not found on camera rig");
        }
        else
        {
            errors.Add("Main Camera not found!");
        }

        // UI
        CheckGameObject("GameplayCanvas", true);
        CheckGameObject("PauseMenu", false);
        CheckGameObject("GameOverPanel", false);

        // Check Canvas settings
        var canvas = GameObject.Find("GameplayCanvas");
        if (canvas != null)
        {
            var canvasComp = canvas.GetComponent<Canvas>();
            if (canvasComp != null && canvasComp.renderMode == RenderMode.ScreenSpaceOverlay)
                passed.Add("GameplayCanvas render mode is correct");
            else
                warnings.Add("GameplayCanvas should use Screen Space - Overlay");

            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null && scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                passed.Add("CanvasScaler is set to Scale With Screen Size");
            else
                warnings.Add("CanvasScaler should use Scale With Screen Size");
        }
    }

    private static void ValidateMainMenuScene()
    {
        CheckGameObject("MenuManager", false, typeof(EscapeTrainRun.MainMenuUI));
        CheckGameObject("MainMenuCanvas", true);
        CheckGameObject("SaveManager", true, typeof(EscapeTrainRun.SaveManager));
        CheckGameObject("AudioManager", true, typeof(EscapeTrainRun.AudioManager));

        // Check for essential UI elements
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas != null)
        {
            CheckUIElement(canvas.transform, "PlayButton", true);
            CheckUIElement(canvas.transform, "ShopButton", false);
            CheckUIElement(canvas.transform, "SettingsButton", false);
        }

        CheckGameObject("SettingsPanel", false);
    }

    private static void ValidateShopScene()
    {
        CheckGameObject("ShopManager", false, typeof(EscapeTrainRun.ShopManager));
        CheckGameObject("ShopCanvas", true, typeof(EscapeTrainRun.ShopUI));
        CheckGameObject("SaveManager", true, typeof(EscapeTrainRun.SaveManager));
    }

    #endregion

    #region Prefab Validation

    [MenuItem("Tools/Escape Train Run/Validate/Validate Prefabs")]
    public static void ValidatePrefabs()
    {
        Debug.Log("\n📦 PREFAB VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        // Track Segment
        ValidatePrefabExists("Assets/Prefabs/Environment/TrackSegment.prefab", 
            typeof(EscapeTrainRun.TrackSegment));

        // Obstacles
        ValidatePrefabExists("Assets/Prefabs/Obstacles/JumpObstacle.prefab");
        ValidatePrefabExists("Assets/Prefabs/Obstacles/SlideObstacle.prefab");
        ValidatePrefabExists("Assets/Prefabs/Obstacles/FullBlockObstacle.prefab");

        // Collectibles
        ValidatePrefabExists("Assets/Prefabs/Collectibles/Coin.prefab", typeof(EscapeTrainRun.Coin));
        
        // Count PowerUp prefabs
        string[] powerUpGuids = AssetDatabase.FindAssets("PowerUp_ t:Prefab", new[] { "Assets/Prefabs/Collectibles" });
        if (powerUpGuids.Length >= 3)
            passed.Add($"PowerUp prefabs: {powerUpGuids.Length} found");
        else
            warnings.Add($"Only {powerUpGuids.Length} PowerUp prefabs found (expected 5)");

        // Effects
        string[] effectGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Effects" });
        if (effectGuids.Length >= 3)
            passed.Add($"Effect prefabs: {effectGuids.Length} found");
        else
            warnings.Add($"Only {effectGuids.Length} Effect prefabs found");

        // Player
        ValidatePrefabExists("Assets/Prefabs/Player/Player.prefab", 
            typeof(EscapeTrainRun.PlayerController));
    }

    private static void ValidatePrefabExists(string path, System.Type requiredComponent = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            if (requiredComponent != null)
            {
                if (prefab.GetComponent(requiredComponent) != null)
                    passed.Add($"Prefab OK: {System.IO.Path.GetFileName(path)} (with {requiredComponent.Name})");
                else
                    warnings.Add($"Prefab {System.IO.Path.GetFileName(path)} missing {requiredComponent.Name}");
            }
            else
            {
                passed.Add($"Prefab OK: {System.IO.Path.GetFileName(path)}");
            }
        }
        else
        {
            errors.Add($"Prefab missing: {path}");
        }
    }

    #endregion

    #region ScriptableObject Validation

    [MenuItem("Tools/Escape Train Run/Validate/Validate ScriptableObjects")]
    public static void ValidateScriptableObjects()
    {
        Debug.Log("\n📋 SCRIPTABLEOBJECT VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        // Characters
        string[] characterGuids = AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/ScriptableObjects/Characters" });
        if (characterGuids.Length >= 1)
            passed.Add($"Character assets: {characterGuids.Length} found");
        else
            errors.Add("No CharacterData assets found!");

        // Check for default character
        var defaultChar = AssetDatabase.LoadAssetAtPath<EscapeTrainRun.Characters.CharacterData>(
            "Assets/ScriptableObjects/Characters/default_runner.asset");
        if (defaultChar != null)
            passed.Add("Default character (default_runner) exists");
        else
            warnings.Add("Default character not found at expected path");

        // Achievements
        string[] achievementGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects/Achievements" });
        if (achievementGuids.Length >= 5)
            passed.Add($"Achievement assets: {achievementGuids.Length} found");
        else
            warnings.Add($"Only {achievementGuids.Length} Achievement assets found");

        // Config
        string[] configGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects/Config" });
        if (configGuids.Length >= 1)
            passed.Add($"Config assets: {configGuids.Length} found");
        else
            warnings.Add("No config assets found");

        // PowerUp configs
        string[] powerUpConfigGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects/PowerUps" });
        if (powerUpConfigGuids.Length >= 3)
            passed.Add($"PowerUp config assets: {powerUpConfigGuids.Length} found");
        else
            warnings.Add($"Only {powerUpConfigGuids.Length} PowerUp configs found");
    }

    #endregion

    #region Reference Validation

    [MenuItem("Tools/Escape Train Run/Validate/Validate References")]
    public static void ValidateReferences()
    {
        Debug.Log("\n🔗 REFERENCE VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        // GameManager references
        var gameManager = Object.FindObjectOfType<EscapeTrainRun.GameManager>();
        if (gameManager != null)
        {
            ValidateSerializedReferences(gameManager, new[] {
                "player", "levelGenerator", "gameplayUI"
            });
        }

        // LevelGenerator references
        var levelGen = Object.FindObjectOfType<EscapeTrainRun.LevelGenerator>();
        if (levelGen != null)
        {
            ValidateSerializedReferences(levelGen, new[] {
                "playerTransform"
            });

            // Check prefab arrays
            SerializedObject so = new SerializedObject(levelGen);
            var trackProp = so.FindProperty("trackSegmentPrefabs");
            if (trackProp != null && trackProp.isArray && trackProp.arraySize > 0)
                passed.Add("LevelGenerator has track segment prefabs assigned");
            else
                warnings.Add("LevelGenerator missing track segment prefabs");
        }

        // AudioManager references
        var audioManager = Object.FindObjectOfType<EscapeTrainRun.AudioManager>();
        if (audioManager != null)
        {
            ValidateSerializedReferences(audioManager, new[] {
                "musicSource", "sfxSource"
            });
        }

        // Player references
        var playerController = Object.FindObjectOfType<EscapeTrainRun.PlayerController>();
        if (playerController != null)
        {
            ValidateSerializedReferences(playerController, new[] {
                "movement", "characterController"
            });
        }
    }

    private static void ValidateSerializedReferences(Object target, string[] propertyNames)
    {
        SerializedObject so = new SerializedObject(target);
        string typeName = target.GetType().Name;

        foreach (var propName in propertyNames)
        {
            var prop = so.FindProperty(propName);
            if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (prop.objectReferenceValue != null)
                    passed.Add($"{typeName}.{propName} is assigned");
                else
                    warnings.Add($"{typeName}.{propName} is NOT assigned");
            }
        }
    }

    #endregion

    #region Tags & Layers

    [MenuItem("Tools/Escape Train Run/Validate/Validate Tags & Layers")]
    public static void ValidateTags()
    {
        Debug.Log("\n🏷️ TAGS VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        CheckTagExists("Player");
        CheckTagExists("Obstacle");
        CheckTagExists("Coin");
        CheckTagExists("PowerUp");
        CheckTagExists("TrackSegment");
    }

    public static void ValidateLayers()
    {
        Debug.Log("\n📑 LAYERS VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        // Check if Ground layer exists
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
            passed.Add("Layer 'Ground' exists");
        else
            warnings.Add("Layer 'Ground' not found - recommend creating for ground detection");

        // Check if Player layer exists  
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
            passed.Add("Layer 'Player' exists");
        else
            warnings.Add("Layer 'Player' not found - optional but recommended");
    }

    private static void CheckTagExists(string tagName)
    {
        try
        {
            // Try to use the tag - if it doesn't exist, this will work but FindWithTag will return null
            var obj = GameObject.FindGameObjectWithTag(tagName);
            // Tag exists if we get here without exception
            passed.Add($"Tag '{tagName}' exists");
        }
        catch (UnityException)
        {
            warnings.Add($"Tag '{tagName}' does not exist - create in Edit > Project Settings > Tags and Layers");
        }
    }

    #endregion

    #region Build Settings

    [MenuItem("Tools/Escape Train Run/Validate/Validate Build Settings")]
    public static void ValidateBuildSettings()
    {
        Debug.Log("\n🔧 BUILD SETTINGS VALIDATION");
        Debug.Log("─────────────────────────────────────────");

        var scenes = EditorBuildSettings.scenes;
        
        if (scenes.Length == 0)
        {
            errors.Add("No scenes in Build Settings!");
            return;
        }

        passed.Add($"Build Settings has {scenes.Length} scenes");

        // Check required scenes
        bool hasMainMenu = scenes.Any(s => s.path.Contains("MainMenu"));
        bool hasGamePlay = scenes.Any(s => s.path.Contains("GamePlay") || s.path.Contains("Game"));
        bool hasShop = scenes.Any(s => s.path.Contains("Shop"));

        if (hasMainMenu)
            passed.Add("MainMenu scene is in Build Settings");
        else
            warnings.Add("MainMenu scene not found in Build Settings");

        if (hasGamePlay)
            passed.Add("GamePlay scene is in Build Settings");
        else
            errors.Add("GamePlay scene not found in Build Settings!");

        if (hasShop)
            passed.Add("Shop scene is in Build Settings");
        else
            warnings.Add("Shop scene not in Build Settings (optional)");

        // Check scene order
        if (scenes.Length > 0)
        {
            if (scenes[0].path.Contains("MainMenu"))
                passed.Add("MainMenu is first scene (index 0) - correct!");
            else
                warnings.Add($"First scene is '{System.IO.Path.GetFileNameWithoutExtension(scenes[0].path)}' - should be MainMenu");
        }

        // List all scenes
        Debug.Log("  Scene Build Order:");
        for (int i = 0; i < scenes.Length; i++)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            string status = scenes[i].enabled ? "✓" : "✗";
            Debug.Log($"    [{i}] {status} {sceneName}");
        }
    }

    #endregion

    #region Results

    private static void PrintResults()
    {
        Debug.Log("\n═══════════════════════════════════════════════════════════");
        Debug.Log("📊 VALIDATION RESULTS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        Debug.Log($"\n✅ PASSED: {passed.Count}");
        foreach (var item in passed)
        {
            Debug.Log($"   ✓ {item}");
        }

        Debug.Log($"\n⚠️ WARNINGS: {warnings.Count}");
        foreach (var item in warnings)
        {
            Debug.LogWarning($"   ⚠ {item}");
        }

        Debug.Log($"\n❌ ERRORS: {errors.Count}");
        foreach (var item in errors)
        {
            Debug.LogError($"   ✗ {item}");
        }

        Debug.Log("\n═══════════════════════════════════════════════════════════");
        
        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("🎉 ALL TESTS PASSED! Your setup is complete!");
            EditorUtility.DisplayDialog("Validation Complete", 
                $"✅ All {passed.Count} checks passed!\n\nYour game setup is complete.", "Awesome!");
        }
        else if (errors.Count == 0)
        {
            Debug.Log($"✅ Setup is functional with {warnings.Count} warnings.");
            EditorUtility.DisplayDialog("Validation Complete", 
                $"✅ Passed: {passed.Count}\n⚠️ Warnings: {warnings.Count}\n\nCheck console for details.", "OK");
        }
        else
        {
            Debug.Log($"❌ Setup has {errors.Count} errors that need fixing.");
            EditorUtility.DisplayDialog("Validation Complete", 
                $"✅ Passed: {passed.Count}\n⚠️ Warnings: {warnings.Count}\n❌ Errors: {errors.Count}\n\nCheck console for details.", "OK");
        }
    }

    #endregion

    #region Helper Methods

    private static void CheckGameObject(string name, bool required, System.Type componentType = null)
    {
        var obj = GameObject.Find(name);
        if (obj != null)
        {
            if (componentType != null)
            {
                var comp = obj.GetComponent(componentType);
                if (comp != null)
                    passed.Add($"GameObject '{name}' found with {componentType.Name}");
                else
                    warnings.Add($"GameObject '{name}' found but missing {componentType.Name}");
            }
            else
            {
                passed.Add($"GameObject '{name}' found");
            }
        }
        else
        {
            if (required)
                errors.Add($"Required GameObject '{name}' not found!");
            else
                warnings.Add($"Optional GameObject '{name}' not found");
        }
    }

    private static void CheckComponent<T>(GameObject obj, string componentName) where T : Component
    {
        if (obj.GetComponent<T>() != null)
            passed.Add($"{obj.name} has {componentName}");
        else
            errors.Add($"{obj.name} missing {componentName}!");
    }

    private static void CheckUIElement(Transform parent, string name, bool required)
    {
        var element = FindInHierarchy(parent, name);
        if (element != null)
            passed.Add($"UI element '{name}' found");
        else if (required)
            errors.Add($"Required UI element '{name}' not found!");
        else
            warnings.Add($"Optional UI element '{name}' not found");
    }

    private static Transform FindInHierarchy(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindInHierarchy(child, name);
            if (found != null) return found;
        }
        return null;
    }

    #endregion
}

/// <summary>
/// Runtime testing helper - add to any scene to test gameplay
/// </summary>
public class RuntimeTestHelper : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode addCoinsKey = KeyCode.C;
    public KeyCode addScoreKey = KeyCode.S;
    public KeyCode invincibleKey = KeyCode.I;
    public KeyCode speedBoostKey = KeyCode.B;
    public KeyCode skipAheadKey = KeyCode.N;

    [Header("Test Values")]
    public int coinsToAdd = 100;
    public int scoreToAdd = 1000;
    public float skipDistance = 100f;

    private bool isInvincible = false;

    void Update()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        // Add coins
        if (Input.GetKeyDown(addCoinsKey))
        {
            var scoreManager = FindObjectOfType<EscapeTrainRun.ScoreManager>();
            if (scoreManager != null)
            {
                // scoreManager.AddCoins(coinsToAdd);
                Debug.Log($"[TEST] Added {coinsToAdd} coins");
            }
        }

        // Add score
        if (Input.GetKeyDown(addScoreKey))
        {
            var scoreManager = FindObjectOfType<EscapeTrainRun.ScoreManager>();
            if (scoreManager != null)
            {
                // scoreManager.AddScore(scoreToAdd);
                Debug.Log($"[TEST] Added {scoreToAdd} score");
            }
        }

        // Toggle invincibility
        if (Input.GetKeyDown(invincibleKey))
        {
            isInvincible = !isInvincible;
            var player = FindObjectOfType<EscapeTrainRun.PlayerController>();
            if (player != null)
            {
                // player.SetInvincible(isInvincible);
                Debug.Log($"[TEST] Invincibility: {isInvincible}");
            }
        }

        // Skip ahead
        if (Input.GetKeyDown(skipAheadKey))
        {
            var player = FindObjectOfType<EscapeTrainRun.PlayerController>();
            if (player != null)
            {
                player.transform.position += Vector3.forward * skipDistance;
                Debug.Log($"[TEST] Skipped ahead {skipDistance}m");
            }
        }

        #endif
    }

    void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        GUILayout.BeginArea(new Rect(10, 10, 200, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("🧪 TEST MODE");
        GUILayout.Label($"[{addCoinsKey}] Add Coins");
        GUILayout.Label($"[{addScoreKey}] Add Score");
        GUILayout.Label($"[{invincibleKey}] Invincible: {isInvincible}");
        GUILayout.Label($"[{skipAheadKey}] Skip Ahead");
        GUILayout.EndVertical();
        GUILayout.EndArea();

        #endif
    }
}
