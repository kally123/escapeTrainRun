using System;
using UnityEngine;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Collectibles;

namespace EscapeTrainRun.Events
{
    /// <summary>
    /// Central event system for game-wide communication.
    /// Following event-driven architecture guidelines for loose coupling.
    /// Publishers don't know about subscribers - complete decoupling.
    /// </summary>
    public static class GameEvents
    {
        #region Player Events

        /// <summary>Fired when the score changes.</summary>
        public static event Action<int> OnScoreChanged;

        /// <summary>Fired when coins are collected.</summary>
        public static event Action<int> OnCoinsCollected;

        /// <summary>Fired when the player position updates.</summary>
        public static event Action<Vector3> OnPlayerMoved;

        /// <summary>Fired when a power-up is activated.</summary>
        public static event Action<PowerUpType> OnPowerUpActivated;

        /// <summary>Fired when a power-up expires or is consumed.</summary>
        public static event Action<PowerUpType> OnPowerUpDeactivated;

        /// <summary>Fired when the player jumps.</summary>
        public static event Action OnPlayerJumped;

        /// <summary>Fired when the player slides.</summary>
        public static event Action OnPlayerSlide;

        /// <summary>Fired when the player crashes into an obstacle.</summary>
        public static event Action OnPlayerCrashed;

        /// <summary>Fired when the player changes lanes.</summary>
        public static event Action<int> OnLaneChanged;

        #endregion

        #region Game State Events

        /// <summary>Fired when a new game starts.</summary>
        public static event Action OnGameStarted;

        /// <summary>Fired when the game is paused.</summary>
        public static event Action OnGamePaused;

        /// <summary>Fired when the game is resumed from pause.</summary>
        public static event Action OnGameResumed;

        /// <summary>Fired when the game ends with final data.</summary>
        public static event Action<GameOverData> OnGameOver;

        #endregion

        #region Environment Events

        /// <summary>Fired when the game theme changes.</summary>
        public static event Action<ThemeType> OnThemeChanged;

        /// <summary>Fired when a theme is selected (before game starts).</summary>
        public static event Action<ThemeType> OnThemeSelected;

        /// <summary>Fired when a new track segment is spawned.</summary>
        public static event Action<TrackSegment> OnSegmentSpawned;

        /// <summary>Fired when a track segment is despawned/recycled.</summary>
        public static event Action<TrackSegment> OnSegmentDespawned;

        /// <summary>Fired when segment count changes (no specific segment).</summary>
        public static event Action OnSegmentCountChanged;

        #endregion

        #region UI Events

        /// <summary>Fired when a UI panel should be shown.</summary>
        public static event Action<string> OnShowPanel;

        /// <summary>Fired when a UI panel should be hidden.</summary>
        public static event Action<string> OnHidePanel;

        #endregion

        #region Player Event Triggers

        public static void TriggerScoreChanged(int newScore)
        {
            OnScoreChanged?.Invoke(newScore);
        }

        public static void TriggerCoinsCollected(int amount)
        {
            OnCoinsCollected?.Invoke(amount);
        }

        public static void TriggerPlayerMoved(Vector3 position)
        {
            OnPlayerMoved?.Invoke(position);
        }

        public static void TriggerPowerUpActivated(PowerUpType type)
        {
            OnPowerUpActivated?.Invoke(type);
            Debug.Log($"[GameEvents] Power-up activated: {type}");
        }

        public static void TriggerPowerUpDeactivated(PowerUpType type)
        {
            OnPowerUpDeactivated?.Invoke(type);
            Debug.Log($"[GameEvents] Power-up deactivated: {type}");
        }

        public static void TriggerPlayerJumped()
        {
            OnPlayerJumped?.Invoke();
        }

        public static void TriggerPlayerSlide()
        {
            OnPlayerSlide?.Invoke();
        }

        public static void TriggerPlayerCrashed()
        {
            OnPlayerCrashed?.Invoke();
            Debug.Log("[GameEvents] Player crashed!");
        }

        public static void TriggerLaneChanged(int newLane)
        {
            OnLaneChanged?.Invoke(newLane);
        }

        #endregion

        #region Game State Event Triggers

        public static void TriggerGameStarted()
        {
            OnGameStarted?.Invoke();
            Debug.Log("[GameEvents] Game started!");
        }

        public static void TriggerGamePaused()
        {
            OnGamePaused?.Invoke();
            Debug.Log("[GameEvents] Game paused");
        }

        public static void TriggerGameResumed()
        {
            OnGameResumed?.Invoke();
            Debug.Log("[GameEvents] Game resumed");
        }

        public static void TriggerGameOver(GameOverData data)
        {
            OnGameOver?.Invoke(data);
            Debug.Log($"[GameEvents] Game over - Score: {data.FinalScore}, High Score: {data.IsHighScore}");
        }

        #endregion

        #region Environment Event Triggers

        public static void TriggerThemeChanged(ThemeType theme)
        {
            OnThemeChanged?.Invoke(theme);
            Debug.Log($"[GameEvents] Theme changed to: {theme}");
        }

        public static void TriggerThemeSelected(ThemeType theme)
        {
            OnThemeSelected?.Invoke(theme);
            Debug.Log($"[GameEvents] Theme selected: {theme}");
        }

        public static void TriggerSegmentSpawned(TrackSegment segment)
        {
            OnSegmentSpawned?.Invoke(segment);
        }

        public static void TriggerSegmentDespawned(TrackSegment segment)
        {
            OnSegmentDespawned?.Invoke(segment);
        }

        public static void TriggerSegmentDespawned()
        {
            OnSegmentCountChanged?.Invoke();
        }

        #endregion

        #region UI Event Triggers

        public static void TriggerShowPanel(string panelName)
        {
            OnShowPanel?.Invoke(panelName);
        }

        public static void TriggerHidePanel(string panelName)
        {
            OnHidePanel?.Invoke(panelName);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clears all event subscriptions. 
        /// Call this when transitioning between major game states to prevent memory leaks.
        /// </summary>
        public static void ClearAllEvents()
        {
            OnScoreChanged = null;
            OnCoinsCollected = null;
            OnPlayerMoved = null;
            OnPowerUpActivated = null;
            OnPowerUpDeactivated = null;
            OnPlayerJumped = null;
            OnPlayerSlide = null;
            OnPlayerCrashed = null;
            OnLaneChanged = null;
            OnGameStarted = null;
            OnGamePaused = null;
            OnGameResumed = null;
            OnGameOver = null;
            OnThemeChanged = null;
            OnThemeSelected = null;
            OnSegmentSpawned = null;
            OnSegmentDespawned = null;
            OnSegmentCountChanged = null;
            OnShowPanel = null;
            OnHidePanel = null;

            Debug.Log("[GameEvents] All events cleared");
        }

        #endregion
    }

    /// <summary>
    /// Immutable game over data structure.
    /// Following WebFlux guidelines for immutable POJOs.
    /// </summary>
    public readonly struct GameOverData
    {
        public readonly int FinalScore;
        public readonly int CoinsCollected;
        public readonly float DistanceTraveled;
        public readonly bool IsHighScore;
        public readonly ThemeType GameMode;
        public readonly float GameDuration;

        public GameOverData(int score, int coins, float distance, bool highScore, ThemeType mode, float duration = 0f)
        {
            FinalScore = score;
            CoinsCollected = coins;
            DistanceTraveled = distance;
            IsHighScore = highScore;
            GameMode = mode;
            GameDuration = duration;
        }

        public override string ToString()
        {
            return $"Score: {FinalScore}, Coins: {CoinsCollected}, Distance: {DistanceTraveled:F1}m, Mode: {GameMode}";
        }
    }
}
