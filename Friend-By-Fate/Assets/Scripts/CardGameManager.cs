using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text playerScoreText;
    public TMP_Text dealerScoreText;
    public TMP_Text resultText;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform playerHand;
    public Transform dealerHand;

    [Header("Buttons")]
    public Button hitButton;
    public Button standButton;
    public Button restartButton;

    // Игровые данные
    private List<Card> deck = new List<Card>();
    private List<Card> playerCards = new List<Card>();
    private List<Card> dealerCards = new List<Card>();

    // Масти для генерации названий
    private string[] suits = { "hearts", "diamonds", "clubs", "spades" };
    private string[] values = { "02", "03", "04", "05", "06", "07", "08", "09", "10", "J", "Q", "K", "A" };
    private int[] points = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 11 };

    void Start()
    {
        // Назначаем действия на кнопки
        if (hitButton != null) hitButton.onClick.AddListener(Hit);
        if (standButton != null) standButton.onClick.AddListener(Stand);
        if (restartButton != null) restartButton.onClick.AddListener(StartNewGame);

        StartNewGame();
    }

    void StartNewGame()
    {
        // Очищаем руки
        playerCards.Clear();
        dealerCards.Clear();

        // Очищаем стол от старых карт
        foreach (Transform child in playerHand) Destroy(child.gameObject);
        foreach (Transform child in dealerHand) Destroy(child.gameObject);

        // Создаём и перемешиваем колоду
        CreateDeck();
        ShuffleDeck();

        // Раздаём начальные карты (по 2 карты игроку и дилеру)
        DealCard(playerCards, playerHand);
        DealCard(playerCards, playerHand);
        DealCard(dealerCards, dealerHand);
        DealCard(dealerCards, dealerHand);

        // Обновляем UI
        UpdateUI();

        // Сбрасываем текст результата
        if (resultText != null) resultText.text = "";

        // Активируем кнопки
        if (hitButton != null) hitButton.interactable = true;
        if (standButton != null) standButton.interactable = true;

        // Проверяем, не выиграл ли игрок сразу (21 очко)
        if (CalculateScore(playerCards) == 21)
        {
            EndGame("Блэкджек! Вы выиграли!");
        }
    }

    void CreateDeck()
    {
        deck.Clear();

        // Создаём 52 карты (4 масти × 13 значений)
        foreach (string suit in suits)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Card card = new Card();
                card.suit = suit;
                card.value = values[i];
                card.points = points[i];

                // Формируем имя текстуры: card_hearts_2, card_spades_A и т.д.
                card.textureName = $"card_{suit}_{card.value}";

                deck.Add(card);
            }
        }
    }

    void ShuffleDeck()
    {
        // Перемешивание колоды
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
            // Если колода пуста, пересоздаём
            CreateDeck();
            ShuffleDeck();
        }

        Card card = deck[0];
        deck.RemoveAt(0);
        hand.Add(card);

        // Создаём визуальную карту
        GameObject newCard = Instantiate(cardPrefab, handTransform);

        // Настраиваем изображение карты
        Image cardImage = newCard.GetComponent<Image>();
        if (cardImage != null)
        {
            // Загружаем текстуру из папки Textures
            Sprite cardSprite = LoadCardTexture(card.textureName);
            if (cardSprite != null)
            {
                cardImage.sprite = cardSprite;
            }
            else
            {
                // Если текстура не найдена, показываем текст
                Debug.LogWarning($"Текстура не найдена: {card.textureName}");
                ShowCardText(newCard, card);
            }
        }

        // Отключаем текст, если используем изображения (или оставляем как запасной вариант)
        TMP_Text cardText = newCard.GetComponentInChildren<TMP_Text>();
        if (cardText != null && cardImage != null && cardImage.sprite != null)
        {
            cardText.enabled = false; // Скрываем текст, если есть спрайт
        }
        else if (cardText != null)
        {
            ShowCardText(newCard, card);
        }
    }

    Sprite LoadCardTexture(string textureName)
    {
        // Пробуем загрузить текстуру из папки Textures
        Sprite sprite = Resources.Load<Sprite>($"Textures/{textureName}");

        // Если не нашли в Resources, ищем в Assets через AssetDatabase (только в редакторе)
        if (sprite == null)
        {
#if UNITY_EDITOR
            // Ищем все спрайты в проекте
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

            // Цвет в зависимости от масти
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

        // Если перебор и есть тузы, меняем их с 11 на 1
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
            playerScoreText.text = $"Очки игрока: {CalculateScore(playerCards)}";

        if (dealerScoreText != null)
            dealerScoreText.text = $"Очки дилера: {CalculateScore(dealerCards)}";
    }

    public void Hit()
    {
        // Игрок берёт карту
        DealCard(playerCards, playerHand);
        UpdateUI();

        int playerScore = CalculateScore(playerCards);
        if (playerScore > 21)
        {
            EndGame("Перебор! Вы проиграли");
        }
        else if (playerScore == 21)
        {
            Stand();
        }
    }

    public void Stand()
    {
        // Отключаем кнопки во время хода дилера
        if (hitButton != null) hitButton.interactable = false;
        if (standButton != null) standButton.interactable = false;

        StartCoroutine(DealerTurn());
    }

    IEnumerator DealerTurn()
    {
        yield return new WaitForSeconds(1f);

        // Дилер берёт карты, пока у него меньше 17 очков
        while (CalculateScore(dealerCards) < 17)
        {
            DealCard(dealerCards, dealerHand);
            UpdateUI();
            yield return new WaitForSeconds(0.8f);
        }

        // Определяем победителя
        int playerFinal = CalculateScore(playerCards);
        int dealerFinal = CalculateScore(dealerCards);

        if (dealerFinal > 21)
        {
            EndGame("Дилер перебрал! Вы выиграли!");
        }
        else if (playerFinal > dealerFinal)
        {
            EndGame("Вы выиграли!");
        }
        else if (dealerFinal > playerFinal)
        {
            EndGame("Дилер выиграл");
        }
        else
        {
            EndGame("Ничья");
        }
    }

    void EndGame(string message)
    {
        // Отключаем кнопки после окончания игры
        if (hitButton != null) hitButton.interactable = false;
        if (standButton != null) standButton.interactable = false;

        if (resultText != null)
        {
            resultText.text = message;
        }

        Debug.Log(message);
    }
}

// Класс для хранения данных о карте
[System.Serializable]
public class Card
{
    public string suit;        // Масть: hearts, diamonds, clubs, spades
    public string value;       // Значение: 2-10, J, Q, K, A
    public int points;         // Очки: 2-10, 10, 10, 10, 11
    public string textureName; // Имя текстуры: card_hearts_2 и т.д.
}