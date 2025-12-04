using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class RotatableTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Settings")]
    public float correctRotation = 0f;
    public float rotationDuration = 0.3f;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;

    private bool isRotating = false;
    private PuzzleManager puzzleManager;

    private void Start()
    {
        puzzleManager = FindAnyObjectByType<PuzzleManager>();
        
        if (puzzleManager == null)
        {
            Debug.LogError("PuzzleManager не найден!");
        }
        
        // Случайно вращаем плитку при старте
        float[] randomAngles = { 0, 90, 180, 270 };
        float randomStartAngle = randomAngles[Random.Range(0, randomAngles.Length)];
        transform.localEulerAngles = new Vector3(0, 0, randomStartAngle);
        
        // Обновляем визуальную обратную связь
        UpdateVisualFeedback();
        
        Debug.Log($"{name}: начальный угол = {randomStartAngle}, правильный = {correctRotation}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRotating && puzzleManager != null && !puzzleManager.IsGameWon)
        {
            StartCoroutine(Rotate90Degrees());
        }
    }

    private IEnumerator Rotate90Degrees()
    {
        isRotating = true;

        float startRotation = transform.localEulerAngles.z;
        float targetRotation = startRotation - 90f;
        
        float time = 0;

        while (time < rotationDuration)
        {
            time += Time.deltaTime;
            float z = Mathf.Lerp(startRotation, targetRotation, time / rotationDuration);
            transform.localEulerAngles = new Vector3(0, 0, z);
            yield return null;
        }

        // Фиксируем и нормализуем угол
        targetRotation = targetRotation % 360; 
        if (targetRotation < 0) targetRotation += 360;

        transform.localEulerAngles = new Vector3(0, 0, targetRotation);
        
        isRotating = false;
        
        // Обновляем визуальную обратную связь
        UpdateVisualFeedback();
        
        // Проверяем победу
        if (puzzleManager != null)
        {
            puzzleManager.CheckWinCondition();
        }
    }

    public bool IsCorrect()
    {
        float currentZ = transform.localEulerAngles.z;
        
        // Нормализуем углы к диапазону 0-360
        float normalizedCurrent = currentZ % 360;
        if (normalizedCurrent < 0) normalizedCurrent += 360;
        
        float normalizedCorrect = correctRotation % 360;
        if (normalizedCorrect < 0) normalizedCorrect += 360;
        
        // Сравниваем с допуском 1 градус
        return Mathf.Abs(normalizedCurrent - normalizedCorrect) < 1f;
    }

    public void UpdateVisualFeedback()
    {
        Image tileImage = GetComponent<Image>();
        if (tileImage == null) return;
        
        tileImage.color = IsCorrect() ? correctColor : normalColor;
    }
}