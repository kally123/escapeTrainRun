using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Editor script to automatically wire up references between GameObjects.
/// Run from menu: Tools > Escape Train Run > Wire Up References
/// </summary>
public class ReferenceWiring : Editor
{
    [MenuItem("Tools/Escape Train Run/Wire Up All References")]
    public static void WireUpAllReferences()
    {
        if (!EditorUtility.DisplayDialog("Wire Up References",
            "This will attempt to automatically wire up references between scene objects. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        int wiringCount = 0;

        wiringCount += WireGameManager();
        wiringCount += WireLevelGenerator();
        wiringCount += WirePlayerReferences();
        wiringCount += WireAudioManager();
        wiringCount += WireUIReferences();
        wiringCount += WireButtonEvents();

        Debug.Log($"✅ Reference Wiring Complete! {wiringCount} references connected.");
        EditorUtility.DisplayDialog("Success", $"Reference wiring complete!\n\n{wiringCount} references connected.\n\nCheck Inspector for any missing references.", "OK");
    }

    #region GameManager

    [MenuItem("Tools/Escape Train Run/Wire References/Wire GameManager")]
    public static int WireGameManager()
    {
        int count = 0;
        var gameManager = FindObjectByName<EscapeTrainRun.GameManager>("GameManager");
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found in scene");
            return 0;
        }

        SerializedObject so = new SerializedObject(gameManager);

        // Find and assign Player
        var player = FindObjectByTag<EscapeTrainRun.PlayerController>("Player");
        if (player != null && TrySetReference(so, "player", player))
        {
            count++;
            Debug.Log("  → GameManager.player wired");
        }

        // Find and assign LevelGenerator
        var levelGen = FindObjectByName<EscapeTrainRun.LevelGenerator>("LevelGenerator");
        if (levelGen != null && TrySetReference(so, "levelGenerator", levelGen))
        {
            count++;
            Debug.Log("  → GameManager.levelGenerator wired");
        }

        // Find and assign ScoreManager
        var scoreManager = FindObjectByName<EscapeTrainRun.ScoreManager>("ScoreManager");
        if (scoreManager != null && TrySetReference(so, "scoreManager", scoreManager))
        {
            count++;
            Debug.Log("  → GameManager.scoreManager wired");
        }

        // Find and assign UI panels
        var gameplayUI = FindObjectByName<EscapeTrainRun.GameplayUI>("GameplayCanvas");
        if (gameplayUI != null && TrySetReference(so, "gameplayUI", gameplayUI))
        {
            count++;
            Debug.Log("  → GameManager.gameplayUI wired");
        }

        var pauseMenu = FindObjectByName<EscapeTrainRun.PauseMenuUI>("PauseMenu");
        if (pauseMenu != null && TrySetReference(so, "pauseMenu", pauseMenu))
        {
            count++;
            Debug.Log("  → GameManager.pauseMenu wired");
        }

        var gameOverUI = FindObjectByName<EscapeTrainRun.GameOverUI>("GameOverPanel");
        if (gameOverUI != null && TrySetReference(so, "gameOverUI", gameOverUI))
        {
            count++;
            Debug.Log("  → GameManager.gameOverUI wired");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameManager);

        Debug.Log($"✅ GameManager: {count} references wired");
        return count;
    }

    #endregion

    #region LevelGenerator

    [MenuItem("Tools/Escape Train Run/Wire References/Wire LevelGenerator")]
    public static int WireLevelGenerator()
    {
        int count = 0;
        var levelGen = FindObjectByName<EscapeTrainRun.LevelGenerator>("LevelGenerator");
        if (levelGen == null)
        {
            Debug.LogWarning("LevelGenerator not found in scene");
            return 0;
        }

        SerializedObject so = new SerializedObject(levelGen);

        // Find Player transform
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && TrySetReference(so, "playerTransform", player.transform))
        {
            count++;
            Debug.Log("  → LevelGenerator.playerTransform wired");
        }

        // Load prefabs from Assets
        count += LoadPrefabsToArray(so, "trackSegmentPrefabs", "Assets/Prefabs/Environment", "TrackSegment");
        count += LoadPrefabsToArray(so, "obstaclePrefabs", "Assets/Prefabs/Obstacles");
        count += LoadPrefabsToArray(so, "coinPrefab", "Assets/Prefabs/Collectibles", "Coin");
        count += LoadPrefabsToArray(so, "powerUpPrefabs", "Assets/Prefabs/Collectibles", "PowerUp_");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(levelGen);

        Debug.Log($"✅ LevelGenerator: {count} references wired");
        return count;
    }

    private static int LoadPrefabsToArray(SerializedObject so, string propertyName, string folderPath, string filter = null)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null) return 0;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        var prefabs = guids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Where(p => filter == null || System.IO.Path.GetFileNameWithoutExtension(p).Contains(filter))
            .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
            .Where(p => p != null)
            .ToArray();

        if (prefabs.Length == 0) return 0;

        if (prop.isArray)
        {
            prop.arraySize = prefabs.Length;
            for (int i = 0; i < prefabs.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            }
            Debug.Log($"  → {propertyName}: {prefabs.Length} prefabs loaded");
            return prefabs.Length;
        }
        else
        {
            prop.objectReferenceValue = prefabs[0];
            Debug.Log($"  → {propertyName}: prefab loaded");
            return 1;
        }
    }

    #endregion

    #region Player

    [MenuItem("Tools/Escape Train Run/Wire References/Wire Player")]
    public static int WirePlayerReferences()
    {
        int count = 0;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene");
            return 0;
        }

        // Wire PlayerController
        var controller = player.GetComponent<EscapeTrainRun.PlayerController>();
        if (controller != null)
        {
            SerializedObject so = new SerializedObject(controller);

            var movement = player.GetComponent<EscapeTrainRun.PlayerMovement>();
            if (movement != null && TrySetReference(so, "movement", movement))
            {
                count++;
                Debug.Log("  → PlayerController.movement wired");
            }

            var collision = player.GetComponent<EscapeTrainRun.PlayerCollision>();
            if (collision != null && TrySetReference(so, "collision", collision))
            {
                count++;
                Debug.Log("  → PlayerController.collision wired");
            }

            var animation = player.GetComponent<EscapeTrainRun.PlayerAnimation>();
            if (animation != null && TrySetReference(so, "playerAnimation", animation))
            {
                count++;
                Debug.Log("  → PlayerController.playerAnimation wired");
            }

            var charController = player.GetComponent<CharacterController>();
            if (charController != null && TrySetReference(so, "characterController", charController))
            {
                count++;
                Debug.Log("  → PlayerController.characterController wired");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        // Wire PlayerMovement
        var playerMovement = player.GetComponent<EscapeTrainRun.PlayerMovement>();
        if (playerMovement != null)
        {
            SerializedObject so = new SerializedObject(playerMovement);

            var charController = player.GetComponent<CharacterController>();
            if (charController != null && TrySetReference(so, "characterController", charController))
            {
                count++;
                Debug.Log("  → PlayerMovement.characterController wired");
            }

            // Find ground check
            var groundCheck = player.transform.Find("GroundCheck");
            if (groundCheck != null && TrySetReference(so, "groundCheck", groundCheck))
            {
                count++;
                Debug.Log("  → PlayerMovement.groundCheck wired");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(playerMovement);
        }

        // Wire PlayerAnimation
        var playerAnim = player.GetComponent<EscapeTrainRun.PlayerAnimation>();
        if (playerAnim != null)
        {
            SerializedObject so = new SerializedObject(playerAnim);

            var model = player.transform.Find("Model");
            if (model != null)
            {
                var animator = model.GetComponent<Animator>();
                if (animator != null && TrySetReference(so, "animator", animator))
                {
                    count++;
                    Debug.Log("  → PlayerAnimation.animator wired");
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(playerAnim);
        }

        Debug.Log($"✅ Player: {count} references wired");
        return count;
    }

    #endregion

    #region AudioManager

    [MenuItem("Tools/Escape Train Run/Wire References/Wire AudioManager")]
    public static int WireAudioManager()
    {
        int count = 0;
        var audioManagerObj = GameObject.Find("AudioManager");
        if (audioManagerObj == null)
        {
            Debug.LogWarning("AudioManager not found in scene");
            return 0;
        }

        var audioManager = audioManagerObj.GetComponent<EscapeTrainRun.AudioManager>();
        if (audioManager == null) return 0;

        SerializedObject so = new SerializedObject(audioManager);

        // Find child AudioSources
        var musicSource = audioManagerObj.transform.Find("MusicSource");
        if (musicSource != null)
        {
            var source = musicSource.GetComponent<AudioSource>();
            if (source != null && TrySetReference(so, "musicSource", source))
            {
                count++;
                Debug.Log("  → AudioManager.musicSource wired");
            }
        }

        var sfxSource = audioManagerObj.transform.Find("SFXSource");
        if (sfxSource != null)
        {
            var source = sfxSource.GetComponent<AudioSource>();
            if (source != null && TrySetReference(so, "sfxSource", source))
            {
                count++;
                Debug.Log("  → AudioManager.sfxSource wired");
            }
        }

        var ambientSource = audioManagerObj.transform.Find("AmbientSource");
        if (ambientSource != null)
        {
            var source = ambientSource.GetComponent<AudioSource>();
            if (source != null && TrySetReference(so, "ambientSource", source))
            {
                count++;
                Debug.Log("  → AudioManager.ambientSource wired");
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(audioManager);

        Debug.Log($"✅ AudioManager: {count} references wired");
        return count;
    }

    #endregion

    #region UI References

    [MenuItem("Tools/Escape Train Run/Wire References/Wire UI")]
    public static int WireUIReferences()
    {
        int count = 0;

        count += WireGameplayUI();
        count += WirePauseMenuUI();
        count += WireGameOverUI();

        Debug.Log($"✅ UI: {count} references wired");
        return count;
    }

    private static int WireGameplayUI()
    {
        int count = 0;
        var canvas = GameObject.Find("GameplayCanvas");
        if (canvas == null) return 0;

        var gameplayUI = canvas.GetComponent<EscapeTrainRun.GameplayUI>();
        if (gameplayUI == null) return 0;

        SerializedObject so = new SerializedObject(gameplayUI);

        // Find TopBar elements
        var topBar = canvas.transform.Find("TopBar");
        if (topBar != null)
        {
            count += TryFindAndSetText(so, "scoreText", topBar, "ScoreText");
            
            var coinCounter = topBar.Find("CoinCounter");
            if (coinCounter != null)
            {
                count += TryFindAndSetText(so, "coinText", coinCounter, "CoinText");
            }

            var pauseBtn = topBar.Find("PauseButton");
            if (pauseBtn != null)
            {
                var btn = pauseBtn.GetComponent<Button>();
                if (btn != null && TrySetReference(so, "pauseButton", btn))
                {
                    count++;
                }
            }
        }

        // Find PowerUpIndicator
        var powerUpIndicator = canvas.transform.Find("PowerUpIndicator");
        if (powerUpIndicator != null)
        {
            if (TrySetReference(so, "powerUpPanel", powerUpIndicator.gameObject))
                count++;

            count += TryFindAndSetImage(so, "powerUpIcon", powerUpIndicator, "PowerUpIcon");
            count += TryFindAndSetImage(so, "powerUpTimer", powerUpIndicator, "TimerBar");
        }

        // Find ComboDisplay
        var comboDisplay = canvas.transform.Find("ComboDisplay");
        if (comboDisplay != null)
        {
            count += TryFindAndSetText(so, "comboText", comboDisplay, "ComboText");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameplayUI);

        return count;
    }

    private static int WirePauseMenuUI()
    {
        int count = 0;
        var pauseMenu = GameObject.Find("PauseMenu");
        if (pauseMenu == null) return 0;

        var pauseUI = pauseMenu.GetComponent<EscapeTrainRun.PauseMenuUI>();
        if (pauseUI == null) return 0;

        SerializedObject so = new SerializedObject(pauseUI);

        var panel = pauseMenu.transform.Find("Panel");
        if (panel != null)
        {
            count += TryFindAndSetButton(so, "resumeButton", panel, "ResumeButton");
            count += TryFindAndSetButton(so, "settingsButton", panel, "SettingsButton");
            count += TryFindAndSetButton(so, "mainMenuButton", panel, "MainMenuButton");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(pauseUI);

        return count;
    }

    private static int WireGameOverUI()
    {
        int count = 0;
        var gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel == null) return 0;

        var gameOverUI = gameOverPanel.GetComponent<EscapeTrainRun.GameOverUI>();
        if (gameOverUI == null) return 0;

        SerializedObject so = new SerializedObject(gameOverUI);

        var panel = gameOverPanel.transform.Find("Panel");
        if (panel != null)
        {
            count += TryFindAndSetText(so, "finalScoreText", panel, "FinalScoreText");
            count += TryFindAndSetText(so, "highScoreText", panel, "HighScoreText");
            count += TryFindAndSetText(so, "coinsText", panel, "CoinsCollectedText");
            count += TryFindAndSetText(so, "distanceText", panel, "DistanceText");

            count += TryFindAndSetButton(so, "playAgainButton", panel, "PlayAgainButton");
            count += TryFindAndSetButton(so, "mainMenuButton", panel, "MainMenuButton");
            count += TryFindAndSetButton(so, "doubleCoinsButton", panel, "DoubleCoinsButton");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameOverUI);

        return count;
    }

    #endregion

    #region Button Events

    [MenuItem("Tools/Escape Train Run/Wire References/Wire Button Events")]
    public static int WireButtonEvents()
    {
        int count = 0;

        // Pause Menu buttons
        count += WireButtonToMethod("PauseMenu/Panel/ResumeButton", "PauseMenu", "OnResumeClicked");
        count += WireButtonToMethod("PauseMenu/Panel/MainMenuButton", "PauseMenu", "OnMainMenuClicked");

        // Game Over buttons
        count += WireButtonToMethod("GameOverPanel/Panel/PlayAgainButton", "GameOverPanel", "OnPlayAgainClicked");
        count += WireButtonToMethod("GameOverPanel/Panel/MainMenuButton", "GameOverPanel", "OnMainMenuClicked");

        // Gameplay pause button
        var gameplayCanvas = GameObject.Find("GameplayCanvas");
        if (gameplayCanvas != null)
        {
            var pauseBtn = FindInHierarchy(gameplayCanvas.transform, "PauseButton");
            if (pauseBtn != null)
            {
                var gameManager = Object.FindObjectOfType<EscapeTrainRun.GameManager>();
                if (gameManager != null)
                {
                    var button = pauseBtn.GetComponent<Button>();
                    if (button != null)
                    {
                        // Clear existing and add new
                        button.onClick.RemoveAllListeners();
                        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                            button.onClick,
                            new UnityEngine.Events.UnityAction(gameManager.PauseGame)
                        );
                        count++;
                        Debug.Log("  → PauseButton onClick wired to GameManager.PauseGame");
                    }
                }
            }
        }

        Debug.Log($"✅ Button Events: {count} wired");
        return count;
    }

    private static int WireButtonToMethod(string buttonPath, string targetObjectName, string methodName)
    {
        string[] pathParts = buttonPath.Split('/');
        var root = GameObject.Find(pathParts[0]);
        if (root == null) return 0;

        Transform current = root.transform;
        for (int i = 1; i < pathParts.Length; i++)
        {
            current = current.Find(pathParts[i]);
            if (current == null) return 0;
        }

        var button = current.GetComponent<Button>();
        if (button == null) return 0;

        var targetObj = GameObject.Find(targetObjectName);
        if (targetObj == null) return 0;

        // Find component with the method
        var components = targetObj.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            var method = comp.GetType().GetMethod(methodName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance);
            
            if (method != null && method.GetParameters().Length == 0)
            {
                button.onClick.RemoveAllListeners();
                var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), comp, method) as UnityEngine.Events.UnityAction;
                if (action != null)
                {
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(button.onClick, action);
                    Debug.Log($"  → {buttonPath} onClick wired to {methodName}");
                    return 1;
                }
            }
        }

        return 0;
    }

    #endregion

    #region Helper Methods

    private static T FindObjectByName<T>(string name) where T : Component
    {
        var obj = GameObject.Find(name);
        if (obj == null) return null;
        return obj.GetComponent<T>();
    }

    private static T FindObjectByTag<T>(string tag) where T : Component
    {
        var obj = GameObject.FindGameObjectWithTag(tag);
        if (obj == null) return null;
        return obj.GetComponent<T>();
    }

    private static bool TrySetReference(SerializedObject so, string propertyName, Object value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = value;
                return true;
            }
        }
        return false;
    }

    private static int TryFindAndSetText(SerializedObject so, string propertyName, Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child == null) child = FindInHierarchy(parent, childName);
        if (child == null) return 0;

        var tmp = child.GetComponent<TextMeshProUGUI>();
        if (tmp != null && TrySetReference(so, propertyName, tmp))
        {
            Debug.Log($"  → {propertyName} wired");
            return 1;
        }
        return 0;
    }

    private static int TryFindAndSetImage(SerializedObject so, string propertyName, Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child == null) child = FindInHierarchy(parent, childName);
        if (child == null) return 0;

        var image = child.GetComponent<Image>();
        if (image != null && TrySetReference(so, propertyName, image))
        {
            Debug.Log($"  → {propertyName} wired");
            return 1;
        }
        return 0;
    }

    private static int TryFindAndSetButton(SerializedObject so, string propertyName, Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child == null) child = FindInHierarchy(parent, childName);
        if (child == null) return 0;

        var button = child.GetComponent<Button>();
        if (button != null && TrySetReference(so, propertyName, button))
        {
            Debug.Log($"  → {propertyName} wired");
            return 1;
        }
        return 0;
    }

    private static Transform FindInHierarchy(Transform parent, string name)
    {
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
