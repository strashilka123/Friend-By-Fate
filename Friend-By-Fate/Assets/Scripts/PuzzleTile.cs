using UnityEngine;
using TMPro; // Для работы с текстом TextMeshPro

public class PuzzleTile : MonoBehaviour
{
    // --- ГЛОБАЛЬНЫЙ ФЛАГ СОСТОЯНИЯ ИГРЫ ---
    // Переменная, которая блокирует вращение после победы.
    public static bool isGameOver = false; 
    
    // --- Настройки в Инспекторе ---
    
    [Header("Настройка Плитки")]
    // Угол, на который плитка должна повернуться для победы (0, 90, 180, 270)
    public float winRotationZ = 0f; 

    // Скорость вращения (200 - значение для плавного поворота)
    public float rotationSpeed = 200f; 

    [Header("UI Настройка")]
    // Ссылка на текстовый элемент для сообщения о победе
    public TMP_Text winText;
    // Ссылка на фоновое изображение для сообщения о победе (чтобы улучшить читаемость)
    public GameObject winBackground; 
    
    // --- Внутренние переменные ---
    
    // Флаг, который показывает, вращается ли плитка сейчас
    private bool isRotating = false;
    
    // Целевой угол вращения Z
    private float targetRotationZ; 

    // Вызывается один раз, когда скрипт начинает работу
    void Start()
    {
        // При старте новой игры убеждаемся, что флаг сброшен (важно, если перезапускать сцену)
        // Это сработает только для первой плитки, которая запустится.
        if (transform.parent != null && transform.parent.GetChild(0).gameObject == gameObject)
        {
             isGameOver = false;
        }

        // Устанавливаем цель вращения равной текущему углу (0, 90, 180 или 270)
        targetRotationZ = transform.localEulerAngles.z; 
    }

    // Вызывается каждый кадр
    void Update()
    {
        // Если плитка вращается, плавно поворачиваем ее к цели
        if (isRotating)
        {
            // Плавно двигаемся к целевому углу
            float newRotationZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, targetRotationZ, rotationSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, 0, newRotationZ);

            // Если достигли цели, останавливаем вращение и проверяем победу
            if (Mathf.Approximately(transform.localEulerAngles.z, targetRotationZ))
            {
                isRotating = false;
                CheckForWin();
            }
        }
    }

    // Обработка клика мыши по объекту (требуется Box Collider 2D)
    void OnMouseDown()
    {
        // Блокировка клика, если игра окончена
        if (isGameOver) 
        {
            return; // Выходим из функции, игнорируя вращение
        }

        // Если плитка уже вращается, игнорируем клик
        if (isRotating) return; 

        // Задаем новый целевой угол (поворот на 90 градусов)
        targetRotationZ += 90f;
        
        // Убеждаемся, что угол остается в пределах 0-360
        if (targetRotationZ >= 360f)
        {
            targetRotationZ -= 360f;
        }

        isRotating = true; // Начинаем вращение
    }

    // Проверка, собрана ли вся головоломка
    void CheckForWin()
    {
        // 1. Проверяем, находится ли текущая плитка в правильном положении
        bool isCorrect = Mathf.Approximately(targetRotationZ, winRotationZ);

        // 2. Если плитка в правильном положении, проверяем все остальные плитки
        if (isCorrect)
        {
            // Находим все объекты со скриптом PuzzleTile на сцене
            PuzzleTile[] allTiles = FindObjectsOfType<PuzzleTile>();
            int correctTilesCount = 0;

            foreach (PuzzleTile tile in allTiles)
            {
                // Проверяем, находится ли КАЖДАЯ плитка в своем правильном положении
                if (Mathf.Approximately(tile.targetRotationZ, tile.winRotationZ))
                {
                    correctTilesCount++;
                }
            }

            // Если количество правильно собранных плиток равно общему количеству плиток (4)
            if (correctTilesCount == allTiles.Length)
            {
                // Устанавливаем флаг, чтобы заблокировать дальнейшие клики
                isGameOver = true; 
                
                // ПОБЕДА! Показываем текст и фон
                Debug.Log("Вы восстановили порванную фотографию!");

                // Активируем текстовое сообщение
                if (winText != null) 
                {
                    winText.gameObject.SetActive(true);
                }
                
                // Активируем фоновое изображение
                if (winBackground != null) 
                {
                    winBackground.SetActive(true);
                }
            }
        }
    }
}