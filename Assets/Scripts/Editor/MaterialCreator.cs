using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create all game materials with proper settings.
/// Run from menu: Tools > Escape Train Run > Create All Materials
/// </summary>
public class MaterialCreator : Editor
{
    private static string materialsPath = "Assets/Materials";

    [MenuItem("Tools/Escape Train Run/Create All Materials")]
    public static void CreateAllMaterials()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("🎨 CREATING ALL MATERIALS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        CreateDirectories();
        CreateTrackMaterials();
        CreateObstacleMaterials();
        CreateCollectibleMaterials();
        CreateCharacterMaterials();
        CreateEnvironmentMaterials();
        CreateUIEffectMaterials();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ All materials created successfully!");
        EditorUtility.DisplayDialog("Materials Created", 
            "All game materials have been created in Assets/Materials/", "OK");
    }

    private static void CreateDirectories()
    {
        string[] folders = new string[]
        {
            "Assets/Materials",
            "Assets/Materials/Track",
            "Assets/Materials/Obstacles",
            "Assets/Materials/Collectibles",
            "Assets/Materials/Characters",
            "Assets/Materials/Environment",
            "Assets/Materials/UI"
        };

        foreach (var folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
                string name = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    #region Track Materials

    [MenuItem("Tools/Escape Train Run/Materials/Create Track Materials")]
    public static void CreateTrackMaterials()
    {
        Debug.Log("\n🛤️ Creating Track Materials...");

        // City Theme - Gray concrete
        CreateMaterial("Track/Track_City_Floor", 
            new Color(0.6f, 0.6f, 0.65f), 0f, 0.7f, 
            "City street floor - gray concrete");

        CreateMaterial("Track/Track_City_Wall",
            new Color(0.5f, 0.5f, 0.55f), 0f, 0.8f,
            "City building walls");

        CreateMaterial("Track/Track_City_Rail",
            new Color(0.3f, 0.3f, 0.35f), 0.6f, 0.3f,
            "Metal rails - dark metallic");

        // Forest Theme - Natural browns/greens
        CreateMaterial("Track/Track_Forest_Floor",
            new Color(0.4f, 0.3f, 0.2f), 0f, 0.9f,
            "Forest dirt path");

        CreateMaterial("Track/Track_Forest_Grass",
            new Color(0.3f, 0.5f, 0.2f), 0f, 0.95f,
            "Forest grass sides");

        // Beach Theme - Sandy colors
        CreateMaterial("Track/Track_Beach_Sand",
            new Color(0.9f, 0.85f, 0.7f), 0f, 0.95f,
            "Beach sand floor");

        CreateMaterial("Track/Track_Beach_Boardwalk",
            new Color(0.6f, 0.45f, 0.3f), 0f, 0.8f,
            "Wooden boardwalk");

        // Space Theme - Sci-fi metals
        CreateMaterial("Track/Track_Space_Floor",
            new Color(0.2f, 0.25f, 0.3f), 0.7f, 0.3f,
            "Space station metal floor");

        CreateEmissiveMaterial("Track/Track_Space_Glow",
            new Color(0.1f, 0.1f, 0.15f), 
            new Color(0f, 0.5f, 1f), 1.5f,
            "Space station glowing strips");
    }

    #endregion

    #region Obstacle Materials

    [MenuItem("Tools/Escape Train Run/Materials/Create Obstacle Materials")]
    public static void CreateObstacleMaterials()
    {
        Debug.Log("\n🚧 Creating Obstacle Materials...");

        // Jump Obstacles - Yellow/Orange warning colors
        CreateMaterial("Obstacles/Obstacle_Jump_Main",
            new Color(1f, 0.8f, 0.2f), 0f, 0.6f,
            "Jump obstacle - warning yellow");

        CreateMaterial("Obstacles/Obstacle_Jump_Stripe",
            new Color(0.1f, 0.1f, 0.1f), 0f, 0.5f,
            "Jump obstacle - black stripes");

        // Slide Obstacles - Red danger colors
        CreateMaterial("Obstacles/Obstacle_Slide_Main",
            new Color(0.9f, 0.2f, 0.2f), 0.3f, 0.5f,
            "Slide obstacle - danger red");

        CreateMaterial("Obstacles/Obstacle_Slide_Accent",
            new Color(0.8f, 0.8f, 0.8f), 0.5f, 0.3f,
            "Slide obstacle - metal accent");

        // Full Block Obstacles - Dark imposing
        CreateMaterial("Obstacles/Obstacle_Block_Main",
            new Color(0.3f, 0.3f, 0.35f), 0.4f, 0.4f,
            "Block obstacle - dark metal");

        CreateEmissiveMaterial("Obstacles/Obstacle_Block_Warning",
            new Color(0.2f, 0.2f, 0.2f),
            new Color(1f, 0f, 0f), 2f,
            "Block obstacle - red warning light");

        // Side Obstacles
        CreateMaterial("Obstacles/Obstacle_Side_Left",
            new Color(0.2f, 0.6f, 0.9f), 0.2f, 0.5f,
            "Side obstacle - blue");

        CreateMaterial("Obstacles/Obstacle_Side_Right",
            new Color(0.9f, 0.4f, 0.2f), 0.2f, 0.5f,
            "Side obstacle - orange");

        // Train Car
        CreateMaterial("Obstacles/Obstacle_Train_Body",
            new Color(0.15f, 0.4f, 0.15f), 0.3f, 0.5f,
            "Train car - dark green");

        CreateMaterial("Obstacles/Obstacle_Train_Accent",
            new Color(0.9f, 0.9f, 0.7f), 0f, 0.7f,
            "Train car - cream accent");
    }

    #endregion

    #region Collectible Materials

    [MenuItem("Tools/Escape Train Run/Materials/Create Collectible Materials")]
    public static void CreateCollectibleMaterials()
    {
        Debug.Log("\n💰 Creating Collectible Materials...");

        // Coins - Metallic gold
        CreateMaterial("Collectibles/Coin_Gold",
            new Color(1f, 0.84f, 0f), 1f, 0.2f,
            "Gold coin - main material");

        CreateMaterial("Collectibles/Coin_Silver",
            new Color(0.85f, 0.85f, 0.9f), 1f, 0.15f,
            "Silver coin variant");

        CreateMaterial("Collectibles/Coin_Bronze",
            new Color(0.8f, 0.5f, 0.2f), 0.9f, 0.25f,
            "Bronze coin variant");

        // PowerUps - Glowing effects
        CreateEmissiveMaterial("Collectibles/PowerUp_Magnet",
            new Color(0.6f, 0.2f, 0.8f),
            new Color(0.8f, 0.3f, 1f), 2f,
            "Magnet powerup - purple glow");

        CreateEmissiveMaterial("Collectibles/PowerUp_Shield",
            new Color(0.2f, 0.6f, 0.9f),
            new Color(0.3f, 0.7f, 1f), 2f,
            "Shield powerup - blue glow");

        CreateEmissiveMaterial("Collectibles/PowerUp_SpeedBoost",
            new Color(0.9f, 0.5f, 0.1f),
            new Color(1f, 0.6f, 0.2f), 2.5f,
            "Speed boost powerup - orange glow");

        CreateEmissiveMaterial("Collectibles/PowerUp_CoinMultiplier",
            new Color(0.9f, 0.8f, 0.2f),
            new Color(1f, 0.9f, 0.3f), 2f,
            "Coin multiplier - gold glow");

        CreateEmissiveMaterial("Collectibles/PowerUp_SlowTime",
            new Color(0.2f, 0.8f, 0.6f),
            new Color(0.3f, 1f, 0.7f), 1.5f,
            "Slow time powerup - cyan glow");

        // Mystery Box
        CreateMaterial("Collectibles/MysteryBox_Main",
            new Color(0.6f, 0.3f, 0.8f), 0.3f, 0.5f,
            "Mystery box - purple");

        CreateEmissiveMaterial("Collectibles/MysteryBox_Glow",
            new Color(0.4f, 0.2f, 0.6f),
            new Color(0.8f, 0.5f, 1f), 1.5f,
            "Mystery box - glow effect");
    }

    #endregion

    #region Character Materials

    [MenuItem("Tools/Escape Train Run/Materials/Create Character Materials")]
    public static void CreateCharacterMaterials()
    {
        Debug.Log("\n🏃 Creating Character Materials...");

        // Default Runner
        CreateMaterial("Characters/Char_Default_Skin",
            new Color(0.9f, 0.75f, 0.65f), 0f, 0.8f,
            "Default character skin tone");

        CreateMaterial("Characters/Char_Default_Outfit",
            new Color(0.2f, 0.5f, 0.9f), 0f, 0.6f,
            "Default character - blue outfit");

        CreateMaterial("Characters/Char_Default_Shoes",
            new Color(0.9f, 0.3f, 0.2f), 0f, 0.5f,
            "Default character - red shoes");

        // Speed Runner
        CreateMaterial("Characters/Char_Speed_Outfit",
            new Color(0.9f, 0.4f, 0.1f), 0.2f, 0.4f,
            "Speed character - orange sporty");

        // Ninja
        CreateMaterial("Characters/Char_Ninja_Outfit",
            new Color(0.1f, 0.1f, 0.15f), 0f, 0.7f,
            "Ninja character - black");

        CreateMaterial("Characters/Char_Ninja_Accent",
            new Color(0.8f, 0.1f, 0.1f), 0f, 0.5f,
            "Ninja character - red accent");

        // Robot
        CreateMaterial("Characters/Char_Robot_Body",
            new Color(0.7f, 0.7f, 0.75f), 0.8f, 0.2f,
            "Robot character - chrome body");

        CreateEmissiveMaterial("Characters/Char_Robot_Lights",
            new Color(0.2f, 0.2f, 0.3f),
            new Color(0f, 0.8f, 1f), 3f,
            "Robot character - cyan lights");

        // Astronaut
        CreateMaterial("Characters/Char_Astro_Suit",
            new Color(0.95f, 0.95f, 0.95f), 0f, 0.5f,
            "Astronaut - white suit");

        CreateMaterial("Characters/Char_Astro_Visor",
            new Color(0.3f, 0.3f, 0.4f), 0.9f, 0.1f,
            "Astronaut - reflective visor");

        // Pirate
        CreateMaterial("Characters/Char_Pirate_Outfit",
            new Color(0.5f, 0.25f, 0.1f), 0f, 0.7f,
            "Pirate - brown leather");

        CreateMaterial("Characters/Char_Pirate_Accent",
            new Color(0.8f, 0.7f, 0.3f), 0.6f, 0.3f,
            "Pirate - gold accents");
    }

    #endregion

    #region Environment Materials

    [MenuItem("Tools/Escape Train Run/Materials/Create Environment Materials")]
    public static void CreateEnvironmentMaterials()
    {
        Debug.Log("\n🌆 Creating Environment Materials...");

        // Skybox gradient colors (for procedural skybox)
        CreateMaterial("Environment/Sky_Day_Top",
            new Color(0.4f, 0.7f, 1f), 0f, 1f,
            "Day sky - top blue");

        CreateMaterial("Environment/Sky_Day_Horizon",
            new Color(0.8f, 0.9f, 1f), 0f, 1f,
            "Day sky - horizon light");

        CreateMaterial("Environment/Sky_Sunset_Top",
            new Color(0.3f, 0.2f, 0.5f), 0f, 1f,
            "Sunset sky - purple top");

        CreateMaterial("Environment/Sky_Sunset_Horizon",
            new Color(1f, 0.5f, 0.3f), 0f, 1f,
            "Sunset sky - orange horizon");

        // Buildings (background)
        CreateMaterial("Environment/Building_Glass",
            new Color(0.5f, 0.6f, 0.7f), 0.9f, 0.1f,
            "Building glass windows");

        CreateMaterial("Environment/Building_Concrete",
            new Color(0.6f, 0.58f, 0.55f), 0f, 0.85f,
            "Building concrete");

        CreateMaterial("Environment/Building_Brick",
            new Color(0.6f, 0.35f, 0.25f), 0f, 0.9f,
            "Building brick");

        // Trees/Nature
        CreateMaterial("Environment/Tree_Leaves",
            new Color(0.3f, 0.6f, 0.2f), 0f, 0.95f,
            "Tree leaves - green");

        CreateMaterial("Environment/Tree_Trunk",
            new Color(0.4f, 0.25f, 0.15f), 0f, 0.9f,
            "Tree trunk - brown bark");

        // Water
        CreateTransparentMaterial("Environment/Water_Surface",
            new Color(0.2f, 0.5f, 0.8f, 0.6f), 0.3f, 0.2f,
            "Water surface - semi-transparent");
    }

    #endregion

    #region UI Effect Materials

    [MenuItem("Tools/Escape Train Run/Materials/Create UI Effect Materials")]
    public static void CreateUIEffectMaterials()
    {
        Debug.Log("\n✨ Creating UI Effect Materials...");

        // Particle effects
        CreateParticleMaterial("UI/Particle_Sparkle",
            new Color(1f, 0.95f, 0.7f),
            "Sparkle particles for coins");

        CreateParticleMaterial("UI/Particle_Trail",
            new Color(0.5f, 0.8f, 1f),
            "Player trail effect");

        CreateParticleMaterial("UI/Particle_Dust",
            new Color(0.7f, 0.6f, 0.5f),
            "Running dust particles");

        CreateParticleMaterial("UI/Particle_PowerUp",
            new Color(1f, 1f, 1f),
            "PowerUp activation burst");

        // UI Glow
        CreateEmissiveMaterial("UI/Glow_Button",
            new Color(0.2f, 0.4f, 0.8f),
            new Color(0.3f, 0.6f, 1f), 1f,
            "Button glow effect");

        CreateEmissiveMaterial("UI/Glow_Coin",
            new Color(0.9f, 0.7f, 0.2f),
            new Color(1f, 0.85f, 0.3f), 1.5f,
            "Coin UI glow");
    }

    #endregion

    #region Material Creation Helpers

    private static void CreateMaterial(string path, Color color, float metallic, float smoothness, string description)
    {
        string fullPath = $"{materialsPath}/{path}.mat";
        
        if (AssetDatabase.LoadAssetAtPath<Material>(fullPath) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {path}");
            return;
        }

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
        {
            mat = new Material(Shader.Find("Standard"));
        }

        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color); // For Standard shader
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", 1f - smoothness);
        mat.SetFloat("_Glossiness", 1f - smoothness); // For Standard shader

        // Ensure directory exists
        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            CreateFolderRecursive(directory);
        }

        AssetDatabase.CreateAsset(mat, fullPath);
        Debug.Log($"  ✅ Created: {path}");
    }

    private static void CreateEmissiveMaterial(string path, Color baseColor, Color emissionColor, float emissionIntensity, string description)
    {
        string fullPath = $"{materialsPath}/{path}.mat";
        
        if (AssetDatabase.LoadAssetAtPath<Material>(fullPath) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {path}");
            return;
        }

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
        {
            mat = new Material(Shader.Find("Standard"));
        }

        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_Color", baseColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            CreateFolderRecursive(directory);
        }

        AssetDatabase.CreateAsset(mat, fullPath);
        Debug.Log($"  ✅ Created: {path} (emissive)");
    }

    private static void CreateTransparentMaterial(string path, Color color, float metallic, float smoothness, string description)
    {
        string fullPath = $"{materialsPath}/{path}.mat";
        
        if (AssetDatabase.LoadAssetAtPath<Material>(fullPath) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {path}");
            return;
        }

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
        {
            mat = new Material(Shader.Find("Standard"));
        }

        // Set to transparent mode
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0); // Alpha
        mat.SetFloat("_Mode", 3); // Standard shader transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", 1f - smoothness);

        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            CreateFolderRecursive(directory);
        }

        AssetDatabase.CreateAsset(mat, fullPath);
        Debug.Log($"  ✅ Created: {path} (transparent)");
    }

    private static void CreateParticleMaterial(string path, Color color, string description)
    {
        string fullPath = $"{materialsPath}/{path}.mat";
        
        if (AssetDatabase.LoadAssetAtPath<Material>(fullPath) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {path}");
            return;
        }

        // Try to find particle shader
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Particles/Standard Unlit");
        }
        if (particleShader == null)
        {
            particleShader = Shader.Find("Particles/Alpha Blended");
        }
        if (particleShader == null)
        {
            particleShader = Shader.Find("Standard");
        }

        Material mat = new Material(particleShader);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetColor("_TintColor", color);

        string directory = Path.GetDirectoryName(fullPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            CreateFolderRecursive(directory);
        }

        AssetDatabase.CreateAsset(mat, fullPath);
        Debug.Log($"  ✅ Created: {path} (particle)");
    }

    private static void CreateFolderRecursive(string path)
    {
        path = path.Replace("\\", "/");
        string[] folders = path.Split('/');
        string currentPath = folders[0];

        for (int i = 1; i < folders.Length; i++)
        {
            string newPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(newPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = newPath;
        }
    }

    #endregion
}
