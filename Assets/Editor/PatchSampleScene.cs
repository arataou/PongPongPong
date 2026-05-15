// SampleScene 一键改造工具
//
// 菜单: Tools/PongPongPong/Patch SampleScene
//
// 把扩展第二批需要的 GameObject 挂到现有 SampleScene 上:
// - ArenaLoader (含 Background / ObstacleParent 引用)
// - PlayerRoster (自动找 PlayerController / Player2Controller / Player3Controller)
// - DemoSceneOverlay (挂 AIBall.prefab 引用)
// - ImpactParticle (新建 ParticleSystem,接到 GameFeel.impactParticle)
//
// 工具是幂等的:多次运行不会重复创建已存在的 GameObject。

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PatchSampleScene
{
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string AIPrefabPath = "Assets/Prefabs/AIBall.prefab";

    [MenuItem("Tools/PongPongPong/Patch SampleScene")]
    public static void Patch()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("失败", $"打不开 {ScenePath}", "好");
            return;
        }

        int created = 0;
        int wired = 0;

        // ----- 1. Background (SpriteRenderer) -----
        var background = FindByName("Background") ?? CreateBackground(out _);
        if (background.GetComponent<SpriteRenderer>() == null)
        {
            background.AddComponent<SpriteRenderer>();
        }
        var bgSr = background.GetComponent<SpriteRenderer>();
        bgSr.sortingOrder = -10;

        // ----- 2. ObstacleParent -----
        var obstacleParent = FindByName("ObstacleParent");
        if (obstacleParent == null)
        {
            obstacleParent = new GameObject("ObstacleParent");
            created++;
        }

        // ----- 3. ImpactParticle -----
        var impactGo = FindByName("ImpactParticle");
        if (impactGo == null)
        {
            impactGo = new GameObject("ImpactParticle", typeof(ParticleSystem));
            ConfigureImpactParticle(impactGo.GetComponent<ParticleSystem>());
            created++;
        }

        // ----- 4. ArenaLoader -----
        var arenaLoaderGo = FindByName("ArenaLoader");
        if (arenaLoaderGo == null)
        {
            arenaLoaderGo = new GameObject("ArenaLoader");
            created++;
        }
        var arenaLoader = arenaLoaderGo.GetComponent<ArenaLoader>() ?? arenaLoaderGo.AddComponent<ArenaLoader>();

        var arenaBounds = Object.FindFirstObjectByType<ArenaBounds>();
        var alSo = new SerializedObject(arenaLoader);
        if (arenaBounds != null) alSo.FindProperty("arenaBounds").objectReferenceValue = arenaBounds;
        alSo.FindProperty("backgroundRenderer").objectReferenceValue = bgSr;
        alSo.FindProperty("obstacleParent").objectReferenceValue     = obstacleParent.transform;
        alSo.ApplyModifiedPropertiesWithoutUndo();
        wired++;

        // ----- 5. PlayerRoster -----
        var rosterGo = FindByName("PlayerRoster");
        if (rosterGo == null)
        {
            rosterGo = new GameObject("PlayerRoster");
            created++;
        }
        var roster = rosterGo.GetComponent<PlayerRoster>() ?? rosterGo.AddComponent<PlayerRoster>();

        var p1 = Object.FindFirstObjectByType<PlayerController>();
        var p2 = Object.FindFirstObjectByType<Player2Controller>();
        var p3 = Object.FindFirstObjectByType<Player3Controller>();

        var rSo = new SerializedObject(roster);
        if (p1 != null) rSo.FindProperty("player1").objectReferenceValue = p1.gameObject;
        if (p2 != null) rSo.FindProperty("player2").objectReferenceValue = p2.gameObject;
        if (p3 != null) rSo.FindProperty("player3").objectReferenceValue = p3.gameObject;
        rSo.ApplyModifiedPropertiesWithoutUndo();
        wired++;

        // ----- 6. DemoSceneOverlay -----
        var demoGo = FindByName("DemoSceneOverlay");
        if (demoGo == null)
        {
            demoGo = new GameObject("DemoSceneOverlay");
            created++;
        }
        var demo = demoGo.GetComponent<DemoSceneOverlay>() ?? demoGo.AddComponent<DemoSceneOverlay>();

        var aiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AIPrefabPath);
        var dSo = new SerializedObject(demo);
        if (aiPrefab != null) dSo.FindProperty("aiPrefab").objectReferenceValue = aiPrefab;
        dSo.ApplyModifiedPropertiesWithoutUndo();
        wired++;

        // ----- 7. GameFeel.impactParticle -----
        var gameFeel = Object.FindFirstObjectByType<GameFeel>();
        if (gameFeel != null)
        {
            var gfSo = new SerializedObject(gameFeel);
            var prop = gfSo.FindProperty("impactParticle");
            if (prop != null)
            {
                prop.objectReferenceValue = impactGo.GetComponent<ParticleSystem>();
                gfSo.ApplyModifiedPropertiesWithoutUndo();
                wired++;
            }
        }

        // ----- 8. BallTrail (挂到 P1/P2/P3 球上) -----
        foreach (var p in new PlayerBase[] { p1, p2, p3 })
        {
            if (p == null) continue;
            if (p.GetComponent<BallTrail>() == null)
            {
                p.gameObject.AddComponent<BallTrail>();
                created++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorUtility.DisplayDialog(
            "完成",
            $"SampleScene 已 patch:\n" +
            $"新建 GameObject: {created}\n" +
            $"接线: {wired} 个组件\n\n" +
            $"P1/P2/P3 找到: P1={p1!=null} P2={p2!=null} P3={p3!=null}\n" +
            $"AIBall.prefab: {(aiPrefab!=null?"OK":"未找到 — DemoSceneOverlay.aiPrefab 留空")}\n\n" +
            $"如果 P3 没找到,在 Hierarchy 加一个 Player3 球(挂 Player3Controller),再点一次本菜单。",
            "好");

        Debug.Log($"[PatchSampleScene] 完成 → SampleScene 保存,创建 {created} 个 GO,接线 {wired} 个组件");
    }

    static GameObject FindByName(string name)
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindDescendant(root, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject FindDescendant(GameObject root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform child in root.transform)
        {
            var f = FindDescendant(child.gameObject, name);
            if (f != null) return f;
        }
        return null;
    }

    static GameObject CreateBackground(out SpriteRenderer sr)
    {
        var go = new GameObject("Background", typeof(SpriteRenderer));
        sr = go.GetComponent<SpriteRenderer>();
        sr.sortingOrder = -10;
        return go;
    }

    static void ConfigureImpactParticle(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 4f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.9f, 0.6f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;

        var emit = ps.emission;
        emit.enabled = true;
        emit.rateOverTime = 0f; // 只用 Emit() 触发

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.5f, 0.2f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = grad;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var curve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 0f));
        size.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 5;
    }
}
