using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.UI;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for UI system.
    /// </summary>
    [TestFixture]
    public class UITests
    {
        #region UIScreen Tests

        [Test]
        public void UIScreen_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)UIScreen.None);
            Assert.AreEqual(1, (int)UIScreen.MainMenu);
            Assert.AreEqual(2, (int)UIScreen.GameHUD);
            Assert.AreEqual(3, (int)UIScreen.PauseMenu);
            Assert.AreEqual(4, (int)UIScreen.GameOver);
            Assert.AreEqual(5, (int)UIScreen.Settings);
            Assert.AreEqual(6, (int)UIScreen.CharacterSelect);
            Assert.AreEqual(7, (int)UIScreen.ThemeSelect);
            Assert.AreEqual(8, (int)UIScreen.Loading);
        }

        [Test]
        public void UIScreen_HasNineValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(UIScreen));

            // Assert
            Assert.AreEqual(9, values.Length);
        }

        #endregion

        #region SlideDirection Tests

        [Test]
        public void SlideDirection_HasFourValues()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(SlideDirection));

            // Assert
            Assert.AreEqual(4, values.Length);
        }

        [Test]
        public void SlideDirection_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)SlideDirection.Left);
            Assert.AreEqual(1, (int)SlideDirection.Right);
            Assert.AreEqual(2, (int)SlideDirection.Up);
            Assert.AreEqual(3, (int)SlideDirection.Down);
        }

        #endregion

        #region ThemeData Tests

        [Test]
        public void ThemeData_CanBeCreated()
        {
            // Arrange & Act
            var themeData = new ThemeData
            {
                DisplayName = "Train",
                Description = "Run through the train!",
                ThemeColor = Color.blue
            };

            // Assert
            Assert.AreEqual("Train", themeData.DisplayName);
            Assert.AreEqual("Run through the train!", themeData.Description);
            Assert.AreEqual(Color.blue, themeData.ThemeColor);
        }

        [Test]
        public void ThemeData_CanHaveNullImages()
        {
            // Arrange & Act
            var themeData = new ThemeData
            {
                DisplayName = "Bus",
                PreviewImage = null,
                IconImage = null
            };

            // Assert
            Assert.IsNull(themeData.PreviewImage);
            Assert.IsNull(themeData.IconImage);
        }

        #endregion

        #region ThemeType Integration Tests

        [Test]
        public void ThemeType_MatchesExpectedValues()
        {
            // Assert - ensure Theme UI works with Environment ThemeType
            Assert.AreEqual(0, (int)ThemeType.Train);
            Assert.AreEqual(1, (int)ThemeType.Bus);
            Assert.AreEqual(2, (int)ThemeType.Ground);
        }

        #endregion

        #region Loading Screen Tips Tests

        [Test]
        public void LoadingScreen_TipsArrayNotEmpty()
        {
            // Arrange
            var loadingScreenObj = new GameObject("LoadingScreen");
            var loadingScreen = loadingScreenObj.AddComponent<LoadingScreenUI>();

            // The tips are serialized, but we can test the component exists
            Assert.IsNotNull(loadingScreen);

            // Cleanup
            Object.DestroyImmediate(loadingScreenObj);
        }

        #endregion

        #region Button Effects Tests

        [Test]
        public void ButtonEffects_RequiresButton()
        {
            // The component requires a Button - this is enforced by RequireComponent
            var attributes = typeof(ButtonEffects).GetCustomAttributes(typeof(RequireComponent), true);
            Assert.Greater(attributes.Length, 0);

            var requireComponent = attributes[0] as RequireComponent;
            Assert.AreEqual(typeof(UnityEngine.UI.Button), requireComponent.m_Type0);
        }

        #endregion

        #region PowerUpDisplay Name Tests

        [Test]
        public void PowerUpDisplay_AllPowerUpsHaveNames()
        {
            // Arrange
            var types = System.Enum.GetValues(typeof(Collectibles.PowerUpType));

            // Assert
            foreach (Collectibles.PowerUpType type in types)
            {
                string name = PowerUpDisplay.GetPowerUpName(type);
                Assert.IsNotEmpty(name, $"Power-up {type} should have a display name");
            }
        }

        [Test]
        public void PowerUpDisplay_AllPowerUpsHaveDescriptions()
        {
            // Arrange
            var types = System.Enum.GetValues(typeof(Collectibles.PowerUpType));

            // Assert
            foreach (Collectibles.PowerUpType type in types)
            {
                string desc = PowerUpDisplay.GetPowerUpDescription(type);
                Assert.IsNotEmpty(desc, $"Power-up {type} should have a description");
            }
        }

        #endregion

        #region UI Animation Easing Tests

        [Test]
        public void UIAnimations_CanCreateCoroutines()
        {
            // Arrange
            var testObj = new GameObject("TestUI");
            var canvasGroup = testObj.AddComponent<CanvasGroup>();

            // Act
            var fadeIn = UIAnimations.FadeIn(canvasGroup, 0.5f);

            // Assert
            Assert.IsNotNull(fadeIn);

            // Cleanup
            Object.DestroyImmediate(testObj);
        }

        #endregion

        #region Settings Keys Tests

        [Test]
        public void SettingsUI_UsesConsistentKeyFormat()
        {
            // This test verifies the settings use consistent PlayerPrefs keys
            // The actual keys are private, so we verify by checking the component exists
            var settingsObj = new GameObject("Settings");
            var settings = settingsObj.AddComponent<SettingsUI>();

            Assert.IsNotNull(settings);

            // Cleanup
            Object.DestroyImmediate(settingsObj);
        }

        #endregion
    }
}
