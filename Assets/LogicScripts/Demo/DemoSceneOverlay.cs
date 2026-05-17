using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoSceneOverlay : MonoBehaviour
{
    [SerializeField] GameObject aiPrefab;
    [SerializeField] int        botCount = 4;
    [SerializeField] string     mainMenuSceneName = "MainMenu";
    [SerializeField] GameObject demoHud;

    void Start()
    {
        bool isDemo = GameSession.Instance != null && GameSession.Instance.IsDemo;
        if (demoHud != null) demoHud.SetActive(isDemo);
        if (!isDemo) return;

        var players = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
        foreach (var p in players)
            if (p != null) Destroy(p.gameObject);

        if (aiPrefab != null && ArenaBounds.Instance != null)
        {
            var b = ArenaBounds.Instance;
            for (int i = 0; i < botCount; i++)
            {
                float fx = Mathf.Lerp(b.Min.x + 1f, b.Max.x - 1f, (i + 1f) / (botCount + 1));
                float fy = Mathf.Lerp(b.Min.y + 1f, b.Max.y - 1f, ((i + 1f) * 0.6f) % 1f);
                Instantiate(aiPrefab, new Vector3(fx, fy, 0f), Quaternion.identity);
            }
        }
    }

    void Update()
    {
        if (GameSession.Instance == null || !GameSession.Instance.IsDemo) return;
        if (Input.anyKeyDown)
        {
            GameSession.Instance.IsDemo = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
