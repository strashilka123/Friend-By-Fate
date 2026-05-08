using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MessengerController : MonoBehaviour
{
    [Header("UI Components")]
    public Transform messagesContainer;
    public GameObject messagePrefab;
    public ScrollRect scrollRect;

    [Header("Top Panel")]
    public GameObject topPanel;
    public Button blockButton;

    [Header("Сохранения")]
    public string qId = "Massengers";

    [Header("Messages")]
    public string senderName = "Неизвестно";

    [TextArea(2, 4)]
    public string[] messages = new string[]
    {
        "Чем ты вообще живешь? Сериалы, еда и жалость к себе?",
        "У тебя даже хобби нет.",
        "Ты — пустое место.",
        "Ты просто испуганный ребёнок, который боится живых людей.",
        "Ты даже обижаться нормально не умеешь.",
        "Наверное, прочитаешь это, вздохнёшь и пойдёшь дальше лить свою тоску в потолок.",
        "Ни характера, ни искры.",
        "Ты просто пятно на диване."
    };

    [Header("Settings")]
    public float minDelay = 2f;
    public float maxDelay = 4f;

    [Header("Sound")]
    public AudioSource notificationSound;

    [Header("Next Scene")]
    public string nextSceneName = "KitchenTruth";

    [Header("Colors")]
    public Color textColor = new Color(0.2f, 0.6f, 0.9f);
    public Color backgroundColor = new Color(0.85f, 0.92f, 1f);

    private bool isBlocked = false;
    private bool allMessagesShown = false;

    private void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.scrollSensitivity = 15f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.05f;
        }

        if (blockButton != null)
        {
            blockButton.onClick.AddListener(OnBlockButtonPressed);
        }

        if (topPanel != null)
        {
            Image panelImage = topPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.1f, 0.5f, 0.8f);
            }
        }

        StartCoroutine(SendMessages());
    }

    private IEnumerator SendMessages()
    {
        for (int i = 0; i < messages.Length; i++)
        {
            if (isBlocked) yield break;

            SendMessageToChat(messages[i]);

            if (i < messages.Length - 1)
            {
                float randomDelay = Random.Range(minDelay, maxDelay);
                yield return new WaitForSeconds(randomDelay);
            }
        }

        allMessagesShown = true;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            yield return new WaitForSeconds(3f);

            SaveProgress();

            SceneTransition.LoadScene(nextSceneName);
        }
    }

    private void SendMessageToChat(string messageText)
    {
        if (isBlocked) return;

        GameObject newMessage = Instantiate(messagePrefab, messagesContainer);

        TMP_Text textComponent = newMessage.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = $"<b><size=40>{senderName}</size></b>\n{messageText}";
            textComponent.color = textColor;
        }

        Image background = newMessage.GetComponent<Image>();
        if (background != null)
        {
            background.color = backgroundColor;
        }

        if (notificationSound != null)
        {
            notificationSound.Play();
        }

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void OnBlockButtonPressed()
    {
        if (isBlocked) return;

        isBlocked = true;
        Debug.Log("Пользователь заблокирован!");

        if (blockButton != null)
        {
            TMP_Text buttonText = blockButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = "Заблокирован";
            blockButton.interactable = false;
        }

        if (topPanel != null)
        {
            Image panelImage = topPanel.GetComponent<Image>();
            if (panelImage != null) panelImage.color = new Color(0.3f, 0.4f, 0.6f);
        }

        StopAllCoroutines();
        ShowBlockedMessage();

        SaveProgress();

        SceneTransition.LoadScene(nextSceneName);
    }

    private void SaveProgress()
    {
        if (SaveManager.Instance != null)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SaveManager.Instance.SaveLastScene(currentSceneIndex - 1);
        }
    }

    private void ShowBlockedMessage()
    {
        GameObject newMessage = Instantiate(messagePrefab, messagesContainer);
        TMP_Text textComponent = newMessage.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = $"<b>Система</b>\nПользователь заблокирован. Сообщения больше не принимаются.";
            textComponent.color = new Color(0.5f, 0.3f, 0.5f);
        }

        Image background = newMessage.GetComponent<Image>();
        if (background != null) background.color = new Color(0.7f, 0.8f, 1f);

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}