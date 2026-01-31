using NUnit.Framework;
using EscapeTrainRun.Utils;
using EscapeTrainRun.Core;
using UnityEngine;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for core utility functions.
    /// Following AAA pattern: Arrange, Act, Assert.
    /// </summary>
    [TestFixture]
    public class CoreUtilsTests
    {
        #region MathHelpers Tests

        [Test]
        public void ParabolicJump_AtMidpoint_ReturnsMaxHeight()
        {
            // Arrange
            float t = 0.5f;
            float maxHeight = 2.5f;

            // Act
            float result = MathHelpers.ParabolicJump(t, maxHeight);

            // Assert
            Assert.AreEqual(maxHeight, result, 0.001f);
        }

        [Test]
        public void ParabolicJump_AtStart_ReturnsZero()
        {
            // Arrange
            float t = 0f;
            float maxHeight = 2.5f;

            // Act
            float result = MathHelpers.ParabolicJump(t, maxHeight);

            // Assert
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void ParabolicJump_AtEnd_ReturnsZero()
        {
            // Arrange
            float t = 1f;
            float maxHeight = 2.5f;

            // Act
            float result = MathHelpers.ParabolicJump(t, maxHeight);

            // Assert
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void LaneToWorldX_CenterLane_ReturnsZero()
        {
            // Arrange
            int lane = 1; // Center lane

            // Act
            float result = MathHelpers.LaneToWorldX(lane);

            // Assert
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void LaneToWorldX_LeftLane_ReturnsNegativeLaneWidth()
        {
            // Arrange
            int lane = 0; // Left lane
            float laneWidth = Constants.LaneWidth;

            // Act
            float result = MathHelpers.LaneToWorldX(lane);

            // Assert
            Assert.AreEqual(-laneWidth, result);
        }

        [Test]
        public void LaneToWorldX_RightLane_ReturnsPositiveLaneWidth()
        {
            // Arrange
            int lane = 2; // Right lane
            float laneWidth = Constants.LaneWidth;

            // Act
            float result = MathHelpers.LaneToWorldX(lane);

            // Assert
            Assert.AreEqual(laneWidth, result);
        }

        [Test]
        public void WorldXToLane_AtCenter_ReturnsCenterLane()
        {
            // Arrange
            float worldX = 0f;

            // Act
            int result = MathHelpers.WorldXToLane(worldX);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void RandomChance_WithZeroProbability_ReturnsFalse()
        {
            // Arrange & Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.IsFalse(MathHelpers.RandomChance(0f));
            }
        }

        [Test]
        public void RandomChance_WithFullProbability_ReturnsTrue()
        {
            // Arrange & Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(MathHelpers.RandomChance(1f));
            }
        }

        [Test]
        public void FormatScore_WithThousands_IncludesSeparator()
        {
            // Arrange
            int score = 1234567;

            // Act
            string result = MathHelpers.FormatScore(score);

            // Assert
            Assert.IsTrue(result.Contains(",") || result.Contains(".") || result.Contains(" "));
        }

        [Test]
        public void FormatDistance_UnderKilometer_ReturnsMeters()
        {
            // Arrange
            float distance = 500f;

            // Act
            string result = MathHelpers.FormatDistance(distance);

            // Assert
            Assert.IsTrue(result.EndsWith("m"));
            Assert.IsFalse(result.Contains("km"));
        }

        [Test]
        public void FormatDistance_OverKilometer_ReturnsKilometers()
        {
            // Arrange
            float distance = 1500f;

            // Act
            string result = MathHelpers.FormatDistance(distance);

            // Assert
            Assert.IsTrue(result.EndsWith("km"));
        }

        [Test]
        public void ClampLane_WithNegativeValue_ReturnsZero()
        {
            // Arrange
            int lane = -1;

            // Act
            int result = MathHelpers.ClampLane(lane);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void ClampLane_WithExcessiveValue_ReturnsMaxLane()
        {
            // Arrange
            int lane = 10;

            // Act
            int result = MathHelpers.ClampLane(lane);

            // Assert
            Assert.AreEqual(Constants.LaneCount - 1, result);
        }

        #endregion

        #region ObjectPool Tests

        [Test]
        public void ObjectPool_Get_CreatesNewItemWhenEmpty()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolItem>(
                createFunc: () => new TestPoolItem()
            );

            // Act
            var item = pool.Get();

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(1, pool.CountAll);
            Assert.AreEqual(1, pool.CountActive);
        }

        [Test]
        public void ObjectPool_Release_ReturnsItemToPool()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolItem>(
                createFunc: () => new TestPoolItem()
            );
            var item = pool.Get();

            // Act
            pool.Release(item);

            // Assert
            Assert.AreEqual(1, pool.CountInactive);
            Assert.AreEqual(0, pool.CountActive);
        }

        [Test]
        public void ObjectPool_GetAfterRelease_ReusesSameItem()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolItem>(
                createFunc: () => new TestPoolItem()
            );
            var item1 = pool.Get();
            pool.Release(item1);

            // Act
            var item2 = pool.Get();

            // Assert
            Assert.AreSame(item1, item2);
            Assert.AreEqual(1, pool.CountAll);
        }

        [Test]
        public void ObjectPool_Prewarm_CreatesSpecifiedCount()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolItem>(
                createFunc: () => new TestPoolItem()
            );

            // Act
            pool.Prewarm(5);

            // Assert
            Assert.AreEqual(5, pool.CountAll);
            Assert.AreEqual(5, pool.CountInactive);
        }

        [Test]
        public void ObjectPool_Clear_RemovesAllItems()
        {
            // Arrange
            var pool = new ObjectPool<TestPoolItem>(
                createFunc: () => new TestPoolItem()
            );
            pool.Prewarm(5);

            // Act
            pool.Clear();

            // Assert
            Assert.AreEqual(0, pool.CountAll);
            Assert.AreEqual(0, pool.CountInactive);
        }

        // Helper class for pool testing
        private class TestPoolItem
        {
            public int Value { get; set; }
        }

        #endregion

        #region ServiceLocator Tests

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
        }

        [Test]
        public void ServiceLocator_Register_AddsService()
        {
            // Arrange
            var service = new TestService();

            // Act
            ServiceLocator.Register(service);

            // Assert
            Assert.IsTrue(ServiceLocator.IsRegistered<TestService>());
        }

        [Test]
        public void ServiceLocator_Get_ReturnsRegisteredService()
        {
            // Arrange
            var service = new TestService { Value = 42 };
            ServiceLocator.Register(service);

            // Act
            var retrieved = ServiceLocator.Get<TestService>();

            // Assert
            Assert.AreSame(service, retrieved);
            Assert.AreEqual(42, retrieved.Value);
        }

        [Test]
        public void ServiceLocator_TryGet_ReturnsTrueForRegistered()
        {
            // Arrange
            var service = new TestService();
            ServiceLocator.Register(service);

            // Act
            bool result = ServiceLocator.TryGet<TestService>(out var retrieved);

            // Assert
            Assert.IsTrue(result);
            Assert.AreSame(service, retrieved);
        }

        [Test]
        public void ServiceLocator_TryGet_ReturnsFalseForUnregistered()
        {
            // Arrange - no service registered

            // Act
            bool result = ServiceLocator.TryGet<TestService>(out var retrieved);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrieved);
        }

        [Test]
        public void ServiceLocator_Unregister_RemovesService()
        {
            // Arrange
            var service = new TestService();
            ServiceLocator.Register(service);

            // Act
            ServiceLocator.Unregister<TestService>();

            // Assert
            Assert.IsFalse(ServiceLocator.IsRegistered<TestService>());
        }

        // Helper class for service locator testing
        private class TestService
        {
            public int Value { get; set; }
        }

        #endregion

        #region Constants Tests

        [Test]
        public void Constants_LaneCount_IsThree()
        {
            Assert.AreEqual(3, Constants.LaneCount);
        }

        [Test]
        public void Constants_CenterLane_IsOne()
        {
            Assert.AreEqual(1, Constants.CenterLane);
        }

        [Test]
        public void Constants_MaxSpeed_GreaterThanBaseSpeed()
        {
            Assert.Greater(Constants.MaxRunSpeed, Constants.BaseRunSpeed);
        }

        [Test]
        public void Constants_JumpDuration_IsPositive()
        {
            Assert.Greater(Constants.JumpDuration, 0f);
        }

        [Test]
        public void Constants_SlideDuration_IsPositive()
        {
            Assert.Greater(Constants.SlideDuration, 0f);
        }

        #endregion
    }
}
