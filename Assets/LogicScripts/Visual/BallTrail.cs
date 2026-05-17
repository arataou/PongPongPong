using UnityEngine;

[RequireComponent(typeof(PlayerBase))]
public class BallTrail : MonoBehaviour
{
    [SerializeField] float baseWidth   = 0.25f;
    [SerializeField] float baseTime    = 0.22f;
    [SerializeField] float fastTimeMul = 2f;
    [Tooltip("拖尾随球大小缩放的指数。1=线性,>1 大球更夸张/小球更细短")]
    [SerializeField] float sizeTrailPower = 1.3f;
    [SerializeField] Material trailMaterial;

    TrailRenderer trail;
    PlayerBase    player;
    SpriteRenderer sprite;

    void Awake()
    {
        player = GetComponent<PlayerBase>();
        sprite = BallVisualUtility.EnsureChildSpriteRenderer(gameObject);

        trail = GetComponent<TrailRenderer>();
        if (trail == null) trail = gameObject.AddComponent<TrailRenderer>();

        if (trailMaterial == null)
            trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trail.material      = trailMaterial;
        trail.time          = baseTime;
        trail.startWidth    = baseWidth;
        trail.endWidth      = 0f;
        trail.minVertexDistance = 0.05f;
        trail.emitting      = true;
        trail.sortingOrder  = -1;

        RefreshColor();
    }

    void LateUpdate()
    {
        RefreshColor();
        bool isFast = player.State == BallState.Fast;
        float s = Mathf.Abs(transform.lossyScale.x);
        float sizeMul = Mathf.Pow(s, sizeTrailPower);
        trail.time       = baseTime * (isFast ? fastTimeMul : 1f) * sizeMul;
        trail.startWidth = baseWidth * sizeMul;
        trail.emitting   = !player.IsInvincible;
    }

    void RefreshColor()
    {
        if (sprite == null) return;
        Color c = sprite.color;
        Color head = new Color(c.r, c.g, c.b, 0.7f);
        Color tail = new Color(c.r, c.g, c.b, 0f);
        trail.startColor = head;
        trail.endColor   = tail;
    }
}
