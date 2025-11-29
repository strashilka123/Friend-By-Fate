using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class Settings : MonoBehaviour
{
    public bool IsGodModeEnabled = false;
    float currentVolume = 0;

    public Text buttonText;

    public Slider volumeSlider;

    public AudioSource BackGroundAudio;
    public static event Action onVolumeChanged;
    
    void Start()
    {
        IsGodModeEnabled = PlayerPrefs.GetInt("IsGodModeEnabled", 0) == 1;
        float savedVolume = PlayerPrefs.GetFloat("VolumeLevel", 1);
        volumeSlider.value = savedVolume;
        BackGroundAudio.volume = savedVolume;
        UpdateButtonText();
    }
    private void UpdateButtonText()
    {
        buttonText.text = IsGodModeEnabled ? "Вкл" : "Выкл";
    }
    public void OnGodButtonClick()
    {
        IsGodModeEnabled = !IsGodModeEnabled;
        UpdateButtonText();
    }

    public void ChangeVolume()
    {
        BackGroundAudio.volume = volumeSlider.value;
    }

    public void OnResetButtonClicked()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OnBackButtonClicked()
    {
        BackGroundAudio.volume = currentVolume;
        gameObject.SetActive(false);
    }

    public void OnSaveButtonClicked()
    {
        PlayerPrefs.SetInt("IsGodModeEnabled", IsGodModeEnabled ? 1 : 0);

        PlayerPrefs.SetFloat("VolumeLevel", BackGroundAudio.volume);
        onVolumeChanged?.Invoke();
        PlayerPrefs.Save();

        gameObject.SetActive(false);

    }

    private void OnEnable()
    {
        currentVolume = PlayerPrefs.GetFloat("VolumeLevel");
        volumeSlider.value = currentVolume;
    }
}