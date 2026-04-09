using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Настройки звука")]
    public Slider volumeSlider;
    public Text volumePercentageText; 
    public AudioSource BackGroundAudio;

    private float volumeBeforeOpening;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("VolumeLevel", 0.4f);

        if (BackGroundAudio != null) BackGroundAudio.volume = savedVolume;
        if (volumeSlider != null) volumeSlider.value = savedVolume;

        UpdateVolumeText(savedVolume);
        volumeBeforeOpening = savedVolume;
    }

    private void OnEnable()
    {
        if (BackGroundAudio != null)
        {
            volumeBeforeOpening = BackGroundAudio.volume;
            if (volumeSlider != null) volumeSlider.value = volumeBeforeOpening;
            UpdateVolumeText(volumeBeforeOpening);
        }
    }

    public void ChangeVolume()
    {
        if (BackGroundAudio != null && volumeSlider != null)
        {
            BackGroundAudio.volume = volumeSlider.value;
            UpdateVolumeText(volumeSlider.value);
        }
    }

    private void UpdateVolumeText(float vol)
    {
        if (volumePercentageText != null)
        {
            volumePercentageText.text = Mathf.RoundToInt(vol * 100) + "%";
        }
    }

    public void OnSaveButtonClicked()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("VolumeLevel", volumeSlider.value);
            PlayerPrefs.Save();
        }
        gameObject.SetActive(false);
    }

    public void OnBackButtonClicked()
    {
        if (BackGroundAudio != null)
        {
            BackGroundAudio.volume = volumeBeforeOpening;
        }
        gameObject.SetActive(false);
    }
}