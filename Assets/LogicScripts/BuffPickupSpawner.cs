using System.Collections.Generic;
using UnityEngine;

public class BuffPickupSpawner : MonoBehaviour
{
    const int CueSegments = 48;
    const int PickupSortingOrder = 3;
    const int CueSpriteSortingOrder = 2;
    const int CueRingSortingOrder = 4;

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
    [SerializeField] float  spawnCueLeadTime = 0.45f;
    [SerializeField] float  spawnCueScale = 1.35f;

    static readonly PickupBuff[] BuffPool =
    {
        PickupBuff.Speed,
        PickupBuff.Heavy,
        PickupBuff.Reverse,
        PickupBuff.Invincible,
        PickupBuff.Shockwave,
    };

    float timer;
    bool hasPendingSpawn;
    Vector2 pendingSpawnPosition;
    PickupBuff pendingSpawnBuff;
    GameObject spawnCue;
    SpriteRenderer spawnCueSprite;
    LineRenderer spawnCueRing;
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

        if (active.Count >= maxOnField)
        {
            hasPendingSpawn = false;
            ClearSpawnCue();
            return;
        }

        timer -= Time.deltaTime;
        if (!hasPendingSpawn && timer <= spawnCueLeadTime)
            PreparePendingSpawn();

        UpdateSpawnCue();

        if (timer <= 0f)
        {
            if (hasPendingSpawn)
                PromotePendingSpawnToPickup();
            else
                SpawnOne();
            timer = spawnInterval;
            hasPendingSpawn = false;
        }
    }

    void SpawnOne()
    {
        if (ArenaBounds.Instance == null) return;

        var buffType = BuffPool[Random.Range(0, BuffPool.Length)];
        SpawnOne(RandomArenaPoint(), buffType);
    }

    void SpawnOne(Vector2 position, PickupBuff buffType)
    {
        var go = new GameObject("BuffPickup");
        go.transform.position   = position;
        go.transform.localScale = Vector3.one * pickupScale;

        var sprite = SpriteFor(buffType);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite != null ? sprite : pickupSprite;
        sr.sortingOrder = PickupSortingOrder;
        sr.color        = sprite != null ? Color.white : ColorFor(buffType);

        ConfigurePickupColliderAndBuff(go, buffType);

        active.Add(go.GetComponent<BuffPickup>());
    }

    void PromotePendingSpawnToPickup()
    {
        if (spawnCue == null || spawnCueSprite == null)
        {
            ClearSpawnCue();
            SpawnOne(pendingSpawnPosition, pendingSpawnBuff);
            return;
        }

        var go = spawnCue;
        go.name = "BuffPickup";
        go.transform.position = pendingSpawnPosition;
        go.transform.localScale = Vector3.one * pickupScale;

        var sprite = SpriteFor(pendingSpawnBuff);
        spawnCueSprite.sprite = sprite != null ? sprite : pickupSprite;
        spawnCueSprite.color = sprite != null ? Color.white : ColorFor(pendingSpawnBuff);
        spawnCueSprite.sortingOrder = PickupSortingOrder;

        if (spawnCueRing != null)
            Destroy(spawnCueRing);

        ConfigurePickupColliderAndBuff(go, pendingSpawnBuff);
        active.Add(go.GetComponent<BuffPickup>());
        ClearSpawnCueReferences();
    }

    void ConfigurePickupColliderAndBuff(GameObject go, PickupBuff buffType)
    {
        var col = go.GetComponent<CircleCollider2D>();
        if (col == null) col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = pickupRadius;

        var pickup = go.GetComponent<BuffPickup>();
        if (pickup == null) pickup = go.AddComponent<BuffPickup>();
        pickup.buffType = buffType;
    }

    void PreparePendingSpawn()
    {
        if (ArenaBounds.Instance == null) return;

        hasPendingSpawn = true;
        pendingSpawnPosition = RandomArenaPoint();
        pendingSpawnBuff = BuffPool[Random.Range(0, BuffPool.Length)];
        CreateSpawnCue();
    }

    void CreateSpawnCue()
    {
        ClearSpawnCue();

        spawnCue = new GameObject("BuffPickupSpawnCue");
        spawnCue.transform.position = pendingSpawnPosition;
        spawnCue.transform.localScale = Vector3.one * (pickupScale * spawnCueScale);

        var sprite = SpriteFor(pendingSpawnBuff);
        spawnCueSprite = spawnCue.AddComponent<SpriteRenderer>();
        spawnCueSprite.sprite = sprite != null ? sprite : pickupSprite;
        spawnCueSprite.color = CueColor(0.34f);
        spawnCueSprite.sortingOrder = CueSpriteSortingOrder;

        spawnCueRing = spawnCue.AddComponent<LineRenderer>();
        spawnCueRing.useWorldSpace = false;
        spawnCueRing.loop = true;
        spawnCueRing.positionCount = CueSegments;
        spawnCueRing.startWidth = 0.035f;
        spawnCueRing.endWidth = 0.035f;
        spawnCueRing.numCapVertices = 3;
        spawnCueRing.numCornerVertices = 3;
        spawnCueRing.material = new Material(Shader.Find("Sprites/Default"));
        spawnCueRing.sortingOrder = CueRingSortingOrder;
    }

    void UpdateSpawnCue()
    {
        if (spawnCue == null || spawnCueRing == null) return;

        float lead = Mathf.Max(0.001f, spawnCueLeadTime);
        float k = Mathf.Clamp01(1f - timer / lead);
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 11f);
        spawnCue.transform.localScale = Vector3.one * (pickupScale * spawnCueScale * Mathf.Lerp(0.88f, 1.08f, pulse));

        if (spawnCueSprite != null)
            spawnCueSprite.color = CueColor(Mathf.Lerp(0.22f, 0.52f, k));

        float r = pickupRadius * Mathf.Lerp(1.4f, 0.95f, k) * (1f + pulse * 0.08f);
        for (int i = 0; i < CueSegments; i++)
        {
            float a = (i / (float)CueSegments) * Mathf.PI * 2f;
            spawnCueRing.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }

        Color c = CueColor(Mathf.Lerp(0.22f, 0.72f, k));
        spawnCueRing.startColor = c;
        spawnCueRing.endColor = c;
    }

    void ClearSpawnCue()
    {
        if (spawnCue != null) Destroy(spawnCue);
        ClearSpawnCueReferences();
    }

    void ClearSpawnCueReferences()
    {
        spawnCue = null;
        spawnCueSprite = null;
        spawnCueRing = null;
    }

    Color CueColor(float alpha)
    {
        Color c = ColorFor(pendingSpawnBuff);
        c = Color.Lerp(Color.white, c, 0.72f);
        c.a = alpha;
        return c;
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
