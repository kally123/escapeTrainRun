using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace EscapeTrainRun.Quality
{
    /// <summary>
    /// Real-time performance monitoring for FPS, frame times, and stability metrics.
    /// Provides data for quality checks and on-screen debugging.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        public static PerformanceMonitor Instance { get; private set; }

        [Header("Monitoring Settings")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private int sampleCount = 60;
        [SerializeField] private float updateInterval = 0.5f;

        [Header("Thresholds")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float warningFPS = 45f;
        [SerializeField] private float criticalFPS = 30f;
        [SerializeField] private float spikeThresholdMs = 33.33f; // > 30fps frame time

        [Header("Display")]
        [SerializeField] private bool showOnScreen = false;
        [SerializeField] private Rect displayRect = new Rect(10, 10, 200, 100);

        // FPS tracking
        private Queue<float> fpsSamples = new Queue<float>();
        private Queue<float> frameTimeSamples = new Queue<float>();
        private float accumulatedTime;
        private int accumulatedFrames;
        private float lastUpdateTime;

        // Current metrics
        private float currentFPS;
        private float averageFPS;
        private float minFPS;
        private float maxFPS;
        private float frameTimeMs;
        private float averageFrameTimeMs;

        // Stability tracking
        private int frameDropCount;
        private int spikeCount;
        private float stabilityScore;
        private bool isStable = true;

        // Session stats
        private float sessionStartTime;
        private int totalFrames;
        private int droppedFrames;

        // Public properties
        public float CurrentFPS => currentFPS;
        public float AverageFPS => averageFPS;
        public float MinFPS => minFPS;
        public float MaxFPS => maxFPS;
        public float FrameTimeMs => frameTimeMs;
        public float AverageFrameTimeMs => averageFrameTimeMs;
        public float StabilityScore => stabilityScore;
        public bool IsStable => isStable;
        public int FrameDropCount => frameDropCount;
        public int SpikeCount => spikeCount;
        public float SessionDuration => Time.realtimeSinceStartup - sessionStartTime;
        public float DroppedFramePercentage => totalFrames > 0 ? (float)droppedFrames / totalFrames * 100f : 0f;

        // Events
        public event System.Action<float> OnFPSUpdated;
        public event System.Action<PerformanceLevel> OnPerformanceLevelChanged;
        public event System.Action OnFrameSpike;

        private PerformanceLevel currentLevel = PerformanceLevel.Good;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            sessionStartTime = Time.realtimeSinceStartup;
            ResetMetrics();
        }

        private void Update()
        {
            if (!enableMonitoring) return;

            // Track frame time
            float deltaTime = Time.unscaledDeltaTime;
            frameTimeMs = deltaTime * 1000f;

            // Accumulate for FPS calculation
            accumulatedTime += deltaTime;
            accumulatedFrames++;
            totalFrames++;

            // Check for frame spike
            if (frameTimeMs > spikeThresholdMs)
            {
                spikeCount++;
                droppedFrames++;
                OnFrameSpike?.Invoke();
            }

            // Sample frame times
            frameTimeSamples.Enqueue(frameTimeMs);
            if (frameTimeSamples.Count > sampleCount)
            {
                frameTimeSamples.Dequeue();
            }

            // Update metrics at interval
            if (Time.realtimeSinceStartup - lastUpdateTime >= updateInterval)
            {
                UpdateMetrics();
                lastUpdateTime = Time.realtimeSinceStartup;
            }
        }

        private void OnGUI()
        {
            if (!showOnScreen) return;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 14;

            Color bgColor = GetPerformanceColor();
            GUI.backgroundColor = bgColor;

            GUILayout.BeginArea(displayRect, style);
            GUILayout.Label($"FPS: {currentFPS:F1} (avg: {averageFPS:F1})");
            GUILayout.Label($"Frame: {frameTimeMs:F2} ms");
            GUILayout.Label($"Min/Max: {minFPS:F0}/{maxFPS:F0}");
            GUILayout.Label($"Stability: {stabilityScore:F0}%");
            GUILayout.Label($"Drops: {droppedFrames} ({DroppedFramePercentage:F1}%)");
            GUILayout.EndArea();
        }

        #region Public API

        /// <summary>
        /// Resets all metrics.
        /// </summary>
        public void ResetMetrics()
        {
            fpsSamples.Clear();
            frameTimeSamples.Clear();

            currentFPS = 0f;
            averageFPS = 0f;
            minFPS = float.MaxValue;
            maxFPS = 0f;
            frameTimeMs = 0f;
            averageFrameTimeMs = 0f;

            frameDropCount = 0;
            spikeCount = 0;
            stabilityScore = 100f;
            isStable = true;

            totalFrames = 0;
            droppedFrames = 0;

            accumulatedTime = 0f;
            accumulatedFrames = 0;
            lastUpdateTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Gets a performance snapshot.
        /// </summary>
        public PerformanceSnapshot GetSnapshot()
        {
            return new PerformanceSnapshot
            {
                Timestamp = Time.realtimeSinceStartup,
                CurrentFPS = currentFPS,
                AverageFPS = averageFPS,
                MinFPS = minFPS,
                MaxFPS = maxFPS,
                FrameTimeMs = frameTimeMs,
                StabilityScore = stabilityScore,
                PerformanceLevel = currentLevel,
                DroppedFramePercentage = DroppedFramePercentage
            };
        }

        /// <summary>
        /// Enables/disables on-screen display.
        /// </summary>
        public void SetShowOnScreen(bool show)
        {
            showOnScreen = show;
        }

        /// <summary>
        /// Sets target FPS for calculations.
        /// </summary>
        public void SetTargetFPS(float fps)
        {
            targetFPS = fps;
        }

        #endregion

        #region Private Methods

        private void UpdateMetrics()
        {
            // Calculate current FPS
            if (accumulatedTime > 0)
            {
                currentFPS = accumulatedFrames / accumulatedTime;
            }

            // Add to samples
            fpsSamples.Enqueue(currentFPS);
            if (fpsSamples.Count > sampleCount)
            {
                fpsSamples.Dequeue();
            }

            // Calculate averages
            if (fpsSamples.Count > 0)
            {
                averageFPS = fpsSamples.Average();
                minFPS = Mathf.Min(minFPS, fpsSamples.Min());
                maxFPS = Mathf.Max(maxFPS, fpsSamples.Max());
            }

            if (frameTimeSamples.Count > 0)
            {
                averageFrameTimeMs = frameTimeSamples.Average();
            }

            // Calculate stability
            CalculateStability();

            // Update performance level
            UpdatePerformanceLevel();

            // Reset accumulators
            accumulatedTime = 0f;
            accumulatedFrames = 0;

            // Fire event
            OnFPSUpdated?.Invoke(currentFPS);
        }

        private void CalculateStability()
        {
            if (fpsSamples.Count < 2)
            {
                stabilityScore = 100f;
                isStable = true;
                return;
            }

            // Calculate variance
            float mean = fpsSamples.Average();
            float variance = fpsSamples.Average(fps => Mathf.Pow(fps - mean, 2));
            float stdDev = Mathf.Sqrt(variance);

            // Stability score based on standard deviation
            // Lower stdDev = more stable = higher score
            float maxAcceptableStdDev = 10f;
            stabilityScore = Mathf.Clamp((1f - stdDev / maxAcceptableStdDev) * 100f, 0f, 100f);

            // Consider stable if within 10% of average
            isStable = stdDev < mean * 0.1f;
        }

        private void UpdatePerformanceLevel()
        {
            PerformanceLevel newLevel;

            if (averageFPS >= targetFPS * 0.9f)
            {
                newLevel = PerformanceLevel.Excellent;
            }
            else if (averageFPS >= warningFPS)
            {
                newLevel = PerformanceLevel.Good;
            }
            else if (averageFPS >= criticalFPS)
            {
                newLevel = PerformanceLevel.Warning;
            }
            else
            {
                newLevel = PerformanceLevel.Critical;
            }

            if (newLevel != currentLevel)
            {
                currentLevel = newLevel;
                OnPerformanceLevelChanged?.Invoke(currentLevel);
            }
        }

        private Color GetPerformanceColor()
        {
            switch (currentLevel)
            {
                case PerformanceLevel.Excellent:
                    return new Color(0.2f, 0.8f, 0.2f, 0.8f);
                case PerformanceLevel.Good:
                    return new Color(0.6f, 0.8f, 0.2f, 0.8f);
                case PerformanceLevel.Warning:
                    return new Color(0.9f, 0.7f, 0.1f, 0.8f);
                case PerformanceLevel.Critical:
                    return new Color(0.9f, 0.2f, 0.2f, 0.8f);
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }

        #endregion
    }

    /// <summary>
    /// Performance level categories.
    /// </summary>
    public enum PerformanceLevel
    {
        Excellent,  // >= 54 FPS (90% of target)
        Good,       // >= 45 FPS
        Warning,    // >= 30 FPS
        Critical    // < 30 FPS
    }

    /// <summary>
    /// Snapshot of performance metrics at a point in time.
    /// </summary>
    [System.Serializable]
    public struct PerformanceSnapshot
    {
        public float Timestamp;
        public float CurrentFPS;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float FrameTimeMs;
        public float StabilityScore;
        public PerformanceLevel PerformanceLevel;
        public float DroppedFramePercentage;
    }
}
