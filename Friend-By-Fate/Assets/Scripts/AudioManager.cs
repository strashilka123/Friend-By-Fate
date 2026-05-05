using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string MusicVolumeKey = "VolumeLevel";
    private const string VoiceVolumeKey = "VoiceVolumeLevel";

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string voiceVolumeParameter = "VoiceVolume";

    [Header("QTE Звуки")]
    public AudioClip qteSuccess;
    public AudioClip qteFail;

    [Header("Результат")]
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Fallback музыка")]
    public AudioClip barAmbience;

    [Header("Громкость")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0f, 1f)] public float ambienceVolume = 0.3f;
    [Range(0f, 1f)] public float voiceVolume = 0.8f;

    private AudioSource sfxSource;
    private AudioSource ambienceSource;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.AbsorbSceneAudioSettings(this);
            //Debug.Log("[AudioManager] Duplicate AudioManager component removed after syncing scene settings.");
            Destroy(this);
            return;
        }

        Instance = this;
        Debug.Log("[AudioManager] Instance initialized.");

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;

        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, ambienceVolume);
        SetMusicVolume(savedMusicVolume, false);
        float savedVoiceVolume = PlayerPrefs.GetFloat(VoiceVolumeKey, voiceVolume);
        SetVoiceVolume(savedVoiceVolume, false);

        ApplySceneMusicConfig(SceneManager.GetActiveScene());

        DontDestroyOnLoad(gameObject);
    }

    private void AbsorbSceneAudioSettings(AudioManager source)
    {
        if (source == null)
        {
            return;
        }

        if (source.qteSuccess != null) qteSuccess = source.qteSuccess;
        if (source.qteFail != null) qteFail = source.qteFail;
        if (source.winSound != null) winSound = source.winSound;
        if (source.loseSound != null) loseSound = source.loseSound;
        if (source.barAmbience != null) barAmbience = source.barAmbience;

        if (source.audioMixer != null) audioMixer = source.audioMixer;
        if (!string.IsNullOrWhiteSpace(source.musicVolumeParameter)) musicVolumeParameter = source.musicVolumeParameter;
        if (!string.IsNullOrWhiteSpace(source.voiceVolumeParameter)) voiceVolumeParameter = source.voiceVolumeParameter;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DisableSceneAutoplayMusicSources(scene);
        ApplySceneMusicConfig(scene);
        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, ambienceVolume);
        SetMusicVolume(savedMusicVolume, false);
        float savedVoiceVolume = PlayerPrefs.GetFloat(VoiceVolumeKey, voiceVolume);
        SetVoiceVolume(savedVoiceVolume, false);
    }

    private void DisableSceneAutoplayMusicSources(Scene scene)
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null || source == sfxSource || source == ambienceSource)
            {
                continue;
            }

            if (source.gameObject.scene != scene)
            {
                continue;
            }

            if (source.clip != null && source.playOnAwake && source.loop)
            {
                source.Stop();
                source.enabled = false;
                Debug.Log($"[AudioManager] Disabled scene autoplay music source '{source.gameObject.name}' in '{scene.name}'.");
            }
        }
    }

    private void ApplySceneMusicConfig(Scene scene)
    {
        SceneMusicConfig sceneMusic = FindSceneMusicConfig(scene);
        if (sceneMusic != null && sceneMusic.PlayOnSceneLoad)
        {
            Debug.Log($"[AudioManager] Scene music config found in '{scene.name}'.");
            PlayBackgroundMusic(sceneMusic.MusicClip, sceneMusic.MusicOutputGroup, sceneMusic.Loop);
            return;
        }

        if (!ambienceSource.isPlaying && barAmbience != null)
        {
            Debug.Log($"[AudioManager] No SceneMusicConfig in '{scene.name}', fallback ambience started.");
            PlayBackgroundMusic(barAmbience, null, true);
        }
    }

    private SceneMusicConfig FindSceneMusicConfig(Scene scene)
    {
        SceneMusicConfig[] configs = FindObjectsByType<SceneMusicConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (SceneMusicConfig config in configs)
        {
            if (config.gameObject.scene == scene)
            {
                return config;
            }
        }

        return null;
    }


    public void PlayBackgroundMusic(AudioClip clip, AudioMixerGroup outputGroup, bool loop = true)
    {
        if (clip == null)
        {
            return;
        }

        ambienceSource.loop = loop;
        if (outputGroup != null)
        {
            ambienceSource.outputAudioMixerGroup = outputGroup;
        }

        if (ambienceSource.clip == clip && ambienceSource.isPlaying)
        {
            return;
        }

        ambienceSource.clip = clip;
        ambienceSource.Play();
        Debug.Log($"[AudioManager] Background music started: {clip.name}");
    }

    public void PlayQTESuccess() { if (qteSuccess != null) sfxSource.PlayOneShot(qteSuccess, sfxVolume); }
    public void PlayQTEFail() { if (qteFail != null) sfxSource.PlayOneShot(qteFail, sfxVolume); }
    public void PlayWinSound() { if (winSound != null) sfxSource.PlayOneShot(winSound, sfxVolume); }
    public void PlayLoseSound() { if (loseSound != null) sfxSource.PlayOneShot(loseSound, sfxVolume); }
    public void StopAmbience() { if (ambienceSource.isPlaying) ambienceSource.Stop(); }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float normalizedVolume, bool save = true)
    {
        ambienceVolume = Mathf.Clamp01(normalizedVolume);
        ambienceSource.volume = ambienceVolume;

        if (audioMixer != null)
        {
            float db = ambienceVolume <= 0.0001f ? -80f : Mathf.Log10(ambienceVolume) * 20f;
            audioMixer.SetFloat(musicVolumeParameter, db);
        }

        if (save)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, ambienceVolume);
            PlayerPrefs.Save();
        }

        Debug.Log($"[AudioManager] Music volume set to {ambienceVolume:F2}");
    }

    public void SetVoiceVolume(float normalizedVolume, bool save = true)
    {
        voiceVolume = Mathf.Clamp01(normalizedVolume);

        if (audioMixer != null)
        {
            float db = voiceVolume <= 0.0001f ? -80f : Mathf.Log10(voiceVolume) * 20f;
            audioMixer.SetFloat(voiceVolumeParameter, db);
        }

        if (save)
        {
            PlayerPrefs.SetFloat(VoiceVolumeKey, voiceVolume);
            PlayerPrefs.Save();
        }

        Debug.Log($"[AudioManager] Voice volume {voiceVolume:F2}");
    }
}
