using UnityEngine;
using UnityEngine.UI;
using TMPro; // Поддержка TextMeshPro, если используется он

public class EndingSceneController : MonoBehaviour
{
    [Header("Настройки текста")]
    [Tooltip("Ссылка на компонент текста. Если не назначено, будет найден первый в сцене.")]
    public Graphic targetText;

    [Header("Настройки анимации")]
    [Tooltip("Минимальный размер шрифта")]
    public float minSize = 40f;
    [Tooltip("Максимальный размер шрифта")]
    public float maxSize = 80f;
    [Tooltip("Скорость пульсации (чем меньше, тем быстрее)")]
    public float speed = 2f;

    private float _initialSize;
    private bool _isGrowing = true;

    void Start()
    {
        // Если текст не назначен вручную, пытаемся найти его в сцене
        if (targetText == null)
        {
            // Пробуем найти Text (Legacy)
            targetText = FindObjectOfType<Text>();

            // Если не нашли, пробуем найти TextMeshProUGUI
            if (targetText == null)
            {
                var tmpText = FindObjectOfType<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    // Для TMP мы будем работать через компонент, но логика размера похожа
                    // Сохраним ссылку как графический объект для универсальности, 
                    // но размер шрифта будем менять отдельно
                    targetText = tmpText;
                }
            }

            if (targetText == null)
            {
                Debug.LogError("Текст для концовки не найден в сцене! Добавьте компонент Text или TextMeshProUGUI.");
                enabled = false;
                return;
            }
        }

        // Инициализация начального размера
        if (targetText is Text legacyText)
        {
            _initialSize = legacyText.fontSize;
            // Обновляем мин/макс если они равны 0 (дефолт инспектора), чтобы не ломать анимацию
            if (minSize <= 0) minSize = _initialSize * 0.8f;
            if (maxSize <= 0) maxSize = _initialSize * 1.2f;
        }
        else if (targetText is TextMeshProUGUI tmpText)
        {
            _initialSize = tmpText.fontSize;
            if (minSize <= 0) minSize = _initialSize * 0.8f;
            if (maxSize <= 0) maxSize = _initialSize * 1.2f;
        }
    }

    void Update()
    {
        if (targetText == null) return;

        // Вычисляем текущий размер с помощью синусоиды для плавности
        // Mathf.PingPong тоже подходит, но Sin дает более мягкие переходы на пиках
        float timeValue = Time.time * speed;
        float normalizedValue = (Mathf.Sin(timeValue) + 1) / 2; // От 0 до 1

        float currentSize = Mathf.Lerp(minSize, maxSize, normalizedValue);

        // Применяем размер в зависимости от типа текста
        if (targetText is Text legacyText)
        {
            legacyText.fontSize = (int)currentSize;
        }
        else if (targetText is TextMeshProUGUI tmpText)
        {
            tmpText.fontSize = currentSize;
        }
    }
}