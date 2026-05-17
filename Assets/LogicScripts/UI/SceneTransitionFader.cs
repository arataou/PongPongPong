using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionFader : MonoBehaviour
{
    static SceneTransitionFader instance;

    CanvasGroup canvasGroup;
    bool isTransitioning;

    public static SceneTransitionFader Instance
    {
        get
        {
            if (instance == null) CreateInstance();
            return instance;
        }
    }

    public static bool IsTransitioning => instance != null && instance.isTransitioning;

    static void CreateInstance()
    {
        var go = new GameObject("SceneTransitionFader", typeof(RectTransform));
        instance = go.AddComponent<SceneTransitionFader>();
        instance.BuildOverlay();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (canvasGroup == null) BuildOverlay();
        DontDestroyOnLoad(gameObject);
    }

    public void FadeToScene(string sceneName, float fadeOutDuration = 0.35f, float fadeInDuration = 0.35f)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeToSceneRoutine(sceneName, fadeOutDuration, fadeInDuration));
    }

    IEnumerator FadeToSceneRoutine(string sceneName, float fadeOutDuration, float fadeInDuration)
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = true;

        yield return Fade(1f, fadeOutDuration);

        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op != null)
        {
            while (!op.isDone)
                yield return null;
        }

        yield return null;
        yield return Fade(0f, fadeInDuration);

        canvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    IEnumerator Fade(float target, float duration)
    {
        float start = canvasGroup.alpha;
        if (duration <= 0f)
        {
            canvasGroup.alpha = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = k * k * (3f - 2f * k);
            canvasGroup.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    void BuildOverlay()
    {
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        if (gameObject.GetComponent<CanvasScaler>() == null)
            gameObject.AddComponent<CanvasScaler>();
        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var image = gameObject.GetComponent<Image>();
        if (image == null) image = gameObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        var rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
