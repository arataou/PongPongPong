using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] TMP_Text stateLabel;
    [SerializeField] TMP_Text aiLabel;
    [SerializeField] TMP_Text coopTimeLabel;
    [SerializeField] float warningThreshold = 3f;
    [SerializeField] Color normalColor  = Color.white;
    [SerializeField] Color warningColor = Color.red;

    void Update()
    {
        bool isCoop = GameSession.Instance != null && GameSession.Instance.Mode == GameMode.Coop;
        UpdateStateLabel();
        UpdateAILabel();
        UpdateCoopLabel(isCoop);
    }

    void UpdateStateLabel()
    {
        if (stateLabel == null) return;
        var roul = StateRouletteController.Instance;
        if (roul == null) { stateLabel.text = ""; return; }

        float t = Mathf.Max(0f, roul.TimeUntilNext);
        stateLabel.text = $"{Mathf.CeilToInt(t)} 秒后切换状态";
    }

    void UpdateAILabel()
    {
        if (aiLabel == null) return;
        var sp = AISpawner.Instance;
        if (sp == null) { aiLabel.text = ""; return; }

        float t = Mathf.Max(0f, sp.TimeUntilNext);
        aiLabel.text = $"{Mathf.CeilToInt(t)} 秒后 AI";

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
        coopTimeLabel.text = $"存活 {min:00}:{sec:00}";
    }
}
