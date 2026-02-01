using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the GamePlay scene with all required GameObjects.
/// Run from menu: Tools > Escape Train Run > Setup GamePlay Scene
/// </summary>
public class GamePlaySceneSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup GamePlay Scene")]
    public static void SetupGamePlayScene()
    {
        if (!EditorUtility.DisplayDialog("Setup GamePlay Scene",
            "This will create all GamePlay scene objects. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateManagers();
        CreatePlayer();
        CreateCameraRig();
        CreateInputHandler();
        CreateTrack();
        CreateUICanvas();
        CreatePauseMenu();
        CreateGameOverPanel();

        Debug.Log("✅ GamePlay Scene Setup Complete!");
        EditorUtility.DisplayDialog("Success", "GamePlay scene setup complete!\n\nRemember to:\n1. Assign prefabs to LevelGenerator\n2. Configure PoolManager pools\n3. Add AudioClips to AudioManager", "OK");
    }

    #region Managers

    private static void CreateManagers()
    {
        // GameManager
        var gameManager = CreateGameObject("GameManager");
        AddComponentSafe<EscapeTrainRun.GameManager>(gameManager);

        // SaveManager
        var saveManager = CreateGameObject("SaveManager");
        AddComponentSafe<EscapeTrainRun.SaveManager>(saveManager);

        // PoolManager
        var poolManager = CreateGameObject("PoolManager");
        AddComponentSafe<EscapeTrainRun.PoolManager>(poolManager);

        // AudioManager
        var audioManager = CreateGameObject("AudioManager");
        AddComponentSafe<EscapeTrainRun.AudioManager>(audioManager);

        // Create AudioSource children
        var musicSource = CreateChildGameObject(audioManager, "MusicSource");
        var musicAudio = musicSource.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.playOnAwake = false;

        var sfxSource = CreateChildGameObject(audioManager, "SFXSource");
        var sfxAudio = sfxSource.AddComponent<AudioSource>();
        sfxAudio.playOnAwake = false;

        var ambientSource = CreateChildGameObject(audioManager, "AmbientSource");
        var ambientAudio = ambientSource.AddComponent<AudioSource>();
        ambientAudio.loop = true;
        ambientAudio.playOnAwake = false;

        // LevelGenerator
        var levelGenerator = CreateGameObject("LevelGenerator");
        AddComponentSafe<EscapeTrainRun.LevelGenerator>(levelGenerator);

        // ScoreManager
        var scoreManager = CreateGameObject("ScoreManager");
        AddComponentSafe<EscapeTrainRun.ScoreManager>(scoreManager);

        // DifficultyManager
        var difficultyManager = CreateGameObject("DifficultyManager");
        AddComponentSafe<EscapeTrainRun.DifficultyManager>(difficultyManager);

        // PowerUpManager
        var powerUpManager = CreateGameObject("PowerUpManager");
        AddComponentSafe<EscapeTrainRun.PowerUpManager>(powerUpManager);

        // ParentManager
        var parentManager = CreateGameObject("ParentManager");
        AddComponentSafe<EscapeTrainRun.ParentControlManager>(parentManager);

        Debug.Log("✅ Managers created");
    }

    #endregion

    #region Player

    private static void CreatePlayer()
    {
        var player = CreateGameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector3.zero;

        // Add components
        var charController = player.AddComponent<CharacterController>();
        charController.height = 2f;
        charController.radius = 0.5f;
        charController.center = new Vector3(0, 1, 0);

        var rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Add capsule collider for trigger detection
        var capsule = player.AddComponent<CapsuleCollider>();
        capsule.height = 2f;
        capsule.radius = 0.5f;
        capsule.center = new Vector3(0, 1, 0);
        capsule.isTrigger = true;

        // Add player scripts
        AddComponentSafe<EscapeTrainRun.PlayerController>(player);
        AddComponentSafe<EscapeTrainRun.PlayerMovement>(player);
        AddComponentSafe<EscapeTrainRun.PlayerCollision>(player);
        AddComponentSafe<EscapeTrainRun.PlayerAnimation>(player);

        // Create child objects
        var model = CreateChildGameObject(player, "Model");
        model.transform.localPosition = Vector3.zero;

        var groundCheck = CreateChildGameObject(player, "GroundCheck");
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);

        var shieldEffect = CreateChildGameObject(player, "ShieldEffect");
        shieldEffect.transform.localPosition = Vector3.zero;
        // Particle system can be added later

        var magnetRange = CreateChildGameObject(player, "MagnetRange");
        magnetRange.transform.localPosition = Vector3.zero;
        var magnetCollider = magnetRange.AddComponent<SphereCollider>();
        magnetCollider.radius = 5f;
        magnetCollider.isTrigger = true;

        Debug.Log("✅ Player created");
    }

    #endregion

    #region Camera

    private static void CreateCameraRig()
    {
        var cameraRig = CreateGameObject("CameraRig");
        cameraRig.transform.position = Vector3.zero;
        AddComponentSafe<EscapeTrainRun.CameraController>(cameraRig);

        // Find or create main camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        mainCam.transform.SetParent(cameraRig.transform);
        mainCam.transform.localPosition = new Vector3(0, 8, -12);
        mainCam.transform.localRotation = Quaternion.Euler(30, 0, 0);
        mainCam.fieldOfView = 60f;
        mainCam.nearClipPlane = 0.1f;
        mainCam.farClipPlane = 500f;

        Debug.Log("✅ Camera rig created");
    }

    #endregion

    #region Input

    private static void CreateInputHandler()
    {
        var inputHandler = CreateGameObject("InputHandler");
        AddComponentSafe<EscapeTrainRun.SwipeDetector>(inputHandler);

        Debug.Log("✅ Input handler created");
    }

    #endregion

    #region Track

    private static void CreateTrack()
    {
        var track = CreateGameObject("Track");
        track.transform.position = Vector3.zero;

        // Create lane markers (visual reference)
        var lanes = CreateChildGameObject(track, "LaneMarkers");
        
        // Left lane marker
        var leftLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftLane.name = "LeftLaneMarker";
        leftLane.transform.SetParent(lanes.transform);
        leftLane.transform.localPosition = new Vector3(-2.5f, 0.01f, 0);
        leftLane.transform.localScale = new Vector3(0.1f, 0.01f, 100f);

        // Center lane marker
        var centerLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centerLane.name = "CenterLaneMarker";
        centerLane.transform.SetParent(lanes.transform);
        centerLane.transform.localPosition = new Vector3(0, 0.01f, 0);
        centerLane.transform.localScale = new Vector3(0.1f, 0.01f, 100f);

        // Right lane marker
        var rightLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightLane.name = "RightLaneMarker";
        rightLane.transform.SetParent(lanes.transform);
        rightLane.transform.localPosition = new Vector3(2.5f, 0.01f, 0);
        rightLane.transform.localScale = new Vector3(0.1f, 0.01f, 100f);

        // Ground plane
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(track.transform);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(1, 1, 10);

        // Spawn points container
        var spawnPoints = CreateChildGameObject(track, "SpawnPoints");
        
        // Obstacle spawn point
        var obstacleSpawn = CreateChildGameObject(spawnPoints, "ObstacleSpawnPoint");
        obstacleSpawn.transform.localPosition = new Vector3(0, 0, 100);

        // Collectible spawn point
        var collectibleSpawn = CreateChildGameObject(spawnPoints, "CollectibleSpawnPoint");
        collectibleSpawn.transform.localPosition = new Vector3(0, 1, 100);

        Debug.Log("✅ Track created");
    }

    #endregion

    #region UI

    private static void CreateUICanvas()
    {
        // Main Canvas
        var canvasObj = CreateGameObject("GameplayCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        AddComponentSafe<EscapeTrainRun.GameplayUI>(canvasObj);

        // TopBar
        var topBar = CreateUIPanel(canvasObj, "TopBar");
        SetRectTransform(topBar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(1000, 100));

        // Score Text
        var scoreText = CreateTextMeshPro(topBar, "ScoreText", "0", 48);
        SetRectTransform(scoreText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 60));

        // Coin Counter
        var coinCounter = CreateUIPanel(topBar, "CoinCounter");
        SetRectTransform(coinCounter, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 0), new Vector2(150, 50));

        var coinIcon = CreateUIImage(coinCounter, "CoinIcon");
        SetRectTransform(coinIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(25, 0), new Vector2(40, 40));

        var coinText = CreateTextMeshPro(coinCounter, "CoinText", "0", 32);
        SetRectTransform(coinText, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(80, 40));

        // Pause Button
        var pauseBtn = CreateUIButton(topBar, "PauseButton", "||");
        SetRectTransform(pauseBtn, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(60, 60));

        // PowerUp Indicator
        var powerUpIndicator = CreateUIPanel(canvasObj, "PowerUpIndicator");
        SetRectTransform(powerUpIndicator, new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -180), new Vector2(120, 80));
        powerUpIndicator.SetActive(false); // Hidden by default

        var powerUpIcon = CreateUIImage(powerUpIndicator, "PowerUpIcon");
        SetRectTransform(powerUpIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(50, 50));

        var timerBar = CreateUIImage(powerUpIndicator, "TimerBar");
        SetRectTransform(timerBar, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(100, 10));
        timerBar.GetComponent<Image>().color = Color.green;

        // Combo Display
        var comboDisplay = CreateUIPanel(canvasObj, "ComboDisplay");
        SetRectTransform(comboDisplay, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(300, 80));
        comboDisplay.SetActive(false); // Hidden by default

        var comboText = CreateTextMeshPro(comboDisplay, "ComboText", "COMBO x2", 36);
        SetRectTransform(comboText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 80));

        Debug.Log("✅ UI Canvas created");
    }

    private static void CreatePauseMenu()
    {
        var canvas = GameObject.Find("GameplayCanvas");
        if (canvas == null) return;

        var pauseMenu = CreateUIPanel(canvas, "PauseMenu");
        SetRectTransform(pauseMenu, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddComponentSafe<EscapeTrainRun.PauseMenuUI>(pauseMenu);
        pauseMenu.SetActive(false); // Hidden by default

        // Semi-transparent background
        var bg = pauseMenu.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = new Color(0, 0, 0, 0.8f);
        }

        // Panel content
        var panel = CreateUIPanel(pauseMenu, "Panel");
        SetRectTransform(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 800));
        panel.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // Title
        var title = CreateTextMeshPro(panel, "PauseTitle", "PAUSED", 64);
        SetRectTransform(title, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(400, 80));

        // Buttons
        var resumeBtn = CreateUIButton(panel, "ResumeButton", "RESUME");
        SetRectTransform(resumeBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(400, 80));

        var settingsBtn = CreateUIButton(panel, "SettingsButton", "SETTINGS");
        SetRectTransform(settingsBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(400, 80));

        var mainMenuBtn = CreateUIButton(panel, "MainMenuButton", "MAIN MENU");
        SetRectTransform(mainMenuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(400, 80));

        Debug.Log("✅ Pause menu created");
    }

    private static void CreateGameOverPanel()
    {
        var canvas = GameObject.Find("GameplayCanvas");
        if (canvas == null) return;

        var gameOverPanel = CreateUIPanel(canvas, "GameOverPanel");
        SetRectTransform(gameOverPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddComponentSafe<EscapeTrainRun.GameOverUI>(gameOverPanel);
        gameOverPanel.SetActive(false); // Hidden by default

        // Semi-transparent background
        var bg = gameOverPanel.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = new Color(0, 0, 0, 0.85f);
        }

        // Panel content
        var panel = CreateUIPanel(gameOverPanel, "Panel");
        SetRectTransform(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 1000));
        panel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 1f);

        // Title
        var title = CreateTextMeshPro(panel, "GameOverTitle", "GAME OVER", 72);
        SetRectTransform(title, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(600, 100));

        // Stats
        var finalScore = CreateTextMeshPro(panel, "FinalScoreText", "Score: 0", 48);
        SetRectTransform(finalScore, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(500, 60));

        var highScore = CreateTextMeshPro(panel, "HighScoreText", "Best: 0", 36);
        SetRectTransform(highScore, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(500, 50));

        var coins = CreateTextMeshPro(panel, "CoinsCollectedText", "Coins: 0", 36);
        SetRectTransform(coins, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -340), new Vector2(500, 50));

        var distance = CreateTextMeshPro(panel, "DistanceText", "Distance: 0m", 36);
        SetRectTransform(distance, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -400), new Vector2(500, 50));

        // Buttons
        var playAgainBtn = CreateUIButton(panel, "PlayAgainButton", "PLAY AGAIN");
        SetRectTransform(playAgainBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(450, 80));

        var doubleCoinsBtn = CreateUIButton(panel, "DoubleCoinsButton", "DOUBLE COINS (AD)");
        SetRectTransform(doubleCoinsBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(450, 80));
        doubleCoinsBtn.GetComponent<Image>().color = new Color(0.8f, 0.6f, 0.2f, 1f);

        var mainMenuBtn = CreateUIButton(panel, "MainMenuButton", "MAIN MENU");
        SetRectTransform(mainMenuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(450, 80));

        Debug.Log("✅ Game over panel created");
    }

    #endregion

    #region Helper Methods

    private static GameObject CreateGameObject(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            Debug.LogWarning($"GameObject '{name}' already exists, skipping creation.");
            return existing;
        }
        
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        return go;
    }

    private static GameObject CreateChildGameObject(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        return go;
    }

    private static T AddComponentSafe<T>(GameObject go) where T : Component
    {
        var existing = go.GetComponent<T>();
        if (existing != null) return existing;
        return go.AddComponent<T>();
    }

    private static GameObject CreateUIPanel(GameObject parent, string name)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        var image = panel.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0); // Transparent by default
        Undo.RegisterCreatedObjectUndo(panel, $"Create {name}");
        return panel;
    }

    private static GameObject CreateUIImage(GameObject parent, string name)
    {
        var imageObj = new GameObject(name);
        imageObj.transform.SetParent(parent.transform, false);
        imageObj.AddComponent<RectTransform>();
        imageObj.AddComponent<CanvasRenderer>();
        var image = imageObj.AddComponent<Image>();
        image.color = Color.white;
        Undo.RegisterCreatedObjectUndo(imageObj, $"Create {name}");
        return imageObj;
    }

    private static GameObject CreateUIButton(GameObject parent, string name, string text)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        buttonObj.AddComponent<RectTransform>();
        buttonObj.AddComponent<CanvasRenderer>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        buttonObj.AddComponent<Button>();

        // Button text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        Undo.RegisterCreatedObjectUndo(buttonObj, $"Create {name}");
        return buttonObj;
    }

    private static GameObject CreateTextMeshPro(GameObject parent, string name, string text, float fontSize)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        textObj.AddComponent<RectTransform>();
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        Undo.RegisterCreatedObjectUndo(textObj, $"Create {name}");
        return textObj;
    }

    private static void SetRectTransform(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = go.AddComponent<RectTransform>();
        
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    #endregion
}
