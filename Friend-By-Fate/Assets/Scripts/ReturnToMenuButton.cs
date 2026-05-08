using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class ReturnToMenuButton : MonoBehaviour
{
    [Header("Settings")]
    public float spawnDelay = 2.0f;
    public string buttonText = "ВОЗВРАТ В ГЛАВНОЕ МЕНЮ";
    public Color buttonColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    public Color fillColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
    public Color textColor = Color.white;
    public Vector2 buttonSize = new Vector2(350, 80);
    public float bottomMargin = 50f;
    public float holdDuration = 3.0f;

    private Image fillImage;
    private bool isHolding = false;
    private float currentHoldTime = 0f;
    private Font targetFont;
    private GameObject spawnedCanvasObj;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Очищаем старую кнопку, если она осталась с прошлой сцены
        if (spawnedCanvasObj != null) Destroy(spawnedCanvasObj);

        StopAllCoroutines();
        StartCoroutine(SpawnCoroutine());
    }

    // Убрали запуск из Start(), чтобы не было дублей

    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSecondsRealtime(spawnDelay);
        CreateButtonWithOwnCanvas();
    }

    void CreateButtonWithOwnCanvas()
    {
        // 1. Создаем персональный Canvas для кнопки, чтобы никто её не перекрыл
        spawnedCanvasObj = new GameObject("ReturnButton_RootCanvas");
        Canvas canvas = spawnedCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        CanvasScaler scaler = spawnedCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1.0f;

        spawnedCanvasObj.AddComponent<GraphicRaycaster>();

        // 2. EventSystem (если нет в сцене)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

 
        if (buttonSize.x == 350 && buttonSize.y == 80) 
        {
            buttonSize = new Vector2(500, 120); 
            bottomMargin = 80f; 
        }

        // 3. Сама кнопка
        GameObject btnObj = new GameObject("ReturnButton_Body");
        btnObj.transform.SetParent(spawnedCanvasObj.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f); // Низ-центр
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = buttonSize;
        rect.anchoredPosition = new Vector2(0, bottomMargin);

        Image bgImage = btnObj.AddComponent<Image>();
        bgImage.color = buttonColor;
        bgImage.sprite = CreateSimpleSprite();

        // 4. Слой прогресса (Fill)
        GameObject fillObj = new GameObject("FillProgress");
        fillObj.transform.SetParent(btnObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.sprite = CreateSimpleSprite();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0;

        // 5. Текст (Legacy Text)
        GameObject textObj = new GameObject("BtnText");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = buttonText;

        // Загрузка шрифта
        if (targetFont == null) targetFont = Resources.Load<Font>("MontserratAlternates-Regular");
        t.font = targetFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        t.color = textColor;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontSize = 29;
        t.raycastTarget = false; // Текст не должен мешать кликам

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // 6. Логика удержания
        EventTrigger trigger = btnObj.AddComponent<EventTrigger>();
        AddEvent(trigger, EventTriggerType.PointerDown, () => { isHolding = true; currentHoldTime = 0; });
        AddEvent(trigger, EventTriggerType.PointerUp, () => { isHolding = false; if (fillImage) fillImage.fillAmount = 0; });
        AddEvent(trigger, EventTriggerType.PointerExit, () => { isHolding = false; if (fillImage) fillImage.fillAmount = 0; });

        Debug.Log("[MenuButton] Кнопка создана на отдельном Canvas (Order 999) с FullHD референсом.");
    }

    void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => action());
        trigger.triggers.Add(entry);
    }

    void Update()
    {
        if (isHolding && fillImage != null)
        {
            currentHoldTime += Time.unscaledDeltaTime;
            fillImage.fillAmount = Mathf.Clamp01(currentHoldTime / holdDuration);

            if (currentHoldTime >= holdDuration)
            {
                isHolding = false;
                Debug.Log("[MenuButton] Возвращаемся в меню...");
                SceneTransition.LoadScene(0);
            }
        }
    }

    Sprite CreateSimpleSprite()
    {
        Texture2D tex = new Texture2D(2, 2);
        for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++) tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
    }
}