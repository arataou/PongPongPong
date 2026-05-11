using UnityEngine;

public enum BallState { Normal, Fast, Big, Small }

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

    [Header("Visual (optional)")]
    [Tooltip("Child GameObject shown only while in Fast state (the bright outline ring).")]
    [SerializeField] GameObject fastStateOutline;

    Rigidbody2D rb;
    Vector2 moveDir;

    float currentSpeed;
    float currentAccel;

    public BallState State { get; private set; } = BallState.Normal;
    public bool IsPlayer => true;
    public abstract int PlayerIndex { get; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.linearDamping = 0.8f;
        ApplyState(BallState.Normal);
    }

    protected virtual void Start()
    {
        MatchManager.RegisterPlayer(this);
    }

    protected virtual void OnDestroy()
    {
        MatchManager.UnregisterPlayer(this);
    }

    protected abstract Vector2 ReadInput();

    protected virtual void Update()
    {
        moveDir = ReadInput().normalized;
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
        switch (s)
        {
            case BallState.Fast:
                currentSpeed = baseMoveSpeed * fastSpeedMul;
                currentAccel = baseAcceleration * fastAccelMul;
                transform.localScale = Vector3.one;
                rb.mass = baseMass;
                break;
            case BallState.Big:
                currentSpeed = baseMoveSpeed;
                currentAccel = baseAcceleration;
                transform.localScale = Vector3.one * bigScaleMul;
                rb.mass = baseMass * bigMassMul;
                break;
            case BallState.Small:
                currentSpeed = baseMoveSpeed;
                currentAccel = baseAcceleration;
                transform.localScale = Vector3.one * smallScaleMul;
                rb.mass = baseMass * smallMassMul;
                break;
            default:
                currentSpeed = baseMoveSpeed;
                currentAccel = baseAcceleration;
                transform.localScale = Vector3.one;
                rb.mass = baseMass;
                break;
        }

        if (fastStateOutline != null)
            fastStateOutline.SetActive(s == BallState.Fast);
    }
}
