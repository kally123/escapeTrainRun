using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create visual effect prefabs.
/// Run from menu: Tools > Escape Train Run > Create Visual Effects
/// </summary>
public class VisualEffectsCreator : Editor
{
    [MenuItem("Tools/Escape Train Run/Create Visual Effects")]
    public static void CreateAllVisualEffects()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("✨ CREATING VISUAL EFFECTS");
        Debug.Log("═══════════════════════════════════════════════════════════");

        EnsureDirectories();
        CreateParticleEffects();
        CreateTrailEffects();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("\n✅ All visual effects created!");
        EditorUtility.DisplayDialog("Visual Effects Created", 
            "All visual effects have been created in Assets/Prefabs/Effects/", "OK");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            "Assets/Prefabs",
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

    private static void CreateParticleEffects()
    {
        Debug.Log("\n🎆 Creating Particle Effects...");

        // Coin Collect Burst
        CreateBurstEffect("CoinCollectVFX", new Color(1f, 0.85f, 0.2f), 20, 0.4f);

        // Power-up Activation
        CreateBurstEffect("PowerUpVFX", new Color(0.3f, 0.7f, 1f), 30, 0.6f);

        // Impact/Crash Effect
        CreateBurstEffect("ImpactVFX", new Color(1f, 0.3f, 0.2f), 40, 0.5f);

        // Jump Landing Dust
        CreateBurstEffect("LandingDustVFX", new Color(0.6f, 0.55f, 0.4f), 15, 0.3f);

        // Speed Lines
        CreateSpeedLinesEffect();
    }

    private static void CreateBurstEffect(string name, Color color, int particleCount, float duration)
    {
        string path = $"Assets/Prefabs/Effects/{name}.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {name}");
            return;
        }

        var effectObj = new GameObject(name);
        var ps = effectObj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.duration = duration;
        main.loop = false;
        main.startLifetime = duration;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = color;
        main.maxParticles = particleCount * 2;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.gravityModifier = 1f;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });

        // Shape
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // Color over lifetime
        var colorLife = ps.colorOverLifetime;
        colorLife.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(color, 0f), 
                new GradientColorKey(color, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorLife.color = gradient;

        // Size over lifetime
        var sizeLife = ps.sizeOverLifetime;
        sizeLife.enabled = true;
        sizeLife.size = new ParticleSystem.MinMaxCurve(1f, 
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        PrefabUtility.SaveAsPrefabAsset(effectObj, path);
        Object.DestroyImmediate(effectObj);

        Debug.Log($"  ✅ Created: {name}");
    }

    private static void CreateSpeedLinesEffect()
    {
        string name = "SpeedLinesVFX";
        string path = $"Assets/Prefabs/Effects/{name}.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {name}");
            return;
        }

        var effectObj = new GameObject(name);
        var ps = effectObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.3f;
        main.startSpeed = 30f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = new Color(1f, 1f, 1f, 0.3f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5f;
        shape.radius = 0.5f;

        // Make particles stretch in direction of movement
        var renderer = effectObj.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 5f;
        }

        PrefabUtility.SaveAsPrefabAsset(effectObj, path);
        Object.DestroyImmediate(effectObj);

        Debug.Log($"  ✅ Created: {name}");
    }

    private static void CreateTrailEffects()
    {
        Debug.Log("\n🌈 Creating Trail Effects...");

        CreateTrailEffect("PlayerTrail", new Color(0.3f, 0.6f, 1f, 0.6f));
        CreateTrailEffect("SpeedTrail", new Color(1f, 0.5f, 0.2f, 0.7f));
    }

    private static void CreateTrailEffect(string name, Color color)
    {
        string path = $"Assets/Prefabs/Effects/{name}.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log($"  ⏭️ Skipped (exists): {name}");
            return;
        }

        var trailObj = new GameObject(name);
        var tr = trailObj.AddComponent<TrailRenderer>();
        
        tr.time = 0.5f;
        tr.startWidth = 0.3f;
        tr.endWidth = 0f;
        tr.startColor = color;
        tr.endColor = new Color(color.r, color.g, color.b, 0f);
        tr.numCornerVertices = 3;
        tr.numCapVertices = 3;

        // Create simple material
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        tr.material = mat;

        PrefabUtility.SaveAsPrefabAsset(trailObj, path);
        Object.DestroyImmediate(trailObj);

        Debug.Log($"  ✅ Created: {name}");
    }
}
