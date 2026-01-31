using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Level Generation system.
    /// </summary>
    [TestFixture]
    public class LevelGeneratorTests
    {
        #region TrackSegment Tests

        [Test]
        public void TrackSegment_GetEndZ_ReturnsCorrectValue()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();
            segmentObj.transform.position = new Vector3(0, 0, 100f);

            // Act
            float endZ = segment.GetEndZ();

            // Assert
            Assert.AreEqual(100f + Constants.SegmentLength, endZ);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        [Test]
        public void TrackSegment_GetStartZ_ReturnsPosition()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();
            segmentObj.transform.position = new Vector3(0, 0, 50f);

            // Act
            float startZ = segment.GetStartZ();

            // Assert
            Assert.AreEqual(50f, startZ);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        [Test]
        public void TrackSegment_ContainsZ_ReturnsTrueForPositionInSegment()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();
            segmentObj.transform.position = new Vector3(0, 0, 0f);

            // Act
            bool contains = segment.ContainsZ(15f);

            // Assert
            Assert.IsTrue(contains);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        [Test]
        public void TrackSegment_ContainsZ_ReturnsFalseForPositionOutsideSegment()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();
            segmentObj.transform.position = new Vector3(0, 0, 0f);

            // Act
            bool contains = segment.ContainsZ(100f);

            // Assert
            Assert.IsFalse(contains);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        [Test]
        public void TrackSegment_GetRandomPosition_ReturnsPositionInSegment()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();
            segmentObj.transform.position = new Vector3(0, 0, 0f);

            // Act
            Vector3 randomPos = segment.GetRandomPosition(1f);

            // Assert
            Assert.GreaterOrEqual(randomPos.z, segment.GetStartZ());
            Assert.LessOrEqual(randomPos.z, segment.GetEndZ());
            Assert.AreEqual(1f, randomPos.y);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        #endregion

        #region ThemeType Tests

        [Test]
        public void ThemeType_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)ThemeType.Train);
            Assert.AreEqual(1, (int)ThemeType.Bus);
            Assert.AreEqual(2, (int)ThemeType.Ground);
        }

        [Test]
        public void ThemeType_AllValuesExist()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(ThemeType));

            // Assert
            Assert.AreEqual(3, values.Length);
        }

        #endregion

        #region Spawn Point Tests

        [Test]
        public void TrackSegment_GetObstacleSpawnPoints_ReturnsNonNull()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();

            // Act
            var spawnPoints = segment.GetObstacleSpawnPoints();

            // Assert
            Assert.IsNotNull(spawnPoints);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        [Test]
        public void TrackSegment_GetCoinSpawnPoints_ReturnsNonNull()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();

            // Act
            var spawnPoints = segment.GetCoinSpawnPoints();

            // Assert
            Assert.IsNotNull(spawnPoints);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        [Test]
        public void TrackSegment_GetPowerUpSpawnPoints_ReturnsNonNull()
        {
            // Arrange
            var segmentObj = new GameObject("TestSegment");
            var segment = segmentObj.AddComponent<TrackSegment>();

            // Act
            var spawnPoints = segment.GetPowerUpSpawnPoints();

            // Assert
            Assert.IsNotNull(spawnPoints);

            // Cleanup
            Object.DestroyImmediate(segmentObj);
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for Obstacle system.
    /// </summary>
    [TestFixture]
    public class ObstacleTests
    {
        [Test]
        public void ObstacleAction_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)Player.ObstacleAction.Jump);
            Assert.AreEqual(1, (int)Player.ObstacleAction.Slide);
            Assert.AreEqual(2, (int)Player.ObstacleAction.ChangeLane);
            Assert.AreEqual(3, (int)Player.ObstacleAction.Any);
        }

        [Test]
        public void ObstacleType_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)Obstacles.ObstacleType.Static);
            Assert.AreEqual(1, (int)Obstacles.ObstacleType.Moving);
            Assert.AreEqual(2, (int)Obstacles.ObstacleType.Barrier);
            Assert.AreEqual(3, (int)Obstacles.ObstacleType.LowBarrier);
            Assert.AreEqual(4, (int)Obstacles.ObstacleType.Overhead);
        }
    }

    /// <summary>
    /// Unit tests for Parallax system.
    /// </summary>
    [TestFixture]
    public class ParallaxTests
    {
        [Test]
        public void ParallaxLayer_DefaultValues_AreCorrect()
        {
            // Arrange
            var layer = new ParallaxLayer();

            // Assert
            Assert.IsNull(layer.renderer);
            Assert.AreEqual(0.5f, layer.parallaxFactor);
            Assert.AreEqual(Vector2.right, layer.scrollDirection);
        }
    }
}
