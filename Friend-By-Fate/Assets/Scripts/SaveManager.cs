using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Сохранение прогресса только для мини-игр.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static event Action ProgressReset;

    private static SaveManager _instance;
    public const string SaveKey = "FriendByFate_SaveData";

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

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGame();
        Debug.Log("[SaveManager] Awake complete. Save loaded.");
    }

    public void SaveGame()
    {
        _currentSave.lastSaveTime = DateTime.Now.Ticks;
        string json = JsonUtility.ToJson(_currentSave, true);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] SaveGame done. Last scene: {_currentSave.lastCompletedSceneIndex}");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            _currentSave = new GameSaveData();
            Debug.Log("[SaveManager] No save found. Created new save data.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        _currentSave = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
        Debug.Log("[SaveManager] Save data loaded from PlayerPrefs.");

        _currentSave.puzzleProgress ??= new List<PuzzleSaveData>();
        _currentSave.qteProgress ??= new List<QTESaveData>();
        _currentSave.cardGameProgress ??= new List<CardGameSaveData>();
        _currentSave.drawingProgress ??= new List<DrawingSaveData>();
    }

    public bool HasSave() => PlayerPrefs.HasKey(SaveKey);

    public void DeleteSave()
    {
        ResetAllProgress();
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        _currentSave = new GameSaveData();
        Debug.Log("[SaveManager] Full progress reset executed.");
        ProgressReset?.Invoke();
    }

    public void SaveLastScene(int sceneIndex)
    {
        _currentSave.lastCompletedSceneIndex = sceneIndex;
        SaveGame();
        Debug.Log($"[SaveManager] Last scene saved: {sceneIndex}");
    }

    public int GetLastSceneIndex() => _currentSave.lastCompletedSceneIndex;

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
            DateTime dateTime = DateTime.FromBinary(data.lastSaveTime);
            return dateTime.ToString("dd.MM.yyyy HH:mm");
        }
        catch
        {
            return "Ошибка чтения";
        }
    }

    public void SavePuzzleProgress(string puzzleId, int gridWidth, int gridHeight, bool isCompleted)
    {
        PuzzleSaveData existing = _currentSave.puzzleProgress.FirstOrDefault(p => p.puzzleId == puzzleId);
        if (existing == null)
        {
            existing = new PuzzleSaveData { puzzleId = puzzleId };
            _currentSave.puzzleProgress.Add(existing);
        }

        existing.gridWidth = gridWidth;
        existing.gridHeight = gridHeight;
        existing.isCompleted = isCompleted;
        existing.lastPlayedTime = DateTime.Now.Ticks;

        SaveGame();
        Debug.Log($"[SaveManager] Puzzle progress saved: {puzzleId}, completed={isCompleted}");
    }

    public PuzzleSaveData GetPuzzleProgress(string puzzleId) =>
        _currentSave.puzzleProgress.FirstOrDefault(p => p.puzzleId == puzzleId);

    public bool IsPuzzleCompleted(string puzzleId) => GetPuzzleProgress(puzzleId)?.isCompleted == true;

    public void SaveQTEProgress(string qteId, bool isWin)
    {
        if (!isWin)
        {
            return;
        }

        QTESaveData existing = _currentSave.qteProgress.FirstOrDefault(q => q.qteId == qteId);
        if (existing == null)
        {
            existing = new QTESaveData { qteId = qteId };
            _currentSave.qteProgress.Add(existing);
        }

        existing.isCompleted = true;
        existing.lastPlayedTime = DateTime.Now.Ticks;

        SaveGame();
        Debug.Log($"[SaveManager] QTE saved as completed: {qteId}");
    }

    public QTESaveData GetQTEProgress(string qteId) =>
        _currentSave.qteProgress.FirstOrDefault(q => q.qteId == qteId);

    public bool IsQTECompleted(string qteId) => GetQTEProgress(qteId)?.isCompleted == true;

    public void SaveCardGameProgress(string gameId, int playerWins, int dealerWins, bool isCompleted)
    {
        CardGameSaveData existing = _currentSave.cardGameProgress.FirstOrDefault(c => c.gameId == gameId);
        if (existing == null)
        {
            existing = new CardGameSaveData { gameId = gameId };
            _currentSave.cardGameProgress.Add(existing);
        }

        existing.playerWins = playerWins;
        existing.dealerWins = dealerWins;
        existing.isCompleted = isCompleted;
        existing.lastPlayedTime = DateTime.Now.Ticks;

        SaveGame();
        Debug.Log($"[SaveManager] Card game progress saved: {gameId}, completed={isCompleted}");
    }

    public CardGameSaveData GetCardGameProgress(string gameId) =>
        _currentSave.cardGameProgress.FirstOrDefault(c => c.gameId == gameId);

    public bool IsCardGameCompleted(string gameId) => GetCardGameProgress(gameId)?.isCompleted == true;

    public void SaveDrawingProgress(string drawingId, bool isCompleted, int drawingsCount)
    {
        DrawingSaveData existing = _currentSave.drawingProgress.FirstOrDefault(d => d.drawingId == drawingId);
        if (existing == null)
        {
            existing = new DrawingSaveData { drawingId = drawingId };
            _currentSave.drawingProgress.Add(existing);
        }

        existing.isCompleted = isCompleted;
        existing.drawingsCount = drawingsCount;
        existing.lastPlayedTime = DateTime.Now.Ticks;

        SaveGame();
        Debug.Log($"[SaveManager] Drawing progress saved: {drawingId}, completed={isCompleted}, count={drawingsCount}");
    }

    public DrawingSaveData GetDrawingProgress(string drawingId) =>
        _currentSave.drawingProgress.FirstOrDefault(d => d.drawingId == drawingId);
}

[Serializable]
public class GameSaveData
{
    public List<PuzzleSaveData> puzzleProgress = new();
    public List<QTESaveData> qteProgress = new();
    public List<CardGameSaveData> cardGameProgress = new();
    public List<DrawingSaveData> drawingProgress = new();
    public long lastSaveTime = DateTime.Now.Ticks;
    public int lastCompletedSceneIndex;
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
