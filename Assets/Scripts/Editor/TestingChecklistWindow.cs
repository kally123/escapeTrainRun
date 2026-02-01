using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window that provides an interactive testing checklist.
/// Open from menu: Tools > Escape Train Run > Testing Checklist
/// </summary>
public class TestingChecklistWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private bool[] checklistItems;
    private string[] checklistDescriptions;
    private string[] checklistHints;

    [MenuItem("Tools/Escape Train Run/Testing Checklist")]
    public static void ShowWindow()
    {
        var window = GetWindow<TestingChecklistWindow>("Testing Checklist");
        window.minSize = new Vector2(500, 600);
        window.Initialize();
    }

    private void Initialize()
    {
        checklistDescriptions = new string[]
        {
            "Press Play - No console errors",
            "Player appears at start position",
            "Track generates ahead of player",
            "Swipe/keyboard moves player (A/D or Arrow keys)",
            "Player can jump (Space or W)",
            "Player can slide (S or Down Arrow)",
            "Coins are visible on track",
            "Coins are collectible (score increases)",
            "Obstacles appear on track",
            "Obstacles cause collision/game over",
            "UI shows score correctly",
            "UI shows coin count correctly",
            "Pause menu works (Escape key)",
            "Resume from pause works",
            "Game Over triggers on collision",
            "Game Over panel appears",
            "Restart button works",
            "Main Menu button works",
            "Scene transitions are smooth",
            "Audio plays (music and SFX)"
        };

        checklistHints = new string[]
        {
            "Check Console window for red error messages",
            "Player should be at approximately (0, 1, 0)",
            "Look for multiple track segments spawning",
            "Press A/D or Left/Right arrow keys",
            "Press Space bar or W key",
            "Press S key or Down arrow",
            "Yellow/gold objects should appear",
            "Watch the coin counter in UI",
            "Various colored obstacles should spawn",
            "Run into an obstacle to test",
            "Top of screen should show distance/score",
            "Coin count should update when collecting",
            "Press Escape or tap pause button",
            "Click Resume button in pause menu",
            "Collide with an obstacle",
            "Panel with score and buttons should appear",
            "Click Restart to reload GamePlay scene",
            "Click Main Menu to go to menu scene",
            "No stuttering or black screens",
            "Background music and coin collect sounds"
        };

        checklistItems = new bool[checklistDescriptions.Length];
    }

    private void OnGUI()
    {
        if (checklistDescriptions == null)
        {
            Initialize();
        }

        GUILayout.Space(10);
        
        // Header
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("🧪 Phase 9: Testing Checklist", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Complete this checklist to verify your game setup is working correctly.\n" +
            "Check each item as you test it in Play mode.", 
            MessageType.Info);

        GUILayout.Space(10);

        // Progress bar
        int completedCount = 0;
        foreach (var item in checklistItems) if (item) completedCount++;
        float progress = (float)completedCount / checklistItems.Length;
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Progress: {completedCount}/{checklistItems.Length}", GUILayout.Width(100));
        EditorGUI.ProgressBar(GUILayoutUtility.GetRect(300, 20), progress, $"{(progress * 100):F0}%");
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);

        // Checklist
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Core Functionality
        DrawSection("🎮 Core Gameplay", 0, 6);
        DrawSection("💰 Collectibles & Obstacles", 6, 4);
        DrawSection("📊 User Interface", 10, 4);
        DrawSection("⏸️ Game States", 14, 4);
        DrawSection("🎵 Polish", 18, 2);

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // Buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Run Automated Validation", GUILayout.Height(30)))
        {
            SetupValidator.RunFullValidation();
        }
        
        if (GUILayout.Button("Reset Checklist", GUILayout.Height(30)))
        {
            for (int i = 0; i < checklistItems.Length; i++)
            {
                checklistItems[i] = false;
            }
        }
        
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button("▶️ Enter Play Mode", GUILayout.Height(35)))
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
        }

        GUILayout.Space(10);

        // Completion message
        if (completedCount == checklistItems.Length)
        {
            EditorGUILayout.HelpBox(
                "🎉 Congratulations! All tests passed!\n\n" +
                "Your game setup is complete. Consider moving to Phase 10 (Visual Polish) " +
                "to add materials, lighting, and effects.", 
                MessageType.None);
        }
    }

    private void DrawSection(string title, int startIndex, int count)
    {
        GUILayout.Space(10);
        
        var style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = 13;
        GUILayout.Label(title, style);
        
        EditorGUI.indentLevel++;

        for (int i = startIndex; i < startIndex + count && i < checklistItems.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            checklistItems[i] = EditorGUILayout.Toggle(checklistItems[i], GUILayout.Width(20));
            
            var labelStyle = new GUIStyle(EditorStyles.label);
            if (checklistItems[i])
            {
                labelStyle.normal.textColor = new Color(0.2f, 0.7f, 0.2f);
            }
            
            GUILayout.Label(checklistDescriptions[i], labelStyle);
            
            if (GUILayout.Button("?", GUILayout.Width(25)))
            {
                EditorUtility.DisplayDialog(
                    checklistDescriptions[i],
                    checklistHints[i],
                    "Got it!");
            }
            
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
    }
}
