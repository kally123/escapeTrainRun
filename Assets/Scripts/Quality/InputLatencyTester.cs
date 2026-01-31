using UnityEngine;
using System.Collections.Generic;
using System;

namespace EscapeTrainRun.Quality
{
    /// <summary>
    /// Measures input latency from user input to game response.
    /// Critical for ensuring responsive controls (< 100ms target).
    /// </summary>
    public class InputLatencyTester : MonoBehaviour
    {
        public static InputLatencyTester Instance { get; private set; }

        [Header("Test Settings")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private int sampleCount = 30;
        [SerializeField] private KeyCode testKey = KeyCode.Space;

        [Header("Thresholds")]
        [SerializeField] private float targetLatencyMs = 50f;
        [SerializeField] private float warningLatencyMs = 80f;
        [SerializeField] private float criticalLatencyMs = 100f;

        // Latency samples
        private Queue<float> latencySamples = new Queue<float>();
        private float inputStartTime;
        private bool waitingForResponse;

        // Metrics
        private float currentLatencyMs;
        private float averageLatencyMs;
        private float minLatencyMs = float.MaxValue;
        private float maxLatencyMs;
        private LatencyLevel currentLevel = LatencyLevel.Excellent;

        // Public properties
        public float CurrentLatencyMs => currentLatencyMs;
        public float AverageLatencyMs => averageLatencyMs;
        public float MinLatencyMs => minLatencyMs;
        public float MaxLatencyMs => maxLatencyMs;
        public LatencyLevel Level => currentLevel;
        public bool IsTestInProgress => waitingForResponse;

        // Events
        public event Action<float> OnLatencyMeasured;
        public event Action<LatencyLevel> OnLatencyLevelChanged;

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

        private void Update()
        {
            if (!enableTesting) return;

            // Detect test key press
            if (Input.GetKeyDown(testKey) && !waitingForResponse)
            {
                StartMeasurement();
            }

            // Also measure touch input
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !waitingForResponse)
            {
                StartMeasurement();
            }
        }

        #region Public API

        /// <summary>
        /// Starts a latency measurement.
        /// </summary>
        public void StartMeasurement()
        {
            inputStartTime = Time.realtimeSinceStartup * 1000f; // Convert to ms
            waitingForResponse = true;
        }

        /// <summary>
        /// Records the response to complete measurement.
        /// Call this when the game responds to input.
        /// </summary>
        public void RecordResponse()
        {
            if (!waitingForResponse) return;

            float responseTime = Time.realtimeSinceStartup * 1000f;
            currentLatencyMs = responseTime - inputStartTime;
            waitingForResponse = false;

            // Add sample
            latencySamples.Enqueue(currentLatencyMs);
            while (latencySamples.Count > sampleCount)
            {
                latencySamples.Dequeue();
            }

            // Update metrics
            UpdateMetrics();

            OnLatencyMeasured?.Invoke(currentLatencyMs);
        }

        /// <summary>
        /// Manually adds a latency sample (for automated tests).
        /// </summary>
        public void AddSample(float latencyMs)
        {
            currentLatencyMs = latencyMs;

            latencySamples.Enqueue(latencyMs);
            while (latencySamples.Count > sampleCount)
            {
                latencySamples.Dequeue();
            }

            UpdateMetrics();
            OnLatencyMeasured?.Invoke(latencyMs);
        }

        /// <summary>
        /// Clears all samples.
        /// </summary>
        public void ClearSamples()
        {
            latencySamples.Clear();
            currentLatencyMs = 0f;
            averageLatencyMs = 0f;
            minLatencyMs = float.MaxValue;
            maxLatencyMs = 0f;
            waitingForResponse = false;
        }

        /// <summary>
        /// Gets a latency snapshot.
        /// </summary>
        public LatencySnapshot GetSnapshot()
        {
            return new LatencySnapshot
            {
                Timestamp = Time.realtimeSinceStartup,
                CurrentLatencyMs = currentLatencyMs,
                AverageLatencyMs = averageLatencyMs,
                MinLatencyMs = minLatencyMs,
                MaxLatencyMs = maxLatencyMs,
                SampleCount = latencySamples.Count,
                Level = currentLevel
            };
        }

        /// <summary>
        /// Runs an automated latency test.
        /// </summary>
        public void RunAutomatedTest(int testCount, Action<LatencyTestResult> onComplete)
        {
            StartCoroutine(AutomatedTestRoutine(testCount, onComplete));
        }

        private System.Collections.IEnumerator AutomatedTestRoutine(int testCount, Action<LatencyTestResult> onComplete)
        {
            ClearSamples();

            var samples = new List<float>();

            for (int i = 0; i < testCount; i++)
            {
                // Simulate input
                float startTime = Time.realtimeSinceStartup * 1000f;

                // Wait a frame (simulates minimum response time)
                yield return null;

                // Measure response
                float endTime = Time.realtimeSinceStartup * 1000f;
                float latency = endTime - startTime;

                samples.Add(latency);
                AddSample(latency);

                // Small delay between tests
                yield return new WaitForSeconds(0.1f);
            }

            // Generate result
            var result = new LatencyTestResult
            {
                TestCount = testCount,
                AverageLatencyMs = averageLatencyMs,
                MinLatencyMs = minLatencyMs,
                MaxLatencyMs = maxLatencyMs,
                PassedTarget = averageLatencyMs < targetLatencyMs,
                PassedCritical = averageLatencyMs < criticalLatencyMs,
                Level = currentLevel
            };

            onComplete?.Invoke(result);
        }

        #endregion

        #region Private Methods

        private void UpdateMetrics()
        {
            if (latencySamples.Count == 0) return;

            // Calculate average
            float sum = 0f;
            float min = float.MaxValue;
            float max = 0f;

            foreach (var sample in latencySamples)
            {
                sum += sample;
                min = Mathf.Min(min, sample);
                max = Mathf.Max(max, sample);
            }

            averageLatencyMs = sum / latencySamples.Count;
            minLatencyMs = min;
            maxLatencyMs = max;

            // Update level
            UpdateLatencyLevel();
        }

        private void UpdateLatencyLevel()
        {
            LatencyLevel newLevel;

            if (averageLatencyMs <= targetLatencyMs)
            {
                newLevel = LatencyLevel.Excellent;
            }
            else if (averageLatencyMs <= warningLatencyMs)
            {
                newLevel = LatencyLevel.Good;
            }
            else if (averageLatencyMs <= criticalLatencyMs)
            {
                newLevel = LatencyLevel.Warning;
            }
            else
            {
                newLevel = LatencyLevel.Critical;
            }

            if (newLevel != currentLevel)
            {
                currentLevel = newLevel;
                OnLatencyLevelChanged?.Invoke(currentLevel);
            }
        }

        #endregion
    }

    /// <summary>
    /// Latency level categories.
    /// </summary>
    public enum LatencyLevel
    {
        Excellent,  // <= 50ms
        Good,       // <= 80ms
        Warning,    // <= 100ms
        Critical    // > 100ms
    }

    /// <summary>
    /// Latency snapshot.
    /// </summary>
    [Serializable]
    public struct LatencySnapshot
    {
        public float Timestamp;
        public float CurrentLatencyMs;
        public float AverageLatencyMs;
        public float MinLatencyMs;
        public float MaxLatencyMs;
        public int SampleCount;
        public LatencyLevel Level;
    }

    /// <summary>
    /// Result of automated latency test.
    /// </summary>
    [Serializable]
    public struct LatencyTestResult
    {
        public int TestCount;
        public float AverageLatencyMs;
        public float MinLatencyMs;
        public float MaxLatencyMs;
        public bool PassedTarget;
        public bool PassedCritical;
        public LatencyLevel Level;
    }
}
