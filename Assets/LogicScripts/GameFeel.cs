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

    [Header("Impact FX")]
    [SerializeField] ParticleSystem impactParticle;
    [SerializeField] float impactShakeMinVelocity = 4f;
    [SerializeField] float impactShakeAmp         = 0.06f;
    [SerializeField] float impactShakeDur         = 0.08f;

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

    public void PlayImpact(Vector2 worldPos, float relativeVelocity)
    {
        if (impactParticle != null && relativeVelocity >= 1f)
        {
            impactParticle.transform.position = worldPos;
            var emit = new ParticleSystem.EmitParams { applyShapeToPosition = true };
            impactParticle.Emit(emit, Mathf.Clamp(Mathf.RoundToInt(relativeVelocity * 1.5f), 4, 24));
        }
        if (relativeVelocity >= impactShakeMinVelocity)
        {
            float strength = Mathf.Clamp01(relativeVelocity / 12f);
            StartShake(impactShakeAmp * (0.5f + strength), impactShakeDur);
        }
    }

    IEnumerator SlowMoRoutine(Action onComplete)
    {
        Time.timeScale = endSlowMoScale;
        yield return new WaitForSecondsRealtime(endSlowMoDuration);
        onComplete?.Invoke();
    }

    void StartShake(float amp, float dur)
    {
        if (amp > currentShakeAmp || shakeTimeLeft <= 0f)
        {
            currentShakeAmp = amp;
            currentShakeDur = dur;
            shakeTimeLeft   = dur;
        }
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
