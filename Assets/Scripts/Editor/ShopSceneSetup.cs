using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the Shop scene with all required GameObjects.
/// Run from menu: Tools > Escape Train Run > Setup Shop Scene
/// </summary>
public class ShopSceneSetup : Editor
{
    private static Color primaryColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    private static Color accentColor = new Color(0.9f, 0.7f, 0.2f, 1f);
    private static Color panelColor = new Color(0.12f, 0.12f, 0.2f, 1f);
    private static Color cardColor = new Color(0.18f, 0.18f, 0.28f, 1f);

    [MenuItem("Tools/Escape Train Run/Setup Shop Scene")]
    public static void SetupShopScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Shop Scene",
            "This will create all Shop scene objects. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateManagers();
        CreateShopCanvas();
        CreateCharacterPreviewArea();
        CreateCharacterCardPrefab();

        Debug.Log("✅ Shop Scene Setup Complete!");
        EditorUtility.DisplayDialog("Success", "Shop scene setup complete!\n\nRemember to:\n1. Create CharacterData ScriptableObjects\n2. Assign character models to preview\n3. Connect ShopUI references in Inspector", "OK");
    }

    #region Managers

    private static void CreateManagers()
    {
        // ShopManager
        var shopManager = CreateGameObject("ShopManager");
        AddComponentSafe<EscapeTrainRun.ShopManager>(shopManager);

        // SaveManager
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

        Debug.Log("✅ Shop Managers created");
    }

    #endregion

    #region Shop Canvas

    private static void CreateShopCanvas()
    {
        // Main Canvas
        var canvasObj = CreateGameObject("ShopCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        AddComponentSafe<EscapeTrainRun.ShopUI>(canvasObj);

        // Background
        var background = CreateUIImage(canvasObj, "Background");
        SetRectTransformFull(background);
        background.GetComponent<Image>().color = panelColor;

        // Header
        var header = CreateUIPanel(canvasObj, "Header");
        SetRectTransform(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, 120));
        header.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 1f);

        // Back Button
        var backBtn = CreateUIButton(header, "BackButton", "←", 48);
        SetRectTransform(backBtn, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(60, 0), new Vector2(80, 80));
        backBtn.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

        // Shop Title
        var shopTitle = CreateTextMeshPro(header, "ShopTitle", "CHARACTER SHOP", 48);
        SetRectTransform(shopTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 60));
        shopTitle.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Coin Display
        var coinDisplay = CreateUIPanel(header, "CoinDisplay");
        SetRectTransform(coinDisplay, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-100, 0), new Vector2(160, 50));
        coinDisplay.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);

        var coinIcon = CreateUIImage(coinDisplay, "CoinIcon");
        SetRectTransform(coinIcon, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(25, 0), new Vector2(35, 35));
        coinIcon.GetComponent<Image>().color = accentColor;

        var coinText = CreateTextMeshPro(coinDisplay, "CoinText", "0", 28);
        SetRectTransform(coinText, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(90, 40));

        // Character Scroll View
        CreateCharacterScrollView(canvasObj);

        // Bottom Action Bar
        CreateBottomActionBar(canvasObj);

        Debug.Log("✅ Shop Canvas created");
    }

    private static void CreateCharacterScrollView(GameObject canvas)
    {
        // Scroll View Container
        var scrollView = new GameObject("CharacterScrollView");
        scrollView.transform.SetParent(canvas.transform, false);
        var scrollRect = scrollView.AddComponent<RectTransform>();
        SetRectTransform(scrollView, new Vector2(0, 0.35f), new Vector2(1, 0.9f), Vector2.zero, Vector2.zero);

        var scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        var scrollImage = scrollView.AddComponent<Image>();
        scrollImage.color = new Color(0, 0, 0, 0);
        scrollView.AddComponent<Mask>().showMaskGraphic = false;

        // Viewport
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 800); // Will expand based on content

        // Grid Layout
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 380);
        grid.spacing = new Vector2(30, 30);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.padding = new RectOffset(40, 40, 20, 20);

        // Content Size Fitter
        var sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Connect scroll rect
        scroll.viewport = viewportRect;
        scroll.content = contentRect;

        // Add sample character cards (3 for demonstration)
        for (int i = 0; i < 6; i++)
        {
            CreateSampleCharacterCard(content, i);
        }

        Debug.Log("✅ Character ScrollView created");
    }

    private static void CreateSampleCharacterCard(GameObject parent, int index)
    {
        string[] names = { "Runner", "Ninja", "Pirate", "Astronaut", "Wizard", "Robot" };
        int[] prices = { 0, 500, 1000, 2000, 3000, 5000 };
        bool[] unlocked = { true, false, false, false, false, false };

        var card = CreateUIPanel(parent, $"CharacterCard_{index}");
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(300, 380);
        card.GetComponent<Image>().color = cardColor;

        var button = card.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        button.colors = colors;

        // Character Preview Image
        var previewBg = CreateUIImage(card, "PreviewBackground");
        SetRectTransform(previewBg, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(260, 220));
        previewBg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.18f, 1f);

        var characterImage = CreateUIImage(previewBg, "CharacterImage");
        SetRectTransform(characterImage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 180));
        characterImage.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.6f, 0.5f); // Placeholder

        // Character Name
        var nameText = CreateTextMeshPro(card, "CharacterName", names[index], 28);
        SetRectTransform(nameText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(260, 40));
        nameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Price or Status
        var priceText = CreateTextMeshPro(card, "PriceText", unlocked[index] ? "OWNED" : $"🪙 {prices[index]}", 24);
        SetRectTransform(priceText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(200, 35));
        var priceTMP = priceText.GetComponent<TextMeshProUGUI>();
        priceTMP.color = unlocked[index] ? new Color(0.4f, 0.8f, 0.4f, 1f) : accentColor;

        // Selected Indicator (hidden by default)
        var selectedIndicator = CreateUIImage(card, "SelectedIndicator");
        SetRectTransform(selectedIndicator, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-15, -15), new Vector2(40, 40));
        selectedIndicator.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f, 1f);
        selectedIndicator.SetActive(index == 0); // First one is selected

        // Lock Overlay (for locked characters)
        if (!unlocked[index])
        {
            var lockOverlay = CreateUIPanel(card, "LockOverlay");
            SetRectTransformFull(lockOverlay);
            lockOverlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            var lockIcon = CreateTextMeshPro(lockOverlay, "LockIcon", "🔒", 48);
            SetRectTransform(lockIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(80, 80));
        }
    }

    private static void CreateBottomActionBar(GameObject canvas)
    {
        var actionBar = CreateUIPanel(canvas, "ActionBar");
        SetRectTransform(actionBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 100), new Vector2(0, 200));
        actionBar.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 1f);

        // Selected Character Info
        var selectedInfo = CreateUIPanel(actionBar, "SelectedInfo");
        SetRectTransform(selectedInfo, new Vector2(0, 0), new Vector2(0.6f, 1), Vector2.zero, Vector2.zero);

        var selectedName = CreateTextMeshPro(selectedInfo, "SelectedName", "Runner", 36);
        SetRectTransform(selectedName, new Vector2(0, 0.7f), new Vector2(1, 1), new Vector2(30, -10), new Vector2(-60, 0));
        selectedName.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        selectedName.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var selectedDesc = CreateTextMeshPro(selectedInfo, "SelectedDescription", "The default runner. Fast and agile!", 22);
        SetRectTransform(selectedDesc, new Vector2(0, 0.3f), new Vector2(1, 0.7f), new Vector2(30, 0), new Vector2(-60, 0));
        var descTMP = selectedDesc.GetComponent<TextMeshProUGUI>();
        descTMP.alignment = TextAlignmentOptions.TopLeft;
        descTMP.color = new Color(0.7f, 0.7f, 0.8f, 1f);

        var abilityText = CreateTextMeshPro(selectedInfo, "AbilityText", "Ability: None", 20);
        SetRectTransform(abilityText, new Vector2(0, 0), new Vector2(1, 0.3f), new Vector2(30, 10), new Vector2(-60, 0));
        var abilityTMP = abilityText.GetComponent<TextMeshProUGUI>();
        abilityTMP.alignment = TextAlignmentOptions.Left;
        abilityTMP.color = new Color(0.5f, 0.7f, 0.9f, 1f);

        // Action Buttons
        var buttonContainer = CreateUIPanel(actionBar, "ButtonContainer");
        SetRectTransform(buttonContainer, new Vector2(0.6f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

        // Select/Buy Button
        var selectBtn = CreateUIButton(buttonContainer, "SelectButton", "SELECT", 32);
        SetRectTransform(selectBtn, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(280, 70));
        selectBtn.GetComponent<Image>().color = primaryColor;

        // Buy Button (alternative)
        var buyBtn = CreateUIButton(buttonContainer, "BuyButton", "BUY - 500", 32);
        SetRectTransform(buyBtn, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(280, 70));
        buyBtn.GetComponent<Image>().color = accentColor;
        buyBtn.SetActive(false); // Hidden when character is owned

        // Equip Status
        var equipStatus = CreateTextMeshPro(buttonContainer, "EquipStatus", "✓ EQUIPPED", 24);
        SetRectTransform(equipStatus, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(200, 35));
        equipStatus.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.8f, 0.4f, 1f);

        Debug.Log("✅ Action Bar created");
    }

    #endregion

    #region Character Preview (3D)

    private static void CreateCharacterPreviewArea()
    {
        // 3D Preview Setup (optional, for rotating character display)
        var previewContainer = CreateGameObject("CharacterPreview3D");
        previewContainer.transform.position = new Vector3(0, 0, 5);

        // Preview Camera
        var previewCamObj = new GameObject("PreviewCamera");
        previewCamObj.transform.SetParent(previewContainer.transform);
        previewCamObj.transform.localPosition = new Vector3(0, 1, -3);
        previewCamObj.transform.localRotation = Quaternion.Euler(10, 0, 0);

        var previewCam = previewCamObj.AddComponent<Camera>();
        previewCam.clearFlags = CameraClearFlags.SolidColor;
        previewCam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0f);
        previewCam.cullingMask = 1 << LayerMask.NameToLayer("Default"); // Adjust layer as needed
        previewCam.fieldOfView = 40f;
        previewCam.nearClipPlane = 0.1f;
        previewCam.farClipPlane = 10f;
        previewCam.depth = -1; // Render before main camera

        // Character Spawn Point
        var spawnPoint = CreateChildGameObject(previewContainer, "CharacterSpawnPoint");
        spawnPoint.transform.localPosition = Vector3.zero;

        // Turntable (for rotation)
        var turntable = CreateChildGameObject(previewContainer, "Turntable");
        turntable.transform.localPosition = Vector3.zero;

        // Light for preview
        var lightObj = new GameObject("PreviewLight");
        lightObj.transform.SetParent(previewContainer.transform);
        lightObj.transform.localPosition = new Vector3(2, 3, -2);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = Color.white;
        lightObj.transform.LookAt(spawnPoint.transform);

        Debug.Log("✅ Character Preview 3D area created");
    }

    #endregion

    #region Character Card Prefab

    private static void CreateCharacterCardPrefab()
    {
        // Create a template that can be saved as a prefab
        var prefabContainer = CreateGameObject("_CharacterCardTemplate");
        prefabContainer.SetActive(false); // Hidden template

        var cardTemplate = CreateUIPanel(prefabContainer, "CharacterCard");
        var cardRect = cardTemplate.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(300, 380);
        cardTemplate.GetComponent<Image>().color = cardColor;

        var button = cardTemplate.AddComponent<Button>();
        
        // Add CharacterCard component if it exists
        // AddComponentSafe<EscapeTrainRun.CharacterCard>(cardTemplate);

        // Preview Background
        var previewBg = CreateUIImage(cardTemplate, "PreviewBackground");
        SetRectTransform(previewBg, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(260, 220));
        previewBg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.18f, 1f);

        // Character Image
        var charImage = CreateUIImage(previewBg, "CharacterImage");
        SetRectTransform(charImage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 180));

        // Name
        var nameText = CreateTextMeshPro(cardTemplate, "NameText", "Character", 28);
        SetRectTransform(nameText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(260, 40));
        nameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Price
        var priceText = CreateTextMeshPro(cardTemplate, "PriceText", "🪙 1000", 24);
        SetRectTransform(priceText, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(200, 35));
        priceText.GetComponent<TextMeshProUGUI>().color = accentColor;

        // Selected Indicator
        var selectedInd = CreateUIImage(cardTemplate, "SelectedIndicator");
        SetRectTransform(selectedInd, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-15, -15), new Vector2(40, 40));
        selectedInd.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f, 1f);
        selectedInd.SetActive(false);

        // Lock Overlay
        var lockOverlay = CreateUIPanel(cardTemplate, "LockOverlay");
        SetRectTransformFull(lockOverlay);
        lockOverlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        var lockIcon = CreateTextMeshPro(lockOverlay, "LockIcon", "🔒", 48);
        SetRectTransform(lockIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(80, 80));

        Debug.Log("✅ Character Card Template created (save as prefab from Hierarchy)");
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
        image.color = primaryColor;
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
