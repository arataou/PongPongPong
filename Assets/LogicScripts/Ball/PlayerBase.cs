using System.Collections;
using TMPro;
using UnityEngine;

public enum BallState { Normal, Fast, Big, Small }

[System.Flags]
public enum PickupBuff
{
    None       = 0,
    Speed      = 1,
    Heavy      = 2,
    Reverse    = 4,
    Invincible = 8,
    Shockwave  = 16,
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public abstract class PlayerBase : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] float baseMoveSpeed   = 5f;
    [SerializeField] float baseAcceleration = 25f;
    [SerializeField] float baseMass        = 1f;

    [Header("State Multipliers")]
    [SerializeField] float fastSpeedMul  = 2f;
    [SerializeField] float fastAccelMul  = 2f;
    [SerializeField] float bigScaleMul   = 1.6f;
    [SerializeField] float bigMassMul    = 2.56f;
    [SerializeField] float smallScaleMul = 0.6f;
    [SerializeField] float smallMassMul  = 0.36f;

    [Header("Pickup Buff")]
    [SerializeField] float speedBuffMul       = 1.5f;
    [SerializeField] float heavyBuffMul       = 3f;
    [SerializeField] float buffDuration       = 5f;
    [SerializeField] float invincibleDuration = 3f;
    [SerializeField] float shockwaveRadius    = 8f;
    [SerializeField] float shockwaveForce     = 70f;

    [Header("Visual (optional)")]
    [Tooltip("Child GameObject shown only while in Fast state (the bright outline ring).")]
    [SerializeField] GameObject fastStateOutline;
    [Tooltip("Child GameObject shown only while invincible (an aura/halo).")]
    [SerializeField] GameObject invincibleAura;
    [Tooltip("中文飘字字体 (拖 MSYH SDF)。留空则飘字降级显示英文缩写。")]
    [SerializeField] TMP_FontAsset chineseFont;

    [Header("Audio")]
    [SerializeField] float impactCooldown = 0.05f;

    Rigidbody2D rb;
    Vector2 moveDir;

    float currentSpeed;
    float currentAccel;

    CircleCollider2D    circleCol;
    BuffVisual          buffVisual;
    SpriteRenderer      ballSprite;
    Transform           ballVisualTransform;
    Vector3             ballVisualBaseScale = Vector3.one;
    Coroutine           impactSquashCoroutine;
    PickupBuff          activeBuffs = PickupBuff.None;
    float speedTimer;
    float heavyTimer;
    float reverseTimer;
    float invincibleTimer;
    float lastImpactTime;
    readonly System.Collections.Generic.List<Collider2D> invincibleIgnoredCols = new System.Collections.Generic.List<Collider2D>();

    public BallState State { get; private set; } = BallState.Normal;
    public bool IsPlayer => true;
    public bool IsInvincible => (activeBuffs & PickupBuff.Invincible) != 0;
    public PickupBuff    ActiveBuffs => activeBuffs;
    public TMP_FontAsset ChineseFont => chineseFont;
    public abstract int PlayerIndex { get; }

    static int  s_playerLayer       = -1;
    static int  s_invinciblePlayerLayer = -1;
    static bool s_layerMatrixInited = false;

    public static int PlayerLayer
    {
        get
        {
            EnsureLayersInited();
            return s_playerLayer;
        }
    }

    static void EnsureLayersInited()
    {
        if (s_layerMatrixInited) return;
        s_playerLayer           = LayerMask.NameToLayer("Player");
        s_invinciblePlayerLayer = LayerMask.NameToLayer("InvinciblePlayer");
        if (s_playerLayer >= 0 && s_invinciblePlayerLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(s_invinciblePlayerLayer, s_playerLayer, true);
            Physics2D.IgnoreLayerCollision(s_invinciblePlayerLayer, s_invinciblePlayerLayer, true);
        }
        s_layerMatrixInited = true;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.linearDamping = 0.8f;
        circleCol = GetComponent<CircleCollider2D>();
        ballSprite = BallVisualUtility.EnsureChildSpriteRenderer(gameObject);
        if (ballSprite != null)
        {
            ballVisualTransform = ballSprite.transform != transform ? ballSprite.transform : null;
            if (ballVisualTransform != null)
                ballVisualBaseScale = ballVisualTransform.localScale;
        }

        var bouncy = Resources.Load<PhysicsMaterial2D>("Bouncy");
        if (bouncy != null) circleCol.sharedMaterial = bouncy;
        EnsureLayersInited();
        if (s_playerLayer >= 0) gameObject.layer = s_playerLayer;
        ApplyState(BallState.Normal);
        if (invincibleAura != null) invincibleAura.SetActive(false);

        buffVisual = GetComponent<BuffVisual>();
        if (buffVisual == null) buffVisual = gameObject.AddComponent<BuffVisual>();

        if (GetComponent<BallIdentityVisual>() == null)
            gameObject.AddComponent<BallIdentityVisual>();
    }

    protected virtual void Start()
    {
        MatchManager.RegisterPlayer(this);
    }

    protected virtual void OnDestroy()
    {
        if (GameFeel.Instance != null)
            GameFeel.Instance.PlayDeath(transform.position, GetBallAccentColor());
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxDeath);
        MatchManager.UnregisterPlayer(this);
    }

    protected abstract Vector2 ReadInput();

    protected virtual void Update()
    {
        TickBuffTimers();

        Vector2 input = ReadInput();
        if ((activeBuffs & PickupBuff.Reverse) != 0) input = -input;
        moveDir = input.normalized;
    }

    void TickBuffTimers()
    {
        bool changed = false;
        if ((activeBuffs & PickupBuff.Speed) != 0)
        {
            speedTimer -= Time.deltaTime;
            if (speedTimer <= 0f) { activeBuffs &= ~PickupBuff.Speed; changed = true; }
        }
        if ((activeBuffs & PickupBuff.Heavy) != 0)
        {
            heavyTimer -= Time.deltaTime;
            if (heavyTimer <= 0f) { activeBuffs &= ~PickupBuff.Heavy; changed = true; }
        }
        if ((activeBuffs & PickupBuff.Reverse) != 0)
        {
            reverseTimer -= Time.deltaTime;
            if (reverseTimer <= 0f) activeBuffs &= ~PickupBuff.Reverse;
        }
        if ((activeBuffs & PickupBuff.Invincible) != 0)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                activeBuffs &= ~PickupBuff.Invincible;
                if (invincibleAura != null) invincibleAura.SetActive(false);
                if (s_playerLayer >= 0) gameObject.layer = s_playerLayer;
                ClearInvincibleIgnoreCollisions();
            }
        }
        if (changed) RecalcStats();
    }

    public void ApplyPickupBuff(PickupBuff buff)
    {
        if ((buff & PickupBuff.Shockwave) != 0)
        {
            TriggerShockwave();
        }
        activeBuffs |= (buff & ~PickupBuff.Shockwave);
        if ((buff & PickupBuff.Speed)      != 0) speedTimer      = buffDuration;
        if ((buff & PickupBuff.Heavy)      != 0) heavyTimer      = buffDuration;
        if ((buff & PickupBuff.Reverse)    != 0) reverseTimer    = buffDuration;
        if ((buff & PickupBuff.Invincible) != 0)
        {
            invincibleTimer = invincibleDuration;
            if (invincibleAura != null) invincibleAura.SetActive(true);
            if (s_invinciblePlayerLayer >= 0) gameObject.layer = s_invinciblePlayerLayer;
            // 保险丝: Layer 矩阵对"未来接触"有效,但 Unity 物理对"当前已接触"的接触点不会立即重算
            // 显式 IgnoreCollision 一遍场上所有球,把已存在的接触也清掉
            ApplyInvincibleIgnoreCollisions();
        }
        RecalcStats();

        if (buffVisual != null) buffVisual.OnPickup(buff);
    }

    void ApplyInvincibleIgnoreCollisions()
    {
        ClearInvincibleIgnoreCollisions();
        if (circleCol == null) return;

        var players = MatchManager.AlivePlayers;
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (p == null || p == this) continue;
            var c = p.GetComponent<CircleCollider2D>();
            if (c != null) { Physics2D.IgnoreCollision(circleCol, c, true); invincibleIgnoredCols.Add(c); }
        }
        var ais = FindObjectsByType<AIController>(FindObjectsSortMode.None);
        for (int i = 0; i < ais.Length; i++)
        {
            var c = ais[i].GetComponent<CircleCollider2D>();
            if (c != null) { Physics2D.IgnoreCollision(circleCol, c, true); invincibleIgnoredCols.Add(c); }
        }
    }

    void ClearInvincibleIgnoreCollisions()
    {
        if (circleCol != null)
        {
            for (int i = 0; i < invincibleIgnoredCols.Count; i++)
            {
                var c = invincibleIgnoredCols[i];
                if (c != null) Physics2D.IgnoreCollision(circleCol, c, false);
            }
        }
        invincibleIgnoredCols.Clear();
    }

    void TriggerShockwave()
    {
        Vector2 origin = transform.position;
        var hits = Physics2D.OverlapCircleAll(origin, shockwaveRadius);
        foreach (var h in hits)
        {
            if (h == null || h.attachedRigidbody == null) continue;
            if (h.gameObject == gameObject) continue;
            Vector2 toward = ((Vector2)h.transform.position - origin);
            float dist = toward.magnitude;
            if (dist < 0.001f) continue;
            float falloff = 1f - Mathf.Clamp01(dist / shockwaveRadius);
            h.attachedRigidbody.AddForce(toward.normalized * (shockwaveForce * falloff), ForceMode2D.Impulse);
        }
        if (GameFeel.Instance != null) GameFeel.Instance.PlayImpact(origin, shockwaveForce, GetBallAccentColor());
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxImpactHard, 1f);
    }

    protected virtual void FixedUpdate()
    {
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            currentSpeed * moveDir,
            currentAccel * Time.fixedDeltaTime
        );
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Time.unscaledTime - lastImpactTime < impactCooldown) return;
        lastImpactTime = Time.unscaledTime;

        float rv = col.relativeVelocity.magnitude;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayImpact(rv);
        if (GameFeel.Instance != null)
        {
            Vector2 pt = col.contactCount > 0 ? col.GetContact(0).point : (Vector2)transform.position;
            GameFeel.Instance.PlayImpact(pt, rv, GetBallAccentColor());
        }
        PlayImpactSquash(rv);
    }

    public float GetBuffRemaining(PickupBuff buff)
    {
        switch (buff)
        {
            case PickupBuff.Speed:      return (activeBuffs & PickupBuff.Speed) != 0 ? Mathf.Max(0f, speedTimer) : 0f;
            case PickupBuff.Heavy:      return (activeBuffs & PickupBuff.Heavy) != 0 ? Mathf.Max(0f, heavyTimer) : 0f;
            case PickupBuff.Reverse:    return (activeBuffs & PickupBuff.Reverse) != 0 ? Mathf.Max(0f, reverseTimer) : 0f;
            case PickupBuff.Invincible: return (activeBuffs & PickupBuff.Invincible) != 0 ? Mathf.Max(0f, invincibleTimer) : 0f;
            default:                    return 0f;
        }
    }

    public void ApplyState(BallState s)
    {
        State = s;
        RecalcStats();
        if (fastStateOutline != null)
            fastStateOutline.SetActive(s == BallState.Fast);
    }

    void RecalcStats()
    {
        float   speed = baseMoveSpeed;
        float   accel = baseAcceleration;
        float   mass  = baseMass;
        Vector3 scale = Vector3.one;

        switch (State)
        {
            case BallState.Fast:
                speed *= fastSpeedMul;
                accel *= fastAccelMul;
                break;
            case BallState.Big:
                scale *= bigScaleMul;
                mass  *= bigMassMul;
                break;
            case BallState.Small:
                scale *= smallScaleMul;
                mass  *= smallMassMul;
                break;
        }

        if ((activeBuffs & PickupBuff.Speed) != 0)
        {
            speed *= speedBuffMul;
            accel *= speedBuffMul;
        }
        if ((activeBuffs & PickupBuff.Heavy) != 0)
        {
            mass *= heavyBuffMul;
        }

        currentSpeed = speed;
        currentAccel = accel;
        if (rb != null) rb.mass = mass;
        transform.localScale   = scale;
    }

    Color GetBallAccentColor()
    {
        if (ballSprite != null) return ballSprite.color;
        return BallVisualUtility.AccentColor(gameObject);
    }

    void PlayImpactSquash(float relativeVelocity)
    {
        if (ballVisualTransform == null) return;

        float strength = Mathf.Clamp01(relativeVelocity / 12f);
        if (strength <= 0.03f) return;

        if (impactSquashCoroutine != null) StopCoroutine(impactSquashCoroutine);
        impactSquashCoroutine = StartCoroutine(ImpactSquashRoutine(strength));
    }

    IEnumerator ImpactSquashRoutine(float strength)
    {
        if (ballVisualTransform == null) yield break;

        const float dur = 0.16f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float wave = Mathf.Sin(k * Mathf.PI);
            ballVisualTransform.localScale = new Vector3(
                ballVisualBaseScale.x * (1f + 0.16f * strength * wave),
                ballVisualBaseScale.y * (1f - 0.1f * strength * wave),
                ballVisualBaseScale.z
            );
            yield return null;
        }

        if (ballVisualTransform != null)
            ballVisualTransform.localScale = ballVisualBaseScale;
        impactSquashCoroutine = null;
    }
}
