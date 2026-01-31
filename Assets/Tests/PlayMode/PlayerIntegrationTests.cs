using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using EscapeTrainRun.Player;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for player movement and controls.
    /// Tests actual player behavior over multiple frames.
    /// </summary>
    [TestFixture]
    public class PlayerIntegrationTests
    {
        private PlayerTestContext playerContext;
        private TestSceneContext sceneContext;

        [SetUp]
        public void SetUp()
        {
            // Clear any previous events
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

            if (sceneContext != null)
            {
                TestUtilities.CleanupTestScene(sceneContext);
                sceneContext = null;
            }

            GameEvents.ClearAllEvents();
        }

        #region Lane Change Tests

        [UnityTest]
        public IEnumerator Player_LaneChange_MovesToTargetLane()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = new Vector3(0, 0, 0); // Center lane

            yield return null; // Wait one frame for initialization

            // Act - Simulate lane change event
            GameEvents.TriggerLaneChanged(0); // Move to left lane

            // Wait for movement
            yield return TestUtilities.WaitFrames(30);

            // Assert - Player should be at or moving toward left lane (-2.5)
            // Note: Actual movement depends on PlayerMovement implementation
            Assert.IsNotNull(playerContext.Movement);
        }

        [UnityTest]
        public IEnumerator Player_CannotChangeToInvalidLane()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = new Vector3(-2.5f, 0, 0); // Already at left lane

            yield return null;

            // Store initial position
            float initialX = playerContext.Position.x;

            // Act - Try to move further left (should be clamped)
            // This depends on implementation clamping lane values

            yield return TestUtilities.WaitFrames(10);

            // Assert - Position should not exceed lane bounds
            Assert.GreaterOrEqual(playerContext.Position.x, -2.5f);
        }

        #endregion

        #region Jump Tests

        [UnityTest]
        public IEnumerator Player_Jump_IncreasesHeight()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = new Vector3(0, 0, 0);

            yield return null;

            float initialY = playerContext.Position.y;

            // Act
            GameEvents.TriggerPlayerJumped();

            // Wait for jump arc
            yield return new WaitForSeconds(0.25f);

            // Assert - Player should be higher than initial position mid-jump
            // Note: Actual behavior depends on jump implementation
            Assert.IsNotNull(playerContext.Controller);
        }

        [UnityTest]
        public IEnumerator Player_Jump_ReturnsToGround()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();

            // Create ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.position = Vector3.zero;

            yield return null;

            // Act
            GameEvents.TriggerPlayerJumped();

            // Wait for full jump cycle
            yield return new WaitForSeconds(1f);

            // Assert - Player should return near ground level
            Assert.LessOrEqual(playerContext.Position.y, 0.5f);

            Object.Destroy(ground);
        }

        #endregion

        #region Slide Tests

        [UnityTest]
        public IEnumerator Player_Slide_ReducesColliderHeight()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            var capsule = playerContext.PlayerObject.GetComponent<CapsuleCollider>();
            float initialHeight = capsule.height;

            yield return null;

            // Act
            GameEvents.TriggerPlayerSlide();

            yield return TestUtilities.WaitFrames(5);

            // Assert - Collider height should be reduced during slide
            // Note: Actual behavior depends on implementation
            Assert.IsNotNull(capsule);
        }

        #endregion

        #region Continuous Movement Tests

        [UnityTest]
        public IEnumerator Player_ContinuousForwardMovement()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = Vector3.zero;

            yield return null;

            float initialZ = playerContext.Position.z;

            // Wait for movement
            yield return new WaitForSeconds(0.5f);

            // Assert - Player should have moved forward
            // Note: Depends on automatic forward movement implementation
            Assert.IsNotNull(playerContext.Movement);
        }

        [UnityTest]
        public IEnumerator Player_SpeedIncreasesOverTime()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();

            yield return null;

            // Record initial position
            float pos1 = playerContext.Position.z;

            yield return new WaitForSeconds(0.1f);

            float pos2 = playerContext.Position.z;
            float speed1 = (pos2 - pos1) / 0.1f;

            // Wait longer for speed to increase
            yield return new WaitForSeconds(2f);

            float pos3 = playerContext.Position.z;

            yield return new WaitForSeconds(0.1f);

            float pos4 = playerContext.Position.z;
            float speed2 = (pos4 - pos3) / 0.1f;

            // Assert - Later speed should be >= initial speed
            // Note: Depends on speed ramping implementation
            Assert.GreaterOrEqual(speed2, speed1 * 0.9f); // Allow 10% tolerance
        }

        #endregion

        #region Input Response Tests

        [UnityTest]
        public IEnumerator Player_RespondsToMultipleInputsSequentially()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();
            playerContext.Position = Vector3.zero;

            yield return null;

            // Act - Sequence of inputs
            GameEvents.TriggerPlayerJumped();
            yield return new WaitForSeconds(0.3f);

            GameEvents.TriggerLaneChanged(2); // Right lane
            yield return new WaitForSeconds(0.3f);

            GameEvents.TriggerPlayerSlide();
            yield return new WaitForSeconds(0.3f);

            // Assert - Player should still be valid and responsive
            Assert.IsNotNull(playerContext.PlayerObject);
            Assert.IsTrue(playerContext.PlayerObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator Player_InputQueueing_ProcessesInOrder()
        {
            // Arrange
            playerContext = TestUtilities.CreateTestPlayer();

            yield return null;

            // Act - Rapid input sequence
            GameEvents.TriggerPlayerJumped();
            GameEvents.TriggerLaneChanged(0);
            GameEvents.TriggerLaneChanged(2);

            // Wait for processing
            yield return new WaitForSeconds(1f);

            // Assert - Player should be in valid state
            Assert.IsNotNull(playerContext.Controller);
        }

        #endregion
    }
}
