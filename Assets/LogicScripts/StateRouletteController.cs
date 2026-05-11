using UnityEngine;

public class StateRouletteController : MonoBehaviour
{
    [SerializeField] float interval = 1f;

    public static StateRouletteController Instance { get; private set; }
    public float TimeUntilNext => timer;
    public float Interval => interval;

    float timer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (MatchManager.Instance != null &&
            MatchManager.Instance.CurrentOutcome != MatchManager.Outcome.Ongoing) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SwitchAll();
            timer = interval;
        }
    }

    void SwitchAll()
    {
        var players = MatchManager.AlivePlayers;
        for (int i = 0; i < players.Count; i++)
            players[i].ApplyState(RandomState());
    }

    static BallState RandomState()
    {
        int r = Random.Range(0, 3);
        switch (r)
        {
            case 0:  return BallState.Fast;
            case 1:  return BallState.Big;
            default: return BallState.Small;
        }
    }
}
