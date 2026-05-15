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
    [SerializeField] float shockwaveRadius    = 3f;
    [SerializeField] float shockwaveForce     = 12f;

    [Header("Visual (optional)")]
    [Tooltip("Child GameObject shown only while in Fast state (the bright outline ring).")]
    [SerializeField] GameObject fastStateOutline;
    [Tooltip("Child GameObject shown only while invincible (an aura/halo).")]
    [SerializeField] GameObject invincibleAura;

    [Header("Audio")]
    [SerializeField] float impactCooldown = 0.05f;

    Rigidbody2D rb;
    Vector2 moveDir;

    float currentSpeed;
    float currentAccel;

    CircleCollider2D    circleCol;
    PickupBuff          activeBuffs = PickupBuff.None;
    float speedTimer;
    float heavyTimer;
    float reverseTimer;
    float invincibleTimer;
    float lastImpactTime;

    public BallState State { get; private set; } = BallState.Normal;
    public bool IsPlayer => true;
    public bool IsInvincible => (activeBuffs & PickupBuff.Invincible) != 0;
    public abstract int PlayerIndex { get; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.linearDamping = 0.8f;
        circleCol = GetComponent<CircleCollider2D>();
        var bouncy = Resources.Load<PhysicsMaterial2D>("Bouncy");
        if (bouncy != null) circleCol.sharedMaterial = bouncy;
        ApplyState(BallState.Normal);
        if (invincibleAura != null) invincibleAura.SetActive(false);
    }

    protected virtual void Start()
    {
        MatchManager.RegisterPlayer(this);
    }

    protected virtual void OnDestroy()
    {
        if (GameFeel.Instance != null) GameFeel.Instance.PlayDeath();
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
        }
        RecalcStats();
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
        if (GameFeel.Instance != null) GameFeel.Instance.PlayImpact(origin, shockwaveForce);
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
            var pt = col.GetContact(0).point;
            GameFeel.Instance.PlayImpact(pt, rv);
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
}
