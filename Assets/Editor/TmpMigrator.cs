// PongPongPong 项目专用：Legacy UI Text → TextMeshPro 一键迁移
//
// 因为 Unity 在每一步之间会触发 domain reload，所以分 3 个菜单点；按顺序点完即可。
// 工具会自检状态，重复点击是安全的。
//
//   Tools/TMP/1. 导入 TMP Essentials
//   Tools/TMP/2. 创建中文 Font Asset 并设为默认
//   Tools/TMP/3. 迁移场景 Legacy Text 到 TMP
//
// 字体来源：Assets/Fonts/msyh.ttc（仓库里已放好，微软雅黑）。

using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class TmpMigrator
{
    const string FontTtcPath        = "Assets/Fonts/msyh.ttc";
    const string GeneratedAssetPath = "Assets/Fonts/MSYH SDF.asset";

    // ---------- Step 1 ----------
    [MenuItem("Tools/TMP/1. 导入 TMP Essentials")]
    public static void Step1_ImportEssentials()
    {
        if (Directory.Exists("Assets/TextMesh Pro"))
        {
            EditorUtility.DisplayDialog("已完成",
                "TMP Essentials 已经导入过，跳过此步，直接点 [2. 创建中文 Font Asset]。", "好");
            return;
        }

        string pkg = FindEssentialsPackage();
        if (pkg == null)
        {
            EditorUtility.DisplayDialog("找不到资源包",
                "在 Packages 目录下没找到 TMP Essential Resources.unitypackage。\n" +
                "请手动菜单 Window > TextMeshPro > Import TMP Essential Resources。", "好");
            return;
        }

        AssetDatabase.ImportPackage(pkg, false);
        Debug.Log($"[TmpMigrator] 已触发导入：{pkg}\n等编辑器编译完再点 [2. 创建中文 Font Asset]。");
    }

    static string FindEssentialsPackage()
    {
        string[] candidates =
        {
            "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage",
            "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage",
        };
        foreach (var p in candidates)
            if (File.Exists(p)) return p;
        return null;
    }

    // ---------- Step 2 ----------
    [MenuItem("Tools/TMP/2. 创建中文 Font Asset 并设为默认")]
    public static void Step2_CreateChineseFont()
    {
        if (!Directory.Exists("Assets/TextMesh Pro"))
        {
            EditorUtility.DisplayDialog("先做第 1 步",
                "TMP Essentials 还没导入，请先点 [1. 导入 TMP Essentials]。", "好");
            return;
        }

        if (!File.Exists(FontTtcPath))
        {
            EditorUtility.DisplayDialog("缺字体文件",
                $"找不到 {FontTtcPath}。请检查仓库里这个文件是否被忽略。", "好");
            return;
        }

        var srcFont = AssetDatabase.LoadAssetAtPath<Font>(FontTtcPath);
        if (srcFont == null)
        {
            EditorUtility.DisplayDialog("字体加载失败",
                $"Unity 尚未把 {FontTtcPath} 识别为 Font，等编辑器扫描完再试。", "好");
            return;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GeneratedAssetPath);
        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(
                srcFont,
                90,                                  // samplingPointSize
                9,                                   // atlasPadding
                GlyphRenderMode.SDFAA,
                1024, 1024,
                AtlasPopulationMode.Dynamic,
                true);
            AssetDatabase.CreateAsset(fontAsset, GeneratedAssetPath);
            Debug.Log($"[TmpMigrator] 已创建 Font Asset：{GeneratedAssetPath}（Dynamic SDF, 1024x1024）。");
        }
        else
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fontAsset);
            Debug.Log($"[TmpMigrator] 复用现有 Font Asset：{GeneratedAssetPath}。");
        }

        var settings = TMP_Settings.instance;
        if (settings == null)
        {
            EditorUtility.DisplayDialog("找不到 TMP Settings",
                "Assets/TextMesh Pro/Resources/TMP Settings.asset 不存在，重做第 1 步。", "好");
            return;
        }

        var so = new SerializedObject(settings);
        so.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("[TmpMigrator] 已把中文 Font Asset 设为 TMP 默认字体。现在可以点 [3. 迁移场景]。");
    }

    // ---------- Step 3 ----------
    [MenuItem("Tools/TMP/3. 迁移场景 Legacy Text 到 TMP")]
    public static void Step3_MigrateScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("迁移失败", "请先打开要迁移的场景。", "好");
            return;
        }

        TMP_FontAsset defaultFont = null;
        try { defaultFont = TMP_Settings.defaultFontAsset; } catch { }
        if (defaultFont == null)
        {
            bool go = EditorUtility.DisplayDialog(
                "TMP 默认字体未设置",
                "TMP Settings 里没有 Default Font Asset，迁移后中文会显示为方框。\n建议先点 [2]。是否仍然继续？",
                "继续", "取消");
            if (!go) return;
        }

        var roots = scene.GetRootGameObjects();
        int replaced = 0;

        foreach (var root in roots)
            foreach (var t in root.GetComponentsInChildren<Text>(true))
            {
                ReplaceTextWithTmp(t);
                replaced++;
            }

        foreach (var root in roots)
        {
            foreach (var hud in root.GetComponentsInChildren<HudController>(true))
                RewireHud(hud, root);
            foreach (var es in root.GetComponentsInChildren<EndScreen>(true))
                RewireEndScreen(es, root);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[TmpMigrator] 完成：替换 {replaced} 个 Text 组件，已保存场景 {scene.name}。");
    }

    static void ReplaceTextWithTmp(Text t)
    {
        var go = t.gameObject;

        string txt        = t.text;
        float  fontSize   = t.fontSize;
        Color  color      = t.color;
        var    style      = t.fontStyle;
        var    anchor     = t.alignment;
        bool   raycast    = t.raycastTarget;
        bool   richText   = t.supportRichText;
        bool   maskable   = t.maskable;
        bool   bestFit    = t.resizeTextForBestFit;
        int    minSize    = t.resizeTextMinSize;
        int    maxSize    = t.resizeTextMaxSize;

        Object.DestroyImmediate(t, true);

        var cr = go.GetComponent<CanvasRenderer>();
        if (cr != null) Object.DestroyImmediate(cr, true);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = txt;
        tmp.fontSize      = fontSize;
        tmp.color         = color;
        tmp.fontStyle     = MapFontStyle(style);
        tmp.alignment     = MapAlignment(anchor);
        tmp.raycastTarget = raycast;
        tmp.richText      = richText;
        tmp.maskable      = maskable;

        if (bestFit)
        {
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minSize;
            tmp.fontSizeMax = maxSize;
        }

        EditorUtility.SetDirty(go);
    }

    static void RewireHud(HudController hud, GameObject root)
    {
        var so = new SerializedObject(hud);

        var stateGo = FindDescendant(root, "StateLabel");
        if (stateGo != null)
            so.FindProperty("stateLabel").objectReferenceValue = stateGo.GetComponent<TMP_Text>();

        var hudGo = FindDescendant(root, "HUD");
        if (hudGo != null)
        {
            foreach (var candidate in hudGo.GetComponentsInChildren<TMP_Text>(true))
            {
                if (candidate.gameObject.name == "StateLabel") continue;
                candidate.gameObject.name = "AILabel";
                so.FindProperty("aiLabel").objectReferenceValue = candidate;
                break;
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void RewireEndScreen(EndScreen es, GameObject root)
    {
        var so = new SerializedObject(es);
        var resultGo = FindDescendant(root, "ResultText");
        if (resultGo != null)
            so.FindProperty("resultText").objectReferenceValue = resultGo.GetComponent<TMP_Text>();
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject FindDescendant(GameObject root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform child in root.transform)
        {
            var found = FindDescendant(child.gameObject, name);
            if (found != null) return found;
        }
        return null;
    }

    static FontStyles MapFontStyle(FontStyle style)
    {
        var result = FontStyles.Normal;
        if (style == FontStyle.Bold || style == FontStyle.BoldAndItalic)
            result |= FontStyles.Bold;
        if (style == FontStyle.Italic || style == FontStyle.BoldAndItalic)
            result |= FontStyles.Italic;
        return result;
    }

    static TextAlignmentOptions MapAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:    return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:  return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:   return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:   return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:  return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft:    return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:  return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:   return TextAlignmentOptions.BottomRight;
            default:                       return TextAlignmentOptions.Center;
        }
    }
}
