using UnityEngine;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Configuration settings for backend services.
    /// ScriptableObject for easy configuration in Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "BackendConfig", menuName = "EscapeTrainRun/Backend Config")]
    public class BackendConfig : ScriptableObject
    {
        [Header("API Endpoints")]
        [Tooltip("Base URL for the backend API")]
        [SerializeField] private string apiBaseUrl = "https://api.escapetrainrun.com";

        [Tooltip("Leaderboard API endpoint")]
        [SerializeField] private string leaderboardEndpoint = "/api/leaderboard";

        [Tooltip("Cloud save API endpoint")]
        [SerializeField] private string cloudSaveEndpoint = "/api/saves";

        [Tooltip("Achievements API endpoint")]
        [SerializeField] private string achievementsEndpoint = "/api/achievements";

        [Header("Connection Settings")]
        [Tooltip("Connection timeout in seconds")]
        [SerializeField] private float connectionTimeout = 10f;

        [Tooltip("Request timeout in seconds")]
        [SerializeField] private float requestTimeout = 30f;

        [Tooltip("Number of retry attempts for failed requests")]
        [SerializeField] private int retryAttempts = 3;

        [Tooltip("Delay between retry attempts in seconds")]
        [SerializeField] private float retryDelay = 1f;

        [Header("Offline Mode")]
        [Tooltip("Enable offline fallback mode")]
        [SerializeField] private bool enableOfflineMode = true;

        [Tooltip("Time to wait before switching to offline mode")]
        [SerializeField] private float offlineModeDelay = 5f;

        [Tooltip("Sync interval when back online (seconds)")]
        [SerializeField] private float syncInterval = 60f;

        [Header("Leaderboard Settings")]
        [Tooltip("Number of entries to fetch per page")]
        [SerializeField] private int leaderboardPageSize = 50;

        [Tooltip("Cache duration for leaderboard data (seconds)")]
        [SerializeField] private float leaderboardCacheDuration = 300f;

        [Header("Cloud Save Settings")]
        [Tooltip("Auto-sync interval for cloud saves (seconds)")]
        [SerializeField] private float cloudSaveSyncInterval = 30f;

        [Tooltip("Maximum cloud save size in KB")]
        [SerializeField] private int maxCloudSaveSize = 512;

        [Header("Debug")]
        [Tooltip("Enable verbose logging")]
        [SerializeField] private bool verboseLogging = false;

        [Tooltip("Use mock data instead of real API")]
        [SerializeField] private bool useMockData = true;

        // Properties
        public string ApiBaseUrl => apiBaseUrl;
        public string LeaderboardEndpoint => $"{apiBaseUrl}{leaderboardEndpoint}";
        public string CloudSaveEndpoint => $"{apiBaseUrl}{cloudSaveEndpoint}";
        public string AchievementsEndpoint => $"{apiBaseUrl}{achievementsEndpoint}";

        public float ConnectionTimeout => connectionTimeout;
        public float RequestTimeout => requestTimeout;
        public int RetryAttempts => retryAttempts;
        public float RetryDelay => retryDelay;

        public bool EnableOfflineMode => enableOfflineMode;
        public float OfflineModeDelay => offlineModeDelay;
        public float SyncInterval => syncInterval;

        public int LeaderboardPageSize => leaderboardPageSize;
        public float LeaderboardCacheDuration => leaderboardCacheDuration;

        public float CloudSaveSyncInterval => cloudSaveSyncInterval;
        public int MaxCloudSaveSize => maxCloudSaveSize;

        public bool VerboseLogging => verboseLogging;
        public bool UseMockData => useMockData;
    }
}
