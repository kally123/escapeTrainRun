using UnityEngine;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Bootstrap script that initializes all core systems.
    /// Attach this to a GameObject in the first loaded scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Core Systems Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject saveManagerPrefab;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;

        private static bool isInitialized = false;

        private void Awake()
        {
            if (isInitialized)
            {
                // Core systems already exist, destroy this bootstrap
                Destroy(gameObject);
                return;
            }

            InitializeCoreSystems();
            isInitialized = true;

            if (showDebugLogs)
            {
                Debug.Log("[GameBootstrap] All core systems initialized successfully");
            }
        }

        private void InitializeCoreSystems()
        {
            // Create core systems in order of dependency

            // 1. Save Manager - needed for loading saved data
            if (SaveManager.Instance == null)
            {
                if (saveManagerPrefab != null)
                {
                    Instantiate(saveManagerPrefab);
                }
                else
                {
                    CreateDefaultSaveManager();
                }
            }

            // 2. Audio Manager - for sound and music
            if (AudioManager.Instance == null)
            {
                if (audioManagerPrefab != null)
                {
                    Instantiate(audioManagerPrefab);
                }
                else
                {
                    CreateDefaultAudioManager();
                }
            }

            // 3. Game Manager - main game controller
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                }
                else
                {
                    CreateDefaultGameManager();
                }
            }
        }

        private void CreateDefaultSaveManager()
        {
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
            DontDestroyOnLoad(saveManagerObj);
        }

        private void CreateDefaultAudioManager()
        {
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManagerObj.AddComponent<AudioManager>();
            
            // Add required audio sources
            AudioSource musicSource = audioManagerObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            
            AudioSource sfxSource = audioManagerObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            
            AudioSource ambientSource = audioManagerObj.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            
            DontDestroyOnLoad(audioManagerObj);
        }

        private void CreateDefaultGameManager()
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObj);
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.OnApplicationQuit();
            Events.GameEvents.ClearAllEvents();
            isInitialized = false;
        }
    }
}
