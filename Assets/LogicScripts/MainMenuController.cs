using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] string gameSceneName = "SampleScene";

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button howToPlayButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button historyButton;
    [SerializeField] Button quitButton;

    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject howToPlayPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject historyPanel;

    [Header("Selectors (always visible on main panel)")]
    [SerializeField] PlayerCountSelector playerCountSelector;
    [SerializeField] MapSelectController mapSelectController;

    void Start()
    {
        GameSession.Ensure();

        if (startButton      != null) startButton.onClick.AddListener(StartGame);
        if (howToPlayButton  != null) howToPlayButton.onClick.AddListener(OpenHowToPlay);
        if (settingsButton   != null) settingsButton.onClick.AddListener(OpenSettings);
        if (historyButton    != null) historyButton.onClick.AddListener(OpenHistory);
        if (quitButton       != null) quitButton.onClick.AddListener(Quit);

        ShowMain();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuBgm();
    }

    public void ShowMain()
    {
        SetActive(mainPanel,       true);
        SetActive(howToPlayPanel,  false);
        SetActive(settingsPanel,   false);
        SetActive(historyPanel,    false);
    }

    public void OpenHowToPlay()
    {
        SetActive(mainPanel,       false);
        SetActive(howToPlayPanel,  true);
        SetActive(settingsPanel,   false);
        SetActive(historyPanel,    false);
    }

    public void OpenSettings()
    {
        SetActive(mainPanel,       false);
        SetActive(howToPlayPanel,  false);
        SetActive(settingsPanel,   true);
        SetActive(historyPanel,    false);
    }

    public void OpenHistory()
    {
        SetActive(mainPanel,       false);
        SetActive(howToPlayPanel,  false);
        SetActive(settingsPanel,   false);
        SetActive(historyPanel,    true);
    }

    public void StartGame()
    {
        if (GameSession.Instance != null && GameSession.Instance.SelectedArena == null
            && mapSelectController != null)
        {
            mapSelectController.SelectDefaultIfNone();
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void SetActive(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }
}
