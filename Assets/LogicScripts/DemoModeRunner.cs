using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoModeRunner : MonoBehaviour
{
    [SerializeField] float idleSeconds  = 30f;
    [SerializeField] string gameSceneName = "SampleScene";

    float idleTimer;

    void Update()
    {
        if (GameSession.Instance == null) return;
        if (GameSession.Instance.IsDemo) return;

        bool anyInput = Input.anyKeyDown || Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f;
        if (anyInput || Input.mousePresent && Input.GetAxis("Mouse X") != 0f)
        {
            idleTimer = 0f;
            return;
        }

        idleTimer += Time.unscaledDeltaTime;
        if (idleTimer >= idleSeconds)
        {
            idleTimer = 0f;
            GameSession.Instance.IsDemo = true;
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
