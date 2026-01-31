using UnityEngine;
using System.Collections.Generic;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Effects
{
    /// <summary>
    /// Central manager for all visual effects in the game.
    /// Handles particle systems, screen effects, and VFX pooling.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Particle Prefabs - Collectibles")]
        [SerializeField] private GameObject coinCollectVFX;
        [SerializeField] private GameObject superCoinCollectVFX;
        [SerializeField] private GameObject powerUpCollectVFX;
        [SerializeField] private GameObject mysteryBoxVFX;

        [Header("Particle Prefabs - Power-Ups")]
        [SerializeField] private GameObject magnetFieldVFX;
        [SerializeField] private GameObject shieldBubbleVFX;
        [SerializeField] private GameObject speedBoostTrailVFX;
        [SerializeField] private GameObject starPowerGlowVFX;
        [SerializeField] private GameObject multiplierVFX;

        [Header("Particle Prefabs - Player")]
        [SerializeField] private GameObject jumpDustVFX;
        [SerializeField] private GameObject landingDustVFX;
        [SerializeField] private GameObject slideDustVFX;
        [SerializeField] private GameObject laneChangeDustVFX;
        [SerializeField] private GameObject runningDustVFX;

        [Header("Particle Prefabs - Impacts")]
        [SerializeField] private GameObject crashVFX;
        [SerializeField] private GameObject shieldBreakVFX;
        [SerializeField] private GameObject nearMissVFX;

        [Header("Particle Prefabs - Environment")]
        [SerializeField] private GameObject sparklesVFX;
        [SerializeField] private GameObject confettiVFX;
        [SerializeField] private GameObject dustCloudVFX;

        [Header("Pool Settings")]
        [SerializeField] private int poolSizePerType = 10;
        [SerializeField] private Transform poolContainer;

        // VFX Pools
        private Dictionary<VFXType, Queue<GameObject>> vfxPools = new Dictionary<VFXType, Queue<GameObject>>();
        private Dictionary<VFXType, GameObject> vfxPrefabs = new Dictionary<VFXType, GameObject>();

        // Active effects tracking
        private Dictionary<Transform, GameObject> activePlayerEffects = new Dictionary<Transform, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ServiceLocator.Register(this);
            InitializePrefabMapping();
            InitializePools();
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
            GameEvents.OnCoinsCollected += OnCoinsCollected;
            GameEvents.OnPowerUpActivated += OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated += OnPowerUpDeactivated;
            GameEvents.OnPlayerJumped += OnPlayerJumped;
            GameEvents.OnPlayerSlide += OnPlayerSlide;
            GameEvents.OnPlayerCrashed += OnPlayerCrashed;
            GameEvents.OnLaneChanged += OnLaneChanged;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnCoinsCollected -= OnCoinsCollected;
            GameEvents.OnPowerUpActivated -= OnPowerUpActivated;
            GameEvents.OnPowerUpDeactivated -= OnPowerUpDeactivated;
            GameEvents.OnPlayerJumped -= OnPlayerJumped;
            GameEvents.OnPlayerSlide -= OnPlayerSlide;
            GameEvents.OnPlayerCrashed -= OnPlayerCrashed;
            GameEvents.OnLaneChanged -= OnLaneChanged;
        }

        private void InitializePrefabMapping()
        {
            vfxPrefabs[VFXType.CoinCollect] = coinCollectVFX;
            vfxPrefabs[VFXType.SuperCoinCollect] = superCoinCollectVFX;
            vfxPrefabs[VFXType.PowerUpCollect] = powerUpCollectVFX;
            vfxPrefabs[VFXType.MysteryBox] = mysteryBoxVFX;
            vfxPrefabs[VFXType.MagnetField] = magnetFieldVFX;
            vfxPrefabs[VFXType.ShieldBubble] = shieldBubbleVFX;
            vfxPrefabs[VFXType.SpeedBoostTrail] = speedBoostTrailVFX;
            vfxPrefabs[VFXType.StarPowerGlow] = starPowerGlowVFX;
            vfxPrefabs[VFXType.Multiplier] = multiplierVFX;
            vfxPrefabs[VFXType.JumpDust] = jumpDustVFX;
            vfxPrefabs[VFXType.LandingDust] = landingDustVFX;
            vfxPrefabs[VFXType.SlideDust] = slideDustVFX;
            vfxPrefabs[VFXType.LaneChangeDust] = laneChangeDustVFX;
            vfxPrefabs[VFXType.RunningDust] = runningDustVFX;
            vfxPrefabs[VFXType.Crash] = crashVFX;
            vfxPrefabs[VFXType.ShieldBreak] = shieldBreakVFX;
            vfxPrefabs[VFXType.NearMiss] = nearMissVFX;
            vfxPrefabs[VFXType.Sparkles] = sparklesVFX;
            vfxPrefabs[VFXType.Confetti] = confettiVFX;
            vfxPrefabs[VFXType.DustCloud] = dustCloudVFX;
        }

        private void InitializePools()
        {
            if (poolContainer == null)
            {
                var containerObj = new GameObject("VFX_Pool");
                containerObj.transform.SetParent(transform);
                poolContainer = containerObj.transform;
            }

            foreach (var kvp in vfxPrefabs)
            {
                if (kvp.Value != null)
                {
                    CreatePool(kvp.Key, kvp.Value);
                }
            }
        }

        private void CreatePool(VFXType type, GameObject prefab)
        {
            vfxPools[type] = new Queue<GameObject>();

            for (int i = 0; i < poolSizePerType; i++)
            {
                var instance = Instantiate(prefab, poolContainer);
                instance.SetActive(false);
                vfxPools[type].Enqueue(instance);
            }
        }

        #region Public API

        /// <summary>
        /// Spawns a VFX at the specified position.
        /// </summary>
        public GameObject SpawnVFX(VFXType type, Vector3 position, Quaternion rotation = default)
        {
            if (rotation == default) rotation = Quaternion.identity;

            if (!vfxPools.TryGetValue(type, out var pool) || pool.Count == 0)
            {
                // Try to create a new instance if we have the prefab
                if (vfxPrefabs.TryGetValue(type, out var prefab) && prefab != null)
                {
                    return InstantiateVFX(prefab, position, rotation);
                }
                return null;
            }

            var vfx = pool.Dequeue();
            vfx.transform.position = position;
            vfx.transform.rotation = rotation;
            vfx.SetActive(true);

            // Auto-return to pool after particle system completes
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                StartCoroutine(ReturnToPoolAfterDelay(vfx, type, duration));
            }

            return vfx;
        }

        /// <summary>
        /// Spawns a VFX attached to a parent transform.
        /// </summary>
        public GameObject SpawnVFXAttached(VFXType type, Transform parent, Vector3 localOffset = default)
        {
            var vfx = SpawnVFX(type, parent.position + localOffset);
            if (vfx != null)
            {
                vfx.transform.SetParent(parent);
                vfx.transform.localPosition = localOffset;
            }
            return vfx;
        }

        /// <summary>
        /// Spawns a persistent VFX on the player (for power-ups).
        /// </summary>
        public GameObject SpawnPlayerEffect(VFXType type, Transform player)
        {
            // Remove existing effect of same type
            if (activePlayerEffects.TryGetValue(player, out var existing))
            {
                ReturnToPool(existing, type);
            }

            var vfx = SpawnVFXAttached(type, player);
            if (vfx != null)
            {
                activePlayerEffects[player] = vfx;
            }
            return vfx;
        }

        /// <summary>
        /// Removes a player effect.
        /// </summary>
        public void RemovePlayerEffect(Transform player, VFXType type)
        {
            if (activePlayerEffects.TryGetValue(player, out var vfx))
            {
                ReturnToPool(vfx, type);
                activePlayerEffects.Remove(player);
            }
        }

        /// <summary>
        /// Returns a VFX instance to the pool.
        /// </summary>
        public void ReturnToPool(GameObject vfx, VFXType type)
        {
            if (vfx == null) return;

            vfx.SetActive(false);
            vfx.transform.SetParent(poolContainer);

            if (vfxPools.TryGetValue(type, out var pool))
            {
                pool.Enqueue(vfx);
            }
        }

        #endregion

        #region Event Handlers

        private void OnCoinsCollected(int amount)
        {
            // VFX is typically spawned at coin position by the coin itself
        }

        private void OnPowerUpActivated(PowerUpType type)
        {
            // Power-up VFX are handled by PowerUpManager or individual effects
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            // Cleanup handled by power-up effects
        }

        private void OnPlayerJumped()
        {
            var player = FindPlayerTransform();
            if (player != null)
            {
                SpawnVFX(VFXType.JumpDust, player.position);
            }
        }

        private void OnPlayerSlide()
        {
            var player = FindPlayerTransform();
            if (player != null)
            {
                SpawnVFX(VFXType.SlideDust, player.position);
            }
        }

        private void OnPlayerCrashed()
        {
            var player = FindPlayerTransform();
            if (player != null)
            {
                SpawnVFX(VFXType.Crash, player.position);
            }
        }

        private void OnLaneChanged(int lane)
        {
            var player = FindPlayerTransform();
            if (player != null)
            {
                SpawnVFX(VFXType.LaneChangeDust, player.position);
            }
        }

        private Transform FindPlayerTransform()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            return player?.transform;
        }

        #endregion

        #region Helpers

        private GameObject InstantiateVFX(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var vfx = Instantiate(prefab, position, rotation, poolContainer);
            
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(vfx, duration);
            }
            
            return vfx;
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject vfx, VFXType type, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(vfx, type);
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ServiceLocator.Unregister<VFXManager>();
        }
    }

    /// <summary>
    /// Types of visual effects available.
    /// </summary>
    public enum VFXType
    {
        // Collectibles
        CoinCollect,
        SuperCoinCollect,
        PowerUpCollect,
        MysteryBox,

        // Power-Ups
        MagnetField,
        ShieldBubble,
        SpeedBoostTrail,
        StarPowerGlow,
        Multiplier,

        // Player Actions
        JumpDust,
        LandingDust,
        SlideDust,
        LaneChangeDust,
        RunningDust,

        // Impacts
        Crash,
        ShieldBreak,
        NearMiss,

        // Environment
        Sparkles,
        Confetti,
        DustCloud
    }
}
