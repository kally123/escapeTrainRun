using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the MainMenu scene with all required GameObjects.
/// Run from menu: Tools > Escape Train Run > Setup MainMenu Scene
/// </summary>
public class MainMenuSceneSetup : Editor
{
    private static Color primaryColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    private static Color secondaryColor = new Color(0.9f, 0.4f, 0.2f, 1f);
    private static Color panelColor = new Color(0.15f, 0.15f, 0.25f, 0.95f);

    [MenuItem("Tools/Escape Train Run/Setup MainMenu Scene")]
    public static void SetupMainMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Setup MainMenu Scene",
            "This will create all MainMenu scene objects. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateManagers();
        CreateMainMenuCanvas();
        CreateSettingsPanel();
        CreateParentGatePanel();
        CreateBackground();

        Debug.Log("✅ MainMenu Scene Setup Complete!");
        EditorUtility.DisplayDialog("Success", "MainMenu scene setup complete!\n\nRemember to:\n1. Add background image/sprite\n2. Add logo image\n3. Connect button events in Inspector", "OK");
    }

    #region Managers

    private static void CreateManagers()
    {
        // MenuManager
        var menuManager = CreateGameObject("MenuManager");
        AddComponentSafe<EscapeTrainRun.MainMenuUI>(menuManager);

        // SaveManager (needed for high scores)
        var saveManager = CreateGameObject("SaveManager");
        AddComponentSafe<EscapeTrainRun.SaveManager>(saveManager);

        // AudioManager
        var audioManager = CreateGameObject("AudioManager");
        AddComponentSafe<EscapeTrainRun.AudioManager>(audioManager);

        var musicSource = CreateChildGameObject(audioManager, "MusicSource");
        var musicAudio = musicSource.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.playOnAwake = true;

        var sfxSource = CreateChildGameObject(audioManager, "SFXSource");
        sfxSource.AddComponent<AudioSource>();

        // ParentControlManager
        var parentManager = CreateGameObject("ParentControlManager");
        AddComponentSafe<EscapeTrainRun.ParentControlManager>(parentManager);

        Debug.Log("✅ MainMenu Managers created");
    }

    #endregion

    #region Main Canvas

    private static void CreateMainMenuCanvas()
    {
        // Main Canvas
        var canvasObj = CreateGameObject("MainMenuCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Background Image
        var background = CreateUIImage(canvasObj, "Background");
        SetRectTransformFull(background);
        background.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 1f);

        // Logo placeholder
        var logo = CreateUIImage(canvasObj, "Logo");
        SetRectTransform(logo, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(400, 200));
        logo.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Placeholder

        // Title Text
        var titleText = CreateTextMeshPro(canvasObj, "TitleText", "ESCAPE TRAIN RUN", 72);
        SetRectTransform(titleText, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -420), new Vector2(800, 100));
        var titleTMP = titleText.GetComponent<TextMeshProUGUI>();
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white;

        // High Score Display
        var highScore = CreateTextMeshPro(canvasObj, "HighScoreDisplay", "BEST: 0", 36);
        SetRectTransform(highScore, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -520), new Vector2(400, 50));
        highScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.4f, 1f);

        // Play Button (main CTA)
        var playButton = CreateUIButton(canvasObj, "PlayButton", "PLAY", 56);
        SetRectTransform(playButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(500, 120));
        playButton.GetComponent<Image>().color = secondaryColor;

        // Theme Selection Container
        var themeSelection = CreateUIPanel(canvasObj, "ThemeSelection");
        SetRectTransform(themeSelection, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(700, 150));

        // Theme Label
        var themeLabel = CreateTextMeshPro(themeSelection, "ThemeLabel", "SELECT THEME", 28);
        SetRectTransform(themeLabel, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(300, 40));

        // Train Theme Button
        var trainBtn = CreateThemeButton(themeSelection, "TrainButton", "🚂\nTRAIN", -220);
        
        // Bus Theme Button
        var busBtn = CreateThemeButton(themeSelection, "BusButton", "🚌\nBUS", 0);
        
        // Park Theme Button
        var parkBtn = CreateThemeButton(themeSelection, "ParkButton", "🌳\nPARK", 220);

        // Bottom Buttons Container
        var bottomButtons = CreateUIPanel(canvasObj, "BottomButtons");
        SetRectTransform(bottomButtons, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(800, 120));

        // Shop Button
        var shopBtn = CreateUIButton(bottomButtons, "ShopButton", "SHOP", 32);
        SetRectTransform(shopBtn, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 0), new Vector2(180, 80));
        shopBtn.GetComponent<Image>().color = primaryColor;

        // Leaderboard Button
        var leaderboardBtn = CreateUIButton(bottomButtons, "LeaderboardButton", "RANKS", 32);
        SetRectTransform(leaderboardBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(180, 80));
        leaderboardBtn.GetComponent<Image>().color = primaryColor;

        // Settings Button
        var settingsBtn = CreateUIButton(bottomButtons, "SettingsButton", "⚙", 48);
        SetRectTransform(settingsBtn, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-100, 0), new Vector2(80, 80));
        settingsBtn.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.5f, 1f);

        // Coin Display (top right)
        var coinDisplay = CreateUIPanel(canvasObj, "CoinDisplay");
        SetRectTransform(coinDisplay, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-100, -60), new Vector2(180, 60));
        coinDisplay.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        var coinIcon = CreateUIImage(coinDisplay, "CoinIcon");
        SetRectTransform(coinIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(40, 40));
        coinIcon.GetComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 1f);

        var coinText = CreateTextMeshPro(coinDisplay, "CoinText", "0", 32);
        SetRectTransform(coinText, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(100, 40));

        Debug.Log("✅ MainMenu Canvas created");
    }

    private static GameObject CreateThemeButton(GameObject parent, string name, string text, float xOffset)
    {
        var btn = CreateUIButton(parent, name, text, 24);
        SetRectTransform(btn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffset, -20), new Vector2(150, 100));
        btn.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 1f);
        
        var tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.fontSize = 24;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 18;
            tmpText.fontSizeMax = 28;
        }
        
        return btn;
    }

    #endregion

    #region Settings Panel

    private static void CreateSettingsPanel()
    {
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null) return;

        var settingsPanel = CreateUIPanel(canvas, "SettingsPanel");
        SetRectTransformFull(settingsPanel);
        AddComponentSafe<EscapeTrainRun.SettingsUI>(settingsPanel);
        settingsPanel.SetActive(false);

        // Dim background
        var bg = settingsPanel.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);

        // Content Panel
        var panel = CreateUIPanel(settingsPanel, "Panel");
        SetRectTransform(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 900));
        panel.GetComponent<Image>().color = panelColor;

        // Title
        var title = CreateTextMeshPro(panel, "SettingsTitle", "SETTINGS", 56);
        SetRectTransform(title, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(400, 70));

        // Close Button
        var closeBtn = CreateUIButton(panel, "CloseButton", "✕", 36);
        SetRectTransform(closeBtn, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40), new Vector2(60, 60));
        closeBtn.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);

        // Music Slider
        var musicLabel = CreateTextMeshPro(panel, "MusicLabel", "MUSIC", 28);
        SetRectTransform(musicLabel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -160), new Vector2(150, 40));
        musicLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        var musicSlider = CreateSlider(panel, "MusicSlider");
        SetRectTransform(musicSlider, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(50, -200), new Vector2(400, 40));

        // SFX Slider
        var sfxLabel = CreateTextMeshPro(panel, "SFXLabel", "SOUND FX", 28);
        SetRectTransform(sfxLabel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -280), new Vector2(150, 40));
        sfxLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        var sfxSlider = CreateSlider(panel, "SFXSlider");
        SetRectTransform(sfxSlider, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(50, -320), new Vector2(400, 40));

        // Vibration Toggle
        var vibrateLabel = CreateTextMeshPro(panel, "VibrateLabel", "VIBRATION", 28);
        SetRectTransform(vibrateLabel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -400), new Vector2(200, 40));
        vibrateLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        var vibrateToggle = CreateToggle(panel, "VibrateToggle");
        SetRectTransform(vibrateToggle, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-100, -400), new Vector2(80, 40));

        // Divider
        var divider = CreateUIImage(panel, "Divider");
        SetRectTransform(divider, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -480), new Vector2(600, 2));
        divider.GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);

        // Privacy Button
        var privacyBtn = CreateUIButton(panel, "PrivacyButton", "Privacy Policy", 28);
        SetRectTransform(privacyBtn, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -550), new Vector2(500, 60));
        privacyBtn.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

        // Credits Button
        var creditsBtn = CreateUIButton(panel, "CreditsButton", "Credits", 28);
        SetRectTransform(creditsBtn, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -630), new Vector2(500, 60));
        creditsBtn.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

        // Parent Controls Button
        var parentBtn = CreateUIButton(panel, "ParentControlsButton", "Parent Controls", 28);
        SetRectTransform(parentBtn, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -710), new Vector2(500, 60));
        parentBtn.GetComponent<Image>().color = new Color(0.4f, 0.3f, 0.5f, 1f);

        // Version Text
        var versionText = CreateTextMeshPro(panel, "VersionText", "Version 1.0.0", 20);
        SetRectTransform(versionText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(200, 30));
        versionText.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);

        Debug.Log("✅ Settings Panel created");
    }

    #endregion

    #region Parent Gate Panel

    private static void CreateParentGatePanel()
    {
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null) return;

        var parentGate = CreateUIPanel(canvas, "ParentGatePanel");
        SetRectTransformFull(parentGate);
        AddComponentSafe<EscapeTrainRun.ParentGateUI>(parentGate);
        parentGate.SetActive(false);

        var bg = parentGate.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);

        // Content Panel
        var panel = CreateUIPanel(parentGate, "Panel");
        SetRectTransform(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 500));
        panel.GetComponent<Image>().color = panelColor;

        // Title
        var title = CreateTextMeshPro(panel, "Title", "PARENT VERIFICATION", 36);
        SetRectTransform(title, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(500, 50));

        // Question
        var question = CreateTextMeshPro(panel, "Question", "What is 7 × 8?", 48);
        SetRectTransform(question, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(400, 70));

        // Answer Buttons Container
        var answersContainer = CreateUIPanel(panel, "AnswersContainer");
        SetRectTransform(answersContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(500, 100));

        // Answer buttons (4 options)
        var ans1 = CreateUIButton(answersContainer, "Answer1", "54", 32);
        SetRectTransform(ans1, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(60, 0), new Vector2(100, 80));

        var ans2 = CreateUIButton(answersContainer, "Answer2", "56", 32);
        SetRectTransform(ans2, new Vector2(0.33f, 0.5f), new Vector2(0.33f, 0.5f), new Vector2(25, 0), new Vector2(100, 80));

        var ans3 = CreateUIButton(answersContainer, "Answer3", "58", 32);
        SetRectTransform(ans3, new Vector2(0.66f, 0.5f), new Vector2(0.66f, 0.5f), new Vector2(-25, 0), new Vector2(100, 80));

        var ans4 = CreateUIButton(answersContainer, "Answer4", "62", 32);
        SetRectTransform(ans4, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-60, 0), new Vector2(100, 80));

        // Cancel Button
        var cancelBtn = CreateUIButton(panel, "CancelButton", "CANCEL", 28);
        SetRectTransform(cancelBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(200, 60));
        cancelBtn.GetComponent<Image>().color = new Color(0.5f, 0.3f, 0.3f, 1f);

        Debug.Log("✅ Parent Gate Panel created");
    }

    #endregion

    #region Background

    private static void CreateBackground()
    {
        // Create a simple 3D background for visual depth
        var bgContainer = CreateGameObject("Background3D");
        bgContainer.transform.position = new Vector3(0, 0, 10);

        // Directional Light
        var lightObj = new GameObject("DirectionalLight");
        lightObj.transform.SetParent(bgContainer.transform);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f, 1f);
        light.intensity = 1f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Camera (for 3D background if needed)
        var camObj = GameObject.Find("Main Camera");
        if (camObj == null)
        {
            camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        camObj.transform.position = new Vector3(0, 2, -10);
        camObj.transform.rotation = Quaternion.identity;

        Debug.Log("✅ Background created");
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
        image.color = new Color(1, 1, 1, 0);
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

    private static GameObject CreateUIButton(GameObject parent, string name, string text, float fontSize = 32)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        buttonObj.AddComponent<RectTransform>();
        buttonObj.AddComponent<CanvasRenderer>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        buttonObj.AddComponent<Button>();

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
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

    private static GameObject CreateSlider(GameObject parent, string name)
    {
        var sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent.transform, false);
        sliderObj.AddComponent<RectTransform>();
        
        // Background
        var bgObj = CreateUIImage(sliderObj, "Background");
        SetRectTransformFull(bgObj);
        bgObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(5, 5);
        fillRect.offsetMax = new Vector2(-5, -5);

        var fill = CreateUIImage(fillArea, "Fill");
        var fillRectTransform = fill.GetComponent<RectTransform>();
        fillRectTransform.anchorMin = Vector2.zero;
        fillRectTransform.anchorMax = new Vector2(0, 1);
        fillRectTransform.offsetMin = Vector2.zero;
        fillRectTransform.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = primaryColor;

        // Handle
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        var handle = CreateUIImage(handleArea, "Handle");
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(30, 0);
        handle.GetComponent<Image>().color = Color.white;

        // Add Slider component
        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        Undo.RegisterCreatedObjectUndo(sliderObj, $"Create {name}");
        return sliderObj;
    }

    private static GameObject CreateToggle(GameObject parent, string name)
    {
        var toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent.transform, false);
        toggleObj.AddComponent<RectTransform>();

        // Background
        var bg = CreateUIImage(toggleObj, "Background");
        SetRectTransformFull(bg);
        bg.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

        // Checkmark
        var checkmark = CreateUIImage(toggleObj, "Checkmark");
        SetRectTransform(checkmark, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
        checkmark.GetComponent<Image>().color = primaryColor;

        var toggle = toggleObj.AddComponent<Toggle>();
        toggle.targetGraphic = bg.GetComponent<Image>();
        toggle.graphic = checkmark.GetComponent<Image>();
        toggle.isOn = true;

        Undo.RegisterCreatedObjectUndo(toggleObj, $"Create {name}");
        return toggleObj;
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

    private static void SetRectTransformFull(GameObject go)
    {
        var rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = go.AddComponent<RectTransform>();
        
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #endregion
}
