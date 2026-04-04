using UnityEngine;

public class JigsawPiece : MonoBehaviour
{
    [HideInInspector] public PuzzleManag manager;
    public Vector2 targetPosition;
    public float targetRotation = 0f;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 startMousePos;

    private const float dragThreshold = 5f;

    void OnMouseDown()
    {
        startMousePos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);
        isDragging = false;
    }

    void OnMouseDrag()
    {
        if (Vector3.Distance(startMousePos, Input.mousePosition) > dragThreshold)
        {
            isDragging = true;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Во время перетаскивания "поднимаем" деталь поверх остальных (Z = -1)
            transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, -1f) + offset;
        }
    }

    void OnMouseUp()
    {
        if (!isDragging)
        {
            // Обычный клик - поворот
            transform.Rotate(0, 0, -90f);
        }
        else
        {
            // Отпускаем после перетаскивания - ищем ячейку для примагничивания
            Vector2 nearestPos = manager.GetNearestGridPosition(transform.position);

            // Дистанция срабатывания магнита (чуть больше половины детали)
            float snapDistance = Mathf.Min(manager.pieceWidthUnits, manager.pieceHeightUnits) * 0.6f;

            if (Vector2.Distance(transform.position, nearestPos) < snapDistance)
            {
                // Примагничиваем точно в центр ячейки
                transform.position = new Vector3(nearestPos.x, nearestPos.y, -0.1f);
            }
            else
            {
                // Если бросили далеко от сетки, просто "кладем" деталь обратно
                transform.position = new Vector3(transform.position.x, transform.position.y, -0.1f);
            }
        }

        // При любом действии (перетаскивании или повороте) проверяем, не собралась ли картинка
        manager.CheckWinCondition();
    }

    public bool IsInCorrectPlace()
    {
        // Проверка: позиция совпадает (с погрешностью 0.1) И угол правильный
        return Vector2.Distance(transform.position, targetPosition) < 0.1f &&
               Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetRotation)) < 1f;
    }
}