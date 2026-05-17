using System.Collections.Generic;
using UnityEngine;

public class BuffPickupSpawner : MonoBehaviour
{
    const int PickupSortingOrder = 3;
    const int CueSpriteSortingOrder = 2;

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
    readonly List<BuffPickup> active = new List<BuffPickup>();

    void Start()
    {
        // COOP 模式下按难度收紧 buff 生成节奏 (Easy×1.4 / Normal×1.0 / Hard×0.55)
        if (GameSession.Instance != null && GameSession.Instance.Mode == GameMode.Coop)
        {
            float mul = GameSession.Instance.Profile.coopPacingMul;
            if (mul > 0f) spawnInterval *= mul;
        }
        timer = spawnInterval;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            CancelPendingSpawn();
            return;
        }
        if (MatchManager.Instance != null &&
            MatchManager.Instance.CurrentOutcome != MatchManager.Outcome.Ongoing)
        {
            CancelPendingSpawn();
            return;
        }

        for (int i = active.Count - 1; i >= 0; i--)
            if (active[i] == null) active.RemoveAt(i);

        if (active.Count >= maxOnField)
        {
            CancelPendingSpawn();
            return;
        }

        // 状态一致性兜底:flag 说"有预告",但实际 GO 引用没了 → 重置 flag,下一帧重新走
        if (hasPendingSpawn && spawnCue == null)
        {
            hasPendingSpawn = false;
            ClearSpawnCueReferences();
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
        go.transform.SetParent(transform, false);
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
        if (!go.activeSelf) go.SetActive(true);   // 防御:任何路径意外 SetActive(false) 都强制开

        var sprite = SpriteFor(pendingSpawnBuff);
        spawnCueSprite.sprite = sprite != null ? sprite : pickupSprite;
        spawnCueSprite.color = sprite != null ? Color.white : ColorFor(pendingSpawnBuff);
        spawnCueSprite.sortingOrder = PickupSortingOrder;
        spawnCueSprite.enabled = true;

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
        spawnCue.transform.SetParent(transform, false);   // 绑定到 spawner,生命周期一致
        spawnCue.transform.position = pendingSpawnPosition;
        spawnCue.transform.localScale = Vector3.one * (pickupScale * spawnCueScale);

        var sprite = SpriteFor(pendingSpawnBuff);
        spawnCueSprite = spawnCue.AddComponent<SpriteRenderer>();
        spawnCueSprite.sprite = sprite != null ? sprite : pickupSprite;
        spawnCueSprite.color = CueColor(0.34f);
        spawnCueSprite.sortingOrder = CueSpriteSortingOrder;
    }

    void UpdateSpawnCue()
    {
        if (spawnCue == null || spawnCueSprite == null) return;

        float lead = Mathf.Max(0.001f, spawnCueLeadTime);
        float k = Mathf.Clamp01(1f - timer / lead);
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 11f);
        float settle = Mathf.Lerp(spawnCueScale, 1f, k);
        spawnCue.transform.localScale = Vector3.one * (pickupScale * settle * Mathf.Lerp(0.94f, 1.05f, pulse));
        spawnCueSprite.color = CueColor(Mathf.Lerp(0.18f, 0.58f, k));
    }

    void CancelPendingSpawn()
    {
        hasPendingSpawn = false;
        ClearSpawnCue();
    }

    void ClearSpawnCue()
    {
        // 双保险:立即停渲染 + SetActive 让 GO 帧内就消失,不等帧尾 Destroy
        if (spawnCueSprite != null) spawnCueSprite.enabled = false;
        if (spawnCue != null)
        {
            spawnCue.SetActive(false);
            Destroy(spawnCue);
        }
        ClearSpawnCueReferences();
        // 兜底:扫一遍 spawner 子节点里有没有遗留的 SpawnCue(防止某条路径引用断了但 GO 还活着)
        SweepOrphanSpawnCues();
    }

    void SweepOrphanSpawnCues()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child != null && child.name == "BuffPickupSpawnCue")
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }

    void OnDisable()
    {
        CancelPendingSpawn();
    }

    void OnDestroy()
    {
        CancelPendingSpawn();
    }

    void ClearSpawnCueReferences()
    {
        spawnCue = null;
        spawnCueSprite = null;
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
