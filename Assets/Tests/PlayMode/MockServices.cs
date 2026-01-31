using UnityEngine;
using System;
using System.Threading.Tasks;
using EscapeTrainRun.Services;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Mock implementations of backend services for testing.
    /// Provides controlled, predictable behavior for tests.
    /// </summary>
    public static class MockServices
    {
        /// <summary>
        /// Creates a mock leaderboard service.
        /// </summary>
        public static MockLeaderboardService CreateMockLeaderboard()
        {
            var obj = new GameObject("MockLeaderboardService");
            return obj.AddComponent<MockLeaderboardService>();
        }

        /// <summary>
        /// Creates a mock cloud save service.
        /// </summary>
        public static MockCloudSaveService CreateMockCloudSave()
        {
            var obj = new GameObject("MockCloudSaveService");
            return obj.AddComponent<MockCloudSaveService>();
        }

        /// <summary>
        /// Creates a mock achievement service.
        /// </summary>
        public static MockAchievementService CreateMockAchievements()
        {
            var obj = new GameObject("MockAchievementService");
            return obj.AddComponent<MockAchievementService>();
        }
    }

    /// <summary>
    /// Mock leaderboard service for testing.
    /// </summary>
    public class MockLeaderboardService : MonoBehaviour, IBackendService
    {
        public string ServiceName => "MockLeaderboardService";
        public bool IsConnected { get; set; } = true;
        public bool IsOfflineMode { get; set; } = true;

        // Test hooks
        public bool ShouldFail { get; set; } = false;
        public int SubmitCallCount { get; private set; } = 0;
        public int GetCallCount { get; private set; } = 0;
        public LeaderboardEntry LastSubmittedEntry { get; private set; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public Task<ServiceResult<LeaderboardPage>> GetLeaderboardAsync(
            LeaderboardCategory category,
            ThemeType? gameMode = null,
            int page = 0)
        {
            GetCallCount++;

            if (ShouldFail)
            {
                return Task.FromResult(
                    ServiceResult<LeaderboardPage>.Failed("Mock failure", ServiceErrorCode.NetworkError)
                );
            }

            var mockPage = new LeaderboardPage
            {
                Entries = new System.Collections.Generic.List<LeaderboardEntry>
                {
                    new LeaderboardEntry { rank = 1, playerName = "MockPlayer1", score = 100000 },
                    new LeaderboardEntry { rank = 2, playerName = "MockPlayer2", score = 90000 },
                    new LeaderboardEntry { rank = 3, playerName = "MockPlayer3", score = 80000 }
                },
                TotalCount = 3,
                PageNumber = page,
                PageSize = 50
            };

            return Task.FromResult(ServiceResult<LeaderboardPage>.Succeeded(mockPage));
        }

        public Task<ServiceResult<LeaderboardEntry>> SubmitScoreAsync(
            int score,
            ThemeType gameMode,
            int coinsCollected,
            float distanceTraveled)
        {
            SubmitCallCount++;

            if (ShouldFail)
            {
                return Task.FromResult(
                    ServiceResult<LeaderboardEntry>.Failed("Mock failure", ServiceErrorCode.NetworkError)
                );
            }

            LastSubmittedEntry = new LeaderboardEntry
            {
                score = score,
                gameMode = gameMode.ToString(),
                coinsCollected = coinsCollected,
                distanceTraveled = distanceTraveled,
                rank = 1
            };

            return Task.FromResult(ServiceResult<LeaderboardEntry>.Succeeded(LastSubmittedEntry));
        }

        public void Reset()
        {
            ShouldFail = false;
            SubmitCallCount = 0;
            GetCallCount = 0;
            LastSubmittedEntry = null;
        }
    }

    /// <summary>
    /// Mock cloud save service for testing.
    /// </summary>
    public class MockCloudSaveService : MonoBehaviour, IBackendService
    {
        public string ServiceName => "MockCloudSaveService";
        public bool IsConnected { get; set; } = true;
        public bool IsOfflineMode { get; set; } = true;

        // Test hooks
        public bool ShouldFail { get; set; } = false;
        public int SyncCallCount { get; private set; } = 0;
        public CloudSaveData MockData { get; set; }

        public Task InitializeAsync()
        {
            MockData = new CloudSaveData();
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public Task<ServiceResult<CloudSaveData>> SyncAsync()
        {
            SyncCallCount++;

            if (ShouldFail)
            {
                return Task.FromResult(
                    ServiceResult<CloudSaveData>.Failed("Mock failure", ServiceErrorCode.NetworkError)
                );
            }

            return Task.FromResult(ServiceResult<CloudSaveData>.Succeeded(MockData));
        }

        public Task<ServiceResult<CloudSaveData>> DownloadAsync()
        {
            if (ShouldFail)
            {
                return Task.FromResult(
                    ServiceResult<CloudSaveData>.Failed("Mock failure", ServiceErrorCode.NetworkError)
                );
            }

            return Task.FromResult(ServiceResult<CloudSaveData>.Succeeded(MockData));
        }

        public Task<ServiceResult<bool>> UploadAsync(CloudSaveData data)
        {
            if (ShouldFail)
            {
                return Task.FromResult(
                    ServiceResult<bool>.Failed("Mock failure", ServiceErrorCode.NetworkError)
                );
            }

            MockData = data;
            return Task.FromResult(ServiceResult<bool>.Succeeded(true));
        }

        public void Reset()
        {
            ShouldFail = false;
            SyncCallCount = 0;
            MockData = new CloudSaveData();
        }
    }

    /// <summary>
    /// Mock achievement service for testing.
    /// </summary>
    public class MockAchievementService : MonoBehaviour, IBackendService
    {
        public string ServiceName => "MockAchievementService";
        public bool IsConnected { get; set; } = true;
        public bool IsOfflineMode { get; set; } = true;

        // Test hooks
        public bool ShouldFail { get; set; } = false;
        public int UnlockCallCount { get; private set; } = 0;
        public string LastUnlockedId { get; private set; }
        public System.Collections.Generic.Dictionary<string, int> Progress { get; }
            = new System.Collections.Generic.Dictionary<string, int>();

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void IncrementProgress(string achievementId, int amount = 1)
        {
            if (!Progress.ContainsKey(achievementId))
            {
                Progress[achievementId] = 0;
            }
            Progress[achievementId] += amount;
        }

        public void UnlockAchievement(string achievementId)
        {
            UnlockCallCount++;
            LastUnlockedId = achievementId;
        }

        public bool IsUnlocked(string achievementId)
        {
            return LastUnlockedId == achievementId;
        }

        public int GetProgress(string achievementId)
        {
            return Progress.TryGetValue(achievementId, out int value) ? value : 0;
        }

        public void Reset()
        {
            ShouldFail = false;
            UnlockCallCount = 0;
            LastUnlockedId = null;
            Progress.Clear();
        }
    }
}
