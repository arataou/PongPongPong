using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text   resultText;
    [SerializeField] TMP_Text   coopDetailText;
    [SerializeField] Button     restartButton;
    [SerializeField] Button     quitButton;
    [SerializeField] Button     menuButton;
    [SerializeField] string     mainMenuSceneName = "MainMenu";

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (coopDetailText != null) coopDetailText.gameObject.SetActive(false);
    }

    void Start()
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.OnMatchEnded += HandleEnd;
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (quitButton    != null) quitButton.onClick.AddListener(Quit);
        if (menuButton    != null) menuButton.onClick.AddListener(BackToMenu);
    }

    void OnDestroy()
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.OnMatchEnded -= HandleEnd;
    }

    void Update()
    {
        if (MatchManager.Instance == null) return;
        if (MatchManager.Instance.CurrentOutcome == MatchManager.Outcome.Ongoing) return;
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.M)) BackToMenu();
    }

    void HandleEnd(MatchManager.Outcome o)
    {
        if (panel != null) panel.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxWin);

        if (coopDetailText != null) coopDetailText.gameObject.SetActive(false);
        if (resultText == null) return;

        switch (o)
        {
            case MatchManager.Outcome.Win:
                resultText.text = $"P{MatchManager.Instance.WinningPlayerIndex} 胜";
                break;
            case MatchManager.Outcome.Draw:
                resultText.text = "平局";
                break;
            case MatchManager.Outcome.CoopEnd:
                ShowCoopResult();
                break;
        }
    }

    void ShowCoopResult()
    {
        var mm = MatchManager.Instance;
        float t   = mm.ElapsedTime;
        int   min = Mathf.FloorToInt(t / 60f);
        int   sec = Mathf.FloorToInt(t % 60f);
        resultText.text = $"存活 {min:00}:{sec:00}";

        if (coopDetailText == null) return;
        coopDetailText.gameObject.SetActive(true);

        var sess = GameSession.Instance;
        bool isHard = sess != null && sess.Difficulty == AIDifficulty.Hard;
        if (!isHard)
        {
            coopDetailText.text = "仅变态难度记录排行";
            return;
        }

        if (mm.CoopNewRecord)
        {
            coopDetailText.text = $"新纪录! (此前 {FormatTime(mm.CoopBestBefore)})";
        }
        else
        {
            coopDetailText.text = $"历史最长: {FormatTime(MatchHistory.GetCoopBest(sess.PlayerCount))}";
        }
    }

    static string FormatTime(float s)
    {
        if (s <= 0f) return "—";
        int min = Mathf.FloorToInt(s / 60f);
        int sec = Mathf.FloorToInt(s % 60f);
        return $"{min:00}:{sec:00}";
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
        BackToMenu();
    }
}
