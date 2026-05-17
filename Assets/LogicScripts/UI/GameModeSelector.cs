using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelector : MonoBehaviour
{
    [SerializeField] Button   pvpButton;
    [SerializeField] Button   coopButton;
    [SerializeField] TMP_Text currentLabel;
    [SerializeField] GameObject coopHintGroup;
    [Tooltip("整个难度行 (DifficultyRow). PvP 模式自动隐藏 - 不让玩家选难度. 留空则不隐藏.")]
    [SerializeField] GameObject difficultyGroup;

    [SerializeField] Color selectedColor   = new Color(0.3f, 0.7f, 1f, 1f);
    [SerializeField] Color unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    void Start()
    {
        GameSession.Ensure();
        if (pvpButton  != null) pvpButton.onClick.AddListener(()  => Set(GameMode.PvP));
        if (coopButton != null) coopButton.onClick.AddListener(() => Set(GameMode.Coop));
        Refresh();
    }

    public void Set(GameMode m)
    {
        if (GameSession.Instance != null) GameSession.Instance.Mode = m;
        Refresh();
    }

    void Refresh()
    {
        var m = GameSession.Instance != null ? GameSession.Instance.Mode : GameMode.PvP;
        if (currentLabel != null)
            currentLabel.text = m == GameMode.PvP ? "模式: 对战" : "模式: 合作";
        Paint(pvpButton,  m == GameMode.PvP);
        Paint(coopButton, m == GameMode.Coop);
        if (coopHintGroup != null) coopHintGroup.SetActive(m == GameMode.Coop);
        if (difficultyGroup != null) difficultyGroup.SetActive(m == GameMode.Coop);
    }

    void Paint(Button btn, bool selected)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = selected ? selectedColor : unselectedColor;
    }
}
