using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the Shop scene.
/// Run from menu: Tools > Escape Train Run > Setup Shop Scene
/// </summary>
public class ShopSceneSetup : Editor
{
    [MenuItem("Tools/Escape Train Run/Setup Shop Scene")]
    public static void SetupShopScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Shop Scene",
            "This will create all Shop scene objects. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateManagers();
        CreateCamera();
        CreateShopCanvas();
        CreateCharacterPreview();

        Debug.Log("✅ Shop Scene Setup Complete!");
        EditorUtility.DisplayDialog("Success", "Shop scene setup complete!", "OK");
    }

    private static void CreateManagers()
    {
        var saveManager = CreateGameObject("SaveManager");
        AddComponentSafe<EscapeTrainRun.Core.SaveManager>(saveManager);

        var audioManager = CreateGameObject("AudioManager");
        AddComponentSafe<EscapeTrainRun.Core.AudioManager>(audioManager);

        var characterManager = CreateGameObject("CharacterManager");
        AddComponentSafe<EscapeTrainRun.Characters.CharacterManager>(characterManager);

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

    private static void CreateShopCanvas()
    {
        var canvas = CreateGameObject("ShopCanvas");
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvas.AddComponent<GraphicRaycaster>();
        AddComponentSafe<EscapeTrainRun.UI.CharacterSelectionUI>(canvas);

        // Header Panel
        var header = CreateUIPanel(canvas.transform, "Header");
        SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1));
        header.sizeDelta = new Vector2(0, 120);
        header.anchoredPosition = new Vector2(0, -60);
        header.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        // Title
        var title = CreateTMPText(header, "Title", "CHARACTER SHOP", 48);
        title.rectTransform.anchoredPosition = new Vector2(0, 0);

        // Back Button
        var backBtn = CreateButton(header, "BackButton", "< BACK");
        backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-800, 0);
        backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 60);

        // Coins Display
        var coinsPanel = CreateUIPanel(header, "CoinsPanel");
        coinsPanel.anchoredPosition = new Vector2(800, 0);
        coinsPanel.sizeDelta = new Vector2(200, 60);

        var coinsText = CreateTMPText(coinsPanel, "CoinsText", "1000", 36);
        coinsText.color = new Color(1f, 0.85f, 0.2f);

        // Character Grid
        var gridPanel = CreateUIPanel(canvas.transform, "CharacterGrid");
        SetAnchors(gridPanel, new Vector2(0, 0), new Vector2(0.5f, 1));
        gridPanel.offsetMin = new Vector2(50, 150);
        gridPanel.offsetMax = new Vector2(-25, -150);

        var scrollRect = gridPanel.gameObject.AddComponent<ScrollRect>();
        
        var content = CreateUIPanel(gridPanel, "Content");
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.sizeDelta = new Vector2(0, 800);

        var gridLayout = content.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(200, 250);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.padding = new RectOffset(20, 20, 20, 20);
        gridLayout.childAlignment = TextAnchor.UpperCenter;

        scrollRect.content = content;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        // Create sample character cards
        for (int i = 0; i < 6; i++)
        {
            CreateCharacterCard(content, $"Character_{i}");
        }

        // Preview Panel
        var previewPanel = CreateUIPanel(canvas.transform, "PreviewPanel");
        SetAnchors(previewPanel, new Vector2(0.5f, 0), new Vector2(1, 1));
        previewPanel.offsetMin = new Vector2(25, 150);
        previewPanel.offsetMax = new Vector2(-50, -150);
        previewPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

        AddComponentSafe<EscapeTrainRun.UI.CharacterPreviewUI>(previewPanel.gameObject);

        // Preview Character Name
        var charName = CreateTMPText(previewPanel, "CharacterName", "Select a Character", 42);
        charName.rectTransform.anchoredPosition = new Vector2(0, 200);

        // Buy/Select Button
        var actionBtn = CreateButton(previewPanel, "ActionButton", "SELECT");
        actionBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
        actionBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);

        Debug.Log("✅ Created Shop Canvas");
    }

    private static void CreateCharacterCard(RectTransform parent, string name)
    {
        var card = CreateUIPanel(parent, name);
        card.sizeDelta = new Vector2(200, 250);
        card.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

        AddComponentSafe<EscapeTrainRun.UI.CharacterCard>(card.gameObject);

        var icon = CreateUIPanel(card, "Icon");
        icon.anchoredPosition = new Vector2(0, 25);
        icon.sizeDelta = new Vector2(150, 150);
        icon.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.5f);

        var nameText = CreateTMPText(card, "Name", "Character", 24);
        nameText.rectTransform.anchoredPosition = new Vector2(0, -80);

        var priceText = CreateTMPText(card, "Price", "500", 20);
        priceText.rectTransform.anchoredPosition = new Vector2(0, -105);
        priceText.color = new Color(1f, 0.85f, 0.2f);
    }

    private static void CreateCharacterPreview()
    {
        var preview = CreateGameObject("CharacterPreview");
        preview.transform.position = new Vector3(5, 0, 0);

        var previewSpot = new GameObject("PreviewSpot");
        previewSpot.transform.SetParent(preview.transform);
        previewSpot.transform.localPosition = Vector3.zero;

        var previewLight = new GameObject("PreviewLight");
        previewLight.transform.SetParent(preview.transform);
        previewLight.transform.localPosition = new Vector3(0, 3, -2);
        previewLight.transform.rotation = Quaternion.Euler(30, 0, 0);
        var light = previewLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;

        Debug.Log("✅ Created Character Preview");
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

    private static TextMeshProUGUI CreateTMPText(RectTransform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(400, 60);
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
        image.color = new Color(0.3f, 0.6f, 0.9f);

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
