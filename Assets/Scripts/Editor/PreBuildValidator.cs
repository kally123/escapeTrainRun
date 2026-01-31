#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using EscapeTrainRun.Quality;

namespace EscapeTrainRun.Editor
{
    /// <summary>
    /// Pre-build validation that runs before building the game.
    /// Ensures all quality checks pass before allowing a build.
    /// </summary>
    public class PreBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Check if validation should run
            if (!EditorPrefs.GetBool("EscapeTrainRun_PreBuildValidation", true))
            {
                return;
            }

            Debug.Log("[PreBuildValidator] Running pre-build validation...");

            // Create temporary validator
            var validatorGO = new GameObject("TempBuildValidator");
            var validator = validatorGO.AddComponent<BuildValidator>();

            try
            {
                var validationReport = validator.ValidateAll();

                if (!validationReport.ReadyForBuild)
                {
                    string message = $"Build validation failed!\n\n" +
                                   $"Errors: {validationReport.ErrorCount}\n" +
                                   $"Warnings: {validationReport.WarningCount}\n\n" +
                                   "Check the Console for details.\n\n" +
                                   "Do you want to continue anyway?";

                    bool continueAnyway = EditorUtility.DisplayDialog(
                        "Build Validation Failed",
                        message,
                        "Continue Anyway",
                        "Cancel Build"
                    );

                    if (!continueAnyway)
                    {
                        throw new BuildFailedException("Build cancelled due to validation failures.");
                    }
                }
                else
                {
                    Debug.Log("[PreBuildValidator] All checks passed! ✓");
                }
            }
            finally
            {
                Object.DestroyImmediate(validatorGO);
            }
        }
    }

    /// <summary>
    /// Post-build actions for quality tracking.
    /// </summary>
    public class PostBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[PostBuildProcessor] Build succeeded!");
                Debug.Log($"  Platform: {report.summary.platform}");
                Debug.Log($"  Size: {report.summary.totalSize / (1024 * 1024):F2} MB");
                Debug.Log($"  Time: {report.summary.totalTime.TotalSeconds:F1} seconds");

                // Increment build number for tracking
                int buildNumber = PlayerPrefs.GetInt("BuildNumber", 0);
                PlayerPrefs.SetInt("BuildNumber", buildNumber + 1);
                PlayerPrefs.Save();
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[PostBuildProcessor] Build failed with {report.summary.totalErrors} errors.");
            }
        }
    }

    /// <summary>
    /// Build settings helper for quality configuration.
    /// </summary>
    public static class BuildSettingsHelper
    {
        [MenuItem("Escape Train Run/Build/Configure for Release")]
        public static void ConfigureForRelease()
        {
            // Set release configuration
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;

            // Quality settings
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            // Player settings
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Medium);

            Debug.Log("[BuildSettings] Configured for release build.");
            EditorUtility.DisplayDialog("Release Configuration", "Build settings configured for release.", "OK");
        }

        [MenuItem("Escape Train Run/Build/Configure for Development")]
        public static void ConfigureForDevelopment()
        {
            // Set development configuration
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.allowDebugging = true;

            // Keep debug symbols
            PlayerSettings.stripEngineCode = false;

            Debug.Log("[BuildSettings] Configured for development build.");
            EditorUtility.DisplayDialog("Development Configuration", "Build settings configured for development.", "OK");
        }

        [MenuItem("Escape Train Run/Build/Toggle Pre-Build Validation")]
        public static void TogglePreBuildValidation()
        {
            bool current = EditorPrefs.GetBool("EscapeTrainRun_PreBuildValidation", true);
            EditorPrefs.SetBool("EscapeTrainRun_PreBuildValidation", !current);

            Debug.Log($"[BuildSettings] Pre-build validation: {(!current ? "Enabled" : "Disabled")}");
        }
    }
}
#endif
