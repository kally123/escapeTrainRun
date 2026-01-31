using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Collectibles;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Power-Up system.
    /// </summary>
    [TestFixture]
    public class PowerUpTests
    {
        #region PowerUpType Tests

        [Test]
        public void PowerUpType_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)PowerUpType.Magnet);
            Assert.AreEqual(1, (int)PowerUpType.Shield);
            Assert.AreEqual(2, (int)PowerUpType.SpeedBoost);
            Assert.AreEqual(3, (int)PowerUpType.StarPower);
            Assert.AreEqual(4, (int)PowerUpType.Multiplier);
            Assert.AreEqual(5, (int)PowerUpType.MysteryBox);
        }

        [Test]
        public void PowerUpType_HasSixValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(PowerUpType));

            // Assert
            Assert.AreEqual(6, values.Length);
        }

        #endregion

        #region Duration Constants Tests

        [Test]
        public void MagnetDuration_IsPositive()
        {
            // Assert
            Assert.Greater(Constants.MagnetDuration, 0f);
        }

        [Test]
        public void ShieldDuration_IsPositive()
        {
            // Assert
            Assert.Greater(Constants.ShieldDuration, 0f);
        }

        [Test]
        public void SpeedBoostDuration_IsPositive()
        {
            // Assert
            Assert.Greater(Constants.SpeedBoostDuration, 0f);
        }

        [Test]
        public void StarPowerDuration_IsPositive()
        {
            // Assert
            Assert.Greater(Constants.StarPowerDuration, 0f);
        }

        [Test]
        public void MultiplierDuration_IsPositive()
        {
            // Assert
            Assert.Greater(Constants.MultiplierDuration, 0f);
        }

        [Test]
        public void MultiplierValue_IsAtLeastTwo()
        {
            // Assert
            Assert.GreaterOrEqual(Constants.MultiplierValue, 2);
        }

        #endregion

        #region PowerUpDisplay Tests

        [Test]
        public void GetPowerUpName_ReturnsNonEmptyString()
        {
            // Arrange
            var types = System.Enum.GetValues(typeof(PowerUpType));

            // Act & Assert
            foreach (PowerUpType type in types)
            {
                string name = PowerUpDisplay.GetPowerUpName(type);
                Assert.IsNotEmpty(name, $"Power-up {type} should have a name");
            }
        }

        [Test]
        public void GetPowerUpDescription_ReturnsNonEmptyString()
        {
            // Arrange
            var types = System.Enum.GetValues(typeof(PowerUpType));

            // Act & Assert
            foreach (PowerUpType type in types)
            {
                string description = PowerUpDisplay.GetPowerUpDescription(type);
                Assert.IsNotEmpty(description, $"Power-up {type} should have a description");
            }
        }

        [Test]
        public void GetPowerUpName_Magnet_ReturnsCorrectName()
        {
            // Act
            string name = PowerUpDisplay.GetPowerUpName(PowerUpType.Magnet);

            // Assert
            Assert.AreEqual("Coin Magnet", name);
        }

        [Test]
        public void GetPowerUpName_Shield_ReturnsCorrectName()
        {
            // Act
            string name = PowerUpDisplay.GetPowerUpName(PowerUpType.Shield);

            // Assert
            Assert.AreEqual("Shield", name);
        }

        [Test]
        public void GetPowerUpName_SpeedBoost_ReturnsCorrectName()
        {
            // Act
            string name = PowerUpDisplay.GetPowerUpName(PowerUpType.SpeedBoost);

            // Assert
            Assert.AreEqual("Speed Boost", name);
        }

        [Test]
        public void GetPowerUpName_StarPower_ReturnsCorrectName()
        {
            // Act
            string name = PowerUpDisplay.GetPowerUpName(PowerUpType.StarPower);

            // Assert
            Assert.AreEqual("Star Power", name);
        }

        [Test]
        public void GetPowerUpName_Multiplier_ReturnsCorrectName()
        {
            // Act
            string name = PowerUpDisplay.GetPowerUpName(PowerUpType.Multiplier);

            // Assert
            Assert.AreEqual("2x Multiplier", name);
        }

        [Test]
        public void GetPowerUpName_MysteryBox_ReturnsCorrectName()
        {
            // Act
            string name = PowerUpDisplay.GetPowerUpName(PowerUpType.MysteryBox);

            // Assert
            Assert.AreEqual("Mystery Box", name);
        }

        #endregion

        #region ActivePowerUp Tests

        [Test]
        public void ActivePowerUp_CanBeCreated()
        {
            // Arrange & Act
            var activePowerUp = new ActivePowerUp
            {
                Type = PowerUpType.Magnet,
                Duration = 10f,
                RemainingTime = 10f
            };

            // Assert
            Assert.AreEqual(PowerUpType.Magnet, activePowerUp.Type);
            Assert.AreEqual(10f, activePowerUp.Duration);
            Assert.AreEqual(10f, activePowerUp.RemainingTime);
            Assert.IsFalse(activePowerUp.WarningPlayed);
        }

        [Test]
        public void ActivePowerUp_RemainingTimeCanBeModified()
        {
            // Arrange
            var activePowerUp = new ActivePowerUp
            {
                Type = PowerUpType.Shield,
                Duration = 10f,
                RemainingTime = 10f
            };

            // Act
            activePowerUp.RemainingTime = 5f;

            // Assert
            Assert.AreEqual(5f, activePowerUp.RemainingTime);
        }

        [Test]
        public void ActivePowerUp_WarningCanBeSet()
        {
            // Arrange
            var activePowerUp = new ActivePowerUp();

            // Act
            activePowerUp.WarningPlayed = true;

            // Assert
            Assert.IsTrue(activePowerUp.WarningPlayed);
        }

        #endregion

        #region Coin Component Tests

        [Test]
        public void Coin_CanBeInstantiated()
        {
            // Arrange
            var coinObj = new GameObject("TestCoin");
            
            // Act
            var coin = coinObj.AddComponent<Coin>();

            // Assert
            Assert.IsNotNull(coin);
            Assert.IsFalse(coin.IsCollected);
            Assert.IsFalse(coin.IsBeingAttracted);

            // Cleanup
            Object.DestroyImmediate(coinObj);
        }

        #endregion

        #region PowerUp Component Tests

        [Test]
        public void PowerUp_CanBeInstantiated()
        {
            // Arrange
            var powerUpObj = new GameObject("TestPowerUp");

            // Act
            var powerUp = powerUpObj.AddComponent<PowerUp>();

            // Assert
            Assert.IsNotNull(powerUp);
            Assert.IsFalse(powerUp.IsCollected);

            // Cleanup
            Object.DestroyImmediate(powerUpObj);
        }

        #endregion
    }
}
