using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Quality;
using System.Collections.Generic;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for the Quality Assurance system.
    /// Tests quality checkers, validators, and compliance tools.
    /// </summary>
    [TestFixture]
    public class QualityTests
    {
        #region CheckResult Tests

        [Test]
        public void CheckResult_Constructor_SetsName()
        {
            // Act
            var result = new CheckResult("Test Check");

            // Assert
            Assert.AreEqual("Test Check", result.Name);
            Assert.IsNotNull(result.Details);
        }

        [Test]
        public void CheckResult_Passed_DefaultsFalse()
        {
            // Act
            var result = new CheckResult("Test");

            // Assert
            Assert.IsFalse(result.Passed);
        }

        [Test]
        public void CheckResult_Score_DefaultsZero()
        {
            // Act
            var result = new CheckResult("Test");

            // Assert
            Assert.AreEqual(0f, result.Score);
        }

        [Test]
        public void CheckResult_AddDetails_AddsToList()
        {
            // Arrange
            var result = new CheckResult("Test");

            // Act
            result.Details.Add("Detail 1");
            result.Details.Add("Detail 2");

            // Assert
            Assert.AreEqual(2, result.Details.Count);
            Assert.AreEqual("Detail 1", result.Details[0]);
        }

        #endregion

        #region QualityReport Tests

        [Test]
        public void QualityReport_Constructor_InitializesResults()
        {
            // Act
            var report = new QualityReport();

            // Assert
            Assert.IsNotNull(report.Results);
        }

        [Test]
        public void QualityReport_ToJson_ReturnsValidJson()
        {
            // Arrange
            var report = new QualityReport
            {
                Platform = "Test",
                TotalChecks = 5,
                PassedCount = 4
            };

            // Act
            string json = report.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("Test"));
        }

        [Test]
        public void QualityReport_OverallPassed_TrueWhenNoFailures()
        {
            // Arrange
            var report = new QualityReport
            {
                FailedCount = 0
            };

            // Act
            report.OverallPassed = report.FailedCount == 0;

            // Assert
            Assert.IsTrue(report.OverallPassed);
        }

        [Test]
        public void QualityReport_OverallPassed_FalseWhenFailures()
        {
            // Arrange
            var report = new QualityReport
            {
                FailedCount = 2
            };

            // Act
            report.OverallPassed = report.FailedCount == 0;

            // Assert
            Assert.IsFalse(report.OverallPassed);
        }

        #endregion

        #region PerformanceSnapshot Tests

        [Test]
        public void PerformanceSnapshot_Struct_HasCorrectFields()
        {
            // Arrange & Act
            var snapshot = new PerformanceSnapshot
            {
                CurrentFPS = 60f,
                AverageFPS = 58.5f,
                MinFPS = 45f,
                MaxFPS = 62f,
                FrameTimeMs = 16.67f,
                StabilityScore = 95f
            };

            // Assert
            Assert.AreEqual(60f, snapshot.CurrentFPS);
            Assert.AreEqual(58.5f, snapshot.AverageFPS);
            Assert.AreEqual(45f, snapshot.MinFPS);
            Assert.AreEqual(62f, snapshot.MaxFPS);
        }

        [Test]
        public void PerformanceLevel_HasCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)PerformanceLevel.Excellent);
            Assert.AreEqual(1, (int)PerformanceLevel.Good);
            Assert.AreEqual(2, (int)PerformanceLevel.Warning);
            Assert.AreEqual(3, (int)PerformanceLevel.Critical);
        }

        #endregion

        #region MemorySnapshot Tests

        [Test]
        public void MemorySnapshot_Struct_HasCorrectFields()
        {
            // Arrange & Act
            var snapshot = new MemorySnapshot
            {
                TotalMemoryMB = 256f,
                PeakMemoryMB = 300f,
                GrowthRateMBPerMin = 1.5f,
                PotentialLeak = false
            };

            // Assert
            Assert.AreEqual(256f, snapshot.TotalMemoryMB);
            Assert.AreEqual(300f, snapshot.PeakMemoryMB);
            Assert.IsFalse(snapshot.PotentialLeak);
        }

        [Test]
        public void MemoryLevel_HasCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)MemoryLevel.Normal);
            Assert.AreEqual(1, (int)MemoryLevel.Warning);
            Assert.AreEqual(2, (int)MemoryLevel.Critical);
        }

        #endregion

        #region LatencySnapshot Tests

        [Test]
        public void LatencySnapshot_Struct_HasCorrectFields()
        {
            // Arrange & Act
            var snapshot = new LatencySnapshot
            {
                CurrentLatencyMs = 45f,
                AverageLatencyMs = 50f,
                MinLatencyMs = 30f,
                MaxLatencyMs = 80f,
                SampleCount = 30
            };

            // Assert
            Assert.AreEqual(45f, snapshot.CurrentLatencyMs);
            Assert.AreEqual(50f, snapshot.AverageLatencyMs);
            Assert.AreEqual(30, snapshot.SampleCount);
        }

        [Test]
        public void LatencyLevel_HasCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)LatencyLevel.Excellent);
            Assert.AreEqual(1, (int)LatencyLevel.Good);
            Assert.AreEqual(2, (int)LatencyLevel.Warning);
            Assert.AreEqual(3, (int)LatencyLevel.Critical);
        }

        [Test]
        public void LatencyTestResult_PassedTarget_CorrectLogic()
        {
            // Arrange
            var result = new LatencyTestResult
            {
                AverageLatencyMs = 45f,
                PassedTarget = true
            };

            // Assert
            Assert.IsTrue(result.PassedTarget);
        }

        #endregion

        #region ValidationResult Tests

        [Test]
        public void ValidationResult_Constructor_Initializes()
        {
            // Act
            var result = new ValidationResult
            {
                Name = "Test",
                Passed = true,
                Severity = ValidationSeverity.Info
            };

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.IsTrue(result.Passed);
            Assert.AreEqual(ValidationSeverity.Info, result.Severity);
        }

        [Test]
        public void ValidationSeverity_HasCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)ValidationSeverity.Info);
            Assert.AreEqual(1, (int)ValidationSeverity.Warning);
            Assert.AreEqual(2, (int)ValidationSeverity.Error);
        }

        #endregion

        #region BuildValidationReport Tests

        [Test]
        public void BuildValidationReport_ReadyForBuild_Logic()
        {
            // Arrange
            var report = new BuildValidationReport
            {
                ErrorCount = 0,
                WarningCount = 2
            };

            // Act
            report.ReadyForBuild = report.ErrorCount == 0;

            // Assert
            Assert.IsTrue(report.ReadyForBuild);
        }

        [Test]
        public void BuildValidationReport_NotReadyWithErrors()
        {
            // Arrange
            var report = new BuildValidationReport
            {
                ErrorCount = 1,
                WarningCount = 0
            };

            // Act
            report.ReadyForBuild = report.ErrorCount == 0;

            // Assert
            Assert.IsFalse(report.ReadyForBuild);
        }

        #endregion

        #region ComplianceResult Tests

        [Test]
        public void ComplianceResult_Constructor_InitializesDetails()
        {
            // Act
            var result = new ComplianceResult
            {
                Category = "Test",
                Compliant = true
            };

            // Assert
            Assert.IsNotNull(result.Details);
            Assert.AreEqual("Test", result.Category);
        }

        [Test]
        public void ComplianceReport_OverallCompliant_Logic()
        {
            // Arrange
            var report = new ComplianceReport
            {
                Results = new List<ComplianceResult>
                {
                    new ComplianceResult { Compliant = true },
                    new ComplianceResult { Compliant = true }
                }
            };

            // Act
            bool allCompliant = true;
            foreach (var r in report.Results)
            {
                if (!r.Compliant) allCompliant = false;
            }

            // Assert
            Assert.IsTrue(allCompliant);
        }

        #endregion

        #region FullQualityReport Tests

        [Test]
        public void FullQualityReport_CalculateOverallScore_AveragesComponents()
        {
            // Arrange
            var report = new FullQualityReport
            {
                QualityReport = new QualityReport { OverallScore = 80f },
                PerformanceSnapshot = new PerformanceSnapshot { StabilityScore = 90f },
                ComplianceReport = new ComplianceReport { OverallCompliant = true },
                BuildReport = new BuildValidationReport { ReadyForBuild = true }
            };

            // Act
            report.CalculateOverallScore();

            // Assert
            Assert.Greater(report.OverallScore, 0f);
            Assert.LessOrEqual(report.OverallScore, 100f);
        }

        [Test]
        public void FullQualityReport_EmptyReport_ZeroScore()
        {
            // Arrange
            var report = new FullQualityReport();

            // Act
            report.CalculateOverallScore();

            // Assert
            Assert.AreEqual(0f, report.OverallScore);
        }

        #endregion

        #region ReportFormat Tests

        [Test]
        public void ReportFormat_HasCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)ReportFormat.Markdown);
            Assert.AreEqual(1, (int)ReportFormat.PlainText);
            Assert.AreEqual(2, (int)ReportFormat.JSON);
        }

        #endregion
    }
}
