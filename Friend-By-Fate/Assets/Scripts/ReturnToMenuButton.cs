using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.Scripting;


public class ReturnMenuButtonSpawner : MonoBehaviour
{
    [Header("Настройки таймера")]
    [SerializeField] private float _spawnDelay = 60f; // Время до появления кнопки
    [SerializeField] private float _holdDuration = 3f; // Время удержания для активации

    [Header("Настройки внешнего вида")]
    [SerializeField] private Color _buttonColor = new Color(0f, 0f, 0f, 0.7f); // Черный полупрозрачный
    [SerializeField] private Color _pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f); // Цвет при нажатии
    [SerializeField] private Color _fillColor = new Color(1f, 1f, 1f, 0.5f);   // Белый полупрозрачный для заполнения
    [SerializeField] private Vector2 _buttonSize = new Vector2(300, 80);      // Размер кнопки
    [SerializeField] private float _fontSize = 24f;
    [SerializeField] private string _buttonText = "УДЕРЖИВАЙ ДЛЯ СБРОСА";

    [Header("Сцена главного меню")]
    [Tooltip("Оставьте 0, если нужно грузить первую сцену в списке построения, или впишите имя сцены")]
    [SerializeField] private string _mainMenuSceneName = "";
    [SerializeField] private int _mainMenuSceneIndex = 0;

    private Button _spawnedButton;
    private Image _fillImage;
    private RectTransform _fillTransform;
    private Image _buttonImage;

    private bool _isHolding = false;
    private float _currentHoldTime = 0f;
    private bool _buttonActive = false;

    void Start()
    {
        if (_spawnDelay <= 0)
        {
            ShowButton();
        }
        else
        {
            Invoke(nameof(ShowButton), _spawnDelay);
        }
    }

    void ShowButton()
    {
        CreateButtonUI();
        _buttonActive = true;
        Debug.Log("Кнопка возврата в меню появилась!");
    }

    void CreateButtonUI()
    {
        // 1. Ищем существующий Canvas на сцене
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Если канваса нет (что вряд ли в UI сцене), создаем свой
            GameObject canvasObj = new GameObject("DynamicCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Важно: создаем EventSystem если его нет
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        // 2. Создаем объект кнопки
        GameObject btnObj = new GameObject("ReturnMenuButton");
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f); // Низ по центру
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0, 50); // Чуть выше самого низа
        btnRect.sizeDelta = _buttonSize;
        btnRect.SetAsLastSibling();

        // 3. Добавляем компоненты
        _spawnedButton = btnObj.AddComponent<Button>();
        _buttonImage = btnObj.AddComponent<Image>();
        _buttonImage.color = _buttonColor;

        // Настраиваем стандартные цвета кнопки, чтобы они не мешали нашей логике
        ColorBlock cb = _spawnedButton.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = Color.white;
        cb.selectedColor = Color.white;
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1;
        cb.fadeDuration = 0;
        _spawnedButton.colors = cb;
        _spawnedButton.interactable = true;

        // 4. Создаем текст
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = _buttonText;
        text.fontSize = _fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false; // Текст не должен перехватывать клики

        // 5. Создаем полоску заполнения
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(btnObj.transform, false);
        _fillTransform = fillObj.AddComponent<RectTransform>();

        _fillTransform.anchorMin = new Vector2(0, 0);
        _fillTransform.anchorMax = new Vector2(0, 1);
        _fillTransform.pivot = new Vector2(0, 0.5f);
        _fillTransform.sizeDelta = new Vector2(0, -10); // Чуть меньше высоты кнопки для отступа

        _fillImage = fillObj.AddComponent<Image>();
        _fillImage.color = _fillColor;
        _fillImage.raycastTarget = false; // Полоска не перехватывает клики

        // Порядок слоев: Фон (0), Полоска (1), Текст (2)
        _fillTransform.SetSiblingIndex(1);

        // 6. Добавляем обработчик событий через EventTrigger
        // Это надежнее, чем интерфейсы на родителе, так как события вешаются прямо на кнопку
        EventTrigger trigger = btnObj.AddComponent<EventTrigger>();

        // PointerDown
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        //pointerDownEntry.callback.AddListener((data) => OnPointerDown());
        pointerDownEntry.callback.AddListener(OnPointerDownEvent);
        trigger.triggers.Add(pointerDownEntry);

        // PointerUp
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        //pointerUpEntry.callback.AddListener((data) => OnPointerUp());
        pointerUpEntry.callback.AddListener(OnPointerUpEvent);
        trigger.triggers.Add(pointerUpEntry);

        // PointerExit (если увели мышку с кнопки во время удержания)
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        //pointerExitEntry.callback.AddListener((data) => OnPointerExit());
        pointerExitEntry.callback.AddListener(OnPointerExitEvent);
        trigger.triggers.Add(pointerExitEntry);
    }

    void Update()
    {
        if (!_buttonActive || !_isHolding) return;

        // Увеличиваем время удержания
        _currentHoldTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_currentHoldTime / _holdDuration);

        // Обновляем ширину полоски заполнения
        Vector2 newSize = _fillTransform.sizeDelta;
        newSize.x = _buttonSize.x * progress;
        _fillTransform.sizeDelta = newSize;

        // Проверка завершения
        if (progress >= 1f)
        {
            ResetProgressAndLoadMenu();
            _isHolding = false;
        }
    }

    private void OnPointerDownEvent(BaseEventData data)
    {
        OnPointerDown();
    }

    private void OnPointerUpEvent(BaseEventData data)
    {
        OnPointerUp();
    }

    private void OnPointerExitEvent(BaseEventData data)
    {
        OnPointerExit();
    }

    // Обработчики событий
    [Preserve]
    private void OnPointerDown()
    {
        if (!_buttonActive) return;
        _isHolding = true;
        _currentHoldTime = 0f;

        if (_buttonImage != null)
            _buttonImage.color = _pressedColor;
    }

    [Preserve]
    private void OnPointerUp()
    {
        ResetHoldState();
    }

    [Preserve]
    private void OnPointerExit()
    {
        ResetHoldState();
    }

    private void ResetHoldState()
    {
        _isHolding = false;
        _currentHoldTime = 0f;

        if (_buttonImage != null)
            _buttonImage.color = _buttonColor;

        if (_fillTransform != null)
        {
            Vector2 newSize = _fillTransform.sizeDelta;
            newSize.x = 0;
            _fillTransform.sizeDelta = newSize;
        }
    }

    void ResetProgressAndLoadMenu()
    {
        SaveManager.Instance.ResetAllProgress();
        SceneTransition.LoadScene(0);
    }
}