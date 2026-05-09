using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class PlayerBase : MonoBehaviour
{
    [SerializeField] float moveSpeed   = 5f;
    [SerializeField] float acceleration = 25f;
    [SerializeField] Vector2 boundsMin = new Vector2(-8f, -4.5f);
    [SerializeField] Vector2 boundsMax = new Vector2( 8f,  4.5f);

    Rigidbody2D rb;
    Vector2 moveDir;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
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
            moveDir * moveSpeed,
            acceleration * Time.fixedDeltaTime
        );

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
        pos.y = Mathf.Clamp(pos.y, boundsMin.y, boundsMax.y);
        rb.position = pos;
    }
}
