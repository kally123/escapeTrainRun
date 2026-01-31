#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using EscapeTrainRun.Quality;
using System.Diagnostics;

namespace EscapeTrainRun.Editor
{
    /// <summary>
    /// Editor window for running quality checks and generating reports.
    /// Provides quick access to all QA tools from the Unity Editor.
    /// </summary>
    public class QualityCheckerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private QualityReport lastQualityReport;
        private BuildValidationReport lastBuildReport;
        private bool showQualityChecks = true;
        private bool showPerformance = true;
        private bool showBuildValidation = true;
        private bool showCompliance = true;

        [MenuItem("Escape Train Run/Quality/Quality Checker")]
        public static void ShowWindow()
        {
            var window = GetWindow<QualityCheckerWindow>("Quality Checker");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawQuickActions();
            EditorGUILayout.Space(10);

            DrawQualitySection();
            DrawPerformanceSection();
            DrawBuildSection();
            DrawComplianceSection();
            DrawReportSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("🎮 Escape Train Run Quality Checker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Run quality checks to ensure the game meets all requirements before release.", MessageType.Info);
            EditorGUILayout.Space(5);
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("▶ Run All Checks", GUILayout.Height(30)))
            {
                RunAllChecks();
            }

            if (GUILayout.Button("📄 Generate Report", GUILayout.Height(30)))
            {
                GenerateReport();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawQualitySection()
        {
            showQualityChecks = EditorGUILayout.Foldout(showQualityChecks, "✅ Quality Checks", true);

            if (showQualityChecks)
            {
                EditorGUI.indentLevel++;

                if (lastQualityReport != null)
                {
                    EditorGUILayout.LabelField($"Score: {lastQualityReport.OverallScore:F0}%");
                    EditorGUILayout.LabelField($"Passed: {lastQualityReport.PassedCount}/{lastQualityReport.TotalChecks}");

                    EditorGUILayout.Space(5);

                    foreach (var kvp in lastQualityReport.Results)
                    {
                        string icon = kvp.Value.Passed ? "✓" : "✗";
                        EditorGUILayout.LabelField($"{icon} {kvp.Key}", $"{kvp.Value.Score:F0}%");
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Click 'Run All Checks' to see results.", MessageType.None);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
        }

        private void DrawPerformanceSection()
        {
            showPerformance = EditorGUILayout.Foldout(showPerformance, "⚡ Performance Monitoring", true);

            if (showPerformance)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Add Performance Monitor"))
                {
                    AddComponentToScene<PerformanceMonitor>("PerformanceMonitor");
                }

                if (GUILayout.Button("Add Memory Tracker"))
                {
                    AddComponentToScene<MemoryTracker>("MemoryTracker");
                }

                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Add Input Latency Tester"))
                {
                    AddComponentToScene<InputLatencyTester>("InputLatencyTester");
                }

                EditorGUILayout.Space(5);

                var perfMonitor = FindObjectOfType<PerformanceMonitor>();
                if (perfMonitor != null)
                {
                    EditorGUILayout.LabelField("FPS:", $"{perfMonitor.CurrentFPS:F1} (avg: {perfMonitor.AverageFPS:F1})");
                    EditorGUILayout.LabelField("Stability:", $"{perfMonitor.StabilityScore:F0}%");
                }

                var memTracker = FindObjectOfType<MemoryTracker>();
                if (memTracker != null)
                {
                    EditorGUILayout.LabelField("Memory:", $"{memTracker.CurrentMemoryMB:F1} MB");
                    EditorGUILayout.LabelField("Peak:", $"{memTracker.PeakMemoryMB:F1} MB");
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
        }

        private void DrawBuildSection()
        {
            showBuildValidation = EditorGUILayout.Foldout(showBuildValidation, "🔨 Build Validation", true);

            if (showBuildValidation)
            {
                EditorGUI.indentLevel++;

                if (GUILayout.Button("Validate Build Settings"))
                {
                    ValidateBuild();
                }

                if (lastBuildReport != null)
                {
                    EditorGUILayout.Space(5);

                    string status = lastBuildReport.ReadyForBuild ? "✓ Ready for Build" : "✗ Issues Found";
                    EditorGUILayout.LabelField("Status:", status);
                    EditorGUILayout.LabelField("Errors:", lastBuildReport.ErrorCount.ToString());
                    EditorGUILayout.LabelField("Warnings:", lastBuildReport.WarningCount.ToString());
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
        }

        private void DrawComplianceSection()
        {
            showCompliance = EditorGUILayout.Foldout(showCompliance, "👶 COPPA Compliance", true);

            if (showCompliance)
            {
                EditorGUI.indentLevel++;

                if (GUILayout.Button("Run Compliance Check"))
                {
                    RunComplianceCheck();
                }

                EditorGUILayout.HelpBox(
                    "COPPA compliance is critical for games targeting children under 13.\n" +
                    "Ensure no personal data is collected without parental consent.",
                    MessageType.Warning);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
        }

        private void DrawReportSection()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📁 Open Reports Folder"))
            {
                OpenReportsFolder();
            }

            if (GUILayout.Button("🔗 View Checklist"))
            {
                ViewChecklist();
            }

            EditorGUILayout.EndHorizontal();
        }

        #region Actions

        private void RunAllChecks()
        {
            // Create temporary quality checker if needed
            var checker = FindObjectOfType<QualityChecker>();
            bool createdTemp = false;

            if (checker == null)
            {
                var go = new GameObject("TempQualityChecker");
                checker = go.AddComponent<QualityChecker>();
                createdTemp = true;
            }

            lastQualityReport = checker.RunAllChecks();

            if (createdTemp)
            {
                DestroyImmediate(checker.gameObject);
            }

            Repaint();
        }

        private void ValidateBuild()
        {
            var validator = FindObjectOfType<BuildValidator>();
            bool createdTemp = false;

            if (validator == null)
            {
                var go = new GameObject("TempBuildValidator");
                validator = go.AddComponent<BuildValidator>();
                createdTemp = true;
            }

            lastBuildReport = validator.ValidateAll();

            if (createdTemp)
            {
                DestroyImmediate(validator.gameObject);
            }

            Repaint();
        }

        private void RunComplianceCheck()
        {
            var checker = FindObjectOfType<ComplianceChecker>();
            bool createdTemp = false;

            if (checker == null)
            {
                var go = new GameObject("TempComplianceChecker");
                checker = go.AddComponent<ComplianceChecker>();
                createdTemp = true;
            }

            checker.RunComplianceCheck();

            if (createdTemp)
            {
                DestroyImmediate(checker.gameObject);
            }
        }

        private void GenerateReport()
        {
            var generator = FindObjectOfType<QualityReportGenerator>();
            bool createdTemp = false;

            if (generator == null)
            {
                var go = new GameObject("TempReportGenerator");
                generator = go.AddComponent<QualityReportGenerator>();
                createdTemp = true;
            }

            string path = generator.GenerateAndSaveReport(ReportFormat.Markdown);
            EditorUtility.DisplayDialog("Report Generated", $"Report saved to:\n{path}", "OK");

            if (createdTemp)
            {
                DestroyImmediate(generator.gameObject);
            }
        }

        private void OpenReportsFolder()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "QualityReports");

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            Process.Start(path);
        }

        private void ViewChecklist()
        {
            // Open the implementation plan with the checklist
            string path = System.IO.Path.Combine(Application.dataPath, "../docs/IMPLEMENTATION_PLAN.md");

            if (System.IO.File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found", "Implementation plan not found.", "OK");
            }
        }

        private void AddComponentToScene<T>(string objectName) where T : Component
        {
            var existing = FindObjectOfType<T>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Already Exists", $"{typeof(T).Name} already exists in scene.", "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var go = new GameObject(objectName);
            go.AddComponent<T>();
            Selection.activeGameObject = go;

            Undo.RegisterCreatedObjectUndo(go, $"Create {objectName}");
        }

        #endregion
    }
}
#endif
