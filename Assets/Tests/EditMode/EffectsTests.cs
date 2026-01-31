using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Effects;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Effects system.
    /// </summary>
    [TestFixture]
    public class EffectsTests
    {
        #region VFXType Tests

        [Test]
        public void VFXType_HasExpectedCategories()
        {
            // Assert - Collectibles
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.CoinCollect));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.SuperCoinCollect));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.PowerUpCollect));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.MysteryBox));

            // Assert - Power-Ups
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.MagnetField));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.ShieldBubble));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.SpeedBoostTrail));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.StarPowerGlow));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.Multiplier));

            // Assert - Player Actions
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.JumpDust));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.LandingDust));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.SlideDust));

            // Assert - Impacts
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.Crash));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.ShieldBreak));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VFXType), VFXType.NearMiss));
        }

        [Test]
        public void VFXType_HasTwentyValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(VFXType));

            // Assert
            Assert.AreEqual(20, values.Length);
        }

        #endregion

        #region ShakePreset Tests

        [Test]
        public void ShakePreset_CanBeCreated()
        {
            // Arrange & Act
            var preset = new ShakePreset
            {
                intensity = 0.5f,
                duration = 0.3f,
                frequency = 15f
            };

            // Assert
            Assert.AreEqual(0.5f, preset.intensity);
            Assert.AreEqual(0.3f, preset.duration);
            Assert.AreEqual(15f, preset.frequency);
        }

        [Test]
        public void ShakePreset_HasDefaultValues()
        {
            // Arrange & Act
            var preset = new ShakePreset();

            // Assert - defaults are 0 for value types
            Assert.AreEqual(0f, preset.intensity);
            Assert.AreEqual(0f, preset.duration);
            Assert.AreEqual(0f, preset.frequency);
        }

        #endregion

        #region Component Creation Tests

        [Test]
        public void VFXManager_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestVFX");

            // Act
            var vfxManager = gameObj.AddComponent<VFXManager>();

            // Assert
            Assert.IsNotNull(vfxManager);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void ScreenShakeController_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestShake");

            // Act
            var shake = gameObj.AddComponent<ScreenShakeController>();

            // Assert
            Assert.IsNotNull(shake);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void ScreenFlashController_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestFlash");

            // Act
            var flash = gameObj.AddComponent<ScreenFlashController>();

            // Assert
            Assert.IsNotNull(flash);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void PostProcessingController_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestPostProcess");

            // Act
            var postProcess = gameObj.AddComponent<PostProcessingController>();

            // Assert
            Assert.IsNotNull(postProcess);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void SlowMotionController_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestSlowMo");

            // Act
            var slowMo = gameObj.AddComponent<SlowMotionController>();

            // Assert
            Assert.IsNotNull(slowMo);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void CameraEffects_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestCameraFX");

            // Act
            var cameraFx = gameObj.AddComponent<CameraEffects>();

            // Assert
            Assert.IsNotNull(cameraFx);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void MaterialFlasher_CanBeAddedToGameObject()
        {
            // Arrange
            var gameObj = new GameObject("TestMaterialFlash");

            // Act
            var flasher = gameObj.AddComponent<MaterialFlasher>();

            // Assert
            Assert.IsNotNull(flasher);

            // Cleanup
            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void TrailEffect_RequiresTrailRenderer()
        {
            // The component requires TrailRenderer
            var attributes = typeof(TrailEffect).GetCustomAttributes(typeof(RequireComponent), true);
            Assert.Greater(attributes.Length, 0);

            var requireComponent = attributes[0] as RequireComponent;
            Assert.AreEqual(typeof(TrailRenderer), requireComponent.m_Type0);
        }

        [Test]
        public void ParticleSystemController_RequiresParticleSystem()
        {
            // The component requires ParticleSystem
            var attributes = typeof(ParticleSystemController).GetCustomAttributes(typeof(RequireComponent), true);
            Assert.Greater(attributes.Length, 0);

            var requireComponent = attributes[0] as RequireComponent;
            Assert.AreEqual(typeof(ParticleSystem), requireComponent.m_Type0);
        }

        #endregion

        #region Color Tests

        [Test]
        public void FlashColors_HaveCorrectAlpha()
        {
            // Flash colors should have some transparency
            Color damageColor = new Color(1f, 0f, 0f, 0.5f);
            Color powerUpColor = new Color(1f, 1f, 0f, 0.4f);

            Assert.Greater(damageColor.a, 0f);
            Assert.Less(damageColor.a, 1f);
            Assert.Greater(powerUpColor.a, 0f);
            Assert.Less(powerUpColor.a, 1f);
        }

        #endregion

        #region Time Scale Tests

        [Test]
        public void SlowMotionScale_IsValid()
        {
            // Slow motion should be between 0 and 1
            float slowMoScale = 0.3f;

            Assert.Greater(slowMoScale, 0f);
            Assert.Less(slowMoScale, 1f);
        }

        [Test]
        public void CrashSlowMo_IsSlowestPreset()
        {
            // Crash should be slower than near miss
            float crashSlowMo = 0.2f;
            float nearMissSlowMo = 0.5f;

            Assert.Less(crashSlowMo, nearMissSlowMo);
        }

        #endregion

        #region FOV Tests

        [Test]
        public void FOVValues_AreInValidRange()
        {
            // FOV should be between reasonable camera limits
            float normalFOV = 60f;
            float speedBoostFOV = 75f;
            float starPowerFOV = 80f;

            Assert.Greater(normalFOV, 30f);
            Assert.Less(normalFOV, 120f);
            Assert.Greater(speedBoostFOV, normalFOV);
            Assert.Greater(starPowerFOV, speedBoostFOV);
        }

        #endregion
    }
}
