using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor window displaying a testing checklist for the game.
/// Run from menu: Tools > Escape Train Run > Testing Checklist
/// </summary>
public class TestingChecklistWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, bool> checklistItems = new Dictionary<string, bool>();

    [MenuItem("Tools/Escape Train Run/Testing Checklist")]
    public static void ShowWindow()
    {
        var window = GetWindow<TestingChecklistWindow>("Testing Checklist");
        window.minSize = new Vector2(450, 500);
    }

    private void OnEnable()
    {
        LoadChecklist();
    }

    private void LoadChecklist()
    {
        // Initialize checklist items with saved state
        string[] items = GetChecklistItems();
        foreach (var item in items)
        {
            string key = "TestChecklist_" + item.GetHashCode();
            checklistItems[item] = EditorPrefs.GetBool(key, false);
        }
    }

    private string[] GetChecklistItems()
    {
        return new string[]
        {
            // Core Mechanics
            "[CORE] Player can move left/center/right lanes",
            "[CORE] Player can jump over obstacles",
            "[CORE] Player can slide under obstacles",
            "[CORE] Collision with obstacles triggers game over",
            "[CORE] Coins can be collected",
            "[CORE] Score increases over time",
            
            // Power-ups
            "[POWERUP] Magnet collects nearby coins",
            "[POWERUP] Shield protects from one hit",
            "[POWERUP] Speed boost increases movement",
            "[POWERUP] Coin multiplier doubles coin value",
            "[POWERUP] Power-ups have duration and expire",
            
            // UI
            "[UI] Main menu displays and buttons work",
            "[UI] HUD shows score and coins",
            "[UI] Pause menu pauses gameplay",
            "[UI] Game over screen shows final score",
            "[UI] Settings menu controls audio",
            
            // Shop System
            "[SHOP] Characters display with prices",
            "[SHOP] Can purchase characters with coins",
            "[SHOP] Purchased characters can be selected",
            "[SHOP] Selected character persists",
            
            // Audio
            "[AUDIO] Background music plays",
            "[AUDIO] Coin collection sound",
            "[AUDIO] Power-up activation sound",
            "[AUDIO] Jump/slide sounds",
            "[AUDIO] Crash/game over sound",
            
            // Performance
            "[PERF] Steady 60 FPS on target device",
            "[PERF] No memory leaks during gameplay",
            "[PERF] Level generation doesn't cause hitches",
            
            // Save System
            "[SAVE] Coins persist between sessions",
            "[SAVE] High score saves correctly",
            "[SAVE] Unlocked characters persist",
            "[SAVE] Settings save and load",
            
            // Polish
            "[POLISH] Visual effects display correctly",
            "[POLISH] Animations are smooth",
            "[POLISH] Camera follows player smoothly",
            "[POLISH] No Z-fighting or visual glitches"
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("📋 Testing Checklist", EditorStyles.boldLabel);
        GUILayout.Label("Check off items as you test them", EditorStyles.miniLabel);
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Check All"))
        {
            SetAllChecks(true);
        }
        
        if (GUILayout.Button("Uncheck All"))
        {
            SetAllChecks(false);
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Progress bar
        int total = checklistItems.Count;
        int completed = 0;
        foreach (var item in checklistItems)
        {
            if (item.Value) completed++;
        }
        
        float progress = total > 0 ? (float)completed / total : 0;
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Height(20)), 
            progress, $"Progress: {completed}/{total} ({(int)(progress * 100)}%)");

        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        string currentCategory = "";
        string[] items = GetChecklistItems();

        foreach (var item in items)
        {
            // Extract category
            int bracketEnd = item.IndexOf(']');
            if (bracketEnd > 0)
            {
                string cat = item.Substring(1, bracketEnd - 1);
                if (cat != currentCategory)
                {
                    currentCategory = cat;
                    GUILayout.Space(10);
                    GUILayout.Label(GetCategoryLabel(cat), EditorStyles.boldLabel);
                }
            }

            EditorGUILayout.BeginHorizontal();
            
            bool oldValue = checklistItems.ContainsKey(item) ? checklistItems[item] : false;
            bool newValue = EditorGUILayout.Toggle(oldValue, GUILayout.Width(20));
            
            if (newValue != oldValue)
            {
                checklistItems[item] = newValue;
                string key = "TestChecklist_" + item.GetHashCode();
                EditorPrefs.SetBool(key, newValue);
            }

            // Display without the category prefix
            string displayText = bracketEnd > 0 ? item.Substring(bracketEnd + 2) : item;
            
            if (newValue)
            {
                GUI.color = Color.green;
            }
            
            GUILayout.Label(displayText);
            GUI.color = Color.white;
            
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private string GetCategoryLabel(string cat)
    {
        return cat switch
        {
            "CORE" => "🎮 Core Mechanics",
            "POWERUP" => "⚡ Power-ups",
            "UI" => "🖥️ User Interface",
            "SHOP" => "🛒 Shop System",
            "AUDIO" => "🔊 Audio",
            "PERF" => "📊 Performance",
            "SAVE" => "💾 Save System",
            "POLISH" => "✨ Polish",
            _ => cat
        };
    }

    private void SetAllChecks(bool value)
    {
        var keys = new List<string>(checklistItems.Keys);
        foreach (var key in keys)
        {
            checklistItems[key] = value;
            string prefKey = "TestChecklist_" + key.GetHashCode();
            EditorPrefs.SetBool(prefKey, value);
        }
        Repaint();
    }
}
