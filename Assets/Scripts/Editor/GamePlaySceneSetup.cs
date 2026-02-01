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
        EditorUtility.DisplayDialog("Success", "GamePlay scene setup complete!\n\nRemember to:\n1. Assign prefabs to LevelGenerator\n2. Add AudioClips to AudioManager", "OK");
    }

    #region Managers

    private static void CreateManagers()
    {
        // GameManager
        var gameManager = CreateGameObject("GameManager");
        AddComponentSafe<EscapeTrainRun.Core.GameManager>(gameManager);

        // SaveManager
        var saveManager = CreateGameObject("SaveManager");
        AddComponentSafe<EscapeTrainRun.Core.SaveManager>(saveManager);

        // AudioManager
        var audioManager = CreateGameObject("AudioManager");
        AddComponentSafe<EscapeTrainRun.Core.AudioManager>(audioManager);

        // Create AudioSource children
        var musicSource = CreateChildGameObject(audioManager, "MusicSource");
        var musicAudio = musicSource.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.playOnAwake = false;

        var sfxSource = CreateChildGameObject(audioManager, "SFXSource");
        var sfxAudio = sfxSource.AddComponent<AudioSource>();
        sfxAudio.playOnAwake = false;

        // LevelGenerator
        var levelGenerator = CreateGameObject("LevelGenerator");
        AddComponentSafe<EscapeTrainRun.Environment.LevelGenerator>(levelGenerator);

        // ScoreManager
        var scoreManager = CreateGameObject("ScoreManager");
        AddComponentSafe<EscapeTrainRun.Core.ScoreManager>(scoreManager);

        // VFXManager
        var vfxManager = CreateGameObject("VFXManager");
        AddComponentSafe<EscapeTrainRun.Effects.VFXManager>(vfxManager);

        // ObstacleManager
        var obstacleManager = CreateGameObject("ObstacleManager");
        AddComponentSafe<EscapeTrainRun.Obstacles.ObstacleManager>(obstacleManager);

        // CoinManager
        var coinManager = CreateGameObject("CoinManager");
        AddComponentSafe<EscapeTrainRun.Collectibles.CoinManager>(coinManager);

        // PowerUpManager  
        var powerUpManager = CreateGameObject("PowerUpManager");
        AddComponentSafe<EscapeTrainRun.Collectibles.PowerUpManager>(powerUpManager);

        Debug.Log("✅ Created all manager objects");
    }

    #endregion

    #region Player

    private static void CreatePlayer()
    {
        var player = CreateGameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, 0);

        // Add CharacterController
        var cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2f;
        cc.radius = 0.3f;

        // Add player components
        AddComponentSafe<EscapeTrainRun.Player.PlayerController>(player);
        AddComponentSafe<EscapeTrainRun.Player.PlayerCollision>(player);
        AddComponentSafe<EscapeTrainRun.Player.PlayerAnimation>(player);

        // Create child objects
        var model = CreateChildGameObject(player, "Model");
        
        var groundCheck = CreateChildGameObject(player, "GroundCheck");
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);

        Debug.Log("✅ Created Player object");
    }

    #endregion

    #region Camera

    private static void CreateCameraRig()
    {
        var cameraRig = CreateGameObject("CameraRig");
        cameraRig.transform.position = new Vector3(0, 5, -8);
        
        AddComponentSafe<EscapeTrainRun.Player.PlayerCamera>(cameraRig);

        // Main Camera
        var mainCamera = CreateChildGameObject(cameraRig, "Main Camera");
        mainCamera.tag = "MainCamera";
        
        var camera = mainCamera.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 200f;

        mainCamera.AddComponent<AudioListener>();

        Debug.Log("✅ Created Camera Rig");
    }

    #endregion

    #region Input

    private static void CreateInputHandler()
    {
        var inputHandler = CreateGameObject("InputHandler");
        AddComponentSafe<EscapeTrainRun.Player.SwipeDetector>(inputHandler);

        Debug.Log("✅ Created Input Handler");
    }

    #endregion

    #region Track

    private static void CreateTrack()
    {
        var track = CreateGameObject("Track");
        
        var spawnPoints = CreateChildGameObject(track, "SpawnPoints");
        
        var leftSpawn = CreateChildGameObject(spawnPoints, "Left");
        leftSpawn.transform.localPosition = new Vector3(-1.5f, 0, 0);
        
        var centerSpawn = CreateChildGameObject(spawnPoints, "Center");
        centerSpawn.transform.localPosition = Vector3.zero;
        
        var rightSpawn = CreateChildGameObject(spawnPoints, "Right");
        rightSpawn.transform.localPosition = new Vector3(1.5f, 0, 0);

        Debug.Log("✅ Created Track object");
    }

    #endregion

    #region UI

    private static void CreateUICanvas()
    {
        var canvas = CreateGameObject("GameplayCanvas");
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 0;

        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvas.AddComponent<GraphicRaycaster>();

        // HUD Panel
        var hudPanel = CreateUIPanel(canvas.transform, "HUDPanel");
        SetAnchors(hudPanel, new Vector2(0, 1), new Vector2(1, 1));
        hudPanel.sizeDelta = new Vector2(0, 150);
        hudPanel.anchoredPosition = new Vector2(0, -75);
        
        AddComponentSafe<EscapeTrainRun.UI.GameHUD>(hudPanel.gameObject);

        // Score Text
        var scoreText = CreateTMPText(hudPanel, "ScoreText", "0", 48);
        SetAnchors(scoreText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        scoreText.rectTransform.anchoredPosition = new Vector2(0, -50);
        scoreText.alignment = TextAlignmentOptions.Center;

        // Coin Counter
        var coinPanel = CreateUIPanel(hudPanel, "CoinPanel");
        SetAnchors(coinPanel, new Vector2(1, 1), new Vector2(1, 1));
        coinPanel.sizeDelta = new Vector2(200, 60);
        coinPanel.anchoredPosition = new Vector2(-120, -50);

        var coinText = CreateTMPText(coinPanel, "CoinText", "0", 36);
        coinText.alignment = TextAlignmentOptions.Right;

        // Pause Button
        var pauseBtn = CreateButton(canvas.transform, "PauseButton", "||");
        SetAnchors(pauseBtn.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1));
        pauseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, -60);
        pauseBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

        Debug.Log("✅ Created UI Canvas");
    }

    private static void CreatePauseMenu()
    {
        var existingCanvas = GameObject.Find("GameplayCanvas");
        Transform parent = existingCanvas != null ? existingCanvas.transform : null;

        if (parent == null)
        {
            Debug.LogWarning("GameplayCanvas not found, creating PauseMenu as root");
            var pauseCanvas = CreateGameObject("PauseMenuCanvas");
            var canvasComp = pauseCanvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.sortingOrder = 10;
            pauseCanvas.AddComponent<CanvasScaler>();
            pauseCanvas.AddComponent<GraphicRaycaster>();
            parent = pauseCanvas.transform;
        }

        var pauseMenu = CreateUIPanel(parent, "PauseMenu");
        SetAnchors(pauseMenu, Vector2.zero, Vector2.one);
        pauseMenu.offsetMin = Vector2.zero;
        pauseMenu.offsetMax = Vector2.zero;
        pauseMenu.gameObject.SetActive(false);

        AddComponentSafe<EscapeTrainRun.UI.PauseMenuUI>(pauseMenu.gameObject);

        // Dark overlay
        var overlay = pauseMenu.gameObject.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.7f);

        // Panel
        var panel = CreateUIPanel(pauseMenu, "Panel");
        panel.sizeDelta = new Vector2(400, 500);
        panel.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Title
        var title = CreateTMPText(panel, "Title", "PAUSED", 48);
        title.rectTransform.anchoredPosition = new Vector2(0, 180);

        // Resume Button
        var resumeBtn = CreateButton(panel, "ResumeButton", "RESUME");
        resumeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);
        resumeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        // Main Menu Button
        var menuBtn = CreateButton(panel, "MainMenuButton", "MAIN MENU");
        menuBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
        menuBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        Debug.Log("✅ Created Pause Menu");
    }

    private static void CreateGameOverPanel()
    {
        var existingCanvas = GameObject.Find("GameplayCanvas");
        Transform parent = existingCanvas != null ? existingCanvas.transform : null;

        if (parent == null) return;

        var gameOverPanel = CreateUIPanel(parent, "GameOverPanel");
        SetAnchors(gameOverPanel, Vector2.zero, Vector2.one);
        gameOverPanel.offsetMin = Vector2.zero;
        gameOverPanel.offsetMax = Vector2.zero;
        gameOverPanel.gameObject.SetActive(false);

        AddComponentSafe<EscapeTrainRun.UI.GameOverUI>(gameOverPanel.gameObject);

        // Dark overlay
        var overlay = gameOverPanel.gameObject.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.8f);

        // Panel
        var panel = CreateUIPanel(gameOverPanel, "Panel");
        panel.sizeDelta = new Vector2(500, 600);
        panel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // Title
        var title = CreateTMPText(panel, "Title", "GAME OVER", 56);
        title.rectTransform.anchoredPosition = new Vector2(0, 220);
        title.color = new Color(1f, 0.3f, 0.3f);

        // Score
        var scoreValue = CreateTMPText(panel, "ScoreValue", "0", 64);
        scoreValue.rectTransform.anchoredPosition = new Vector2(0, 100);
        scoreValue.color = new Color(1f, 0.85f, 0.2f);

        // Retry Button
        var retryBtn = CreateButton(panel, "RetryButton", "PLAY AGAIN");
        retryBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
        retryBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        // Main Menu Button
        var menuBtn = CreateButton(panel, "MainMenuButton", "MAIN MENU");
        menuBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
        menuBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        Debug.Log("✅ Created Game Over Panel");
    }

    #endregion

    #region Helper Methods

    private static GameObject CreateGameObject(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            Debug.Log($"  ⏭️ '{name}' already exists, skipping");
            return existing;
        }

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        Debug.Log($"  ✓ Created '{name}'");
        return go;
    }

    private static GameObject CreateChildGameObject(GameObject parent, string name)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing.gameObject;

        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private static void AddComponentSafe<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null)
        {
            go.AddComponent<T>();
        }
    }

    private static RectTransform CreateUIPanel(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        
        var image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
        
        return rt;
    }

    private static TextMeshProUGUI CreateTMPText(RectTransform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(400, 80);
        rt.anchoredPosition = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(200, 60);
        rt.anchoredPosition = Vector2.zero;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.3f, 0.6f, 0.9f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        // Text child
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        textRt.localScale = Vector3.one;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
    }

    #endregion
}
