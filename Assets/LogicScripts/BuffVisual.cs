using System.Collections;
using TMPro;
using UnityEngine;

// 挂在 PlayerBase 上的 buff 表现层(运行时自动 AddComponent)。
// 负责:拾取瞬间反馈 + 持续期间光环 + Shockwave 圈扩散。
[RequireComponent(typeof(PlayerBase))]
[DisallowMultipleComponent]
public class BuffVisual : MonoBehaviour
{
    const int CircleSegments = 48;

    PlayerBase       player;
    SpriteRenderer   ballSprite;
    Transform        spriteTransform;
    Color            spriteOriginalColor = Color.white;
    Vector3          spriteOriginalScale = Vector3.one;
    CircleCollider2D circleCol;

    LineRenderer ringSpeed;
    LineRenderer ringHeavy;
    LineRenderer ringReverse;
    LineRenderer ringInvincible;

    PickupBuff   lastSeenBuffs = PickupBuff.None;
    Coroutine    flashCoroutine;

    void Awake()
    {
        player    = GetComponent<PlayerBase>();
        circleCol = GetComponent<CircleCollider2D>();
        ballSprite = BallVisualUtility.EnsureChildSpriteRenderer(gameObject);
        if (ballSprite != null)
        {
            spriteTransform     = ballSprite.transform;
            spriteOriginalColor = ballSprite.color;
            spriteOriginalScale = spriteTransform.localScale;
        }

        ringSpeed      = CreateRing("RingSpeed",      1.2f, new Color(1f,   0.9f,  0.3f, 1f));
        ringHeavy      = CreateRing("RingHeavy",      1.4f, new Color(0.6f, 0.3f,  0.9f, 1f));
        ringReverse    = CreateRing("RingReverse",    1.6f, new Color(0.95f,0.25f, 0.25f, 1f));
        ringInvincible = CreateRing("RingInvincible", 1.8f, new Color(1f,   0.95f, 0.6f, 1f));
    }

    LineRenderer CreateRing(string n, float radiusMul, Color color)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop          = true;
        lr.startWidth    = 0.06f;
        lr.endWidth      = 0.06f;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = color;
        lr.endColor      = color;
        lr.positionCount = CircleSegments;
        lr.sortingOrder  = 3;
        lr.enabled       = false;

        float baseR = circleCol != null ? circleCol.radius : 0.5f;
        float r = baseR * radiusMul;
        for (int i = 0; i < CircleSegments; i++)
        {
            float a = (i / (float)CircleSegments) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0));
        }
        return lr;
    }

    void Update()
    {
        if (player == null) return;
        var cur = player.ActiveBuffs;
        if (cur != lastSeenBuffs)
        {
            ringSpeed.enabled      = (cur & PickupBuff.Speed)      != 0;
            ringHeavy.enabled      = (cur & PickupBuff.Heavy)      != 0;
            ringReverse.enabled    = (cur & PickupBuff.Reverse)    != 0;
            ringInvincible.enabled = (cur & PickupBuff.Invincible) != 0;
            lastSeenBuffs = cur;
        }

        float t = Time.time;

        if (ringSpeed.enabled)
        {
            float pulse = 1f + 0.06f * Mathf.Sin(t * 6f);
            ringSpeed.transform.localScale = Vector3.one * pulse;
        }
        if (ringHeavy.enabled)
        {
            float pulse = 1f + 0.06f * Mathf.Sin(t * 4f);
            ringHeavy.transform.localScale = Vector3.one * pulse;
        }
        if (ringReverse.enabled)
        {
            // 红圈强烈闪烁,提醒玩家手柄被反向
            float a = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(t * 9f));
            var c = ringReverse.startColor; c.a = a;
            ringReverse.startColor = c; ringReverse.endColor = c;
            float pulse = 1f + 0.1f * Mathf.Sin(t * 9f);
            ringReverse.transform.localScale = Vector3.one * pulse;
        }
        if (ringInvincible.enabled)
        {
            float a = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(t * 7f));
            var c = ringInvincible.startColor; c.a = a;
            ringInvincible.startColor = c; ringInvincible.endColor = c;
        }
    }

    public void OnPickup(PickupBuff buff)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(PickupFlashAndBounce());

        SpawnFloatingText(LabelFor(buff), ColorForBuff(buff));

        if ((buff & PickupBuff.Shockwave) != 0)
        {
            StartCoroutine(PlayShockwaveRing());
        }
    }

    IEnumerator PickupFlashAndBounce()
    {
        if (ballSprite == null || spriteTransform == null) yield break;

        const float dur = 0.3f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            ballSprite.color = Color.Lerp(Color.white, spriteOriginalColor, k);
            float bump = 1f + 0.25f * Mathf.Sin(k * Mathf.PI);
            spriteTransform.localScale = spriteOriginalScale * bump;
            yield return null;
        }
        ballSprite.color = spriteOriginalColor;
        spriteTransform.localScale = spriteOriginalScale;
    }

    IEnumerator PlayShockwaveRing()
    {
        var go = new GameObject("ShockwaveRingFX");
        go.transform.position = transform.position;
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop          = true;
        lr.startWidth    = 0.1f;
        lr.endWidth      = 0.1f;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        var col = new Color(1f, 0.45f, 0.2f, 1f);
        lr.startColor    = col;
        lr.endColor      = col;
        lr.positionCount = CircleSegments;
        lr.sortingOrder  = 4;

        const float dur = 0.5f;
        const float maxRadius = 4.5f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (lr == null) yield break;
            float k = t / dur;
            float r = Mathf.Lerp(0f, maxRadius, k);
            for (int i = 0; i < CircleSegments; i++)
            {
                float a = (i / (float)CircleSegments) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0));
            }
            var c = col; c.a = 1f - k;
            lr.startColor = c; lr.endColor = c;
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    void SpawnFloatingText(string text, Color color)
    {
        if (string.IsNullOrEmpty(text)) return;

        var go = new GameObject("BuffFloatText");
        go.transform.position = transform.position + Vector3.up * 0.6f;

        var tmp = go.AddComponent<TextMeshPro>();
        var font = player != null ? player.ChineseFont : null;
        if (font == null) font = TMP_Settings.defaultFontAsset;
        if (font != null) tmp.font = font;
        tmp.text          = text;
        tmp.fontSize      = 4;
        tmp.color         = color;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.sortingOrder  = 10;
        tmp.fontStyle     = FontStyles.Bold;

        StartCoroutine(FloatTextRoutine(go, tmp));
    }

    static IEnumerator FloatTextRoutine(GameObject go, TextMeshPro tmp)
    {
        if (go == null || tmp == null) yield break;
        const float dur = 0.8f;
        Vector3 start = go.transform.position;
        Vector3 end   = start + Vector3.up * 1.2f;
        Color baseColor = tmp.color;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (go == null) yield break;
            float k = t / dur;
            go.transform.position = Vector3.Lerp(start, end, k);
            var c = baseColor; c.a = 1f - k;
            tmp.color = c;
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    string LabelFor(PickupBuff buff)
    {
        bool zh = player != null && player.ChineseFont != null;
        if ((buff & PickupBuff.Shockwave)  != 0) return zh ? "波!"   : "BOOM!";
        if ((buff & PickupBuff.Invincible) != 0) return zh ? "盾!"   : "SHIELD!";
        if ((buff & PickupBuff.Reverse)    != 0) return zh ? "反!"   : "REV!";
        if ((buff & PickupBuff.Heavy)      != 0) return zh ? "重!"   : "HVY!";
        if ((buff & PickupBuff.Speed)      != 0) return zh ? "速!"   : "SPD!";
        return "";
    }

    static Color ColorForBuff(PickupBuff b)
    {
        if ((b & PickupBuff.Shockwave)  != 0) return new Color(1f,   0.5f,  0.2f);
        if ((b & PickupBuff.Invincible) != 0) return new Color(1f,   0.9f,  0.4f);
        if ((b & PickupBuff.Reverse)    != 0) return new Color(0.95f,0.3f,  0.3f);
        if ((b & PickupBuff.Heavy)      != 0) return new Color(0.75f,0.45f, 1f);
        if ((b & PickupBuff.Speed)      != 0) return new Color(1f,   0.95f, 0.4f);
        return Color.white;
    }
}
