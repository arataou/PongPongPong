using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCountSelector : MonoBehaviour
{
    [SerializeField] Button   twoPlayerButton;
    [SerializeField] Button   threePlayerButton;
    [SerializeField] TMP_Text currentLabel;

    [Header("Visual Feedback")]
    [SerializeField] Color selectedColor   = new Color(0.3f, 0.7f, 1f, 1f);
    [SerializeField] Color unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    void Start()
    {
        GameSession.Ensure();
        if (twoPlayerButton   != null) twoPlayerButton.onClick.AddListener(() => Set(2));
        if (threePlayerButton != null) threePlayerButton.onClick.AddListener(() => Set(3));
        Refresh();
    }

    public void Set(int n)
    {
        if (GameSession.Instance != null) GameSession.Instance.PlayerCount = n;
        Refresh();
    }

    void Refresh()
    {
        int c = GameSession.Instance != null ? GameSession.Instance.PlayerCount : 2;
        if (currentLabel != null) currentLabel.text = $"当前: {c} 人";
        Paint(twoPlayerButton,   c == 2);
        Paint(threePlayerButton, c == 3);
    }

    void Paint(Button btn, bool selected)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = selected ? selectedColor : unselectedColor;
    }
}
