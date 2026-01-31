using UnityEngine;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Handles all player movement calculations.
    /// Follows single responsibility principle - only movement logic.
    /// </summary>
    public class PlayerMovement
    {
        private readonly Transform playerTransform;
        private readonly Transform modelTransform;
        private readonly float laneWidth;
        private readonly float laneChangeSpeed;
        private readonly float jumpHeight;
        private readonly float jumpDuration;
        private readonly float slideDuration;

        // Lane movement
        private float targetXPosition;
        private float currentXPosition;

        // Jump state
        private bool isJumping;
        private float jumpTimer;
        private float originalYPosition;

        // Slide state
        private bool isSliding;
        private float slideTimer;
        private float originalModelScaleY;
        private float slideScaleY = 0.5f;

        // Properties
        public bool CanJump => !isJumping;
        public bool CanSlide => !isJumping && !isSliding;
        public bool IsGrounded => !isJumping;
        public bool IsSliding => isSliding;
        public bool IsJumping => isJumping;

        public PlayerMovement(
            Transform playerTransform,
            Transform modelTransform,
            float laneWidth,
            float laneChangeSpeed,
            float jumpHeight,
            float jumpDuration,
            float slideDuration)
        {
            this.playerTransform = playerTransform;
            this.modelTransform = modelTransform;
            this.laneWidth = laneWidth;
            this.laneChangeSpeed = laneChangeSpeed;
            this.jumpHeight = jumpHeight;
            this.jumpDuration = jumpDuration;
            this.slideDuration = slideDuration;

            this.originalYPosition = playerTransform.position.y;
            this.targetXPosition = 0f;
            this.currentXPosition = 0f;

            if (modelTransform != null)
            {
                this.originalModelScaleY = modelTransform.localScale.y;
            }
        }

        /// <summary>
        /// Updates movement each frame.
        /// </summary>
        /// <param name="forwardSpeed">Current forward movement speed.</param>
        /// <param name="deltaTime">Time since last frame.</param>
        public void Update(float forwardSpeed, float deltaTime)
        {
            UpdateHorizontalPosition(deltaTime);
            UpdateVerticalPosition(deltaTime);
            UpdateSlideState(deltaTime);
            MoveForward(forwardSpeed, deltaTime);
        }

        #region Horizontal Movement (Lane Changes)

        private void UpdateHorizontalPosition(float deltaTime)
        {
            if (!Mathf.Approximately(currentXPosition, targetXPosition))
            {
                currentXPosition = Mathf.MoveTowards(
                    currentXPosition,
                    targetXPosition,
                    laneChangeSpeed * deltaTime
                );

                Vector3 pos = playerTransform.position;
                pos.x = currentXPosition;
                playerTransform.position = pos;
            }
        }

        /// <summary>
        /// Initiates a lane change to the target lane.
        /// </summary>
        /// <param name="targetLane">Lane index (0=left, 1=center, 2=right).</param>
        public void ChangeLane(int targetLane)
        {
            targetXPosition = MathHelpers.LaneToWorldX(targetLane, laneWidth);
        }

        /// <summary>
        /// Sets the lane immediately without animation.
        /// </summary>
        public void SetLane(int lane)
        {
            targetXPosition = MathHelpers.LaneToWorldX(lane, laneWidth);
            currentXPosition = targetXPosition;

            Vector3 pos = playerTransform.position;
            pos.x = currentXPosition;
            playerTransform.position = pos;
        }

        #endregion

        #region Vertical Movement (Jumping)

        private void UpdateVerticalPosition(float deltaTime)
        {
            if (!isJumping) return;

            jumpTimer += deltaTime;
            float normalizedTime = jumpTimer / jumpDuration;

            if (normalizedTime >= 1f)
            {
                // Jump complete
                isJumping = false;
                jumpTimer = 0f;
                SetYPosition(originalYPosition);
            }
            else
            {
                // Parabolic jump curve: peaks at t=0.5
                float height = MathHelpers.ParabolicJump(normalizedTime, jumpHeight);
                SetYPosition(originalYPosition + height);
            }
        }

        /// <summary>
        /// Initiates a jump.
        /// </summary>
        public void Jump()
        {
            if (!CanJump) return;

            isJumping = true;
            jumpTimer = 0f;
            originalYPosition = 0f; // Ground level

            // If sliding, cancel the slide first
            if (isSliding)
            {
                CancelSlide();
            }
        }

        private void SetYPosition(float y)
        {
            Vector3 pos = playerTransform.position;
            pos.y = y;
            playerTransform.position = pos;
        }

        #endregion

        #region Slide Movement

        private void UpdateSlideState(float deltaTime)
        {
            if (!isSliding) return;

            slideTimer += deltaTime;

            if (slideTimer >= slideDuration)
            {
                EndSlide();
            }
        }

        /// <summary>
        /// Initiates a slide.
        /// </summary>
        public void Slide()
        {
            if (!CanSlide) return;

            isSliding = true;
            slideTimer = 0f;

            // Shrink the model to simulate sliding
            if (modelTransform != null)
            {
                Vector3 scale = modelTransform.localScale;
                scale.y = originalModelScaleY * slideScaleY;
                modelTransform.localScale = scale;

                // Move model down to stay grounded
                Vector3 localPos = modelTransform.localPosition;
                localPos.y = -originalModelScaleY * (1f - slideScaleY) * 0.5f;
                modelTransform.localPosition = localPos;
            }
        }

        /// <summary>
        /// Cancels the current slide.
        /// </summary>
        public void CancelSlide()
        {
            if (!isSliding) return;
            EndSlide();
        }

        private void EndSlide()
        {
            isSliding = false;
            slideTimer = 0f;

            // Restore model scale
            if (modelTransform != null)
            {
                Vector3 scale = modelTransform.localScale;
                scale.y = originalModelScaleY;
                modelTransform.localScale = scale;

                // Reset model position
                Vector3 localPos = modelTransform.localPosition;
                localPos.y = 0f;
                modelTransform.localPosition = localPos;
            }
        }

        #endregion

        #region Forward Movement

        private void MoveForward(float speed, float deltaTime)
        {
            playerTransform.position += Vector3.forward * speed * deltaTime;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets all movement state.
        /// </summary>
        public void Reset()
        {
            isJumping = false;
            isSliding = false;
            jumpTimer = 0f;
            slideTimer = 0f;
            currentXPosition = 0f;
            targetXPosition = 0f;

            // Reset model if sliding
            if (modelTransform != null)
            {
                Vector3 scale = modelTransform.localScale;
                scale.y = originalModelScaleY;
                modelTransform.localScale = scale;

                Vector3 localPos = modelTransform.localPosition;
                localPos.y = 0f;
                modelTransform.localPosition = localPos;
            }
        }

        #endregion

        #region Getters

        /// <summary>
        /// Gets the normalized jump progress (0-1).
        /// </summary>
        public float GetJumpProgress()
        {
            if (!isJumping) return 0f;
            return Mathf.Clamp01(jumpTimer / jumpDuration);
        }

        /// <summary>
        /// Gets the normalized slide progress (0-1).
        /// </summary>
        public float GetSlideProgress()
        {
            if (!isSliding) return 0f;
            return Mathf.Clamp01(slideTimer / slideDuration);
        }

        /// <summary>
        /// Gets the normalized lane change progress (0-1).
        /// </summary>
        public float GetLaneChangeProgress()
        {
            float distance = Mathf.Abs(targetXPosition - currentXPosition);
            float maxDistance = laneWidth;
            return 1f - Mathf.Clamp01(distance / maxDistance);
        }

        #endregion
    }
}
