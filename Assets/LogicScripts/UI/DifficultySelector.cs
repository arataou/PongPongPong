using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] Button   easyButton;
    [SerializeField] Button   normalButton;
    [SerializeField] Button   hardButton;
    [SerializeField] TMP_Text currentLabel;
    [SerializeField] TMP_Text profilePreview;

    [SerializeField] Color selectedColor   = new Color(0.3f, 0.7f, 1f, 1f);
    [SerializeField] Color unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    void Start()
    {
        GameSession.Ensure();
        if (easyButton   != null) easyButton.onClick.AddListener(()   => Set(AIDifficulty.Easy));
        if (normalButton != null) normalButton.onClick.AddListener(() => Set(AIDifficulty.Normal));
        if (hardButton   != null) hardButton.onClick.AddListener(()   => Set(AIDifficulty.Hard));
        Refresh();
    }

    public void Set(AIDifficulty d)
    {
        if (GameSession.Instance != null) GameSession.Instance.Difficulty = d;
        Refresh();
    }

    void Refresh()
    {
        var d = GameSession.Instance != null ? GameSession.Instance.Difficulty : AIDifficulty.Normal;
        if (currentLabel != null) currentLabel.text = $"难度: {DifficultyProfile.Label(d)}";

        Paint(easyButton,   d == AIDifficulty.Easy);
        Paint(normalButton, d == AIDifficulty.Normal);
        Paint(hardButton,   d == AIDifficulty.Hard);

        if (profilePreview != null)
        {
            var p = DifficultyProfile.Get(d);
            profilePreview.text =
                $"首 AI: {p.firstSpawnDelay:0}s  ·  间隔: {p.spawnInterval:0}s\n" +
                $"AI 速度: x{p.speedMul:0.0}  ·  AI 质量: x{p.massMul:0.0}";
        }
    }

    void Paint(Button btn, bool selected)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = selected ? selectedColor : unselectedColor;
    }
}
