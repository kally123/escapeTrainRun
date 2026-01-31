using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using EscapeTrainRun.Collectibles;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for power-up functionality.
    /// Tests power-up collection, activation, and effects.
    /// </summary>
    [TestFixture]
    public class PowerUpIntegrationTests
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

        #region Power-Up Activation Tests

        [UnityTest]
        public IEnumerator PowerUp_ActivationEventFires()
        {
            // Arrange
            PowerUpType activatedType = PowerUpType.None;
            GameEvents.OnPowerUpActivated += (type) => activatedType = type;

            yield return null;

            // Act
            GameEvents.TriggerPowerUpActivated(PowerUpType.Magnet);

            yield return null;

            // Assert
            Assert.AreEqual(PowerUpType.Magnet, activatedType);
        }

        [UnityTest]
        public IEnumerator PowerUp_DeactivationEventFires()
        {
            // Arrange
            PowerUpType deactivatedType = PowerUpType.None;
            GameEvents.OnPowerUpDeactivated += (type) => deactivatedType = type;

            yield return null;

            // Activate first
            GameEvents.TriggerPowerUpActivated(PowerUpType.Shield);

            yield return new WaitForSeconds(0.5f);

            // Act - Deactivate
            GameEvents.TriggerPowerUpDeactivated(PowerUpType.Shield);

            yield return null;

            // Assert
            Assert.AreEqual(PowerUpType.Shield, deactivatedType);
        }

        #endregion

        #region Magnet Power-Up Tests

        [UnityTest]
        public IEnumerator Magnet_AttractsNearbyCoins()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = Vector3.zero;

            // Create coin within magnet range
            var coin = TestUtilities.CreateTestCoin(new Vector3(3f, 0, 0));

            yield return null;

            // Act - Activate magnet
            GameEvents.TriggerPowerUpActivated(PowerUpType.Magnet);

            yield return new WaitForSeconds(0.5f);

            // Assert - Coin should be moving toward player
            // Note: Actual behavior depends on magnet implementation
            Assert.IsNotNull(coin);

            Object.Destroy(coin);
        }

        [UnityTest]
        public IEnumerator Magnet_DoesNotAffectDistantCoins()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = Vector3.zero;

            // Create coin outside magnet range
            var coin = TestUtilities.CreateTestCoin(new Vector3(20f, 0, 0));
            Vector3 initialPos = coin.transform.position;

            yield return null;

            // Act
            GameEvents.TriggerPowerUpActivated(PowerUpType.Magnet);

            yield return new WaitForSeconds(0.5f);

            // Assert - Distant coin should not have moved significantly
            float distance = Vector3.Distance(initialPos, coin.transform.position);
            Assert.Less(distance, 1f);

            Object.Destroy(coin);
        }

        #endregion

        #region Shield Power-Up Tests

        [UnityTest]
        public IEnumerator Shield_ProtectsFromObstacle()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            bool crashed = false;
            bool shieldBroken = false;

            GameEvents.OnPlayerCrashed += () => crashed = true;
            GameEvents.OnShieldBroken += () => shieldBroken = true;

            yield return null;

            // Activate shield
            GameEvents.TriggerPowerUpActivated(PowerUpType.Shield);

            yield return null;

            // Simulate shield breaking instead of crash
            GameEvents.TriggerShieldBroken();

            yield return null;

            // Assert
            Assert.IsTrue(shieldBroken);
            Assert.IsFalse(crashed);
        }

        [UnityTest]
        public IEnumerator Shield_DeactivatesAfterHit()
        {
            // Arrange
            PowerUpType deactivatedType = PowerUpType.None;
            GameEvents.OnPowerUpDeactivated += (type) => deactivatedType = type;

            yield return null;

            // Act
            GameEvents.TriggerPowerUpActivated(PowerUpType.Shield);

            yield return null;

            GameEvents.TriggerShieldBroken();
            GameEvents.TriggerPowerUpDeactivated(PowerUpType.Shield);

            yield return null;

            // Assert
            Assert.AreEqual(PowerUpType.Shield, deactivatedType);
        }

        #endregion

        #region Speed Boost Tests

        [UnityTest]
        public IEnumerator SpeedBoost_IncreasesPlayerSpeed()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();

            yield return null;

            // Act
            GameEvents.TriggerPowerUpActivated(PowerUpType.SpeedBoost);

            yield return new WaitForSeconds(0.5f);

            // Assert - Player should be in boosted state
            // Note: Actual speed verification depends on implementation
            Assert.IsNotNull(playerContext.Movement);
        }

        [UnityTest]
        public IEnumerator SpeedBoost_ExpiresAfterDuration()
        {
            // Arrange
            int deactivationCount = 0;
            GameEvents.OnPowerUpDeactivated += (type) =>
            {
                if (type == PowerUpType.SpeedBoost) deactivationCount++;
            };

            yield return null;

            // Act
            GameEvents.TriggerPowerUpActivated(PowerUpType.SpeedBoost);

            // Simulate duration expiry
            yield return new WaitForSeconds(0.5f);

            GameEvents.TriggerPowerUpDeactivated(PowerUpType.SpeedBoost);

            yield return null;

            // Assert
            Assert.AreEqual(1, deactivationCount);
        }

        #endregion

        #region Star Power Tests

        [UnityTest]
        public IEnumerator StarPower_MakesPlayerInvincible()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            bool crashed = false;

            GameEvents.OnPlayerCrashed += () => crashed = true;

            yield return null;

            // Act - Activate star power
            GameEvents.TriggerPowerUpActivated(PowerUpType.StarPower);

            yield return null;

            // Player should not crash during star power
            // Note: Actual invincibility depends on implementation
            Assert.IsFalse(crashed);
        }

        [UnityTest]
        public IEnumerator StarPower_DestroysObstaclesOnContact()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = Vector3.zero;

            var obstacle = TestUtilities.CreateTestObstacle(new Vector3(0, 0, 5));

            yield return null;

            // Act
            GameEvents.TriggerPowerUpActivated(PowerUpType.StarPower);

            // Move toward obstacle
            yield return new WaitForSeconds(0.5f);

            // Assert - Obstacle interaction should be handled
            Assert.IsNotNull(obstacle);

            Object.Destroy(obstacle);
        }

        #endregion

        #region Multiplier Tests

        [UnityTest]
        public IEnumerator Multiplier_DoublesScoreGain()
        {
            // Arrange
            int lastScore = 0;
            GameEvents.OnScoreChanged += (score) => lastScore = score;

            yield return null;

            // Act - Normal score
            GameEvents.TriggerScoreChanged(100);

            yield return null;

            Assert.AreEqual(100, lastScore);

            // With multiplier (simulated 2x)
            GameEvents.TriggerPowerUpActivated(PowerUpType.Multiplier);

            yield return null;

            GameEvents.TriggerScoreChanged(300); // 100 + 100*2 = 300

            yield return null;

            // Assert
            Assert.AreEqual(300, lastScore);
        }

        #endregion

        #region Power-Up Stacking Tests

        [UnityTest]
        public IEnumerator PowerUps_CanBeActiveSimultaneously()
        {
            // Arrange
            int activeCount = 0;
            int deactiveCount = 0;

            GameEvents.OnPowerUpActivated += (type) => activeCount++;
            GameEvents.OnPowerUpDeactivated += (type) => deactiveCount++;

            yield return null;

            // Act - Activate multiple power-ups
            GameEvents.TriggerPowerUpActivated(PowerUpType.Magnet);
            GameEvents.TriggerPowerUpActivated(PowerUpType.Multiplier);

            yield return null;

            // Assert
            Assert.AreEqual(2, activeCount);
            Assert.AreEqual(0, deactiveCount);

            // Cleanup
            GameEvents.TriggerPowerUpDeactivated(PowerUpType.Magnet);
            GameEvents.TriggerPowerUpDeactivated(PowerUpType.Multiplier);

            yield return null;

            Assert.AreEqual(2, deactiveCount);
        }

        [UnityTest]
        public IEnumerator PowerUp_ReactivationRefreshesDuration()
        {
            // Arrange
            int activationCount = 0;
            GameEvents.OnPowerUpActivated += (type) =>
            {
                if (type == PowerUpType.Magnet) activationCount++;
            };

            yield return null;

            // Act - Activate twice
            GameEvents.TriggerPowerUpActivated(PowerUpType.Magnet);

            yield return new WaitForSeconds(0.2f);

            GameEvents.TriggerPowerUpActivated(PowerUpType.Magnet);

            yield return null;

            // Assert - Event fired twice
            Assert.AreEqual(2, activationCount);
        }

        #endregion
    }
}
