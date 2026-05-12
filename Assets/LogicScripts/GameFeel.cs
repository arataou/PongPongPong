using System;
using System.Collections;
using UnityEngine;

public class GameFeel : MonoBehaviour
{
    [Header("Death Feel (per-player death)")]
    [SerializeField] float deathShakeAmp = 0.15f;
    [SerializeField] float deathShakeDur = 0.18f;

    [Header("Match End Feel (last player dies)")]
    [SerializeField] float endShakeAmp       = 0.4f;
    [SerializeField] float endShakeDur       = 0.45f;
    [SerializeField] float endSlowMoScale    = 0.25f;
    [SerializeField] float endSlowMoDuration = 0.45f;

    public static GameFeel Instance { get; private set; }

    Vector3 baseLocalPos;
    float   shakeTimeLeft;
    float   currentShakeAmp;
    float   currentShakeDur;

    void Awake()
    {
        Instance = this;
        baseLocalPos = transform.localPosition;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayDeath()
    {
        StartShake(deathShakeAmp, deathShakeDur);
    }

    public void PlayMatchEnd(Action onComplete)
    {
        StartShake(endShakeAmp, endShakeDur);
        StartCoroutine(SlowMoRoutine(onComplete));
    }

    IEnumerator SlowMoRoutine(Action onComplete)
    {
        Time.timeScale = endSlowMoScale;
        yield return new WaitForSecondsRealtime(endSlowMoDuration);
        onComplete?.Invoke();
    }

    void StartShake(float amp, float dur)
    {
        currentShakeAmp = amp;
        currentShakeDur = dur;
        shakeTimeLeft   = dur;
    }

    void LateUpdate()
    {
        if (shakeTimeLeft <= 0f)
        {
            transform.localPosition = baseLocalPos;
            return;
        }

        shakeTimeLeft -= Time.unscaledDeltaTime;
        float falloff = Mathf.Clamp01(shakeTimeLeft / currentShakeDur);
        Vector2 offset = UnityEngine.Random.insideUnitCircle * currentShakeAmp * falloff;
        transform.localPosition = baseLocalPos + new Vector3(offset.x, offset.y, 0f);
    }
}
