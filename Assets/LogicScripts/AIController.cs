using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIController : MonoBehaviour
{
    [SerializeField] float moveSpeed    = 4f;
    [SerializeField] float acceleration = 25f;

    Rigidbody2D rb;
    float       speedMul = 1f;

    void Awake()
    {
        BallVisualUtility.EnsureChildSpriteRenderer(gameObject);
        if (GetComponent<AIDangerVisual>() == null)
            gameObject.AddComponent<AIDangerVisual>();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.linearDamping = 0.8f;

        int playerLayer = PlayerBase.PlayerLayer;
        if (playerLayer >= 0) gameObject.layer = playerLayer;

        if (GameSession.Instance != null)
        {
            var p = GameSession.Instance.Profile;
            speedMul = p.speedMul;
            rb.mass *= p.massMul;
        }
    }

    void FixedUpdate()
    {
        var target = FindNearestPlayer();
        Vector2 dir = Vector2.zero;
        if (target != null)
            dir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            (moveSpeed * speedMul) * dir,
            acceleration * Time.fixedDeltaTime
        );
    }

    PlayerBase FindNearestPlayer()
    {
        var players = MatchManager.AlivePlayers;
        PlayerBase nearest = null;
        float bestSqr = float.MaxValue;
        Vector2 self = transform.position;
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (p == null) continue;
            float d = ((Vector2)p.transform.position - self).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; nearest = p; }
        }
        return nearest;
    }
}
