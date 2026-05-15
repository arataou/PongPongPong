using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [SerializeField] GameObject aiPrefab;
    [SerializeField] float firstSpawnDelay = 60f;
    [SerializeField] float spawnInterval   = 10f;

    [Header("COOP Scaling")]
    [Tooltip("COOP 模式下,每隔此秒数把 spawnInterval 减半。")]
    [SerializeField] float coopHalveEvery     = 30f;
    [SerializeField] float coopMinInterval    = 1f;
    [SerializeField] int   coopMaxConcurrent  = 8;

    public static AISpawner Instance { get; private set; }

    public float TimeUntilNext => timer;
    public bool  FirstSpawned  => firstSpawned;

    float timer;
    float currentInterval;
    float coopElapsed;
    bool  firstSpawned;
    bool  isCoop;
    static int aliveAICount;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        aliveAICount = 0;
    }

    void Start()
    {
        if (GameSession.Instance != null)
        {
            var p = GameSession.Instance.Profile;
            firstSpawnDelay = p.firstSpawnDelay;
            spawnInterval   = p.spawnInterval;
            isCoop          = GameSession.Instance.Mode == GameMode.Coop;
        }
        timer           = firstSpawnDelay;
        currentInterval = spawnInterval;
        coopElapsed     = 0f;
        firstSpawned    = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (MatchManager.Instance != null &&
            MatchManager.Instance.CurrentOutcome != MatchManager.Outcome.Ongoing) return;

        timer -= Time.deltaTime;

        if (isCoop)
        {
            coopElapsed += Time.deltaTime;
            float halvings = Mathf.Floor(coopElapsed / Mathf.Max(0.01f, coopHalveEvery));
            float target = spawnInterval * Mathf.Pow(0.5f, halvings);
            currentInterval = Mathf.Max(coopMinInterval, target);
        }
        else
        {
            currentInterval = spawnInterval;
        }

        if (timer <= 0f)
        {
            if (!isCoop || aliveAICount < coopMaxConcurrent)
            {
                SpawnOne();
                firstSpawned = true;
            }
            timer = currentInterval;
        }
    }

    void SpawnOne()
    {
        if (aiPrefab == null || ArenaBounds.Instance == null) return;
        Vector2 pos = RandomPerimeterPoint();
        var go = Instantiate(aiPrefab, pos, Quaternion.identity);
        var tracker = go.AddComponent<AILifeTracker>();
        tracker.OnSpawn();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxAISpawn);
    }

    public static void NotifyAIDestroyed()
    {
        if (aliveAICount > 0) aliveAICount--;
    }

    public static void NotifyAISpawned()
    {
        aliveAICount++;
    }

    Vector2 RandomPerimeterPoint()
    {
        var b = ArenaBounds.Instance;
        int edge = Random.Range(0, 4);
        float t  = Random.value;
        switch (edge)
        {
            case 0:  return new Vector2(b.Min.x, Mathf.Lerp(b.Min.y, b.Max.y, t));
            case 1:  return new Vector2(b.Max.x, Mathf.Lerp(b.Min.y, b.Max.y, t));
            case 2:  return new Vector2(Mathf.Lerp(b.Min.x, b.Max.x, t), b.Min.y);
            default: return new Vector2(Mathf.Lerp(b.Min.x, b.Max.x, t), b.Max.y);
        }
    }
}

class AILifeTracker : MonoBehaviour
{
    public void OnSpawn() { AISpawner.NotifyAISpawned(); }
    void OnDestroy()      { AISpawner.NotifyAIDestroyed(); }
}
