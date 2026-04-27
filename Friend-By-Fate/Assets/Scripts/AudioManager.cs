using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("QTE Звуки")]
    public AudioClip qteSuccess;
    public AudioClip qteFail;

    [Header("Результат")]
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Атмосфера")]
    public AudioClip barAmbience;

    [Header("Громкость")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f; // ← Регулируйте в Inspector!

    [Range(0f, 1f)]
    public float ambienceVolume = 0.3f; // ← Регулируйте в Inspector!

    private AudioSource sfxSource;
    private AudioSource ambienceSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;
        ambienceSource.volume = ambienceVolume;

        if (barAmbience != null)
        {
            ambienceSource.clip = barAmbience;
            ambienceSource.Play();
        }
    }

    public void PlayQTESuccess()
    {
        if (qteSuccess != null)
            sfxSource.PlayOneShot(qteSuccess, sfxVolume);
    }

    public void PlayQTEFail()
    {
        if (qteFail != null)
            sfxSource.PlayOneShot(qteFail, sfxVolume);
    }

    public void PlayWinSound()
    {
        if (winSound != null)
            sfxSource.PlayOneShot(winSound, sfxVolume);
    }

    public void PlayLoseSound()
    {
        if (loseSound != null)
            sfxSource.PlayOneShot(loseSound, sfxVolume);
    }

    public void StopAmbience()
    {
        if (ambienceSource.isPlaying)
            ambienceSource.Stop();
    }

    // Метод для изменения громкости во время игры
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        ambienceSource.volume = ambienceVolume;
    }
}