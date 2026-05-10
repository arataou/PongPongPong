using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [SerializeField] GameObject aiPrefab;
    [SerializeField] float firstSpawnDelay = 60f;
    [SerializeField] float spawnInterval   = 10f;

    public static AISpawner Instance { get; private set; }

    public float TimeUntilNext => timer;
    public bool  FirstSpawned  => firstSpawned;

    float timer;
    bool  firstSpawned;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        timer = firstSpawnDelay;
        firstSpawned = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (MatchManager.Instance != null &&
            MatchManager.Instance.CurrentOutcome != MatchManager.Outcome.Ongoing) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnOne();
            firstSpawned = true;
            timer = spawnInterval;
        }
    }

    void SpawnOne()
    {
        if (aiPrefab == null || ArenaBounds.Instance == null) return;
        Vector2 pos = RandomPerimeterPoint();
        Instantiate(aiPrefab, pos, Quaternion.identity);
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
