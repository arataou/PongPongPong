using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObstacleMover : MonoBehaviour
{
    public enum Axis { Horizontal, Vertical }

    [SerializeField] Axis  axis     = Axis.Horizontal;
    [SerializeField] float distance = 3f;
    [SerializeField] float speed    = 2f;
    [SerializeField] float startPhase = 0f;

    Rigidbody2D rb;
    Vector2     center;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType    = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        center = rb.position;
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;
        float t = Time.time * speed + startPhase;
        float offset = Mathf.Sin(t) * distance;
        Vector2 target = axis == Axis.Horizontal
            ? center + new Vector2(offset, 0f)
            : center + new Vector2(0f, offset);
        rb.MovePosition(target);
    }
}
