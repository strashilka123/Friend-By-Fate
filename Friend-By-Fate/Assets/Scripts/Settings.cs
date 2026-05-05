using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Настройки звука")]
    public Slider musicVolumeSlider;
    public Text musicVolumePercentageText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumePercentageText;
    public Slider voiceVolumeSlider;
    public Text voiceVolumePercentageText;
    public AudioSource BackGroundAudio;

    private const string MusicVolumeKey = "VolumeLevel";
    private const string VoiceVolumeKey = "VoiceVolumeLevel";
    private float musicVolumeBeforeOpening;
    private float voiceVolumeBeforeOpening;

    private Slider ActiveMusicSlider => musicVolumeSlider != null ? musicVolumeSlider : volumeSlider;
    private Text ActiveMusicText => musicVolumePercentageText != null ? musicVolumePercentageText : volumePercentageText;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.3f);
        float savedVoiceVolume = PlayerPrefs.GetFloat(VoiceVolumeKey, 0.8f);
        Debug.Log($"[Settings] Start: loaded saved volume {savedVolume:F2}");
        ApplyMusicVolume(savedVolume);
        ApplyVoiceVolume(savedVoiceVolume);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = savedVolume;
        }

        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = savedVoiceVolume;
        }

        UpdateMusicVolumeText(savedVolume);
        UpdateVoiceVolumeText(savedVoiceVolume);
        musicVolumeBeforeOpening = savedVolume;
        voiceVolumeBeforeOpening = savedVoiceVolume;
    }

    private void OnEnable()
    {
        float currentVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.3f);
        float currentVoiceVolume = PlayerPrefs.GetFloat(VoiceVolumeKey, 0.8f);
        musicVolumeBeforeOpening = currentVolume;
        voiceVolumeBeforeOpening = currentVoiceVolume;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = currentVolume;
        }

        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = currentVoiceVolume;
        }

        UpdateMusicVolumeText(currentVolume);
        UpdateVoiceVolumeText(currentVoiceVolume);
    }

    public void ChangeMusicVolume()
    {
        if (ActiveMusicSlider == null)
        {
            return;
        }

        Debug.Log($"[Settings] ChangeMusicVolume: {ActiveMusicSlider.value:F2}");
        ApplyMusicVolume(ActiveMusicSlider.value);
        UpdateMusicVolumeText(ActiveMusicSlider.value);
    }

    public void ChangeVolume()
    {
        ChangeMusicVolume();
    }

    public void ChangeVoiceVolume()
    {
        if (voiceVolumeSlider == null)
        {
            return;
        }

        Debug.Log($"[Settings] ChangeVoiceVolume: {voiceVolumeSlider.value:F2}");
        ApplyVoiceVolume(voiceVolumeSlider.value);
        UpdateVoiceVolumeText(voiceVolumeSlider.value);
    }

    public void OnSaveButtonClicked()
    {
        if (ActiveMusicSlider != null)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, ActiveMusicSlider.value);
            Debug.Log($"[Settings] Music volume saved: {ActiveMusicSlider.value:F2}");
        }

        if (voiceVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(VoiceVolumeKey, voiceVolumeSlider.value);
            Debug.Log($"[Settings] Voice volume saved: {voiceVolumeSlider.value:F2}");
        }

        PlayerPrefs.Save();

        gameObject.SetActive(false);
    }


    public void OnBackButtonClicked()
    {
        ApplyMusicVolume(musicVolumeBeforeOpening);
        ApplyVoiceVolume(voiceVolumeBeforeOpening);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolumeBeforeOpening;
        }

        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = voiceVolumeBeforeOpening;
        }

        gameObject.SetActive(false);
    }

    private void ApplyMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (BackGroundAudio != null)
        {
            BackGroundAudio.volume = volume;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume, false);
        }
    }

    private void ApplyVoiceVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVoiceVolume(volume, false);
        }
    }

    private void UpdateMusicVolumeText(float vol)
    {
        if (ActiveMusicText != null)
        {
            ActiveMusicText.text = Mathf.RoundToInt(vol * 100) + "%";
        }
    }

    private void UpdateVoiceVolumeText(float vol)
    {
        if (voiceVolumePercentageText != null)
            voiceVolumePercentageText.text = Mathf.RoundToInt(vol * 100) + "%";
    }
}
