using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Joystick joystick;
    
    [Header("Settings")]
    public bool IsPaused = false;
    public float PlayerSpeed;

    private float HorizontalVectoring;
    private float VerticalVectoring;
    private Rigidbody2D Rigidbody;
    private Animator MortAnim;
    public Transform PlayerBody;

    private bool FacingRight = true;

    void Start()
    {
        // Кэшируем ссылки на компоненты
        if (MortAnim == null)
            MortAnim = GetComponent<Animator>();
        
        if (Rigidbody == null)
            Rigidbody = GetComponent<Rigidbody2D>();
        
        // Проверка на наличие обязательных компонентов
        if (joystick == null)
            Debug.LogWarning("Joystick не назначен в PlayerController!");
        
        if (PlayerBody == null)
            Debug.LogWarning("PlayerBody не назначен в PlayerController!");
    }

    private void FixedUpdate()
    {
        if (IsPaused)
            return;

        // Проверка на NullReferenceException
        if (joystick == null || Rigidbody == null || MortAnim == null || PlayerBody == null)
            return;

        HorizontalVectoring = joystick.Horizontal;
        VerticalVectoring = joystick.Vertical;

        Vector2 movement = new Vector2(HorizontalVectoring * PlayerSpeed, VerticalVectoring * PlayerSpeed);
        Rigidbody.linearVelocity = new Vector2(movement.x, movement.y);

        // Проверка на движение
        bool isMoving = Mathf.Abs(HorizontalVectoring) > 0.1f || Mathf.Abs(VerticalVectoring) > 0.1f;

        // Устанавливаем параметр IsRunning в Animator
        MortAnim.SetBool("IsRunning", isMoving);

        if (isMoving)
        {
            if (FacingRight && HorizontalVectoring < 0)
            {
                Flip();
            }
            else if (!FacingRight && HorizontalVectoring > 0)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        if (PlayerBody == null)
            return;
            
        FacingRight = !FacingRight;
        Vector3 scale = PlayerBody.localScale;
        scale.x *= -1;
        PlayerBody.localScale = scale;
    }
}