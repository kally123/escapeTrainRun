using UnityEngine;
using UnityEditor;
using EscapeTrainRun.Characters;

/// <summary>
/// Editor script to automatically create all ScriptableObject assets.
/// Run from menu: Tools > Escape Train Run > Create All ScriptableObjects
/// </summary>
public class ScriptableObjectCreator : Editor
{
    private static string soPath = "Assets/ScriptableObjects";

    [MenuItem("Tools/Escape Train Run/Create All ScriptableObjects")]
    public static void CreateAllScriptableObjects()
    {
        if (!EditorUtility.DisplayDialog("Create ScriptableObjects",
            "This will create all ScriptableObject assets. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        // Ensure folders exist
        EnsureFolderExists($"{soPath}/Characters");
        EnsureFolderExists($"{soPath}/Achievements");
        EnsureFolderExists($"{soPath}/Config");
        EnsureFolderExists($"{soPath}/PowerUps");

        CreateCharacterAssets();
        CreateAchievementAssets();
        CreateConfigAssets();
        CreatePowerUpConfigAssets();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ All ScriptableObjects Created!");
        EditorUtility.DisplayDialog("Success", "All ScriptableObjects created!\n\nCheck Assets/ScriptableObjects/ folder.", "OK");
    }

    #region Character Data

    [MenuItem("Tools/Escape Train Run/ScriptableObjects/Create Characters")]
    public static void CreateCharacterAssets()
    {
        EnsureFolderExists($"{soPath}/Characters");

        // Default Runner
        CreateCharacter(new CharacterConfig
        {
            id = "default_runner",
            name = "Runner",
            description = "The default runner. Fast and reliable!",
            rarity = CharacterRarity.Common,
            price = 0,
            isUnlockedByDefault = true,
            speedModifier = 1.0f,
            coinMultiplier = 1.0f,
            scoreMultiplier = 1.0f,
            magnetRangeBonus = 0f,
            ability = CharacterAbility.None,
            abilityDescription = "No special ability",
            themeColor = new Color(0.3f, 0.5f, 0.8f)
        });

        // Speed Demon
        CreateCharacter(new CharacterConfig
        {
            id = "speed_demon",
            name = "Speed Demon",
            description = "Blazing fast! Perfect for high scores.",
            rarity = CharacterRarity.Rare,
            price = 500,
            isUnlockedByDefault = false,
            speedModifier = 1.2f,
            coinMultiplier = 1.0f,
            scoreMultiplier = 1.1f,
            magnetRangeBonus = 0f,
            ability = CharacterAbility.SpeedBoost,
            abilityDescription = "Starts with a speed boost",
            themeColor = new Color(0.9f, 0.3f, 0.2f)
        });

        // Coin Master
        CreateCharacter(new CharacterConfig
        {
            id = "coin_master",
            name = "Coin Master",
            description = "Attracts coins like a magnet!",
            rarity = CharacterRarity.Rare,
            price = 750,
            isUnlockedByDefault = false,
            speedModifier = 1.0f,
            coinMultiplier = 1.25f,
            scoreMultiplier = 1.0f,
            magnetRangeBonus = 2f,
            ability = CharacterAbility.CoinMagnet,
            abilityDescription = "Extended coin magnet range",
            themeColor = new Color(1f, 0.8f, 0.2f)
        });

        // Lucky Star
        CreateCharacter(new CharacterConfig
        {
            id = "lucky_star",
            name = "Lucky Star",
            description = "Fortune favors the bold!",
            rarity = CharacterRarity.Epic,
            price = 1000,
            isUnlockedByDefault = false,
            speedModifier = 1.1f,
            coinMultiplier = 1.15f,
            scoreMultiplier = 1.2f,
            magnetRangeBonus = 1f,
            ability = CharacterAbility.LuckyCharm,
            abilityDescription = "More power-ups spawn nearby",
            themeColor = new Color(0.9f, 0.9f, 0.2f)
        });

        // Ninja
        CreateCharacter(new CharacterConfig
        {
            id = "ninja",
            name = "Shadow Ninja",
            description = "Swift and silent. Dodges with ease.",
            rarity = CharacterRarity.Epic,
            price = 1500,
            isUnlockedByDefault = false,
            speedModifier = 1.15f,
            coinMultiplier = 1.0f,
            scoreMultiplier = 1.15f,
            magnetRangeBonus = 0f,
            ability = CharacterAbility.DoubleJump,
            abilityDescription = "Can double jump",
            themeColor = new Color(0.2f, 0.2f, 0.3f)
        });

        // Pirate
        CreateCharacter(new CharacterConfig
        {
            id = "pirate",
            name = "Captain Coins",
            description = "Arr! Treasure hunter extraordinaire!",
            rarity = CharacterRarity.Rare,
            price = 800,
            isUnlockedByDefault = false,
            speedModifier = 0.95f,
            coinMultiplier = 1.5f,
            scoreMultiplier = 1.0f,
            magnetRangeBonus = 1.5f,
            ability = CharacterAbility.CoinMagnet,
            abilityDescription = "Coins are worth 50% more",
            themeColor = new Color(0.6f, 0.4f, 0.2f)
        });

        // Astronaut
        CreateCharacter(new CharacterConfig
        {
            id = "astronaut",
            name = "Astro Runner",
            description = "Defies gravity with every jump!",
            rarity = CharacterRarity.Legendary,
            price = 2500,
            isUnlockedByDefault = false,
            speedModifier = 1.1f,
            coinMultiplier = 1.1f,
            scoreMultiplier = 1.25f,
            magnetRangeBonus = 1f,
            ability = CharacterAbility.DoubleJump,
            abilityDescription = "Lower gravity, higher jumps",
            themeColor = new Color(0.8f, 0.8f, 0.9f)
        });

        // Wizard
        CreateCharacter(new CharacterConfig
        {
            id = "wizard",
            name = "Mystic Mage",
            description = "Magic flows through every step!",
            rarity = CharacterRarity.Legendary,
            price = 3000,
            isUnlockedByDefault = false,
            speedModifier = 1.05f,
            coinMultiplier = 1.2f,
            scoreMultiplier = 1.3f,
            magnetRangeBonus = 2f,
            ability = CharacterAbility.Shield,
            abilityDescription = "Starts with a magic shield",
            themeColor = new Color(0.5f, 0.2f, 0.8f)
        });

        // Robot
        CreateCharacter(new CharacterConfig
        {
            id = "robot",
            name = "Mech Runner",
            description = "Built for speed and durability!",
            rarity = CharacterRarity.Legendary,
            price = 5000,
            isUnlockedByDefault = false,
            speedModifier = 1.25f,
            coinMultiplier = 1.0f,
            scoreMultiplier = 1.2f,
            magnetRangeBonus = 3f,
            ability = CharacterAbility.Shield,
            abilityDescription = "Extra hit point from shield",
            themeColor = new Color(0.4f, 0.6f, 0.7f)
        });

        Debug.Log("✅ Character assets created");
    }

    private struct CharacterConfig
    {
        public string id;
        public string name;
        public string description;
        public CharacterRarity rarity;
        public int price;
        public bool isUnlockedByDefault;
        public float speedModifier;
        public float coinMultiplier;
        public float scoreMultiplier;
        public float magnetRangeBonus;
        public CharacterAbility ability;
        public string abilityDescription;
        public Color themeColor;
    }

    private static void CreateCharacter(CharacterConfig config)
    {
        string path = $"{soPath}/Characters/{config.id}.asset";
        
        // Check if already exists
        var existing = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
        if (existing != null)
        {
            Debug.Log($"Character {config.id} already exists, skipping.");
            return;
        }

        var character = ScriptableObject.CreateInstance<CharacterData>();
        
        // Use SerializedObject to set private fields
        SerializedObject so = new SerializedObject(character);
        so.FindProperty("characterId").stringValue = config.id;
        so.FindProperty("displayName").stringValue = config.name;
        so.FindProperty("description").stringValue = config.description;
        so.FindProperty("rarity").enumValueIndex = (int)config.rarity;
        so.FindProperty("unlockPrice").intValue = config.price;
        so.FindProperty("isUnlockedByDefault").boolValue = config.isUnlockedByDefault;
        so.FindProperty("speedModifier").floatValue = config.speedModifier;
        so.FindProperty("coinMultiplier").floatValue = config.coinMultiplier;
        so.FindProperty("scoreMultiplier").floatValue = config.scoreMultiplier;
        so.FindProperty("magnetRangeBonus").floatValue = config.magnetRangeBonus;
        so.FindProperty("primaryAbility").enumValueIndex = (int)config.ability;
        so.FindProperty("abilityDescription").stringValue = config.abilityDescription;
        so.FindProperty("themeColor").colorValue = config.themeColor;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(character, path);
    }

    #endregion

    #region Achievements

    [MenuItem("Tools/Escape Train Run/ScriptableObjects/Create Achievements")]
    public static void CreateAchievementAssets()
    {
        EnsureFolderExists($"{soPath}/Achievements");

        // Distance Achievements
        CreateAchievement("first_steps", "First Steps", "Run 100 meters", "distance", 100, 10);
        CreateAchievement("marathon_runner", "Marathon Runner", "Run 1,000 meters in one run", "distance", 1000, 50);
        CreateAchievement("ultra_runner", "Ultra Runner", "Run 5,000 meters in one run", "distance", 5000, 200);
        CreateAchievement("legendary_runner", "Legendary Runner", "Run 10,000 meters in one run", "distance", 10000, 500);

        // Coin Achievements
        CreateAchievement("coin_collector", "Coin Collector", "Collect 100 coins", "coins_total", 100, 25);
        CreateAchievement("coin_hoarder", "Coin Hoarder", "Collect 1,000 coins total", "coins_total", 1000, 100);
        CreateAchievement("treasure_hunter", "Treasure Hunter", "Collect 10,000 coins total", "coins_total", 10000, 500);
        CreateAchievement("coin_mogul", "Coin Mogul", "Collect 100,000 coins total", "coins_total", 100000, 2000);

        // Score Achievements
        CreateAchievement("high_scorer", "High Scorer", "Score 10,000 points", "score", 10000, 50);
        CreateAchievement("score_master", "Score Master", "Score 50,000 points", "score", 50000, 150);
        CreateAchievement("score_legend", "Score Legend", "Score 100,000 points", "score", 100000, 500);

        // Play Achievements
        CreateAchievement("dedicated_runner", "Dedicated Runner", "Play 10 games", "games_played", 10, 25);
        CreateAchievement("addicted", "Addicted", "Play 100 games", "games_played", 100, 200);
        CreateAchievement("veteran", "Veteran", "Play 500 games", "games_played", 500, 1000);

        // Power-Up Achievements
        CreateAchievement("power_user", "Power User", "Use 10 power-ups", "powerups_used", 10, 20);
        CreateAchievement("powered_up", "Powered Up", "Use 100 power-ups", "powerups_used", 100, 100);
        CreateAchievement("supercharged", "Supercharged", "Use 500 power-ups", "powerups_used", 500, 500);

        // Special Achievements
        CreateAchievement("no_coins", "Minimalist", "Complete a run without collecting coins", "special", 1, 100);
        CreateAchievement("close_call", "Close Call", "Narrowly avoid 10 obstacles", "close_calls", 10, 50);
        CreateAchievement("combo_master", "Combo Master", "Get a 50x combo", "max_combo", 50, 200);
        CreateAchievement("perfect_run", "Perfect Run", "Run 1000m without hitting anything", "perfect_distance", 1000, 300);

        // Character Achievements
        CreateAchievement("character_collector", "Character Collector", "Unlock 3 characters", "characters_unlocked", 3, 100);
        CreateAchievement("full_roster", "Full Roster", "Unlock all characters", "characters_unlocked", 9, 1000);

        Debug.Log("✅ Achievement assets created");
    }

    private static void CreateAchievement(string id, string name, string description, string statType, int target, int reward)
    {
        string path = $"{soPath}/Achievements/{id}.asset";
        
        var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (existing != null)
        {
            Debug.Log($"Achievement {id} already exists, skipping.");
            return;
        }

        // Create a simple achievement data holder since we may not have direct access to the class
        var achievement = ScriptableObject.CreateInstance<AchievementDataAsset>();
        
        SerializedObject so = new SerializedObject(achievement);
        so.FindProperty("achievementId").stringValue = id;
        so.FindProperty("achievementName").stringValue = name;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("statType").stringValue = statType;
        so.FindProperty("targetValue").intValue = target;
        so.FindProperty("coinReward").intValue = reward;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(achievement, path);
    }

    #endregion

    #region Config Assets

    [MenuItem("Tools/Escape Train Run/ScriptableObjects/Create Configs")]
    public static void CreateConfigAssets()
    {
        EnsureFolderExists($"{soPath}/Config");

        // Game Config
        CreateGameConfig();
        
        // Difficulty Config
        CreateDifficultyConfig();

        Debug.Log("✅ Config assets created");
    }

    private static void CreateGameConfig()
    {
        string path = $"{soPath}/Config/GameConfig.asset";
        
        var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (existing != null) return;

        var config = ScriptableObject.CreateInstance<GameConfigAsset>();
        
        SerializedObject so = new SerializedObject(config);
        so.FindProperty("gameName").stringValue = "Escape Train Run";
        so.FindProperty("gameVersion").stringValue = "1.0.0";
        so.FindProperty("startingLives").intValue = 1;
        so.FindProperty("baseSpeed").floatValue = 10f;
        so.FindProperty("maxSpeed").floatValue = 25f;
        so.FindProperty("laneWidth").floatValue = 2.5f;
        so.FindProperty("laneCount").intValue = 3;
        so.FindProperty("jumpHeight").floatValue = 2.5f;
        so.FindProperty("jumpDuration").floatValue = 0.5f;
        so.FindProperty("slideDuration").floatValue = 0.8f;
        so.FindProperty("coinBaseValue").intValue = 1;
        so.FindProperty("scorePerMeter").intValue = 1;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateDifficultyConfig()
    {
        string path = $"{soPath}/Config/DifficultyConfig.asset";
        
        var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (existing != null) return;

        var config = ScriptableObject.CreateInstance<DifficultyConfigAsset>();
        
        SerializedObject so = new SerializedObject(config);
        so.FindProperty("speedIncreaseRate").floatValue = 0.1f;
        so.FindProperty("obstacleFrequencyIncrease").floatValue = 0.05f;
        so.FindProperty("maxDifficultyDistance").floatValue = 5000f;
        so.FindProperty("easyModeSpeedMultiplier").floatValue = 0.7f;
        so.FindProperty("hardModeSpeedMultiplier").floatValue = 1.3f;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(config, path);
    }

    #endregion

    #region Power-Up Configs

    [MenuItem("Tools/Escape Train Run/ScriptableObjects/Create PowerUp Configs")]
    public static void CreatePowerUpConfigAssets()
    {
        EnsureFolderExists($"{soPath}/PowerUps");

        CreatePowerUpConfig("Magnet", 8f, 5f, new Color(0.8f, 0.2f, 0.2f), "Attracts nearby coins");
        CreatePowerUpConfig("Shield", 10f, 0f, new Color(0.2f, 0.5f, 0.9f), "Protects from one hit");
        CreatePowerUpConfig("SpeedBoost", 5f, 1.5f, new Color(0.2f, 0.8f, 0.3f), "Run faster temporarily");
        CreatePowerUpConfig("ScoreMultiplier", 10f, 2f, new Color(0.9f, 0.7f, 0.1f), "Double score points");
        CreatePowerUpConfig("StarPower", 8f, 0f, new Color(0.9f, 0.9f, 0.2f), "Invincible and destroy obstacles");

        Debug.Log("✅ Power-up config assets created");
    }

    private static void CreatePowerUpConfig(string name, float duration, float strength, Color color, string description)
    {
        string path = $"{soPath}/PowerUps/{name}Config.asset";
        
        var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (existing != null) return;

        var config = ScriptableObject.CreateInstance<PowerUpConfigAsset>();
        
        SerializedObject so = new SerializedObject(config);
        so.FindProperty("powerUpName").stringValue = name;
        so.FindProperty("duration").floatValue = duration;
        so.FindProperty("strength").floatValue = strength;
        so.FindProperty("iconColor").colorValue = color;
        so.FindProperty("description").stringValue = description;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(config, path);
    }

    #endregion

    #region Helper Methods

    private static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = $"{currentPath}/{folders[i]}";
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }

    #endregion
}

#region ScriptableObject Data Classes

/// <summary>
/// Simple achievement data asset for storing achievement configurations
/// </summary>
public class AchievementDataAsset : ScriptableObject
{
    public string achievementId;
    public string achievementName;
    public string description;
    public string statType;
    public int targetValue;
    public int coinReward;
    public Sprite icon;
    public bool isHidden;
}

/// <summary>
/// Game configuration asset
/// </summary>
public class GameConfigAsset : ScriptableObject
{
    [Header("Game Info")]
    public string gameName = "Escape Train Run";
    public string gameVersion = "1.0.0";

    [Header("Player Settings")]
    public int startingLives = 1;
    public float baseSpeed = 10f;
    public float maxSpeed = 25f;

    [Header("Lane Settings")]
    public float laneWidth = 2.5f;
    public int laneCount = 3;

    [Header("Movement")]
    public float jumpHeight = 2.5f;
    public float jumpDuration = 0.5f;
    public float slideDuration = 0.8f;
    public float laneSwitchSpeed = 10f;

    [Header("Scoring")]
    public int coinBaseValue = 1;
    public int scorePerMeter = 1;
    public int scorePerCoin = 10;
}

/// <summary>
/// Difficulty configuration asset
/// </summary>
public class DifficultyConfigAsset : ScriptableObject
{
    [Header("Difficulty Scaling")]
    public float speedIncreaseRate = 0.1f;
    public float obstacleFrequencyIncrease = 0.05f;
    public float maxDifficultyDistance = 5000f;

    [Header("Difficulty Modes")]
    public float easyModeSpeedMultiplier = 0.7f;
    public float normalModeSpeedMultiplier = 1.0f;
    public float hardModeSpeedMultiplier = 1.3f;

    [Header("Obstacle Spawning")]
    public float minObstacleDistance = 15f;
    public float maxObstacleDistance = 40f;
    public float obstacleDistanceReduction = 0.5f;
}

/// <summary>
/// Power-up configuration asset
/// </summary>
public class PowerUpConfigAsset : ScriptableObject
{
    public string powerUpName;
    public float duration = 10f;
    public float strength = 1f;
    public Color iconColor = Color.white;
    public string description;
    public Sprite icon;
    public AudioClip collectSound;
    public AudioClip activeSound;
    public GameObject effectPrefab;
}

#endregion
