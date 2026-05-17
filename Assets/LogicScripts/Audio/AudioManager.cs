using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] int         sfxChannels = 6;

    [Header("BGM")]
    [SerializeField] AudioClip menuBgm;
    [SerializeField] AudioClip gameBgm;

    [Header("SFX Library")]
    public AudioClip sfxClickUI;
    public AudioClip sfxImpactSoft;
    public AudioClip sfxImpactHard;
    public AudioClip sfxDeath;
    public AudioClip sfxBuffPickup;
    public AudioClip sfxStateSwitch;
    public AudioClip sfxAISpawn;
    public AudioClip sfxWin;

    [Header("Tuning")]
    [SerializeField] float impactHardThreshold = 6f;
    [SerializeField] float sfxMinInterval      = 0.02f;

    AudioSource[] sfxSources;
    int           nextSfxIndex;
    float         lastSfxTime;
    float         bgmVolume = 1f;
    float         sfxVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (IsDedicatedAudioObject()) Destroy(gameObject);
            else Destroy(this);
            return;
        }

        Instance = this;
        if (IsDedicatedAudioObject())
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("AudioManager should be placed on a dedicated GameObject to persist across scenes.");
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        sfxSources = new AudioSource[Mathf.Max(1, sfxChannels)];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            sfxSources[i] = src;
        }

        if (GameSession.Instance != null)
            ApplyVolumes(GameSession.Instance.BgmVolume, GameSession.Instance.SfxVolume);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    bool IsDedicatedAudioObject()
    {
        var components = GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null) continue;
            if (component is Transform) continue;
            if (component is AudioManager) continue;
            if (component is AudioSource) continue;
            return false;
        }
        return true;
    }

    public void ApplyVolumes(float bgm, float sfx)
    {
        bgmVolume = Mathf.Clamp01(bgm);
        sfxVolume = Mathf.Clamp01(sfx);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    public void PlayMenuBgm() => PlayBgm(menuBgm);
    public void PlayGameBgm() => PlayBgm(gameBgm);

    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null) return;
        if (clip == null) { bgmSource.Stop(); return; }
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip   = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSources == null) return;
        if (Time.unscaledTime - lastSfxTime < sfxMinInterval) return;
        lastSfxTime = Time.unscaledTime;

        var src = sfxSources[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Length;
        src.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    public void PlayImpact(float relativeVelocity)
    {
        AudioClip c = relativeVelocity >= impactHardThreshold ? sfxImpactHard : sfxImpactSoft;
        float vol = Mathf.Clamp01(relativeVelocity / 12f) * 0.6f + 0.4f;
        PlaySfx(c, vol);
    }
}
