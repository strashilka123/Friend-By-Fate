using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManag : MonoBehaviour
{
    [Header("Настройки пазла")]
    public Texture2D sourceImage;
    public int gridWidth = 3;
    public int gridHeight = 3;
    public GameObject piecePrefab;

    [Header("Настройки поля")]
    public Rect scatterArea = new Rect(-10f, -4f, 5f, 8f);
    public Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [Header("Переходы")]

    [HideInInspector] public float pieceWidthUnits;
    [HideInInspector] public float pieceHeightUnits;
    [HideInInspector] public Vector2 boardBottomLeft;

    private List<JigsawPiece> allPieces = new List<JigsawPiece>();
    private float pixelsPerUnit;

    [Header("Масштабирование")]
    public float maxBoardUnits = 7f;
    private float totalWidth;
    private float totalHeight;

    [SerializeField] private GameObject devPanel; // панель победы

    void Start()
    {
        CalculateGridSizes();
        CreatePuzzleBoard();
        GeneratePuzzlePieces();
    }

    private void CalculateGridSizes()
    {
        float maxSidePx = Mathf.Max(sourceImage.width, sourceImage.height);
        pixelsPerUnit = maxSidePx / maxBoardUnits;

        pieceWidthUnits = (sourceImage.width / (float)gridWidth) / pixelsPerUnit;
        pieceHeightUnits = (sourceImage.height / (float)gridHeight) / pixelsPerUnit;

        totalWidth = pieceWidthUnits * gridWidth;
        totalHeight = pieceHeightUnits * gridHeight;

        boardBottomLeft = new Vector2(
            -totalWidth / 2f + pieceWidthUnits / 2f,
            -totalHeight / 2f + pieceHeightUnits / 2f
        );
    }

    private void CreatePuzzleBoard()
    {
        GameObject oldBoard = GameObject.Find("Puzzle_Board_Background");
        if (oldBoard != null) Destroy(oldBoard);

        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Quad);
        board.name = "Puzzle_Board_Background";

        board.transform.position = new Vector3(0, 0, 0.5f);

        board.transform.localScale = new Vector3(totalWidth, totalHeight, 1f);

        Material mat = board.GetComponent<Renderer>().material;
        mat.shader = Shader.Find("Sprites/Default");
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
                Sprite newSprite = Sprite.Create(sourceImage, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);

                GameObject pieceObj = Instantiate(piecePrefab);
                pieceObj.name = $"Piece_{x}_{y}";

                SpriteRenderer sr = pieceObj.GetComponent<SpriteRenderer>();
                if (sr == null) sr = pieceObj.AddComponent<SpriteRenderer>();
                sr.sprite = newSprite;

                JigsawPiece pieceScript = pieceObj.GetComponent<JigsawPiece>();
                if (pieceScript == null) pieceScript = pieceObj.AddComponent<JigsawPiece>();

                pieceScript.manager = this;

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

    public Vector2 GetNearestGridPosition(Vector2 currentPos)
    {
        int x = Mathf.RoundToInt((currentPos.x - boardBottomLeft.x) / pieceWidthUnits);
        int y = Mathf.RoundToInt((currentPos.y - boardBottomLeft.y) / pieceHeightUnits);

        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        return new Vector2(boardBottomLeft.x + x * pieceWidthUnits, boardBottomLeft.y + y * pieceHeightUnits);
    }

    public bool IsPositionOccupied(Vector2 pos, JigsawPiece requester)
    {
        foreach (var piece in allPieces)
        {
            if (piece == requester) continue;

            if (Vector2.Distance(piece.transform.position, pos) < 0.1f)
            {
                return true; 
            }
        }
        return false; 
    }

    public void CheckWinCondition()
    {
        if (allPieces.Count == 0) return;

        foreach (var piece in allPieces)
        {
            if (!piece.IsInCorrectPlace()) return;
        }

        Debug.Log("ПОБЕДА! Картинка собрана.");
        CompleteMiniGame();
    }

    private IEnumerator ShowVictoryPanelDelayed()
    {
        // Ждем 2 секунды
        yield return new WaitForSeconds(1.5f);

        // Показываем панель победы
        if (devPanel != null)
        {
            devPanel.SetActive(true);
        }
    }

    public void CompleteMiniGame()
    {
        Debug.Log("Мини-игра завершена!");

        foreach (var piece in allPieces)
        {
            if (piece != null)
            {
                Collider2D col = piece.GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false; // отключаем коллайдер
                }
            }
        }

        // Запускаем корутину с задержкой перед показом панели
        StartCoroutine(ShowVictoryPanelDelayed());

        // Очищаем список сразу, чтобы нельзя было двигать кусочки
        allPieces.Clear();
    }

    // Метод для загрузки сцены с картами
    public void LoadCardGameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ТЕСТОВЫЙ МЕТОД: нажмите P для принудительной победы 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Принудительная победа (тест)");
            CompleteMiniGame();
        }
    }
}