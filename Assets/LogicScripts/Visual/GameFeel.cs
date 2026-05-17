using System;
using System.Collections;
using UnityEngine;

public class GameFeel : MonoBehaviour
{
    const int CircleSegments = 64;

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
    [SerializeField] float impactRingRadius       = 0.8f;
    [SerializeField] float deathRingRadius        = 1.35f;

    public static GameFeel Instance { get; private set; }

    Vector3 baseLocalPos;
    float   shakeTimeLeft;
    float   currentShakeAmp;
    float   currentShakeDur;

    void Awake()
    {
        Instance = this;
        baseLocalPos = transform.localPosition;

        if (GetComponent<BoundaryDangerVisual>() == null)
            gameObject.AddComponent<BoundaryDangerVisual>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayDeath()
    {
        StartShake(deathShakeAmp, deathShakeDur);
    }

    public void PlayDeath(Vector2 worldPos, Color accentColor)
    {
        PlayDeath();
        StartCoroutine(DeathBurstRoutine(worldPos, accentColor));
    }

    public void PlayMatchEnd(Action onComplete)
    {
        StartShake(endShakeAmp, endShakeDur);
        StartCoroutine(SlowMoRoutine(onComplete));
    }

    public void PlayImpact(Vector2 worldPos, float relativeVelocity)
    {
        PlayImpact(worldPos, relativeVelocity, Color.white);
    }

    public void PlayImpact(Vector2 worldPos, float relativeVelocity, Color accentColor)
    {
        if (impactParticle != null && relativeVelocity >= 1f)
        {
            impactParticle.transform.position = worldPos;
            var emit = new ParticleSystem.EmitParams
            {
                applyShapeToPosition = true,
                startColor = Color.Lerp(Color.white, accentColor, 0.75f)
            };
            impactParticle.Emit(emit, Mathf.Clamp(Mathf.RoundToInt(relativeVelocity * 1.5f), 4, 24));
        }
        if (relativeVelocity >= 1f)
            StartCoroutine(ImpactRingRoutine(worldPos, relativeVelocity, accentColor, impactRingRadius, 0.08f, 0.22f));
        if (relativeVelocity >= impactShakeMinVelocity)
        {
            float strength = Mathf.Clamp01(relativeVelocity / 12f);
            StartShake(impactShakeAmp * (0.5f + strength), impactShakeDur);
        }
    }

    IEnumerator DeathBurstRoutine(Vector2 worldPos, Color accentColor)
    {
        for (int i = 0; i < 3; i++)
        {
            float radius = deathRingRadius * (1f + i * 0.22f);
            StartCoroutine(ImpactRingRoutine(worldPos, 8f, accentColor, radius, 0.11f - i * 0.018f, 0.36f + i * 0.04f));
            yield return new WaitForSecondsRealtime(0.045f);
        }
    }

    IEnumerator ImpactRingRoutine(Vector2 worldPos, float relativeVelocity, Color accentColor, float maxRadius, float width, float dur)
    {
        if (this == null) yield break;  // GameFeel 已被销毁 (场景关闭路径), 不再 spawn 顶层 GO
        var go = new GameObject("ImpactRingFX");
        go.transform.SetParent(transform, false);  // 跟 GameFeel 生命周期一致, 场景关闭时自动跟随销毁
        go.transform.position = worldPos;

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = CircleSegments;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 8;

        float velocityScale = Mathf.Clamp01(relativeVelocity / 12f);
        Color c = Color.Lerp(Color.white, accentColor, 0.82f);
        c.a = Mathf.Lerp(0.42f, 0.78f, velocityScale);

        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            if (lr == null) yield break;

            float k = Mathf.Clamp01(t / dur);
            float eased = 1f - Mathf.Pow(1f - k, 2f);
            float r = Mathf.Lerp(maxRadius * 0.18f, maxRadius, eased);
            for (int i = 0; i < CircleSegments; i++)
            {
                float a = (i / (float)CircleSegments) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
            }

            Color fade = c;
            fade.a *= 1f - k;
            lr.startColor = fade;
            lr.endColor = fade;
            yield return null;
        }

        if (go != null) Destroy(go);
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
