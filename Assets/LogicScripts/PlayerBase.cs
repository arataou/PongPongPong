using UnityEngine;

public enum BallState { Normal, Fast, Big, Small }

[System.Flags]
public enum PickupBuff { None = 0, Speed = 1, Heavy = 2, Reverse = 4 }

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
    [SerializeField] float speedBuffMul = 1.5f;
    [SerializeField] float heavyBuffMul = 3f;
    [SerializeField] float buffDuration = 5f;

    [Header("Visual (optional)")]
    [Tooltip("Child GameObject shown only while in Fast state (the bright outline ring).")]
    [SerializeField] GameObject fastStateOutline;

    Rigidbody2D rb;
    Vector2 moveDir;

    float currentSpeed;
    float currentAccel;

    CircleCollider2D    circleCol;
    PickupBuff          activeBuffs = PickupBuff.None;
    float speedTimer;
    float heavyTimer;
    float reverseTimer;

    public BallState State { get; private set; } = BallState.Normal;
    public bool IsPlayer => true;
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
    }

    protected virtual void Start()
    {
        MatchManager.RegisterPlayer(this);
    }

    protected virtual void OnDestroy()
    {
        if (GameFeel.Instance != null) GameFeel.Instance.PlayDeath();
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
        if (changed) RecalcStats();
    }

    public void ApplyPickupBuff(PickupBuff buff)
    {
        activeBuffs |= buff;
        if ((buff & PickupBuff.Speed)   != 0) speedTimer   = buffDuration;
        if ((buff & PickupBuff.Heavy)   != 0) heavyTimer   = buffDuration;
        if ((buff & PickupBuff.Reverse) != 0) reverseTimer = buffDuration;
        RecalcStats();
    }

    protected virtual void FixedUpdate()
    {
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            moveDir * currentSpeed,
            currentAccel * Time.fixedDeltaTime
        );
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
