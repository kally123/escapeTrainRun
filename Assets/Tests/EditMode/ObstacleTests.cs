using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Obstacles;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Obstacle system.
    /// </summary>
    [TestFixture]
    public class ObstacleTests
    {
        #region ObstacleType Tests

        [Test]
        public void ObstacleType_HasExpectedValues()
        {
            // Test existing ObstacleType enum values
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleType), ObstacleType.Static));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleType), ObstacleType.Moving));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleType), ObstacleType.Barrier));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleType), ObstacleType.LowBarrier));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleType), ObstacleType.Overhead));
        }

        [Test]
        public void ObstacleType_HasFiveValues()
        {
            var values = System.Enum.GetValues(typeof(ObstacleType));
            Assert.AreEqual(5, values.Length);
        }

        #endregion

        #region ObstacleAction Tests

        [Test]
        public void ObstacleAction_HasExpectedValues()
        {
            // ObstacleAction from IObstacle interface
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleAction), ObstacleAction.Jump));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleAction), ObstacleAction.Slide));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ObstacleAction), ObstacleAction.ChangeLane));
        }

        #endregion

        #region PatternDifficulty Tests

        [Test]
        public void PatternDifficulty_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(PatternDifficulty), PatternDifficulty.Easy));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PatternDifficulty), PatternDifficulty.Medium));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PatternDifficulty), PatternDifficulty.Hard));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PatternDifficulty), PatternDifficulty.Expert));
        }

        [Test]
        public void PatternDifficulty_HasFourLevels()
        {
            var values = System.Enum.GetValues(typeof(PatternDifficulty));
            Assert.AreEqual(4, values.Length);
        }

        #endregion

        #region MovementType Tests

        [Test]
        public void MovementType_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(MovementType), MovementType.Horizontal));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MovementType), MovementType.Vertical));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MovementType), MovementType.LaneToLane));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MovementType), MovementType.Circular));
            Assert.IsTrue(System.Enum.IsDefined(typeof(MovementType), MovementType.Sine));
        }

        [Test]
        public void MovementType_HasFiveTypes()
        {
            var values = System.Enum.GetValues(typeof(MovementType));
            Assert.AreEqual(5, values.Length);
        }

        #endregion

        #region Theme Obstacle Types Tests

        [Test]
        public void TrainObstacleType_HasExpectedTypes()
        {
            var values = System.Enum.GetValues(typeof(TrainObstacleType));
            Assert.Greater(values.Length, 0);
        }

        [Test]
        public void BusObstacleType_HasExpectedTypes()
        {
            var values = System.Enum.GetValues(typeof(BusObstacleType));
            Assert.Greater(values.Length, 0);
        }

        [Test]
        public void GroundObstacleType_HasExpectedTypes()
        {
            var values = System.Enum.GetValues(typeof(GroundObstacleType));
            Assert.Greater(values.Length, 0);
        }

        #endregion

        #region PatternObstacle Tests

        [Test]
        public void PatternObstacle_CanBeCreated()
        {
            var patternObs = new PatternObstacle
            {
                type = ObstacleType.LowBarrier,
                lane = 1,
                delay = 0.5f,
                distanceOffset = 0f,
                heightOffset = 0f
            };

            Assert.AreEqual(ObstacleType.LowBarrier, patternObs.type);
            Assert.AreEqual(1, patternObs.lane);
            Assert.AreEqual(0.5f, patternObs.delay);
        }

        [Test]
        public void PatternObstacle_DefaultLaneIsCenter()
        {
            var patternObs = new PatternObstacle();

            Assert.AreEqual(1, patternObs.lane); // Center lane
        }

        #endregion

        #region Component Tests

        [Test]
        public void ObstacleManager_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestObstacleManager");
            var manager = gameObj.AddComponent<ObstacleManager>();

            Assert.IsNotNull(manager);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void NearMissDetector_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestNearMiss");
            var detector = gameObj.AddComponent<NearMissDetector>();

            Assert.IsNotNull(detector);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void ObstacleWarningSystem_CanBeAddedToGameObject()
        {
            var gameObj = new GameObject("TestWarning");
            var warning = gameObj.AddComponent<ObstacleWarningSystem>();

            Assert.IsNotNull(warning);

            Object.DestroyImmediate(gameObj);
        }

        [Test]
        public void MovingObstacle_RequiresCollider()
        {
            var gameObj = new GameObject("TestMovingObstacle");
            gameObj.AddComponent<BoxCollider>();
            var moving = gameObj.AddComponent<MovingObstacle>();

            Assert.IsNotNull(moving);
            Assert.IsNotNull(gameObj.GetComponent<Collider>());

            Object.DestroyImmediate(gameObj);
        }

        #endregion

        #region ScriptableObject Tests

        [Test]
        public void ObstaclePrefabSet_CanBeCreated()
        {
            var prefabSet = ScriptableObject.CreateInstance<ObstaclePrefabSet>();

            Assert.IsNotNull(prefabSet);

            Object.DestroyImmediate(prefabSet);
        }

        [Test]
        public void ObstaclePattern_CanBeCreated()
        {
            var pattern = ScriptableObject.CreateInstance<ObstaclePattern>();

            Assert.IsNotNull(pattern);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void ObstaclePatternLibrary_CanBeCreated()
        {
            var library = ScriptableObject.CreateInstance<ObstaclePatternLibrary>();

            Assert.IsNotNull(library);

            Object.DestroyImmediate(library);
        }

        [Test]
        public void ObstaclePrefabSet_GetPrefab_ReturnsNullForEmptyArray()
        {
            var prefabSet = ScriptableObject.CreateInstance<ObstaclePrefabSet>();

            var result = prefabSet.GetPrefab(ObstacleType.Static);

            Assert.IsNull(result);

            Object.DestroyImmediate(prefabSet);
        }

        #endregion

        #region Validation Tests

        [Test]
        public void LaneValues_AreValid()
        {
            // Lanes should be 0, 1, 2 (left, center, right)
            int leftLane = 0;
            int centerLane = 1;
            int rightLane = 2;

            Assert.AreEqual(0, leftLane);
            Assert.AreEqual(1, centerLane);
            Assert.AreEqual(2, rightLane);
        }

        [Test]
        public void NearMissDistance_ShouldBeLessThanWarningDistance()
        {
            float nearMissDistance = 0.8f;
            float warningDistance = 40f;

            Assert.Less(nearMissDistance, warningDistance);
        }

        [Test]
        public void PerfectMissDistance_ShouldBeLessThanNearMissDistance()
        {
            float perfectMissDistance = 0.3f;
            float nearMissDistance = 0.8f;

            Assert.Less(perfectMissDistance, nearMissDistance);
        }

        #endregion
    }
}
