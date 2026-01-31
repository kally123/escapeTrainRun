using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using EscapeTrainRun.Core;
using EscapeTrainRun.Player;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Test utilities and helpers for PlayMode tests.
    /// Provides common setup/teardown and mock object creation.
    /// </summary>
    public static class TestUtilities
    {
        #region Test Scene Setup

        /// <summary>
        /// Creates a minimal test scene with required managers.
        /// </summary>
        public static TestSceneContext CreateTestScene()
        {
            var context = new TestSceneContext();

            // Create root object
            context.Root = new GameObject("TestRoot");

            // Create GameManager
            var gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(context.Root.transform);
            context.GameManager = gmObj.AddComponent<GameManager>();

            return context;
        }

        /// <summary>
        /// Creates a test player with all required components.
        /// </summary>
        public static PlayerTestContext CreateTestPlayer(Transform parent = null)
        {
            var context = new PlayerTestContext();

            // Create player object
            context.PlayerObject = new GameObject("TestPlayer");
            if (parent != null)
            {
                context.PlayerObject.transform.SetParent(parent);
            }

            // Add required components
            context.PlayerObject.AddComponent<CharacterController>();
            context.Controller = context.PlayerObject.AddComponent<PlayerController>();
            context.Movement = context.PlayerObject.AddComponent<PlayerMovement>();
            context.Collision = context.PlayerObject.AddComponent<PlayerCollision>();

            // Add collider
            var capsule = context.PlayerObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);

            // Add rigidbody (kinematic for character controller)
            var rb = context.PlayerObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            return context;
        }

        /// <summary>
        /// Creates a test obstacle.
        /// </summary>
        public static GameObject CreateTestObstacle(Vector3 position, ObstacleType type = ObstacleType.Static)
        {
            var obstacle = new GameObject($"TestObstacle_{type}");
            obstacle.transform.position = position;
            obstacle.tag = "Obstacle";
            obstacle.layer = LayerMask.NameToLayer("Obstacle");

            // Add collider
            var collider = obstacle.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);
            collider.isTrigger = true;

            // Add obstacle component
            var obstacleComp = obstacle.AddComponent<Obstacles.Obstacle>();

            return obstacle;
        }

        /// <summary>
        /// Creates a test collectible (coin).
        /// </summary>
        public static GameObject CreateTestCoin(Vector3 position, int value = 1)
        {
            var coin = new GameObject("TestCoin");
            coin.transform.position = position;
            coin.tag = "Coin";

            // Add collider
            var collider = coin.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            // Add coin component
            var coinComp = coin.AddComponent<Collectibles.Coin>();

            return coin;
        }

        /// <summary>
        /// Creates a test power-up.
        /// </summary>
        public static GameObject CreateTestPowerUp(Vector3 position, Collectibles.PowerUpType type)
        {
            var powerUp = new GameObject($"TestPowerUp_{type}");
            powerUp.transform.position = position;
            powerUp.tag = "PowerUp";

            // Add collider
            var collider = powerUp.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            // Add power-up component
            var puComp = powerUp.AddComponent<Collectibles.PowerUp>();

            return powerUp;
        }

        /// <summary>
        /// Creates a test track segment.
        /// </summary>
        public static GameObject CreateTestTrackSegment(Vector3 position, float length = 50f)
        {
            var segment = new GameObject("TestTrackSegment");
            segment.transform.position = position;

            // Add floor collider
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(segment.transform);
            floor.transform.localPosition = new Vector3(0, -0.5f, length / 2f);
            floor.transform.localScale = new Vector3(7.5f, 1f, length);
            floor.layer = LayerMask.NameToLayer("Ground");

            // Add track segment component
            var trackComp = segment.AddComponent<TrackSegment>();

            return segment;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Destroys all test objects.
        /// </summary>
        public static void CleanupTestScene(TestSceneContext context)
        {
            if (context?.Root != null)
            {
                Object.Destroy(context.Root);
            }

            // Clear events
            GameEvents.ClearAllEvents();
        }

        /// <summary>
        /// Destroys a test player.
        /// </summary>
        public static void CleanupTestPlayer(PlayerTestContext context)
        {
            if (context?.PlayerObject != null)
            {
                Object.Destroy(context.PlayerObject);
            }
        }

        #endregion

        #region Wait Utilities

        /// <summary>
        /// Waits for a specified number of frames.
        /// </summary>
        public static IEnumerator WaitFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Waits until a condition is true or timeout.
        /// </summary>
        public static IEnumerator WaitUntilOrTimeout(System.Func<bool> condition, float timeout = 5f)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Waits for physics to settle.
        /// </summary>
        public static IEnumerator WaitForPhysics(int iterations = 3)
        {
            for (int i = 0; i < iterations; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        #endregion

        #region Assertion Helpers

        /// <summary>
        /// Asserts that two vectors are approximately equal.
        /// </summary>
        public static void AssertVectorApproxEqual(Vector3 expected, Vector3 actual, float tolerance = 0.01f)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance, $"X mismatch: expected {expected.x}, got {actual.x}");
            Assert.AreEqual(expected.y, actual.y, tolerance, $"Y mismatch: expected {expected.y}, got {actual.y}");
            Assert.AreEqual(expected.z, actual.z, tolerance, $"Z mismatch: expected {expected.z}, got {actual.z}");
        }

        /// <summary>
        /// Asserts that a value is within range.
        /// </summary>
        public static void AssertInRange(float value, float min, float max, string message = "")
        {
            Assert.GreaterOrEqual(value, min, $"Value {value} below minimum {min}. {message}");
            Assert.LessOrEqual(value, max, $"Value {value} above maximum {max}. {message}");
        }

        #endregion

        #region Input Simulation

        /// <summary>
        /// Simulates a swipe gesture.
        /// </summary>
        public static SwipeData SimulateSwipe(SwipeDirection direction)
        {
            Vector2 start = Vector2.zero;
            Vector2 end = Vector2.zero;

            switch (direction)
            {
                case SwipeDirection.Left:
                    start = new Vector2(500, 500);
                    end = new Vector2(100, 500);
                    break;
                case SwipeDirection.Right:
                    start = new Vector2(100, 500);
                    end = new Vector2(500, 500);
                    break;
                case SwipeDirection.Up:
                    start = new Vector2(300, 200);
                    end = new Vector2(300, 600);
                    break;
                case SwipeDirection.Down:
                    start = new Vector2(300, 600);
                    end = new Vector2(300, 200);
                    break;
            }

            return new SwipeData
            {
                Direction = direction,
                StartPosition = start,
                EndPosition = end,
                Duration = 0.2f
            };
        }

        #endregion
    }

    /// <summary>
    /// Context object for test scene.
    /// </summary>
    public class TestSceneContext
    {
        public GameObject Root;
        public GameManager GameManager;
    }

    /// <summary>
    /// Context object for test player.
    /// </summary>
    public class PlayerTestContext
    {
        public GameObject PlayerObject;
        public PlayerController Controller;
        public PlayerMovement Movement;
        public PlayerCollision Collision;

        public Vector3 Position
        {
            get => PlayerObject.transform.position;
            set => PlayerObject.transform.position = value;
        }
    }

    /// <summary>
    /// Swipe data for input simulation.
    /// </summary>
    public struct SwipeData
    {
        public SwipeDirection Direction;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public float Duration;
    }
}
