using UnityEngine;
using System;
using System.Text;
using System.IO;

namespace EscapeTrainRun.Quality
{
    /// <summary>
    /// Generates comprehensive quality reports for review.
    /// Combines data from all quality tools into exportable formats.
    /// </summary>
    public class QualityReportGenerator : MonoBehaviour
    {
        public static QualityReportGenerator Instance { get; private set; }

        [Header("Report Settings")]
        [SerializeField] private string reportDirectory = "QualityReports";
        [SerializeField] private bool includePerformanceData = true;
        [SerializeField] private bool includeMemoryData = true;
        [SerializeField] private bool includeComplianceData = true;
        [SerializeField] private bool includeBuildValidation = true;

        // Events
        public event Action<string> OnReportGenerated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Public API

        /// <summary>
        /// Generates a full quality report.
        /// </summary>
        public FullQualityReport GenerateFullReport()
        {
            var report = new FullQualityReport
            {
                GeneratedAt = DateTime.Now,
                GameVersion = Application.version,
                UnityVersion = Application.unityVersion,
                Platform = Application.platform.ToString(),
                DeviceName = SystemInfo.deviceName,
                DeviceModel = SystemInfo.deviceModel,
                OperatingSystem = SystemInfo.operatingSystem
            };

            // Quality checks
            if (QualityChecker.Instance != null)
            {
                report.QualityReport = QualityChecker.Instance.RunAllChecks();
            }

            // Performance data
            if (includePerformanceData && PerformanceMonitor.Instance != null)
            {
                report.PerformanceSnapshot = PerformanceMonitor.Instance.GetSnapshot();
            }

            // Memory data
            if (includeMemoryData && MemoryTracker.Instance != null)
            {
                report.MemorySnapshot = MemoryTracker.Instance.GetSnapshot();
            }

            // Compliance data
            if (includeComplianceData && ComplianceChecker.Instance != null)
            {
                report.ComplianceReport = ComplianceChecker.Instance.RunComplianceCheck();
            }

            // Build validation
            if (includeBuildValidation && BuildValidator.Instance != null)
            {
                report.BuildReport = BuildValidator.Instance.ValidateAll();
            }

            // Calculate overall score
            report.CalculateOverallScore();

            return report;
        }

        /// <summary>
        /// Generates report and saves to file.
        /// </summary>
        public string GenerateAndSaveReport(ReportFormat format = ReportFormat.Markdown)
        {
            var report = GenerateFullReport();
            string content = FormatReport(report, format);
            string filePath = SaveReport(content, format);

            OnReportGenerated?.Invoke(filePath);
            return filePath;
        }

        /// <summary>
        /// Gets report as string in specified format.
        /// </summary>
        public string GetReportString(ReportFormat format = ReportFormat.Markdown)
        {
            var report = GenerateFullReport();
            return FormatReport(report, format);
        }

        #endregion

        #region Formatting

        private string FormatReport(FullQualityReport report, ReportFormat format)
        {
            switch (format)
            {
                case ReportFormat.Markdown:
                    return FormatMarkdown(report);
                case ReportFormat.PlainText:
                    return FormatPlainText(report);
                case ReportFormat.JSON:
                    return FormatJSON(report);
                default:
                    return FormatMarkdown(report);
            }
        }

        private string FormatMarkdown(FullQualityReport report)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("# 🎮 Escape Train Run - Quality Report");
            sb.AppendLine();
            sb.AppendLine($"**Generated:** {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"**Version:** {report.GameVersion}");
            sb.AppendLine($"**Platform:** {report.Platform}");
            sb.AppendLine($"**Unity:** {report.UnityVersion}");
            sb.AppendLine();

            // Overall Score
            sb.AppendLine("## 📊 Overall Score");
            sb.AppendLine();
            sb.AppendLine($"### {report.OverallScore:F0}% - {GetScoreGrade(report.OverallScore)}");
            sb.AppendLine();

            // Quality Checks
            if (report.QualityReport != null)
            {
                sb.AppendLine("## ✅ Quality Checks");
                sb.AppendLine();
                sb.AppendLine($"Passed: **{report.QualityReport.PassedCount}/{report.QualityReport.TotalChecks}**");
                sb.AppendLine();
                sb.AppendLine("| Check | Status | Score |");
                sb.AppendLine("|-------|--------|-------|");

                foreach (var kvp in report.QualityReport.Results)
                {
                    string status = kvp.Value.Passed ? "✓ Pass" : "✗ Fail";
                    sb.AppendLine($"| {kvp.Key} | {status} | {kvp.Value.Score:F0}% |");
                }
                sb.AppendLine();
            }

            // Performance
            if (report.PerformanceSnapshot.AverageFPS > 0)
            {
                sb.AppendLine("## ⚡ Performance");
                sb.AppendLine();
                sb.AppendLine($"- **Average FPS:** {report.PerformanceSnapshot.AverageFPS:F1}");
                sb.AppendLine($"- **Min/Max FPS:** {report.PerformanceSnapshot.MinFPS:F0} / {report.PerformanceSnapshot.MaxFPS:F0}");
                sb.AppendLine($"- **Stability:** {report.PerformanceSnapshot.StabilityScore:F0}%");
                sb.AppendLine($"- **Dropped Frames:** {report.PerformanceSnapshot.DroppedFramePercentage:F2}%");
                sb.AppendLine();
            }

            // Memory
            if (report.MemorySnapshot.TotalMemoryMB > 0)
            {
                sb.AppendLine("## 💾 Memory");
                sb.AppendLine();
                sb.AppendLine($"- **Current:** {report.MemorySnapshot.TotalMemoryMB:F1} MB");
                sb.AppendLine($"- **Peak:** {report.MemorySnapshot.PeakMemoryMB:F1} MB");
                sb.AppendLine($"- **Growth Rate:** {report.MemorySnapshot.GrowthRateMBPerMin:F2} MB/min");
                sb.AppendLine($"- **GC Collections:** {report.MemorySnapshot.GCCollectionCount}");
                sb.AppendLine($"- **Potential Leak:** {(report.MemorySnapshot.PotentialLeak ? "⚠ Yes" : "No")}");
                sb.AppendLine();
            }

            // Compliance
            if (report.ComplianceReport != null)
            {
                sb.AppendLine("## 👶 COPPA Compliance");
                sb.AppendLine();
                sb.AppendLine($"**Status:** {(report.ComplianceReport.OverallCompliant ? "✓ Compliant" : "✗ Issues Found")}");
                sb.AppendLine($"**Target Age:** {report.ComplianceReport.TargetAgeRange} years");
                sb.AppendLine();

                foreach (var result in report.ComplianceReport.Results)
                {
                    string icon = result.Compliant ? "✓" : "✗";
                    sb.AppendLine($"- {icon} **{result.Category}**");
                }
                sb.AppendLine();
            }

            // Build Validation
            if (report.BuildReport != null)
            {
                sb.AppendLine("## 🔨 Build Validation");
                sb.AppendLine();
                sb.AppendLine($"**Ready for Build:** {(report.BuildReport.ReadyForBuild ? "✓ Yes" : "✗ No")}");
                sb.AppendLine($"**Errors:** {report.BuildReport.ErrorCount}");
                sb.AppendLine($"**Warnings:** {report.BuildReport.WarningCount}");
                sb.AppendLine();
            }

            // System Info
            sb.AppendLine("## 💻 System Info");
            sb.AppendLine();
            sb.AppendLine($"- **Device:** {report.DeviceModel}");
            sb.AppendLine($"- **OS:** {report.OperatingSystem}");
            sb.AppendLine($"- **Graphics:** {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"- **Memory:** {SystemInfo.systemMemorySize} MB");
            sb.AppendLine();

            // Footer
            sb.AppendLine("---");
            sb.AppendLine("*Generated by Escape Train Run Quality System*");

            return sb.ToString();
        }

        private string FormatPlainText(FullQualityReport report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("    ESCAPE TRAIN RUN - QUALITY REPORT");
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Version: {report.GameVersion}");
            sb.AppendLine($"Platform: {report.Platform}");
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine($"OVERALL SCORE: {report.OverallScore:F0}% - {GetScoreGrade(report.OverallScore)}");
            sb.AppendLine("───────────────────────────────────────────");
            sb.AppendLine();

            // Quality checks summary
            if (report.QualityReport != null)
            {
                sb.AppendLine($"Quality Checks: {report.QualityReport.PassedCount}/{report.QualityReport.TotalChecks} passed");
            }

            // Performance summary
            if (report.PerformanceSnapshot.AverageFPS > 0)
            {
                sb.AppendLine($"Performance: {report.PerformanceSnapshot.AverageFPS:F0} FPS avg, {report.PerformanceSnapshot.StabilityScore:F0}% stable");
            }

            // Memory summary
            if (report.MemorySnapshot.TotalMemoryMB > 0)
            {
                sb.AppendLine($"Memory: {report.MemorySnapshot.TotalMemoryMB:F0} MB current, {report.MemorySnapshot.PeakMemoryMB:F0} MB peak");
            }

            // Compliance summary
            if (report.ComplianceReport != null)
            {
                sb.AppendLine($"COPPA: {(report.ComplianceReport.OverallCompliant ? "Compliant" : "Issues Found")}");
            }

            // Build summary
            if (report.BuildReport != null)
            {
                sb.AppendLine($"Build: {(report.BuildReport.ReadyForBuild ? "Ready" : $"{report.BuildReport.ErrorCount} errors")}");
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════");

            return sb.ToString();
        }

        private string FormatJSON(FullQualityReport report)
        {
            return JsonUtility.ToJson(report, true);
        }

        private string GetScoreGrade(float score)
        {
            if (score >= 95) return "Excellent";
            if (score >= 85) return "Great";
            if (score >= 75) return "Good";
            if (score >= 60) return "Acceptable";
            if (score >= 40) return "Needs Work";
            return "Critical";
        }

        #endregion

        #region File Operations

        private string SaveReport(string content, ReportFormat format)
        {
            string extension = GetExtension(format);
            string fileName = $"QualityReport_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

            // Create directory in persistent data path
            string directory = Path.Combine(Application.persistentDataPath, reportDirectory);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filePath = Path.Combine(directory, fileName);

            File.WriteAllText(filePath, content);
            Debug.Log($"Quality report saved: {filePath}");

            return filePath;
        }

        private string GetExtension(ReportFormat format)
        {
            switch (format)
            {
                case ReportFormat.Markdown: return ".md";
                case ReportFormat.PlainText: return ".txt";
                case ReportFormat.JSON: return ".json";
                default: return ".txt";
            }
        }

        #endregion
    }

    /// <summary>
    /// Report output formats.
    /// </summary>
    public enum ReportFormat
    {
        Markdown,
        PlainText,
        JSON
    }

    /// <summary>
    /// Complete quality report with all data.
    /// </summary>
    [Serializable]
    public class FullQualityReport
    {
        public DateTime GeneratedAt;
        public string GameVersion;
        public string UnityVersion;
        public string Platform;
        public string DeviceName;
        public string DeviceModel;
        public string OperatingSystem;

        public float OverallScore;

        public QualityReport QualityReport;
        public PerformanceSnapshot PerformanceSnapshot;
        public MemorySnapshot MemorySnapshot;
        public ComplianceReport ComplianceReport;
        public BuildValidationReport BuildReport;

        public void CalculateOverallScore()
        {
            float totalScore = 0f;
            int components = 0;

            if (QualityReport != null)
            {
                totalScore += QualityReport.OverallScore;
                components++;
            }

            if (PerformanceSnapshot.StabilityScore > 0)
            {
                totalScore += PerformanceSnapshot.StabilityScore;
                components++;
            }

            if (ComplianceReport != null)
            {
                totalScore += ComplianceReport.OverallCompliant ? 100f : 50f;
                components++;
            }

            if (BuildReport != null)
            {
                totalScore += BuildReport.ReadyForBuild ? 100f : (100f - BuildReport.ErrorCount * 10f);
                components++;
            }

            OverallScore = components > 0 ? totalScore / components : 0f;
        }
    }
}
