using UnityEngine;
using UnityEngine.SceneManagement;
using EscapeTrainRun.Events;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Central game manager - handles game state and lifecycle.
    /// Follows single responsibility principle for game flow control.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private ThemeType defaultTheme = ThemeType.Train;
        [SerializeField] private bool startPaused = false;

        // Game State
        private GameState currentState = GameState.Menu;
        private ThemeType currentTheme;
        private bool isPaused;
        private float gameTime;

        // Properties
        public GameState CurrentState => currentState;
        public ThemeType CurrentTheme => currentTheme;
        public bool IsPaused => isPaused;
        public float GameTime => gameTime;

        private void Awake()
        {
            InitializeSingleton();
            RegisterServices();
            currentTheme = defaultTheme;
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

        private void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnPlayerCrashed += HandlePlayerCrashed;
            GameEvents.OnGamePaused += PauseGame;
            GameEvents.OnGameResumed += ResumeGame;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnPlayerCrashed -= HandlePlayerCrashed;
            GameEvents.OnGamePaused -= PauseGame;
            GameEvents.OnGameResumed -= ResumeGame;
        }

        private void Update()
        {
            if (currentState == GameState.Playing && !isPaused)
            {
                gameTime += Time.deltaTime;
            }

            HandlePauseInput();
        }

        private void HandlePauseInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && currentState == GameState.Playing)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        /// <summary>
        /// Starts a new game with the specified theme.
        /// </summary>
        public void StartGame(ThemeType theme)
        {
            currentTheme = theme;
            currentState = GameState.Playing;
            gameTime = 0f;
            isPaused = false;
            Time.timeScale = 1f;

            GameEvents.TriggerGameStarted();
            GameEvents.TriggerThemeChanged(theme);

            Debug.Log($"[GameManager] Game started with theme: {theme}");
        }

        /// <summary>
        /// Starts a new game with the current/default theme.
        /// </summary>
        public void StartGame()
        {
            StartGame(currentTheme);
        }

        /// <summary>
        /// Pauses the current game.
        /// </summary>
        public void PauseGame()
        {
            if (currentState != GameState.Playing || isPaused) return;

            isPaused = true;
            Time.timeScale = 0f;
            currentState = GameState.Paused;

            Debug.Log("[GameManager] Game paused");
        }

        /// <summary>
        /// Resumes the paused game.
        /// </summary>
        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;

            isPaused = false;
            Time.timeScale = 1f;
            currentState = GameState.Playing;

            Debug.Log("[GameManager] Game resumed");
        }

        /// <summary>
        /// Handles player crash - triggers game over sequence.
        /// </summary>
        private void HandlePlayerCrashed()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.GameOver;
            Time.timeScale = 0f;

            // Get score data from ScoreManager
            var scoreManager = ServiceLocator.Get<ScoreManager>();
            var gameOverData = scoreManager.CreateGameOverData(currentTheme);

            GameEvents.TriggerGameOver(gameOverData);

            Debug.Log($"[GameManager] Game over! Score: {gameOverData.FinalScore}");
        }

        /// <summary>
        /// Restarts the current game.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            StartGame(currentTheme);
        }

        /// <summary>
        /// Returns to main menu.
        /// </summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            currentState = GameState.Menu;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Returns to main menu (alias for GoToMainMenu).
        /// </summary>
        public void ReturnToMenu()
        {
            GoToMainMenu();
        }

        /// <summary>
        /// Sets the game theme for the next game.
        /// </summary>
        public void SetTheme(ThemeType theme)
        {
            currentTheme = theme;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// Represents the current state of the game.
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Loading
    }
}
