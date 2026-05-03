using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string MusicVolumeKey = "VolumeLevel";

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";

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
    [Range(0f, 1f)] public float ambienceVolume = 0.4f;

    private AudioSource sfxSource;
    private AudioSource ambienceSource;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("[AudioManager] Duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[AudioManager] Instance initialized and marked DontDestroyOnLoad.");
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;

        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, ambienceVolume);
        SetMusicVolume(savedMusicVolume, false);

        ApplySceneMusicConfig(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneMusicConfig(scene);
        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, ambienceVolume);
        SetMusicVolume(savedMusicVolume, false);
    }

    private void ApplySceneMusicConfig(Scene scene)
    {
        SceneMusicConfig sceneMusic = FindObjectOfType<SceneMusicConfig>();
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
}
