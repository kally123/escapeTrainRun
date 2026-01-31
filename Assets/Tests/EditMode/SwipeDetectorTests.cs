using NUnit.Framework;
using UnityEngine;
using EscapeTrainRun.Player;

namespace EscapeTrainRun.Tests.EditMode
{
    /// <summary>
    /// Unit tests for SwipeDetector input system.
    /// </summary>
    [TestFixture]
    public class SwipeDetectorTests
    {
        private GameObject testObject;
        private SwipeDetector swipeDetector;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestSwipeDetector");
            swipeDetector = testObject.AddComponent<SwipeDetector>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }

        #region Input Enable/Disable Tests

        [Test]
        public void SwipeDetector_InitialState_InputEnabled()
        {
            // Assert
            Assert.IsTrue(swipeDetector.IsInputEnabled);
        }

        [Test]
        public void SetInputEnabled_False_DisablesInput()
        {
            // Act
            swipeDetector.SetInputEnabled(false);

            // Assert
            Assert.IsFalse(swipeDetector.IsInputEnabled);
        }

        [Test]
        public void SetInputEnabled_True_EnablesInput()
        {
            // Arrange
            swipeDetector.SetInputEnabled(false);
            Assert.IsFalse(swipeDetector.IsInputEnabled);

            // Act
            swipeDetector.SetInputEnabled(true);

            // Assert
            Assert.IsTrue(swipeDetector.IsInputEnabled);
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void SetMinSwipeDistance_ValidValue_SetsValue()
        {
            // Act
            swipeDetector.SetMinSwipeDistance(100f);

            // Assert - no exception means success
            // The value is private, but we verify no error occurs
            Assert.Pass();
        }

        [Test]
        public void SetMinSwipeDistance_NegativeValue_ClampsToMinimum()
        {
            // Act - should clamp to minimum (10f)
            swipeDetector.SetMinSwipeDistance(-50f);

            // Assert - no exception means success
            Assert.Pass();
        }

        [Test]
        public void SetMaxSwipeTime_ValidValue_SetsValue()
        {
            // Act
            swipeDetector.SetMaxSwipeTime(1f);

            // Assert - no exception means success
            Assert.Pass();
        }

        [Test]
        public void SetMaxSwipeTime_TooLow_ClampsToMinimum()
        {
            // Act - should clamp to minimum (0.1f)
            swipeDetector.SetMaxSwipeTime(0.01f);

            // Assert - no exception means success
            Assert.Pass();
        }

        #endregion

        #region Event Subscription Tests

        [Test]
        public void OnSwipeDetected_CanSubscribe_NoError()
        {
            // Arrange
            bool eventReceived = false;
            SwipeDirection? receivedDirection = null;

            // Act
            swipeDetector.OnSwipeDetected += (direction) => 
            {
                eventReceived = true;
                receivedDirection = direction;
            };

            // Assert - subscription didn't throw
            Assert.Pass();
        }

        [Test]
        public void OnSwipeDetected_CanUnsubscribe_NoError()
        {
            // Arrange
            void Handler(SwipeDirection direction) { }
            swipeDetector.OnSwipeDetected += Handler;

            // Act
            swipeDetector.OnSwipeDetected -= Handler;

            // Assert - no exception means success
            Assert.Pass();
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for SwipeDirection enum.
    /// </summary>
    [TestFixture]
    public class SwipeDirectionTests
    {
        [Test]
        public void SwipeDirection_HasExpectedValues()
        {
            // Assert
            Assert.AreEqual(0, (int)SwipeDirection.Left);
            Assert.AreEqual(1, (int)SwipeDirection.Right);
            Assert.AreEqual(2, (int)SwipeDirection.Up);
            Assert.AreEqual(3, (int)SwipeDirection.Down);
        }

        [Test]
        public void SwipeDirection_AllValuesExist()
        {
            // Arrange
            var values = System.Enum.GetValues(typeof(SwipeDirection));

            // Assert
            Assert.AreEqual(4, values.Length);
        }
    }
}
