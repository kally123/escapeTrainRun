using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create visual effect prefabs with particle systems.
/// Run from menu: Tools > Escape Train Run > Create Visual Effects
/// </summary>
public class VisualEffectsCreator : Editor
{
    private static string prefabsPath = "Assets/Prefabs/Effects";

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Create All Effects")]
    public static void CreateAllEffects()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("✨ CREATING VISUAL EFFECTS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        EnsureDirectory();
        
        CreateCoinCollectEffect();
        CreatePowerUpActivateEffect();
        CreatePlayerTrailEffect();
        CreateDustRunningEffect();
        CreateJumpLandEffect();
        CreateShieldEffect();
        CreateSpeedLinesEffect();
        CreateGameOverEffect();
        CreateSparkleEffect();
        CreateMagnetFieldEffect();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("\n✅ All visual effects created!");
        EditorUtility.DisplayDialog("Visual Effects Created", 
            "All particle effect prefabs have been created in:\n" +
            "Assets/Prefabs/Effects/\n\n" +
            "Effects created:\n" +
            "• Coin Collect\n" +
            "• PowerUp Activate\n" +
            "• Player Trail\n" +
            "• Dust Running\n" +
            "• Jump Land\n" +
            "• Shield\n" +
            "• Speed Lines\n" +
            "• Game Over\n" +
            "• Sparkle\n" +
            "• Magnet Field", "OK");
    }

    private static void EnsureDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(prefabsPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Effects");
        }
    }

    #region Individual Effects

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Coin Collect")]
    public static void CreateCoinCollectEffect()
    {
        Debug.Log("\n💰 Creating Coin Collect Effect...");

        if (PrefabExists("CoinCollectEffect")) return;

        GameObject effectObj = new GameObject("CoinCollectEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.85f, 0.2f);
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.7f, 0f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1, 1, 0));

        SavePrefab(effectObj, "CoinCollectEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/PowerUp Activate")]
    public static void CreatePowerUpActivateEffect()
    {
        Debug.Log("\n⚡ Creating PowerUp Activate Effect...");

        if (PrefabExists("PowerUpActivateEffect")) return;

        GameObject effectObj = new GameObject("PowerUpActivateEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 5f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.5f, 0.8f, 1f);
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30),
            new ParticleSystem.Burst(0.1f, 20)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Add ring burst as child
        GameObject ringObj = new GameObject("Ring");
        ringObj.transform.SetParent(effectObj.transform);
        ringObj.transform.localPosition = Vector3.zero;
        
        var ringPs = ringObj.AddComponent<ParticleSystem>();
        var ringMain = ringPs.main;
        ringMain.duration = 0.5f;
        ringMain.loop = false;
        ringMain.startLifetime = 0.5f;
        ringMain.startSpeed = 0;
        ringMain.startSize = 0.5f;
        ringMain.maxParticles = 1;

        var ringEmission = ringPs.emission;
        ringEmission.rateOverTime = 0;
        ringEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

        var ringSize = ringPs.sizeOverLifetime;
        ringSize.enabled = true;
        ringSize.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.5f, 1, 3f));

        SavePrefab(effectObj, "PowerUpActivateEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Player Trail")]
    public static void CreatePlayerTrailEffect()
    {
        Debug.Log("\n💨 Creating Player Trail Effect...");

        if (PrefabExists("PlayerTrailEffect")) return;

        GameObject effectObj = new GameObject("PlayerTrailEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 0.3f;
        main.startSpeed = 0f;
        main.startSize = 0.4f;
        main.startColor = new Color(0.5f, 0.7f, 1f, 0.5f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 30;

        var shape = ps.shape;
        shape.enabled = false;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.5f, 0.8f, 1f), 0f),
                new GradientColorKey(new Color(0.3f, 0.5f, 0.8f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.6f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1, 1, 0.3f));

        SavePrefab(effectObj, "PlayerTrailEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Dust Running")]
    public static void CreateDustRunningEffect()
    {
        Debug.Log("\n🏃 Creating Dust Running Effect...");

        if (PrefabExists("DustRunningEffect")) return;

        GameObject effectObj = new GameObject("DustRunningEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = new Color(0.7f, 0.6f, 0.5f, 0.4f);
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.2f;

        var emission = ps.emission;
        emission.rateOverTime = 15;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.6f, 0.55f, 0.5f), 0f),
                new GradientColorKey(new Color(0.5f, 0.45f, 0.4f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.4f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        SavePrefab(effectObj, "DustRunningEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Jump Land")]
    public static void CreateJumpLandEffect()
    {
        Debug.Log("\n🦶 Creating Jump Land Effect...");

        if (PrefabExists("JumpLandEffect")) return;

        GameObject effectObj = new GameObject("JumpLandEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.7f, 0.65f, 0.6f, 0.6f);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 20)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 60f;
        shape.radius = 0.3f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);

        SavePrefab(effectObj, "JumpLandEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Shield")]
    public static void CreateShieldEffect()
    {
        Debug.Log("\n🛡️ Creating Shield Effect...");

        if (PrefabExists("ShieldEffect")) return;

        GameObject effectObj = new GameObject("ShieldEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.5f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        shape.radiusThickness = 0;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.3f, 0.8f, 1f), 0f),
                new GradientColorKey(new Color(0.5f, 0.9f, 1f), 0.5f),
                new GradientColorKey(new Color(0.2f, 0.6f, 0.9f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        SavePrefab(effectObj, "ShieldEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Speed Lines")]
    public static void CreateSpeedLinesEffect()
    {
        Debug.Log("\n⚡ Creating Speed Lines Effect...");

        if (PrefabExists("SpeedLinesEffect")) return;

        GameObject effectObj = new GameObject("SpeedLinesEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 0.3f;
        main.startSpeed = 30f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
        main.startColor = new Color(1f, 1f, 1f, 0.5f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 100;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5f;
        shape.radius = 2f;
        shape.length = 10f;
        shape.rotation = new Vector3(0f, 180f, 0f);

        var renderer = effectObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 5f;
        renderer.velocityScale = 0.5f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.5f, 1, 1f));

        SavePrefab(effectObj, "SpeedLinesEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Game Over")]
    public static void CreateGameOverEffect()
    {
        Debug.Log("\n💥 Creating Game Over Effect...");

        if (PrefabExists("GameOverEffect")) return;

        GameObject effectObj = new GameObject("GameOverEffect");
        
        // Main burst
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 1f;
        main.startSpeed = 8f;
        main.startSize = 0.5f;
        main.startColor = new Color(1f, 0.3f, 0.2f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.gravityModifier = 0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 50)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.5f, 0.2f), 0f),
                new GradientColorKey(new Color(0.3f, 0.1f, 0.1f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Add screen flash child
        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.SetParent(effectObj.transform);
        flashObj.transform.localPosition = Vector3.zero;

        SavePrefab(effectObj, "GameOverEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Sparkle")]
    public static void CreateSparkleEffect()
    {
        Debug.Log("\n✨ Creating Sparkle Effect...");

        if (PrefabExists("SparkleEffect")) return;

        GameObject effectObj = new GameObject("SparkleEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = new Color(1f, 0.95f, 0.7f);
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 10;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.2f, 1f);
        sizeCurve.AddKey(0.5f, 0.3f);
        sizeCurve.AddKey(0.8f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        SavePrefab(effectObj, "SparkleEffect");
    }

    [MenuItem("Tools/Escape Train Run/Create Visual Effects/Magnet Field")]
    public static void CreateMagnetFieldEffect()
    {
        Debug.Log("\n🧲 Creating Magnet Field Effect...");

        if (PrefabExists("MagnetFieldEffect")) return;

        GameObject effectObj = new GameObject("MagnetFieldEffect");
        
        var ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = -2f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.8f, 0.3f, 1f, 0.7f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 40;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 3f;
        shape.radiusThickness = 0;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.5f, 0.2f, 0.8f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 1f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1f, 1, 0.2f));

        SavePrefab(effectObj, "MagnetFieldEffect");
    }

    #endregion

    #region Helper Methods

    private static bool PrefabExists(string name)
    {
        string path = $"{prefabsPath}/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {name}");
            return true;
        }
        return false;
    }

    private static void SavePrefab(GameObject obj, string name)
    {
        EnsureDirectory();
        string path = $"{prefabsPath}/{name}.prefab";
        
        // Remove any existing
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        Debug.Log($"  ✅ Created: {name}");
    }

    #endregion
}
