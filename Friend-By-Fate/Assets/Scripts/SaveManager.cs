using System;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Менеджер системы сохранений прогресса игры.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public const string SaveKey = "FriendByFate_SaveData";
    private const string SAVE_KEY = "FriendByFate_SaveData";

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private GameSaveData _currentSave;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        _instance = this;

        LoadGame();
    }

    #region Public Methods

    public void SaveGame()
    {
        try
        {
            // Обновляем время последнего сохранения перед записью
            _currentSave.lastSaveTime = DateTime.Now.Ticks;

            string json = JsonUtility.ToJson(_currentSave, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] Игра успешно сохранена");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка сохранения: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                _currentSave = JsonUtility.FromJson<GameSaveData>(json);

                // Защита от null списков при загрузке
                if (_currentSave.puzzleProgress == null) _currentSave.puzzleProgress = new System.Collections.Generic.List<PuzzleSaveData>();
                if (_currentSave.qteProgress == null) _currentSave.qteProgress = new System.Collections.Generic.List<QTESaveData>();
                if (_currentSave.cardGameProgress == null) _currentSave.cardGameProgress = new System.Collections.Generic.List<CardGameSaveData>();
                if (_currentSave.drawingProgress == null) _currentSave.drawingProgress = new System.Collections.Generic.List<DrawingSaveData>();
                if (_currentSave.dialogueProgress == null) _currentSave.dialogueProgress = new System.Collections.Generic.List<DialogueSaveData>();

                Debug.Log("[SaveManager] Игра успешно загружена");
            }
            else
            {
                _currentSave = new GameSaveData();
                Debug.Log("[SaveManager] Созданы новые данные сохранения");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка загрузки: {e.Message}");
            _currentSave = new GameSaveData();
        }
    }

    public void DeleteSave()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            _currentSave = new GameSaveData();
            Debug.Log("[SaveManager] Данные сохранения удалены");
        }
    }

    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    #endregion

    #region Puzzle Progress

    public void SavePuzzleProgress(string puzzleId, int gridWidth, int gridHeight, bool isCompleted)
    {
        if (_currentSave.puzzleProgress == null)
            _currentSave.puzzleProgress = new System.Collections.Generic.List<PuzzleSaveData>();

        var existing = GetPuzzleProgress(puzzleId);
        if (existing != null)
        {
            existing.gridWidth = gridWidth;
            existing.gridHeight = gridHeight;
            existing.isCompleted = isCompleted;
            existing.lastPlayedTime = DateTime.Now.Ticks;
        }
        else
        {
            var newData = new PuzzleSaveData
            {
                puzzleId = puzzleId,
                gridWidth = gridWidth,
                gridHeight = gridHeight,
                isCompleted = isCompleted,
                lastPlayedTime = DateTime.Now.Ticks
            };
            _currentSave.puzzleProgress.Add(newData);
        }

        SaveGame();
    }

    public PuzzleSaveData GetPuzzleProgress(string puzzleId)
    {
        if (_currentSave.puzzleProgress == null)
            return null;

        foreach (var puzzle in _currentSave.puzzleProgress)
        {
            if (puzzle.puzzleId == puzzleId)
                return puzzle;
        }
        return null;
    }

    public bool IsPuzzleCompleted(string puzzleId)
    {
        var progress = GetPuzzleProgress(puzzleId);
        return progress != null && progress.isCompleted;
    }

    #endregion

    #region QTE Progress

    /// <summary>
    /// Сохранить прогресс QTE мини-игры.
    /// Исправлено: убираем счетчики кликов, сохраняем только факт победы.
    /// </summary>
    public void SaveQTEProgress(string qteId, bool isWin)
    {
        if (!isWin) return; // Сохраняем только если игрок победил

        if (_currentSave.qteProgress == null)
            _currentSave.qteProgress = new System.Collections.Generic.List<QTESaveData>();

        // Ищем существующую запись в текущих загруженных данных (_currentSave)
        QTESaveData qteData = _currentSave.qteProgress.FirstOrDefault(q => q.qteId == qteId);

        if (qteData == null)
        {
            qteData = new QTESaveData
            {
                qteId = qteId,
                isCompleted = false,
                bestStance = 0,
                successfulClicks = 0,
                totalClicks = 0
            };
            _currentSave.qteProgress.Add(qteData);
        }

        // Обновляем данные о победе
        qteData.isCompleted = true;
        qteData.bestStance = Mathf.Max(qteData.bestStance, 100f);
        qteData.lastPlayedTime = DateTime.Now.Ticks;

        Debug.Log($"[SaveSystem] QTE '{qteId}' сохранен как пройденный.");

        SaveGame();
    }

    public QTESaveData GetQTEProgress(string qteId)
    {
        if (_currentSave.qteProgress == null)
            return null;

        foreach (var qte in _currentSave.qteProgress)
        {
            if (qte.qteId == qteId)
                return qte;
        }
        return null;
    }

    public bool IsQTECompleted(string qteId)
    {
        var progress = GetQTEProgress(qteId);
        return progress != null && progress.isCompleted;
    }

    #endregion

    #region Card Game Progress

    public void SaveCardGameProgress(string gameId, int playerWins, int dealerWins, bool isCompleted)
    {
        if (_currentSave.cardGameProgress == null)
            _currentSave.cardGameProgress = new System.Collections.Generic.List<CardGameSaveData>();

        var existing = GetCardGameProgress(gameId);
        if (existing != null)
        {
            existing.playerWins = playerWins;
            existing.dealerWins = dealerWins;
            existing.isCompleted = isCompleted;
            existing.lastPlayedTime = DateTime.Now.Ticks;
        }
        else
        {
            var newData = new CardGameSaveData
            {
                gameId = gameId,
                playerWins = playerWins,
                dealerWins = dealerWins,
                isCompleted = isCompleted,
                lastPlayedTime = DateTime.Now.Ticks
            };
            _currentSave.cardGameProgress.Add(newData);
        }

        SaveGame();
    }

    public CardGameSaveData GetCardGameProgress(string gameId)
    {
        if (_currentSave.cardGameProgress == null)
            return null;

        foreach (var game in _currentSave.cardGameProgress)
        {
            if (game.gameId == gameId)
                return game;
        }
        return null;
    }

    public bool IsCardGameCompleted(string gameId)
    {
        var progress = GetCardGameProgress(gameId);
        return progress != null && progress.isCompleted;
    }

    #endregion

    #region Drawing Progress

    public void SaveDrawingProgress(string drawingId, bool isCompleted, int drawingsCount)
    {
        if (_currentSave.drawingProgress == null)
            _currentSave.drawingProgress = new System.Collections.Generic.List<DrawingSaveData>();

        var existing = GetDrawingProgress(drawingId);
        if (existing != null)
        {
            existing.isCompleted = isCompleted;
            existing.drawingsCount = drawingsCount;
            existing.lastPlayedTime = DateTime.Now.Ticks;
        }
        else
        {
            var newData = new DrawingSaveData
            {
                drawingId = drawingId,
                isCompleted = isCompleted,
                drawingsCount = drawingsCount,
                lastPlayedTime = DateTime.Now.Ticks
            };
            _currentSave.drawingProgress.Add(newData);
        }

        SaveGame();
    }

    public DrawingSaveData GetDrawingProgress(string drawingId)
    {
        if (_currentSave.drawingProgress == null)
            return null;

        foreach (var drawing in _currentSave.drawingProgress)
        {
            if (drawing.drawingId == drawingId)
                return drawing;
        }
        return null;
    }

    #endregion

    #region Dialogue Progress

    public void SaveDialogueProgress(string dialogueId, string lastNodeTag, System.Collections.Generic.List<string> choices)
    {
        if (_currentSave.dialogueProgress == null)
            _currentSave.dialogueProgress = new System.Collections.Generic.List<DialogueSaveData>();

        var existing = GetDialogueProgress(dialogueId);
        if (existing != null)
        {
            existing.lastNodeTag = lastNodeTag;
            existing.choicesMade = choices;
            existing.lastPlayedTime = DateTime.Now.Ticks;
        }
        else
        {
            var newData = new DialogueSaveData
            {
                dialogueId = dialogueId,
                lastNodeTag = lastNodeTag,
                choicesMade = choices,
                lastPlayedTime = DateTime.Now.Ticks
            };
            _currentSave.dialogueProgress.Add(newData);
        }

        SaveGame();
    }

    public DialogueSaveData GetDialogueProgress(string dialogueId)
    {
        if (_currentSave.dialogueProgress == null)
            return null;

        foreach (var dialogue in _currentSave.dialogueProgress)
        {
            if (dialogue.dialogueId == dialogueId)
                return dialogue;
        }
        return null;
    }

    #endregion

    #region General Stats

    public void SaveLastScene(int sceneIndex)
    {
        _currentSave.lastCompletedSceneIndex = sceneIndex;
        Debug.Log($"[SaveSystem] Прогресс сохранен: сцена {sceneIndex} пройдена.");
        SaveGame();
    }

    public void UpdateStats(int scenesVisited, float playTimeSeconds)
    {
        _currentSave.totalPlayTimeSeconds += playTimeSeconds;
        _currentSave.scenesVisited = Mathf.Max(_currentSave.scenesVisited, scenesVisited);
        _currentSave.lastSaveTime = DateTime.Now.Ticks;

        SaveGame();
    }

    public float GetTotalPlayTimeHours()
    {
        return _currentSave.totalPlayTimeSeconds / 3600f;
    }

    public static string GetLastSaveTime()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return "Нет сохранений";
        }

        string json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            return "Нет сохранений";
        }

        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            System.DateTime dateTime = System.DateTime.FromBinary(data.lastSaveTime);
            return dateTime.ToString("dd.MM.yyyy HH:mm");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при чтении времени сохранения: " + e.Message);
            return "Ошибка чтения";
        }
    }

    public int GetLastSceneIndex()
    {
        return _currentSave.lastCompletedSceneIndex;
    }

    #endregion

    #region Helper Methods

    public void ResetAllProgress()
    {
        _currentSave = new GameSaveData();
        SaveGame();
        Debug.Log("[SaveManager] Весь прогресс сброшен");
    }

    public void PrintSaveStats()
    {
        Debug.Log("СТАТИСТИКА СОХРАНЕНИЙ");
        Debug.Log($"Всего сыграно часов: {GetTotalPlayTimeHours():F2}");
        Debug.Log($"Последнее сохранение: {GetLastSaveTime()}");

        if (_currentSave.puzzleProgress != null)
            Debug.Log($"Завершено пазлов: {_currentSave.puzzleProgress.Count(p => p.isCompleted)}/{_currentSave.puzzleProgress.Count}");

        if (_currentSave.qteProgress != null)
            Debug.Log($"Завершено QTE: {_currentSave.qteProgress.Count(q => q.isCompleted)}/{_currentSave.qteProgress.Count}");

        if (_currentSave.cardGameProgress != null)
            Debug.Log($"Завершено карточных игр: {_currentSave.cardGameProgress.Count(c => c.isCompleted)}/{_currentSave.cardGameProgress.Count}");
    }

    #endregion
}

#region Save Data Classes

[Serializable]
public class GameSaveData
{
    public System.Collections.Generic.List<PuzzleSaveData> puzzleProgress;
    public System.Collections.Generic.List<QTESaveData> qteProgress;
    public System.Collections.Generic.List<CardGameSaveData> cardGameProgress;
    public System.Collections.Generic.List<DrawingSaveData> drawingProgress;
    public System.Collections.Generic.List<DialogueSaveData> dialogueProgress;

    public long lastSaveTime;
    public int scenesVisited;
    public float totalPlayTimeSeconds;
    public int lastCompletedSceneIndex;

    public GameSaveData()
    {
        puzzleProgress = new System.Collections.Generic.List<PuzzleSaveData>();
        qteProgress = new System.Collections.Generic.List<QTESaveData>();
        cardGameProgress = new System.Collections.Generic.List<CardGameSaveData>();
        drawingProgress = new System.Collections.Generic.List<DrawingSaveData>();
        dialogueProgress = new System.Collections.Generic.List<DialogueSaveData>();
        lastSaveTime = DateTime.Now.Ticks;
        scenesVisited = 0;
        totalPlayTimeSeconds = 0f;
        lastCompletedSceneIndex = 0;
    }
}

[Serializable]
public class PuzzleSaveData
{
    public string puzzleId;
    public int gridWidth;
    public int gridHeight;
    public bool isCompleted;
    public long lastPlayedTime;
}

[Serializable]
public class QTESaveData
{
    public string qteId;
    public float bestStance;
    public int successfulClicks;
    public int totalClicks;
    public bool isCompleted;
    public long lastPlayedTime;
}

[Serializable]
public class CardGameSaveData
{
    public string gameId;
    public int playerWins;
    public int dealerWins;
    public bool isCompleted;
    public long lastPlayedTime;
}

[Serializable]
public class DrawingSaveData
{
    public string drawingId;
    public bool isCompleted;
    public int drawingsCount;
    public long lastPlayedTime;
}

[Serializable]
public class DialogueSaveData
{
    public string dialogueId;
    public string lastNodeTag;
    public System.Collections.Generic.List<string> choicesMade;
    public long lastPlayedTime;
}

#endregion