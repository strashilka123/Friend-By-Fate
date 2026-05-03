using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class QTEManager : MonoBehaviour
{
    [Header("Настройки стойкости")]
    public float currentStance = 50f;
    public float maxStance = 300f;
    public float stanceGain = 8f;
    public float stanceLoss = 12f;
    public float passiveRegenRate = 0.5f;

    [Header("Настройки спавна")]
    public GameObject qtePrefab;
    public RectTransform canvasRect;
    public float spawnInterval = 2.0f;
    public float reactionTime = 1.3f;

    [Header("UI Игры")]
    public Slider stanceSlider;

    [Header("UI Результата")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;

    // Кнопки управления
    public Button restartButton;
    public Button nextButton;

    [Header("Эффекты")]
    public CameraShake cameraShakeScript;

    [Header("Аудио")]
    public AudioManager audioManager;

    [Header("Ускорение")]
    public float minSpawnInterval = 0.5f;
    public float spawnAcceleration = 0.015f;
    public int maxQTEOnScreen = 8;

    // --- НАСТРОЙКИ СОХРАНЕНИЙ ---
    [Header("Сохранения")]
    public string qteId = "DefaultQTE"; // Задайте уникальный ID в Inspector (например, "StreetQTE")

    private float spawnTimer;
    private bool gameOver = false;
    private bool isGameOver = false;

    void Start()
    {
        if (canvasRect == null) { Debug.LogError("canvasRect не назначен!", this); return; }
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();

        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);

        UpdateUI();
        spawnTimer = spawnInterval + Random.Range(-0.1f, 0.2f);

        if (string.IsNullOrEmpty(qteId) || qteId == "DefaultQTE")
        {
            Debug.LogWarning($"[QTEManager] Не задан уникальный qteId для объекта {gameObject.name}. Сохранение может не работать корректно.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isGameOver)
        {
            ForceWinQTE();
        }

        if (gameOver) return;

        currentStance = Mathf.Min(currentStance + passiveRegenRate * Time.deltaTime, maxStance);
        UpdateUI();

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            if (GetActiveQTECount() < maxQTEOnScreen)
            {
                SpawnQTE();
                spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnAcceleration);
            }
            spawnTimer = spawnInterval + Random.Range(-0.1f, 0.2f);
        }
    }

    private void ForceWinQTE()
    {
        Debug.Log("Принудительная победа в QTE (тест)");
        isGameOver = true;
        gameOver = true;
        spawnTimer = 9999f;

        foreach (Transform child in canvasRect.transform)
        {
            if (child.GetComponent<QTEPrompt>() != null) Destroy(child.gameObject);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        else
        {
            var panel = GameObject.Find("GameOverPanel");
            if (panel != null) { gameOverPanel = panel; panel.SetActive(true); }
        }

        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(true);

        // Сохраняем победу
        SaveProgress(true);

        StartCoroutine(LoadNextSceneDelayed(2f));
    }

    private IEnumerator LoadNextSceneDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogError("Нет следующей сцены в Build Settings!");
        }
    }

    int GetActiveQTECount()
    {
        int count = 0;
        foreach (Transform child in canvasRect.transform)
            if (child.GetComponent<QTEPrompt>() != null) count++;
        return count;
    }

    void SpawnQTE()
    {
        GameObject qteObj = Instantiate(qtePrefab, canvasRect.transform);
        RectTransform rect = qteObj.GetComponent<RectTransform>();

        float xMax = (canvasRect.rect.width / 2) - 100f;
        float yMax = (canvasRect.rect.height / 2) - 100f;
        rect.anchoredPosition = new Vector2(Random.Range(-xMax, xMax), Random.Range(-yMax, yMax));

        QTEPrompt qte = qteObj.GetComponent<QTEPrompt>();
        if (qte != null)
        {
            QTEType randomType = (QTEType)Random.Range(0, 3);
            qte.SetType(randomType);
            qte.Initialize(this, reactionTime);
        }
        else
        {
            Destroy(qteObj);
        }
    }

    public void OnQTESuccess()
    {
        if (gameOver) return;
        currentStance += stanceGain;
        if (audioManager != null) audioManager.PlayQTESuccess();
        CheckGameState();
    }

    public void OnQTEFail()
    {
        if (gameOver) return;
        currentStance -= stanceLoss;
        if (audioManager != null) audioManager.PlayQTEFail();
        if (cameraShakeScript != null) cameraShakeScript.TriggerShake(0.3f, 0.2f);
        CheckGameState();
    }

    void CheckGameState()
    {
        currentStance = Mathf.Clamp(currentStance, 0, maxStance);
        UpdateUI();
        if (currentStance >= maxStance) EndGame(true);
        else if (currentStance <= 0) EndGame(false);
    }

    void UpdateUI() { if (stanceSlider != null) stanceSlider.value = currentStance / maxStance; }

    void EndGame(bool isWin)
    {
        if (gameOver) return;
        gameOver = true;

        foreach (Transform child in canvasRect.transform)
            if (child.GetComponent<QTEPrompt>() != null) Destroy(child.gameObject);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = isWin ? "УСПЕХ!" : "ПРОВАЛ!";
            resultText.color = isWin ? Color.green : Color.red;
            resultText.fontMaterial = new Material(resultText.fontMaterial);
            resultText.fontMaterial.SetColor("_UnderlayColor", isWin ? new Color32(0, 255, 60, 180) : new Color32(255, 40, 40, 180));
            resultText.fontMaterial.SetFloat("_OutlineWidth", 0.3f);
            resultText.fontMaterial.SetFloat("_UnderlaySoftness", 0.6f);
        }

        if (audioManager != null) audioManager.StopAmbience();

        if (restartButton != null) restartButton.gameObject.SetActive(!isWin);
        if (nextButton != null) nextButton.gameObject.SetActive(isWin);

        if (audioManager != null)
        {
            if (isWin) audioManager.PlayWinSound();
            else audioManager.PlayLoseSound();
        }

        // --- СОХРАНЕНИЕ ТОЛЬКО ПРИ ПОБЕДЕ ---
        if (isWin)
        {
            SaveProgress(true);
        }

        Invoke(nameof(ProceedToStory), 2.5f);
    }

    // Метод сохранения
    private void SaveProgress(bool isWin)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveQTEProgress(qteId, isWin);
            Debug.Log($"[SaveSystem] QTE '{qteId}' сохранен. Победа: {isWin}");
        }
        else
        {
            Debug.LogError("[SaveSystem] SaveManager не найден на сцене! Прогресс QTE не сохранен.");
        }
    }

    void ProceedToStory()
    {
        if (currentStance >= maxStance)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveLastScene(currentSceneIndex);
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("Это последняя сцена в сборке!");
        }
    }
}