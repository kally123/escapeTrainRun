using System.Collections.Generic;
using NUnit.Framework;
using EscapeTrainRun.Services;
using EscapeTrainRun.Environment;
using UnityEngine;

namespace EscapeTrainRun.Tests
{
    /// <summary>
    /// Unit tests for backend services.
    /// Tests leaderboard, cloud save, and achievement functionality.
    /// </summary>
    [TestFixture]
    public class ServicesTests
    {
        #region ServiceResult Tests

        [Test]
        public void ServiceResult_SucceededWithData_HasCorrectProperties()
        {
            var result = ServiceResult<int>.Succeeded(42);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(42, result.Data);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(ServiceErrorCode.None, result.ErrorCode);
        }

        [Test]
        public void ServiceResult_Failed_HasCorrectProperties()
        {
            var result = ServiceResult<int>.Failed("Test error", ServiceErrorCode.NetworkError);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(default(int), result.Data);
            Assert.AreEqual("Test error", result.ErrorMessage);
            Assert.AreEqual(ServiceErrorCode.NetworkError, result.ErrorCode);
        }

        [Test]
        public void ServiceResult_FailedWithDefaultCode_HasUnknownCode()
        {
            var result = ServiceResult<string>.Failed("Error message");

            Assert.AreEqual(ServiceErrorCode.Unknown, result.ErrorCode);
        }

        #endregion

        #region ServiceErrorCode Tests

        [Test]
        public void ServiceErrorCode_HasExpectedValues()
        {
            Assert.AreEqual(0, (int)ServiceErrorCode.None);
            Assert.AreEqual(100, (int)ServiceErrorCode.NetworkError);
            Assert.AreEqual(200, (int)ServiceErrorCode.AuthenticationFailed);
            Assert.AreEqual(500, (int)ServiceErrorCode.ServerError);
        }

        #endregion

        #region LeaderboardEntry Tests

        [Test]
        public void LeaderboardEntry_DefaultConstructor_GeneratesId()
        {
            var entry = new LeaderboardEntry();

            Assert.IsFalse(string.IsNullOrEmpty(entry.entryId));
            Assert.IsFalse(string.IsNullOrEmpty(entry.timestamp));
        }

        [Test]
        public void LeaderboardEntry_ParameterizedConstructor_SetsAllFields()
        {
            var entry = new LeaderboardEntry("player123", "TestPlayer", 50000, ThemeType.Train);

            Assert.AreEqual("player123", entry.playerId);
            Assert.AreEqual("TestPlayer", entry.playerName);
            Assert.AreEqual(50000, entry.score);
            Assert.AreEqual("train", entry.gameMode);
            Assert.IsFalse(string.IsNullOrEmpty(entry.entryId));
        }

        #endregion

        #region LeaderboardCategory Tests

        [Test]
        public void LeaderboardCategory_HasExpectedTypes()
        {
            var values = System.Enum.GetValues(typeof(LeaderboardCategory));
            Assert.AreEqual(4, values.Length);
        }

        [Test]
        public void LeaderboardCategory_ContainsGlobalAllTime()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(LeaderboardCategory), LeaderboardCategory.GlobalAllTime));
        }

        [Test]
        public void LeaderboardCategory_ContainsGlobalWeekly()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(LeaderboardCategory), LeaderboardCategory.GlobalWeekly));
        }

        [Test]
        public void LeaderboardCategory_ContainsGlobalDaily()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(LeaderboardCategory), LeaderboardCategory.GlobalDaily));
        }

        #endregion

        #region LeaderboardPage Tests

        [Test]
        public void LeaderboardPage_HasMorePages_TrueWhenMoreData()
        {
            var page = new LeaderboardPage
            {
                TotalCount = 100,
                PageNumber = 0,
                PageSize = 50
            };

            Assert.IsTrue(page.HasMorePages);
        }

        [Test]
        public void LeaderboardPage_HasMorePages_FalseWhenNoMoreData()
        {
            var page = new LeaderboardPage
            {
                TotalCount = 50,
                PageNumber = 0,
                PageSize = 50
            };

            Assert.IsFalse(page.HasMorePages);
        }

        [Test]
        public void LeaderboardPage_EntriesDefaultsToEmptyList()
        {
            var page = new LeaderboardPage();

            Assert.IsNotNull(page.Entries);
            Assert.AreEqual(0, page.Entries.Count);
        }

        #endregion

        #region CloudSaveData Tests

        [Test]
        public void CloudSaveData_DefaultConstructor_InitializesFields()
        {
            var save = new CloudSaveData();

            Assert.IsFalse(string.IsNullOrEmpty(save.saveId));
            Assert.IsFalse(string.IsNullOrEmpty(save.deviceId));
            Assert.Greater(save.timestamp, 0);
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void CloudSaveData_DefaultValues_AreCorrect()
        {
            var save = new CloudSaveData();

            Assert.AreEqual(0, save.totalCoins);
            Assert.AreEqual(0, save.highScore);
            Assert.AreEqual(1f, save.musicVolume);
            Assert.AreEqual(1f, save.sfxVolume);
            Assert.IsTrue(save.vibrateEnabled);
        }

        [Test]
        public void CloudSaveData_UnlockedLists_AreInitialized()
        {
            var save = new CloudSaveData();

            Assert.IsNotNull(save.unlockedCharacterIds);
            Assert.IsNotNull(save.unlockedAchievementIds);
        }

        #endregion

        #region ConflictResolution Tests

        [Test]
        public void ConflictResolution_HasExpectedValues()
        {
            var values = System.Enum.GetValues(typeof(ConflictResolution));
            Assert.AreEqual(4, values.Length);
        }

        #endregion

        #region AchievementCategory Tests

        [Test]
        public void AchievementCategory_HasExpectedCategories()
        {
            var values = System.Enum.GetValues(typeof(AchievementCategory));
            Assert.AreEqual(7, values.Length);
        }

        [Test]
        public void AchievementCategory_ContainsGameplay()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementCategory), AchievementCategory.Gameplay));
        }

        [Test]
        public void AchievementCategory_ContainsCollection()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementCategory), AchievementCategory.Collection));
        }

        #endregion

        #region AchievementRarity Tests

        [Test]
        public void AchievementRarity_HasFiveTiers()
        {
            var values = System.Enum.GetValues(typeof(AchievementRarity));
            Assert.AreEqual(5, values.Length);
        }

        [Test]
        public void AchievementRarity_OrderIsCorrect()
        {
            Assert.Less((int)AchievementRarity.Common, (int)AchievementRarity.Uncommon);
            Assert.Less((int)AchievementRarity.Uncommon, (int)AchievementRarity.Rare);
            Assert.Less((int)AchievementRarity.Rare, (int)AchievementRarity.Epic);
            Assert.Less((int)AchievementRarity.Epic, (int)AchievementRarity.Legendary);
        }

        #endregion

        #region AchievementType Tests

        [Test]
        public void AchievementType_HasExpectedTypes()
        {
            var values = System.Enum.GetValues(typeof(AchievementType));
            Assert.AreEqual(4, values.Length);
        }

        #endregion

        #region AchievementProgress Tests

        [Test]
        public void AchievementProgress_Constructor_SetsId()
        {
            var progress = new AchievementProgress("test_achievement");

            Assert.AreEqual("test_achievement", progress.achievementId);
            Assert.AreEqual(0, progress.currentValue);
            Assert.IsFalse(progress.isUnlocked);
        }

        [Test]
        public void AchievementProgress_GetProgressPercent_CalculatesCorrectly()
        {
            var progress = new AchievementProgress("test")
            {
                currentValue = 50
            };

            float percent = progress.GetProgressPercent(100);

            Assert.AreEqual(0.5f, percent, 0.001f);
        }

        [Test]
        public void AchievementProgress_GetProgressPercent_ClampsTo1()
        {
            var progress = new AchievementProgress("test")
            {
                currentValue = 150
            };

            float percent = progress.GetProgressPercent(100);

            Assert.AreEqual(1f, percent, 0.001f);
        }

        [Test]
        public void AchievementProgress_GetProgressPercent_HandlesZeroTarget()
        {
            var progress = new AchievementProgress("test")
            {
                currentValue = 50,
                isUnlocked = true
            };

            float percent = progress.GetProgressPercent(0);

            Assert.AreEqual(1f, percent, 0.001f);
        }

        #endregion

        #region DefaultAchievementsLibrary Tests

        [Test]
        public void DefaultAchievementsLibrary_GetDefaultAchievements_ReturnsNonEmptyList()
        {
            var achievements = DefaultAchievementsLibrary.GetDefaultAchievements();

            Assert.IsNotNull(achievements);
            Assert.Greater(achievements.Count, 0);
        }

        [Test]
        public void DefaultAchievementsLibrary_GetDefaultAchievements_HasUniqueIds()
        {
            var achievements = DefaultAchievementsLibrary.GetDefaultAchievements();
            var ids = new HashSet<string>();

            foreach (var achievement in achievements)
            {
                Assert.IsTrue(ids.Add(achievement.id), $"Duplicate ID found: {achievement.id}");
            }
        }

        [Test]
        public void DefaultAchievementsLibrary_GetDefaultAchievements_AllHaveNames()
        {
            var achievements = DefaultAchievementsLibrary.GetDefaultAchievements();

            foreach (var achievement in achievements)
            {
                Assert.IsFalse(string.IsNullOrEmpty(achievement.name), $"Achievement {achievement.id} has no name");
            }
        }

        [Test]
        public void DefaultAchievementsLibrary_GetDefaultAchievements_AllHaveDescriptions()
        {
            var achievements = DefaultAchievementsLibrary.GetDefaultAchievements();

            foreach (var achievement in achievements)
            {
                Assert.IsFalse(string.IsNullOrEmpty(achievement.description), $"Achievement {achievement.id} has no description");
            }
        }

        [Test]
        public void DefaultAchievementsLibrary_GetDefaultAchievements_AllHavePositiveTargets()
        {
            var achievements = DefaultAchievementsLibrary.GetDefaultAchievements();

            foreach (var achievement in achievements)
            {
                Assert.Greater(achievement.targetValue, 0, $"Achievement {achievement.id} has non-positive target");
            }
        }

        #endregion

        #region Component Tests

        [Test]
        public void LeaderboardService_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestLeaderboard");
            var service = gameObj.AddComponent<LeaderboardService>();

            Assert.IsNotNull(service);
            Assert.AreEqual("LeaderboardService", service.ServiceName);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void CloudSaveService_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestCloudSave");
            var service = gameObj.AddComponent<CloudSaveService>();

            Assert.IsNotNull(service);
            Assert.AreEqual("CloudSaveService", service.ServiceName);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void AchievementService_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestAchievements");
            var service = gameObj.AddComponent<AchievementService>();

            Assert.IsNotNull(service);
            Assert.AreEqual("AchievementService", service.ServiceName);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void ServiceManager_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestServiceManager");
            var manager = gameObj.AddComponent<ServiceManager>();

            Assert.IsNotNull(manager);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void AchievementTracker_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestTracker");
            var tracker = gameObj.AddComponent<AchievementTracker>();

            Assert.IsNotNull(tracker);

            Object.DestroyImmediate(gameObj);
        }

        #endregion
    }
}
