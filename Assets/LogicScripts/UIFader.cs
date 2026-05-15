using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    [SerializeField] float fadeInDuration  = 0.25f;
    [SerializeField] float fadeOutDuration = 0.15f;
    [SerializeField] bool  fadeInOnEnable  = true;
    [SerializeField] bool  ignoreTimeScale = true;

    CanvasGroup cg;
    Coroutine   running;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (!fadeInOnEnable) return;
        cg.alpha = 0f;
        Run(1f, fadeInDuration);
    }

    public void FadeIn()  => Run(1f, fadeInDuration);
    public void FadeOut() => Run(0f, fadeOutDuration);

    void Run(float target, float duration)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Co(target, duration));
    }

    System.Collections.IEnumerator Co(float target, float duration)
    {
        if (duration <= 0f) { cg.alpha = target; yield break; }
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        cg.alpha = target;
    }
}
