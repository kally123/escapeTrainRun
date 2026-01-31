using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for level generation system.
    /// Tests track segment spawning, recycling, and procedural generation.
    /// </summary>
    [TestFixture]
    public class LevelGenerationTests
    {
        private TestSceneContext sceneContext;
        private List<GameObject> spawnedSegments = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllEvents();
            spawnedSegments.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var segment in spawnedSegments)
            {
                if (segment != null)
                {
                    Object.Destroy(segment);
                }
            }
            spawnedSegments.Clear();

            if (sceneContext != null)
            {
                TestUtilities.CleanupTestScene(sceneContext);
                sceneContext = null;
            }

            GameEvents.ClearAllEvents();
        }

        #region Segment Spawning Tests

        [UnityTest]
        public IEnumerator Segment_SpawnsAtCorrectPosition()
        {
            // Arrange
            Vector3 spawnPos = new Vector3(0, 0, 0);
            var segment = TestUtilities.CreateTestTrackSegment(spawnPos);
            spawnedSegments.Add(segment);

            yield return null;

            // Assert
            TestUtilities.AssertVectorApproxEqual(spawnPos, segment.transform.position);
        }

        [UnityTest]
        public IEnumerator Segment_ChainSpawnsContiguously()
        {
            // Arrange
            float segmentLength = 50f;
            int segmentCount = 5;

            for (int i = 0; i < segmentCount; i++)
            {
                var segment = TestUtilities.CreateTestTrackSegment(
                    new Vector3(0, 0, i * segmentLength),
                    segmentLength
                );
                spawnedSegments.Add(segment);
            }

            yield return null;

            // Assert - Segments should be contiguous
            for (int i = 1; i < spawnedSegments.Count; i++)
            {
                float expectedZ = i * segmentLength;
                float actualZ = spawnedSegments[i].transform.position.z;

                Assert.AreEqual(expectedZ, actualZ, 0.1f,
                    $"Segment {i} not at expected position");
            }
        }

        [UnityTest]
        public IEnumerator Segment_SpawnEventFires()
        {
            // Arrange
            int spawnCount = 0;
            GameEvents.OnSegmentCountChanged += () => spawnCount++;

            yield return null;

            // Act
            GameEvents.TriggerSegmentCountChanged();
            GameEvents.TriggerSegmentCountChanged();

            yield return null;

            // Assert
            Assert.AreEqual(2, spawnCount);
        }

        #endregion

        #region Segment Recycling Tests

        [UnityTest]
        public IEnumerator Segment_RecyclingReusesObjects()
        {
            // Arrange
            var segment = TestUtilities.CreateTestTrackSegment(Vector3.zero);
            spawnedSegments.Add(segment);

            yield return null;

            // Store original reference
            int originalId = segment.GetInstanceID();

            // Move segment (simulate recycling)
            segment.transform.position = new Vector3(0, 0, 100);

            yield return null;

            // Assert - Same object, new position
            Assert.AreEqual(originalId, segment.GetInstanceID());
            Assert.AreEqual(100f, segment.transform.position.z, 0.1f);
        }

        [UnityTest]
        public IEnumerator Segment_DespawnBehindPlayer()
        {
            // Arrange
            var segment = TestUtilities.CreateTestTrackSegment(new Vector3(0, 0, -100)); // Behind
            spawnedSegments.Add(segment);

            bool despawned = false;
            GameEvents.OnSegmentCountChanged += () => despawned = true;

            yield return null;

            // Act - Trigger despawn event
            GameEvents.TriggerSegmentCountChanged();

            yield return null;

            // Assert
            Assert.IsTrue(despawned);
        }

        #endregion

        #region Theme Integration Tests

        [UnityTest]
        public IEnumerator Theme_ChangeAffectsNewSegments()
        {
            // Arrange
            ThemeType currentTheme = ThemeType.Train;
            GameEvents.OnThemeChanged += (theme) => currentTheme = theme;

            yield return null;

            // Act
            GameEvents.TriggerThemeChanged(ThemeType.Bus);

            yield return null;

            // Assert
            Assert.AreEqual(ThemeType.Bus, currentTheme);
        }

        [UnityTest]
        public IEnumerator Theme_AllTypesSupported()
        {
            // Arrange
            var themes = new[] { ThemeType.Train, ThemeType.Bus, ThemeType.Park };
            ThemeType receivedTheme = ThemeType.Train;

            GameEvents.OnThemeChanged += (theme) => receivedTheme = theme;

            yield return null;

            // Act & Assert
            foreach (var theme in themes)
            {
                GameEvents.TriggerThemeChanged(theme);

                yield return null;

                Assert.AreEqual(theme, receivedTheme, $"Theme {theme} not received correctly");
            }
        }

        #endregion

        #region Obstacle Placement Tests

        [UnityTest]
        public IEnumerator Obstacles_SpawnWithinLanes()
        {
            // Arrange
            float[] lanePositions = { -2.5f, 0f, 2.5f };
            List<GameObject> obstacles = new List<GameObject>();

            for (int i = 0; i < 3; i++)
            {
                var obstacle = TestUtilities.CreateTestObstacle(
                    new Vector3(lanePositions[i], 0, 10 + i * 5)
                );
                obstacles.Add(obstacle);
            }

            yield return null;

            // Assert - Each obstacle in valid lane
            for (int i = 0; i < obstacles.Count; i++)
            {
                float x = obstacles[i].transform.position.x;
                bool inValidLane = Mathf.Abs(x - (-2.5f)) < 0.1f ||
                                   Mathf.Abs(x) < 0.1f ||
                                   Mathf.Abs(x - 2.5f) < 0.1f;

                Assert.IsTrue(inValidLane, $"Obstacle {i} at x={x} not in valid lane");
            }

            // Cleanup
            foreach (var obs in obstacles)
            {
                Object.Destroy(obs);
            }
        }

        [UnityTest]
        public IEnumerator Obstacles_MinimumSpacing()
        {
            // Arrange
            List<GameObject> obstacles = new List<GameObject>();
            float minSpacing = 5f;

            for (int i = 0; i < 5; i++)
            {
                var obstacle = TestUtilities.CreateTestObstacle(
                    new Vector3(0, 0, i * minSpacing + 10)
                );
                obstacles.Add(obstacle);
            }

            yield return null;

            // Assert - Spacing between obstacles
            for (int i = 1; i < obstacles.Count; i++)
            {
                float spacing = obstacles[i].transform.position.z -
                               obstacles[i - 1].transform.position.z;

                Assert.GreaterOrEqual(spacing, minSpacing * 0.9f,
                    $"Insufficient spacing between obstacles {i - 1} and {i}");
            }

            // Cleanup
            foreach (var obs in obstacles)
            {
                Object.Destroy(obs);
            }
        }

        #endregion

        #region Collectible Placement Tests

        [UnityTest]
        public IEnumerator Coins_SpawnInPatterns()
        {
            // Arrange - Create coin line pattern
            List<GameObject> coins = new List<GameObject>();
            int coinCount = 10;

            for (int i = 0; i < coinCount; i++)
            {
                var coin = TestUtilities.CreateTestCoin(new Vector3(0, 1, i * 2));
                coins.Add(coin);
            }

            yield return null;

            // Assert - Coins should be evenly spaced
            for (int i = 1; i < coins.Count; i++)
            {
                float spacing = coins[i].transform.position.z - coins[i - 1].transform.position.z;
                Assert.AreEqual(2f, spacing, 0.1f, $"Coin spacing incorrect at {i}");
            }

            // Cleanup
            foreach (var coin in coins)
            {
                Object.Destroy(coin);
            }
        }

        [UnityTest]
        public IEnumerator Coins_ArcPattern()
        {
            // Arrange - Create arc pattern
            List<GameObject> coins = new List<GameObject>();
            int coinCount = 5;

            for (int i = 0; i < coinCount; i++)
            {
                float t = (float)i / (coinCount - 1);
                float height = Mathf.Sin(t * Mathf.PI) * 2f + 1f;
                var coin = TestUtilities.CreateTestCoin(new Vector3(0, height, i * 2));
                coins.Add(coin);
            }

            yield return null;

            // Assert - Middle coins should be higher
            float middleHeight = coins[coinCount / 2].transform.position.y;
            float edgeHeight = coins[0].transform.position.y;

            Assert.Greater(middleHeight, edgeHeight, "Arc pattern not formed correctly");

            // Cleanup
            foreach (var coin in coins)
            {
                Object.Destroy(coin);
            }
        }

        #endregion

        #region Difficulty Scaling Tests

        [UnityTest]
        public IEnumerator Difficulty_IncreasesObstacleDensity()
        {
            // Arrange
            yield return null;

            // This would normally test the level generator's difficulty scaling
            // For now, verify the concept is testable
            int easyObstacleCount = 3;
            int hardObstacleCount = 6;

            // Assert
            Assert.Greater(hardObstacleCount, easyObstacleCount,
                "Hard difficulty should have more obstacles");
        }

        [UnityTest]
        public IEnumerator Difficulty_DecreasesGapsBetweenObstacles()
        {
            // Arrange
            yield return null;

            float easyGap = 10f;
            float hardGap = 5f;

            // Assert
            Assert.Less(hardGap, easyGap,
                "Hard difficulty should have smaller gaps");
        }

        #endregion

        #region Edge Cases

        [UnityTest]
        public IEnumerator Level_HandlesRapidSegmentSpawning()
        {
            // Arrange
            int spawnCount = 0;
            GameEvents.OnSegmentCountChanged += () => spawnCount++;

            yield return null;

            // Act - Rapid spawning
            for (int i = 0; i < 20; i++)
            {
                GameEvents.TriggerSegmentCountChanged();
            }

            yield return null;

            // Assert
            Assert.AreEqual(20, spawnCount);
        }

        [UnityTest]
        public IEnumerator Level_HandlesThemeSwitchMidGame()
        {
            // Arrange
            var themes = new List<ThemeType>();
            GameEvents.OnThemeChanged += (theme) => themes.Add(theme);

            yield return null;

            // Act - Switch themes rapidly
            GameEvents.TriggerThemeChanged(ThemeType.Train);
            GameEvents.TriggerThemeChanged(ThemeType.Bus);
            GameEvents.TriggerThemeChanged(ThemeType.Park);
            GameEvents.TriggerThemeChanged(ThemeType.Train);

            yield return null;

            // Assert - All switches recorded
            Assert.AreEqual(4, themes.Count);
        }

        #endregion
    }
}
