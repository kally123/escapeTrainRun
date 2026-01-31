namespace EscapeTrainRun.Utils
{
    /// <summary>
    /// Centralized game constants for easy tuning and maintenance.
    /// All magic numbers should be defined here.
    /// </summary>
    public static class Constants
    {
        #region Player Movement

        /// <summary>Width of each lane in world units.</summary>
        public const float LaneWidth = 2.5f;

        /// <summary>Speed of lane change transition.</summary>
        public const float LaneChangeSpeed = 10f;

        /// <summary>Maximum height of player jump.</summary>
        public const float JumpHeight = 2.5f;

        /// <summary>Duration of jump animation in seconds.</summary>
        public const float JumpDuration = 0.5f;

        /// <summary>Duration of slide animation in seconds.</summary>
        public const float SlideDuration = 0.8f;

        /// <summary>Number of lanes (0, 1, 2 = Left, Center, Right).</summary>
        public const int LaneCount = 3;

        /// <summary>Center lane index.</summary>
        public const int CenterLane = 1;

        #endregion

        #region Speed Settings

        /// <summary>Starting run speed in units per second.</summary>
        public const float BaseRunSpeed = 15f;

        /// <summary>Maximum run speed cap.</summary>
        public const float MaxRunSpeed = 35f;

        /// <summary>Speed increase rate per second.</summary>
        public const float SpeedIncreaseRate = 0.1f;

        /// <summary>Speed boost multiplier when power-up active.</summary>
        public const float SpeedBoostMultiplier = 2f;

        #endregion

        #region Level Generation

        /// <summary>Length of each track segment.</summary>
        public const float SegmentLength = 30f;

        /// <summary>Number of segments spawned ahead of player.</summary>
        public const int InitialSegments = 5;

        /// <summary>Maximum number of active segments at once.</summary>
        public const int MaxActiveSegments = 10;

        /// <summary>Distance behind player before segment despawns.</summary>
        public const float DespawnDistance = 50f;

        /// <summary>Minimum distance between obstacle spawns.</summary>
        public const float MinObstacleSpacing = 10f;

        /// <summary>Maximum distance between obstacle spawns.</summary>
        public const float MaxObstacleSpacing = 25f;

        #endregion

        #region Power-Ups

        /// <summary>Magnet power-up duration in seconds.</summary>
        public const float MagnetDuration = 10f;

        /// <summary>Magnet attraction range in units.</summary>
        public const float MagnetRange = 5f;

        /// <summary>Speed boost duration in seconds.</summary>
        public const float SpeedBoostDuration = 5f;

        /// <summary>Star power (fly) duration in seconds.</summary>
        public const float StarPowerDuration = 8f;

        /// <summary>Score multiplier duration in seconds.</summary>
        public const float MultiplierDuration = 15f;

        /// <summary>Score multiplier value.</summary>
        public const int MultiplierValue = 2;

        /// <summary>Shield duration in seconds.</summary>
        public const float ShieldDuration = 10f;

        /// <summary>Base spawn chance for power-ups (0-1).</summary>
        public const float PowerUpSpawnChance = 0.05f;

        /// <summary>Minimum distance between power-up spawns.</summary>
        public const float MinPowerUpSpacing = 200f;

        #endregion

        #region Scoring

        /// <summary>Points awarded per meter traveled.</summary>
        public const int PointsPerMeter = 1;

        /// <summary>Points awarded per coin collected.</summary>
        public const int PointsPerCoin = 10;

        /// <summary>Base coins awarded per game.</summary>
        public const int BaseCoinsPerRun = 5;

        #endregion

        #region Input

        /// <summary>Minimum swipe distance to register (in pixels).</summary>
        public const float MinSwipeDistance = 50f;

        /// <summary>Maximum time for a swipe gesture (in seconds).</summary>
        public const float MaxSwipeTime = 0.5f;

        #endregion

        #region Coins & Economy

        /// <summary>Coin value for regular coin pickup.</summary>
        public const int RegularCoinValue = 1;

        /// <summary>Coin value for special/rare coin.</summary>
        public const int SpecialCoinValue = 5;

        #endregion

        #region Character Costs

        public const int CostLuna = 500;
        public const int CostMax = 1000;
        public const int CostRoboKid = 5000;
        public const int CostSuperSara = 10000;
        public const int CostPrincessPenny = 15000;
        public const int CostDinoDan = 20000;
        public const int CostNinjaNick = 25000;

        #endregion

        #region API Endpoints

        /// <summary>Leaderboard API endpoint.</summary>
        public const string LeaderboardEndpoint = "/api/v1/leaderboard";

        /// <summary>Save game API endpoint.</summary>
        public const string SaveGameEndpoint = "/api/v1/savegame";

        /// <summary>Achievements API endpoint.</summary>
        public const string AchievementsEndpoint = "/api/v1/achievements";

        #endregion

        #region PlayerPrefs Keys

        /// <summary>Key for storing overall high score.</summary>
        public const string HighScoreKey = "HighScore";

        /// <summary>Key for storing total coins.</summary>
        public const string TotalCoinsKey = "TotalCoins";

        /// <summary>Key for storing unlocked characters.</summary>
        public const string UnlockedCharactersKey = "UnlockedCharacters";

        /// <summary>Key for storing master volume setting.</summary>
        public const string SettingsVolumeKey = "Volume";

        /// <summary>Key for storing music volume.</summary>
        public const string SettingsMusicKey = "MusicVolume";

        /// <summary>Key for storing SFX volume.</summary>
        public const string SettingsSfxKey = "SFXVolume";

        /// <summary>Key for storing selected character.</summary>
        public const string SelectedCharacterKey = "SelectedCharacter";

        /// <summary>Key for storing first launch flag.</summary>
        public const string FirstLaunchKey = "FirstLaunch";

        #endregion

        #region Tags

        public const string PlayerTag = "Player";
        public const string ObstacleTag = "Obstacle";
        public const string CoinTag = "Coin";
        public const string PowerUpTag = "PowerUp";
        public const string GroundTag = "Ground";

        #endregion

        #region Layers

        public const string PlayerLayer = "Player";
        public const string ObstacleLayer = "Obstacle";
        public const string CollectibleLayer = "Collectible";
        public const string GroundLayer = "Ground";

        #endregion

        #region Scene Names

        public const string MainMenuScene = "MainMenu";
        public const string GameplayScene = "Gameplay";
        public const string ShopScene = "Shop";
        public const string LoadingScene = "Loading";

        #endregion

        #region Animation Parameters

        public const string AnimRunning = "IsRunning";
        public const string AnimJumping = "IsJumping";
        public const string AnimSliding = "IsSliding";
        public const string AnimCrashed = "Crashed";
        public const string AnimSpeed = "Speed";
        public const string AnimLaneChange = "LaneChange";

        #endregion
    }
}
