using System;
using UnityEngine;
using EscapeTrainRun.Collectibles;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Handles player collision detection with obstacles, coins, and power-ups.
    /// Uses trigger-based collision for smooth gameplay.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerCollision : MonoBehaviour
    {
        [Header("Collision Settings")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private LayerMask collectibleLayer;

        [Header("Colliders")]
        [SerializeField] private Collider mainCollider;
        [SerializeField] private Collider slideCollider;

        [Header("Collision Adjustment")]
        [SerializeField] private float collisionGracePeriod = 0.1f;

        /// <summary>Event triggered when hitting an obstacle.</summary>
        public event Action OnObstacleHit;

        /// <summary>Event triggered when collecting a coin.</summary>
        public event Action<int> OnCoinCollected;

        /// <summary>Event triggered when collecting a power-up.</summary>
        public event Action<PowerUpType> OnPowerUpCollected;

        // State
        private bool isInvincible;
        private float lastCollisionTime;
        private PlayerController playerController;

        private void Awake()
        {
            if (mainCollider == null)
            {
                mainCollider = GetComponent<Collider>();
            }

            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            // Ensure collider is set as trigger
            if (mainCollider != null)
            {
                mainCollider.isTrigger = true;
            }

            if (slideCollider != null)
            {
                slideCollider.isTrigger = true;
                slideCollider.enabled = false;
            }
        }

        private void Update()
        {
            UpdateColliderState();
        }

        /// <summary>
        /// Updates active collider based on player state.
        /// </summary>
        private void UpdateColliderState()
        {
            if (playerController == null) return;

            bool isSliding = playerController.CurrentState == PlayerState.Sliding;

            if (mainCollider != null && slideCollider != null)
            {
                mainCollider.enabled = !isSliding;
                slideCollider.enabled = isSliding;
            }
        }

        #region Trigger Handlers

        private void OnTriggerEnter(Collider other)
        {
            // Check obstacle collision
            if (IsObstacle(other))
            {
                HandleObstacleCollision(other);
                return;
            }

            // Check coin collision
            if (IsCoin(other))
            {
                HandleCoinCollection(other);
                return;
            }

            // Check power-up collision
            if (IsPowerUp(other))
            {
                HandlePowerUpCollection(other);
                return;
            }
        }

        #endregion

        #region Obstacle Collision

        private bool IsObstacle(Collider other)
        {
            return other.CompareTag(Constants.ObstacleTag) ||
                   ((1 << other.gameObject.layer) & obstacleLayer) != 0;
        }

        private void HandleObstacleCollision(Collider obstacle)
        {
            // Check grace period (avoid double hits)
            if (Time.time - lastCollisionTime < collisionGracePeriod)
            {
                return;
            }

            lastCollisionTime = Time.time;

            if (isInvincible)
            {
                // Player is invincible, consume shield if active
                if (playerController != null)
                {
                    playerController.ConsumeShield();
                }
                Debug.Log("[PlayerCollision] Collision absorbed by invincibility");
                return;
            }

            // Check if this is a collision we can avoid based on state
            var obstacleInfo = obstacle.GetComponent<IObstacle>();
            if (obstacleInfo != null)
            {
                if (!ShouldCollide(obstacleInfo))
                {
                    Debug.Log("[PlayerCollision] Collision avoided by player state");
                    return;
                }
            }

            Debug.Log($"[PlayerCollision] Hit obstacle: {obstacle.name}");
            OnObstacleHit?.Invoke();
        }

        /// <summary>
        /// Determines if the player should collide based on obstacle type and player state.
        /// </summary>
        private bool ShouldCollide(IObstacle obstacle)
        {
            if (playerController == null) return true;

            var state = playerController.CurrentState;

            switch (obstacle.RequiredAction)
            {
                case ObstacleAction.Jump:
                    // Avoid if jumping
                    return state != PlayerState.Jumping;

                case ObstacleAction.Slide:
                    // Avoid if sliding
                    return state != PlayerState.Sliding;

                case ObstacleAction.ChangeLane:
                    // Must change lane to avoid
                    return true;

                default:
                    return true;
            }
        }

        #endregion

        #region Coin Collection

        private bool IsCoin(Collider other)
        {
            return other.CompareTag(Constants.CoinTag) ||
                   other.GetComponent<Coin>() != null;
        }

        private void HandleCoinCollection(Collider coinCollider)
        {
            var coin = coinCollider.GetComponent<Coin>();
            if (coin != null)
            {
                int value = coin.Collect();
                OnCoinCollected?.Invoke(value);
            }
            else
            {
                // Fallback for simple coins without Coin component
                OnCoinCollected?.Invoke(Constants.RegularCoinValue);
                Destroy(coinCollider.gameObject);
            }
        }

        #endregion

        #region Power-Up Collection

        private bool IsPowerUp(Collider other)
        {
            return other.CompareTag(Constants.PowerUpTag) ||
                   other.GetComponent<PowerUp>() != null;
        }

        private void HandlePowerUpCollection(Collider powerUpCollider)
        {
            var powerUp = powerUpCollider.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                PowerUpType type = powerUp.Collect();
                OnPowerUpCollected?.Invoke(type);
            }
            else
            {
                // Fallback - try to get type from name
                Debug.LogWarning("[PlayerCollision] PowerUp component not found on object");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the invincibility state.
        /// </summary>
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
        }

        /// <summary>
        /// Checks if the player is currently invincible.
        /// </summary>
        public bool IsInvincible => isInvincible;

        /// <summary>
        /// Resets collision state.
        /// </summary>
        public void Reset()
        {
            isInvincible = false;
            lastCollisionTime = 0f;

            if (mainCollider != null)
            {
                mainCollider.enabled = true;
            }

            if (slideCollider != null)
            {
                slideCollider.enabled = false;
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            // Visualize collision bounds
            if (mainCollider != null)
            {
                Gizmos.color = isInvincible ? Color.cyan : Color.red;
                Gizmos.DrawWireCube(mainCollider.bounds.center, mainCollider.bounds.size);
            }

            if (slideCollider != null && slideCollider.enabled)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(slideCollider.bounds.center, slideCollider.bounds.size);
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface for obstacles to provide collision information.
    /// </summary>
    public interface IObstacle
    {
        /// <summary>The action required to avoid this obstacle.</summary>
        ObstacleAction RequiredAction { get; }

        /// <summary>Whether this obstacle is currently active.</summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// Actions that can be taken to avoid obstacles.
    /// </summary>
    public enum ObstacleAction
    {
        Jump,
        Slide,
        ChangeLane,
        Any
    }
}
