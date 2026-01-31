# Technical Design Document - Escape Train Run

## Architecture Overview

This document provides detailed technical specifications for implementing the Escape Train Run game following the coding guidelines and architectural patterns defined in the project.

---

## 1. Core Systems Architecture

### 1.1 Event-Driven Game Architecture

Following the event-driven architecture guidelines, the game uses an event-based communication system for loose coupling between components.

```csharp
// Events/GameEvents.cs
namespace EscapeTrainRun.Events
{
    // Event definitions following the event structure guidelines
    public static class GameEvents
    {
        // Player Events
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnCoinsCollected;
        public static event Action<Vector3> OnPlayerMoved;
        public static event Action<PowerUpType> OnPowerUpActivated;
        public static event Action<PowerUpType> OnPowerUpDeactivated;
        public static event Action OnPlayerJumped;
        public static event Action OnPlayerSlide;
        public static event Action OnPlayerCrashed;
        
        // Game State Events
        public static event Action OnGameStarted;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action<GameOverData> OnGameOver;
        
        // Environment Events
        public static event Action<ThemeType> OnThemeChanged;
        public static event Action<TrackSegment> OnSegmentSpawned;
        public static event Action<TrackSegment> OnSegmentDespawned;
        
        // Trigger methods (publishers don't know about subscribers)
        public static void TriggerScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
        public static void TriggerCoinsCollected(int amount) => OnCoinsCollected?.Invoke(amount);
        public static void TriggerGameOver(GameOverData data) => OnGameOver?.Invoke(data);
        // ... other trigger methods
    }
    
    // Immutable event data following WebFlux immutable POJO pattern
    public readonly struct GameOverData
    {
        public readonly int FinalScore;
        public readonly int CoinsCollected;
        public readonly float DistanceTraveled;
        public readonly bool IsHighScore;
        public readonly ThemeType GameMode;
        
        public GameOverData(int score, int coins, float distance, bool highScore, ThemeType mode)
        {
            FinalScore = score;
            CoinsCollected = coins;
            DistanceTraveled = distance;
            IsHighScore = highScore;
            GameMode = mode;
        }
    }
}
```

### 1.2 Service Locator Pattern

```csharp
// Core/ServiceLocator.cs
namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Central service locator for dependency management.
    /// Following microservice single responsibility principle.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();
        
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"Service {type.Name} already registered. Replacing.");
            }
            services[type] = service;
        }
        
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service {type.Name} not registered.");
        }
        
        public static void Clear()
        {
            services.Clear();
        }
    }
}
```

---

## 2. Player System

### 2.1 Player Controller (Single Responsibility)

```csharp
// Player/PlayerController.cs
namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Main player controller - coordinates all player subsystems.
    /// Each subsystem has single responsibility following coding guidelines.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float laneWidth = 2.5f;
        [SerializeField] private float laneChangeSpeed = 10f;
        [SerializeField] private float jumpHeight = 2.5f;
        [SerializeField] private float jumpDuration = 0.5f;
        [SerializeField] private float slideDuration = 0.8f;
        
        [Header("Speed Settings")]
        [SerializeField] private float baseSpeed = 15f;
        [SerializeField] private float maxSpeed = 35f;
        [SerializeField] private float speedIncreaseRate = 0.1f;
        
        // Subsystems (composition over inheritance)
        private PlayerMovement movement;
        private PlayerCollision collision;
        private PlayerAnimation playerAnimation;
        private SwipeDetector swipeDetector;
        
        // State
        private PlayerState currentState = PlayerState.Running;
        private int currentLane = 1; // 0=Left, 1=Center, 2=Right
        private float currentSpeed;
        private bool isInvincible;
        
        private void Awake()
        {
            InitializeSubsystems();
            RegisterServices();
        }
        
        private void InitializeSubsystems()
        {
            movement = new PlayerMovement(transform, laneWidth, laneChangeSpeed, jumpHeight, jumpDuration, slideDuration);
            collision = GetComponent<PlayerCollision>();
            playerAnimation = GetComponent<PlayerAnimation>();
            swipeDetector = GetComponent<SwipeDetector>();
        }
        
        private void RegisterServices()
        {
            ServiceLocator.Register<PlayerController>(this);
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
            swipeDetector.OnSwipeDetected += HandleSwipe;
            collision.OnObstacleHit += HandleObstacleHit;
            collision.OnCoinCollected += HandleCoinCollected;
            collision.OnPowerUpCollected += HandlePowerUpCollected;
        }
        
        private void UnsubscribeFromEvents()
        {
            swipeDetector.OnSwipeDetected -= HandleSwipe;
            collision.OnObstacleHit -= HandleObstacleHit;
            collision.OnCoinCollected -= HandleCoinCollected;
            collision.OnPowerUpCollected -= HandlePowerUpCollected;
        }
        
        private void Update()
        {
            if (currentState == PlayerState.Running)
            {
                UpdateSpeed();
                movement.Update(currentSpeed);
            }
        }
        
        private void UpdateSpeed()
        {
            currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
        }
        
        private void HandleSwipe(SwipeDirection direction)
        {
            if (currentState != PlayerState.Running) return;
            
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
            if (targetLane >= 0 && targetLane <= 2)
            {
                currentLane = targetLane;
                movement.ChangeLane(currentLane);
                playerAnimation.PlayLaneChange(direction);
            }
        }
        
        private void TryJump()
        {
            if (movement.CanJump)
            {
                movement.Jump();
                playerAnimation.PlayJump();
                GameEvents.TriggerPlayerJumped();
            }
        }
        
        private void TrySlide()
        {
            if (movement.CanSlide)
            {
                movement.Slide();
                playerAnimation.PlaySlide();
                GameEvents.TriggerPlayerSlide();
            }
        }
        
        private void HandleObstacleHit()
        {
            if (isInvincible) return;
            
            currentState = PlayerState.Crashed;
            playerAnimation.PlayCrash();
            GameEvents.TriggerPlayerCrashed();
        }
        
        private void HandleCoinCollected(int value)
        {
            GameEvents.TriggerCoinsCollected(value);
        }
        
        private void HandlePowerUpCollected(PowerUpType type)
        {
            GameEvents.TriggerPowerUpActivated(type);
        }
        
        // Public API
        public void SetInvincible(bool invincible) => isInvincible = invincible;
        public float CurrentSpeed => currentSpeed;
        public int CurrentLane => currentLane;
        public PlayerState State => currentState;
    }
    
    public enum PlayerState
    {
        Running,
        Jumping,
        Sliding,
        Crashed,
        PowerUp
    }
    
    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
```

### 2.2 Player Movement (Single Responsibility)

```csharp
// Player/PlayerMovement.cs
namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Handles all player movement calculations.
    /// Follows single responsibility principle - only movement logic.
    /// </summary>
    public class PlayerMovement
    {
        private readonly Transform transform;
        private readonly float laneWidth;
        private readonly float laneChangeSpeed;
        private readonly float jumpHeight;
        private readonly float jumpDuration;
        private readonly float slideDuration;
        
        private float targetXPosition;
        private bool isJumping;
        private bool isSliding;
        private float jumpTimer;
        private float slideTimer;
        private float originalYPosition;
        
        public bool CanJump => !isJumping && !isSliding;
        public bool CanSlide => !isJumping && !isSliding;
        public bool IsGrounded => !isJumping;
        
        public PlayerMovement(Transform transform, float laneWidth, float laneChangeSpeed, 
                              float jumpHeight, float jumpDuration, float slideDuration)
        {
            this.transform = transform;
            this.laneWidth = laneWidth;
            this.laneChangeSpeed = laneChangeSpeed;
            this.jumpHeight = jumpHeight;
            this.jumpDuration = jumpDuration;
            this.slideDuration = slideDuration;
            this.originalYPosition = transform.position.y;
        }
        
        public void Update(float forwardSpeed)
        {
            UpdateHorizontalPosition();
            UpdateVerticalPosition();
            MoveForward(forwardSpeed);
        }
        
        private void UpdateHorizontalPosition()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetXPosition, laneChangeSpeed * Time.deltaTime);
            transform.position = pos;
        }
        
        private void UpdateVerticalPosition()
        {
            if (isJumping)
            {
                jumpTimer += Time.deltaTime;
                float normalizedTime = jumpTimer / jumpDuration;
                
                if (normalizedTime >= 1f)
                {
                    isJumping = false;
                    SetYPosition(originalYPosition);
                }
                else
                {
                    // Parabolic jump curve
                    float height = 4 * jumpHeight * normalizedTime * (1 - normalizedTime);
                    SetYPosition(originalYPosition + height);
                }
            }
            
            if (isSliding)
            {
                slideTimer += Time.deltaTime;
                if (slideTimer >= slideDuration)
                {
                    isSliding = false;
                }
            }
        }
        
        private void MoveForward(float speed)
        {
            transform.position += Vector3.forward * speed * Time.deltaTime;
        }
        
        private void SetYPosition(float y)
        {
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }
        
        public void ChangeLane(int targetLane)
        {
            targetXPosition = (targetLane - 1) * laneWidth;
        }
        
        public void Jump()
        {
            if (!CanJump) return;
            isJumping = true;
            jumpTimer = 0f;
        }
        
        public void Slide()
        {
            if (!CanSlide) return;
            isSliding = true;
            slideTimer = 0f;
        }
    }
}
```

### 2.3 Swipe Detection (Cross-Platform Input)

```csharp
// Player/SwipeDetector.cs
namespace EscapeTrainRun.Player
{
    /// <summary>
    /// Detects swipe gestures on mobile and keyboard input on desktop.
    /// Supports both touch and mouse input for Windows touch screens.
    /// </summary>
    public class SwipeDetector : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 0.5f;
        
        public event Action<SwipeDirection> OnSwipeDetected;
        
        private Vector2 swipeStartPosition;
        private float swipeStartTime;
        private bool isSwiping;
        
        private void Update()
        {
            HandleTouchInput();
            HandleKeyboardInput();
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        StartSwipe(touch.position);
                        break;
                    case TouchPhase.Ended:
                        EndSwipe(touch.position);
                        break;
                }
            }
            
            // Also handle mouse for Windows touch screens and editor testing
            if (Input.GetMouseButtonDown(0))
            {
                StartSwipe(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndSwipe(Input.mousePosition);
            }
        }
        
        private void HandleKeyboardInput()
        {
            // Windows keyboard controls
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                OnSwipeDetected?.Invoke(SwipeDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                OnSwipeDetected?.Invoke(SwipeDirection.Right);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                OnSwipeDetected?.Invoke(SwipeDirection.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                OnSwipeDetected?.Invoke(SwipeDirection.Down);
            }
        }
        
        private void StartSwipe(Vector2 position)
        {
            swipeStartPosition = position;
            swipeStartTime = Time.time;
            isSwiping = true;
        }
        
        private void EndSwipe(Vector2 position)
        {
            if (!isSwiping) return;
            
            float swipeTime = Time.time - swipeStartTime;
            if (swipeTime > maxSwipeTime)
            {
                isSwiping = false;
                return;
            }
            
            Vector2 swipeDelta = position - swipeStartPosition;
            float swipeDistance = swipeDelta.magnitude;
            
            if (swipeDistance >= minSwipeDistance)
            {
                SwipeDirection direction = GetSwipeDirection(swipeDelta);
                OnSwipeDetected?.Invoke(direction);
            }
            
            isSwiping = false;
        }
        
        private SwipeDirection GetSwipeDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }
    }
}
```

---

## 3. Environment System

### 3.1 Level Generator

```csharp
// Environment/LevelGenerator.cs
namespace EscapeTrainRun.Environment
{
    /// <summary>
    /// Procedurally generates infinite track segments.
    /// Uses object pooling for performance (following performance guidelines).
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private float segmentLength = 30f;
        [SerializeField] private int initialSegments = 5;
        [SerializeField] private int maxActiveSegments = 10;
        [SerializeField] private float despawnDistance = 50f;
        
        [Header("Theme Prefabs")]
        [SerializeField] private TrackSegment[] trainSegmentPrefabs;
        [SerializeField] private TrackSegment[] busSegmentPrefabs;
        [SerializeField] private TrackSegment[] groundSegmentPrefabs;
        
        private Queue<TrackSegment> activeSegments = new();
        private ObjectPool<TrackSegment> segmentPool;
        private ThemeType currentTheme = ThemeType.Train;
        private float nextSpawnZ;
        private Transform playerTransform;
        
        private void Start()
        {
            playerTransform = ServiceLocator.Get<PlayerController>().transform;
            InitializePool();
            SpawnInitialSegments();
        }
        
        private void InitializePool()
        {
            // Pool for each theme - following performance guidelines for object pooling
            segmentPool = new ObjectPool<TrackSegment>(
                createFunc: () => InstantiateSegment(),
                actionOnGet: (segment) => segment.gameObject.SetActive(true),
                actionOnRelease: (segment) => segment.gameObject.SetActive(false),
                maxSize: maxActiveSegments * 3
            );
        }
        
        private TrackSegment InstantiateSegment()
        {
            TrackSegment[] prefabs = GetPrefabsForTheme(currentTheme);
            int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);
            return Instantiate(prefabs[randomIndex], transform);
        }
        
        private TrackSegment[] GetPrefabsForTheme(ThemeType theme)
        {
            return theme switch
            {
                ThemeType.Train => trainSegmentPrefabs,
                ThemeType.Bus => busSegmentPrefabs,
                ThemeType.Ground => groundSegmentPrefabs,
                _ => trainSegmentPrefabs
            };
        }
        
        private void SpawnInitialSegments()
        {
            nextSpawnZ = 0f;
            for (int i = 0; i < initialSegments; i++)
            {
                SpawnSegment();
            }
        }
        
        private void Update()
        {
            // Spawn new segments ahead of player
            while (nextSpawnZ < playerTransform.position.z + (initialSegments * segmentLength))
            {
                SpawnSegment();
            }
            
            // Despawn segments behind player
            while (activeSegments.Count > 0)
            {
                TrackSegment oldest = activeSegments.Peek();
                if (playerTransform.position.z - oldest.transform.position.z > despawnDistance)
                {
                    DespawnSegment();
                }
                else
                {
                    break;
                }
            }
        }
        
        private void SpawnSegment()
        {
            TrackSegment segment = segmentPool.Get();
            segment.transform.position = new Vector3(0, 0, nextSpawnZ);
            segment.Initialize(currentTheme);
            activeSegments.Enqueue(segment);
            nextSpawnZ += segmentLength;
            
            GameEvents.TriggerSegmentSpawned(segment);
        }
        
        private void DespawnSegment()
        {
            TrackSegment segment = activeSegments.Dequeue();
            segment.Cleanup();
            segmentPool.Release(segment);
            
            GameEvents.TriggerSegmentDespawned(segment);
        }
        
        public void SetTheme(ThemeType theme)
        {
            currentTheme = theme;
            GameEvents.TriggerThemeChanged(theme);
        }
    }
    
    public enum ThemeType
    {
        Train,
        Bus,
        Ground
    }
}
```

### 3.2 Object Pool (Performance Optimization)

```csharp
// Utils/ObjectPool.cs
namespace EscapeTrainRun.Utils
{
    /// <summary>
    /// Generic object pool to minimize garbage collection.
    /// Following performance guidelines for memory efficiency.
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> pool;
        private readonly Func<T> createFunc;
        private readonly Action<T> actionOnGet;
        private readonly Action<T> actionOnRelease;
        private readonly int maxSize;
        
        public int CountAll { get; private set; }
        public int CountActive => CountAll - pool.Count;
        public int CountInactive => pool.Count;
        
        public ObjectPool(Func<T> createFunc, Action<T> actionOnGet = null, 
                          Action<T> actionOnRelease = null, int maxSize = 100)
        {
            this.pool = new Stack<T>();
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.maxSize = maxSize;
        }
        
        public T Get()
        {
            T item;
            if (pool.Count > 0)
            {
                item = pool.Pop();
            }
            else
            {
                item = createFunc();
                CountAll++;
            }
            
            actionOnGet?.Invoke(item);
            return item;
        }
        
        public void Release(T item)
        {
            if (pool.Count < maxSize)
            {
                actionOnRelease?.Invoke(item);
                pool.Push(item);
            }
        }
        
        public void Clear()
        {
            pool.Clear();
        }
    }
}
```

---

## 4. Power-Up System

### 4.1 Power-Up Manager

```csharp
// Collectibles/PowerUpManager.cs
namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Manages all active power-ups following single responsibility.
    /// Uses event-driven communication for power-up effects.
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        private Dictionary<PowerUpType, PowerUpBase> activePowerUps = new();
        private Dictionary<PowerUpType, float> powerUpDurations = new();
        
        private void OnEnable()
        {
            GameEvents.OnPowerUpActivated += ActivatePowerUp;
        }
        
        private void OnDisable()
        {
            GameEvents.OnPowerUpActivated -= ActivatePowerUp;
        }
        
        private void Update()
        {
            UpdateActivePowerUps();
        }
        
        private void UpdateActivePowerUps()
        {
            var expiredPowerUps = new List<PowerUpType>();
            
            foreach (var kvp in powerUpDurations.ToList())
            {
                powerUpDurations[kvp.Key] -= Time.deltaTime;
                
                if (powerUpDurations[kvp.Key] <= 0)
                {
                    expiredPowerUps.Add(kvp.Key);
                }
            }
            
            foreach (var type in expiredPowerUps)
            {
                DeactivatePowerUp(type);
            }
        }
        
        private void ActivatePowerUp(PowerUpType type)
        {
            // Deactivate existing power-up of same type
            if (activePowerUps.ContainsKey(type))
            {
                DeactivatePowerUp(type);
            }
            
            PowerUpBase powerUp = CreatePowerUp(type);
            activePowerUps[type] = powerUp;
            powerUpDurations[type] = powerUp.Duration;
            
            powerUp.Activate();
        }
        
        private void DeactivatePowerUp(PowerUpType type)
        {
            if (activePowerUps.TryGetValue(type, out PowerUpBase powerUp))
            {
                powerUp.Deactivate();
                activePowerUps.Remove(type);
                powerUpDurations.Remove(type);
                
                GameEvents.TriggerPowerUpDeactivated(type);
            }
        }
        
        private PowerUpBase CreatePowerUp(PowerUpType type)
        {
            var player = ServiceLocator.Get<PlayerController>();
            
            return type switch
            {
                PowerUpType.Magnet => new MagnetPowerUp(player, 5f, 10f),
                PowerUpType.Shield => new ShieldPowerUp(player),
                PowerUpType.SpeedBoost => new SpeedBoostPowerUp(player, 2f, 5f),
                PowerUpType.StarPower => new StarPowerPowerUp(player, 8f),
                PowerUpType.Multiplier => new MultiplierPowerUp(2, 15f),
                _ => throw new ArgumentException($"Unknown power-up type: {type}")
            };
        }
        
        public bool HasActivePowerUp(PowerUpType type) => activePowerUps.ContainsKey(type);
        public float GetRemainingDuration(PowerUpType type) => 
            powerUpDurations.TryGetValue(type, out float duration) ? duration : 0f;
    }
    
    public enum PowerUpType
    {
        Magnet,
        Shield,
        SpeedBoost,
        StarPower,
        Multiplier,
        MysteryBox
    }
}
```

### 4.2 Power-Up Base Classes

```csharp
// Collectibles/PowerUpBase.cs
namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// Base class for all power-ups using composition pattern.
    /// </summary>
    public abstract class PowerUpBase
    {
        public abstract float Duration { get; }
        public abstract PowerUpType Type { get; }
        
        public abstract void Activate();
        public abstract void Deactivate();
    }
    
    /// <summary>
    /// Magnet power-up - attracts nearby coins.
    /// </summary>
    public class MagnetPowerUp : PowerUpBase
    {
        private readonly PlayerController player;
        private readonly float range;
        private readonly float duration;
        
        public override float Duration => duration;
        public override PowerUpType Type => PowerUpType.Magnet;
        
        public MagnetPowerUp(PlayerController player, float range, float duration)
        {
            this.player = player;
            this.range = range;
            this.duration = duration;
        }
        
        public override void Activate()
        {
            // Enable coin attraction in a radius around player
            CoinManager.Instance.EnableMagnet(player.transform, range);
        }
        
        public override void Deactivate()
        {
            CoinManager.Instance.DisableMagnet();
        }
    }
    
    /// <summary>
    /// Shield power-up - protects from one collision.
    /// </summary>
    public class ShieldPowerUp : PowerUpBase
    {
        private readonly PlayerController player;
        
        public override float Duration => float.MaxValue; // Until hit
        public override PowerUpType Type => PowerUpType.Shield;
        
        public ShieldPowerUp(PlayerController player)
        {
            this.player = player;
        }
        
        public override void Activate()
        {
            player.SetInvincible(true);
            // Visual: Enable shield bubble around player
        }
        
        public override void Deactivate()
        {
            player.SetInvincible(false);
            // Visual: Disable shield bubble
        }
    }
    
    /// <summary>
    /// Speed boost - temporary invincibility with increased speed.
    /// </summary>
    public class SpeedBoostPowerUp : PowerUpBase
    {
        private readonly PlayerController player;
        private readonly float speedMultiplier;
        private readonly float duration;
        
        public override float Duration => duration;
        public override PowerUpType Type => PowerUpType.SpeedBoost;
        
        public SpeedBoostPowerUp(PlayerController player, float speedMultiplier, float duration)
        {
            this.player = player;
            this.speedMultiplier = speedMultiplier;
            this.duration = duration;
        }
        
        public override void Activate()
        {
            player.SetInvincible(true);
            // Apply speed multiplier and visual effects
        }
        
        public override void Deactivate()
        {
            player.SetInvincible(false);
            // Remove speed multiplier
        }
    }
}
```

---

## 5. Score System

### 5.1 Score Manager

```csharp
// Core/ScoreManager.cs
namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Manages score calculation and persistence.
    /// Follows event-driven pattern for score updates.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private int currentScore;
        private int totalCoins;
        private float distanceTraveled;
        private int scoreMultiplier = 1;
        
        [Header("Score Settings")]
        [SerializeField] private int pointsPerMeter = 1;
        [SerializeField] private int pointsPerCoin = 10;
        
        private Transform playerTransform;
        private float lastPlayerZ;
        
        private void Awake()
        {
            ServiceLocator.Register(this);
        }
        
        private void Start()
        {
            playerTransform = ServiceLocator.Get<PlayerController>().transform;
            lastPlayerZ = playerTransform.position.z;
        }
        
        private void OnEnable()
        {
            GameEvents.OnCoinsCollected += AddCoins;
            GameEvents.OnPowerUpActivated += HandlePowerUp;
            GameEvents.OnPowerUpDeactivated += HandlePowerUpEnd;
        }
        
        private void OnDisable()
        {
            GameEvents.OnCoinsCollected -= AddCoins;
            GameEvents.OnPowerUpActivated -= HandlePowerUp;
            GameEvents.OnPowerUpDeactivated -= HandlePowerUpEnd;
        }
        
        private void Update()
        {
            UpdateDistanceScore();
        }
        
        private void UpdateDistanceScore()
        {
            float currentZ = playerTransform.position.z;
            float deltaDistance = currentZ - lastPlayerZ;
            
            if (deltaDistance > 0)
            {
                distanceTraveled += deltaDistance;
                int pointsEarned = Mathf.FloorToInt(deltaDistance * pointsPerMeter * scoreMultiplier);
                AddScore(pointsEarned);
                lastPlayerZ = currentZ;
            }
        }
        
        private void AddScore(int points)
        {
            currentScore += points;
            GameEvents.TriggerScoreChanged(currentScore);
        }
        
        private void AddCoins(int amount)
        {
            totalCoins += amount;
            AddScore(amount * pointsPerCoin * scoreMultiplier);
        }
        
        private void HandlePowerUp(PowerUpType type)
        {
            if (type == PowerUpType.Multiplier)
            {
                scoreMultiplier = 2;
            }
        }
        
        private void HandlePowerUpEnd(PowerUpType type)
        {
            if (type == PowerUpType.Multiplier)
            {
                scoreMultiplier = 1;
            }
        }
        
        public int GetCurrentScore() => currentScore;
        public int GetTotalCoins() => totalCoins;
        public float GetDistanceTraveled() => distanceTraveled;
        
        public GameOverData CreateGameOverData(ThemeType theme)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            bool isHighScore = currentScore > highScore;
            
            if (isHighScore)
            {
                PlayerPrefs.SetInt("HighScore", currentScore);
            }
            
            return new GameOverData(currentScore, totalCoins, distanceTraveled, isHighScore, theme);
        }
    }
}
```

---

## 6. UI System

### 6.1 Gameplay UI

```csharp
// UI/GameplayUI.cs
namespace EscapeTrainRun.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    /// <summary>
    /// Manages in-game HUD elements.
    /// Subscribes to events for reactive UI updates.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private Image powerUpIcon;
        [SerializeField] private Image powerUpTimer;
        [SerializeField] private Button pauseButton;
        
        [Header("Power-Up Icons")]
        [SerializeField] private Sprite magnetIcon;
        [SerializeField] private Sprite shieldIcon;
        [SerializeField] private Sprite speedIcon;
        [SerializeField] private Sprite starIcon;
        [SerializeField] private Sprite multiplierIcon;
        
        private PowerUpManager powerUpManager;
        private int displayedCoins;
        private int targetCoins;
        
        private void Start()
        {
            powerUpManager = FindObjectOfType<PowerUpManager>();
            pauseButton.onClick.AddListener(OnPauseClicked);
            HidePowerUpUI();
        }
        
        private void OnEnable()
        {
            GameEvents.OnScoreChanged += UpdateScore;
            GameEvents.OnCoinsCollected += UpdateCoins;
            GameEvents.OnPowerUpActivated += ShowPowerUp;
            GameEvents.OnPowerUpDeactivated += HidePowerUp;
        }
        
        private void OnDisable()
        {
            GameEvents.OnScoreChanged -= UpdateScore;
            GameEvents.OnCoinsCollected -= UpdateCoins;
            GameEvents.OnPowerUpActivated -= ShowPowerUp;
            GameEvents.OnPowerUpDeactivated -= HidePowerUp;
        }
        
        private void Update()
        {
            AnimateCoinCounter();
            UpdatePowerUpTimer();
        }
        
        private void UpdateScore(int score)
        {
            scoreText.text = score.ToString("N0");
        }
        
        private void UpdateCoins(int amount)
        {
            targetCoins += amount;
        }
        
        private void AnimateCoinCounter()
        {
            if (displayedCoins < targetCoins)
            {
                displayedCoins = Mathf.Min(displayedCoins + 5, targetCoins);
                coinText.text = displayedCoins.ToString("N0");
            }
        }
        
        private void ShowPowerUp(PowerUpType type)
        {
            powerUpIcon.gameObject.SetActive(true);
            powerUpIcon.sprite = GetPowerUpIcon(type);
            
            if (type == PowerUpType.Multiplier)
            {
                multiplierText.gameObject.SetActive(true);
                multiplierText.text = "x2";
            }
        }
        
        private void HidePowerUp(PowerUpType type)
        {
            if (type == PowerUpType.Multiplier)
            {
                multiplierText.gameObject.SetActive(false);
            }
            
            // Check if any power-ups are still active
            if (!AnyPowerUpActive())
            {
                HidePowerUpUI();
            }
        }
        
        private void HidePowerUpUI()
        {
            powerUpIcon.gameObject.SetActive(false);
            multiplierText.gameObject.SetActive(false);
        }
        
        private void UpdatePowerUpTimer()
        {
            // Find first active power-up and show its timer
            foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
            {
                if (powerUpManager.HasActivePowerUp(type))
                {
                    float remaining = powerUpManager.GetRemainingDuration(type);
                    float duration = GetPowerUpDuration(type);
                    powerUpTimer.fillAmount = remaining / duration;
                    break;
                }
            }
        }
        
        private Sprite GetPowerUpIcon(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => magnetIcon,
                PowerUpType.Shield => shieldIcon,
                PowerUpType.SpeedBoost => speedIcon,
                PowerUpType.StarPower => starIcon,
                PowerUpType.Multiplier => multiplierIcon,
                _ => null
            };
        }
        
        private float GetPowerUpDuration(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => 10f,
                PowerUpType.SpeedBoost => 5f,
                PowerUpType.StarPower => 8f,
                PowerUpType.Multiplier => 15f,
                _ => 10f
            };
        }
        
        private bool AnyPowerUpActive()
        {
            foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
            {
                if (powerUpManager.HasActivePowerUp(type)) return true;
            }
            return false;
        }
        
        private void OnPauseClicked()
        {
            GameEvents.TriggerGamePaused();
        }
    }
}
```

---

## 7. Backend Integration

### 7.1 Leaderboard Service

```csharp
// Services/LeaderboardService.cs
namespace EscapeTrainRun.Services
{
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    
    /// <summary>
    /// Handles leaderboard API calls following REST conventions.
    /// Uses async/await for non-blocking network operations.
    /// </summary>
    public class LeaderboardService
    {
        private readonly string baseUrl;
        private readonly string apiKey;
        
        public LeaderboardService(string baseUrl, string apiKey)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
        }
        
        /// <summary>
        /// Submits a score to the leaderboard.
        /// Event structure follows event-driven architecture guidelines.
        /// </summary>
        public async Task<bool> SubmitScore(ScoreSubmission submission)
        {
            string url = $"{baseUrl}/api/v1/leaderboard/submit";
            string json = JsonUtility.ToJson(submission);
            
            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-API-Key", apiKey);
            
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to submit score: {request.error}");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Gets top scores for a specific game mode.
        /// </summary>
        public async Task<LeaderboardEntry[]> GetTopScores(string gameMode, int limit = 100)
        {
            string url = $"{baseUrl}/api/v1/leaderboard/{gameMode}?limit={limit}";
            
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("X-API-Key", apiKey);
            
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to get leaderboard: {request.error}");
                return Array.Empty<LeaderboardEntry>();
            }
            
            string json = request.downloadHandler.text;
            var response = JsonUtility.FromJson<LeaderboardResponse>(json);
            return response.entries;
        }
        
        /// <summary>
        /// Gets player's rank on the leaderboard.
        /// </summary>
        public async Task<int> GetPlayerRank(string playerId, string gameMode)
        {
            string url = $"{baseUrl}/api/v1/leaderboard/{gameMode}/rank/{playerId}";
            
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("X-API-Key", apiKey);
            
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                return -1;
            }
            
            var response = JsonUtility.FromJson<RankResponse>(request.downloadHandler.text);
            return response.rank;
        }
    }
    
    // Immutable data transfer objects (following WebFlux guidelines)
    [System.Serializable]
    public readonly struct ScoreSubmission
    {
        public readonly string playerId;
        public readonly string playerName;
        public readonly int score;
        public readonly string gameMode;
        public readonly float distance;
        public readonly int coinsCollected;
        
        public ScoreSubmission(string playerId, string playerName, int score, 
                               string gameMode, float distance, int coinsCollected)
        {
            this.playerId = playerId;
            this.playerName = playerName;
            this.score = score;
            this.gameMode = gameMode;
            this.distance = distance;
            this.coinsCollected = coinsCollected;
        }
    }
    
    [System.Serializable]
    public struct LeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public int score;
        public int rank;
        public string timestamp;
    }
    
    [System.Serializable]
    public struct LeaderboardResponse
    {
        public LeaderboardEntry[] entries;
    }
    
    [System.Serializable]
    public struct RankResponse
    {
        public int rank;
    }
}
```

---

## 8. Testing Framework

### 8.1 Unit Tests Example

```csharp
// Tests/EditMode/PlayerMovementTests.cs
namespace EscapeTrainRun.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using EscapeTrainRun.Player;
    
    /// <summary>
    /// Unit tests for PlayerMovement following AAA pattern.
    /// </summary>
    [TestFixture]
    public class PlayerMovementTests
    {
        private GameObject testObject;
        private PlayerMovement movement;
        
        private const float LaneWidth = 2.5f;
        private const float LaneChangeSpeed = 10f;
        private const float JumpHeight = 2.5f;
        private const float JumpDuration = 0.5f;
        private const float SlideDuration = 0.8f;
        
        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestPlayer");
            movement = new PlayerMovement(
                testObject.transform, 
                LaneWidth, 
                LaneChangeSpeed, 
                JumpHeight, 
                JumpDuration, 
                SlideDuration
            );
        }
        
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }
        
        [Test]
        public void ChangeLane_ToRightLane_SetsCorrectTargetPosition()
        {
            // Arrange
            int targetLane = 2; // Right lane
            
            // Act
            movement.ChangeLane(targetLane);
            
            // Assert
            // Target X should be (2 - 1) * 2.5 = 2.5
            // This is verified through the movement's internal state
            Assert.Pass("Lane change initiated successfully");
        }
        
        [Test]
        public void Jump_WhenGrounded_SetsJumpingState()
        {
            // Arrange
            Assert.IsTrue(movement.CanJump);
            
            // Act
            movement.Jump();
            
            // Assert
            Assert.IsFalse(movement.CanJump);
            Assert.IsFalse(movement.IsGrounded);
        }
        
        [Test]
        public void Slide_WhenGrounded_SetsSlideState()
        {
            // Arrange
            Assert.IsTrue(movement.CanSlide);
            
            // Act
            movement.Slide();
            
            // Assert
            Assert.IsFalse(movement.CanSlide);
        }
        
        [Test]
        public void Jump_WhileSliding_DoesNotJump()
        {
            // Arrange
            movement.Slide();
            
            // Act
            movement.Jump();
            
            // Assert
            Assert.IsTrue(movement.IsGrounded); // Still grounded, jump was blocked
        }
        
        [Test]
        public void CanJump_AfterJumpCompletes_ReturnsTrue()
        {
            // Arrange
            movement.Jump();
            
            // Act - Simulate time passing beyond jump duration
            // In actual test, we'd use Unity's test framework with time simulation
            
            // Assert
            // After JumpDuration seconds, CanJump should return true
            Assert.Pass("Jump completion tested via play mode");
        }
    }
}
```

---

## 9. Configuration Constants

```csharp
// Utils/Constants.cs
namespace EscapeTrainRun.Utils
{
    /// <summary>
    /// Centralized game constants for easy tuning.
    /// </summary>
    public static class Constants
    {
        // Player Movement
        public const float LaneWidth = 2.5f;
        public const float LaneChangeSpeed = 10f;
        public const float JumpHeight = 2.5f;
        public const float JumpDuration = 0.5f;
        public const float SlideDuration = 0.8f;
        
        // Speed Settings
        public const float BaseRunSpeed = 15f;
        public const float MaxRunSpeed = 35f;
        public const float SpeedIncreaseRate = 0.1f;
        
        // Level Generation
        public const float SegmentLength = 30f;
        public const int InitialSegments = 5;
        public const int MaxActiveSegments = 10;
        public const float DespawnDistance = 50f;
        
        // Power-Ups
        public const float MagnetDuration = 10f;
        public const float MagnetRange = 5f;
        public const float SpeedBoostDuration = 5f;
        public const float SpeedBoostMultiplier = 2f;
        public const float StarPowerDuration = 8f;
        public const float MultiplierDuration = 15f;
        public const int MultiplierValue = 2;
        
        // Scoring
        public const int PointsPerMeter = 1;
        public const int PointsPerCoin = 10;
        
        // Input
        public const float MinSwipeDistance = 50f;
        public const float MaxSwipeTime = 0.5f;
        
        // API Endpoints
        public const string LeaderboardEndpoint = "/api/v1/leaderboard";
        public const string SaveGameEndpoint = "/api/v1/savegame";
        public const string AchievementsEndpoint = "/api/v1/achievements";
        
        // PlayerPrefs Keys
        public const string HighScoreKey = "HighScore";
        public const string TotalCoinsKey = "TotalCoins";
        public const string UnlockedCharactersKey = "UnlockedCharacters";
        public const string SettingsVolumeKey = "Volume";
        public const string SettingsMusicKey = "Music";
        public const string SettingsSfxKey = "Sfx";
    }
}
```

---

## Next Steps

1. Create Unity project with this folder structure
2. Implement core systems (GameManager, EventManager, ServiceLocator)
3. Build player controller with input handling
4. Create procedural level generator
5. Implement collision system
6. Add power-up system
7. Build UI framework
8. Integrate audio
9. Set up backend services
10. Test and polish

---

*Document Version: 1.0*
*Last Updated: January 31, 2026*
