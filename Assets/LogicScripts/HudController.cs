using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] TMP_Text stateLabel;
    [SerializeField] TMP_Text aiLabel;
    [SerializeField] float warningThreshold = 3f;
    [SerializeField] Color normalColor  = Color.white;
    [SerializeField] Color warningColor = Color.red;

    void Update()
    {
        UpdateStateLabel();
        UpdateAILabel();
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
}
