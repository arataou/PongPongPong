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
    public AudioClip sfxShockwave;
    public AudioClip sfxSpeedBoost;
    public AudioClip sfxStateSwitch;
    public AudioClip sfxAISpawn;
    public AudioClip sfxWin;

    [Header("Tuning")]
    [SerializeField] float impactHardThreshold = 6f;
    [SerializeField] float impactMinVelocity = 0.75f;
    [SerializeField] float impactSfxMinInterval = 0.16f;
    [SerializeField] float sfxMinInterval      = 0.02f;

    AudioSource[] sfxSources;
    int           nextSfxIndex;
    float         lastSfxTime;
    float         lastImpactSfxTime = -999f;
    float         nextUiClickInstallTime = -999f;
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

        EnsureDefaultSfxClips();

        if (GameSession.Instance != null)
            ApplyVolumes(GameSession.Instance.BgmVolume, GameSession.Instance.SfxVolume);
    }

    void Update()
    {
        if (Time.unscaledTime < nextUiClickInstallTime) return;
        nextUiClickInstallTime = Time.unscaledTime + 0.75f;
        UIClickSound.InstallAll();
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

    public void PlaySfx(AudioClip clip, float volumeScale = 1f, bool ignoreRateLimit = false)
    {
        if (clip == null || sfxSources == null) return;
        if (!ignoreRateLimit)
        {
            if (Time.unscaledTime - lastSfxTime < sfxMinInterval) return;
            lastSfxTime = Time.unscaledTime;
        }

        var src = sfxSources[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Length;
        src.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    public void PlayImpact(float relativeVelocity)
    {
        if (relativeVelocity < impactMinVelocity) return;
        if (Time.unscaledTime - lastImpactSfxTime < impactSfxMinInterval) return;
        lastImpactSfxTime = Time.unscaledTime;

        AudioClip c = relativeVelocity >= impactHardThreshold ? sfxImpactHard : sfxImpactSoft;
        float vol = Mathf.Clamp01(relativeVelocity / 12f) * 0.55f + 0.35f;
        PlaySfx(c, vol);
    }

    void EnsureDefaultSfxClips()
    {
        if (sfxClickUI == null) sfxClickUI = CreateClickClip();
        if (sfxImpactSoft == null) sfxImpactSoft = CreateImpactClip("DefaultImpactSoft", false);
        if (sfxImpactHard == null) sfxImpactHard = CreateImpactClip("DefaultImpactHard", true);
        if (sfxDeath == null) sfxDeath = CreateDeathClip();
        if (sfxBuffPickup == null) sfxBuffPickup = CreateBuffPickupClip();
        if (sfxShockwave == null) sfxShockwave = CreateShockwaveClip();
        if (sfxSpeedBoost == null) sfxSpeedBoost = CreateSpeedBoostClip();
    }

    AudioClip CreateImpactClip(string clipName, bool hard)
    {
        int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
        float duration = hard ? 0.14f : 0.09f;
        int samples = Mathf.CeilToInt(rate * duration);
        float[] data = new float[samples];
        uint seed = hard ? 0x71f3u : 0x35a9u;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            float k = t / duration;
            float hit = Mathf.Exp(-t * (hard ? 170f : 210f));
            float bodyEnv = Mathf.Exp(-t * (hard ? 18f : 26f));
            float bounceFreq = Mathf.Lerp(hard ? 560f : 680f, hard ? 230f : 340f, Mathf.SmoothStep(0f, 1f, k));
            float body = Mathf.Sin(2f * Mathf.PI * bounceFreq * t);
            float overtone = Mathf.Sin(2f * Mathf.PI * bounceFreq * 1.92f * t) * 0.22f;
            float rubberTail = Mathf.Sin(2f * Mathf.PI * bounceFreq * 0.58f * t) * Mathf.Exp(-t * 42f) * 0.1f;
            float slap = NextNoise(ref seed) * hit * (hard ? 0.14f : 0.06f);
            data[i] = Mathf.Clamp((body + overtone) * bodyEnv * 0.64f + rubberTail + slap, -1f, 1f);
        }

        return ClipFromData(clipName, data, rate);
    }

    AudioClip CreateClickClip()
    {
        int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
        float duration = 0.07f;
        int samples = Mathf.CeilToInt(rate * duration);
        float[] data = new float[samples];
        uint seed = 0x51cdu;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            float k = t / duration;
            float env = Mathf.Exp(-t * 42f);
            float tone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(980f, 560f, k) * t);
            float tick = NextNoise(ref seed) * Mathf.Exp(-t * 180f) * 0.16f;
            data[i] = Mathf.Clamp(tone * env * 0.42f + tick, -1f, 1f);
        }

        return ClipFromData("DefaultClickUI", data, rate);
    }

    AudioClip CreateDeathClip()
    {
        int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
        float duration = 0.36f;
        int samples = Mathf.CeilToInt(rate * duration);
        float[] data = new float[samples];
        uint seed = 0xdeadu;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            float k = t / duration;
            float popEnv = Mathf.Exp(-t * 38f);
            float fallEnv = Mathf.Sin(Mathf.PI * Mathf.Clamp01(k)) * Mathf.Exp(-t * 4.5f);
            float pop = NextNoise(ref seed) * popEnv * 0.38f;
            float fall = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(360f, 95f, k) * t) * fallEnv * 0.58f;
            float wobble = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(170f, 55f, k) * t) * fallEnv * 0.18f;
            data[i] = Mathf.Clamp(pop + fall + wobble, -1f, 1f);
        }

        return ClipFromData("DefaultDeath", data, rate);
    }

    AudioClip CreateBuffPickupClip()
    {
        int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
        float duration = 0.22f;
        int samples = Mathf.CeilToInt(rate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            float k = t / duration;
            float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(k)) * Mathf.Exp(-t * 2.2f);
            float chimeA = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(760f, 1160f, k) * t);
            float chimeB = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(1140f, 1520f, k) * t);
            data[i] = Mathf.Clamp((chimeA * 0.5f + chimeB * 0.28f) * env, -1f, 1f);
        }

        return ClipFromData("DefaultBuffPickup", data, rate);
    }

    AudioClip CreateShockwaveClip()
    {
        int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
        float duration = 0.46f;
        int samples = Mathf.CeilToInt(rate * duration);
        float[] data = new float[samples];
        uint seed = 0xb41du;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            float k = t / duration;
            float env = Mathf.Exp(-t * 4.8f);
            float sweep = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(80f, 28f, k) * t);
            float ripple = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(170f, 62f, k) * t) * (1f - k);
            float air = NextNoise(ref seed) * 0.18f * Mathf.Exp(-t * 9f);
            data[i] = Mathf.Clamp((sweep * 0.78f + ripple * 0.34f + air) * env, -1f, 1f);
        }

        return ClipFromData("DefaultShockwave", data, rate);
    }

    AudioClip CreateSpeedBoostClip()
    {
        int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
        float duration = 0.28f;
        int samples = Mathf.CeilToInt(rate * duration);
        float[] data = new float[samples];
        uint seed = 0x9e37u;

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            float k = t / duration;
            float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(k));
            float rise = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(360f, 1040f, k) * t);
            float shimmer = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(980f, 1760f, k) * t);
            float air = NextNoise(ref seed) * 0.16f;
            data[i] = Mathf.Clamp((rise * 0.45f + shimmer * 0.22f + air) * env, -1f, 1f);
        }

        return ClipFromData("DefaultSpeedBoost", data, rate);
    }

    AudioClip ClipFromData(string clipName, float[] data, int rate)
    {
        var clip = AudioClip.Create(clipName, data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    float NextNoise(ref uint seed)
    {
        seed = seed * 1664525u + 1013904223u;
        return ((seed >> 8) / 16777215f) * 2f - 1f;
    }
}
