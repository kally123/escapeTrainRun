using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System;

namespace EscapeTrainRun.Quality
{
    /// <summary>
    /// Tracks memory usage and detects potential memory leaks.
    /// Provides data for quality checks and memory optimization.
    /// </summary>
    public class MemoryTracker : MonoBehaviour
    {
        public static MemoryTracker Instance { get; private set; }

        [Header("Tracking Settings")]
        [SerializeField] private bool enableTracking = true;
        [SerializeField] private float sampleInterval = 5f;
        [SerializeField] private int maxSamples = 100;
        [SerializeField] private float leakDetectionMinutes = 5f;

        [Header("Thresholds")]
        [SerializeField] private float warningMemoryMB = 300f;
        [SerializeField] private float criticalMemoryMB = 450f;
        [SerializeField] private float maxMemoryMB = 500f;
        [SerializeField] private float leakGrowthRateMBPerMin = 5f;

        [Header("Display")]
        [SerializeField] private bool showOnScreen = false;
        [SerializeField] private Rect displayRect = new Rect(10, 120, 200, 80);

        // Memory samples
        private List<MemorySample> memorySamples = new List<MemorySample>();
        private float lastSampleTime;

        // Current metrics
        private float currentMemoryMB;
        private float peakMemoryMB;
        private float averageMemoryMB;
        private float memoryGrowthRate;
        private bool potentialLeak;
        private MemoryLevel currentLevel = MemoryLevel.Normal;

        // GC tracking
        private int gcCollectionCount;
        private float lastGCTime;
        private int gcCountSinceStart;

        // Public properties
        public float CurrentMemoryMB => currentMemoryMB;
        public float PeakMemoryMB => peakMemoryMB;
        public float AverageMemoryMB => averageMemoryMB;
        public float MemoryGrowthRateMBPerMin => memoryGrowthRate;
        public bool PotentialLeakDetected => potentialLeak;
        public MemoryLevel Level => currentLevel;
        public int GCCollectionCount => gcCountSinceStart;
        public float MemoryUsagePercent => maxMemoryMB > 0 ? currentMemoryMB / maxMemoryMB * 100f : 0f;

        // Events
        public event Action<float> OnMemoryUpdated;
        public event Action<MemoryLevel> OnMemoryLevelChanged;
        public event Action OnPotentialLeakDetected;
        public event Action OnGarbageCollected;

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
            gcCollectionCount = GC.CollectionCount(0);
            TakeSample();
        }

        private void Update()
        {
            if (!enableTracking) return;

            // Check for GC
            int currentGCCount = GC.CollectionCount(0);
            if (currentGCCount > gcCollectionCount)
            {
                gcCollectionCount = currentGCCount;
                gcCountSinceStart++;
                lastGCTime = Time.realtimeSinceStartup;
                OnGarbageCollected?.Invoke();
            }

            // Sample at interval
            if (Time.realtimeSinceStartup - lastSampleTime >= sampleInterval)
            {
                TakeSample();
                lastSampleTime = Time.realtimeSinceStartup;
            }
        }

        private void OnGUI()
        {
            if (!showOnScreen) return;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 14;

            Color bgColor = GetMemoryColor();
            GUI.backgroundColor = bgColor;

            GUILayout.BeginArea(displayRect, style);
            GUILayout.Label($"Memory: {currentMemoryMB:F1} MB");
            GUILayout.Label($"Peak: {peakMemoryMB:F1} MB");
            GUILayout.Label($"Growth: {memoryGrowthRate:F2} MB/min");
            GUILayout.Label($"GC Count: {gcCountSinceStart}");
            if (potentialLeak)
            {
                GUILayout.Label("⚠ POTENTIAL LEAK");
            }
            GUILayout.EndArea();
        }

        #region Public API

        /// <summary>
        /// Takes a memory sample immediately.
        /// </summary>
        public void TakeSample()
        {
            currentMemoryMB = GetCurrentMemoryMB();
            peakMemoryMB = Mathf.Max(peakMemoryMB, currentMemoryMB);

            var sample = new MemorySample
            {
                Timestamp = Time.realtimeSinceStartup,
                TotalMemoryMB = currentMemoryMB,
                MonoHeapMB = GetMonoHeapMB(),
                GfxMemoryMB = GetGfxMemoryMB()
            };

            memorySamples.Add(sample);

            // Limit samples
            while (memorySamples.Count > maxSamples)
            {
                memorySamples.RemoveAt(0);
            }

            // Update metrics
            UpdateMetrics();

            OnMemoryUpdated?.Invoke(currentMemoryMB);
        }

        /// <summary>
        /// Forces garbage collection.
        /// </summary>
        public void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Take sample after GC
            TakeSample();
        }

        /// <summary>
        /// Clears all memory samples.
        /// </summary>
        public void ClearSamples()
        {
            memorySamples.Clear();
            peakMemoryMB = currentMemoryMB;
            potentialLeak = false;
        }

        /// <summary>
        /// Gets a memory snapshot.
        /// </summary>
        public MemorySnapshot GetSnapshot()
        {
            return new MemorySnapshot
            {
                Timestamp = Time.realtimeSinceStartup,
                TotalMemoryMB = currentMemoryMB,
                PeakMemoryMB = peakMemoryMB,
                AverageMemoryMB = averageMemoryMB,
                GrowthRateMBPerMin = memoryGrowthRate,
                MonoHeapMB = GetMonoHeapMB(),
                GfxMemoryMB = GetGfxMemoryMB(),
                GCCollectionCount = gcCountSinceStart,
                PotentialLeak = potentialLeak,
                Level = currentLevel
            };
        }

        /// <summary>
        /// Gets memory history for graphing.
        /// </summary>
        public List<MemorySample> GetHistory()
        {
            return new List<MemorySample>(memorySamples);
        }

        /// <summary>
        /// Enables/disables on-screen display.
        /// </summary>
        public void SetShowOnScreen(bool show)
        {
            showOnScreen = show;
        }

        #endregion

        #region Private Methods

        private float GetCurrentMemoryMB()
        {
            return Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        }

        private float GetMonoHeapMB()
        {
            return Profiler.GetMonoHeapSizeLong() / (1024f * 1024f);
        }

        private float GetGfxMemoryMB()
        {
            return Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);
        }

        private void UpdateMetrics()
        {
            if (memorySamples.Count == 0) return;

            // Calculate average
            float sum = 0f;
            foreach (var sample in memorySamples)
            {
                sum += sample.TotalMemoryMB;
            }
            averageMemoryMB = sum / memorySamples.Count;

            // Calculate growth rate
            CalculateGrowthRate();

            // Detect potential leak
            DetectPotentialLeak();

            // Update memory level
            UpdateMemoryLevel();
        }

        private void CalculateGrowthRate()
        {
            if (memorySamples.Count < 2)
            {
                memoryGrowthRate = 0f;
                return;
            }

            // Use linear regression for more accurate rate
            float minDuration = leakDetectionMinutes * 60f;
            var recentSamples = memorySamples.FindAll(s =>
                Time.realtimeSinceStartup - s.Timestamp < minDuration);

            if (recentSamples.Count < 2)
            {
                memoryGrowthRate = 0f;
                return;
            }

            var first = recentSamples[0];
            var last = recentSamples[recentSamples.Count - 1];

            float timeDiffMin = (last.Timestamp - first.Timestamp) / 60f;
            if (timeDiffMin > 0)
            {
                memoryGrowthRate = (last.TotalMemoryMB - first.TotalMemoryMB) / timeDiffMin;
            }
        }

        private void DetectPotentialLeak()
        {
            // Check if memory is consistently growing
            bool wasLeaking = potentialLeak;
            potentialLeak = memoryGrowthRate > leakGrowthRateMBPerMin &&
                           memorySamples.Count >= maxSamples / 2;

            if (potentialLeak && !wasLeaking)
            {
                Debug.LogWarning($"[MemoryTracker] Potential memory leak detected! " +
                               $"Growth rate: {memoryGrowthRate:F2} MB/min");
                OnPotentialLeakDetected?.Invoke();
            }
        }

        private void UpdateMemoryLevel()
        {
            MemoryLevel newLevel;

            if (currentMemoryMB >= criticalMemoryMB || potentialLeak)
            {
                newLevel = MemoryLevel.Critical;
            }
            else if (currentMemoryMB >= warningMemoryMB)
            {
                newLevel = MemoryLevel.Warning;
            }
            else
            {
                newLevel = MemoryLevel.Normal;
            }

            if (newLevel != currentLevel)
            {
                currentLevel = newLevel;
                OnMemoryLevelChanged?.Invoke(currentLevel);

                if (currentLevel == MemoryLevel.Critical)
                {
                    Debug.LogWarning($"[MemoryTracker] Memory critical: {currentMemoryMB:F1} MB");
                }
            }
        }

        private Color GetMemoryColor()
        {
            switch (currentLevel)
            {
                case MemoryLevel.Normal:
                    return new Color(0.2f, 0.8f, 0.2f, 0.8f);
                case MemoryLevel.Warning:
                    return new Color(0.9f, 0.7f, 0.1f, 0.8f);
                case MemoryLevel.Critical:
                    return new Color(0.9f, 0.2f, 0.2f, 0.8f);
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }

        #endregion
    }

    /// <summary>
    /// Memory level categories.
    /// </summary>
    public enum MemoryLevel
    {
        Normal,     // < 300 MB
        Warning,    // < 450 MB
        Critical    // >= 450 MB or leak detected
    }

    /// <summary>
    /// Single memory sample.
    /// </summary>
    [Serializable]
    public struct MemorySample
    {
        public float Timestamp;
        public float TotalMemoryMB;
        public float MonoHeapMB;
        public float GfxMemoryMB;
    }

    /// <summary>
    /// Complete memory snapshot.
    /// </summary>
    [Serializable]
    public struct MemorySnapshot
    {
        public float Timestamp;
        public float TotalMemoryMB;
        public float PeakMemoryMB;
        public float AverageMemoryMB;
        public float GrowthRateMBPerMin;
        public float MonoHeapMB;
        public float GfxMemoryMB;
        public int GCCollectionCount;
        public bool PotentialLeak;
        public MemoryLevel Level;
    }
}
