using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Класс для хранения данных о карте
[System.Serializable]
public class Card
{
    public string suit;
    public string value;
    public int points;
    public string textureName;
}

public class CardGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text playerScoreText;
    public TMP_Text dealerScoreText;
    public TMP_Text roundResultText;
    public TMP_Text playerWinsText;
    public TMP_Text dealerWinsText;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform playerHand;
    public Transform dealerHand;

    [Header("Buttons")]
    public Button hitButton;
    public Button standButton;

    [Header("End Game Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public Button nextLevelButton;
    public Button backButton;

    [Header("Transition")]
    public string nextSceneName = "SketchBook";
    public string previousSceneName = "";

    // Игровые данные
    private List<Card> deck = new List<Card>();
    private List<Card> playerCards = new List<Card>();
    private List<Card> dealerCards = new List<Card>();

    // Счёт побед
    private int playerWinCount = 0;
    private int dealerWinCount = 0;
    private const int WIN_LIMIT = 5;

    // Флаг, что дилер уже взял вторую карту (для правил, где дилер берет вторую карту только после игрока)
    private bool dealerHasSecondCard = false;

    // Масти для генерации названий
    private string[] suits = { "hearts", "diamonds", "clubs", "spades" };
    private string[] values = { "02", "03", "04", "05", "06", "07", "08", "09", "10", "J", "Q", "K", "A" };
    private int[] points = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 11 };
    
    [Header("Сохранения")]
    public string cardGameId = "DefaultCardGame"; // Уникальный идентификатор карточной игры для сохранения
    private SaveManager saveManager;

    void Start()
    {
        // Получаем ссылку на SaveManager
        saveManager = SaveManager.Instance;
        
        if (hitButton != null) hitButton.onClick.AddListener(Hit);
        if (standButton != null) standButton.onClick.AddListener(Stand);

        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(GoToNextLevel);
        if (backButton != null) backButton.onClick.AddListener(GoBack);

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Проверяем, была ли уже завершена эта карточная игра
        if (saveManager.IsCardGameCompleted(cardGameId))
        {
            Debug.Log($"[CardGameManager] Карточная игра '{cardGameId}' уже была завершена ранее");
        }

        StartNewGame();
    }

    void StartNewGame()
    {
        // Сбрасываем флаг второй карты дилера
        dealerHasSecondCard = false;

        // Очищаем руки
        playerCards.Clear();
        dealerCards.Clear();

        // Очищаем стол от старых карт
        foreach (Transform child in playerHand) Destroy(child.gameObject);
        foreach (Transform child in dealerHand) Destroy(child.gameObject);

        // Создаём и перемешиваем колоду
        CreateDeck();
        ShuffleDeck();

        // Раздаём начальные карты (по 1 карте игроку и дилеру)
        DealCard(playerCards, playerHand);
        DealCard(dealerCards, dealerHand);

        // Обновляем UI
        UpdateUI();

        // Сбрасываем текст результата раунда
        if (roundResultText != null) roundResultText.text = "";

        // Активируем кнопки
        if (hitButton != null) hitButton.interactable = true;
        if (standButton != null) standButton.interactable = true;

        // Проверяем, не выиграл ли игрок сразу (21 очко с одной карты — это туз, но 21 бывает только с двумя картами, 
        // поэтому проверка остаётся на случай, если в будущем будут добавляться карты)
        if (CalculateScore(playerCards) == 21)
        {
            // Если у игрока 21 с одной карты (невозможно), но оставим для логики
            StartCoroutine(EndRoundWithDelay("player"));
        }
    }

    void CreateDeck()
    {
        deck.Clear();

        foreach (string suit in suits)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Card card = new Card();
                card.suit = suit;
                card.value = values[i];
                card.points = points[i];
                card.textureName = $"card_{suit}_{card.value}";
                deck.Add(card);
            }
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    void DealCard(List<Card> hand, Transform handTransform)
    {
        if (deck.Count == 0)
        {
            CreateDeck();
            ShuffleDeck();
        }

        Card card = deck[0];
        deck.RemoveAt(0);
        hand.Add(card);

        GameObject newCard = Instantiate(cardPrefab, handTransform);

        Image cardImage = newCard.GetComponent<Image>();
        if (cardImage != null)
        {
            Sprite cardSprite = LoadCardTexture(card.textureName);
            if (cardSprite != null)
            {
                cardImage.sprite = cardSprite;
            }
            else
            {
                Debug.LogWarning($"Текстура не найдена: {card.textureName}");
                ShowCardText(newCard, card);
            }
        }

        TMP_Text cardText = newCard.GetComponentInChildren<TMP_Text>();
        if (cardText != null && cardImage != null && cardImage.sprite != null)
        {
            cardText.enabled = false;
        }
        else if (cardText != null)
        {
            ShowCardText(newCard, card);
        }
    }

    Sprite LoadCardTexture(string textureName)
    {
        Sprite sprite = Resources.Load<Sprite>($"Textures/{textureName}");
        if (sprite == null)
        {
#if UNITY_EDITOR
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite s in allSprites)
            {
                if (s.name == textureName || s.name.Contains(textureName))
                {
                    return s;
                }
            }
#endif
        }
        return sprite;
    }

    void ShowCardText(GameObject card, Card cardData)
    {
        TMP_Text cardText = card.GetComponentInChildren<TMP_Text>();
        if (cardText != null)
        {
            string displayValue = cardData.value;
            cardText.text = displayValue;

            if (cardData.suit == "hearts" || cardData.suit == "diamonds")
            {
                cardText.color = Color.red;
            }
            else
            {
                cardText.color = Color.black;
            }
        }
    }

    int CalculateScore(List<Card> hand)
    {
        int score = 0;
        int aces = 0;

        foreach (Card card in hand)
        {
            if (card.value == "A") aces++;
            score += card.points;
        }

        while (score > 21 && aces > 0)
        {
            score -= 10;
            aces--;
        }

        return score;
    }

    void UpdateUI()
    {
        if (playerScoreText != null)
            playerScoreText.text = $"Ваши очки: {CalculateScore(playerCards)}";

        if (dealerScoreText != null)
            dealerScoreText.text = $"Очки Саши: {CalculateScore(dealerCards)}";

        if (playerWinsText != null)
            playerWinsText.text = $"Вы: {playerWinCount}";

        if (dealerWinsText != null)
            dealerWinsText.text = $"Саша: {dealerWinCount}";
    }

    public void Hit()
    {
        // Игрок берёт карту
        DealCard(playerCards, playerHand);
        UpdateUI();

        int playerScore = CalculateScore(playerCards);
        if (playerScore > 21)
        {
            StartCoroutine(EndRoundWithDelay("dealer"));
        }
        else if (playerScore == 21)
        {
            Stand();
        }
    }

    public void Stand()
    {
        if (hitButton != null) hitButton.interactable = false;
        if (standButton != null) standButton.interactable = false;

        StartCoroutine(DealerTurn());
    }

    IEnumerator DealerTurn()
    {
        // Даём дилеру вторую карту, если её ещё нет (по правилам блэкджека дилер открывает вторую карту после хода игрока)
        if (!dealerHasSecondCard)
        {
            yield return new WaitForSeconds(0.8f);
            DealCard(dealerCards, dealerHand);
            dealerHasSecondCard = true;
            UpdateUI();
            yield return new WaitForSeconds(0.8f);
        }

        // Дилер добирает карты до 17
        while (CalculateScore(dealerCards) < 17)
        {
            DealCard(dealerCards, dealerHand);
            UpdateUI();
            yield return new WaitForSeconds(0.8f);
        }

        int playerFinal = CalculateScore(playerCards);
        int dealerFinal = CalculateScore(dealerCards);

        if (dealerFinal > 21)
        {
            StartCoroutine(EndRoundWithDelay("player"));
        }
        else if (playerFinal > dealerFinal)
        {
            StartCoroutine(EndRoundWithDelay("player"));
        }
        else if (dealerFinal > playerFinal)
        {
            StartCoroutine(EndRoundWithDelay("dealer"));
        }
        else
        {
            StartCoroutine(EndRoundWithDelay("draw"));
        }
    }

    IEnumerator EndRoundWithDelay(string winner)
    {
        // Отключаем кнопки во время подведения итогов
        if (hitButton != null) hitButton.interactable = false;
        if (standButton != null) standButton.interactable = false;

        // Показываем результат раунда
        string roundMessage = "";
        if (winner == "player")
        {
            roundMessage = "Вы выиграли раунд! +1 очко";
            playerWinCount++;
        }
        else if (winner == "dealer")
        {
            roundMessage = "Саша выиграл раунд! +1 очко";
            dealerWinCount++;
        }
        else
        {
            roundMessage = "Ничья! Очки не начисляются";
        }

        if (roundResultText != null) roundResultText.text = roundMessage;
        UpdateUI();

        yield return new WaitForSeconds(2f);

        // Проверяем, достиг ли кто-то 5 побед
        if (playerWinCount >= WIN_LIMIT)
        {
            ShowWinGame();
        }
        else if (dealerWinCount >= WIN_LIMIT)
        {
            ShowLoseGame();
        }
        else
        {
            // Начинаем новый раунд
            StartNewGame();
        }
    }

    void ShowWinGame()
    {
        Debug.Log("ПОБЕДА В ИГРЕ! 5 побед!");

        if (hitButton != null) hitButton.interactable = false;
        if (standButton != null) standButton.interactable = false;

        // Сохраняем прогресс карточной игры
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveLastScene(currentSceneIndex);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    void ShowLoseGame()
    {
        Debug.Log("ПОРАЖЕНИЕ В ИГРЕ! 5 побед у Саши");

        if (hitButton != null) hitButton.interactable = false;
        if (standButton != null) standButton.interactable = false;

        // Сохраняем прогресс карточной игры
        if (saveManager != null)
        {
            saveManager.SaveCardGameProgress(cardGameId, playerWinCount, dealerWinCount, false);
            Debug.Log($"[CardGameManager] Прогресс карточной игры '{cardGameId}' сохранён");
        }

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
    }

    // Метод для кнопки "Далее" (после победы)
    public void GoToNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("Переход на следующую сцену (имя не указано)");
        }
    }

    // Метод для кнопки "Назад" (после поражения)
    public void GoBack()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            Debug.Log("Возврат на предыдущую сцену (имя не указано)");
        }
    }

    // ТЕСТОВЫЙ МЕТОД: нажмите P для автоматической победы в раунде (только для отладки)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("=== ТЕСТ: Принудительная победа игрока ===");

            // Отключаем кнопки
            if (hitButton != null) hitButton.interactable = false;
            if (standButton != null) standButton.interactable = false;

            // Завершаем раунд с победой игрока
            StartCoroutine(EndRoundWithDelay("player"));
        }
    }

}