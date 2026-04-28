using UnityEngine;

public class JigsawPiece : MonoBehaviour
{
    [HideInInspector] public PuzzleManager manager;
    public Vector2 targetPosition;
    public float targetRotation = 0f;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 startMousePos;

    private const float dragThreshold = 5f;

    void Start()
    {
        // Кэшируем ссылку на менеджер при старте
        if (manager == null)
            manager = FindObjectOfType<PuzzleManager>();
        
        if (manager == null)
            Debug.LogError("PuzzleManager не найден в сцене!");
    }

    void OnMouseDown()
    {
        // Проверка на NullReferenceException
        if (manager == null || Camera.main == null)
            return;
            
        startMousePos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);
        isDragging = false;
        transform.position += Vector3.back * 0.1f;
    }

    void OnMouseDrag()
    {
        if (manager == null || Camera.main == null)
            return;
            
        if (Vector3.Distance(startMousePos, Input.mousePosition) > dragThreshold)
        {
            isDragging = true;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, -1f) + offset;
        }
    }

    void OnMouseUp()
    {
        if (manager == null)
            return;
            
        if (!isDragging)
        {
            transform.Rotate(0, 0, -90f);
        }
        else
        {
            Vector2 nearestPos = manager.GetNearestGridPosition(transform.position);
            float snapDistance = Mathf.Min(manager.pieceWidthUnits, manager.pieceHeightUnits) * 0.6f;

            if (Vector2.Distance(transform.position, nearestPos) < snapDistance &&
                !manager.IsPositionOccupied(nearestPos, this))
            {
                transform.position = new Vector3(nearestPos.x, nearestPos.y, -0.1f);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -0.1f);
            }
        }

        manager.CheckWinCondition();
    }

    public bool IsInCorrectPlace()
    {
        // Проверка
        return Vector2.Distance(transform.position, targetPosition) < 0.1f &&
               Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetRotation)) < 1f;
    }
}