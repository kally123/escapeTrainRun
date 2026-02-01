using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create all game prefabs.
/// Run from menu: Tools > Escape Train Run > Create All Prefabs
/// </summary>
public class PrefabCreator : Editor
{
    [MenuItem("Tools/Escape Train Run/Create All Prefabs")]
    public static void CreateAllPrefabs()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("📦 CREATING ALL PREFABS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        EnsureDirectories();
        CreateTrackPrefabs();
        CreateObstaclePrefabs();
        CreateCollectiblePrefabs();
        CreatePlayerPrefab();
        CreateEffectPrefabs();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("\n✅ All prefabs created!");
        EditorUtility.DisplayDialog("Prefabs Created", 
            "All game prefabs have been created in Assets/Prefabs/", "OK");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            "Assets/Prefabs",
            "Assets/Prefabs/Environment",
            "Assets/Prefabs/Obstacles",
            "Assets/Prefabs/Collectibles",
            "Assets/Prefabs/Player",
            "Assets/Prefabs/Effects"
        };

        foreach (var dir in dirs)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                var name = Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    #region Track Prefabs

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Track Prefabs")]
    public static void CreateTrackPrefabs()
    {
        Debug.Log("\n🛤️ Creating Track Prefabs...");

        // Track Segment
        var segment = new GameObject("TrackSegment");
        
        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(segment.transform);
        floor.transform.localPosition = new Vector3(0, -0.5f, 25);
        floor.transform.localScale = new Vector3(6, 1, 50);

        // Left Rail
        var leftRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftRail.name = "LeftRail";
        leftRail.transform.SetParent(segment.transform);
        leftRail.transform.localPosition = new Vector3(-3, 0.25f, 25);
        leftRail.transform.localScale = new Vector3(0.2f, 0.5f, 50);

        // Right Rail
        var rightRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightRail.name = "RightRail";
        rightRail.transform.SetParent(segment.transform);
        rightRail.transform.localPosition = new Vector3(3, 0.25f, 25);
        rightRail.transform.localScale = new Vector3(0.2f, 0.5f, 50);

        // Spawn points for collectibles/obstacles
        var spawnPoints = new GameObject("SpawnPoints");
        spawnPoints.transform.SetParent(segment.transform);

        segment.AddComponent<EscapeTrainRun.Environment.TrackSegment>();

        SavePrefab(segment, "Assets/Prefabs/Environment/TrackSegment.prefab");
    }

    #endregion

    #region Obstacle Prefabs

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Obstacle Prefabs")]
    public static void CreateObstaclePrefabs()
    {
        Debug.Log("\n🚧 Creating Obstacle Prefabs...");

        // Jump Obstacle (low barrier)
        var jumpObs = new GameObject("JumpObstacle");
        var jumpMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        jumpMesh.name = "Mesh";
        jumpMesh.transform.SetParent(jumpObs.transform);
        jumpMesh.transform.localPosition = new Vector3(0, 0.5f, 0);
        jumpMesh.transform.localScale = new Vector3(1.8f, 1f, 0.5f);
        jumpObs.tag = "Obstacle";
        jumpObs.AddComponent<EscapeTrainRun.Obstacles.GroundObstacle>();
        SavePrefab(jumpObs, "Assets/Prefabs/Obstacles/JumpObstacle.prefab");

        // Slide Obstacle (overhead barrier)
        var slideObs = new GameObject("SlideObstacle");
        var slideMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slideMesh.name = "Mesh";
        slideMesh.transform.SetParent(slideObs.transform);
        slideMesh.transform.localPosition = new Vector3(0, 1.5f, 0);
        slideMesh.transform.localScale = new Vector3(1.8f, 1f, 0.5f);
        slideObs.tag = "Obstacle";
        slideObs.AddComponent<EscapeTrainRun.Obstacles.Obstacle>();
        SavePrefab(slideObs, "Assets/Prefabs/Obstacles/SlideObstacle.prefab");

        // Full Block Obstacle
        var blockObs = new GameObject("FullBlockObstacle");
        var blockMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blockMesh.name = "Mesh";
        blockMesh.transform.SetParent(blockObs.transform);
        blockMesh.transform.localPosition = new Vector3(0, 1f, 0);
        blockMesh.transform.localScale = new Vector3(1.8f, 2f, 0.5f);
        blockObs.tag = "Obstacle";
        blockObs.AddComponent<EscapeTrainRun.Obstacles.Obstacle>();
        SavePrefab(blockObs, "Assets/Prefabs/Obstacles/FullBlockObstacle.prefab");

        // Train Obstacle
        var trainObs = new GameObject("TrainObstacle");
        var trainBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trainBody.name = "Body";
        trainBody.transform.SetParent(trainObs.transform);
        trainBody.transform.localPosition = new Vector3(0, 1.5f, 0);
        trainBody.transform.localScale = new Vector3(2f, 3f, 8f);
        trainObs.tag = "Obstacle";
        trainObs.AddComponent<EscapeTrainRun.Obstacles.TrainObstacle>();
        SavePrefab(trainObs, "Assets/Prefabs/Obstacles/TrainObstacle.prefab");

        // Moving Obstacle
        var movingObs = new GameObject("MovingObstacle");
        var moveMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        moveMesh.name = "Mesh";
        moveMesh.transform.SetParent(movingObs.transform);
        moveMesh.transform.localPosition = new Vector3(0, 1f, 0);
        moveMesh.transform.localScale = new Vector3(1.5f, 2f, 0.5f);
        movingObs.tag = "Obstacle";
        movingObs.AddComponent<EscapeTrainRun.Obstacles.MovingObstacle>();
        SavePrefab(movingObs, "Assets/Prefabs/Obstacles/MovingObstacle.prefab");
    }

    #endregion

    #region Collectible Prefabs

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Collectible Prefabs")]
    public static void CreateCollectiblePrefabs()
    {
        Debug.Log("\n💰 Creating Collectible Prefabs...");

        // Coin
        var coin = new GameObject("Coin");
        var coinMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coinMesh.name = "Mesh";
        coinMesh.transform.SetParent(coin.transform);
        coinMesh.transform.localPosition = Vector3.zero;
        coinMesh.transform.localRotation = Quaternion.Euler(90, 0, 0);
        coinMesh.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        Object.DestroyImmediate(coinMesh.GetComponent<Collider>());
        
        var coinCollider = coin.AddComponent<SphereCollider>();
        coinCollider.radius = 0.3f;
        coinCollider.isTrigger = true;
        coin.tag = "Coin";
        coin.AddComponent<EscapeTrainRun.Collectibles.Coin>();
        SavePrefab(coin, "Assets/Prefabs/Collectibles/Coin.prefab");

        // PowerUp - Magnet
        CreatePowerUpPrefab("PowerUp_Magnet", new Color(0.8f, 0.2f, 1f));

        // PowerUp - Shield
        CreatePowerUpPrefab("PowerUp_Shield", new Color(0.2f, 0.6f, 1f));

        // PowerUp - SpeedBoost
        CreatePowerUpPrefab("PowerUp_SpeedBoost", new Color(1f, 0.5f, 0.1f));

        // PowerUp - CoinMultiplier
        CreatePowerUpPrefab("PowerUp_CoinMultiplier", new Color(1f, 0.85f, 0.2f));

        // PowerUp - SlowTime
        CreatePowerUpPrefab("PowerUp_SlowTime", new Color(0.2f, 0.9f, 0.6f));
    }

    private static void CreatePowerUpPrefab(string name, Color color)
    {
        var powerUp = new GameObject(name);
        var mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mesh.name = "Mesh";
        mesh.transform.SetParent(powerUp.transform);
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.localScale = Vector3.one * 0.6f;

        var renderer = mesh.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Glossiness", 0.8f);
            renderer.sharedMaterial = mat;
        }

        Object.DestroyImmediate(mesh.GetComponent<Collider>());
        
        var collider = powerUp.AddComponent<SphereCollider>();
        collider.radius = 0.4f;
        collider.isTrigger = true;
        powerUp.tag = "PowerUp";
        powerUp.AddComponent<EscapeTrainRun.Collectibles.PowerUp>();
        
        SavePrefab(powerUp, $"Assets/Prefabs/Collectibles/{name}.prefab");
    }

    #endregion

    #region Player Prefab

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Player Prefab")]
    public static void CreatePlayerPrefab()
    {
        Debug.Log("\n🏃 Creating Player Prefab...");

        var player = new GameObject("Player");
        player.tag = "Player";

        // Character Controller
        var cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2f;
        cc.radius = 0.3f;

        // Components
        player.AddComponent<EscapeTrainRun.Player.PlayerController>();
        player.AddComponent<EscapeTrainRun.Player.PlayerCollision>();
        player.AddComponent<EscapeTrainRun.Player.PlayerAnimation>();

        // Visual placeholder
        var model = new GameObject("Model");
        model.transform.SetParent(player.transform);
        
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Body";
        capsule.transform.SetParent(model.transform);
        capsule.transform.localPosition = new Vector3(0, 1, 0);
        Object.DestroyImmediate(capsule.GetComponent<Collider>());

        // Ground check
        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);

        SavePrefab(player, "Assets/Prefabs/Player/Player.prefab");
    }

    #endregion

    #region Effect Prefabs

    [MenuItem("Tools/Escape Train Run/Prefabs/Create Effect Prefabs")]
    public static void CreateEffectPrefabs()
    {
        Debug.Log("\n✨ Creating Effect Prefabs...");

        CreateParticleEffect("CoinCollectEffect", new Color(1f, 0.85f, 0.2f), 15, 0.5f);
        CreateParticleEffect("PowerUpActivateEffect", new Color(0.5f, 0.8f, 1f), 30, 1f);
        CreateParticleEffect("DustEffect", new Color(0.7f, 0.6f, 0.5f), 10, 0.3f);
        CreateParticleEffect("ImpactEffect", new Color(1f, 0.3f, 0.2f), 25, 0.5f);
        CreateParticleEffect("SparkleEffect", new Color(1f, 1f, 0.8f), 20, 0.6f);
    }

    private static void CreateParticleEffect(string name, Color color, int count, float lifetime)
    {
        var effect = new GameObject(name);
        var ps = effect.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = lifetime;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.startColor = color;
        main.maxParticles = count * 2;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)count)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        SavePrefab(effect, $"Assets/Prefabs/Effects/{name}.prefab");
    }

    #endregion

    #region Helpers

    private static void SavePrefab(GameObject obj, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {Path.GetFileName(path)}");
            Object.DestroyImmediate(obj);
            return;
        }

        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        Debug.Log($"  ✅ Created: {Path.GetFileName(path)}");
    }

    #endregion
}
