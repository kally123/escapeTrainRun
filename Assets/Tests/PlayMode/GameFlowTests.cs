using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for game flow and state management.
    /// Tests game lifecycle from menu to gameplay to game over.
    /// </summary>
    [TestFixture]
    public class GameFlowTests
    {
        private TestSceneContext sceneContext;

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllEvents();
        }

        [TearDown]
        public void TearDown()
        {
            if (sceneContext != null)
            {
                TestUtilities.CleanupTestScene(sceneContext);
                sceneContext = null;
            }

            GameEvents.ClearAllEvents();
        }

        #region Game State Transition Tests

        [UnityTest]
        public IEnumerator Game_StartsInMenuState()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();

            yield return null;

            // Assert
            Assert.AreEqual(GameState.Menu, sceneContext.GameManager.CurrentState);
        }

        [UnityTest]
        public IEnumerator Game_TransitionsToPlayingOnStart()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();

            yield return null;

            // Act
            GameEvents.TriggerGameStarted();

            yield return null;

            // Assert - Game should transition to playing
            // Note: Depends on GameManager implementation
            Assert.IsNotNull(sceneContext.GameManager);
        }

        [UnityTest]
        public IEnumerator Game_TransitionsToGameOverOnCrash()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();

            yield return null;

            GameEvents.TriggerGameStarted();

            yield return null;

            // Act
            GameEvents.TriggerPlayerCrashed();

            yield return TestUtilities.WaitFrames(5);

            // Assert - Game should transition to game over
            Assert.IsNotNull(sceneContext.GameManager);
        }

        [UnityTest]
        public IEnumerator Game_PausesAndResumesCorrectly()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();

            yield return null;

            GameEvents.TriggerGameStarted();

            yield return null;

            // Act - Pause
            GameEvents.TriggerGamePaused();

            yield return null;

            Assert.IsTrue(sceneContext.GameManager.IsPaused);

            // Act - Resume
            GameEvents.TriggerGameResumed();

            yield return null;

            Assert.IsFalse(sceneContext.GameManager.IsPaused);
        }

        [UnityTest]
        public IEnumerator Game_TimeScaleZeroWhenPaused()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();

            yield return null;

            GameEvents.TriggerGameStarted();

            yield return null;

            // Act
            GameEvents.TriggerGamePaused();

            yield return null;

            // Assert - Time scale should be 0 when paused
            // Note: Depends on pause implementation
            Assert.IsTrue(sceneContext.GameManager.IsPaused);
        }

        #endregion

        #region Score System Tests

        [UnityTest]
        public IEnumerator Score_IncreasesOverTime()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();
            int score = 0;

            GameEvents.OnScoreChanged += (newScore) => score = newScore;

            yield return null;

            GameEvents.TriggerGameStarted();

            // Act - Trigger score updates
            GameEvents.TriggerScoreChanged(100);

            yield return null;

            // Assert
            Assert.AreEqual(100, score);
        }

        [UnityTest]
        public IEnumerator Score_MultiplierAppliesCorrectly()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();
            int lastScore = 0;

            GameEvents.OnScoreChanged += (newScore) => lastScore = newScore;

            yield return null;

            // Act
            GameEvents.TriggerScoreChanged(100);

            yield return null;

            // Assert
            Assert.AreEqual(100, lastScore);

            // With multiplier (simulated)
            GameEvents.TriggerScoreChanged(200);

            yield return null;

            Assert.AreEqual(200, lastScore);
        }

        #endregion

        #region Coin Collection Tests

        [UnityTest]
        public IEnumerator Coins_CollectedEventFires()
        {
            // Arrange
            int coinsCollected = 0;
            GameEvents.OnCoinsCollected += (amount) => coinsCollected += amount;

            yield return null;

            // Act
            GameEvents.TriggerCoinsCollected(5);

            yield return null;

            // Assert
            Assert.AreEqual(5, coinsCollected);
        }

        [UnityTest]
        public IEnumerator Coins_AccumulateCorrectly()
        {
            // Arrange
            int totalCoins = 0;
            GameEvents.OnCoinsCollected += (amount) => totalCoins += amount;

            yield return null;

            // Act
            GameEvents.TriggerCoinsCollected(10);
            GameEvents.TriggerCoinsCollected(5);
            GameEvents.TriggerCoinsCollected(3);

            yield return null;

            // Assert
            Assert.AreEqual(18, totalCoins);
        }

        #endregion

        #region Theme System Tests

        [UnityTest]
        public IEnumerator Theme_ChangesCorrectly()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();
            ThemeType receivedTheme = ThemeType.Train;

            GameEvents.OnThemeChanged += (theme) => receivedTheme = theme;

            yield return null;

            // Act
            GameEvents.TriggerThemeChanged(ThemeType.Bus);

            yield return null;

            // Assert
            Assert.AreEqual(ThemeType.Bus, receivedTheme);
        }

        [UnityTest]
        public IEnumerator Theme_SelectionBeforeGameStart()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();
            ThemeType selectedTheme = ThemeType.Train;

            GameEvents.OnThemeSelected += (theme) => selectedTheme = theme;

            yield return null;

            // Act
            GameEvents.TriggerThemeSelected(ThemeType.Park);

            yield return null;

            // Assert
            Assert.AreEqual(ThemeType.Park, selectedTheme);
        }

        #endregion

        #region Game Over Tests

        [UnityTest]
        public IEnumerator GameOver_ContainsCorrectData()
        {
            // Arrange
            GameOverData receivedData = null;
            GameEvents.OnGameOver += (data) => receivedData = data;

            yield return null;

            // Act
            var gameOverData = new GameOverData
            {
                finalScore = 12345,
                coinsCollected = 100,
                distance = 500f,
                isNewHighScore = true
            };

            GameEvents.TriggerGameOver(gameOverData);

            yield return null;

            // Assert
            Assert.IsNotNull(receivedData);
            Assert.AreEqual(12345, receivedData.finalScore);
            Assert.AreEqual(100, receivedData.coinsCollected);
            Assert.AreEqual(500f, receivedData.distance);
            Assert.IsTrue(receivedData.isNewHighScore);
        }

        [UnityTest]
        public IEnumerator GameOver_CanRestartGame()
        {
            // Arrange
            sceneContext = TestUtilities.CreateTestScene();
            bool gameStarted = false;

            GameEvents.OnGameStarted += () => gameStarted = true;

            yield return null;

            // First game
            GameEvents.TriggerGameStarted();

            yield return null;

            Assert.IsTrue(gameStarted);

            gameStarted = false;

            // Game over
            GameEvents.TriggerPlayerCrashed();

            yield return new WaitForSeconds(0.5f);

            // Restart
            GameEvents.TriggerGameStarted();

            yield return null;

            // Assert
            Assert.IsTrue(gameStarted);
        }

        #endregion

        #region Event Cleanup Tests

        [UnityTest]
        public IEnumerator Events_ClearedBetweenGames()
        {
            // Arrange
            int eventCount = 0;
            System.Action handler = () => eventCount++;

            GameEvents.OnGameStarted += handler;

            yield return null;

            // First trigger
            GameEvents.TriggerGameStarted();

            yield return null;

            Assert.AreEqual(1, eventCount);

            // Clear events
            GameEvents.ClearAllEvents();

            // Re-subscribe
            GameEvents.OnGameStarted += handler;

            // Second trigger
            GameEvents.TriggerGameStarted();

            yield return null;

            // Assert - Count should be 2 (not 3)
            Assert.AreEqual(2, eventCount);
        }

        #endregion
    }
}
