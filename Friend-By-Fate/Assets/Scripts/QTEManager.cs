using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Эффекты")]
    public Image flashOverlay;
    public CameraShake cameraShakeScript;

    [Header("Ускорение")]
    public float minSpawnInterval = 0.5f;
    public float spawnAcceleration = 0.015f;
    public int maxQTEOnScreen = 8;

    private float spawnTimer;
    private bool gameOver = false;

    void Start()
    {
        if (canvasRect == null) { Debug.LogError("canvasRect не назначен!", this); return; }
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (flashOverlay != null) flashOverlay.color = Color.clear;

        UpdateUI();
        spawnTimer = spawnInterval + Random.Range(-0.1f, 0.2f);
    }

    void Update()
    {
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
        PlayFlash(Color.green);
        CheckGameState();
    }

    public void OnQTEFail()
    {
        if (gameOver) return;
        currentStance -= stanceLoss;
        PlayFlash(Color.red);
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
            resultText.text = isWin ? "УСПЕХ" : "ПРОВАЛ";
            resultText.color = isWin ? Color.green : Color.red;
        }
        Invoke(nameof(ProceedToStory), 2.5f);
    }

    void ProceedToStory() { Debug.Log("Мини-игра завершена."); }

    void PlayFlash(Color color)
    {
        if (flashOverlay == null) return;
        StartCoroutine(FlashCoroutine(color));
    }

    IEnumerator FlashCoroutine(Color flashColor)
    {
        flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.35f);
        yield return new WaitForSeconds(0.08f);
        flashOverlay.color = Color.clear;
    }
}