using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MessengerController : MonoBehaviour
{
    [Header("UI Components")]
    public Transform messagesContainer;   // Content
    public GameObject messagePrefab;      // Префаб сообщения
    public ScrollRect scrollRect;         // Scroll View для автопрокрутки

    [Header("Top Panel")]
    public GameObject topPanel;
    public Button blockButton;

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
    public string nextSceneName = "";

    [Header("Colors")]
    public Color textColor = new Color(0.2f, 0.6f, 0.9f);      // голубой цвет текста
    public Color backgroundColor = new Color(0.85f, 0.92f, 1f); // светло-голубой фон

    private bool isBlocked = false;
    private bool allMessagesShown = false;

    private void Start()
    {
        // Настройка чувствительности скролла для телефона
        if (scrollRect != null)
        {
            scrollRect.scrollSensitivity = 15f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.05f;
        }

        // Настройка внешнего вида кнопки
        if (blockButton != null)
        {
            blockButton.onClick.AddListener(OnBlockButtonPressed);
        }

        // Настройка цвета верхней панели
        if (topPanel != null)
        {
            Image panelImage = topPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.1f, 0.5f, 0.8f);
            }
        }

        // Запускаем отправку сообщений
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
            yield return new WaitForSeconds(4f);
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
            // Убираем цветные теги, оставляем только жирный текст и размер
            textComponent.text = $"<b><size=40>{senderName}</size></b>\n{messageText}";

            // Задаём цвет всего текста через код
            textComponent.color = textColor;
        }

        Image background = newMessage.GetComponent<Image>();
        if (background != null)
        {
            background.color = backgroundColor;
        }

        // Звук уведомления
        if (notificationSound != null)
        {
            notificationSound.Play();
        }

        // Автопрокрутка вниз
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

        // Меняем текст кнопки
        if (blockButton != null)
        {
            TMP_Text buttonText = blockButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "Заблокирован";
            }
            blockButton.interactable = false;
        }

        // Меняем цвет верхней панели
        if (topPanel != null)
        {
            Image panelImage = topPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.3f, 0.4f, 0.6f);
            }
        }

        // Останавливаем появление новых сообщений
        StopAllCoroutines();

        // Показываем сообщение о блокировке
        ShowBlockedMessage();
    }

    private void ShowBlockedMessage()
    {
        GameObject newMessage = Instantiate(messagePrefab, messagesContainer);

        TMP_Text textComponent = newMessage.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = $"<b>Система</b>\nПользователь заблокирован. Сообщения больше не принимаются.";
            textComponent.color = new Color(0.5f, 0.3f, 0.5f); // фиолетовый для системного сообщения
        }

        Image background = newMessage.GetComponent<Image>();
        if (background != null)
        {
            background.color = new Color(0.7f, 0.8f, 1f);
        }

        // Автопрокрутка вниз
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}