using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundPanel;
    public Text titleText;
    public RotatableTile[] tiles;
    public GameObject winPanel;
    
    [Header("Game State")]
    public bool IsGameWon { get; private set; } = false;
    
    private void Start()
    {
        Debug.Log("PuzzleManager Started");
        
        IsGameWon = false;
        
        // Скрываем панель победы
        if (winPanel != null)
        {
            winPanel.SetActive(false);
            Debug.Log("Панель победы скрыта при старте");
        }
        else
        {
            Debug.LogError("WinPanel не назначена в инспекторе!");
        }
        
        // Проверяем ссылки на плитки
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("Плитки не назначены в PuzzleManager!");
        }
        else
        {
            Debug.Log($"PuzzleManager имеет {tiles.Length} плиток");
        }
    }
    
    public void CheckWinCondition()
    {
        if (IsGameWon) 
        {
            Debug.Log("Уже победили!");
            return;
        }
        
        Debug.Log("Проверка условий победы...");
        
        bool allCorrect = true;
        
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                Debug.LogError($"Плитка {i} равна null!");
                allCorrect = false;
                break;
            }
            
            bool tileCorrect = tiles[i].IsCorrect();
            Debug.Log($"Плитка {i} ({tiles[i].name}): угол={tiles[i].transform.eulerAngles.z}, правильный={tiles[i].correctRotation}, корректна={tileCorrect}");
            
            if (!tileCorrect)
            {
                allCorrect = false;
                break;
            }
        }
        
        Debug.Log($"Все плитки корректны: {allCorrect}");
        
        if (allCorrect)
        {
            IsGameWon = true;
            ShowWinPanel();
            Debug.Log("ПОБЕДА! Все плитки правильно повернуты!");
        }
    }
    
    private void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            // Убедимся, что панель победы поверх всех элементов
            Canvas winCanvas = winPanel.GetComponent<Canvas>();
            if (winCanvas == null)
            {
                winCanvas = winPanel.AddComponent<Canvas>();
            }
            winCanvas.overrideSorting = true;
            winCanvas.sortingOrder = 100;
            
            Debug.Log("Панель победы показана!");
            
            // Анимация плавного появления
            StartCoroutine(FadeInWinPanel());
        }
        else
        {
            Debug.LogError("WinPanel равна null в ShowWinPanel!");
        }
    }
    
    private IEnumerator FadeInWinPanel()
    {
        CanvasGroup canvasGroup = winPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = winPanel.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0;
        
        float duration = 0.8f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1;
    }
    
    // Метод для отладки (не используется)
    public void DebugTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null)
            {
                Debug.Log($"Плитка {i}: {tiles[i].name}, угол: {tiles[i].transform.eulerAngles.z}, правильный: {tiles[i].correctRotation}, корректна: {tiles[i].IsCorrect()}");
            }
        }
    }
}