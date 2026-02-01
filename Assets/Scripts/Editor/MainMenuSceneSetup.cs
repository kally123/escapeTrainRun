using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the MainMenu scene.
/// Run from menu: Tools > Escape Train Run > Setup MainMenu Scene
/// </summary>
public class MainMenuSceneSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup MainMenu Scene")]
    public static void SetupMainMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Setup MainMenu Scene",
            "This will create all MainMenu scene objects. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateManagers();
        CreateCamera();
        CreateMainMenuCanvas();
        CreateSettingsPanel();

        Debug.Log("✅ MainMenu Scene Setup Complete!");
        EditorUtility.DisplayDialog("Success", "MainMenu scene setup complete!", "OK");
    }

    private static void CreateManagers()
    {
        var saveManager = CreateGameObject("SaveManager");
        AddComponentSafe<EscapeTrainRun.Core.SaveManager>(saveManager);

        var audioManager = CreateGameObject("AudioManager");
        AddComponentSafe<EscapeTrainRun.Core.AudioManager>(audioManager);

        Debug.Log("✅ Created Managers");
    }

    private static void CreateCamera()
    {
        var existingCam = Camera.main;
        if (existingCam != null)
        {
            Debug.Log("  ⏭️ Main Camera already exists");
            return;
        }

        var cameraGo = CreateGameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        var camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraGo.AddComponent<AudioListener>();

        Debug.Log("✅ Created Main Camera");
    }

    private static void CreateMainMenuCanvas()
    {
        var canvas = CreateGameObject("MainMenuCanvas");
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvas.AddComponent<GraphicRaycaster>();
        AddComponentSafe<EscapeTrainRun.UI.MainMenuUI>(canvas);

        // Title
        var title = CreateTMPText(canvas.transform, "Title", "ESCAPE TRAIN RUN", 72);
        title.rectTransform.anchoredPosition = new Vector2(0, 300);
        title.color = new Color(1f, 0.85f, 0.2f);

        // Play Button
        var playBtn = CreateButton(canvas.transform, "PlayButton", "PLAY");
        playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        playBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        var playColors = playBtn.colors;
        playColors.normalColor = new Color(0.2f, 0.8f, 0.3f);
        playBtn.colors = playColors;

        // Shop Button
        var shopBtn = CreateButton(canvas.transform, "ShopButton", "SHOP");
        shopBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
        shopBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        // Settings Button
        var settingsBtn = CreateButton(canvas.transform, "SettingsButton", "SETTINGS");
        settingsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -170);
        settingsBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        // High Score Display
        var highScore = CreateTMPText(canvas.transform, "HighScoreText", "HIGH SCORE: 0", 32);
        highScore.rectTransform.anchoredPosition = new Vector2(0, 180);

        Debug.Log("✅ Created Main Menu Canvas");
    }

    private static void CreateSettingsPanel()
    {
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null) return;

        var settingsPanel = CreateUIPanel(canvas.transform, "SettingsPanel");
        SetAnchors(settingsPanel, Vector2.zero, Vector2.one);
        settingsPanel.offsetMin = Vector2.zero;
        settingsPanel.offsetMax = Vector2.zero;
        settingsPanel.gameObject.SetActive(false);

        AddComponentSafe<EscapeTrainRun.UI.SettingsUI>(settingsPanel.gameObject);

        // Overlay
        var overlay = settingsPanel.gameObject.GetComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.8f);

        // Panel
        var panel = CreateUIPanel(settingsPanel, "Panel");
        panel.sizeDelta = new Vector2(600, 500);
        panel.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.95f);

        // Title
        var title = CreateTMPText(panel, "Title", "SETTINGS", 48);
        title.rectTransform.anchoredPosition = new Vector2(0, 200);

        // Music Volume
        var musicLabel = CreateTMPText(panel, "MusicLabel", "Music", 28);
        musicLabel.rectTransform.anchoredPosition = new Vector2(-150, 80);
        musicLabel.alignment = TextAlignmentOptions.Left;

        var musicSlider = CreateSlider(panel, "MusicSlider");
        musicSlider.anchoredPosition = new Vector2(100, 80);

        // SFX Volume
        var sfxLabel = CreateTMPText(panel, "SFXLabel", "Sound FX", 28);
        sfxLabel.rectTransform.anchoredPosition = new Vector2(-150, 0);
        sfxLabel.alignment = TextAlignmentOptions.Left;

        var sfxSlider = CreateSlider(panel, "SFXSlider");
        sfxSlider.anchoredPosition = new Vector2(100, 0);

        // Close Button
        var closeBtn = CreateButton(panel, "CloseButton", "CLOSE");
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -180);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);

        Debug.Log("✅ Created Settings Panel");
    }

    #region Helper Methods

    private static GameObject CreateGameObject(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            Debug.Log($"  ⏭️ '{name}' already exists");
            return existing;
        }

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
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

    private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(600, 80);
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

        var image = go.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

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
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private static RectTransform CreateSlider(RectTransform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(300, 30);

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.8f;

        // Background
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(go.transform);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgRt.localScale = Vector3.one;
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform);
        var fillRt = fillArea.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        fillRt.localScale = Vector3.one;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        var fillImgRt = fill.AddComponent<RectTransform>();
        fillImgRt.anchorMin = Vector2.zero;
        fillImgRt.anchorMax = Vector2.one;
        fillImgRt.offsetMin = Vector2.zero;
        fillImgRt.offsetMax = Vector2.zero;
        fillImgRt.localScale = Vector3.one;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.6f, 0.9f);

        slider.fillRect = fillImgRt;

        return rt;
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
    }

    #endregion
}
