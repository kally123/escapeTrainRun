using UnityEngine;
using EscapeTrainRun.Player;
using EscapeTrainRun.Utils;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Base class for coin pickups.
    /// Handles collection logic and visual effects.
    /// </summary>
    public class Coin : MonoBehaviour
    {
        [Header("Coin Settings")]
        [SerializeField] private int value = Constants.RegularCoinValue;
        [SerializeField] private CoinType coinType = CoinType.Regular;

        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float bobSpeed = 2f;

        [Header("Magnet Response")]
        [SerializeField] private float magnetSpeed = 15f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem collectEffect;
        [SerializeField] private AudioClip collectSound;

        // State
        private bool isCollected;
        private bool isBeingMagneted;
        private Transform magnetTarget;
        private Vector3 startPosition;
        private float bobOffset;

        public int Value => value;
        public CoinType Type => coinType;
        public bool IsCollected => isCollected;
        public bool IsBeingAttracted => isBeingMagneted;

        private void Start()
        {
            startPosition = transform.position;
            bobOffset = Random.Range(0f, Mathf.PI * 2f);

            // Set value based on type
            value = coinType switch
            {
                CoinType.Regular => Constants.RegularCoinValue,
                CoinType.Special => Constants.SpecialCoinValue,
                CoinType.Bonus => Constants.SpecialCoinValue * 2,
                _ => Constants.RegularCoinValue
            };
        }

        private void Update()
        {
            if (isCollected) return;

            if (isBeingMagneted && magnetTarget != null)
            {
                MoveTowardsMagnet();
            }
            else
            {
                AnimateCoin();
            }
        }

        #region Animation

        private void AnimateCoin()
        {
            // Rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Bobbing
            float bobY = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
            transform.position = startPosition + new Vector3(0, bobY, 0);
        }

        #endregion

        #region Magnet Behavior

        /// <summary>
        /// Starts attracting this coin to the player.
        /// </summary>
        public void StartMagnet(Transform target)
        {
            if (isCollected) return;

            isBeingMagneted = true;
            magnetTarget = target;
        }

        /// <summary>
        /// Stops the magnet effect.
        /// </summary>
        public void StopMagnet()
        {
            isBeingMagneted = false;
            magnetTarget = null;
        }

        private void MoveTowardsMagnet()
        {
            if (magnetTarget == null)
            {
                StopMagnet();
                return;
            }

            // Move towards target
            Vector3 direction = (magnetTarget.position - transform.position).normalized;
            transform.position += direction * magnetSpeed * Time.deltaTime;

            // Faster rotation while being magneted
            transform.Rotate(Vector3.up, rotationSpeed * 3f * Time.deltaTime);
        }

        #endregion

        #region Collection

        /// <summary>
        /// Collects this coin and returns its value.
        /// </summary>
        public int Collect()
        {
            if (isCollected) return 0;

            isCollected = true;

            // Play effects
            PlayCollectEffects();

            // Return value and destroy
            int collectedValue = value;
            
            // Disable visuals immediately
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            // Delay destroy to allow particles to play
            if (collectEffect != null)
            {
                collectEffect.transform.SetParent(null);
                Destroy(collectEffect.gameObject, 2f);
            }

            Destroy(gameObject, 0.1f);

            return collectedValue;
        }

        private void PlayCollectEffects()
        {
            // Particles
            if (collectEffect != null)
            {
                collectEffect.Play();
            }

            // Sound (handled by AudioManager via events)
        }

        #endregion

        #region Pool Support

        /// <summary>
        /// Resets the coin for reuse from object pool.
        /// </summary>
        public void Reset()
        {
            isCollected = false;
            isBeingMagneted = false;
            magnetTarget = null;

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }

        /// <summary>
        /// Sets the coin's world position.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            startPosition = position;
        }

        #endregion
    }

    /// <summary>
    /// Types of coins with different values.
    /// </summary>
    public enum CoinType
    {
        Regular,
        Special,
        Bonus
    }
}
