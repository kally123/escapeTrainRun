using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to wire up references between game objects.
/// Run from menu: Tools > Escape Train Run > Wire References
/// </summary>
public class ReferenceWiring : Editor
{
    [MenuItem("Tools/Escape Train Run/Wire References")]
    public static void WireAllReferences()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("🔗 WIRING REFERENCES");
        Debug.Log("═══════════════════════════════════════════════════════════");

        WireManagerReferences();
        WirePlayerReferences();
        WireUIReferences();

        Debug.Log("\n✅ Reference wiring complete!");
        EditorUtility.DisplayDialog("References Wired", 
            "All component references have been connected.", "OK");
    }

    private static void WireManagerReferences()
    {
        Debug.Log("\n📌 Wiring Manager References...");

        // Find managers
        var gameManager = Object.FindFirstObjectByType<EscapeTrainRun.Core.GameManager>();
        var scoreManager = Object.FindFirstObjectByType<EscapeTrainRun.Core.ScoreManager>();
        var audioManager = Object.FindFirstObjectByType<EscapeTrainRun.Core.AudioManager>();
        var saveManager = Object.FindFirstObjectByType<EscapeTrainRun.Core.SaveManager>();
        var levelGen = Object.FindFirstObjectByType<EscapeTrainRun.Environment.LevelGenerator>();
        var coinManager = Object.FindFirstObjectByType<EscapeTrainRun.Collectibles.CoinManager>();
        var powerUpManager = Object.FindFirstObjectByType<EscapeTrainRun.Collectibles.PowerUpManager>();

        if (gameManager != null)
        {
            var so = new SerializedObject(gameManager);
            
            if (scoreManager != null)
            {
                var prop = so.FindProperty("scoreManager");
                if (prop != null) prop.objectReferenceValue = scoreManager;
            }
            
            if (audioManager != null)
            {
                var prop = so.FindProperty("audioManager");
                if (prop != null) prop.objectReferenceValue = audioManager;
            }

            if (saveManager != null)
            {
                var prop = so.FindProperty("saveManager");
                if (prop != null) prop.objectReferenceValue = saveManager;
            }

            if (levelGen != null)
            {
                var prop = so.FindProperty("levelGenerator");
                if (prop != null) prop.objectReferenceValue = levelGen;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gameManager);
            
            Debug.Log("  ✅ GameManager references wired");
        }
        else
        {
            Debug.Log("  ⚠️ GameManager not found in scene");
        }
    }

    private static void WirePlayerReferences()
    {
        Debug.Log("\n🏃 Wiring Player References...");

        var playerController = Object.FindFirstObjectByType<EscapeTrainRun.Player.PlayerController>();
        
        if (playerController != null)
        {
            var so = new SerializedObject(playerController);
            
            // Find player's children
            var animator = playerController.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                var prop = so.FindProperty("animator");
                if (prop != null) prop.objectReferenceValue = animator;
            }

            var cc = playerController.GetComponent<CharacterController>();
            if (cc != null)
            {
                var prop = so.FindProperty("characterController");
                if (prop != null) prop.objectReferenceValue = cc;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(playerController);
            
            Debug.Log("  ✅ PlayerController references wired");
        }
        else
        {
            Debug.Log("  ⚠️ PlayerController not found in scene");
        }
    }

    private static void WireUIReferences()
    {
        Debug.Log("\n🖥️ Wiring UI References...");

        // Find UI components
        var gameHUD = Object.FindFirstObjectByType<EscapeTrainRun.UI.GameHUD>();
        var mainMenu = Object.FindFirstObjectByType<EscapeTrainRun.UI.MainMenuUI>();
        var pauseMenu = Object.FindFirstObjectByType<EscapeTrainRun.UI.PauseMenuUI>();
        var gameOver = Object.FindFirstObjectByType<EscapeTrainRun.UI.GameOverUI>();

        int wiredCount = 0;

        if (gameHUD != null)
        {
            WireUIComponent(gameHUD);
            wiredCount++;
        }
        
        if (mainMenu != null)
        {
            WireUIComponent(mainMenu);
            wiredCount++;
        }
        
        if (pauseMenu != null)
        {
            WireUIComponent(pauseMenu);
            wiredCount++;
        }
        
        if (gameOver != null)
        {
            WireUIComponent(gameOver);
            wiredCount++;
        }

        if (wiredCount > 0)
        {
            Debug.Log($"  ✅ {wiredCount} UI components wired");
        }
        else
        {
            Debug.Log("  ⚠️ No UI components found in scene");
        }
    }

    private static void WireUIComponent(MonoBehaviour component)
    {
        var so = new SerializedObject(component);
        bool anyWired = false;

        // Auto-wire common UI references by finding children with specific names
        var transform = component.transform;
        
        // Look for common button patterns
        var buttons = new[] { "PlayButton", "StartButton", "PauseButton", "ResumeButton", "RestartButton", "MenuButton", "SettingsButton" };
        
        foreach (var buttonName in buttons)
        {
            var buttonTransform = transform.Find(buttonName);
            if (buttonTransform != null)
            {
                var button = buttonTransform.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    string propName = char.ToLower(buttonName[0]) + buttonName.Substring(1);
                    var prop = so.FindProperty(propName);
                    if (prop != null)
                    {
                        prop.objectReferenceValue = button;
                        anyWired = true;
                    }
                }
            }
        }

        if (anyWired)
        {
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(component);
        }
    }
}
