using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    [SerializeField] Button playButton;
    [SerializeField] Button setupBackButton;

    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject matchSetupPanel;
    [SerializeField] GameObject howToPlayPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject historyPanel;

    [Header("Selectors")]
    [SerializeField] PlayerCountSelector playerCountSelector;
    [SerializeField] MapSelectController mapSelectController;

    bool startingGame;

    void Start()
    {
        GameSession.Ensure();

        if (startButton      != null) startButton.onClick.AddListener(OpenMatchSetup);
        if (howToPlayButton  != null) howToPlayButton.onClick.AddListener(OpenHowToPlay);
        if (settingsButton   != null) settingsButton.onClick.AddListener(OpenSettings);
        if (historyButton    != null) historyButton.onClick.AddListener(OpenHistory);
        if (quitButton       != null) quitButton.onClick.AddListener(Quit);
        if (playButton       != null) playButton.onClick.AddListener(StartGame);
        if (setupBackButton  != null) setupBackButton.onClick.AddListener(ShowMain);

        ShowMain();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuBgm();
    }

    public void ShowMain()
    {
        ShowPanel(mainPanel, startButton);
    }

    public void OpenMatchSetup()
    {
        // Retain support for scenes made with the original menu builder.
        if (matchSetupPanel == null) { StartGame(); return; }
        ShowPanel(matchSetupPanel, playButton);
    }

    public void OpenHowToPlay()
    {
        ShowPanel(howToPlayPanel);
    }

    public void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void OpenHistory()
    {
        ShowPanel(historyPanel);
    }

    void ShowPanel(GameObject panel, Button focus = null)
    {
        SetActive(mainPanel, panel == mainPanel);
        SetActive(matchSetupPanel, panel == matchSetupPanel);
        SetActive(howToPlayPanel, panel == howToPlayPanel);
        SetActive(settingsPanel, panel == settingsPanel);
        SetActive(historyPanel, panel == historyPanel);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(focus != null ? focus.gameObject : null);
    }

    void Update()
    {
        if (matchSetupPanel != null && matchSetupPanel.activeSelf &&
            Input.GetKeyDown(KeyCode.Escape)) ShowMain();
    }

    public void StartGame()
    {
        if (startingGame || SceneTransitionFader.IsTransitioning) return;
        startingGame = true;
        if (playButton != null) playButton.interactable = false;
        else if (startButton != null) startButton.interactable = false;

        if (GameSession.Instance != null && GameSession.Instance.SelectedArena == null
            && mapSelectController != null)
        {
            mapSelectController.SelectDefaultIfNone();
        }
        SceneTransitionFader.Instance.FadeToScene(gameSceneName);
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
