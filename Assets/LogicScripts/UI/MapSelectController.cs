using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectController : MonoBehaviour
{
    [Header("List Container")]
    [SerializeField] Transform  listRoot;
    [SerializeField] GameObject buttonPrefab;

    [Header("Preview")]
    [SerializeField] Image    previewImage;
    [SerializeField] TMP_Text previewName;

    [Header("Data")]
    [Tooltip("留空则自动从 Resources/ArenaPresets/ 加载全部。")]
    [SerializeField] List<ArenaPreset> presets = new List<ArenaPreset>();

    [Header("Visual Feedback")]
    [SerializeField] Color selectedColor   = new Color(0.3f, 0.7f, 1f, 1f);
    [SerializeField] Color unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    readonly List<Button> spawnedButtons = new List<Button>();

    void Start()
    {
        GameSession.Ensure();
        LoadPresetsIfEmpty();
        BuildList();
        SelectDefaultIfNone();
        Refresh();
    }

    void LoadPresetsIfEmpty()
    {
        if (presets.Count > 0) return;
        var loaded = Resources.LoadAll<ArenaPreset>("ArenaPresets");
        if (loaded == null) return;
        foreach (var p in loaded)
            if (p != null) presets.Add(p);
    }

    void BuildList()
    {
        if (listRoot == null || buttonPrefab == null) return;

        foreach (var b in spawnedButtons)
            if (b != null) Destroy(b.gameObject);
        spawnedButtons.Clear();

        foreach (var preset in presets)
        {
            if (preset == null) continue;
            var go  = Instantiate(buttonPrefab, listRoot);
            var btn = go.GetComponent<Button>();
            var img = go.GetComponentInChildren<Image>();
            var txt = go.GetComponentInChildren<TMP_Text>();

            if (txt != null) txt.text = preset.displayName;
            if (img != null && preset.previewSprite != null && img != go.GetComponent<Image>())
                img.sprite = preset.previewSprite;

            var captured = preset;
            if (btn != null)
            {
                btn.onClick.AddListener(() => Choose(captured));
                spawnedButtons.Add(btn);
            }
        }
    }

    public void Choose(ArenaPreset preset)
    {
        if (GameSession.Instance != null) GameSession.Instance.SelectedArena = preset;
        Refresh();
    }

    public void SelectDefaultIfNone()
    {
        if (GameSession.Instance == null) return;
        if (GameSession.Instance.SelectedArena != null) return;
        if (presets.Count == 0) return;
        GameSession.Instance.SelectedArena = presets[0];
    }

    void Refresh()
    {
        var current = GameSession.Instance != null ? GameSession.Instance.SelectedArena : null;

        if (previewImage != null)
        {
            previewImage.sprite  = current != null ? current.previewSprite : null;
            previewImage.enabled = current != null && current.previewSprite != null;
        }
        if (previewName != null)
            previewName.text = current != null ? current.displayName : "(未选择)";

        for (int i = 0; i < spawnedButtons.Count && i < presets.Count; i++)
        {
            var bg = spawnedButtons[i].GetComponent<Image>();
            if (bg != null) bg.color = presets[i] == current ? selectedColor : unselectedColor;
        }
    }
}
