using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] Slider   bgmSlider;
    [SerializeField] Slider   sfxSlider;
    [SerializeField] TMP_Text bgmValueLabel;
    [SerializeField] TMP_Text sfxValueLabel;
    [SerializeField] Button   backButton;
    [SerializeField] MainMenuController menu;

    void OnEnable()
    {
        GameSession.Ensure();
        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(GameSession.Instance.BgmVolume);
            UpdateLabel(bgmValueLabel, bgmSlider.value);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(GameSession.Instance.SfxVolume);
            UpdateLabel(sfxValueLabel, sfxSlider.value);
        }
    }

    void Start()
    {
        if (bgmSlider  != null) bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        if (sfxSlider  != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        if (backButton != null) backButton.onClick.AddListener(Back);
    }

    void OnBgmChanged(float v)
    {
        if (GameSession.Instance != null) GameSession.Instance.BgmVolume = v;
        UpdateLabel(bgmValueLabel, v);
    }

    void OnSfxChanged(float v)
    {
        if (GameSession.Instance != null) GameSession.Instance.SfxVolume = v;
        UpdateLabel(sfxValueLabel, v);
    }

    void Back()
    {
        if (menu != null) menu.ShowMain();
        else gameObject.SetActive(false);
    }

    static void UpdateLabel(TMP_Text label, float v)
    {
        if (label != null) label.text = $"{Mathf.RoundToInt(v * 100f)}";
    }
}
