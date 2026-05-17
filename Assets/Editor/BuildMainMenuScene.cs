// 一键生成 MainMenu 场景
//
// 菜单:Tools/PongPongPong/Build MainMenu Scene
//
// 行为:覆盖 Assets/Scenes/MainMenu.unity,创建完整可用主菜单(默认占位色块 + 中文文字 + 已接线脚本)。
// 美术后续替换 Image.sprite / AudioClip 即可。
//
// 如果想改默认布局/颜色/字号,改下面 LayoutConstants 区域 + 各 Build*Panel 方法。

using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildMainMenuScene
{
    const string ScenePath = "Assets/Scenes/MainMenu.unity";
    const string MapBtnPrefabPath = "Assets/Prefabs/MapButton.prefab";
    const string PrefabsDir = "Assets/Prefabs";

    // ===== Layout Constants(改这里就能调全局尺寸/颜色) =====
    static readonly Color CBackground = new Color(0.08f, 0.08f, 0.1f, 1f);
    static readonly Color CPanel      = new Color(0.12f, 0.13f, 0.18f, 0.95f);
    static readonly Color COverlay    = new Color(0f,    0f,    0f,    0.78f);
    static readonly Color CBtn        = new Color(0.22f, 0.25f, 0.35f, 1f);
    static readonly Color CBtnHover   = new Color(0.3f,  0.4f,  0.55f, 1f);
    static readonly Color CSelected   = new Color(0.3f,  0.7f,  1f,    1f);
    static readonly Color CUnselected = new Color(1f,    1f,    1f,    0.5f);
    static readonly Color CTitleAccent = new Color(0.95f, 0.85f, 0.35f, 1f);
    static readonly Color CWhite      = Color.white;
    static readonly Color CSubText    = new Color(0.85f, 0.85f, 0.9f, 1f);

    [MenuItem("Tools/PongPongPong/Build MainMenu Scene")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog(
            "生成 MainMenu 场景",
            $"会创建 / 覆盖 {ScenePath}\n如果之前手动改过 MainMenu,会被覆盖。\n继续?",
            "继续", "取消")) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ----- 相机 + EventSystem
        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = CBackground;
        cam.orthographic = true;
        camGo.tag = "MainCamera";

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // ----- Canvas
        var canvas = BuildCanvas();

        // ----- 顶层节点
        BuildBackgroundLayer(canvas.transform);
        BuildTitle(canvas.transform);

        var mainPanelData = BuildMainPanel(canvas.transform);
        var howToData     = BuildHowToPlayPanel(canvas.transform);
        var settingsData  = BuildSettingsPanel(canvas.transform);
        var historyData   = BuildHistoryPanel(canvas.transform);

        // ----- Map button prefab(MapSelectController 动态实例化用)
        var mapBtnPrefab = BuildMapButtonPrefab();

        // ----- _MainMenuRoot
        var rootGo = new GameObject("_MainMenuRoot");
        var menu     = rootGo.AddComponent<MainMenuController>();
        var modeSel  = rootGo.AddComponent<GameModeSelector>();
        var pcSel    = rootGo.AddComponent<PlayerCountSelector>();
        var diffSel  = rootGo.AddComponent<DifficultySelector>();
        var mapSel   = rootGo.AddComponent<MapSelectController>();
        rootGo.AddComponent<DemoModeRunner>();

        var audioGo = new GameObject("AudioManager");
        audioGo.AddComponent<AudioManager>();

        var settingsComp = settingsData.panel.AddComponent<SettingsPanel>();
        var historyComp  = historyData.panel.AddComponent<HistoryPanel>();
        var howToComp    = howToData.panel.AddComponent<HowToPlayPanel>();

        // ----- 接线
        WireMainMenu(menu, mainPanelData, howToData.panel, settingsData.panel, historyData.panel, pcSel, mapSel);
        WireModeSelector(modeSel, mainPanelData);
        WirePlayerCountSelector(pcSel, mainPanelData);
        WireDifficultySelector(diffSel, mainPanelData);
        WireMapSelector(mapSel, mainPanelData, mapBtnPrefab);
        WireSettingsPanel(settingsComp, settingsData, menu);
        WireHistoryPanel(historyComp, historyData, menu);
        WireHowToPlayPanel(howToComp, howToData, menu);

        // ----- 默认禁用 overlay 面板
        settingsData.panel.SetActive(false);
        historyData.panel.SetActive(false);
        howToData.panel.SetActive(false);

        // ----- 保存
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorUtility.DisplayDialog(
            "完成",
            $"MainMenu 场景已生成到 {ScenePath}\nMapButton Prefab: {MapBtnPrefabPath}\n\n下一步:\n1. File > Build Profiles → 把 MainMenu 拖到列表最上\n2. Play 测试\n\n(美术资源后续替换 Image.sprite / AudioClip 即可)",
            "好");

        Debug.Log($"[BuildMainMenuScene] 完成 → {ScenePath}");
    }

    // ====================================================================
    // 节点构造
    // ====================================================================

    static Canvas BuildCanvas()
    {
        var go = new GameObject("Canvas",
            typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight = 0.5f;
        return c;
    }

    static void BuildBackgroundLayer(Transform parent)
    {
        var go = CreateImage("Background", parent, CBackground);
        StretchAll((RectTransform)go.transform);
    }

    static void BuildTitle(Transform parent)
    {
        var go = CreateTmp("TitleText", parent, "碰 碰 球", 140, CTitleAccent);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -40);
        rt.sizeDelta = new Vector2(1400, 220);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    public class MainPanelData
    {
        public GameObject panel;
        public Button startBtn, howToBtn, settingsBtn, historyBtn, quitBtn;
        public Button pvpBtn, coopBtn;
        public TextMeshProUGUI modeLabel;
        public GameObject coopHintGroup;
        public Button p2Btn, p3Btn;
        public TextMeshProUGUI pcLabel;
        public GameObject diffRowGo;       // 整个难度行 (PvP 模式自动隐藏)
        public Button easyBtn, normalBtn, hardBtn;
        public TextMeshProUGUI diffLabel;
        public RectTransform mapListRoot;
        public Image mapPreviewImg;
        public TextMeshProUGUI mapPreviewName;
    }

    static MainPanelData BuildMainPanel(Transform parent)
    {
        var d = new MainPanelData();
        d.panel = CreateRect("MainPanel", parent);
        var prt = (RectTransform)d.panel.transform;
        StretchAll(prt);
        prt.offsetMin = new Vector2(0, 0);
        prt.offsetMax = new Vector2(0, -260); // 给标题留位置

        // ---- 左侧:模式 / 人数 / 难度
        var leftCol = CreateRect("LeftColumn", d.panel.transform);
        var lcr = (RectTransform)leftCol.transform;
        lcr.anchorMin = new Vector2(0, 0);
        lcr.anchorMax = new Vector2(0.5f, 1);
        lcr.offsetMin = new Vector2(120, 60);
        lcr.offsetMax = new Vector2(-30, -60);

        // ModeRow
        var modeRow = CreatePanelSection("ModeRow", leftCol.transform, 0, "游戏模式");
        d.modeLabel = CreateTmp("CurrentLabel", modeRow.transform, "模式: 对战", 24, CSubText).GetComponent<TextMeshProUGUI>();
        var modeLblRt = (RectTransform)d.modeLabel.transform;
        modeLblRt.anchorMin = new Vector2(0, 1); modeLblRt.anchorMax = new Vector2(1, 1);
        modeLblRt.pivot = new Vector2(0.5f, 1); modeLblRt.anchoredPosition = new Vector2(0, -50);
        modeLblRt.sizeDelta = new Vector2(0, 30);
        d.modeLabel.alignment = TextAlignmentOptions.Center;

        d.pvpBtn  = CreateButton("PvPButton",  modeRow.transform, "对战", 26, new Vector2(160, 60), new Vector2(0, 0));
        d.coopBtn = CreateButton("CoopButton", modeRow.transform, "合作", 26, new Vector2(160, 60), new Vector2(0, 0));
        AnchorBottomRow(d.pvpBtn.transform,  -100, 70);
        AnchorBottomRow(d.coopBtn.transform, +100, 70);

        d.coopHintGroup = CreateRect("CoopHint", modeRow.transform);
        var hintRt = (RectTransform)d.coopHintGroup.transform;
        hintRt.anchorMin = new Vector2(0, 0); hintRt.anchorMax = new Vector2(1, 0);
        hintRt.pivot = new Vector2(0.5f, 0); hintRt.anchoredPosition = new Vector2(0, 10);
        hintRt.sizeDelta = new Vector2(0, 30);
        var hint = CreateTmp("HintText", d.coopHintGroup.transform, "合作模式: 与队友抗 AI,变态难度记录最长存活", 18, CSubText);
        var hintTxtRt = (RectTransform)hint.transform;
        StretchAll(hintTxtRt);
        hint.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // PlayerCountRow
        var pcRow = CreatePanelSection("PlayerCountRow", leftCol.transform, 1, "玩家人数");
        d.pcLabel = CreateTmp("CurrentLabel", pcRow.transform, "当前: 2 人", 24, CSubText).GetComponent<TextMeshProUGUI>();
        var pcLblRt = (RectTransform)d.pcLabel.transform;
        pcLblRt.anchorMin = new Vector2(0, 1); pcLblRt.anchorMax = new Vector2(1, 1);
        pcLblRt.pivot = new Vector2(0.5f, 1); pcLblRt.anchoredPosition = new Vector2(0, -50);
        pcLblRt.sizeDelta = new Vector2(0, 30);
        d.pcLabel.alignment = TextAlignmentOptions.Center;

        d.p2Btn = CreateButton("TwoPlayerButton",   pcRow.transform, "2 人", 26, new Vector2(160, 60), new Vector2(0, 0));
        d.p3Btn = CreateButton("ThreePlayerButton", pcRow.transform, "3 人", 26, new Vector2(160, 60), new Vector2(0, 0));
        AnchorBottomRow(d.p2Btn.transform, -100, 40);
        AnchorBottomRow(d.p3Btn.transform, +100, 40);

        // DifficultyRow (PvP 模式会被 GameModeSelector 整行隐藏, 只有 COOP 才看得见)
        var diffRow = CreatePanelSection("DifficultyRow", leftCol.transform, 2, "AI 难度 (合作)");
        d.diffRowGo = diffRow;
        d.diffLabel = CreateTmp("CurrentLabel", diffRow.transform, "难度: 普通", 24, CSubText).GetComponent<TextMeshProUGUI>();
        var dlRt = (RectTransform)d.diffLabel.transform;
        dlRt.anchorMin = new Vector2(0, 1); dlRt.anchorMax = new Vector2(1, 1);
        dlRt.pivot = new Vector2(0.5f, 1); dlRt.anchoredPosition = new Vector2(0, -50);
        dlRt.sizeDelta = new Vector2(0, 30);
        d.diffLabel.alignment = TextAlignmentOptions.Center;

        // 按钮放到 panel 底部 +30 ~ +80 区域 (anchor bottom, pivot 0), 不再和 ProfilePreview 挤
        d.easyBtn   = CreateButton("EasyButton",   diffRow.transform, "菜",   22, new Vector2(120, 50), new Vector2(0, 0));
        d.normalBtn = CreateButton("NormalButton", diffRow.transform, "普通", 22, new Vector2(120, 50), new Vector2(0, 0));
        d.hardBtn   = CreateButton("HardButton",   diffRow.transform, "变态", 22, new Vector2(120, 50), new Vector2(0, 0));
        AnchorBottomRow(d.easyBtn.transform,   -160, 30);
        AnchorBottomRow(d.normalBtn.transform,    0, 30);
        AnchorBottomRow(d.hardBtn.transform,   +160, 30);

        // ---- 右侧:地图选择 + 按钮列
        var rightCol = CreateRect("RightColumn", d.panel.transform);
        var rcr = (RectTransform)rightCol.transform;
        rcr.anchorMin = new Vector2(0.5f, 0);
        rcr.anchorMax = new Vector2(1, 1);
        rcr.offsetMin = new Vector2(30, 60);
        rcr.offsetMax = new Vector2(-120, -60);

        // MapSelectArea
        var mapArea = CreatePanelSection("MapSelectArea", rightCol.transform, 0, "选择地图");
        var maRt = (RectTransform)mapArea.transform;
        // 加大占比 — overwrite section auto layout
        maRt.anchorMin = new Vector2(0, 0.45f);
        maRt.anchorMax = new Vector2(1, 1);
        maRt.offsetMin = new Vector2(0, 0);
        maRt.offsetMax = new Vector2(0, 0);

        var listRoot = CreateImage("ListRoot", mapArea.transform, new Color(0.06f, 0.07f, 0.1f, 0.7f));
        d.mapListRoot = (RectTransform)listRoot.transform;
        d.mapListRoot.anchorMin = new Vector2(0, 0);
        d.mapListRoot.anchorMax = new Vector2(1, 0.45f);
        d.mapListRoot.offsetMin = new Vector2(10, 10);
        d.mapListRoot.offsetMax = new Vector2(-10, -10);

        var hlg = listRoot.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        var previewBg = CreateImage("PreviewBg", mapArea.transform, new Color(0.06f, 0.07f, 0.1f, 0.7f));
        var pbRt = (RectTransform)previewBg.transform;
        pbRt.anchorMin = new Vector2(0, 0.5f);
        pbRt.anchorMax = new Vector2(1, 1);
        pbRt.offsetMin = new Vector2(10, 10);
        pbRt.offsetMax = new Vector2(-10, -10);

        d.mapPreviewImg = CreateImage("PreviewImage", previewBg.transform, CWhite).GetComponent<Image>();
        var piRt = (RectTransform)d.mapPreviewImg.transform;
        piRt.anchorMin = new Vector2(0, 0);
        piRt.anchorMax = new Vector2(0.4f, 1);
        piRt.offsetMin = new Vector2(10, 10);
        piRt.offsetMax = new Vector2(-10, -40);

        d.mapPreviewName = CreateTmp("PreviewName", previewBg.transform, "(未选择)", 32, CWhite).GetComponent<TextMeshProUGUI>();
        var pnRt = (RectTransform)d.mapPreviewName.transform;
        pnRt.anchorMin = new Vector2(0.4f, 0);
        pnRt.anchorMax = new Vector2(1, 1);
        pnRt.offsetMin = new Vector2(20, 10);
        pnRt.offsetMax = new Vector2(-10, -10);
        d.mapPreviewName.alignment = TextAlignmentOptions.Center;

        // BottomButtons
        var btnCol = CreateRect("BottomButtons", rightCol.transform);
        var bcRt = (RectTransform)btnCol.transform;
        bcRt.anchorMin = new Vector2(0, 0);
        bcRt.anchorMax = new Vector2(1, 0.4f);
        bcRt.offsetMin = new Vector2(0, 10);
        bcRt.offsetMax = new Vector2(0, -10);

        var vlg = btnCol.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.padding = new RectOffset(20, 20, 0, 0);
        vlg.childForceExpandHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        d.startBtn    = CreateMenuButton("StartButton",    btnCol.transform, "开始游戏",   28, CTitleAccent);
        d.howToBtn    = CreateMenuButton("HowToPlayButton", btnCol.transform, "操作说明",  24, CWhite);
        d.settingsBtn = CreateMenuButton("SettingsButton", btnCol.transform, "设置",       24, CWhite);
        d.historyBtn  = CreateMenuButton("HistoryButton",  btnCol.transform, "战绩",       24, CWhite);
        d.quitBtn     = CreateMenuButton("QuitButton",     btnCol.transform, "退出",       24, CWhite);

        return d;
    }

    static void AnchorBottomRow(Transform t, float x, float yFromBottom)
    {
        var rt = (RectTransform)t;
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(x, yFromBottom);
    }

    static GameObject CreatePanelSection(string name, Transform parent, int row, string title)
    {
        var go = CreateImage(name, parent, CPanel);
        var rt = (RectTransform)go.transform;
        const float totalRows = 3f;
        float anchorTop    = 1f - row / totalRows;
        float anchorBottom = 1f - (row + 1f) / totalRows;
        rt.anchorMin = new Vector2(0, anchorBottom);
        rt.anchorMax = new Vector2(1, anchorTop);
        rt.offsetMin = new Vector2(0, 10);
        rt.offsetMax = new Vector2(0, -10);

        var titleGo = CreateTmp("Title", go.transform, title, 28, CTitleAccent);
        var trt = (RectTransform)titleGo.transform;
        trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1); trt.anchoredPosition = new Vector2(0, -5);
        trt.sizeDelta = new Vector2(0, 40);
        titleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        return go;
    }

    public class HowToPlayData
    {
        public GameObject panel;
        public Button backBtn;
    }

    static HowToPlayData BuildHowToPlayPanel(Transform parent)
    {
        var d = new HowToPlayData();
        d.panel = CreateImage("HowToPlayPanel", parent, COverlay);
        StretchAll((RectTransform)d.panel.transform);

        d.panel.AddComponent<CanvasGroup>();
        d.panel.AddComponent<UIFader>();

        var titleGo = CreateTmp("Title", d.panel.transform, "操作说明", 80, CTitleAccent);
        var trt = (RectTransform)titleGo.transform;
        trt.anchorMin = new Vector2(0.5f, 1); trt.anchorMax = new Vector2(0.5f, 1);
        trt.pivot = new Vector2(0.5f, 1); trt.anchoredPosition = new Vector2(0, -60);
        trt.sizeDelta = new Vector2(800, 120);
        titleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var bodyGo = CreateTmp("HelpText", d.panel.transform,
            "玩法\n\n" +
            "· 把对手挤出场地外即获胜(开放无墙)\n" +
            "· 每隔几秒状态轮盘切换:加速 / 变大 / 变小\n" +
            "· 60 秒后红色 AI 球出现追击,撞死你也算输\n" +
            "· 道具掉落:加速(黄) / 变重(紫) / 反向(灰) / 无敌(金) / 冲击波(橙红)\n" +
            "· 合作模式:所有玩家死亡才结算,记录最长存活\n\n" +
            "操作\n\n" +
            "· 玩家 1: W A S D 移动\n" +
            "· 玩家 2: 方向键 移动\n" +
            "· 玩家 3: I J K L 移动\n" +
            "· ESC: 暂停 / 继续\n" +
            "· R: 结算后重开\n" +
            "· M: 结算后回主菜单",
            28, CWhite);
        var brt = (RectTransform)bodyGo.transform;
        brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f); brt.anchoredPosition = new Vector2(0, -30);
        brt.sizeDelta = new Vector2(1400, 700);
        bodyGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;

        d.backBtn = CreateMenuButton("BackButton", d.panel.transform, "返回 (ESC)", 26, CWhite);
        var bkRt = (RectTransform)d.backBtn.transform;
        bkRt.anchorMin = new Vector2(0.5f, 0); bkRt.anchorMax = new Vector2(0.5f, 0);
        bkRt.pivot = new Vector2(0.5f, 0); bkRt.anchoredPosition = new Vector2(0, 60);
        bkRt.sizeDelta = new Vector2(280, 70);

        return d;
    }

    public class SettingsData
    {
        public GameObject panel;
        public Slider bgmSlider, sfxSlider;
        public TextMeshProUGUI bgmValue, sfxValue;
        public Button backBtn;
    }

    static SettingsData BuildSettingsPanel(Transform parent)
    {
        var d = new SettingsData();
        d.panel = CreateImage("SettingsPanel", parent, COverlay);
        StretchAll((RectTransform)d.panel.transform);
        d.panel.AddComponent<CanvasGroup>();
        d.panel.AddComponent<UIFader>();

        var titleGo = CreateTmp("Title", d.panel.transform, "设置", 80, CTitleAccent);
        var trt = (RectTransform)titleGo.transform;
        trt.anchorMin = new Vector2(0.5f, 1); trt.anchorMax = new Vector2(0.5f, 1);
        trt.pivot = new Vector2(0.5f, 1); trt.anchoredPosition = new Vector2(0, -80);
        trt.sizeDelta = new Vector2(400, 120);
        titleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // BGM Row
        var bgmRow = BuildSliderRow("BgmRow", d.panel.transform, "BGM 音量", -50, out d.bgmSlider, out d.bgmValue);
        // SFX Row
        var sfxRow = BuildSliderRow("SfxRow", d.panel.transform, "SFX 音量",  -160, out d.sfxSlider, out d.sfxValue);

        d.backBtn = CreateMenuButton("BackButton", d.panel.transform, "返回", 26, CWhite);
        var bkRt = (RectTransform)d.backBtn.transform;
        bkRt.anchorMin = new Vector2(0.5f, 0); bkRt.anchorMax = new Vector2(0.5f, 0);
        bkRt.pivot = new Vector2(0.5f, 0); bkRt.anchoredPosition = new Vector2(0, 80);
        bkRt.sizeDelta = new Vector2(280, 70);

        return d;
    }

    static GameObject BuildSliderRow(string name, Transform parent, string label, float anchorY, out Slider slider, out TextMeshProUGUI valueLabel)
    {
        var row = CreateRect(name, parent);
        var rrt = (RectTransform)row.transform;
        rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0.5f);
        rrt.pivot = new Vector2(0.5f, 0.5f);
        rrt.anchoredPosition = new Vector2(0, anchorY);
        rrt.sizeDelta = new Vector2(900, 80);

        var lbl = CreateTmp("Label", row.transform, label, 28, CWhite);
        var lrt = (RectTransform)lbl.transform;
        lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(0.25f, 1);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        lbl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        slider = BuildSlider("Slider", row.transform);
        var srt = (RectTransform)slider.transform;
        srt.anchorMin = new Vector2(0.27f, 0.35f); srt.anchorMax = new Vector2(0.85f, 0.65f);
        srt.offsetMin = srt.offsetMax = Vector2.zero;

        var v = CreateTmp("ValueLabel", row.transform, "80", 28, CWhite);
        var vrt = (RectTransform)v.transform;
        vrt.anchorMin = new Vector2(0.87f, 0); vrt.anchorMax = new Vector2(1, 1);
        vrt.offsetMin = vrt.offsetMax = Vector2.zero;
        valueLabel = v.GetComponent<TextMeshProUGUI>();
        valueLabel.alignment = TextAlignmentOptions.Left;
        return row;
    }

    static Slider BuildSlider(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        var slider = go.GetComponent<Slider>();

        var bg = CreateImage("Background", go.transform, new Color(0.2f, 0.2f, 0.25f, 1f));
        var bgRt = (RectTransform)bg.transform;
        bgRt.anchorMin = new Vector2(0, 0.25f); bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        var fillArea = CreateRect("Fill Area", go.transform);
        var faRt = (RectTransform)fillArea.transform;
        faRt.anchorMin = new Vector2(0, 0.25f); faRt.anchorMax = new Vector2(1, 0.75f);
        faRt.offsetMin = new Vector2(5, 0); faRt.offsetMax = new Vector2(-15, 0);

        var fill = CreateImage("Fill", fillArea.transform, CSelected);
        var fillRt = (RectTransform)fill.transform;
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = new Vector2(10, 0);

        var handleArea = CreateRect("Handle Slide Area", go.transform);
        var haRt = (RectTransform)handleArea.transform;
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10, 0); haRt.offsetMax = new Vector2(-10, 0);

        var handle = CreateImage("Handle", handleArea.transform, CWhite);
        var hrt = (RectTransform)handle.transform;
        hrt.sizeDelta = new Vector2(20, 0);
        hrt.anchorMin = new Vector2(0, 0); hrt.anchorMax = new Vector2(0, 1);

        slider.fillRect = fillRt;
        slider.handleRect = hrt;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.8f;
        return slider;
    }

    public class HistoryData
    {
        public GameObject panel;
        public TextMeshProUGUI pvpText;
        public TextMeshProUGUI coopText;
        public Button clearBtn, refreshBtn, backBtn;
    }

    static HistoryData BuildHistoryPanel(Transform parent)
    {
        var d = new HistoryData();
        d.panel = CreateImage("HistoryPanel", parent, COverlay);
        StretchAll((RectTransform)d.panel.transform);
        d.panel.AddComponent<CanvasGroup>();
        d.panel.AddComponent<UIFader>();

        var titleGo = CreateTmp("Title", d.panel.transform, "战绩", 80, CTitleAccent);
        var trt = (RectTransform)titleGo.transform;
        trt.anchorMin = new Vector2(0.5f, 1); trt.anchorMax = new Vector2(0.5f, 1);
        trt.pivot = new Vector2(0.5f, 1); trt.anchoredPosition = new Vector2(0, -60);
        trt.sizeDelta = new Vector2(400, 120);
        titleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        d.pvpText = CreateTmp("PvPText", d.panel.transform, "PvP", 28, CWhite).GetComponent<TextMeshProUGUI>();
        var prt = (RectTransform)d.pvpText.transform;
        prt.anchorMin = new Vector2(0, 0.5f); prt.anchorMax = new Vector2(0.5f, 1);
        prt.offsetMin = new Vector2(120, 0); prt.offsetMax = new Vector2(-30, -200);
        d.pvpText.alignment = TextAlignmentOptions.TopLeft;

        d.coopText = CreateTmp("CoopText", d.panel.transform, "COOP", 28, CWhite).GetComponent<TextMeshProUGUI>();
        var crt = (RectTransform)d.coopText.transform;
        crt.anchorMin = new Vector2(0.5f, 0.5f); crt.anchorMax = new Vector2(1, 1);
        crt.offsetMin = new Vector2(30, 0); crt.offsetMax = new Vector2(-120, -200);
        d.coopText.alignment = TextAlignmentOptions.TopLeft;

        d.refreshBtn = CreateMenuButton("RefreshButton", d.panel.transform, "刷新", 22, CWhite);
        var rrt = (RectTransform)d.refreshBtn.transform;
        rrt.anchorMin = new Vector2(0.5f, 0); rrt.anchorMax = new Vector2(0.5f, 0);
        rrt.pivot = new Vector2(0.5f, 0); rrt.anchoredPosition = new Vector2(-300, 80);
        rrt.sizeDelta = new Vector2(220, 60);

        d.clearBtn = CreateMenuButton("ClearButton", d.panel.transform, "清除", 22, CWhite);
        var clrRt = (RectTransform)d.clearBtn.transform;
        clrRt.anchorMin = new Vector2(0.5f, 0); clrRt.anchorMax = new Vector2(0.5f, 0);
        clrRt.pivot = new Vector2(0.5f, 0); clrRt.anchoredPosition = new Vector2(0, 80);
        clrRt.sizeDelta = new Vector2(220, 60);

        d.backBtn = CreateMenuButton("BackButton", d.panel.transform, "返回", 22, CWhite);
        var bRt = (RectTransform)d.backBtn.transform;
        bRt.anchorMin = new Vector2(0.5f, 0); bRt.anchorMax = new Vector2(0.5f, 0);
        bRt.pivot = new Vector2(0.5f, 0); bRt.anchoredPosition = new Vector2(300, 80);
        bRt.sizeDelta = new Vector2(220, 60);

        return d;
    }

    // ====================================================================
    // Map button prefab
    // ====================================================================

    static GameObject BuildMapButtonPrefab()
    {
        Directory.CreateDirectory(PrefabsDir);
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MapBtnPrefabPath);
        if (existing != null) return existing;

        var go = new GameObject("MapButton",
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<Image>().color = CUnselected;
        go.GetComponent<LayoutElement>().preferredWidth  = 180;
        go.GetComponent<LayoutElement>().preferredHeight = 140;
        ((RectTransform)go.transform).sizeDelta = new Vector2(180, 140);
        go.AddComponent<ButtonHoverEffect>();
        go.AddComponent<UIClickSound>();

        var preview = CreateImage("Preview", go.transform, CWhite);
        var prRt = (RectTransform)preview.transform;
        prRt.anchorMin = new Vector2(0, 0.3f); prRt.anchorMax = new Vector2(1, 1);
        prRt.offsetMin = new Vector2(6, 0); prRt.offsetMax = new Vector2(-6, -6);
        preview.GetComponent<Image>().raycastTarget = false;

        var lbl = CreateTmp("Label", go.transform, "地图", 22, CWhite);
        var lrt = (RectTransform)lbl.transform;
        lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 0.3f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var lblTmp = lbl.GetComponent<TextMeshProUGUI>();
        lblTmp.alignment = TextAlignmentOptions.Center;
        lblTmp.raycastTarget = false;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, MapBtnPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ====================================================================
    // 接线
    // ====================================================================

    static void WireMainMenu(MainMenuController menu, MainPanelData m,
        GameObject howToPanel, GameObject settingsPanel, GameObject historyPanel,
        PlayerCountSelector pcSel, MapSelectController mapSel)
    {
        var so = new SerializedObject(menu);
        so.FindProperty("startButton").objectReferenceValue       = m.startBtn;
        so.FindProperty("howToPlayButton").objectReferenceValue   = m.howToBtn;
        so.FindProperty("settingsButton").objectReferenceValue    = m.settingsBtn;
        so.FindProperty("historyButton").objectReferenceValue     = m.historyBtn;
        so.FindProperty("quitButton").objectReferenceValue        = m.quitBtn;
        so.FindProperty("mainPanel").objectReferenceValue         = m.panel;
        so.FindProperty("howToPlayPanel").objectReferenceValue    = howToPanel;
        so.FindProperty("settingsPanel").objectReferenceValue     = settingsPanel;
        so.FindProperty("historyPanel").objectReferenceValue      = historyPanel;
        so.FindProperty("playerCountSelector").objectReferenceValue = pcSel;
        so.FindProperty("mapSelectController").objectReferenceValue = mapSel;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireModeSelector(GameModeSelector sel, MainPanelData m)
    {
        var so = new SerializedObject(sel);
        so.FindProperty("pvpButton").objectReferenceValue  = m.pvpBtn;
        so.FindProperty("coopButton").objectReferenceValue = m.coopBtn;
        so.FindProperty("currentLabel").objectReferenceValue = m.modeLabel;
        so.FindProperty("coopHintGroup").objectReferenceValue = m.coopHintGroup;
        so.FindProperty("difficultyGroup").objectReferenceValue = m.diffRowGo;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WirePlayerCountSelector(PlayerCountSelector sel, MainPanelData m)
    {
        var so = new SerializedObject(sel);
        so.FindProperty("twoPlayerButton").objectReferenceValue   = m.p2Btn;
        so.FindProperty("threePlayerButton").objectReferenceValue = m.p3Btn;
        so.FindProperty("currentLabel").objectReferenceValue      = m.pcLabel;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireDifficultySelector(DifficultySelector sel, MainPanelData m)
    {
        var so = new SerializedObject(sel);
        so.FindProperty("easyButton").objectReferenceValue     = m.easyBtn;
        so.FindProperty("normalButton").objectReferenceValue   = m.normalBtn;
        so.FindProperty("hardButton").objectReferenceValue     = m.hardBtn;
        so.FindProperty("currentLabel").objectReferenceValue   = m.diffLabel;
        // profilePreview 已从 UI 移除, 不再接线 (字段保留在脚本里, 留空即可)
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireMapSelector(MapSelectController sel, MainPanelData m, GameObject buttonPrefab)
    {
        var so = new SerializedObject(sel);
        so.FindProperty("listRoot").objectReferenceValue     = m.mapListRoot;
        so.FindProperty("buttonPrefab").objectReferenceValue = buttonPrefab;
        so.FindProperty("previewImage").objectReferenceValue = m.mapPreviewImg;
        so.FindProperty("previewName").objectReferenceValue  = m.mapPreviewName;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireSettingsPanel(SettingsPanel sp, SettingsData d, MainMenuController menu)
    {
        var so = new SerializedObject(sp);
        so.FindProperty("bgmSlider").objectReferenceValue     = d.bgmSlider;
        so.FindProperty("sfxSlider").objectReferenceValue     = d.sfxSlider;
        so.FindProperty("bgmValueLabel").objectReferenceValue = d.bgmValue;
        so.FindProperty("sfxValueLabel").objectReferenceValue = d.sfxValue;
        so.FindProperty("backButton").objectReferenceValue    = d.backBtn;
        so.FindProperty("menu").objectReferenceValue          = menu;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireHistoryPanel(HistoryPanel hp, HistoryData d, MainMenuController menu)
    {
        var so = new SerializedObject(hp);
        so.FindProperty("pvpText").objectReferenceValue       = d.pvpText;
        so.FindProperty("coopText").objectReferenceValue      = d.coopText;
        so.FindProperty("clearButton").objectReferenceValue   = d.clearBtn;
        so.FindProperty("refreshButton").objectReferenceValue = d.refreshBtn;
        so.FindProperty("backButton").objectReferenceValue    = d.backBtn;
        so.FindProperty("menu").objectReferenceValue          = menu;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireHowToPlayPanel(HowToPlayPanel hp, HowToPlayData d, MainMenuController menu)
    {
        var so = new SerializedObject(hp);
        so.FindProperty("backButton").objectReferenceValue = d.backBtn;
        so.FindProperty("menu").objectReferenceValue       = menu;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ====================================================================
    // UI 工厂
    // ====================================================================

    static GameObject CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject CreateImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static GameObject CreateTmp(string name, Transform parent, string text, float fontSize, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        TMP_FontAsset def = null;
        try { def = TMP_Settings.defaultFontAsset; } catch { }
        if (def != null) t.font = def;
        return go;
    }

    static Button CreateButton(string name, Transform parent, string label, float fontSize, Vector2 size, Vector2 anchoredPos)
    {
        var go = CreateImage(name, parent, CBtn);
        var btn = go.AddComponent<Button>();
        go.AddComponent<ButtonHoverEffect>();
        go.AddComponent<UIClickSound>();

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 1); rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var labelGo = CreateTmp("Label", go.transform, label, fontSize, CWhite);
        StretchAll((RectTransform)labelGo.transform);

        var col = btn.colors;
        col.normalColor      = CBtn;
        col.highlightedColor = CBtnHover;
        col.pressedColor     = new Color(0.15f, 0.18f, 0.25f);
        col.selectedColor    = CBtn;
        btn.colors = col;

        return btn;
    }

    static Button CreateMenuButton(string name, Transform parent, string label, float fontSize, Color textColor)
    {
        var go = CreateImage(name, parent, CBtn);
        var btn = go.AddComponent<Button>();
        go.AddComponent<LayoutElement>().preferredHeight = 70;
        go.AddComponent<ButtonHoverEffect>();
        go.AddComponent<UIClickSound>();

        var labelGo = CreateTmp("Label", go.transform, label, fontSize, textColor);
        StretchAll((RectTransform)labelGo.transform);
        labelGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var col = btn.colors;
        col.normalColor      = CBtn;
        col.highlightedColor = CBtnHover;
        col.pressedColor     = new Color(0.15f, 0.18f, 0.25f);
        btn.colors = col;

        return btn;
    }

    static void StretchAll(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
