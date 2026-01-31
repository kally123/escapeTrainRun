using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Collectibles;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Main player controller - coordinates all player subsystems.
    /// Each subsystem has single responsibility following coding guidelines.
    /// </summary>
    [RequireComponent(typeof(PlayerCollision))]
    [RequireComponent(typeof(SwipeDetector))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float laneWidth = Constants.LaneWidth;
        [SerializeField] private float laneChangeSpeed = Constants.LaneChangeSpeed;
        [SerializeField] private float jumpHeight = Constants.JumpHeight;
        [SerializeField] private float jumpDuration = Constants.JumpDuration;
        [SerializeField] private float slideDuration = Constants.SlideDuration;

        [Header("Speed Settings")]
        [SerializeField] private float baseSpeed = Constants.BaseRunSpeed;
        [SerializeField] private float maxSpeed = Constants.MaxRunSpeed;
        [SerializeField] private float speedIncreaseRate = Constants.SpeedIncreaseRate;

        [Header("References")]
        [SerializeField] private PlayerAnimation playerAnimation;
        [SerializeField] private Transform characterModel;

        // Subsystems
        private PlayerMovement movement;
        private PlayerCollision collision;
        private SwipeDetector swipeDetector;

        // State
        private PlayerState currentState = PlayerState.Idle;
        private int currentLane = Constants.CenterLane;
        private float currentSpeed;
        private bool isInvincible;
        private bool isGameActive;

        // Properties
        public PlayerState CurrentState => currentState;
        public int CurrentLane => currentLane;
        public float CurrentSpeed => currentSpeed;
        public bool IsInvincible => isInvincible;
        public bool IsGrounded => movement != null && movement.IsGrounded;
        public Transform CharacterModel => characterModel;

        private void Awake()
        {
            InitializeComponents();
            InitializeMovement();
            RegisterServices();
        }

        private void InitializeComponents()
        {
            collision = GetComponent<PlayerCollision>();
            swipeDetector = GetComponent<SwipeDetector>();

            if (playerAnimation == null)
            {
                playerAnimation = GetComponentInChildren<PlayerAnimation>();
            }

            if (characterModel == null)
            {
                characterModel = transform.GetChild(0);
            }
        }

        private void InitializeMovement()
        {
            movement = new PlayerMovement(
                transform,
                characterModel,
                laneWidth,
                laneChangeSpeed,
                jumpHeight,
                jumpDuration,
                slideDuration
            );
        }

        private void RegisterServices()
        {
            ServiceLocator.Register(this);

            // Set player reference in ScoreManager
            if (ServiceLocator.TryGet<ScoreManager>(out var scoreManager))
            {
                scoreManager.SetPlayerTransform(transform);
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            // Input events
            swipeDetector.OnSwipeDetected += HandleSwipe;

            // Collision events
            collision.OnObstacleHit += HandleObstacleHit;
            collision.OnCoinCollected += HandleCoinCollected;
            collision.OnPowerUpCollected += HandlePowerUpCollected;

            // Game events
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGamePaused += OnGamePaused;
            GameEvents.OnGameResumed += OnGameResumed;
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
        }

        private void UnsubscribeFromEvents()
        {
            if (swipeDetector != null)
            {
                swipeDetector.OnSwipeDetected -= HandleSwipe;
            }

            if (collision != null)
            {
                collision.OnObstacleHit -= HandleObstacleHit;
                collision.OnCoinCollected -= HandleCoinCollected;
                collision.OnPowerUpCollected -= HandlePowerUpCollected;
            }

            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGamePaused -= OnGamePaused;
            GameEvents.OnGameResumed -= OnGameResumed;
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
        }

        private void Start()
        {
            ResetPlayer();
        }

        private void Update()
        {
            if (!isGameActive || currentState == PlayerState.Crashed)
            {
                return;
            }

            UpdateSpeed();
            movement.Update(currentSpeed, Time.deltaTime);
            UpdateAnimationSpeed();
        }

        #region Game State Handlers

        private void OnGameStarted()
        {
            ResetPlayer();
            isGameActive = true;
            currentState = PlayerState.Running;

            if (playerAnimation != null)
            {
                playerAnimation.PlayRun();
            }

            Debug.Log("[PlayerController] Game started - player running");
        }

        private void OnGamePaused()
        {
            isGameActive = false;
        }

        private void OnGameResumed()
        {
            isGameActive = true;
        }

        /// <summary>
        /// Resets the player to initial state.
        /// </summary>
        public void ResetPlayer()
        {
            currentLane = Constants.CenterLane;
            currentSpeed = baseSpeed;
            currentState = PlayerState.Idle;
            isInvincible = false;

            transform.position = new Vector3(0, 0, 0);

            if (movement != null)
            {
                movement.Reset();
                movement.SetLane(currentLane);
            }

            if (playerAnimation != null)
            {
                playerAnimation.ResetAnimations();
            }
        }

        #endregion

        #region Speed Management

        private void UpdateSpeed()
        {
            if (currentState == PlayerState.Running || 
                currentState == PlayerState.Jumping || 
                currentState == PlayerState.Sliding)
            {
                currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
            }
        }

        private void UpdateAnimationSpeed()
        {
            if (playerAnimation != null)
            {
                float speedRatio = currentSpeed / maxSpeed;
                playerAnimation.SetSpeed(speedRatio);
            }
        }

        /// <summary>
        /// Applies a speed multiplier (for power-ups).
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            currentSpeed = Mathf.Min(currentSpeed * multiplier, maxSpeed * 2f);
        }

        /// <summary>
        /// Resets speed to base value.
        /// </summary>
        public void ResetSpeed()
        {
            currentSpeed = baseSpeed;
        }

        #endregion

        #region Input Handling

        private void HandleSwipe(SwipeDirection direction)
        {
            if (!isGameActive || currentState == PlayerState.Crashed)
            {
                return;
            }

            switch (direction)
            {
                case SwipeDirection.Left:
                    TryChangeLane(-1);
                    break;
                case SwipeDirection.Right:
                    TryChangeLane(1);
                    break;
                case SwipeDirection.Up:
                    TryJump();
                    break;
                case SwipeDirection.Down:
                    TrySlide();
                    break;
            }
        }

        private void TryChangeLane(int direction)
        {
            int targetLane = currentLane + direction;

            if (targetLane >= 0 && targetLane < Constants.LaneCount)
            {
                currentLane = targetLane;
                movement.ChangeLane(currentLane);

                if (playerAnimation != null)
                {
                    playerAnimation.PlayLaneChange(direction);
                }

                GameEvents.TriggerLaneChanged(currentLane);
            }
        }

        private void TryJump()
        {
            if (!movement.CanJump)
            {
                return;
            }

            // Can interrupt slide with jump
            if (currentState == PlayerState.Sliding)
            {
                movement.CancelSlide();
            }

            currentState = PlayerState.Jumping;
            movement.Jump();

            if (playerAnimation != null)
            {
                playerAnimation.PlayJump();
            }

            GameEvents.TriggerPlayerJumped();

            // Reset state when jump ends
            StartCoroutine(ResetStateAfterDelay(jumpDuration));
        }

        private void TrySlide()
        {
            if (!movement.CanSlide)
            {
                return;
            }

            currentState = PlayerState.Sliding;
            movement.Slide();

            if (playerAnimation != null)
            {
                playerAnimation.PlaySlide();
            }

            GameEvents.TriggerPlayerSlide();

            // Reset state when slide ends
            StartCoroutine(ResetStateAfterDelay(slideDuration));
        }

        private System.Collections.IEnumerator ResetStateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (currentState != PlayerState.Crashed)
            {
                currentState = PlayerState.Running;

                if (playerAnimation != null)
                {
                    playerAnimation.PlayRun();
                }
            }
        }

        #endregion

        #region Collision Handling

        private void HandleObstacleHit()
        {
            if (isInvincible)
            {
                // Shield absorbed the hit
                Debug.Log("[PlayerController] Hit absorbed by shield");
                return;
            }

            Crash();
        }

        private void HandleCoinCollected(int value)
        {
            GameEvents.TriggerCoinsCollected(value);
        }

        private void HandlePowerUpCollected(PowerUpType type)
        {
            GameEvents.TriggerPowerUpActivated(type);
        }

        private void Crash()
        {
            if (currentState == PlayerState.Crashed)
            {
                return;
            }

            currentState = PlayerState.Crashed;
            isGameActive = false;

            if (playerAnimation != null)
            {
                playerAnimation.PlayCrash();
            }

            GameEvents.TriggerPlayerCrashed();

            Debug.Log("[PlayerController] Player crashed!");
        }

        #endregion

        #region Power-Up Handling

        private void OnPowerUpActivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    SetInvincible(true);
                    break;
                case PowerUpType.SpeedBoost:
                    SetInvincible(true);
                    SetSpeedMultiplier(Constants.SpeedBoostMultiplier);
                    break;
                case PowerUpType.StarPower:
                    SetInvincible(true);
                    if (playerAnimation != null)
                    {
                        playerAnimation.PlayStarPower();
                    }
                    break;
            }
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    SetInvincible(false);
                    break;
                case PowerUpType.SpeedBoost:
                    SetInvincible(false);
                    // Speed will gradually return to normal
                    break;
                case PowerUpType.StarPower:
                    SetInvincible(false);
                    if (playerAnimation != null)
                    {
                        playerAnimation.PlayRun();
                    }
                    break;
            }
        }

        /// <summary>
        /// Sets the player's invincibility state.
        /// </summary>
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
            collision.SetInvincible(invincible);

            Debug.Log($"[PlayerController] Invincibility: {invincible}");
        }

        /// <summary>
        /// Consumes the shield (called when hit while shielded).
        /// </summary>
        public void ConsumeShield()
        {
            SetInvincible(false);
            GameEvents.TriggerPowerUpDeactivated(PowerUpType.Shield);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Forces a lane change (for external control).
        /// </summary>
        public void ForceLaneChange(int targetLane)
        {
            targetLane = MathHelpers.ClampLane(targetLane);
            currentLane = targetLane;
            movement.ChangeLane(currentLane);
        }

        /// <summary>
        /// Gets the current world position of the player.
        /// </summary>
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Gets the target X position for the current lane.
        /// </summary>
        public float GetTargetX()
        {
            return MathHelpers.LaneToWorldX(currentLane, laneWidth);
        }

        #endregion

        private void OnDestroy()
        {
            ServiceLocator.Unregister<PlayerController>();
        }
    }

    /// <summary>
    /// Represents the current state of the player.
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Sliding,
        Crashed,
        PowerUp
    }
}
