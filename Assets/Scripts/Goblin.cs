using UnityEngine;
using UnityEngine.InputSystem;

public class Goblin : MonoBehaviour
{
    [Header("Pulo")]
    [SerializeField] int maxJumps = 2;

    [Header("Wall Slide")]
    [SerializeField] float wallSlideSpeed = 1.5f;

    [Header("Slingshot (Angry Birds)")]
    [SerializeField] float launchPower = 8f;
    [SerializeField] float maxDragDistance = 4f;

    [Header("Pontilhado")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int linePoints = 30;
    [SerializeField] float timeBetweenPoints = 0.05f;

    [Header("Componentes")]
    [SerializeField] Rigidbody2D rb;

    private PlayerControls inputActions;

    // Estados
    private bool touchingSurface;
    private bool isDragging;
    private bool isWallSliding;

    // Pulos restantes
    private int jumpsRemaining;

    // Drag
    private Vector2 dragStart;
    private Vector2 dragEnd;

    // Dire��o visual do sprite
    private int direction = 1;

    void Awake()
    {
        inputActions = new PlayerControls();
    }

    void OnEnable()
    {
        inputActions.Enable();

        inputActions.PlayerMaps.Jump.started += StartInput;
        inputActions.PlayerMaps.Jump.canceled += ReleaseInput;
    }

    void OnDisable()
    {
        inputActions.PlayerMaps.Jump.started -= StartInput;
        inputActions.PlayerMaps.Jump.canceled -= ReleaseInput;

        inputActions.Disable();
    }

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        jumpsRemaining = maxJumps;

        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Atualiza trajet�ria enquanto arrasta
        if (isDragging)
        {
            dragEnd = GetTouchWorldPosition();

            DrawTrajectory();
        }
    }

    void FixedUpdate()
    {
        // Wall slide apenas se:
        // - estiver encostando
        // - estiver caindo
        if (touchingSurface && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(
                    rb.linearVelocity.y,
                    -wallSlideSpeed
                )
            );
        }
        else
        {
            isWallSliding = false;
        }
    }

    // COME�OU O TOQUE
    void StartInput(InputAction.CallbackContext context)
    {
        // Estilingue dispon�vel em qualquer superf�cie
        if (touchingSurface)
        {
            isDragging = true;

            dragStart = GetTouchWorldPosition();

            lineRenderer.enabled = true;
        }
        else
        {
            // Double Jump no ar
            AirJump();
        }
    }

    // SOLTOU O TOQUE
    void ReleaseInput(InputAction.CallbackContext context)
    {
        if (!isDragging)
            return;

        isDragging = false;

        lineRenderer.enabled = false;

        Launch();
    }

    // DOUBLE JUMP
    void AirJump()
    {
        if (jumpsRemaining > 0)
        {
            jumpsRemaining--;

            // Preserve a velocidade horizontal � n�o zere o rb.velocity.
            // Apenas aplique o impulso para subir.
            rb.AddForce(
                Vector2.up * launchPower,
                ForceMode2D.Impulse
            );
        }
    }

    // ESTILINGUE
    void Launch()
    {
        touchingSurface = false;
        isWallSliding = false;

        // Dire��o do arrasto
        Vector2 dragDirection =
            dragStart - dragEnd;

        // Limite do pux�o
        dragDirection =
            Vector2.ClampMagnitude(
                dragDirection,
                maxDragDistance
            );

        // Velocidade estilo Angry Birds
        Vector2 launchVelocity =
            dragDirection * launchPower;

        rb.linearVelocity = launchVelocity;

        // Atualiza dire��o visual
        if (launchVelocity.x > 0.1f)
        {
            direction = 1;
        }
        else if (launchVelocity.x < -0.1f)
        {
            direction = -1;
        }

        UpdateSpriteScale();

        // Consome um pulo
        jumpsRemaining = maxJumps - 1;
    }

    // TRAJET�RIA
    void DrawTrajectory()
    {
        Vector2 dragDirection =
            dragStart - dragEnd;

        dragDirection =
            Vector2.ClampMagnitude(
                dragDirection,
                maxDragDistance
            );

        Vector2 initialVelocity =
            dragDirection * launchPower;

        Vector2 gravity =
            Physics2D.gravity * rb.gravityScale;

        lineRenderer.positionCount =
            linePoints;

        for (int i = 0; i < linePoints; i++)
        {
            float time =
                i * timeBetweenPoints;

            Vector2 point =
                (Vector2)transform.position +
                (initialVelocity * time) +
                (0.5f * gravity * time * time);

            lineRenderer.SetPosition(
                i,
                point
            );
        }
    }

    // POSI��O DO TOQUE
    Vector2 GetTouchWorldPosition()
    {
        Vector2 screenPosition =
            Vector2.zero;

        // Mobile
        if (Touchscreen.current != null)
        {
            screenPosition =
                Touchscreen.current
                .primaryTouch
                .position
                .ReadValue();
        }
        // PC
        else if (Mouse.current != null)
        {
            screenPosition =
                Mouse.current
                .position
                .ReadValue();
        }

        if (Camera.main != null)
        {
            Vector3 worldPoint =
                Camera.main.ScreenToWorldPoint(
                    new Vector3(
                        screenPosition.x,
                        screenPosition.y,
                        Camera.main.nearClipPlane
                    )
                );

            return worldPoint;
        }

        return Vector2.zero;
    }

    // DETECTA SUPERF�CIES
    private void OnCollisionEnter2D(Collision2D collision)
    {
        touchingSurface = true;

        jumpsRemaining = maxJumps;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        touchingSurface = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        touchingSurface = false;
    }

    // VIRA O SPRITE
    void UpdateSpriteScale()
    {
        Vector3 scale =
            transform.localScale;

        scale.x =
            Mathf.Abs(scale.x) * direction;

        transform.localScale = scale;
    }
}