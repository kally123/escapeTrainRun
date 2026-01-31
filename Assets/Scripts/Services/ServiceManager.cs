using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Central service manager for all backend services.
    /// Handles initialization, connection status, and service lifecycle.
    /// </summary>
    public class ServiceManager : MonoBehaviour
    {
        public static ServiceManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private BackendConfig config;

        [Header("Services")]
        [SerializeField] private LeaderboardService leaderboardService;
        [SerializeField] private CloudSaveService cloudSaveService;
        [SerializeField] private AchievementService achievementService;

        [Header("Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private float connectionCheckInterval = 30f;

        // State
        private bool isInitialized;
        private bool isOnline;
        private float lastConnectionCheck;
        private List<IBackendService> allServices = new List<IBackendService>();

        // Events
        public static event Action OnServicesInitialized;
        public static event Action<bool> OnConnectionStatusChanged;
        public static event Action<string, string> OnServiceError;

        // Properties
        public bool IsInitialized => isInitialized;
        public bool IsOnline => isOnline;
        public BackendConfig Config => config;
        public LeaderboardService Leaderboard => leaderboardService;
        public CloudSaveService CloudSave => cloudSaveService;
        public AchievementService Achievements => achievementService;

        private void Awake()
        {
            InitializeSingleton();
            CollectServices();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void CollectServices()
        {
            // Find or create services
            if (leaderboardService == null)
                leaderboardService = GetComponentInChildren<LeaderboardService>();

            if (cloudSaveService == null)
                cloudSaveService = GetComponentInChildren<CloudSaveService>();

            if (achievementService == null)
                achievementService = GetComponentInChildren<AchievementService>();

            // Add to list
            if (leaderboardService != null) allServices.Add(leaderboardService);
            if (cloudSaveService != null) allServices.Add(cloudSaveService);
            if (achievementService != null) allServices.Add(achievementService);
        }

        private async void Start()
        {
            if (autoInitialize)
            {
                await InitializeServicesAsync();
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            // Periodic connection check
            if (Time.time - lastConnectionCheck > connectionCheckInterval)
            {
                CheckConnectionStatus();
                lastConnectionCheck = Time.time;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App going to background - save state
                _ = SyncAllServicesAsync();
            }
        }

        private void OnApplicationQuit()
        {
            _ = ShutdownServicesAsync();
        }

        #region Public API

        /// <summary>
        /// Initializes all backend services.
        /// </summary>
        public async Task InitializeServicesAsync()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[ServiceManager] Services already initialized");
                return;
            }

            Debug.Log("[ServiceManager] Initializing services...");

            try
            {
                // Initialize all services in parallel
                var initTasks = new List<Task>();
                foreach (var service in allServices)
                {
                    initTasks.Add(InitializeServiceAsync(service));
                }

                await Task.WhenAll(initTasks);

                isInitialized = true;
                CheckConnectionStatus();

                Debug.Log("[ServiceManager] All services initialized");
                OnServicesInitialized?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServiceManager] Failed to initialize services: {e.Message}");
            }
        }

        /// <summary>
        /// Shuts down all backend services.
        /// </summary>
        public async Task ShutdownServicesAsync()
        {
            if (!isInitialized) return;

            Debug.Log("[ServiceManager] Shutting down services...");

            var shutdownTasks = new List<Task>();
            foreach (var service in allServices)
            {
                shutdownTasks.Add(ShutdownServiceAsync(service));
            }

            await Task.WhenAll(shutdownTasks);

            isInitialized = false;
            Debug.Log("[ServiceManager] All services shut down");
        }

        /// <summary>
        /// Syncs all services with backend.
        /// </summary>
        public async Task SyncAllServicesAsync()
        {
            if (!isInitialized) return;

            Debug.Log("[ServiceManager] Syncing all services...");

            try
            {
                // Sync cloud save first
                if (cloudSaveService != null)
                {
                    await cloudSaveService.SyncAsync();
                }

                // Then sync achievements
                if (achievementService != null)
                {
                    await achievementService.SyncProgressAsync();
                }

                Debug.Log("[ServiceManager] Sync complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServiceManager] Sync failed: {e.Message}");
            }
        }

        /// <summary>
        /// Gets a service by type.
        /// </summary>
        public T GetService<T>() where T : class, IBackendService
        {
            foreach (var service in allServices)
            {
                if (service is T typedService)
                {
                    return typedService;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if all services are connected.
        /// </summary>
        public bool AreAllServicesConnected()
        {
            foreach (var service in allServices)
            {
                if (!service.IsConnected)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets connection status for all services.
        /// </summary>
        public Dictionary<string, bool> GetServiceConnectionStatus()
        {
            var status = new Dictionary<string, bool>();
            foreach (var service in allServices)
            {
                status[service.ServiceName] = service.IsConnected;
            }
            return status;
        }

        /// <summary>
        /// Reconnects a specific service.
        /// </summary>
        public async Task ReconnectServiceAsync(string serviceName)
        {
            var service = allServices.Find(s => s.ServiceName == serviceName);
            if (service == null) return;

            await service.ShutdownAsync();
            await service.InitializeAsync();
        }

        /// <summary>
        /// Submits game result to all relevant services.
        /// </summary>
        public async Task SubmitGameResultAsync(
            int score,
            Environment.ThemeType gameMode,
            int coinsCollected,
            float distanceTraveled)
        {
            // Submit to leaderboard
            if (leaderboardService != null)
            {
                await leaderboardService.SubmitScoreAsync(score, gameMode, coinsCollected, distanceTraveled);
            }

            // Update cloud save
            if (cloudSaveService != null)
            {
                cloudSaveService.UpdateLocalSave(save =>
                {
                    if (score > save.highScore) save.highScore = score;
                    save.totalCoins += coinsCollected;
                    save.totalGamesPlayed++;
                    save.totalDistanceTraveled += distanceTraveled;

                    switch (gameMode)
                    {
                        case Environment.ThemeType.Train:
                            if (score > save.highScoreTrain) save.highScoreTrain = score;
                            break;
                        case Environment.ThemeType.Bus:
                            if (score > save.highScoreBus) save.highScoreBus = score;
                            break;
                        case Environment.ThemeType.Park:
                            if (score > save.highScorePark) save.highScorePark = score;
                            break;
                    }
                });
            }

            // Sync achievements (handled automatically via events)
        }

        #endregion

        #region Private Methods

        private async Task InitializeServiceAsync(IBackendService service)
        {
            try
            {
                await service.InitializeAsync();
                Debug.Log($"[ServiceManager] {service.ServiceName} initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServiceManager] {service.ServiceName} initialization failed: {e.Message}");
                OnServiceError?.Invoke(service.ServiceName, e.Message);
            }
        }

        private async Task ShutdownServiceAsync(IBackendService service)
        {
            try
            {
                await service.ShutdownAsync();
                Debug.Log($"[ServiceManager] {service.ServiceName} shut down");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServiceManager] {service.ServiceName} shutdown failed: {e.Message}");
            }
        }

        private void CheckConnectionStatus()
        {
            bool wasOnline = isOnline;
            isOnline = Application.internetReachability != NetworkReachability.NotReachable;

            if (wasOnline != isOnline)
            {
                Debug.Log($"[ServiceManager] Connection status changed: {(isOnline ? "Online" : "Offline")}");
                OnConnectionStatusChanged?.Invoke(isOnline);

                if (isOnline)
                {
                    // Reconnect services that were offline
                    _ = SyncAllServicesAsync();
                }
            }
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Initialize Services")]
        private void EditorInitializeServices()
        {
            _ = InitializeServicesAsync();
        }

        [ContextMenu("Sync All Services")]
        private void EditorSyncServices()
        {
            _ = SyncAllServicesAsync();
        }

        [ContextMenu("Print Service Status")]
        private void EditorPrintStatus()
        {
            foreach (var service in allServices)
            {
                Debug.Log($"{service.ServiceName}: Connected={service.IsConnected}, Offline={service.IsOfflineMode}");
            }
        }
#endif

        #endregion
    }
}
