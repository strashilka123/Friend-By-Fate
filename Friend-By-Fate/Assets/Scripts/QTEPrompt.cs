using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum QTEType { Block, Push, Dodge }

public class QTEPrompt : MonoBehaviour, IPointerDownHandler
{
    [Header("Визуал")]
    [SerializeField] private Image shrinkingCircle;
    [SerializeField] private Image iconImage;

    [Header("Ресурсы")]
    public Sprite iconBlock;
    public Sprite iconPush;
    public Sprite iconDodge;

    [Header("Цвета")]
    public Color colorBlock = new Color(0.2f, 0.5f, 1f);
    public Color colorPush = new Color(1f, 0.5f, 0.2f);
    public Color colorDodge = new Color(0.2f, 1f, 0.3f);

    private QTEManager manager;
    private float timeToPress;
    private float timer;
    private bool isResolved = false;

    public void SetType(QTEType type)
    {
        if (shrinkingCircle == null) shrinkingCircle = GetComponentInChildren<Image>();

        switch (type)
        {
            case QTEType.Block:
                shrinkingCircle.color = colorBlock;
                if (iconImage != null && iconBlock != null) iconImage.sprite = iconBlock;
                break;
            case QTEType.Push:
                shrinkingCircle.color = colorPush;
                if (iconImage != null && iconPush != null) iconImage.sprite = iconPush;
                break;
            case QTEType.Dodge:
                shrinkingCircle.color = colorDodge;
                if (iconImage != null && iconDodge != null) iconImage.sprite = iconDodge;
                break;
        }
    }

    public void Initialize(QTEManager qteManager, float time)
    {
        manager = qteManager;
        timeToPress = time;
        timer = time;
    }

    void Awake()
    {
        if (shrinkingCircle == null)
            shrinkingCircle = GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (isResolved || shrinkingCircle == null) return;

        timer -= Time.deltaTime;
        if (timeToPress > 0)
            shrinkingCircle.fillAmount = Mathf.Clamp01(timer / timeToPress);

        if (timer <= 0)
            ResolveQTE(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isResolved) return;
        ResolveQTE(true);
    }

    private void ResolveQTE(bool success)
    {
        isResolved = true;
        if (manager != null)
        {
            if (success) manager.OnQTESuccess();
            else manager.OnQTEFail();
        }
        Destroy(gameObject);
    }
}
