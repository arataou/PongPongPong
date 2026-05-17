using System.Collections.Generic;
using UnityEngine;

public class BuffPickupSpawner : MonoBehaviour
{
    [SerializeField] float  spawnInterval = 5f;
    [SerializeField] int    maxOnField    = 3;
    [SerializeField] Sprite pickupSprite;
    [SerializeField] Sprite speedSprite;
    [SerializeField] Sprite heavySprite;
    [SerializeField] Sprite reverseSprite;
    [SerializeField] Sprite invincibleSprite;
    [SerializeField] Sprite shockwaveSprite;
    [SerializeField] Color  pickupColor   = new Color(0.9f, 0.9f, 0.95f, 1f);
    [SerializeField] float  pickupScale   = 0.7f;
    [SerializeField] float  pickupRadius  = 0.35f;
    [SerializeField] float  edgePadding   = 0.5f;

    static readonly PickupBuff[] BuffPool =
    {
        PickupBuff.Speed,
        PickupBuff.Heavy,
        PickupBuff.Reverse,
        PickupBuff.Invincible,
        PickupBuff.Shockwave,
    };

    float timer;
    readonly List<BuffPickup> active = new List<BuffPickup>();

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (MatchManager.Instance != null &&
            MatchManager.Instance.CurrentOutcome != MatchManager.Outcome.Ongoing) return;

        for (int i = active.Count - 1; i >= 0; i--)
            if (active[i] == null) active.RemoveAt(i);

        if (active.Count >= maxOnField) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnOne();
            timer = spawnInterval;
        }
    }

    void SpawnOne()
    {
        if (ArenaBounds.Instance == null) return;

        var go = new GameObject("BuffPickup");
        go.transform.position   = RandomArenaPoint();
        go.transform.localScale = Vector3.one * pickupScale;

        var buffType = BuffPool[Random.Range(0, BuffPool.Length)];
        var sprite = SpriteFor(buffType);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite != null ? sprite : pickupSprite;
        sr.sortingOrder = 0;
        sr.color        = sprite != null ? Color.white : ColorFor(buffType);

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = pickupRadius;

        var pickup = go.AddComponent<BuffPickup>();
        pickup.buffType = buffType;

        active.Add(pickup);
    }

    Color ColorFor(PickupBuff b)
    {
        switch (b)
        {
            case PickupBuff.Speed:      return new Color(1f,   0.9f, 0.3f, 1f);
            case PickupBuff.Heavy:      return new Color(0.6f, 0.3f, 0.9f, 1f);
            case PickupBuff.Reverse:    return new Color(0.5f, 0.5f, 0.5f, 1f);
            case PickupBuff.Invincible: return new Color(1f,   0.85f, 0.3f, 1f);
            case PickupBuff.Shockwave:  return new Color(1f,   0.45f, 0.2f, 1f);
            default:                    return pickupColor;
        }
    }

    Sprite SpriteFor(PickupBuff b)
    {
        switch (b)
        {
            case PickupBuff.Speed:      return speedSprite;
            case PickupBuff.Heavy:      return heavySprite;
            case PickupBuff.Reverse:    return reverseSprite;
            case PickupBuff.Invincible: return invincibleSprite;
            case PickupBuff.Shockwave:  return shockwaveSprite;
            default:                    return null;
        }
    }

    Vector2 RandomArenaPoint()
    {
        var b = ArenaBounds.Instance;
        return new Vector2(
            Random.Range(b.Min.x + edgePadding, b.Max.x - edgePadding),
            Random.Range(b.Min.y + edgePadding, b.Max.y - edgePadding)
        );
    }
}
