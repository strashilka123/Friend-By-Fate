using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering; // ОБЯЗАТЕЛЬНО для работы с размытием

public class Settings : MonoBehaviour
{
    public bool IsGodModeEnabled = false;
    float currentVolume = 0;

    public Text buttonText;
    public Slider volumeSlider;
    public AudioSource BackGroundAudio;
    public static event Action onVolumeChanged;

    [Header("Blur Settings")]
    public Volume blurVolume; // Перетащи сюда свой Global Volume
    public float fadeSpeed = 2f;

    void Start()
    {
        IsGodModeEnabled = PlayerPrefs.GetInt("IsGodModeEnabled", 0) == 1;
        float savedVolume = PlayerPrefs.GetFloat("VolumeLevel", 1);
        volumeSlider.value = savedVolume;
        if (BackGroundAudio != null) BackGroundAudio.volume = savedVolume;
        UpdateButtonText();

        // При старте размытие должно быть выключено
        if (blurVolume != null) blurVolume.weight = 0;
    }

    private void UpdateButtonText() => buttonText.text = IsGodModeEnabled ? "ВКЛ" : "ВЫКЛ";

    public void OnGodButtonClick()
    {
        IsGodModeEnabled = !IsGodModeEnabled;
        UpdateButtonText();
    }

    public void ChangeVolume()
    {
        if (BackGroundAudio != null) BackGroundAudio.volume = volumeSlider.value;
    }

    public void OnResetButtonClicked()
    {
        PlayerPrefs.DeleteAll();
    }

    // Метод для вызова из Главного Меню (на кнопку Settings)
    public void OpenSettings(GameObject mainMenu)
    {
        mainMenu.SetActive(false); // Выключаем кнопки меню
        gameObject.SetActive(true); // Включаем этот объект настроек
        StopAllCoroutines();
        StartCoroutine(FadeBlur(1f)); // Плавно размываем
    }

    public void OnBackButtonClicked()
    {
        if (BackGroundAudio != null) BackGroundAudio.volume = currentVolume;
        CloseSettings();
    }

    public void OnSaveButtonClicked()
    {
        PlayerPrefs.SetInt("IsGodModeEnabled", IsGodModeEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("VolumeLevel", volumeSlider.value);
        onVolumeChanged?.Invoke();
        PlayerPrefs.Save();
        CloseSettings();
    }

    private void CloseSettings()
    {
        StopAllCoroutines();
        StartCoroutine(FadeBlur(0f, () => gameObject.SetActive(false)));
    }

    private void OnEnable()
    {
        currentVolume = PlayerPrefs.GetFloat("VolumeLevel", 1f);
        volumeSlider.value = currentVolume;
    }

    // Корутина для плавного размытия
    private IEnumerator FadeBlur(float targetWeight, Action onComplete = null)
    {
        if (blurVolume == null) yield break;

        while (!Mathf.Approximately(blurVolume.weight, targetWeight))
        {
            blurVolume.weight = Mathf.MoveTowards(blurVolume.weight, targetWeight, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }
        onComplete?.Invoke();
    }
}