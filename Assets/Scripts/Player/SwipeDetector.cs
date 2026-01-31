using System;
using UnityEngine;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Detects swipe gestures on mobile and keyboard input on desktop.
    /// Supports touch, mouse, and keyboard input for cross-platform play.
    /// </summary>
    public class SwipeDetector : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistance = Constants.MinSwipeDistance;
        [SerializeField] private float maxSwipeTime = Constants.MaxSwipeTime;

        [Header("Dead Zone")]
        [SerializeField] private float deadZoneRadius = 10f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        /// <summary>
        /// Event triggered when a swipe is detected.
        /// </summary>
        public event Action<SwipeDirection> OnSwipeDetected;

        // Touch/Mouse tracking
        private Vector2 swipeStartPosition;
        private float swipeStartTime;
        private bool isSwiping;
        private int activeTouchId = -1;

        // Input state
        private bool isInputEnabled = true;

        private void Update()
        {
            if (!isInputEnabled) return;

            HandleTouchInput();
            HandleMouseInput();
            HandleKeyboardInput();
        }

        #region Touch Input

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (activeTouchId == -1)
                        {
                            StartSwipe(touch.position, touch.fingerId);
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        // Could add visual feedback here
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == activeTouchId)
                        {
                            EndSwipe(touch.position);
                        }
                        break;
                }
            }
        }

        #endregion

        #region Mouse Input (Windows/Editor)

        private void HandleMouseInput()
        {
            // Skip if touch is active (to avoid double input on touch screens)
            if (Input.touchCount > 0) return;

            if (Input.GetMouseButtonDown(0))
            {
                StartSwipe(Input.mousePosition, -1);
            }
            else if (Input.GetMouseButtonUp(0) && isSwiping)
            {
                EndSwipe(Input.mousePosition);
            }
        }

        #endregion

        #region Keyboard Input (Windows)

        private void HandleKeyboardInput()
        {
            // Arrow keys
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                TriggerSwipe(SwipeDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                TriggerSwipe(SwipeDirection.Right);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                TriggerSwipe(SwipeDirection.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                TriggerSwipe(SwipeDirection.Down);
            }
        }

        #endregion

        #region Swipe Processing

        private void StartSwipe(Vector2 position, int touchId)
        {
            swipeStartPosition = position;
            swipeStartTime = Time.unscaledTime;
            isSwiping = true;
            activeTouchId = touchId;

            if (showDebugInfo)
            {
                Debug.Log($"[SwipeDetector] Swipe started at {position}");
            }
        }

        private void EndSwipe(Vector2 position)
        {
            if (!isSwiping) return;

            float swipeTime = Time.unscaledTime - swipeStartTime;
            Vector2 swipeDelta = position - swipeStartPosition;
            float swipeDistance = swipeDelta.magnitude;

            if (showDebugInfo)
            {
                Debug.Log($"[SwipeDetector] Swipe ended - Distance: {swipeDistance:F1}, Time: {swipeTime:F2}s");
            }

            // Check if swipe is valid
            if (swipeTime <= maxSwipeTime && swipeDistance >= minSwipeDistance)
            {
                SwipeDirection direction = GetSwipeDirection(swipeDelta);
                TriggerSwipe(direction);
            }
            else if (swipeDistance < deadZoneRadius)
            {
                // Tap detected (could be used for other actions)
                if (showDebugInfo)
                {
                    Debug.Log("[SwipeDetector] Tap detected (no action)");
                }
            }

            // Reset state
            isSwiping = false;
            activeTouchId = -1;
        }

        private SwipeDirection GetSwipeDirection(Vector2 delta)
        {
            // Determine if swipe is more horizontal or vertical
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // Horizontal swipe
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                // Vertical swipe
                return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }

        private void TriggerSwipe(SwipeDirection direction)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[SwipeDetector] Swipe detected: {direction}");
            }

            OnSwipeDetected?.Invoke(direction);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enables or disables input detection.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;

            if (!enabled)
            {
                // Cancel any active swipe
                isSwiping = false;
                activeTouchId = -1;
            }
        }

        /// <summary>
        /// Checks if input is currently enabled.
        /// </summary>
        public bool IsInputEnabled => isInputEnabled;

        /// <summary>
        /// Sets the minimum swipe distance required.
        /// </summary>
        public void SetMinSwipeDistance(float distance)
        {
            minSwipeDistance = Mathf.Max(10f, distance);
        }

        /// <summary>
        /// Sets the maximum time allowed for a swipe.
        /// </summary>
        public void SetMaxSwipeTime(float time)
        {
            maxSwipeTime = Mathf.Max(0.1f, time);
        }

        #endregion

        #region Debug Visualization

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Swiping: {isSwiping}");
            GUILayout.Label($"Input Enabled: {isInputEnabled}");
            GUILayout.Label($"Touch Count: {Input.touchCount}");
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// Represents the direction of a swipe gesture.
    /// </summary>
    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
