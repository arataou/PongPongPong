using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BoundsKiller : MonoBehaviour
{
    CircleCollider2D col;

    void Awake()
    {
        col = GetComponent<CircleCollider2D>();
    }

    void FixedUpdate()
    {
        var b = ArenaBounds.Instance;
        if (b == null) return;

        Vector2 pos = transform.position;
        float r = col.radius * Mathf.Abs(transform.lossyScale.x);

        bool fullyOutLeft   = pos.x + r < b.Min.x;
        bool fullyOutRight  = pos.x - r > b.Max.x;
        bool fullyOutBottom = pos.y + r < b.Min.y;
        bool fullyOutTop    = pos.y - r > b.Max.y;

        if (fullyOutLeft || fullyOutRight || fullyOutBottom || fullyOutTop)
            Destroy(gameObject);
    }
}
