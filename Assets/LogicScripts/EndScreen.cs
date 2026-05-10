using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Text       resultText;
    [SerializeField] Button     restartButton;
    [SerializeField] Button     quitButton;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Start()
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.OnMatchEnded += HandleEnd;
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (quitButton    != null) quitButton.onClick.AddListener(Quit);
    }

    void OnDestroy()
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.OnMatchEnded -= HandleEnd;
    }

    void HandleEnd(MatchManager.Outcome o)
    {
        if (panel != null) panel.SetActive(true);
        if (resultText != null)
        {
            switch (o)
            {
                case MatchManager.Outcome.P1Wins: resultText.text = "P1 胜"; break;
                case MatchManager.Outcome.P2Wins: resultText.text = "P2 胜"; break;
                case MatchManager.Outcome.Draw:   resultText.text = "平局";  break;
                default:                           resultText.text = "";       break;
            }
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
