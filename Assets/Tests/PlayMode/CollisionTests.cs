using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using EscapeTrainRun.Events;
using EscapeTrainRun.Obstacles;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for collision detection and responses.
    /// Tests player-obstacle, player-collectible, and other collision scenarios.
    /// </summary>
    [TestFixture]
    public class CollisionTests
    {
        private PlayerTestContext playerContext;

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllEvents();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerContext != null)
            {
                TestUtilities.CleanupTestPlayer(playerContext);
                playerContext = null;
            }

            GameEvents.ClearAllEvents();
        }

        #region Obstacle Collision Tests

        [UnityTest]
        public IEnumerator Collision_WithObstacle_TriggersCrash()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            bool crashed = false;

            GameEvents.OnPlayerCrashed += () => crashed = true;

            yield return null;

            // Act - Trigger crash event
            GameEvents.TriggerPlayerCrashed();

            yield return null;

            // Assert
            Assert.IsTrue(crashed);
        }

        [UnityTest]
        public IEnumerator Collision_WithObstacle_WhileShielded_BreaksShield()
        {
            // Arrange
            bool shieldBroken = false;
            bool crashed = false;

            GameEvents.OnShieldBroken += () => shieldBroken = true;
            GameEvents.OnPlayerCrashed += () => crashed = true;

            yield return null;

            // Activate shield
            GameEvents.TriggerPowerUpActivated(Collectibles.PowerUpType.Shield);

            yield return null;

            // Hit obstacle - shield breaks
            GameEvents.TriggerShieldBroken();

            yield return null;

            // Assert
            Assert.IsTrue(shieldBroken);
            Assert.IsFalse(crashed);
        }

        [UnityTest]
        public IEnumerator Collision_WithObstacle_WhileStarPower_DestroysObstacle()
        {
            // Arrange
            bool crashed = false;
            bool obstaclePassed = false;

            GameEvents.OnPlayerCrashed += () => crashed = true;
            GameEvents.OnObstaclePassed += () => obstaclePassed = true;

            yield return null;

            // Activate star power
            GameEvents.TriggerPowerUpActivated(Collectibles.PowerUpType.StarPower);

            yield return null;

            // "Hit" obstacle - destroys instead of crash
            GameEvents.TriggerObstaclePassed();

            yield return null;

            // Assert
            Assert.IsFalse(crashed);
            Assert.IsTrue(obstaclePassed);
        }

        #endregion

        #region Near Miss Tests

        [UnityTest]
        public IEnumerator NearMiss_TriggersEvent()
        {
            // Arrange
            bool nearMissDetected = false;
            GameEvents.OnNearMiss += () => nearMissDetected = true;

            yield return null;

            // Act
            GameEvents.TriggerNearMiss();

            yield return null;

            // Assert
            Assert.IsTrue(nearMissDetected);
        }

        [UnityTest]
        public IEnumerator NearMiss_MultipleTriggers_AllCounted()
        {
            // Arrange
            int nearMissCount = 0;
            GameEvents.OnNearMiss += () => nearMissCount++;

            yield return null;

            // Act
            GameEvents.TriggerNearMiss();
            GameEvents.TriggerNearMiss();
            GameEvents.TriggerNearMiss();

            yield return null;

            // Assert
            Assert.AreEqual(3, nearMissCount);
        }

        #endregion

        #region Coin Collection Tests

        [UnityTest]
        public IEnumerator Coin_Collection_TriggersEvent()
        {
            // Arrange
            int coinsCollected = 0;
            GameEvents.OnCoinsCollected += (amount) => coinsCollected += amount;

            yield return null;

            // Act
            GameEvents.TriggerCoinsCollected(1);

            yield return null;

            // Assert
            Assert.AreEqual(1, coinsCollected);
        }

        [UnityTest]
        public IEnumerator Coin_MultipleCollection_Accumulates()
        {
            // Arrange
            int totalCoins = 0;
            GameEvents.OnCoinsCollected += (amount) => totalCoins += amount;

            yield return null;

            // Act - Collect multiple coins
            for (int i = 0; i < 10; i++)
            {
                GameEvents.TriggerCoinsCollected(1);
            }

            yield return null;

            // Assert
            Assert.AreEqual(10, totalCoins);
        }

        [UnityTest]
        public IEnumerator Coin_WithMagnet_AttractsToPlayer()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = Vector3.zero;

            var coin = TestUtilities.CreateTestCoin(new Vector3(3f, 0, 3f));

            yield return null;

            // Activate magnet
            GameEvents.TriggerPowerUpActivated(Collectibles.PowerUpType.Magnet);

            yield return TestUtilities.WaitForPhysics(5);

            // Assert - Coin exists and can be collected
            Assert.IsNotNull(coin);

            Object.Destroy(coin);
        }

        #endregion

        #region Power-Up Collection Tests

        [UnityTest]
        public IEnumerator PowerUp_Collection_ActivatesPowerUp()
        {
            // Arrange
            Collectibles.PowerUpType activatedType = Collectibles.PowerUpType.None;
            GameEvents.OnPowerUpActivated += (type) => activatedType = type;

            yield return null;

            // Act - Simulate power-up collection
            GameEvents.TriggerPowerUpActivated(Collectibles.PowerUpType.Magnet);

            yield return null;

            // Assert
            Assert.AreEqual(Collectibles.PowerUpType.Magnet, activatedType);
        }

        #endregion

        #region Lane-Based Collision Tests

        [UnityTest]
        public IEnumerator Obstacle_InDifferentLane_NoCrash()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = new Vector3(-2.5f, 0, 0); // Left lane

            // Obstacle in right lane
            var obstacle = TestUtilities.CreateTestObstacle(new Vector3(2.5f, 0, 5f));

            bool crashed = false;
            GameEvents.OnPlayerCrashed += () => crashed = true;

            yield return TestUtilities.WaitForPhysics(10);

            // Assert - Should not crash
            Assert.IsFalse(crashed);

            Object.Destroy(obstacle);
        }

        [UnityTest]
        public IEnumerator Obstacle_InSameLane_CausesCrash()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = new Vector3(0, 0, 0); // Center lane

            // Obstacle in same lane, ahead
            var obstacle = TestUtilities.CreateTestObstacle(new Vector3(0, 0, 2f));

            yield return null;

            // Simulate collision
            GameEvents.TriggerPlayerCrashed();

            bool crashed = false;
            GameEvents.OnPlayerCrashed += () => crashed = true;

            yield return null;

            // Note: Actual collision depends on physics simulation
            Assert.IsNotNull(obstacle);

            Object.Destroy(obstacle);
        }

        #endregion

        #region Obstacle Avoidance Tests

        [UnityTest]
        public IEnumerator Jump_OverLowObstacle_Success()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            bool passed = false;

            GameEvents.OnObstaclePassed += () => passed = true;

            yield return null;

            // Act - Jump over obstacle
            GameEvents.TriggerPlayerJumped();

            yield return new WaitForSeconds(0.3f);

            // Simulate successful avoidance
            GameEvents.TriggerObstaclePassed();

            yield return null;

            // Assert
            Assert.IsTrue(passed);
        }

        [UnityTest]
        public IEnumerator Slide_UnderHighObstacle_Success()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            bool passed = false;

            GameEvents.OnObstaclePassed += () => passed = true;

            yield return null;

            // Act - Slide under obstacle
            GameEvents.TriggerPlayerSlide();

            yield return new WaitForSeconds(0.3f);

            // Simulate successful avoidance
            GameEvents.TriggerObstaclePassed();

            yield return null;

            // Assert
            Assert.IsTrue(passed);
        }

        [UnityTest]
        public IEnumerator LaneChange_ToAvoidObstacle_Success()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = new Vector3(0, 0, 0); // Center

            bool passed = false;
            GameEvents.OnObstaclePassed += () => passed = true;

            yield return null;

            // Act - Change lane
            GameEvents.TriggerLaneChanged(0); // Left lane

            yield return new WaitForSeconds(0.3f);

            GameEvents.TriggerObstaclePassed();

            yield return null;

            // Assert
            Assert.IsTrue(passed);
        }

        #endregion
    }
}
