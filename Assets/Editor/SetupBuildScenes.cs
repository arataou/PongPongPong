// 一键把 MainMenu / SampleScene 加进 Build Settings,MainMenu 排第一。
//
// 菜单: Tools/PongPongPong/Setup Build Scenes

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SetupBuildScenes
{
    static readonly string[] OrderedScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/SampleScene.unity",
    };

    [MenuItem("Tools/PongPongPong/Setup Build Scenes")]
    public static void Setup()
    {
        var list = new List<EditorBuildSettingsScene>();
        var missing = new List<string>();

        foreach (var path in OrderedScenes)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                missing.Add(path);
                continue;
            }
            list.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = list.ToArray();

        string msg = $"Build Settings 已更新:\n";
        for (int i = 0; i < list.Count; i++)
            msg += $"  [{i}] {list[i].path}\n";
        if (missing.Count > 0)
        {
            msg += "\n⚠️ 未找到(跳过):\n";
            foreach (var p in missing) msg += $"  · {p}\n";
            msg += "\n如果 MainMenu 缺失,先跑 Tools/PongPongPong/Build MainMenu Scene。";
        }

        EditorUtility.DisplayDialog("Build Scenes", msg, "好");
        Debug.Log($"[SetupBuildScenes] 完成,列表 {list.Count} 个场景。");
    }
}
