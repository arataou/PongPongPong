using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    const string PrefBgm    = "PPP_BgmVolume";
    const string PrefSfx    = "PPP_SfxVolume";
    const string PrefDiff   = "PPP_Difficulty";
    const string PrefMode   = "PPP_GameMode";
    const string PrefPlayers = "PPP_PlayerCount";

    [SerializeField] int          playerCount = 2;
    [SerializeField] GameMode     gameMode    = GameMode.PvP;
    [SerializeField] AIDifficulty difficulty  = AIDifficulty.Normal;
    [SerializeField] ArenaPreset  selectedArena;
    [SerializeField, Range(0f, 1f)] float bgmVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 0.8f;

    public int          PlayerCount   { get => playerCount;  set { playerCount = Mathf.Clamp(value, 2, 3); PlayerPrefs.SetInt(PrefPlayers, playerCount); } }
    public GameMode     Mode          { get => gameMode;     set { gameMode = value; PlayerPrefs.SetInt(PrefMode, (int)gameMode); } }
    public AIDifficulty Difficulty    { get => difficulty;   set { difficulty = value; PlayerPrefs.SetInt(PrefDiff, (int)difficulty); } }
    public ArenaPreset  SelectedArena { get => selectedArena; set => selectedArena = value; }
    public float        BgmVolume     { get => bgmVolume;    set { bgmVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(PrefBgm, bgmVolume); ApplyAudio(); } }
    public float        SfxVolume     { get => sfxVolume;    set { sfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(PrefSfx, sfxVolume); ApplyAudio(); } }

    public DifficultyProfile Profile => DifficultyProfile.Get(difficulty);

    public bool IsDemo { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        bgmVolume   = PlayerPrefs.GetFloat(PrefBgm,  bgmVolume);
        sfxVolume   = PlayerPrefs.GetFloat(PrefSfx,  sfxVolume);
        difficulty  = (AIDifficulty)PlayerPrefs.GetInt(PrefDiff,   (int)difficulty);
        gameMode    = (GameMode)    PlayerPrefs.GetInt(PrefMode,   (int)gameMode);
        playerCount = PlayerPrefs.GetInt(PrefPlayers, playerCount);
        ApplyAudio();
    }

    public static GameSession Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("GameSession");
        return go.AddComponent<GameSession>();
    }

    void ApplyAudio()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyVolumes(bgmVolume, sfxVolume);
        }
        else
        {
            AudioListener.volume = bgmVolume;
        }
    }
}
