using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager instance;

    [Header("Компоненты")]
    [SerializeField] private AudioSource audioSource;

    [Header("Звуки кнопок")]
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}