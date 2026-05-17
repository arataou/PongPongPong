using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanel : MonoBehaviour
{
    [SerializeField] TMP_Text pvpText;
    [SerializeField] TMP_Text coopText;
    [SerializeField] Button   clearButton;
    [SerializeField] Button   refreshButton;
    [SerializeField] Button   backButton;
    [SerializeField] MainMenuController menu;

    void OnEnable()
    {
        Refresh();
    }

    void Start()
    {
        if (clearButton   != null) clearButton.onClick.AddListener(OnClear);
        if (refreshButton != null) refreshButton.onClick.AddListener(Refresh);
        if (backButton    != null) backButton.onClick.AddListener(Back);
        Refresh();
    }

    void Back()
    {
        if (menu != null) menu.ShowMain();
        else gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (pvpText != null)
        {
            int p1 = MatchHistory.PvPWinsOf(1);
            int p2 = MatchHistory.PvPWinsOf(2);
            int p3 = MatchHistory.PvPWinsOf(3);
            int dr = MatchHistory.PvPDraws();
            int tt = MatchHistory.PvPTotal();
            pvpText.text =
                $"PvP 战绩 (共 {tt} 场)\n" +
                $"P1 胜场: {p1}\n" +
                $"P2 胜场: {p2}\n" +
                $"P3 胜场: {p3}\n" +
                $"平局: {dr}";
        }
        if (coopText != null)
        {
            float c2 = MatchHistory.GetCoopBest(2);
            float c3 = MatchHistory.GetCoopBest(3);
            coopText.text =
                $"合作最长存活 (变态难度)\n" +
                $"2 人组: {FormatTime(c2)}\n" +
                $"3 人组: {FormatTime(c3)}";
        }
    }

    static string FormatTime(float s)
    {
        if (s <= 0f) return "—";
        int min = Mathf.FloorToInt(s / 60f);
        int sec = Mathf.FloorToInt(s % 60f);
        return $"{min:00}:{sec:00}";
    }

    void OnClear()
    {
        MatchHistory.ClearAll();
        Refresh();
    }
}
