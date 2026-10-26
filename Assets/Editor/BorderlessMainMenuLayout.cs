using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BorderlessMainMenuLayout
{
    const string ScenePath = "Assets/Scenes/MainMenu.unity";
    const string BackgroundGuid = "7a3e5d050a6242caa923bb7c5d814501";
    const string State = "PPP.BorderlessMenuReview.";
    static readonly Color Ink = new Color(0.15f, 0.17f, 0.16f, 1f);
    static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
    static string RequestPath => Path.Combine(ProjectRoot, "Library/BorderlessMenu.request");
    static string ReviewDir => Path.Combine(ProjectRoot, "Library/BorderlessMenuReview");
    static double nextCheck;

    static BorderlessMainMenuLayout()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, stack, type) =>
        {
            if (SessionState.GetBool(State + "Running", false) &&
                (type == LogType.Error || type == LogType.Exception))
                SessionState.SetString(State + "Error", message + "\n" + stack);
        };
    }

    [MenuItem("Tools/PongPongPong/Apply Approved Borderless Menu")]
    public static void ApplyFromMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play mode before applying the menu layout.");
        Apply();
    }

    static T Read<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
    {
        var property = new SerializedObject(owner).FindProperty(field);
        if (property == null) throw new InvalidOperationException("Missing field: " + field);
        return property.objectReferenceValue as T;
    }

    static void Wire(UnityEngine.Object owner, string field, UnityEngine.Object value)
    {
        var so = new SerializedObject(owner);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static T Find<T>(Scene scene) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var component = root.GetComponentInChildren<T>(true);
            if (component != null) return component;
        }
        throw new InvalidOperationException(typeof(T).Name + " not found in " + scene.path);
    }

    static RectTransform Rect(string name, Transform parent)
    {
        var existing = parent.Find(name) as RectTransform;
        if (existing != null) return existing;
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "Create borderless menu group");
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static void Top(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(width, height);
        rt.localScale = Vector3.one;
    }

    static Material TextMaterial(TMP_Text source)
    {
        const string path = "Assets/Art/UI/BorderlessMenuText.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        if (!AssetDatabase.IsValidFolder("Assets/Art/UI"))
            AssetDatabase.CreateFolder("Assets/Art", "UI");
        material = new Material(source.font.material) { name = "BorderlessMenuText" };
        foreach (string property in new[] { "_OutlineWidth", "_OutlineSoftness", "_FaceDilate", "_UnderlayDilate", "_UnderlaySoftness" })
            if (material.HasProperty(property)) material.SetFloat(property, 0f);
        material.DisableKeyword("UNDERLAY_ON");
        material.DisableKeyword("UNDERLAY_INNER");
        if (material.HasProperty("_FaceColor")) material.SetColor("_FaceColor", Color.white);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void Text(TMP_Text text, Material material, float size)
    {
        text.fontSharedMaterial = material;
        text.fontStyle = FontStyles.Normal;
        text.fontWeight = FontWeight.Regular;
        text.enableAutoSizing = false;
        text.fontSize = size;
        text.characterSpacing = 2f;
        text.color = Ink;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
    }

    static void Style(UnityEngine.UI.Button button, Material material, float size)
    {
        var hover = button.GetComponent<ButtonHoverEffect>();
        if (hover != null) hover.enabled = false;
        button.transform.localScale = Vector3.one;
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        var image = button.GetComponent<UnityEngine.UI.Image>();
        image.enabled = true;
        image.sprite = null;
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.color = Color.clear;
        image.raycastTarget = true;
        button.targetGraphic = image;
        var label = button.GetComponentInChildren<TMP_Text>(true);
        Text(label, material, size);
        Stretch(label.rectTransform);
        var cursor = Rect("SelectionCursor", button.transform);
        cursor.anchorMin = cursor.anchorMax = new Vector2(0f, 0.5f);
        cursor.pivot = new Vector2(0.5f, 0.5f);
        cursor.anchoredPosition = new Vector2(25f, 0f);
        cursor.sizeDelta = new Vector2(8f, 8f);
        cursor.localRotation = Quaternion.Euler(0f, 0f, 45f);
        var cursorImage = cursor.GetComponent<UnityEngine.UI.Image>();
            if (cursorImage == null) cursorImage = Undo.AddComponent<UnityEngine.UI.Image>(cursor.gameObject);
        cursorImage.color = new Color(0.98f, 0.98f, 0.95f, 1f);
        cursorImage.raycastTarget = false;
        var appearance = button.GetComponent<MenuButtonAppearance>();
        if (appearance == null) appearance = Undo.AddComponent<MenuButtonAppearance>(button.gameObject);
        appearance.Configure(label, image, cursor.gameObject);
    }

    static UnityEngine.UI.Button Action(string name, string label,
        UnityEngine.UI.Button source, Transform parent, Material material)
    {
        var existing = parent.Find(name);
        var button = existing != null ? existing.GetComponent<UnityEngine.UI.Button>() : null;
        if (button == null)
        {
            button = UnityEngine.Object.Instantiate(source, parent);
            button.name = name;
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create match setup action");
            button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        }
        button.GetComponentInChildren<TMP_Text>(true).text = label;
        Style(button, material, 34f);
        return button;
    }

    static void Group(Component selector, string[] fields, string title,
        Transform setup, float y, Material material)
    {
        var first = Read<UnityEngine.UI.Button>(selector, fields[0]);
        var row = (RectTransform)first.transform.parent;
        row.SetParent(setup, false);
        Top(row, 0, y, 640, 116);
        foreach (var layout in row.GetComponents<UnityEngine.UI.LayoutGroup>()) layout.enabled = false;
        var background = row.GetComponent<UnityEngine.UI.Image>();
        if (background != null) { background.color = Color.clear; background.raycastTarget = false; }
        foreach (var text in row.GetComponentsInChildren<TMP_Text>(true))
            if (text.transform.parent == row && text.name != "Title") text.gameObject.SetActive(false);
        var heading = row.Find("Title").GetComponent<TMP_Text>();
        heading.text = title;
        Text(heading, material, 31f);
        Top(heading.rectTransform, 0, 0, 640, 44);
        float spacing = fields.Length == 3 ? 200f : 260f;
        for (int i = 0; i < fields.Length; i++)
        {
            var button = Read<UnityEngine.UI.Button>(selector, fields[i]);
            Top((RectTransform)button.transform, (i - (fields.Length - 1) * 0.5f) * spacing, 56, 190, 54);
            Style(button, material, 32f);
        }
    }

    static void Apply()
    {
        Directory.CreateDirectory(ReviewDir);
        var scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        var menu = Find<MainMenuController>(scene);
        var main = Read<GameObject>(menu, "mainPanel");
        var canvas = main.GetComponentInParent<Canvas>();
        var background = canvas.transform.Find("Background").GetComponent<UnityEngine.UI.Image>();
        Require(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(background.sprite)) == BackgroundGuid,
            "Original background reference must be preserved.");
        if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        var backup = Path.Combine(ReviewDir, "MainMenu.before-apply.unity");
        if (!File.Exists(backup)) File.Copy(Path.Combine(ProjectRoot, ScenePath), backup);
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        foreach (var root in scene.GetRootGameObjects()) Undo.RegisterFullObjectHierarchyUndo(root, "Apply approved borderless menu");
        try
        {
            var start = Read<UnityEngine.UI.Button>(menu, "startButton");
            var material = TextMaterial(start.GetComponentInChildren<TMP_Text>(true));
            var setup = Rect("MatchSetupPanel", canvas.transform);
            Stretch(setup);
            Stretch((RectTransform)main.transform);
            var mainBackground = main.GetComponent<UnityEngine.UI.Image>();
            if (mainBackground != null) { mainBackground.color = Color.clear; mainBackground.raycastTarget = false; }
            var actions = Rect("MainActions", main.transform);
            Top(actions, 0, 336, 420, 356);
            var vertical = actions.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (vertical == null) vertical = Undo.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(actions.gameObject);
            vertical.spacing = 14;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;
            var fields = new[] { "startButton", "howToPlayButton", "settingsButton", "historyButton", "quitButton" };
            var buttons = new UnityEngine.UI.Button[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var button = buttons[i] = Read<UnityEngine.UI.Button>(menu, fields[i]);
                button.transform.SetParent(actions, false);
                Style(button, material, 34f);
                var element = button.GetComponent<UnityEngine.UI.LayoutElement>();
                if (element == null) element = Undo.AddComponent<UnityEngine.UI.LayoutElement>(button.gameObject);
                element.minHeight = element.preferredHeight = 60;
                element.flexibleHeight = 0;
            }
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].navigation = new UnityEngine.UI.Navigation {
                    mode = UnityEngine.UI.Navigation.Mode.Explicit,
                    selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length],
                    selectOnDown = buttons[(i + 1) % buttons.Length]
                };

            var mode = Find<GameModeSelector>(scene);
            var players = Find<PlayerCountSelector>(scene);
            var difficulty = Find<DifficultySelector>(scene);
            Group(mode, new[] { "pvpButton", "coopButton" }, "游戏模式", setup, 340, material);
            Group(players, new[] { "twoPlayerButton", "threePlayerButton" }, "玩家人数", setup, 470, material);
            Group(difficulty, new[] { "easyButton", "normalButton", "hardButton" }, "AI 难度", setup, 600, material);
            var hint = Read<GameObject>(mode, "coopHintGroup");
            if (hint != null) hint.SetActive(false);
            Wire(mode, "coopHintGroup", null);
            foreach (var name in new[] { "LeftColumn", "RightColumn" })
            {
                var oldColumn = main.transform.Find(name);
                if (oldColumn != null) oldColumn.gameObject.SetActive(false);
            }
            var map = Read<MapSelectController>(menu, "mapSelectController");
            if (map != null) map.enabled = false;
            Wire(menu, "mapSelectController", null);

            var play = Action("PlayButton", "开始对局", start, setup, material);
            var back = Action("SetupBackButton", "返回", start, setup, material);
            Top((RectTransform)play.transform, -125, 754, 300, 62);
            Top((RectTransform)back.transform, 175, 754, 180, 62);
            play.navigation = back.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Automatic };
            Wire(menu, "matchSetupPanel", setup.gameObject);
            Wire(menu, "playButton", play);
            Wire(menu, "setupBackButton", back);
            var logo = canvas.transform.Find("TitleText") as RectTransform;
            Top(logo, 0, 142, 620, 150);
            main.SetActive(true);
            setup.gameObject.SetActive(false);
            foreach (var field in new[] { "howToPlayPanel", "settingsPanel", "historyPanel" }) Read<GameObject>(menu, field).SetActive(false);
            start.GetComponent<MenuButtonAppearance>().SetChosen(true);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssetIfDirty(material);
            Undo.CollapseUndoOperations(undoGroup);
            WriteReport("apply.json", true, "Approved menu layout applied to the existing MainMenu scene.", 0);
        }
        catch { Undo.RevertAllDownToGroup(undoGroup); throw; }
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (SessionState.GetBool(State + "Running", false))
        {
            try { VerifyStep(); }
            catch (Exception error) { Finish(false, error.ToString()); }
            return;
        }
        if (EditorApplication.timeSinceStartup < nextCheck) return;
        nextCheck = EditorApplication.timeSinceStartup + 0.5;
        if (!File.Exists(RequestPath)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }
        string request = File.ReadAllText(RequestPath).Trim();
        File.Delete(RequestPath);
        try
        {
            if (request == "apply") { Apply(); File.WriteAllText(RequestPath, "verify"); }
            else if (request == "verify") BeginVerification();
            else if (request == "refresh") AssetDatabase.Refresh();
            else throw new InvalidOperationException("Unknown menu request: " + request);
        }
        catch (Exception error) { WriteReport("result.json", false, error.ToString(), 0); }
    }

    static readonly string[] PrefKeys = { "PPP_GameMode", "PPP_PlayerCount", "PPP_Difficulty" };

    static void BeginVerification()
    {
        Require(SceneManager.GetActiveScene().path == ScenePath, "Open MainMenu before running its integration check.");
        foreach (var key in PrefKeys)
        {
            SessionState.SetBool(State + key + ".Exists", PlayerPrefs.HasKey(key));
            SessionState.SetInt(State + key, PlayerPrefs.GetInt(key));
        }
        SessionState.SetString(State + "Error", "");
        SessionState.SetBool(State + "RunInBackground", Application.runInBackground);
        SessionState.SetInt(State + "Step", 0);
        SessionState.SetFloat(State + "Deadline", (float)EditorApplication.timeSinceStartup + 45f);
        SessionState.SetBool(State + "Running", true);
        EditorApplication.isPlaying = true;
    }

    static void WaitNextFrame(int step)
    {
        SessionState.SetInt(State + "Step", step);
        SessionState.SetInt(State + "NextFrame", Time.frameCount + 2);
    }

    static void Click(UnityEngine.Object owner, string field) => Read<UnityEngine.UI.Button>(owner, field).onClick.Invoke();

    static void VerifyStep()
    {
        Require(EditorApplication.timeSinceStartup < SessionState.GetFloat(State + "Deadline", 0), "Menu verification timed out.");
        if (!EditorApplication.isPlaying) return;
        Application.runInBackground = true;
        int step = SessionState.GetInt(State + "Step", 0);
        if (Time.frameCount < (step == 0 ? 4 : SessionState.GetInt(State + "NextFrame", 0))) return;
        if (step == 5)
        {
            if (SceneManager.GetActiveScene().name != "SampleScene") return;
            Require(GameSession.Instance.Mode == GameMode.Coop && GameSession.Instance.PlayerCount == 3 &&
                GameSession.Instance.Difficulty == AIDifficulty.Normal, "Match choices were not carried into the game.");
            Finish(true, "Main menu, setup, choices, overlays, return actions and game launch passed.");
            return;
        }
        var scene = SceneManager.GetActiveScene();
        var menu = Find<MainMenuController>(scene);
        var main = Read<GameObject>(menu, "mainPanel");
        var setup = Read<GameObject>(menu, "matchSetupPanel");
        var canvas = main.GetComponentInParent<Canvas>();
        if (step == 0)
        {
            SessionState.SetInt(State + "InitialFrame", Time.frameCount);
            var demo = Find<DemoModeRunner>(scene);
            demo.enabled = false; // Test only; Play mode exit restores the user's idle demo.
            Require(main.activeSelf && !setup.activeSelf, "Setup should be hidden on the home page.");
            Require(main.GetComponentsInChildren<UnityEngine.UI.Button>(false).Length == 5, "Home page should have exactly five actions.");
            Capture(canvas, "main-1920x1080.png", 1920, 1080);
            Click(menu, "startButton");
            Require(SceneManager.GetActiveScene().path == ScenePath && setup.activeSelf && !main.activeSelf,
                "Start must open setup without launching the game.");
            var mode = Find<GameModeSelector>(scene);
            var players = Find<PlayerCountSelector>(scene);
            var difficulty = Find<DifficultySelector>(scene);
            Click(mode, "pvpButton");
            var difficultyGroup = Read<GameObject>(mode, "difficultyGroup");
            Require(difficultyGroup == null || !difficultyGroup.activeSelf,
                "Preserve the existing optional PvP difficulty visibility setting.");
            Click(mode, "coopButton");
            Click(players, "twoPlayerButton");
            Require(GameSession.Instance.PlayerCount == 2, "Two-player selection failed.");
            Click(players, "threePlayerButton");
            Click(difficulty, "hardButton");
            Require(GameSession.Instance.Difficulty == AIDifficulty.Hard, "Hard selection failed.");
            Click(difficulty, "normalButton");
            Require(GameSession.Instance.Mode == GameMode.Coop && GameSession.Instance.PlayerCount == 3 &&
                GameSession.Instance.Difficulty == AIDifficulty.Normal, "Setup choices failed.");
            Require(Read<UnityEngine.UI.Button>(mode, "coopButton").GetComponent<UnityEngine.UI.Image>().color.a > 0.5f,
                "Selected options should have a translucent highlight.");
            Capture(canvas, "setup-1920x1080.png", 1920, 1080);
            Capture(canvas, "setup-1280x800.png", 1280, 800);
            Click(menu, "setupBackButton");
            Require(main.activeSelf && !setup.activeSelf, "Setup return failed.");
            Click(menu, "howToPlayButton");
            Require(Read<GameObject>(menu, "howToPlayPanel").activeSelf, "Instructions did not open.");
            WaitNextFrame(1);
        }
        else if (step == 1)
        {
            Click(Find<HowToPlayPanel>(scene), "backButton");
            Require(main.activeSelf, "Instructions return failed.");
            Click(menu, "settingsButton");
            Require(Read<GameObject>(menu, "settingsPanel").activeSelf, "Settings did not open.");
            WaitNextFrame(2);
        }
        else if (step == 2)
        {
            Click(Find<SettingsPanel>(scene), "backButton");
            Require(main.activeSelf, "Settings return failed.");
            Click(menu, "historyButton");
            Require(Read<GameObject>(menu, "historyPanel").activeSelf, "History did not open.");
            WaitNextFrame(3);
        }
        else if (step == 3)
        {
            Click(Find<HistoryPanel>(scene), "backButton");
            Require(main.activeSelf, "History return failed.");
            Click(menu, "startButton");
            WaitNextFrame(4);
        }
        else if (step == 4)
        {
            Click(menu, "playButton");
            WaitNextFrame(5);
        }
    }

    static void Capture(Canvas canvas, string filename, int width, int height)
    {
        var oldMode = canvas.renderMode;
        var oldCamera = canvas.worldCamera;
        float oldDistance = canvas.planeDistance;
        var oldTarget = RenderTexture.active;
        var target = new RenderTexture(width, height, 24);
        var go = new GameObject("MenuReviewCamera") { hideFlags = HideFlags.HideAndDontSave };
        var camera = go.AddComponent<Camera>();
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.orthographic = true;
        camera.orthographicSize = height * 0.5f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.targetTexture = target;
        Texture2D texture = null;
        try
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();
            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate();
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture.active = target;
            texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(ReviewDir, filename), texture.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode = oldMode;
            canvas.worldCamera = oldCamera;
            canvas.planeDistance = oldDistance;
            Canvas.ForceUpdateCanvases();
            RenderTexture.active = oldTarget;
            if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(go);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    static void Finish(bool success, string message)
    {
        string error = SessionState.GetString(State + "Error", "");
        if (!string.IsNullOrEmpty(error)) { success = false; message += "\n" + error; }
        int frames = Time.frameCount - SessionState.GetInt(State + "InitialFrame", Time.frameCount);
        foreach (var key in PrefKeys)
        {
            if (SessionState.GetBool(State + key + ".Exists", false)) PlayerPrefs.SetInt(key, SessionState.GetInt(State + key, 0));
            else PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        Application.runInBackground = SessionState.GetBool(State + "RunInBackground", false);
        SessionState.SetBool(State + "Running", false);
        WriteReport("result.json", success, message, frames);
        EditorApplication.isPlaying = false;
    }

    [Serializable]
    class Report
    {
        public bool success;
        public string summary;
        public int framesAdvanced;
        public string backgroundGuid;
    }

    static void WriteReport(string filename, bool success, string summary, int frames)
    {
        Directory.CreateDirectory(ReviewDir);
        File.WriteAllText(Path.Combine(ReviewDir, filename), JsonUtility.ToJson(new Report {
            success = success, summary = summary, framesAdvanced = frames, backgroundGuid = BackgroundGuid
        }, true));
    }
}
