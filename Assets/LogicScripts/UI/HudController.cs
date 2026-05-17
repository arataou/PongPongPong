using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] TMP_Text stateLabel;
    [SerializeField] TMP_Text aiLabel;
    [SerializeField] TMP_Text coopTimeLabel;
    [SerializeField] float warningThreshold = 3f;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color warningColor = Color.red;
    [SerializeField] Sprite speedIcon;
    [SerializeField] Sprite heavyIcon;
    [SerializeField] Sprite reverseIcon;
    [SerializeField] Sprite invincibleIcon;
    [SerializeField] Sprite shockwaveIcon;

    static readonly PickupBuff[] TimedBuffs =
    {
        PickupBuff.Speed,
        PickupBuff.Heavy,
        PickupBuff.Reverse,
        PickupBuff.Invincible,
    };

    readonly List<BuffHudRow> buffRows = new List<BuffHudRow>();
    RectTransform buffHudRoot;

    void Awake()
    {
        BuildBuffHud();
    }

    void Update()
    {
        bool isCoop = GameSession.Instance != null && GameSession.Instance.Mode == GameMode.Coop;
        UpdateStateLabel();
        UpdateAILabel();
        UpdateCoopLabel(isCoop);
        UpdateBuffHud();
    }

    void UpdateStateLabel()
    {
        if (stateLabel == null) return;
        var roul = StateRouletteController.Instance;
        if (roul == null) { stateLabel.text = ""; return; }

        float t = Mathf.Max(0f, roul.TimeUntilNext);
        stateLabel.text = $"State {Mathf.CeilToInt(t)}s";
    }

    void UpdateAILabel()
    {
        if (aiLabel == null) return;
        var sp = AISpawner.Instance;
        if (sp == null) { aiLabel.text = ""; return; }

        float t = Mathf.Max(0f, sp.TimeUntilNext);
        aiLabel.text = $"AI {Mathf.CeilToInt(t)}s";

        bool warning = !sp.FirstSpawned && t <= warningThreshold;
        if (warning)
        {
            float blink = Mathf.PingPong(Time.unscaledTime * 4f, 1f);
            aiLabel.color = (blink > 0.5f) ? warningColor : normalColor;
        }
        else
        {
            aiLabel.color = normalColor;
        }
    }

    void UpdateCoopLabel(bool isCoop)
    {
        if (coopTimeLabel == null) return;
        if (!isCoop || MatchManager.Instance == null)
        {
            coopTimeLabel.gameObject.SetActive(false);
            return;
        }
        coopTimeLabel.gameObject.SetActive(true);
        float t = MatchManager.Instance.ElapsedTime;
        int min = Mathf.FloorToInt(t / 60f);
        int sec = Mathf.FloorToInt(t % 60f);
        coopTimeLabel.text = $"Survive {min:00}:{sec:00}";
    }

    void BuildBuffHud()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var root = new GameObject("BuffStatusHUD", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        buffHudRoot = root.GetComponent<RectTransform>();
        buffHudRoot.anchorMin = new Vector2(0f, 1f);
        buffHudRoot.anchorMax = new Vector2(0f, 1f);
        buffHudRoot.pivot = new Vector2(0f, 1f);
        buffHudRoot.anchoredPosition = new Vector2(16f, -92f);
        buffHudRoot.sizeDelta = new Vector2(360f, 120f);

        var layout = root.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 6f;
    }

    void UpdateBuffHud()
    {
        if (buffHudRoot == null) return;

        var players = MatchManager.AlivePlayers;
        int visibleRows = 0;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            var row = EnsureRow(visibleRows);
            bool hasAny = false;
            Color accent = BallVisualUtility.AccentColor(player.gameObject);
            row.playerLabel.text = $"P{player.PlayerIndex}";
            row.playerLabel.color = new Color(accent.r, accent.g, accent.b, 0.94f);

            for (int j = 0; j < row.items.Length; j++)
            {
                var item = row.items[j];
                float remaining = player.GetBuffRemaining(item.buff);
                bool active = remaining > 0.05f;
                item.root.SetActive(active);
                if (!active) continue;

                item.timer.text = Mathf.CeilToInt(remaining).ToString();
                item.timer.color = new Color(1f, 1f, 1f, 0.92f);
                hasAny = true;
            }

            row.root.SetActive(hasAny);
            if (hasAny) visibleRows++;
        }

        for (int i = visibleRows; i < buffRows.Count; i++)
            buffRows[i].root.SetActive(false);
    }

    BuffHudRow EnsureRow(int index)
    {
        while (buffRows.Count <= index)
            buffRows.Add(CreateRow(buffRows.Count));
        return buffRows[index];
    }

    BuffHudRow CreateRow(int index)
    {
        var rowGo = new GameObject($"BuffHudRow_{index + 1}", typeof(RectTransform));
        rowGo.transform.SetParent(buffHudRoot, false);
        var rowRt = rowGo.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(340f, 30f);

        var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        rowLayout.spacing = 7f;

        var label = CreateText(rowGo.transform, "PlayerLabel", 17f, FontStyles.Bold);
        var labelRt = label.rectTransform;
        labelRt.sizeDelta = new Vector2(34f, 26f);

        var row = new BuffHudRow
        {
            root = rowGo,
            playerLabel = label,
            items = new BuffHudItem[TimedBuffs.Length]
        };

        for (int i = 0; i < TimedBuffs.Length; i++)
            row.items[i] = CreateItem(rowGo.transform, TimedBuffs[i]);

        row.root.SetActive(false);
        return row;
    }

    BuffHudItem CreateItem(Transform parent, PickupBuff buff)
    {
        var itemGo = new GameObject($"{buff}BuffHudItem", typeof(RectTransform));
        itemGo.transform.SetParent(parent, false);
        var itemRt = itemGo.GetComponent<RectTransform>();
        itemRt.sizeDelta = new Vector2(58f, 26f);

        var bg = itemGo.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.07f, 0.05f, 0.22f);
        bg.raycastTarget = false;

        var layout = itemGo.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 3f;
        layout.padding = new RectOffset(5, 5, 2, 2);

        var iconGo = new GameObject("Icon", typeof(RectTransform));
        iconGo.transform.SetParent(itemGo.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.sizeDelta = new Vector2(20f, 20f);
        var icon = iconGo.AddComponent<Image>();
        icon.sprite = SpriteFor(buff);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.color = Color.white;

        var timer = CreateText(itemGo.transform, "Timer", 14f, FontStyles.Bold);
        timer.rectTransform.sizeDelta = new Vector2(22f, 20f);
        timer.alignment = TextAlignmentOptions.Center;

        itemGo.SetActive(false);
        return new BuffHudItem { root = itemGo, icon = icon, timer = timer, buff = buff };
    }

    TextMeshProUGUI CreateText(Transform parent, string name, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        text.color = Color.white;
        return text;
    }

    Sprite SpriteFor(PickupBuff buff)
    {
        switch (buff)
        {
            case PickupBuff.Speed:      return speedIcon;
            case PickupBuff.Heavy:      return heavyIcon;
            case PickupBuff.Reverse:    return reverseIcon;
            case PickupBuff.Invincible: return invincibleIcon;
            case PickupBuff.Shockwave:  return shockwaveIcon;
            default:                    return null;
        }
    }

    class BuffHudRow
    {
        public GameObject root;
        public TMP_Text playerLabel;
        public BuffHudItem[] items;
    }

    class BuffHudItem
    {
        public GameObject root;
        public Image icon;
        public TMP_Text timer;
        public PickupBuff buff;
    }
}
