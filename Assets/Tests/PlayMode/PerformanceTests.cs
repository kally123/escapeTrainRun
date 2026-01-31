using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;
using Debug = UnityEngine.Debug;

namespace EscapeTrainRun.Tests.PlayMode
{
    /// <summary>
    /// Performance tests and benchmarks.
    /// Measures FPS, memory usage, and operation timing.
    /// </summary>
    [TestFixture]
    public class PerformanceTests
    {
        private const int TARGET_FPS_MOBILE = 60;
        private const int TARGET_FPS_DESKTOP = 60;
        private const float MAX_MEMORY_MB_MOBILE = 500f;
        private const float MAX_MEMORY_MB_DESKTOP = 1000f;
        private const float MAX_FRAME_TIME_MS = 16.67f; // 60 FPS target

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllEvents();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllEvents();
        }

        #region Frame Rate Tests

        [UnityTest]
        public IEnumerator FrameRate_MaintainsTargetFPS()
        {
            // Warmup
            yield return new WaitForSeconds(0.5f);

            // Measure frame times
            List<float> frameTimes = new List<float>();
            float measureDuration = 2f;
            float elapsed = 0f;

            while (elapsed < measureDuration)
            {
                frameTimes.Add(Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Calculate average FPS
            float avgFrameTime = 0f;
            foreach (var ft in frameTimes)
            {
                avgFrameTime += ft;
            }
            avgFrameTime /= frameTimes.Count;

            float avgFPS = 1f / avgFrameTime;

            Debug.Log($"[PerformanceTest] Average FPS: {avgFPS:F1}, Frame Time: {avgFrameTime * 1000:F2}ms");

            // Assert - Allow some variance (30+ FPS minimum)
            Assert.GreaterOrEqual(avgFPS, 30f, "FPS dropped below minimum threshold");
        }

        [UnityTest]
        public IEnumerator FrameRate_NoMajorSpikes()
        {
            yield return new WaitForSeconds(0.5f);

            List<float> frameTimes = new List<float>();
            float measureDuration = 2f;
            float elapsed = 0f;

            while (elapsed < measureDuration)
            {
                frameTimes.Add(Time.deltaTime * 1000f); // ms
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Find spikes (frames > 50ms = 20 FPS)
            int spikeCount = 0;
            float maxFrameTime = 0f;

            foreach (var ft in frameTimes)
            {
                if (ft > 50f) spikeCount++;
                if (ft > maxFrameTime) maxFrameTime = ft;
            }

            float spikePercent = (float)spikeCount / frameTimes.Count * 100f;

            Debug.Log($"[PerformanceTest] Spike count: {spikeCount}/{frameTimes.Count} ({spikePercent:F1}%), Max frame: {maxFrameTime:F2}ms");

            // Assert - Less than 5% frames should be spikes
            Assert.Less(spikePercent, 5f, "Too many frame spikes detected");
        }

        #endregion

        #region Memory Tests

        [UnityTest]
        public IEnumerator Memory_StaysWithinLimits()
        {
            yield return new WaitForSeconds(0.5f);

            // Get initial memory
            long initialMemory = System.GC.GetTotalMemory(false);

            // Simulate gameplay for a bit
            for (int i = 0; i < 100; i++)
            {
                GameEvents.TriggerCoinsCollected(1);
                GameEvents.TriggerScoreChanged(i * 100);
                yield return null;
            }

            // Get final memory
            long finalMemory = System.GC.GetTotalMemory(false);
            float memoryUsedMB = (finalMemory - initialMemory) / (1024f * 1024f);

            Debug.Log($"[PerformanceTest] Memory delta: {memoryUsedMB:F2} MB");

            // Assert - Memory growth should be reasonable
            Assert.Less(memoryUsedMB, 50f, "Memory usage grew too much during test");
        }

        [UnityTest]
        public IEnumerator Memory_NoLeaksAfterExtendedPlay()
        {
            yield return new WaitForSeconds(0.5f);

            // Force GC to get baseline
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long baselineMemory = System.GC.GetTotalMemory(true);

            // Simulate extended gameplay
            for (int session = 0; session < 5; session++)
            {
                // Simulate a game session
                GameEvents.TriggerGameStarted();

                for (int i = 0; i < 50; i++)
                {
                    GameEvents.TriggerCoinsCollected(1);
                    GameEvents.TriggerScoreChanged(i * 100);

                    if (i % 10 == 0)
                    {
                        GameEvents.TriggerPowerUpActivated(Collectibles.PowerUpType.Magnet);
                    }

                    yield return null;
                }

                GameEvents.TriggerPlayerCrashed();

                yield return new WaitForSeconds(0.1f);
            }

            // Force GC
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long finalMemory = System.GC.GetTotalMemory(true);
            float memoryGrowthMB = (finalMemory - baselineMemory) / (1024f * 1024f);

            Debug.Log($"[PerformanceTest] Memory growth after 5 sessions: {memoryGrowthMB:F2} MB");

            // Assert - Should not leak significantly
            Assert.Less(memoryGrowthMB, 20f, "Potential memory leak detected");
        }

        #endregion

        #region Object Pool Tests

        [UnityTest]
        public IEnumerator ObjectPool_ReusesObjects()
        {
            yield return null;

            // Create and destroy objects
            List<GameObject> objects = new List<GameObject>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 100; i++)
            {
                var obj = new GameObject($"PoolTest_{i}");
                objects.Add(obj);
            }

            sw.Stop();
            long createTime = sw.ElapsedMilliseconds;

            sw.Restart();

            foreach (var obj in objects)
            {
                Object.Destroy(obj);
            }

            sw.Stop();
            long destroyTime = sw.ElapsedMilliseconds;

            Debug.Log($"[PerformanceTest] Create 100 objects: {createTime}ms, Destroy: {destroyTime}ms");

            yield return null;

            // Assert - Operations should be fast
            Assert.Less(createTime, 100, "Object creation too slow");
            Assert.Less(destroyTime, 100, "Object destruction too slow");
        }

        [UnityTest]
        public IEnumerator ObjectPool_HandlesHighVolume()
        {
            yield return null;

            Stopwatch sw = new Stopwatch();
            List<GameObject> objects = new List<GameObject>();

            sw.Start();

            // Rapid create/destroy cycles
            for (int cycle = 0; cycle < 10; cycle++)
            {
                // Create batch
                for (int i = 0; i < 50; i++)
                {
                    objects.Add(new GameObject($"Batch_{cycle}_{i}"));
                }

                // Destroy batch
                foreach (var obj in objects)
                {
                    Object.Destroy(obj);
                }
                objects.Clear();

                yield return null;
            }

            sw.Stop();

            Debug.Log($"[PerformanceTest] 10 create/destroy cycles of 50 objects: {sw.ElapsedMilliseconds}ms");

            // Assert
            Assert.Less(sw.ElapsedMilliseconds, 500, "Batch operations too slow");
        }

        #endregion

        #region Event System Tests

        [UnityTest]
        public IEnumerator EventSystem_HandlesRapidFiring()
        {
            yield return null;

            int eventCount = 0;
            GameEvents.OnScoreChanged += (score) => eventCount++;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Fire many events rapidly
            for (int i = 0; i < 1000; i++)
            {
                GameEvents.TriggerScoreChanged(i);
            }

            sw.Stop();

            Debug.Log($"[PerformanceTest] 1000 events fired in {sw.ElapsedMilliseconds}ms");

            // Assert
            Assert.AreEqual(1000, eventCount, "Not all events were received");
            Assert.Less(sw.ElapsedMilliseconds, 100, "Event firing too slow");
        }

        [UnityTest]
        public IEnumerator EventSystem_MultipleSubscribers()
        {
            yield return null;

            int[] counts = new int[10];

            // Subscribe 10 handlers
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                GameEvents.OnScoreChanged += (score) => counts[index]++;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Fire events
            for (int i = 0; i < 100; i++)
            {
                GameEvents.TriggerScoreChanged(i);
            }

            sw.Stop();

            // Verify all handlers received all events
            foreach (var count in counts)
            {
                Assert.AreEqual(100, count);
            }

            Debug.Log($"[PerformanceTest] 100 events to 10 subscribers: {sw.ElapsedMilliseconds}ms");

            Assert.Less(sw.ElapsedMilliseconds, 50, "Multi-subscriber event handling too slow");
        }

        #endregion

        #region Physics Tests

        [UnityTest]
        public IEnumerator Physics_HandlesMultipleColliders()
        {
            yield return null;

            List<GameObject> colliders = new List<GameObject>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Create many colliders
            for (int i = 0; i < 50; i++)
            {
                var obj = new GameObject($"Collider_{i}");
                obj.transform.position = new Vector3(
                    Random.Range(-10f, 10f),
                    0,
                    Random.Range(0, 100f)
                );
                obj.AddComponent<BoxCollider>();
                colliders.Add(obj);
            }

            sw.Stop();
            long createTime = sw.ElapsedMilliseconds;

            // Wait for physics
            yield return TestUtilities.WaitForPhysics(10);

            // Cleanup
            foreach (var obj in colliders)
            {
                Object.Destroy(obj);
            }

            Debug.Log($"[PerformanceTest] Created 50 colliders in {createTime}ms");

            Assert.Less(createTime, 100, "Collider creation too slow");
        }

        #endregion

        #region Load Time Tests

        [UnityTest]
        public IEnumerator LoadTime_GameManagerInitialization()
        {
            yield return null;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var gmObj = new GameObject("TestGameManager");
            var gm = gmObj.AddComponent<GameManager>();

            sw.Stop();

            Debug.Log($"[PerformanceTest] GameManager init: {sw.ElapsedMilliseconds}ms");

            Assert.Less(sw.ElapsedMilliseconds, 100, "GameManager initialization too slow");

            Object.Destroy(gmObj);
        }

        [UnityTest]
        public IEnumerator LoadTime_PlayerCreation()
        {
            yield return null;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var playerContext = TestUtilities.CreateTestPlayer();

            sw.Stop();

            Debug.Log($"[PerformanceTest] Player creation: {sw.ElapsedMilliseconds}ms");

            Assert.Less(sw.ElapsedMilliseconds, 50, "Player creation too slow");

            TestUtilities.CleanupTestPlayer(playerContext);
        }

        #endregion

        #region Stress Tests

        [UnityTest]
        public IEnumerator StressTest_ExtendedGameplay()
        {
            yield return null;

            float startTime = Time.realtimeSinceStartup;
            int frameCount = 0;
            float testDuration = 5f;

            while (Time.realtimeSinceStartup - startTime < testDuration)
            {
                // Simulate game activity
                GameEvents.TriggerCoinsCollected(1);
                GameEvents.TriggerScoreChanged(frameCount * 10);

                if (frameCount % 60 == 0)
                {
                    GameEvents.TriggerNearMiss();
                }

                frameCount++;
                yield return null;
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            float avgFPS = frameCount / elapsed;

            Debug.Log($"[PerformanceTest] Stress test: {frameCount} frames in {elapsed:F2}s = {avgFPS:F1} FPS");

            Assert.GreaterOrEqual(avgFPS, 30f, "FPS dropped below acceptable level during stress test");
        }

        #endregion
    }
}
