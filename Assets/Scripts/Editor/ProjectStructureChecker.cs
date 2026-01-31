#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EscapeTrainRun.Editor
{
    /// <summary>
    /// Editor tool for checking project structure and required assets.
    /// Helps identify missing files, prefabs, or configurations.
    /// </summary>
    public class ProjectStructureChecker : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<CheckItem> checkItems = new List<CheckItem>();
        private bool hasRun = false;

        [MenuItem("Escape Train Run/Quality/Project Structure Checker")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectStructureChecker>("Project Checker");
            window.minSize = new Vector2(450, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("📁 Project Structure Checker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Verifies that all required files and folders exist in the project.", MessageType.Info);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("🔍 Check Project Structure", GUILayout.Height(30)))
            {
                RunCheck();
            }

            if (hasRun)
            {
                EditorGUILayout.Space(10);
                DrawSummary();
                EditorGUILayout.Space(5);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawResults();
                EditorGUILayout.EndScrollView();
            }
        }

        private void RunCheck()
        {
            checkItems.Clear();

            // Check folders
            CheckRequiredFolders();

            // Check scripts
            CheckRequiredScripts();

            // Check prefabs
            CheckRequiredPrefabs();

            // Check scenes
            CheckRequiredScenes();

            // Check resources
            CheckRequiredResources();

            // Check settings
            CheckRequiredSettings();

            hasRun = true;
            Repaint();
        }

        private void DrawSummary()
        {
            int passed = checkItems.Count(c => c.Exists);
            int failed = checkItems.Count(c => !c.Exists);

            EditorGUILayout.BeginHorizontal();

            GUIStyle passStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } };
            GUIStyle failStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } };

            EditorGUILayout.LabelField($"✓ Found: {passed}", passStyle);
            EditorGUILayout.LabelField($"✗ Missing: {failed}", failStyle);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawResults()
        {
            string currentCategory = "";

            foreach (var item in checkItems)
            {
                if (item.Category != currentCategory)
                {
                    currentCategory = item.Category;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(currentCategory, EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginHorizontal();

                string icon = item.Exists ? "✓" : "✗";
                GUIStyle style = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = item.Exists ? Color.green : Color.red }
                };

                EditorGUILayout.LabelField($"{icon} {item.Name}", style);

                if (!item.Exists && item.CanCreate)
                {
                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                    {
                        CreateItem(item);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void CheckRequiredFolders()
        {
            string[] folders = new string[]
            {
                "Assets/Scripts/Core",
                "Assets/Scripts/Player",
                "Assets/Scripts/Environment",
                "Assets/Scripts/Obstacles",
                "Assets/Scripts/Collectibles",
                "Assets/Scripts/Characters",
                "Assets/Scripts/UI",
                "Assets/Scripts/Audio",
                "Assets/Scripts/Effects",
                "Assets/Scripts/Services",
                "Assets/Scripts/Quality",
                "Assets/Scripts/Editor",
                "Assets/Prefabs/Player",
                "Assets/Prefabs/Obstacles",
                "Assets/Prefabs/Collectibles",
                "Assets/Prefabs/Environment",
                "Assets/Prefabs/UI",
                "Assets/Scenes",
                "Assets/Resources",
                "Assets/Tests/EditMode",
                "Assets/Tests/PlayMode"
            };

            foreach (var folder in folders)
            {
                checkItems.Add(new CheckItem
                {
                    Category = "📁 Folders",
                    Name = folder.Replace("Assets/", ""),
                    Path = folder,
                    Exists = Directory.Exists(folder),
                    CanCreate = true,
                    IsFolder = true
                });
            }
        }

        private void CheckRequiredScripts()
        {
            string[] scripts = new string[]
            {
                "Assets/Scripts/Core/GameManager.cs",
                "Assets/Scripts/Core/SaveManager.cs",
                "Assets/Scripts/Core/PoolManager.cs",
                "Assets/Scripts/Player/PlayerController.cs",
                "Assets/Scripts/Player/PlayerMovement.cs",
                "Assets/Scripts/Player/SwipeDetector.cs",
                "Assets/Scripts/Environment/LevelGenerator.cs",
                "Assets/Scripts/Environment/TrackSegment.cs",
                "Assets/Scripts/Quality/QualityChecker.cs",
                "Assets/Scripts/Quality/PerformanceMonitor.cs",
                "Assets/Scripts/Services/LeaderboardService.cs",
                "Assets/Scripts/Services/CloudSaveService.cs"
            };

            foreach (var script in scripts)
            {
                checkItems.Add(new CheckItem
                {
                    Category = "📜 Core Scripts",
                    Name = Path.GetFileName(script),
                    Path = script,
                    Exists = File.Exists(script),
                    CanCreate = false
                });
            }
        }

        private void CheckRequiredPrefabs()
        {
            string[] prefabs = new string[]
            {
                "Assets/Prefabs/Player/Player.prefab",
                "Assets/Prefabs/Managers/GameManager.prefab",
                "Assets/Prefabs/UI/MainCanvas.prefab"
            };

            foreach (var prefab in prefabs)
            {
                checkItems.Add(new CheckItem
                {
                    Category = "🎮 Prefabs",
                    Name = Path.GetFileName(prefab),
                    Path = prefab,
                    Exists = File.Exists(prefab),
                    CanCreate = false
                });
            }
        }

        private void CheckRequiredScenes()
        {
            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/GamePlay.unity",
                "Assets/Scenes/Shop.unity",
                "Assets/Scenes/Loading.unity"
            };

            foreach (var scene in scenes)
            {
                checkItems.Add(new CheckItem
                {
                    Category = "🎬 Scenes",
                    Name = Path.GetFileName(scene),
                    Path = scene,
                    Exists = File.Exists(scene),
                    CanCreate = true,
                    IsScene = true
                });
            }
        }

        private void CheckRequiredResources()
        {
            string[] resources = new string[]
            {
                "Assets/Resources/Config",
                "Assets/Resources/Characters",
                "Assets/Resources/Legal"
            };

            foreach (var resource in resources)
            {
                checkItems.Add(new CheckItem
                {
                    Category = "📦 Resources",
                    Name = resource.Replace("Assets/Resources/", ""),
                    Path = resource,
                    Exists = Directory.Exists(resource),
                    CanCreate = true,
                    IsFolder = true
                });
            }
        }

        private void CheckRequiredSettings()
        {
            // Check for assembly definitions
            string[] asmdefs = new string[]
            {
                "Assets/Scripts/EscapeTrainRun.asmdef",
                "Assets/Tests/EditMode/EscapeTrainRun.Tests.EditMode.asmdef",
                "Assets/Tests/PlayMode/EscapeTrainRun.Tests.PlayMode.asmdef"
            };

            foreach (var asmdef in asmdefs)
            {
                checkItems.Add(new CheckItem
                {
                    Category = "⚙️ Settings",
                    Name = Path.GetFileName(asmdef),
                    Path = asmdef,
                    Exists = File.Exists(asmdef),
                    CanCreate = false
                });
            }
        }

        private void CreateItem(CheckItem item)
        {
            if (item.IsFolder)
            {
                Directory.CreateDirectory(item.Path);
                AssetDatabase.Refresh();
                item.Exists = true;
                Debug.Log($"Created folder: {item.Path}");
            }
            else if (item.IsScene)
            {
                // Create empty scene
                var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                    UnityEditor.SceneManagement.NewSceneMode.Single
                );

                // Ensure directory exists
                string dir = Path.GetDirectoryName(item.Path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, item.Path);
                item.Exists = true;
                Debug.Log($"Created scene: {item.Path}");
            }

            Repaint();
        }

        private class CheckItem
        {
            public string Category;
            public string Name;
            public string Path;
            public bool Exists;
            public bool CanCreate;
            public bool IsFolder;
            public bool IsScene;
        }
    }
}
#endif
