using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Cloud save data structure.
    /// Contains complete game state for synchronization.
    /// </summary>
    [Serializable]
    public class CloudSaveData
    {
        public string saveId;
        public string playerId;
        public string deviceId;
        public long timestamp;
        public int version;

        // Game progress
        public int totalCoins;
        public int highScore;
        public int totalGamesPlayed;
        public float totalDistanceTraveled;

        // Per-mode stats
        public int highScoreTrain;
        public int highScoreBus;
        public int highScorePark;

        // Character data
        public string selectedCharacterId;
        public List<string> unlockedCharacterIds = new List<string>();

        // Achievement data
        public List<string> unlockedAchievementIds = new List<string>();
        public Dictionary<string, int> achievementProgress = new Dictionary<string, int>();

        // Settings
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public bool vibrateEnabled = true;

        public CloudSaveData()
        {
            saveId = Guid.NewGuid().ToString();
            deviceId = SystemInfo.deviceUniqueIdentifier;
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            version = 1;
        }
    }

    /// <summary>
    /// Conflict resolution strategy for cloud saves.
    /// </summary>
    public enum ConflictResolution
    {
        UseLocal,
        UseCloud,
        MergePreferLocal,
        MergePreferCloud
    }

    /// <summary>
    /// Handles cloud save synchronization with Azure backend.
    /// Provides offline support and conflict resolution.
    /// </summary>
    public class CloudSaveService : MonoBehaviour, IBackendService
    {
        public static CloudSaveService Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private BackendConfig config;

        [Header("Conflict Resolution")]
        [SerializeField] private ConflictResolution defaultConflictResolution = ConflictResolution.MergePreferCloud;

        // IBackendService implementation
        public string ServiceName => "CloudSaveService";
        public bool IsConnected { get; private set; }
        public bool IsOfflineMode { get; private set; }

        // Events
        public static event Action<CloudSaveData> OnSaveUploaded;
        public static event Action<CloudSaveData> OnSaveDownloaded;
        public static event Action<CloudSaveData, CloudSaveData> OnSaveConflict;
        public static event Action<string> OnSaveError;
        public static event Action OnSyncComplete;

        // State
        private CloudSaveData localSave;
        private CloudSaveData cloudSave;
        private bool hasPendingChanges;
        private float lastSyncTime;
        private string localSavePath;

        private void Awake()
        {
            InitializeSingleton();
            InitializeLocalPath();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void InitializeLocalPath()
        {
            localSavePath = Path.Combine(Application.persistentDataPath, "cloudsave_cache.json");
        }

        private void Update()
        {
            // Auto-sync if configured
            if (IsConnected && hasPendingChanges && config != null)
            {
                if (Time.time - lastSyncTime > config.CloudSaveSyncInterval)
                {
                    _ = SyncAsync();
                }
            }
        }

        #region IBackendService Implementation

        public async Task InitializeAsync()
        {
            if (config?.VerboseLogging == true)
            {
                Debug.Log($"[{ServiceName}] Initializing...");
            }

            // Load local cache
            LoadLocalCache();

            // Connect to cloud
            await Task.Delay(100);

            IsConnected = true;
            IsOfflineMode = config?.UseMockData ?? true;

            Debug.Log($"[{ServiceName}] Initialized (Offline Mode: {IsOfflineMode})");
        }

        public async Task ShutdownAsync()
        {
            // Save any pending changes
            if (hasPendingChanges)
            {
                await UploadSaveAsync();
            }

            // Save local cache
            SaveLocalCache();

            IsConnected = false;
            Debug.Log($"[{ServiceName}] Shutdown complete");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Syncs local and cloud saves.
        /// </summary>
        public async Task<ServiceResult<CloudSaveData>> SyncAsync()
        {
            if (!IsConnected)
            {
                return ServiceResult<CloudSaveData>.Failed("Not connected", ServiceErrorCode.ConnectionLost);
            }

            try
            {
                // Download latest cloud save
                var downloadResult = await DownloadSaveAsync();

                if (downloadResult.Success && downloadResult.Data != null)
                {
                    cloudSave = downloadResult.Data;

                    // Check for conflicts
                    if (localSave != null && HasConflict(localSave, cloudSave))
                    {
                        localSave = ResolveConflict(localSave, cloudSave);
                        OnSaveConflict?.Invoke(localSave, cloudSave);
                    }
                    else if (cloudSave.timestamp > (localSave?.timestamp ?? 0))
                    {
                        // Cloud is newer
                        localSave = cloudSave;
                    }
                }

                // Upload local changes if needed
                if (hasPendingChanges && localSave != null)
                {
                    await UploadSaveAsync();
                }

                lastSyncTime = Time.time;
                hasPendingChanges = false;
                OnSyncComplete?.Invoke();

                Debug.Log($"[{ServiceName}] Sync complete");
                return ServiceResult<CloudSaveData>.Succeeded(localSave);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{ServiceName}] Sync failed: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return ServiceResult<CloudSaveData>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Downloads the latest cloud save.
        /// </summary>
        public async Task<ServiceResult<CloudSaveData>> DownloadSaveAsync()
        {
            try
            {
                if (IsOfflineMode || config?.UseMockData == true)
                {
                    // Return cached cloud save
                    await Task.Delay(100);
                    OnSaveDownloaded?.Invoke(cloudSave);
                    return ServiceResult<CloudSaveData>.Succeeded(cloudSave);
                }

                // TODO: Implement actual HTTP request
                await Task.Delay(100);
                return ServiceResult<CloudSaveData>.Succeeded(cloudSave);
            }
            catch (Exception e)
            {
                return ServiceResult<CloudSaveData>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Uploads the current local save to cloud.
        /// </summary>
        public async Task<ServiceResult<bool>> UploadSaveAsync()
        {
            if (localSave == null)
            {
                return ServiceResult<bool>.Failed("No save data to upload", ServiceErrorCode.InvalidRequest);
            }

            try
            {
                localSave.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (IsOfflineMode || config?.UseMockData == true)
                {
                    // Store locally only
                    cloudSave = DeepCopy(localSave);
                    SaveLocalCache();
                    await Task.Delay(50);
                }
                else
                {
                    // TODO: Implement actual HTTP request
                    await Task.Delay(100);
                }

                hasPendingChanges = false;
                OnSaveUploaded?.Invoke(localSave);

                Debug.Log($"[{ServiceName}] Save uploaded successfully");
                return ServiceResult<bool>.Succeeded(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{ServiceName}] Upload failed: {e.Message}");
                return ServiceResult<bool>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Updates the local save data.
        /// </summary>
        public void UpdateLocalSave(Action<CloudSaveData> updateAction)
        {
            if (localSave == null)
            {
                localSave = new CloudSaveData();
            }

            updateAction(localSave);
            localSave.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            hasPendingChanges = true;

            SaveLocalCache();
        }

        /// <summary>
        /// Gets the current save data (local).
        /// </summary>
        public CloudSaveData GetCurrentSave()
        {
            return localSave ?? new CloudSaveData();
        }

        /// <summary>
        /// Syncs from GameSaveData (SaveManager).
        /// </summary>
        public void SyncFromLocalSaveManager()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager == null || saveManager.CurrentSave == null) return;

            var gameSave = saveManager.CurrentSave;

            UpdateLocalSave(save =>
            {
                save.totalCoins = gameSave.coins;
                save.highScore = gameSave.highScore;
                save.totalGamesPlayed = gameSave.gamesPlayed;
                save.totalDistanceTraveled = gameSave.totalDistance;
                save.highScoreTrain = gameSave.highScoreTrain;
                save.highScoreBus = gameSave.highScoreBus;
                save.highScorePark = gameSave.highScorePark;
                save.selectedCharacterId = gameSave.selectedCharacter;
                save.unlockedCharacterIds = new List<string>(gameSave.unlockedCharacters);
                save.musicVolume = gameSave.settings.musicVolume;
                save.sfxVolume = gameSave.settings.sfxVolume;
                save.vibrateEnabled = gameSave.settings.vibrateEnabled;
            });
        }

        /// <summary>
        /// Syncs to GameSaveData (SaveManager).
        /// </summary>
        public void SyncToLocalSaveManager()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager == null || localSave == null) return;

            var gameSave = saveManager.CurrentSave;

            gameSave.coins = localSave.totalCoins;
            gameSave.highScore = localSave.highScore;
            gameSave.gamesPlayed = localSave.totalGamesPlayed;
            gameSave.totalDistance = localSave.totalDistanceTraveled;
            gameSave.highScoreTrain = localSave.highScoreTrain;
            gameSave.highScoreBus = localSave.highScoreBus;
            gameSave.highScorePark = localSave.highScorePark;
            gameSave.selectedCharacter = localSave.selectedCharacterId;
            gameSave.unlockedCharacters = new List<string>(localSave.unlockedCharacterIds);
            gameSave.settings.musicVolume = localSave.musicVolume;
            gameSave.settings.sfxVolume = localSave.sfxVolume;
            gameSave.settings.vibrateEnabled = localSave.vibrateEnabled;

            saveManager.SaveGameData();
        }

        /// <summary>
        /// Forces a full download from cloud (discard local).
        /// </summary>
        public async Task<ServiceResult<CloudSaveData>> ForceDownloadAsync()
        {
            var result = await DownloadSaveAsync();

            if (result.Success && result.Data != null)
            {
                localSave = result.Data;
                hasPendingChanges = false;
                SaveLocalCache();
            }

            return result;
        }

        /// <summary>
        /// Forces a full upload to cloud (overwrite cloud).
        /// </summary>
        public async Task<ServiceResult<bool>> ForceUploadAsync()
        {
            hasPendingChanges = true;
            return await UploadSaveAsync();
        }

        /// <summary>
        /// Deletes all cloud save data.
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteCloudSaveAsync()
        {
            try
            {
                cloudSave = null;
                await Task.Delay(100);

                Debug.Log($"[{ServiceName}] Cloud save deleted");
                return ServiceResult<bool>.Succeeded(true);
            }
            catch (Exception e)
            {
                return ServiceResult<bool>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        #endregion

        #region Private Methods

        private void LoadLocalCache()
        {
            try
            {
                if (File.Exists(localSavePath))
                {
                    string json = File.ReadAllText(localSavePath);
                    localSave = JsonUtility.FromJson<CloudSaveData>(json);
                    Debug.Log($"[{ServiceName}] Loaded local cache");
                }
                else
                {
                    localSave = new CloudSaveData();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{ServiceName}] Failed to load local cache: {e.Message}");
                localSave = new CloudSaveData();
            }
        }

        private void SaveLocalCache()
        {
            try
            {
                string json = JsonUtility.ToJson(localSave, true);
                File.WriteAllText(localSavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{ServiceName}] Failed to save local cache: {e.Message}");
            }
        }

        private bool HasConflict(CloudSaveData local, CloudSaveData cloud)
        {
            if (cloud == null) return false;

            // Conflict if both have changes since last sync
            return local.deviceId != cloud.deviceId &&
                   Math.Abs(local.timestamp - cloud.timestamp) < 60; // Within 1 minute
        }

        private CloudSaveData ResolveConflict(CloudSaveData local, CloudSaveData cloud)
        {
            switch (defaultConflictResolution)
            {
                case ConflictResolution.UseLocal:
                    return local;

                case ConflictResolution.UseCloud:
                    return cloud;

                case ConflictResolution.MergePreferLocal:
                    return MergeSaves(local, cloud, preferFirst: true);

                case ConflictResolution.MergePreferCloud:
                    return MergeSaves(local, cloud, preferFirst: false);

                default:
                    return cloud;
            }
        }

        private CloudSaveData MergeSaves(CloudSaveData first, CloudSaveData second, bool preferFirst)
        {
            var primary = preferFirst ? first : second;
            var secondary = preferFirst ? second : first;

            var merged = new CloudSaveData
            {
                // Take maximum values
                totalCoins = Mathf.Max(first.totalCoins, second.totalCoins),
                highScore = Mathf.Max(first.highScore, second.highScore),
                totalGamesPlayed = Mathf.Max(first.totalGamesPlayed, second.totalGamesPlayed),
                totalDistanceTraveled = Mathf.Max(first.totalDistanceTraveled, second.totalDistanceTraveled),
                highScoreTrain = Mathf.Max(first.highScoreTrain, second.highScoreTrain),
                highScoreBus = Mathf.Max(first.highScoreBus, second.highScoreBus),
                highScorePark = Mathf.Max(first.highScorePark, second.highScorePark),

                // Take from primary
                selectedCharacterId = primary.selectedCharacterId ?? secondary.selectedCharacterId,
                musicVolume = primary.musicVolume,
                sfxVolume = primary.sfxVolume,
                vibrateEnabled = primary.vibrateEnabled,

                // Merge lists (union)
                unlockedCharacterIds = new List<string>(
                    new HashSet<string>(first.unlockedCharacterIds ?? new List<string>())
                    .Union(second.unlockedCharacterIds ?? new List<string>())
                ),
                unlockedAchievementIds = new List<string>(
                    new HashSet<string>(first.unlockedAchievementIds ?? new List<string>())
                    .Union(second.unlockedAchievementIds ?? new List<string>())
                )
            };

            return merged;
        }

        private CloudSaveData DeepCopy(CloudSaveData original)
        {
            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<CloudSaveData>(json);
        }

        #endregion
    }
}
