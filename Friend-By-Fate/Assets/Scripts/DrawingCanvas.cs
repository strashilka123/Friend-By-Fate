using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Настройки холста")]
    public RawImage canvasImage;
    public int textureWidth = 1500;
    public int textureHeight = 1080;

    [Header("Настройки кисти")]
    public int brushSize = 8;
    public Color currentColor = Color.black;
    public TMP_Text sizeText;

    [Header("Панель в разработке")]
    public GameObject developmentPanel;  // Перетащите сюда панель "В разработке"
    public float panelDisplayTime = 2f; // Сколько секунд показывать панель

    private Texture2D _texture;
    private Vector2 _previousPosition;
    private bool _wasDrawing;
    private bool _needsApply;

    void Start()
    {
        _texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Bilinear;

        ClearCanvas();
        canvasImage.texture = _texture;

        if (sizeText != null)
            sizeText.text = "Размер: " + brushSize.ToString();

        // Скрываем панель при старте
        if (developmentPanel != null)
            developmentPanel.SetActive(false);
    }

    void Update()
    {
        if (_needsApply)
        {
            _texture.Apply();
            _needsApply = false;
        }
    }

    public void ClearCanvas()
    {
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        _texture.SetPixels(pixels);
        _texture.Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ProcessDrawing(eventData);
        _wasDrawing = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        ProcessDrawing(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _wasDrawing = false;
    }

    private void ProcessDrawing(PointerEventData eventData)
    {
        RectTransform rectTransform = canvasImage.rectTransform;
        Vector2 localPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPos))
        {
            float normalizedX = (localPos.x - rectTransform.rect.x) / rectTransform.rect.width;
            float normalizedY = (localPos.y - rectTransform.rect.y) / rectTransform.rect.height;

            int texX = Mathf.RoundToInt(normalizedX * textureWidth);
            int texY = Mathf.RoundToInt(normalizedY * textureHeight);

            if (_wasDrawing)
            {
                DrawLine(_previousPosition, new Vector2(texX, texY));
            }
            else
            {
                DrawCircle(texX, texY);
            }

            _previousPosition = new Vector2(texX, texY);
            _needsApply = true;
        }
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        float step = Mathf.Max(1f, brushSize / 4f);

        for (float i = 0; i <= distance; i += step)
        {
            Vector2 point = Vector2.Lerp(start, end, i / distance);
            DrawCircle(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
        }
    }

    private void DrawCircle(int x, int y)
    {
        int startX = Mathf.Max(x - brushSize, 0);
        int startY = Mathf.Max(y - brushSize, 0);
        int endX = Mathf.Min(x + brushSize, textureWidth);
        int endY = Mathf.Min(y + brushSize, textureHeight);

        float sqrRadius = brushSize * brushSize;

        for (int i = startX; i < endX; i++)
        {
            for (int j = startY; j < endY; j++)
            {
                float sqrDist = (i - x) * (i - x) + (j - y) * (j - y);
                if (sqrDist <= sqrRadius)
                {
                    _texture.SetPixel(i, j, currentColor);
                }
            }
        }
    }

    // --- Методы для UI ---
    public void SetColorFromButton(Image buttonImage)
    {
        currentColor = buttonImage.color;
    }

    public void SetEraser()
    {
        currentColor = Color.white;
    }

    public void SetBrushSize(float newSize)
    {
        brushSize = Mathf.RoundToInt(newSize);
        if (sizeText != null)
            sizeText.text = "Размер: " + brushSize.ToString();
    }

    public void SaveImage()
    {
        // Показываем панель "В разработке"
        if (developmentPanel != null)
        {
            developmentPanel.SetActive(true);
        }

        // Сохраняем изображение
        byte[] bytes = _texture.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, "MyDrawing.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log("Сохранено в: " + path);

        // Скрываем панель через указанное время
        if (developmentPanel != null)
        {
            StartCoroutine(HidePanelAfterDelay(panelDisplayTime));
        }
    }

    private System.Collections.IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (developmentPanel != null)
        {
            developmentPanel.SetActive(false);
        }
    }
}