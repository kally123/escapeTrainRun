using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to automatically create all game prefabs.
/// Run from menu: Tools > Escape Train Run > Create All Prefabs
/// </summary>
public class PrefabCreator : Editor
{
    private static string prefabPath = "Assets/Prefabs";

    [MenuItem("Tools/Escape Train Run/Create All Prefabs")]
    public static void CreateAllPrefabs()
    {
        if (!EditorUtility.DisplayDialog("Create All Prefabs",
            "This will create all game prefabs in Assets/Prefabs/. Continue?", "Yes", "Cancel"))
        {
            return;
        }

        // Ensure folders exist
        EnsureFolderExists($"{prefabPath}/Environment");
        EnsureFolderExists($"{prefabPath}/Obstacles");
        EnsureFolderExists($"{prefabPath}/Collectibles");
        EnsureFolderExists($"{prefabPath}/Effects");
        EnsureFolderExists($"{prefabPath}/Player");
        EnsureFolderExists($"{prefabPath}/Managers");

        CreateTrackSegmentPrefab();
        CreateObstaclePrefabs();
        CreateCollectiblePrefabs();
        CreateEffectPrefabs();
        CreatePlayerPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ All Prefabs Created!");
        EditorUtility.DisplayDialog("Success", "All prefabs created successfully!\n\nCheck Assets/Prefabs/ folder.", "OK");
    }

    #region Track Segment

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Track Segment")]
    public static void CreateTrackSegmentPrefab()
    {
        var trackSegment = new GameObject("TrackSegment");

        // Add TrackSegment script
        AddComponentSafe<EscapeTrainRun.TrackSegment>(trackSegment);

        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(trackSegment.transform);
        floor.transform.localPosition = new Vector3(0, -0.25f, 25);
        floor.transform.localScale = new Vector3(7.5f, 0.5f, 50f);
        floor.layer = LayerMask.NameToLayer("Default"); // Change to Ground layer if created

        // Left Rail
        var leftRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftRail.name = "LeftRail";
        leftRail.transform.SetParent(trackSegment.transform);
        leftRail.transform.localPosition = new Vector3(-3.5f, 0.25f, 25);
        leftRail.transform.localScale = new Vector3(0.3f, 0.5f, 50f);
        SetColor(leftRail, new Color(0.3f, 0.3f, 0.35f));

        // Right Rail
        var rightRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightRail.name = "RightRail";
        rightRail.transform.SetParent(trackSegment.transform);
        rightRail.transform.localPosition = new Vector3(3.5f, 0.25f, 25);
        rightRail.transform.localScale = new Vector3(0.3f, 0.5f, 50f);
        SetColor(rightRail, new Color(0.3f, 0.3f, 0.35f));

        // Lane Dividers
        var laneDividers = new GameObject("LaneDividers");
        laneDividers.transform.SetParent(trackSegment.transform);

        CreateLaneDivider(laneDividers.transform, -1.25f);
        CreateLaneDivider(laneDividers.transform, 1.25f);

        // Spawn Points
        var spawnPoints = new GameObject("SpawnPoints");
        spawnPoints.transform.SetParent(trackSegment.transform);

        CreateSpawnPoint(spawnPoints.transform, "ObstacleSpawn1", new Vector3(0, 0, 15));
        CreateSpawnPoint(spawnPoints.transform, "ObstacleSpawn2", new Vector3(0, 0, 35));
        CreateSpawnPoint(spawnPoints.transform, "CoinSpawn1", new Vector3(0, 1, 10));
        CreateSpawnPoint(spawnPoints.transform, "CoinSpawn2", new Vector3(0, 1, 25));
        CreateSpawnPoint(spawnPoints.transform, "CoinSpawn3", new Vector3(0, 1, 40));
        CreateSpawnPoint(spawnPoints.transform, "PowerUpSpawn", new Vector3(0, 1.5f, 30));

        // Save as prefab
        SavePrefab(trackSegment, $"{prefabPath}/Environment/TrackSegment.prefab");
        DestroyImmediate(trackSegment);

        Debug.Log("✅ Track Segment prefab created");
    }

    private static void CreateLaneDivider(Transform parent, float xPos)
    {
        var divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        divider.name = $"LaneDivider_{(xPos < 0 ? "Left" : "Right")}";
        divider.transform.SetParent(parent);
        divider.transform.localPosition = new Vector3(xPos, 0.01f, 25);
        divider.transform.localScale = new Vector3(0.1f, 0.02f, 50f);
        SetColor(divider, new Color(1f, 1f, 1f, 0.3f));
        
        // Remove collider for visual-only element
        var col = divider.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
    }

    private static void CreateSpawnPoint(Transform parent, string name, Vector3 position)
    {
        var spawn = new GameObject(name);
        spawn.transform.SetParent(parent);
        spawn.transform.localPosition = position;
        
        // Add visual gizmo icon in editor
        var icon = spawn.AddComponent<SpawnPointGizmo>();
    }

    #endregion

    #region Obstacles

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Obstacles")]
    public static void CreateObstaclePrefabs()
    {
        // Jump Obstacle (low barrier)
        CreateJumpObstacle();
        
        // Slide Obstacle (overhead bar)
        CreateSlideObstacle();
        
        // Full Block (wall)
        CreateFullBlockObstacle();
        
        // Moving Obstacle
        CreateMovingObstacle();
        
        // Train Car Obstacle
        CreateTrainCarObstacle();

        Debug.Log("✅ Obstacle prefabs created");
    }

    private static void CreateJumpObstacle()
    {
        var obstacle = new GameObject("JumpObstacle");
        obstacle.tag = "Obstacle";

        AddComponentSafe<EscapeTrainRun.Obstacle>(obstacle);

        // Visual - barrier/crate
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(obstacle.transform);
        visual.transform.localPosition = new Vector3(0, 0.5f, 0);
        visual.transform.localScale = new Vector3(2f, 1f, 0.5f);
        SetColor(visual, new Color(0.8f, 0.3f, 0.2f));

        // Collider (trigger)
        var col = obstacle.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 0.5f, 0);
        col.size = new Vector3(2f, 1f, 0.5f);

        // Remove visual's collider
        DestroyImmediate(visual.GetComponent<Collider>());

        SavePrefab(obstacle, $"{prefabPath}/Obstacles/JumpObstacle.prefab");
        DestroyImmediate(obstacle);
    }

    private static void CreateSlideObstacle()
    {
        var obstacle = new GameObject("SlideObstacle");
        obstacle.tag = "Obstacle";

        AddComponentSafe<EscapeTrainRun.Obstacle>(obstacle);

        // Overhead bar
        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "Bar";
        bar.transform.SetParent(obstacle.transform);
        bar.transform.localPosition = new Vector3(0, 1.5f, 0);
        bar.transform.localScale = new Vector3(2.5f, 0.3f, 0.3f);
        SetColor(bar, new Color(0.6f, 0.6f, 0.1f));

        // Left Post
        var leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPost.name = "LeftPost";
        leftPost.transform.SetParent(obstacle.transform);
        leftPost.transform.localPosition = new Vector3(-1.1f, 0.75f, 0);
        leftPost.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
        SetColor(leftPost, new Color(0.5f, 0.5f, 0.1f));
        DestroyImmediate(leftPost.GetComponent<Collider>());

        // Right Post
        var rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPost.name = "RightPost";
        rightPost.transform.SetParent(obstacle.transform);
        rightPost.transform.localPosition = new Vector3(1.1f, 0.75f, 0);
        rightPost.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
        SetColor(rightPost, new Color(0.5f, 0.5f, 0.1f));
        DestroyImmediate(rightPost.GetComponent<Collider>());

        // Collider (trigger) - higher position
        var col = obstacle.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 1.5f, 0);
        col.size = new Vector3(2.5f, 1f, 0.5f);

        DestroyImmediate(bar.GetComponent<Collider>());

        SavePrefab(obstacle, $"{prefabPath}/Obstacles/SlideObstacle.prefab");
        DestroyImmediate(obstacle);
    }

    private static void CreateFullBlockObstacle()
    {
        var obstacle = new GameObject("FullBlockObstacle");
        obstacle.tag = "Obstacle";

        AddComponentSafe<EscapeTrainRun.Obstacle>(obstacle);

        // Wall
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(obstacle.transform);
        wall.transform.localPosition = new Vector3(0, 1.25f, 0);
        wall.transform.localScale = new Vector3(2f, 2.5f, 0.5f);
        SetColor(wall, new Color(0.4f, 0.2f, 0.2f));

        // Collider
        var col = obstacle.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 1.25f, 0);
        col.size = new Vector3(2f, 2.5f, 0.5f);

        DestroyImmediate(wall.GetComponent<Collider>());

        SavePrefab(obstacle, $"{prefabPath}/Obstacles/FullBlockObstacle.prefab");
        DestroyImmediate(obstacle);
    }

    private static void CreateMovingObstacle()
    {
        var obstacle = new GameObject("MovingObstacle");
        obstacle.tag = "Obstacle";

        var obstacleComp = AddComponentSafe<EscapeTrainRun.Obstacle>(obstacle);
        AddComponentSafe<EscapeTrainRun.MovingObstacle>(obstacle);

        // Visual - cart/trolley
        var cart = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cart.name = "Cart";
        cart.transform.SetParent(obstacle.transform);
        cart.transform.localPosition = new Vector3(0, 0.6f, 0);
        cart.transform.localScale = new Vector3(1.5f, 1.2f, 2f);
        SetColor(cart, new Color(0.3f, 0.5f, 0.7f));

        // Wheels
        for (int i = 0; i < 4; i++)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = $"Wheel_{i}";
            wheel.transform.SetParent(obstacle.transform);
            float x = (i % 2 == 0) ? -0.8f : 0.8f;
            float z = (i < 2) ? -0.7f : 0.7f;
            wheel.transform.localPosition = new Vector3(x, 0.2f, z);
            wheel.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            SetColor(wheel, new Color(0.2f, 0.2f, 0.2f));
            DestroyImmediate(wheel.GetComponent<Collider>());
        }

        // Collider
        var col = obstacle.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 0.6f, 0);
        col.size = new Vector3(1.5f, 1.2f, 2f);

        DestroyImmediate(cart.GetComponent<Collider>());

        SavePrefab(obstacle, $"{prefabPath}/Obstacles/MovingObstacle.prefab");
        DestroyImmediate(obstacle);
    }

    private static void CreateTrainCarObstacle()
    {
        var obstacle = new GameObject("TrainCarObstacle");
        obstacle.tag = "Obstacle";

        AddComponentSafe<EscapeTrainRun.Obstacle>(obstacle);

        // Train car body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(obstacle.transform);
        body.transform.localPosition = new Vector3(0, 1.5f, 0);
        body.transform.localScale = new Vector3(2.5f, 3f, 6f);
        SetColor(body, new Color(0.2f, 0.4f, 0.6f));

        // Roof
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(obstacle.transform);
        roof.transform.localPosition = new Vector3(0, 3.1f, 0);
        roof.transform.localScale = new Vector3(2.7f, 0.2f, 6.2f);
        SetColor(roof, new Color(0.15f, 0.3f, 0.5f));
        DestroyImmediate(roof.GetComponent<Collider>());

        // Windows
        for (int i = 0; i < 3; i++)
        {
            var window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = $"Window_{i}";
            window.transform.SetParent(obstacle.transform);
            window.transform.localPosition = new Vector3(1.26f, 1.8f, -2f + i * 2f);
            window.transform.localScale = new Vector3(0.05f, 0.8f, 0.8f);
            SetColor(window, new Color(0.6f, 0.8f, 1f, 0.5f));
            DestroyImmediate(window.GetComponent<Collider>());
        }

        // Collider
        var col = obstacle.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 1.5f, 0);
        col.size = new Vector3(2.5f, 3f, 6f);

        DestroyImmediate(body.GetComponent<Collider>());

        SavePrefab(obstacle, $"{prefabPath}/Obstacles/TrainCarObstacle.prefab");
        DestroyImmediate(obstacle);
    }

    #endregion

    #region Collectibles

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Collectibles")]
    public static void CreateCollectiblePrefabs()
    {
        CreateCoinPrefab();
        CreatePowerUpPrefab("Magnet", new Color(0.8f, 0.2f, 0.2f));
        CreatePowerUpPrefab("Shield", new Color(0.2f, 0.5f, 0.9f));
        CreatePowerUpPrefab("SpeedBoost", new Color(0.2f, 0.8f, 0.3f));
        CreatePowerUpPrefab("ScoreMultiplier", new Color(0.9f, 0.7f, 0.1f));
        CreatePowerUpPrefab("StarPower", new Color(0.9f, 0.9f, 0.2f));

        Debug.Log("✅ Collectible prefabs created");
    }

    private static void CreateCoinPrefab()
    {
        var coin = new GameObject("Coin");
        coin.tag = "Coin";

        AddComponentSafe<EscapeTrainRun.Coin>(coin);

        // Visual - cylinder as coin
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(coin.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        visual.transform.localRotation = Quaternion.Euler(90, 0, 0);
        SetColor(visual, new Color(1f, 0.85f, 0.2f));

        // Spin script
        AddComponentSafe<SpinObject>(coin);

        // Collider
        var col = coin.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.4f;

        DestroyImmediate(visual.GetComponent<Collider>());

        SavePrefab(coin, $"{prefabPath}/Collectibles/Coin.prefab");
        DestroyImmediate(coin);
    }

    private static void CreatePowerUpPrefab(string powerUpName, Color color)
    {
        var powerUp = new GameObject($"PowerUp_{powerUpName}");
        powerUp.tag = "PowerUp";

        AddComponentSafe<EscapeTrainRun.PowerUp>(powerUp);

        // Visual - glowing orb
        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "Orb";
        orb.transform.SetParent(powerUp.transform);
        orb.transform.localPosition = Vector3.zero;
        orb.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        SetColor(orb, color);

        // Inner glow
        var innerGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerGlow.name = "InnerGlow";
        innerGlow.transform.SetParent(powerUp.transform);
        innerGlow.transform.localPosition = Vector3.zero;
        innerGlow.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        SetColor(innerGlow, Color.white);
        DestroyImmediate(innerGlow.GetComponent<Collider>());

        // Icon indicator (cube for now, replace with actual icon)
        var icon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        icon.name = "Icon";
        icon.transform.SetParent(powerUp.transform);
        icon.transform.localPosition = new Vector3(0, 0, 0);
        icon.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        icon.transform.localRotation = Quaternion.Euler(45, 45, 0);
        SetColor(icon, Color.white);
        DestroyImmediate(icon.GetComponent<Collider>());

        // Float/bob animation
        AddComponentSafe<FloatObject>(powerUp);

        // Collider
        var col = powerUp.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;

        DestroyImmediate(orb.GetComponent<Collider>());

        SavePrefab(powerUp, $"{prefabPath}/Collectibles/PowerUp_{powerUpName}.prefab");
        DestroyImmediate(powerUp);
    }

    #endregion

    #region Effects

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Effects")]
    public static void CreateEffectPrefabs()
    {
        CreateCoinCollectEffect();
        CreatePowerUpCollectEffect();
        CreateShieldEffectPrefab();
        CreateSpeedTrailPrefab();
        CreateCrashEffectPrefab();

        Debug.Log("✅ Effect prefabs created");
    }

    private static void CreateCoinCollectEffect()
    {
        var effect = new GameObject("CoinCollectEffect");

        var ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.3f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.85f, 0.2f);
        main.gravityModifier = 0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // Auto-destroy
        effect.AddComponent<AutoDestroy>().lifetime = 1f;

        SavePrefab(effect, $"{prefabPath}/Effects/CoinCollectEffect.prefab");
        DestroyImmediate(effect);
    }

    private static void CreatePowerUpCollectEffect()
    {
        var effect = new GameObject("PowerUpCollectEffect");

        var ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.8f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.25f;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.cyan, Color.white);
        main.gravityModifier = -0.2f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        effect.AddComponent<AutoDestroy>().lifetime = 1.5f;

        SavePrefab(effect, $"{prefabPath}/Effects/PowerUpCollectEffect.prefab");
        DestroyImmediate(effect);
    }

    private static void CreateShieldEffectPrefab()
    {
        var effect = new GameObject("ShieldEffect");

        // Shield bubble visual
        var bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bubble.name = "Bubble";
        bubble.transform.SetParent(effect.transform);
        bubble.transform.localPosition = new Vector3(0, 1, 0);
        bubble.transform.localScale = new Vector3(2f, 2.5f, 2f);
        SetColor(bubble, new Color(0.3f, 0.6f, 1f, 0.3f));
        DestroyImmediate(bubble.GetComponent<Collider>());

        // Particle system for sparkles
        var sparkles = new GameObject("Sparkles");
        sparkles.transform.SetParent(effect.transform);
        sparkles.transform.localPosition = new Vector3(0, 1, 0);

        var ps = sparkles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = 0.5f;
        main.startSize = 0.1f;
        main.startColor = new Color(0.5f, 0.8f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = 10;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        SavePrefab(effect, $"{prefabPath}/Effects/ShieldEffect.prefab");
        DestroyImmediate(effect);
    }

    private static void CreateSpeedTrailPrefab()
    {
        var effect = new GameObject("SpeedTrail");

        var trail = effect.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.5f;
        trail.endWidth = 0f;
        trail.startColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        trail.endColor = new Color(0.2f, 0.8f, 1f, 0f);

        // Create simple material
        trail.material = new Material(Shader.Find("Sprites/Default"));

        SavePrefab(effect, $"{prefabPath}/Effects/SpeedTrail.prefab");
        DestroyImmediate(effect);
    }

    private static void CreateCrashEffectPrefab()
    {
        var effect = new GameObject("CrashEffect");

        var ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.5f, 0.2f);
        main.gravityModifier = 1f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.5f;

        effect.AddComponent<AutoDestroy>().lifetime = 2f;

        SavePrefab(effect, $"{prefabPath}/Effects/CrashEffect.prefab");
        DestroyImmediate(effect);
    }

    #endregion

    #region Player Prefab

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Player Prefab")]
    public static void CreatePlayerPrefab()
    {
        var player = new GameObject("Player");
        player.tag = "Player";

        // Character Controller
        var charController = player.AddComponent<CharacterController>();
        charController.height = 2f;
        charController.radius = 0.5f;
        charController.center = new Vector3(0, 1, 0);

        // Rigidbody
        var rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Capsule Collider
        var capsule = player.AddComponent<CapsuleCollider>();
        capsule.height = 2f;
        capsule.radius = 0.5f;
        capsule.center = new Vector3(0, 1, 0);
        capsule.isTrigger = true;

        // Scripts
        AddComponentSafe<EscapeTrainRun.PlayerController>(player);
        AddComponentSafe<EscapeTrainRun.PlayerMovement>(player);
        AddComponentSafe<EscapeTrainRun.PlayerCollision>(player);
        AddComponentSafe<EscapeTrainRun.PlayerAnimation>(player);

        // Model placeholder
        var model = new GameObject("Model");
        model.transform.SetParent(player.transform);
        model.transform.localPosition = Vector3.zero;

        // Placeholder visual
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(model.transform);
        body.transform.localPosition = new Vector3(0, 1, 0);
        body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        SetColor(body, new Color(0.3f, 0.5f, 0.8f));
        DestroyImmediate(body.GetComponent<Collider>());

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(model.transform);
        head.transform.localPosition = new Vector3(0, 2.2f, 0);
        head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        SetColor(head, new Color(0.9f, 0.75f, 0.65f));
        DestroyImmediate(head.GetComponent<Collider>());

        // Ground Check
        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);

        // Magnet Range
        var magnetRange = new GameObject("MagnetRange");
        magnetRange.transform.SetParent(player.transform);
        magnetRange.transform.localPosition = Vector3.zero;
        var magnetCol = magnetRange.AddComponent<SphereCollider>();
        magnetCol.radius = 5f;
        magnetCol.isTrigger = true;

        SavePrefab(player, $"{prefabPath}/Player/Player.prefab");
        DestroyImmediate(player);

        Debug.Log("✅ Player prefab created");
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

    private static void SavePrefab(GameObject obj, string path)
    {
        // Ensure parent folder exists
        string directory = System.IO.Path.GetDirectoryName(path);
        EnsureFolderExists(directory.Replace("\\", "/"));

        PrefabUtility.SaveAsPrefabAsset(obj, path);
    }

    private static T AddComponentSafe<T>(GameObject go) where T : Component
    {
        var existing = go.GetComponent<T>();
        if (existing != null) return existing;
        return go.AddComponent<T>();
    }

    private static void SetColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            
            if (color.a < 1f)
            {
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            
            renderer.material = mat;
        }
    }

    #endregion
}

/// <summary>
/// Simple spawn point visualization for editor
/// </summary>
public class SpawnPointGizmo : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = name.Contains("Obstacle") ? Color.red : 
                       name.Contains("Coin") ? Color.yellow : 
                       name.Contains("PowerUp") ? Color.cyan : Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
    }
#endif
}

/// <summary>
/// Simple spin animation for coins
/// </summary>
public class SpinObject : MonoBehaviour
{
    public float speed = 180f;
    
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}

/// <summary>
/// Float/bob animation for power-ups
/// </summary>
public class FloatObject : MonoBehaviour
{
    public float amplitude = 0.3f;
    public float frequency = 2f;
    private Vector3 startPos;
    
    void Start()
    {
        startPos = transform.localPosition;
    }
    
    void Update()
    {
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * frequency) * amplitude;
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }
}

/// <summary>
/// Auto-destroy effect after lifetime
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 1f;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
