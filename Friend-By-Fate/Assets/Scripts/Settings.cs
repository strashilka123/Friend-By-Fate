using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Настройки звука")]
    public Slider volumeSlider;
    public Text volumePercentageText;
    public AudioSource BackGroundAudio;

    private const string MusicVolumeKey = "VolumeLevel";
    private float volumeBeforeOpening;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.4f);
        Debug.Log($"[Settings] Start: loaded saved volume {savedVolume:F2}");
        ApplyMusicVolume(savedVolume);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        UpdateVolumeText(savedVolume);
        volumeBeforeOpening = savedVolume;
    }

    private void OnEnable()
    {
        float currentVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.4f);
        Debug.Log($"[Settings] OnEnable: current saved volume {currentVolume:F2}");
        volumeBeforeOpening = currentVolume;

        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
        }

        UpdateVolumeText(currentVolume);
    }

    public void ChangeVolume()
    {
        if (volumeSlider == null)
        {
            return;
        }

        Debug.Log($"[Settings] ChangeVolume: {volumeSlider.value:F2}");
        ApplyMusicVolume(volumeSlider.value);
        UpdateVolumeText(volumeSlider.value);
    }

    public void OnSaveButtonClicked()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, volumeSlider.value);
            PlayerPrefs.Save();
            Debug.Log($"[Settings] Volume saved: {volumeSlider.value:F2}");
        }

        gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        Debug.Log($"[Settings] Reverting volume to {volumeBeforeOpening:F2}");
        ApplyMusicVolume(volumeBeforeOpening);

        if (volumeSlider != null)
        {
            volumeSlider.value = volumeBeforeOpening;
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

    private void UpdateVolumeText(float vol)
    {
        if (volumePercentageText != null)
        {
            volumePercentageText.text = Mathf.RoundToInt(vol * 100) + "%";
        }
    }
}
