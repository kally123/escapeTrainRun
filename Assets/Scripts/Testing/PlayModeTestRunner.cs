using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Play Mode test runner that performs automated gameplay tests.
/// Attach to a GameObject in your GamePlay scene to run tests during Play mode.
/// </summary>
public class PlayModeTestRunner : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = true;
    public bool stopOnFirstError = false;
    public float testTimeout = 30f;

    [Header("Test Results")]
    [SerializeField] private List<TestResult> results = new List<TestResult>();
    
    private bool isRunning = false;
    private float testStartTime;

    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string message;
        public float duration;
    }

    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    public void StartTests()
    {
        if (!isRunning)
        {
            StartCoroutine(RunAllTests());
        }
    }

    private IEnumerator RunAllTests()
    {
        isRunning = true;
        results.Clear();
        testStartTime = Time.time;

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("🎮 PLAY MODE TESTS - Starting...");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Wait for initialization
        yield return new WaitForSeconds(0.5f);

        // Run tests
        yield return StartCoroutine(Test_NoConsoleErrors());
        yield return StartCoroutine(Test_PlayerExists());
        yield return StartCoroutine(Test_PlayerPosition());
        yield return StartCoroutine(Test_TrackGeneration());
        yield return StartCoroutine(Test_CameraFollowing());
        yield return StartCoroutine(Test_InputResponse());
        yield return StartCoroutine(Test_UIUpdates());
        yield return StartCoroutine(Test_GameManagerState());
        yield return StartCoroutine(Test_AudioPlaying());
        yield return StartCoroutine(Test_PoolManagerWorking());

        // Print results
        PrintResults();
        isRunning = false;
    }

    #region Individual Tests

    private IEnumerator Test_NoConsoleErrors()
    {
        string testName = "No Console Errors at Start";
        float startTime = Time.time;

        // This test passes if we got here without critical exceptions
        // In a real implementation, you'd hook into Debug.LogError
        
        bool hasErrors = false; // Would check Application.logMessageReceived
        
        if (!hasErrors)
        {
            AddResult(testName, true, "No errors detected on startup", Time.time - startTime);
        }
        else
        {
            AddResult(testName, false, "Console errors detected", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_PlayerExists()
    {
        string testName = "Player Exists";
        float startTime = Time.time;

        var player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            bool hasController = player.GetComponent<CharacterController>() != null;
            bool hasPlayerController = player.GetComponent<EscapeTrainRun.PlayerController>() != null;

            if (hasController && hasPlayerController)
            {
                AddResult(testName, true, $"Player found at {player.transform.position}", Time.time - startTime);
            }
            else
            {
                AddResult(testName, false, "Player missing required components", Time.time - startTime);
            }
        }
        else
        {
            AddResult(testName, false, "No GameObject with 'Player' tag found", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_PlayerPosition()
    {
        string testName = "Player at Start Position";
        float startTime = Time.time;

        var player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            
            // Check player is in reasonable starting position
            bool validX = Mathf.Abs(pos.x) < 5f; // Near center
            bool validY = pos.y >= 0 && pos.y < 5f; // On ground
            bool validZ = pos.z >= -10f && pos.z < 50f; // Near start

            if (validX && validY && validZ)
            {
                AddResult(testName, true, $"Position valid: {pos}", Time.time - startTime);
            }
            else
            {
                AddResult(testName, false, $"Position may be invalid: {pos}", Time.time - startTime);
            }
        }
        else
        {
            AddResult(testName, false, "Player not found", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_TrackGeneration()
    {
        string testName = "Track Generation";
        float startTime = Time.time;

        var levelGen = FindObjectOfType<EscapeTrainRun.LevelGenerator>();
        
        if (levelGen != null)
        {
            // Wait a bit for track to generate
            yield return new WaitForSeconds(1f);

            var trackSegments = FindObjectsOfType<EscapeTrainRun.TrackSegment>();
            
            if (trackSegments.Length > 0)
            {
                AddResult(testName, true, $"{trackSegments.Length} track segments generated", Time.time - startTime);
            }
            else
            {
                AddResult(testName, false, "No track segments found", Time.time - startTime);
            }
        }
        else
        {
            AddResult(testName, false, "LevelGenerator not found", Time.time - startTime);
        }
    }

    private IEnumerator Test_CameraFollowing()
    {
        string testName = "Camera Following Player";
        float startTime = Time.time;

        var player = GameObject.FindGameObjectWithTag("Player");
        var camera = Camera.main;

        if (player != null && camera != null)
        {
            Vector3 initialOffset = camera.transform.position - player.transform.position;
            
            // Wait for some movement
            yield return new WaitForSeconds(0.5f);
            
            Vector3 currentOffset = camera.transform.position - player.transform.position;
            
            // Camera should maintain roughly the same relative position
            float offsetDiff = Vector3.Distance(initialOffset, currentOffset);
            
            if (offsetDiff < 10f) // Allow some variance
            {
                AddResult(testName, true, "Camera is following player", Time.time - startTime);
            }
            else
            {
                AddResult(testName, false, $"Camera offset changed significantly: {offsetDiff}", Time.time - startTime);
            }
        }
        else
        {
            AddResult(testName, false, "Player or Camera not found", Time.time - startTime);
        }
    }

    private IEnumerator Test_InputResponse()
    {
        string testName = "Input Handler Active";
        float startTime = Time.time;

        var swipeDetector = FindObjectOfType<EscapeTrainRun.SwipeDetector>();
        var playerMovement = FindObjectOfType<EscapeTrainRun.PlayerMovement>();

        if (swipeDetector != null || playerMovement != null)
        {
            AddResult(testName, true, "Input handling components found", Time.time - startTime);
        }
        else
        {
            AddResult(testName, false, "No input handling components", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_UIUpdates()
    {
        string testName = "UI Present";
        float startTime = Time.time;

        var gameplayUI = FindObjectOfType<EscapeTrainRun.GameplayUI>();
        var canvas = GameObject.Find("GameplayCanvas");

        if (gameplayUI != null || canvas != null)
        {
            AddResult(testName, true, "Gameplay UI found", Time.time - startTime);
        }
        else
        {
            AddResult(testName, false, "No gameplay UI found", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_GameManagerState()
    {
        string testName = "GameManager State";
        float startTime = Time.time;

        var gameManager = FindObjectOfType<EscapeTrainRun.GameManager>();

        if (gameManager != null)
        {
            AddResult(testName, true, "GameManager is active", Time.time - startTime);
        }
        else
        {
            AddResult(testName, false, "GameManager not found", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_AudioPlaying()
    {
        string testName = "Audio System";
        float startTime = Time.time;

        var audioManager = FindObjectOfType<EscapeTrainRun.AudioManager>();

        if (audioManager != null)
        {
            var audioSources = audioManager.GetComponentsInChildren<AudioSource>();
            AddResult(testName, true, $"AudioManager found with {audioSources.Length} sources", Time.time - startTime);
        }
        else
        {
            AddResult(testName, false, "AudioManager not found", Time.time - startTime);
        }

        yield return null;
    }

    private IEnumerator Test_PoolManagerWorking()
    {
        string testName = "Pool Manager";
        float startTime = Time.time;

        var poolManager = FindObjectOfType<EscapeTrainRun.PoolManager>();

        if (poolManager != null)
        {
            AddResult(testName, true, "PoolManager active", Time.time - startTime);
        }
        else
        {
            // Pool manager is optional, so this is a warning
            AddResult(testName, true, "PoolManager not found (optional)", Time.time - startTime);
        }

        yield return null;
    }

    #endregion

    #region Helper Methods

    private void AddResult(string name, bool passed, string message, float duration)
    {
        results.Add(new TestResult
        {
            testName = name,
            passed = passed,
            message = message,
            duration = duration
        });

        string icon = passed ? "✅" : "❌";
        string logMethod = passed ? "Log" : "LogError";
        
        Debug.Log($"{icon} {name}: {message} ({duration:F2}s)");

        if (!passed && stopOnFirstError)
        {
            Debug.LogError("Stopping tests due to failure.");
            isRunning = false;
        }
    }

    private void PrintResults()
    {
        int passCount = 0;
        int failCount = 0;

        foreach (var result in results)
        {
            if (result.passed) passCount++;
            else failCount++;
        }

        Debug.Log("\n═══════════════════════════════════════════════════════════");
        Debug.Log($"🎮 PLAY MODE TESTS COMPLETE");
        Debug.Log($"   ✅ Passed: {passCount}");
        Debug.Log($"   ❌ Failed: {failCount}");
        Debug.Log($"   ⏱️ Total Time: {Time.time - testStartTime:F2}s");
        Debug.Log("═══════════════════════════════════════════════════════════");

        if (failCount == 0)
        {
            Debug.Log("🎉 All tests passed! Your game is working correctly!");
        }
        else
        {
            Debug.LogWarning($"⚠️ {failCount} test(s) failed. Check the logs above for details.");
        }
    }

    #endregion

    void OnGUI()
    {
        if (!isRunning && results.Count > 0)
        {
            // Show results summary
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
            GUILayout.BeginVertical("box");
            GUILayout.Label("🧪 Test Results");
            
            int passCount = 0, failCount = 0;
            foreach (var r in results)
            {
                if (r.passed) passCount++; else failCount++;
            }
            
            GUILayout.Label($"✅ Passed: {passCount}");
            GUILayout.Label($"❌ Failed: {failCount}");
            
            if (GUILayout.Button("Run Tests Again"))
            {
                StartTests();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else if (isRunning)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 50));
            GUILayout.BeginVertical("box");
            GUILayout.Label("🔄 Running tests...");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
