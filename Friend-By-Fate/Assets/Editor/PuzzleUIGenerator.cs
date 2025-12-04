using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;
using System.Reflection;

public class PuzzleUIGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Puzzle UI")]
    public static void CreatePuzzleUI()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            
            #if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            #endif
            Debug.Log("EventSystem создан");
        }

        // 1. Создаем Canvas
        GameObject canvasGO = new GameObject("PuzzleCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. Создаем Фон
        GameObject bgGO = CreateUIObject("Background", canvasGO.transform);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.95f, 0.95f, 0.95f);
        StretchToFill(bgGO.GetComponent<RectTransform>());

        // 3. Создаем Менеджер игры
        GameObject gmGO = new GameObject("GameManager");
        PuzzleManager manager = gmGO.AddComponent<PuzzleManager>();
        manager.backgroundPanel = bgImage;
        
        // 4. ЗАГОЛОВОЧНАЯ ПАНЕЛЬ С ФОНОМ
        GameObject headerGO = CreateUIObject("Header", bgGO.transform);
        RectTransform headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 0.78f); 
        headerRT.anchorMax = new Vector2(1, 0.95f); 
        headerRT.offsetMin = Vector2.zero;
        headerRT.offsetMax = Vector2.zero;

        // ФИОЛЕТОВЫЙ ФОН ДЛЯ ЗАГОЛОВКА
        Image headerBg = headerGO.AddComponent<Image>();
        headerBg.color = new Color(0.6f, 0.4f, 0.8f, 0.3f); 
        headerBg.raycastTarget = false; 

        // Основной заголовок
        GameObject titleGO = CreateUIObject("TitleText", headerGO.transform);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "PUZZLE GAME";
        titleText.fontSize = 90;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.black;

        Shadow titleShadow = titleGO.AddComponent<Shadow>();
        titleShadow.effectColor = Color.white;
        titleShadow.effectDistance = new Vector2(1, -1);

        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.6f);
        titleRT.anchorMax = new Vector2(1, 0.9f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        manager.titleText = titleText;

        // Подсказка управления
        GameObject hintGO = CreateUIObject("HintText", headerGO.transform);
        Text hintText = hintGO.AddComponent<Text>();
        hintText.text = "нажимайте на плитки для вращения";
        hintText.fontSize = 28; // Немного увеличим для лучшей видимости
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color = new Color(0.9f, 0.9f, 0.9f); // Светло-серый для контраста
        hintText.lineSpacing = 1.2f;

        // Тень для подсказки
        Shadow hintShadow = hintGO.AddComponent<Shadow>();
        hintShadow.effectColor = new Color(0, 0, 0, 0.5f);
        hintShadow.effectDistance = new Vector2(1, -1);

        RectTransform hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0, 0);
        hintRT.anchorMax = new Vector2(1, 0.45f);
        hintRT.offsetMin = Vector2.zero;
        hintRT.offsetMax = Vector2.zero;

        // 5. СЕТКА С ПЛИТКАМИ
        GameObject gridGO = CreateUIObject("GridContainer", bgGO.transform);
        RectTransform gridRT = gridGO.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.5f, 0.5f);
        gridRT.anchorMax = new Vector2(0.5f, 0.5f);
        gridRT.anchoredPosition = new Vector2(0, -30); 
        gridRT.sizeDelta = new Vector2(900, 900);
        
        GridLayoutGroup gridLayout = gridGO.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(400, 400);
        gridLayout.spacing = new Vector2(50, 50);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;

        // Создаем 4 плитки
        manager.tiles = new RotatableTile[4];
        Color[] tileColors = { 
            new Color(0.3f, 0.5f, 0.9f),   // Синий
            new Color(0.3f, 0.7f, 0.4f),   // Зеленый
            new Color(0.9f, 0.6f, 0.2f),   // Оранжевый
            new Color(0.8f, 0.4f, 0.9f)    // Фиолетовый
        };

        float[] correctRotations = { 0f, 90f, 180f, 270f };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject tileGO = CreateUIObject($"Tile_{i}", gridGO.transform);
            
            Image tileImg = tileGO.AddComponent<Image>();
            tileImg.color = tileColors[i];
            tileImg.raycastTarget = true;
            
            // Компонент вращения
            RotatableTile rotatable = tileGO.AddComponent<RotatableTile>();
            rotatable.correctRotation = correctRotations[i];
            rotatable.rotationDuration = 0.3f;
            rotatable.normalColor = tileColors[i];
            rotatable.correctColor = new Color(0.2f, 0.8f, 0.3f); // Ярко-зеленый
            rotatable.incorrectColor = new Color(0.9f, 0.3f, 0.3f); // Красный
            
            manager.tiles[i] = rotatable;

            // Иконка внутри (стрелка)
            GameObject iconGO = CreateUIObject("Icon", tileGO.transform);
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.color = new Color(1, 1, 1, 0.9f);
            iconImg.raycastTarget = false;
            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = new Vector2(180, 180);
            
            // Стрелка
            GameObject arrowTextGO = CreateUIObject("ArrowGuide", iconGO.transform);
            Text arrowText = arrowTextGO.AddComponent<Text>();
            arrowText.text = "▲";
            arrowText.fontSize = 120;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.resizeTextForBestFit = true;
            arrowText.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            arrowText.raycastTarget = false;
            StretchToFill(arrowTextGO.GetComponent<RectTransform>());
        }

        // 6. ПАНЕЛЬ ПОБЕДЫ
        GameObject winPanelGO = CreateUIObject("WinPanel", canvasGO.transform);
        Image winBg = winPanelGO.AddComponent<Image>();
        winBg.color = new Color(0, 0, 0, 0.92f);
        winBg.raycastTarget = true;
        StretchToFill(winPanelGO.GetComponent<RectTransform>());
        manager.winPanel = winPanelGO;

        // CanvasGroup для плавного появления
        CanvasGroup winCanvasGroup = winPanelGO.AddComponent<CanvasGroup>();
        winCanvasGroup.alpha = 0;
        winCanvasGroup.interactable = false;
        winCanvasGroup.blocksRaycasts = false;

        // Центральное сообщение победы
        GameObject winCenterGO = CreateUIObject("WinCenter", winPanelGO.transform);
        RectTransform centerRT = winCenterGO.GetComponent<RectTransform>();
        centerRT.anchorMin = new Vector2(0.1f, 0.3f);
        centerRT.anchorMax = new Vector2(0.9f, 0.7f);
        centerRT.offsetMin = Vector2.zero;
        centerRT.offsetMax = Vector2.zero;
        
        // Фон сообщения
        Image centerBg = winCenterGO.AddComponent<Image>();
        centerBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        centerBg.raycastTarget = false;

        // Основной текст победы
        GameObject winTextGO = CreateUIObject("WinText", winCenterGO.transform);
        Text winText = winTextGO.AddComponent<Text>();
        winText.text = "ПОБЕДА!\n\nУровень пройден!";
        winText.fontSize = 70;
        winText.fontStyle = FontStyle.Bold;
        winText.color = new Color(0.1f, 0.8f, 0.2f); // Ярко-зеленый
        winText.alignment = TextAnchor.MiddleCenter;
        winText.lineSpacing = 1.5f;
        
        RectTransform winTextRT = winTextGO.GetComponent<RectTransform>();
        winTextRT.anchorMin = new Vector2(0.1f, 0.4f);
        winTextRT.anchorMax = new Vector2(0.9f, 0.9f);
        winTextRT.offsetMin = Vector2.zero;
        winTextRT.offsetMax = Vector2.zero;

        // Изначально скрываем панель победы
        winPanelGO.SetActive(false);

        // 7. ВЫБИРАЕМ СОЗДАННЫЙ ОБЪЕКТ
        Selection.activeGameObject = gmGO;
        
        // 8. СОХРАНЯЕМ СЦЕНУ
        #if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        #endif
        
        Debug.Log("✅ UI успешно сгенерирован!");
        Debug.Log("📱 Инструкция:");
        Debug.Log("1. Нажмите Play для запуска игры");
        Debug.Log("2. Кликайте по плиткам для их вращения");
        Debug.Log("3. При правильном повороте плитка станет ярко-зеленой");
        Debug.Log("4. При правильном решении всех плиток появится сообщение победы");
    }

    // Вспомогательный метод для создания RectTransform
    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.AddComponent<RectTransform>();
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        return go;
    }

    // Растянуть на весь родительский объект
    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
    
    [MenuItem("Tools/Open Puzzle UI Generator")]
    public static void ShowWindow()
    {
        GetWindow<PuzzleUIGenerator>("Puzzle UI Generator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Генератор UI для игры-головоломки", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Создать UI для головоломки", GUILayout.Height(40)))
        {
            CreatePuzzleUI();
        }
        
        EditorGUILayout.Space();
        GUILayout.Label("Инструкция:", EditorStyles.boldLabel);
        GUILayout.Label("1. Нажмите кнопку выше для создания UI");
        GUILayout.Label("2. Проверьте, что в сцене есть EventSystem");
        GUILayout.Label("3. Нажмите Play для тестирования");
    }
}