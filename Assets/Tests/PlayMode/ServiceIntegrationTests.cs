using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using EscapeTrainRun.Services;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for backend services.
    /// Tests service communication, sync, and error handling.
    /// </summary>
    [TestFixture]
    public class ServiceIntegrationTests
    {
        private MockLeaderboardService mockLeaderboard;
        private MockCloudSaveService mockCloudSave;
        private MockAchievementService mockAchievements;

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllEvents();
        }

        [TearDown]
        public void TearDown()
        {
            if (mockLeaderboard != null)
            {
                Object.Destroy(mockLeaderboard.gameObject);
                mockLeaderboard = null;
            }

            if (mockCloudSave != null)
            {
                Object.Destroy(mockCloudSave.gameObject);
                mockCloudSave = null;
            }

            if (mockAchievements != null)
            {
                Object.Destroy(mockAchievements.gameObject);
                mockAchievements = null;
            }

            GameEvents.ClearAllEvents();
        }

        #region Leaderboard Tests

        [UnityTest]
        public IEnumerator Leaderboard_SubmitScore_Success()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();

            yield return null;

            // Act
            var result = mockLeaderboard.SubmitScoreAsync(50000, ThemeType.Train, 100, 500f);

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert
            Assert.IsTrue(result.Result.Success);
            Assert.AreEqual(1, mockLeaderboard.SubmitCallCount);
            Assert.AreEqual(50000, mockLeaderboard.LastSubmittedEntry.score);
        }

        [UnityTest]
        public IEnumerator Leaderboard_SubmitScore_Failure()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();
            mockLeaderboard.ShouldFail = true;

            yield return null;

            // Act
            var result = mockLeaderboard.SubmitScoreAsync(50000, ThemeType.Train, 100, 500f);

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert
            Assert.IsFalse(result.Result.Success);
            Assert.AreEqual(ServiceErrorCode.NetworkError, result.Result.ErrorCode);
        }

        [UnityTest]
        public IEnumerator Leaderboard_GetLeaderboard_ReturnsEntries()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();

            yield return null;

            // Act
            var result = mockLeaderboard.GetLeaderboardAsync(LeaderboardCategory.GlobalAllTime);

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert
            Assert.IsTrue(result.Result.Success);
            Assert.Greater(result.Result.Data.Entries.Count, 0);
            Assert.AreEqual(1, mockLeaderboard.GetCallCount);
        }

        [UnityTest]
        public IEnumerator Leaderboard_MultipleSubmissions()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();

            yield return null;

            // Act - Submit multiple scores
            for (int i = 0; i < 5; i++)
            {
                var result = mockLeaderboard.SubmitScoreAsync(i * 10000, ThemeType.Train, i * 20, i * 100f);
                yield return new WaitUntil(() => result.IsCompleted);
            }

            // Assert
            Assert.AreEqual(5, mockLeaderboard.SubmitCallCount);
        }

        #endregion

        #region Cloud Save Tests

        [UnityTest]
        public IEnumerator CloudSave_Sync_Success()
        {
            // Arrange
            mockCloudSave = MockServices.CreateMockCloudSave();
            mockCloudSave.InitializeAsync();

            yield return null;

            // Act
            var result = mockCloudSave.SyncAsync();

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert
            Assert.IsTrue(result.Result.Success);
            Assert.AreEqual(1, mockCloudSave.SyncCallCount);
        }

        [UnityTest]
        public IEnumerator CloudSave_Upload_PreservesData()
        {
            // Arrange
            mockCloudSave = MockServices.CreateMockCloudSave();
            mockCloudSave.InitializeAsync();

            yield return null;

            // Create test data
            var testData = new CloudSaveData
            {
                totalCoins = 5000,
                highScore = 100000,
                totalGamesPlayed = 50
            };

            // Act
            var uploadResult = mockCloudSave.UploadAsync(testData);

            yield return new WaitUntil(() => uploadResult.IsCompleted);

            // Assert
            Assert.IsTrue(uploadResult.Result.Success);
            Assert.AreEqual(5000, mockCloudSave.MockData.totalCoins);
            Assert.AreEqual(100000, mockCloudSave.MockData.highScore);
        }

        [UnityTest]
        public IEnumerator CloudSave_Download_RetrievesData()
        {
            // Arrange
            mockCloudSave = MockServices.CreateMockCloudSave();
            mockCloudSave.InitializeAsync();

            mockCloudSave.MockData = new CloudSaveData
            {
                totalCoins = 9999,
                highScore = 250000
            };

            yield return null;

            // Act
            var result = mockCloudSave.DownloadAsync();

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert
            Assert.IsTrue(result.Result.Success);
            Assert.AreEqual(9999, result.Result.Data.totalCoins);
            Assert.AreEqual(250000, result.Result.Data.highScore);
        }

        [UnityTest]
        public IEnumerator CloudSave_OfflineMode_StillWorks()
        {
            // Arrange
            mockCloudSave = MockServices.CreateMockCloudSave();
            mockCloudSave.IsOfflineMode = true;
            mockCloudSave.InitializeAsync();

            yield return null;

            // Act
            var result = mockCloudSave.SyncAsync();

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert - Should still work in offline mode
            Assert.IsTrue(result.Result.Success);
        }

        #endregion

        #region Achievement Tests

        [UnityTest]
        public IEnumerator Achievement_Unlock_TracksCorrectly()
        {
            // Arrange
            mockAchievements = MockServices.CreateMockAchievements();

            yield return null;

            // Act
            mockAchievements.UnlockAchievement("first_run");

            yield return null;

            // Assert
            Assert.AreEqual(1, mockAchievements.UnlockCallCount);
            Assert.AreEqual("first_run", mockAchievements.LastUnlockedId);
            Assert.IsTrue(mockAchievements.IsUnlocked("first_run"));
        }

        [UnityTest]
        public IEnumerator Achievement_Progress_Increments()
        {
            // Arrange
            mockAchievements = MockServices.CreateMockAchievements();

            yield return null;

            // Act
            mockAchievements.IncrementProgress("collect_coins_100", 10);
            mockAchievements.IncrementProgress("collect_coins_100", 15);

            yield return null;

            // Assert
            Assert.AreEqual(25, mockAchievements.GetProgress("collect_coins_100"));
        }

        [UnityTest]
        public IEnumerator Achievement_MultipleUnlocks()
        {
            // Arrange
            mockAchievements = MockServices.CreateMockAchievements();

            yield return null;

            // Act
            mockAchievements.UnlockAchievement("first_run");
            mockAchievements.UnlockAchievement("score_10000");
            mockAchievements.UnlockAchievement("run_1km");

            yield return null;

            // Assert
            Assert.AreEqual(3, mockAchievements.UnlockCallCount);
        }

        #endregion

        #region Error Handling Tests

        [UnityTest]
        public IEnumerator Service_NetworkError_HandledGracefully()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();
            mockLeaderboard.ShouldFail = true;

            yield return null;

            // Act
            var result = mockLeaderboard.GetLeaderboardAsync(LeaderboardCategory.GlobalAllTime);

            yield return new WaitUntil(() => result.IsCompleted);

            // Assert - Should fail gracefully, not throw
            Assert.IsFalse(result.Result.Success);
            Assert.IsNotNull(result.Result.ErrorMessage);
        }

        [UnityTest]
        public IEnumerator Service_RecoveryAfterFailure()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();
            mockLeaderboard.ShouldFail = true;

            yield return null;

            // First call fails
            var failResult = mockLeaderboard.GetLeaderboardAsync(LeaderboardCategory.GlobalAllTime);

            yield return new WaitUntil(() => failResult.IsCompleted);

            Assert.IsFalse(failResult.Result.Success);

            // Fix the service
            mockLeaderboard.ShouldFail = false;

            // Second call succeeds
            var successResult = mockLeaderboard.GetLeaderboardAsync(LeaderboardCategory.GlobalAllTime);

            yield return new WaitUntil(() => successResult.IsCompleted);

            Assert.IsTrue(successResult.Result.Success);
        }

        #endregion

        #region Service Lifecycle Tests

        [UnityTest]
        public IEnumerator Service_Initialize_SetsConnected()
        {
            // Arrange
            mockCloudSave = MockServices.CreateMockCloudSave();

            yield return null;

            // Act
            mockCloudSave.InitializeAsync();

            yield return null;

            // Assert
            Assert.IsTrue(mockCloudSave.IsConnected);
        }

        [UnityTest]
        public IEnumerator Service_Shutdown_CleansUp()
        {
            // Arrange
            mockCloudSave = MockServices.CreateMockCloudSave();
            mockCloudSave.InitializeAsync();

            yield return null;

            // Act
            mockCloudSave.ShutdownAsync();

            yield return null;

            // Assert - Service still exists but should be in clean state
            Assert.IsNotNull(mockCloudSave);
        }

        #endregion

        #region Cross-Service Integration Tests

        [UnityTest]
        public IEnumerator GameOver_UpdatesAllServices()
        {
            // Arrange
            mockLeaderboard = MockServices.CreateMockLeaderboard();
            mockCloudSave = MockServices.CreateMockCloudSave();
            mockAchievements = MockServices.CreateMockAchievements();

            mockCloudSave.InitializeAsync();

            yield return null;

            // Simulate game over
            int finalScore = 75000;
            int coins = 150;
            float distance = 2500f;

            // Act - Submit to leaderboard
            var leaderboardTask = mockLeaderboard.SubmitScoreAsync(finalScore, ThemeType.Train, coins, distance);

            yield return new WaitUntil(() => leaderboardTask.IsCompleted);

            // Update cloud save
            mockCloudSave.MockData.highScore = finalScore;
            mockCloudSave.MockData.totalCoins += coins;

            var syncTask = mockCloudSave.SyncAsync();

            yield return new WaitUntil(() => syncTask.IsCompleted);

            // Check achievements
            mockAchievements.IncrementProgress("collect_coins_100", coins);
            if (finalScore >= 50000)
            {
                mockAchievements.UnlockAchievement("score_50000");
            }

            yield return null;

            // Assert
            Assert.AreEqual(1, mockLeaderboard.SubmitCallCount);
            Assert.AreEqual(1, mockCloudSave.SyncCallCount);
            Assert.AreEqual(150, mockAchievements.GetProgress("collect_coins_100"));
        }

        #endregion
    }
}
