using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManag : MonoBehaviour
{
    [Header("Настройки пазла")]
    public Texture2D sourceImage;
    public int gridWidth = 4;
    public int gridHeight = 3;
    public GameObject piecePrefab;

    [Header("Настройки поля")]
    public Rect scatterArea = new Rect(-10f, -4f, 5f, 8f); 
    public Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [HideInInspector] public float pieceWidthUnits;
    [HideInInspector] public float pieceHeightUnits;
    [HideInInspector] public Vector2 boardBottomLeft;

    private List<JigsawPiece> allPieces = new List<JigsawPiece>();
    private const float PIXELS_PER_UNIT = 100f;

    // [SerializeField] private string parkSceneName = "ParkScene";
    [SerializeField] private GameObject devPanel;

    void Start()
    {
        CalculateGridSizes();
        CreatePuzzleBoard();
        GeneratePuzzlePieces();
    }

    private void CalculateGridSizes()
    {
        pieceWidthUnits = (sourceImage.width / (float)gridWidth) / PIXELS_PER_UNIT;
        pieceHeightUnits = (sourceImage.height / (float)gridHeight) / PIXELS_PER_UNIT;

        float boardWidth = pieceWidthUnits * gridWidth;
        float boardHeight = pieceHeightUnits * gridHeight;

        boardBottomLeft = new Vector2(-boardWidth / 2f + pieceWidthUnits / 2f, -boardHeight / 2f + pieceHeightUnits / 2f);
    }

    private void CreatePuzzleBoard()
    {
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Quad);
        board.name = "Puzzle_Board_Background";
        board.transform.position = new Vector3(0, 0, 0.5f); // Фон чуть позади деталей

        // Масштабируем доску точно под размер всех спрайтов
        board.transform.localScale = new Vector3(pieceWidthUnits * gridWidth, pieceHeightUnits * gridHeight, 1f);

        Material mat = board.GetComponent<Renderer>().material;
        mat.shader = Shader.Find("Sprites/Default"); // Применяем шейдер для корректного отображения цвета
        mat.color = boardColor;

        Destroy(board.GetComponent<Collider>());
    }

    public void GeneratePuzzlePieces()
    {
        float unitWidthPx = sourceImage.width / (float)gridWidth;
        float unitHeightPx = sourceImage.height / (float)gridHeight;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Rect rect = new Rect(x * unitWidthPx, y * unitHeightPx, unitWidthPx, unitHeightPx);
                Sprite newSprite = Sprite.Create(sourceImage, rect, new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);

                GameObject pieceObj = Instantiate(piecePrefab);
                pieceObj.name = $"Piece_{x}_{y}";

                SpriteRenderer sr = pieceObj.GetComponent<SpriteRenderer>();
                if (sr == null) sr = pieceObj.AddComponent<SpriteRenderer>();
                sr.sprite = newSprite;

                JigsawPiece pieceScript = pieceObj.GetComponent<JigsawPiece>();
                if (pieceScript == null) pieceScript = pieceObj.AddComponent<JigsawPiece>();

                pieceScript.manager = this; // Передаем детали ссылку на менеджер

                // Задаем правильную целевую позицию
                Vector2 targetPos = new Vector2(boardBottomLeft.x + x * pieceWidthUnits, boardBottomLeft.y + y * pieceHeightUnits);
                pieceScript.targetPosition = targetPos;
                pieceScript.targetRotation = 0f;

                ShuffleSinglePiece(pieceObj);

                if (pieceObj.GetComponent<Collider2D>() == null)
                    pieceObj.AddComponent<BoxCollider2D>();

                allPieces.Add(pieceScript);
            }
        }
    }

    private void ShuffleSinglePiece(GameObject piece)
    {
        int randomRot = Random.Range(0, 4) * 90;
        piece.transform.rotation = Quaternion.Euler(0, 0, randomRot);

        float randomX = Random.Range(scatterArea.xMin, scatterArea.xMax);
        float randomY = Random.Range(scatterArea.yMin, scatterArea.yMax);
        piece.transform.position = new Vector3(randomX, randomY, -0.1f);
    }

    // Метод возвращает координаты ближайшей ячейки на доске
    public Vector2 GetNearestGridPosition(Vector2 currentPos)
    {
        int x = Mathf.RoundToInt((currentPos.x - boardBottomLeft.x) / pieceWidthUnits);
        int y = Mathf.RoundToInt((currentPos.y - boardBottomLeft.y) / pieceHeightUnits);

        // Не даем магнититься "в пустоту" за пределами поля
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        return new Vector2(boardBottomLeft.x + x * pieceWidthUnits, boardBottomLeft.y + y * pieceHeightUnits);
    }

    // Вызывается самими кусочками при любом их изменении
    public void CheckWinCondition()
    {
        if (allPieces.Count == 0) return;

        foreach (var piece in allPieces)
        {
            if (!piece.IsInCorrectPlace()) return; // Если хоть одна не на месте - прерываем проверку
        }

        Debug.Log("ПОБЕДА! Картинка собрана.");
        CompleteMiniGame();
        // Сюда можно добавить логику перехода на следующую сцену

    }

    public void CompleteMiniGame()
    {
        Debug.Log("Мини-игра завершена!");

        if (devPanel != null)
        {
            devPanel.SetActive(true);
            allPieces.Clear();
            // SceneManager.LoadScene(parkSceneName); 
        }

    }
}