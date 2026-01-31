using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for Player Movement system.
    /// Following AAA pattern: Arrange, Act, Assert.
    /// </summary>
    [TestFixture]
    public class PlayerMovementTests
    {
        private GameObject testObject;
        private GameObject modelObject;
        private PlayerMovement movement;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestPlayer");
            modelObject = new GameObject("TestModel");
            modelObject.transform.SetParent(testObject.transform);

            movement = new PlayerMovement(
                testObject.transform,
                modelObject.transform,
                Constants.LaneWidth,
                Constants.LaneChangeSpeed,
                Constants.JumpHeight,
                Constants.JumpDuration,
                Constants.SlideDuration
            );
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }

        #region Initial State Tests

        [Test]
        public void Movement_InitialState_CanJump()
        {
            // Assert
            Assert.IsTrue(movement.CanJump);
        }

        [Test]
        public void Movement_InitialState_CanSlide()
        {
            // Assert
            Assert.IsTrue(movement.CanSlide);
        }

        [Test]
        public void Movement_InitialState_IsGrounded()
        {
            // Assert
            Assert.IsTrue(movement.IsGrounded);
        }

        [Test]
        public void Movement_InitialState_NotJumping()
        {
            // Assert
            Assert.IsFalse(movement.IsJumping);
        }

        [Test]
        public void Movement_InitialState_NotSliding()
        {
            // Assert
            Assert.IsFalse(movement.IsSliding);
        }

        #endregion

        #region Lane Change Tests

        [Test]
        public void ChangeLane_ToRightLane_SetsCorrectTargetPosition()
        {
            // Arrange
            int targetLane = 2; // Right lane

            // Act
            movement.ChangeLane(targetLane);

            // Assert - lane change progress should be less than 1 (in progress)
            Assert.Less(movement.GetLaneChangeProgress(), 1f);
        }

        [Test]
        public void SetLane_ToCenter_SetsPositionImmediately()
        {
            // Arrange
            int targetLane = 1; // Center lane

            // Act
            movement.SetLane(targetLane);

            // Assert
            Assert.AreEqual(0f, testObject.transform.position.x, 0.001f);
        }

        [Test]
        public void SetLane_ToLeft_SetsNegativeXPosition()
        {
            // Arrange
            int targetLane = 0; // Left lane

            // Act
            movement.SetLane(targetLane);

            // Assert
            Assert.AreEqual(-Constants.LaneWidth, testObject.transform.position.x, 0.001f);
        }

        [Test]
        public void SetLane_ToRight_SetsPositiveXPosition()
        {
            // Arrange
            int targetLane = 2; // Right lane

            // Act
            movement.SetLane(targetLane);

            // Assert
            Assert.AreEqual(Constants.LaneWidth, testObject.transform.position.x, 0.001f);
        }

        #endregion

        #region Jump Tests

        [Test]
        public void Jump_WhenGrounded_SetsJumpingState()
        {
            // Arrange
            Assert.IsTrue(movement.CanJump);

            // Act
            movement.Jump();

            // Assert
            Assert.IsTrue(movement.IsJumping);
            Assert.IsFalse(movement.CanJump);
            Assert.IsFalse(movement.IsGrounded);
        }

        [Test]
        public void Jump_WhileJumping_DoesNothing()
        {
            // Arrange
            movement.Jump();
            float progressAfterFirstJump = movement.GetJumpProgress();

            // Act - try to jump again immediately
            movement.Jump();

            // Assert - should still be in same jump
            Assert.IsTrue(movement.IsJumping);
        }

        [Test]
        public void Jump_Progress_StartsAtZero()
        {
            // Act
            movement.Jump();

            // Assert
            Assert.AreEqual(0f, movement.GetJumpProgress(), 0.01f);
        }

        #endregion

        #region Slide Tests

        [Test]
        public void Slide_WhenGrounded_SetsSlideState()
        {
            // Arrange
            Assert.IsTrue(movement.CanSlide);

            // Act
            movement.Slide();

            // Assert
            Assert.IsTrue(movement.IsSliding);
            Assert.IsFalse(movement.CanSlide);
        }

        [Test]
        public void Slide_WhileJumping_DoesNothing()
        {
            // Arrange
            movement.Jump();

            // Act
            movement.Slide();

            // Assert - should still be jumping, not sliding
            Assert.IsTrue(movement.IsJumping);
            Assert.IsFalse(movement.IsSliding);
        }

        [Test]
        public void Slide_Progress_StartsAtZero()
        {
            // Act
            movement.Slide();

            // Assert
            Assert.AreEqual(0f, movement.GetSlideProgress(), 0.01f);
        }

        [Test]
        public void CancelSlide_WhileSliding_StopsSlide()
        {
            // Arrange
            movement.Slide();
            Assert.IsTrue(movement.IsSliding);

            // Act
            movement.CancelSlide();

            // Assert
            Assert.IsFalse(movement.IsSliding);
        }

        #endregion

        #region Reset Tests

        [Test]
        public void Reset_AfterJump_RestoresGroundedState()
        {
            // Arrange
            movement.Jump();
            Assert.IsFalse(movement.IsGrounded);

            // Act
            movement.Reset();

            // Assert
            Assert.IsTrue(movement.IsGrounded);
            Assert.IsFalse(movement.IsJumping);
        }

        [Test]
        public void Reset_AfterSlide_RestoresNormalState()
        {
            // Arrange
            movement.Slide();
            Assert.IsTrue(movement.IsSliding);

            // Act
            movement.Reset();

            // Assert
            Assert.IsFalse(movement.IsSliding);
            Assert.IsTrue(movement.CanSlide);
        }

        [Test]
        public void Reset_RestoresCanJump()
        {
            // Arrange
            movement.Jump();
            Assert.IsFalse(movement.CanJump);

            // Act
            movement.Reset();

            // Assert
            Assert.IsTrue(movement.CanJump);
        }

        #endregion

        #region Progress Getters Tests

        [Test]
        public void GetJumpProgress_WhenNotJumping_ReturnsZero()
        {
            // Assert
            Assert.AreEqual(0f, movement.GetJumpProgress());
        }

        [Test]
        public void GetSlideProgress_WhenNotSliding_ReturnsZero()
        {
            // Assert
            Assert.AreEqual(0f, movement.GetSlideProgress());
        }

        [Test]
        public void GetLaneChangeProgress_WhenAtTarget_ReturnsOne()
        {
            // Arrange
            movement.SetLane(1); // Set to center immediately

            // Assert
            Assert.AreEqual(1f, movement.GetLaneChangeProgress(), 0.01f);
        }

        #endregion
    }
}
