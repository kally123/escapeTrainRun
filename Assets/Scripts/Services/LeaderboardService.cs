using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Leaderboard entry data structure.
    /// Immutable record following domain model guidelines.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public string entryId;
        public string playerId;
        public string playerName;
        public int score;
        public int rank;
        public string gameMode;
        public int coinsCollected;
        public float distanceTraveled;
        public string timestamp;

        public LeaderboardEntry()
        {
            entryId = Guid.NewGuid().ToString();
            timestamp = DateTime.UtcNow.ToString("o");
        }

        public LeaderboardEntry(string playerId, string playerName, int score, ThemeType theme)
        {
            this.entryId = Guid.NewGuid().ToString();
            this.playerId = playerId;
            this.playerName = playerName;
            this.score = score;
            this.gameMode = theme.ToString().ToLower();
            this.timestamp = DateTime.UtcNow.ToString("o");
        }
    }

    /// <summary>
    /// Leaderboard category for filtering.
    /// </summary>
    public enum LeaderboardCategory
    {
        GlobalAllTime,
        GlobalWeekly,
        GlobalDaily,
        ByGameMode
    }

    /// <summary>
    /// Leaderboard page result.
    /// </summary>
    public class LeaderboardPage
    {
        public List<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasMorePages => (PageNumber + 1) * PageSize < TotalCount;
    }

    /// <summary>
    /// Handles leaderboard operations with Azure backend.
    /// Implements caching and offline fallback.
    /// </summary>
    public class LeaderboardService : MonoBehaviour, IBackendService
    {
        public static LeaderboardService Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private BackendConfig config;

        // IBackendService implementation
        public string ServiceName => "LeaderboardService";
        public bool IsConnected { get; private set; }
        public bool IsOfflineMode { get; private set; }

        // Events
        public static event Action<LeaderboardPage> OnLeaderboardLoaded;
        public static event Action<LeaderboardEntry> OnScoreSubmitted;
        public static event Action<int> OnPlayerRankUpdated;
        public static event Action<string> OnLeaderboardError;

        // Caching
        private Dictionary<string, LeaderboardPage> cachedPages = new Dictionary<string, LeaderboardPage>();
        private Dictionary<string, float> cacheTimestamps = new Dictionary<string, float>();
        private List<LeaderboardEntry> pendingSubmissions = new List<LeaderboardEntry>();

        // Mock data for testing
        private List<LeaderboardEntry> mockLeaderboard = new List<LeaderboardEntry>();
        private string currentPlayerId;
        private string currentPlayerName;

        private void Awake()
        {
            InitializeSingleton();
            GenerateMockData();
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

        private void GenerateMockData()
        {
            // Generate mock leaderboard entries
            string[] names = { "SuperRunner", "SpeedKing", "CoinMaster", "TrainHero", "JumpChamp",
                               "SlideNinja", "PowerPlayer", "StarRunner", "MegaScore", "TopGamer",
                               "FastFeet", "GoldHunter", "TrackStar", "SwiftPlayer", "ChampionX" };

            ThemeType[] themes = { ThemeType.Train, ThemeType.Bus, ThemeType.Park };

            for (int i = 0; i < 100; i++)
            {
                var entry = new LeaderboardEntry
                {
                    entryId = Guid.NewGuid().ToString(),
                    playerId = $"player_{i:D4}",
                    playerName = names[i % names.Length] + (i / names.Length > 0 ? $"_{i / names.Length}" : ""),
                    score = UnityEngine.Random.Range(10000, 500000),
                    gameMode = themes[i % themes.Length].ToString().ToLower(),
                    coinsCollected = UnityEngine.Random.Range(50, 500),
                    distanceTraveled = UnityEngine.Random.Range(500f, 5000f),
                    timestamp = DateTime.UtcNow.AddDays(-UnityEngine.Random.Range(0, 30)).ToString("o")
                };
                mockLeaderboard.Add(entry);
            }

            // Sort by score
            mockLeaderboard = mockLeaderboard.OrderByDescending(e => e.score).ToList();

            // Assign ranks
            for (int i = 0; i < mockLeaderboard.Count; i++)
            {
                mockLeaderboard[i].rank = i + 1;
            }

            currentPlayerId = SystemInfo.deviceUniqueIdentifier;
            currentPlayerName = "Player_" + currentPlayerId.Substring(0, 6);
        }

        #region IBackendService Implementation

        public async Task InitializeAsync()
        {
            if (config?.VerboseLogging == true)
            {
                Debug.Log($"[{ServiceName}] Initializing...");
            }

            // Simulate connection
            await Task.Delay(100);

            IsConnected = true;
            IsOfflineMode = config?.UseMockData ?? true;

            Debug.Log($"[{ServiceName}] Initialized (Offline Mode: {IsOfflineMode})");
        }

        public async Task ShutdownAsync()
        {
            // Submit any pending scores before shutdown
            await SubmitPendingScoresAsync();

            IsConnected = false;
            Debug.Log($"[{ServiceName}] Shutdown complete");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Fetches leaderboard entries for the specified category.
        /// </summary>
        public async Task<ServiceResult<LeaderboardPage>> GetLeaderboardAsync(
            LeaderboardCategory category,
            ThemeType? gameMode = null,
            int page = 0,
            int pageSize = 50)
        {
            string cacheKey = $"{category}_{gameMode}_{page}";

            // Check cache
            if (IsCacheValid(cacheKey))
            {
                return ServiceResult<LeaderboardPage>.Succeeded(cachedPages[cacheKey]);
            }

            try
            {
                LeaderboardPage result;

                if (IsOfflineMode || config?.UseMockData == true)
                {
                    result = GetMockLeaderboard(category, gameMode, page, pageSize);
                }
                else
                {
                    result = await FetchLeaderboardFromServerAsync(category, gameMode, page, pageSize);
                }

                // Cache result
                cachedPages[cacheKey] = result;
                cacheTimestamps[cacheKey] = Time.time;

                OnLeaderboardLoaded?.Invoke(result);
                return ServiceResult<LeaderboardPage>.Succeeded(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{ServiceName}] Failed to get leaderboard: {e.Message}");
                OnLeaderboardError?.Invoke(e.Message);
                return ServiceResult<LeaderboardPage>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Submits a new score to the leaderboard.
        /// </summary>
        public async Task<ServiceResult<LeaderboardEntry>> SubmitScoreAsync(
            int score,
            ThemeType gameMode,
            int coinsCollected,
            float distanceTraveled)
        {
            var entry = new LeaderboardEntry
            {
                entryId = Guid.NewGuid().ToString(),
                playerId = currentPlayerId,
                playerName = currentPlayerName,
                score = score,
                gameMode = gameMode.ToString().ToLower(),
                coinsCollected = coinsCollected,
                distanceTraveled = distanceTraveled,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            try
            {
                if (IsOfflineMode || config?.UseMockData == true)
                {
                    // Add to mock leaderboard
                    AddToMockLeaderboard(entry);
                }
                else
                {
                    await SubmitScoreToServerAsync(entry);
                }

                OnScoreSubmitted?.Invoke(entry);
                OnPlayerRankUpdated?.Invoke(entry.rank);

                // Invalidate cache
                InvalidateCache();

                Debug.Log($"[{ServiceName}] Score submitted: {score} (Rank: {entry.rank})");
                return ServiceResult<LeaderboardEntry>.Succeeded(entry);
            }
            catch (Exception e)
            {
                // Queue for later submission
                pendingSubmissions.Add(entry);
                Debug.LogWarning($"[{ServiceName}] Score queued for later submission: {e.Message}");
                return ServiceResult<LeaderboardEntry>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Gets the current player's rank.
        /// </summary>
        public async Task<ServiceResult<int>> GetPlayerRankAsync(ThemeType? gameMode = null)
        {
            try
            {
                var entries = gameMode.HasValue
                    ? mockLeaderboard.Where(e => e.gameMode == gameMode.Value.ToString().ToLower())
                    : mockLeaderboard;

                var playerEntry = entries.FirstOrDefault(e => e.playerId == currentPlayerId);

                if (playerEntry != null)
                {
                    await Task.Delay(50); // Simulate network
                    return ServiceResult<int>.Succeeded(playerEntry.rank);
                }

                return ServiceResult<int>.Succeeded(-1); // Not ranked
            }
            catch (Exception e)
            {
                return ServiceResult<int>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Gets entries around the player's rank.
        /// </summary>
        public async Task<ServiceResult<LeaderboardPage>> GetEntriesAroundPlayerAsync(
            ThemeType? gameMode = null,
            int entriesAbove = 5,
            int entriesBelow = 5)
        {
            try
            {
                var entries = gameMode.HasValue
                    ? mockLeaderboard.Where(e => e.gameMode == gameMode.Value.ToString().ToLower()).ToList()
                    : mockLeaderboard;

                var playerEntry = entries.FirstOrDefault(e => e.playerId == currentPlayerId);

                if (playerEntry == null)
                {
                    return ServiceResult<LeaderboardPage>.Succeeded(new LeaderboardPage
                    {
                        Entries = entries.Take(10).ToList(),
                        TotalCount = entries.Count
                    });
                }

                int playerIndex = entries.IndexOf(playerEntry);
                int startIndex = Mathf.Max(0, playerIndex - entriesAbove);
                int endIndex = Mathf.Min(entries.Count, playerIndex + entriesBelow + 1);

                await Task.Delay(50);

                var page = new LeaderboardPage
                {
                    Entries = entries.GetRange(startIndex, endIndex - startIndex),
                    TotalCount = entries.Count,
                    PageNumber = 0,
                    PageSize = endIndex - startIndex
                };

                return ServiceResult<LeaderboardPage>.Succeeded(page);
            }
            catch (Exception e)
            {
                return ServiceResult<LeaderboardPage>.Failed(e.Message, ServiceErrorCode.NetworkError);
            }
        }

        /// <summary>
        /// Sets the current player's display name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            currentPlayerName = name;

            // Update existing entries
            foreach (var entry in mockLeaderboard.Where(e => e.playerId == currentPlayerId))
            {
                entry.playerName = name;
            }
        }

        #endregion

        #region Private Methods

        private bool IsCacheValid(string key)
        {
            if (!cachedPages.ContainsKey(key) || !cacheTimestamps.ContainsKey(key))
                return false;

            float cacheDuration = config?.LeaderboardCacheDuration ?? 300f;
            return Time.time - cacheTimestamps[key] < cacheDuration;
        }

        private void InvalidateCache()
        {
            cachedPages.Clear();
            cacheTimestamps.Clear();
        }

        private LeaderboardPage GetMockLeaderboard(
            LeaderboardCategory category,
            ThemeType? gameMode,
            int page,
            int pageSize)
        {
            IEnumerable<LeaderboardEntry> entries = mockLeaderboard;

            // Filter by game mode
            if (gameMode.HasValue)
            {
                entries = entries.Where(e => e.gameMode == gameMode.Value.ToString().ToLower());
            }

            // Filter by time for daily/weekly
            if (category == LeaderboardCategory.GlobalDaily)
            {
                var today = DateTime.UtcNow.Date;
                entries = entries.Where(e => DateTime.Parse(e.timestamp).Date == today);
            }
            else if (category == LeaderboardCategory.GlobalWeekly)
            {
                var weekAgo = DateTime.UtcNow.AddDays(-7);
                entries = entries.Where(e => DateTime.Parse(e.timestamp) >= weekAgo);
            }

            var sortedEntries = entries.OrderByDescending(e => e.score).ToList();

            // Re-rank filtered list
            for (int i = 0; i < sortedEntries.Count; i++)
            {
                sortedEntries[i].rank = i + 1;
            }

            int skip = page * pageSize;
            var pageEntries = sortedEntries.Skip(skip).Take(pageSize).ToList();

            return new LeaderboardPage
            {
                Entries = pageEntries,
                TotalCount = sortedEntries.Count,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        private void AddToMockLeaderboard(LeaderboardEntry entry)
        {
            // Remove old entry from same player and mode if exists
            mockLeaderboard.RemoveAll(e =>
                e.playerId == entry.playerId && e.gameMode == entry.gameMode);

            mockLeaderboard.Add(entry);

            // Re-sort and re-rank
            mockLeaderboard = mockLeaderboard.OrderByDescending(e => e.score).ToList();
            for (int i = 0; i < mockLeaderboard.Count; i++)
            {
                mockLeaderboard[i].rank = i + 1;
            }

            // Update entry rank
            entry.rank = mockLeaderboard.FindIndex(e => e.entryId == entry.entryId) + 1;
        }

        private async Task<LeaderboardPage> FetchLeaderboardFromServerAsync(
            LeaderboardCategory category,
            ThemeType? gameMode,
            int page,
            int pageSize)
        {
            // TODO: Implement actual HTTP request to Azure backend
            await Task.Delay(100);
            return GetMockLeaderboard(category, gameMode, page, pageSize);
        }

        private async Task SubmitScoreToServerAsync(LeaderboardEntry entry)
        {
            // TODO: Implement actual HTTP request to Azure backend
            await Task.Delay(100);
        }

        private async Task SubmitPendingScoresAsync()
        {
            if (pendingSubmissions.Count == 0) return;

            Debug.Log($"[{ServiceName}] Submitting {pendingSubmissions.Count} pending scores...");

            foreach (var entry in pendingSubmissions.ToList())
            {
                try
                {
                    await SubmitScoreToServerAsync(entry);
                    pendingSubmissions.Remove(entry);
                }
                catch
                {
                    // Keep in queue for next attempt
                }
            }
        }

        #endregion
    }
}
