using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EscapeTrainRun.Quality
{
    /// <summary>
    /// Validates build configuration and project settings.
    /// Ensures proper setup before building for release.
    /// </summary>
    public class BuildValidator : MonoBehaviour
    {
        public static BuildValidator Instance { get; private set; }

        [Header("Validation Settings")]
        [SerializeField] private bool validateOnStart = false;

        // Validation results
        private List<ValidationResult> validationResults = new List<ValidationResult>();

        // Public properties
        public List<ValidationResult> Results => new List<ValidationResult>(validationResults);
        public bool AllPassed => validationResults.All(r => r.Passed || r.Severity == ValidationSeverity.Warning);
        public int ErrorCount => validationResults.Count(r => !r.Passed && r.Severity == ValidationSeverity.Error);
        public int WarningCount => validationResults.Count(r => !r.Passed && r.Severity == ValidationSeverity.Warning);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (validateOnStart)
            {
                ValidateAll();
            }
        }

        #region Public API

        /// <summary>
        /// Runs all validation checks.
        /// </summary>
        public BuildValidationReport ValidateAll()
        {
            validationResults.Clear();

            // Project settings
            ValidateProjectSettings();

            // Quality settings
            ValidateQualitySettings();

            // Build settings
            ValidateBuildSettings();

            // Required resources
            ValidateRequiredResources();

            // Scenes
            ValidateScenes();

            // Player settings
            ValidatePlayerSettings();

            // Generate report
            return GenerateReport();
        }

        /// <summary>
        /// Validates for a specific platform.
        /// </summary>
        public BuildValidationReport ValidateForPlatform(RuntimePlatform platform)
        {
            validationResults.Clear();

            ValidateProjectSettings();
            ValidateQualitySettings();

            switch (platform)
            {
                case RuntimePlatform.Android:
                    ValidateAndroidSettings();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    ValidateIOSSettings();
                    break;
                case RuntimePlatform.WindowsPlayer:
                    ValidateWindowsSettings();
                    break;
            }

            ValidateRequiredResources();
            ValidateScenes();

            return GenerateReport();
        }

        #endregion

        #region Validation Methods

        private void ValidateProjectSettings()
        {
            // Company name
            AddResult("Company Name", !string.IsNullOrEmpty(Application.companyName),
                "Company name must be set",
                $"Company: {Application.companyName}",
                ValidationSeverity.Error);

            // Product name
            AddResult("Product Name", !string.IsNullOrEmpty(Application.productName),
                "Product name must be set",
                $"Product: {Application.productName}",
                ValidationSeverity.Error);

            // Version
            bool validVersion = !string.IsNullOrEmpty(Application.version) && Application.version != "0.1";
            AddResult("Version Number", validVersion,
                "Version should be set for release",
                $"Version: {Application.version}",
                ValidationSeverity.Warning);

            // Bundle identifier
            AddResult("Bundle Identifier", !Application.identifier.Contains("DefaultCompany"),
                "Bundle identifier should not use default company",
                $"Identifier: {Application.identifier}",
                ValidationSeverity.Error);
        }

        private void ValidateQualitySettings()
        {
            // VSync
            bool vsyncOff = QualitySettings.vSyncCount == 0;
            AddResult("VSync", true,
                vsyncOff ? "VSync disabled (target frame rate will control)" : "VSync enabled",
                $"VSync Count: {QualitySettings.vSyncCount}",
                ValidationSeverity.Info);

            // Target frame rate
            bool validFrameRate = Application.targetFrameRate >= 30 || Application.targetFrameRate == -1;
            AddResult("Target Frame Rate", validFrameRate,
                "Target frame rate should be 30+ or unlimited",
                $"Target: {Application.targetFrameRate}",
                ValidationSeverity.Warning);

            // Anti-aliasing
            int aa = QualitySettings.antiAliasing;
            AddResult("Anti-Aliasing", true,
                $"Anti-aliasing level: {aa}x",
                aa > 0 ? "May impact mobile performance" : "Disabled for performance",
                aa > 2 ? ValidationSeverity.Warning : ValidationSeverity.Info);
        }

        private void ValidateBuildSettings()
        {
            // Development build check (should be false for release)
            AddResult("Development Build", !Debug.isDebugBuild,
                "Development build should be disabled for release",
                Debug.isDebugBuild ? "Debug build active" : "Release build",
                ValidationSeverity.Warning);

            // Script debugging
            AddResult("Script Debugging", true,
                "Script debugging check",
                "Check is editor-only",
                ValidationSeverity.Info);
        }

        private void ValidateRequiredResources()
        {
            // Check for required prefabs
            string[] requiredPrefabs = new string[]
            {
                "Prefabs/Player/Player",
                "Prefabs/Managers/GameManager",
                "Prefabs/UI/MainCanvas"
            };

            foreach (var prefabPath in requiredPrefabs)
            {
                var prefab = Resources.Load(prefabPath);
                AddResult($"Prefab: {Path.GetFileName(prefabPath)}",
                    prefab != null,
                    $"Required prefab missing: {prefabPath}",
                    prefab != null ? "Found" : "Missing",
                    ValidationSeverity.Error);
            }

            // Check for ScriptableObjects
            var characters = Resources.LoadAll("Characters");
            AddResult("Character Data",
                characters.Length > 0,
                "At least one character should be defined",
                $"Found {characters.Length} characters",
                ValidationSeverity.Warning);

            // Check for audio
            var music = Resources.LoadAll("Audio/Music");
            var sfx = Resources.LoadAll("Audio/SFX");
            AddResult("Audio Assets",
                music.Length > 0 || sfx.Length > 0,
                "Audio assets should be included",
                $"Music: {music.Length}, SFX: {sfx.Length}",
                ValidationSeverity.Warning);
        }

        private void ValidateScenes()
        {
            // Check scene count
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            AddResult("Build Scenes",
                sceneCount > 0,
                "At least one scene must be in build settings",
                $"{sceneCount} scenes in build",
                ValidationSeverity.Error);

            // Check for required scenes
            string[] requiredScenes = new string[] { "MainMenu", "GamePlay" };
            foreach (var sceneName in requiredScenes)
            {
                bool found = false;
                for (int i = 0; i < sceneCount; i++)
                {
                    string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                    if (scenePath.Contains(sceneName))
                    {
                        found = true;
                        break;
                    }
                }

                AddResult($"Scene: {sceneName}",
                    found,
                    $"Required scene missing: {sceneName}",
                    found ? "In build settings" : "Not found",
                    ValidationSeverity.Error);
            }
        }

        private void ValidatePlayerSettings()
        {
            // Orientation
            AddResult("Screen Orientation",
                Screen.orientation != ScreenOrientation.AutoRotation || Application.platform == RuntimePlatform.WindowsPlayer,
                "Consider locking orientation for better UX",
                $"Orientation: {Screen.orientation}",
                ValidationSeverity.Info);

            // Resolution
            AddResult("Screen Resolution",
                Screen.width > 0 && Screen.height > 0,
                "Screen resolution valid",
                $"Resolution: {Screen.width}x{Screen.height}",
                ValidationSeverity.Info);
        }

        private void ValidateAndroidSettings()
        {
            AddResult("Android Platform",
                true,
                "Validating Android-specific settings",
                "Platform: Android",
                ValidationSeverity.Info);

            // Minimum API level (would check PlayerSettings in editor)
            AddResult("Minimum API Level",
                true,
                "Should be API 24+ (Android 7.0)",
                "Check in Player Settings",
                ValidationSeverity.Info);

            // ARM64 support
            AddResult("ARM64 Support",
                true,
                "ARM64 (64-bit) should be enabled",
                "Check in Player Settings",
                ValidationSeverity.Info);
        }

        private void ValidateIOSSettings()
        {
            AddResult("iOS Platform",
                true,
                "Validating iOS-specific settings",
                "Platform: iOS",
                ValidationSeverity.Info);

            // Minimum iOS version
            AddResult("Minimum iOS Version",
                true,
                "Should be iOS 12.0+",
                "Check in Player Settings",
                ValidationSeverity.Info);

            // App Transport Security
            AddResult("App Transport Security",
                true,
                "HTTPS required for network calls",
                "Check Info.plist",
                ValidationSeverity.Warning);
        }

        private void ValidateWindowsSettings()
        {
            AddResult("Windows Platform",
                true,
                "Validating Windows-specific settings",
                "Platform: Windows",
                ValidationSeverity.Info);

            // DirectX
            AddResult("Graphics API",
                SystemInfo.graphicsDeviceType.ToString().Contains("Direct"),
                "DirectX 11+ recommended",
                $"API: {SystemInfo.graphicsDeviceType}",
                ValidationSeverity.Info);
        }

        #endregion

        #region Helpers

        private void AddResult(string name, bool passed, string message, string details, ValidationSeverity severity)
        {
            validationResults.Add(new ValidationResult
            {
                Name = name,
                Passed = passed,
                Message = message,
                Details = details,
                Severity = severity
            });
        }

        private BuildValidationReport GenerateReport()
        {
            var report = new BuildValidationReport
            {
                Timestamp = DateTime.Now,
                Platform = Application.platform.ToString(),
                Results = new List<ValidationResult>(validationResults),
                TotalChecks = validationResults.Count,
                PassedCount = validationResults.Count(r => r.Passed),
                ErrorCount = ErrorCount,
                WarningCount = WarningCount,
                ReadyForBuild = AllPassed
            };

            LogReport(report);
            return report;
        }

        private void LogReport(BuildValidationReport report)
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log($"      BUILD VALIDATION - {report.Platform}");
            Debug.Log("═══════════════════════════════════════════");

            foreach (var result in report.Results)
            {
                string icon = result.Passed ? "✓" : (result.Severity == ValidationSeverity.Error ? "✗" : "⚠");
                Debug.Log($"{icon} {result.Name}: {result.Details}");

                if (!result.Passed)
                {
                    Debug.Log($"   → {result.Message}");
                }
            }

            Debug.Log("───────────────────────────────────────────");
            Debug.Log($"Passed: {report.PassedCount}/{report.TotalChecks} | Errors: {report.ErrorCount} | Warnings: {report.WarningCount}");
            Debug.Log($"STATUS: {(report.ReadyForBuild ? "✓ READY FOR BUILD" : "✗ FIX ISSUES BEFORE BUILD")}");
            Debug.Log("═══════════════════════════════════════════");
        }

        #endregion
    }

    /// <summary>
    /// Severity levels for validation issues.
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Single validation result.
    /// </summary>
    [Serializable]
    public class ValidationResult
    {
        public string Name;
        public bool Passed;
        public string Message;
        public string Details;
        public ValidationSeverity Severity;
    }

    /// <summary>
    /// Complete build validation report.
    /// </summary>
    [Serializable]
    public class BuildValidationReport
    {
        public DateTime Timestamp;
        public string Platform;
        public List<ValidationResult> Results;
        public int TotalChecks;
        public int PassedCount;
        public int ErrorCount;
        public int WarningCount;
        public bool ReadyForBuild;
    }
}
