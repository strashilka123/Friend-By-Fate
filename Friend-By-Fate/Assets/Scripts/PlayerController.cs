using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class playerController : MonoBehaviour
{
    public Joystick joystick;
    public bool IsPaused = false;
    public float PlayerSpeed;

    private float HorizontalVectoring;
    private float VerticalVectoring;
    private Rigidbody2D Rigidbody;
    public Animator MortAnim;
    public Transform PlayerBody;

    private bool FacingRight = true;

    void Start()
    {
        MortAnim = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (IsPaused)
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
        FacingRight = !FacingRight;
        Vector3 scale = PlayerBody.localScale;
        scale.x *= -1;
        PlayerBody.localScale = scale;
    }
}