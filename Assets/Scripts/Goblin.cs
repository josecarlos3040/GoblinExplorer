using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Goblin : MonoBehaviour
{
    public static Goblin instance;

    public bool gameStarted = false;

    [Header("Pulo")]
    [SerializeField] float launchPower = 8f;
    [SerializeField] int jumpsRemaining;

    [Header("Estilingue")]
    [SerializeField] float maxDragDistance = 4f;

    [Header("Wall Slide")]
    [SerializeField] float wallSlideSpeed = 1.5f;

    [Header("Trajetória")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int linePoints = 30;
    [SerializeField] float timeBetweenPoints = 0.05f;

    [Header("Componentes")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D bc;
    [SerializeField] TextMeshProUGUI startText;
    [SerializeField] TextMeshProUGUI gameoverText;
    [SerializeField] TextMeshProUGUI continueText;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    private PlayerControls inputActions;

    private bool touchingSurface;
    private bool isDragging;
    private bool isDead = false;

    private Vector2 dragStart;
    private Vector2 dragEnd;




    void Awake()
    {
        instance = this;
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
        if (bc == null)
            bc = GetComponent<BoxCollider2D>();
        bc.enabled = true;
        
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        gameoverText.enabled = false;
        continueText.enabled = false;
        startText.enabled = true;

        jumpsRemaining = 2;

        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (isDead)
        {
            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
            {
                RestartGame();
            }
            return;
        }

        if (isDragging)
        {
            dragEnd = GetTouchWorldPosition();
            DrawTrajectory();
        }
        
        if (rb.linearVelocityY < -0.1f)
        {
            bool isFalling = !touchingSurface && rb.linearVelocity.y < -0.2f;

            animator.SetBool("isFalling", isFalling);
        }
    }

    void FixedUpdate()
    {
        if (touchingSurface && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed)
            );
        }
    }

    // START = começa drag sempre
    void StartInput(InputAction.CallbackContext context)
    {
        if (isDead) return;
        if (jumpsRemaining <= 0)
            return;

        isDragging = true;
        startText.enabled = false;
        gameStarted = true;

        dragStart = GetTouchWorldPosition();

        lineRenderer.enabled = true;
    }

    void ReleaseInput(InputAction.CallbackContext context)
    {
        if (!isDragging) return;

        isDragging = false;

        lineRenderer.enabled = false;
        Launch();
        
    }

    void Launch()
    {

        touchingSurface = false;

        Vector2 dragDirection = dragStart - dragEnd;

        dragDirection = Vector2.ClampMagnitude(dragDirection, maxDragDistance);


        Vector2 launchVelocity = dragDirection * launchPower;
        
        if(jumpsRemaining < 2)
        {
            launchVelocity = dragDirection * ((launchPower+2)/2);
        }

        UpdateSprite(launchVelocity.x);

        rb.linearVelocity = launchVelocity;
        jumpsRemaining--;

        

        
    }

    void DrawTrajectory()
    {
        Vector2 dragDirection = dragStart - dragEnd;

        dragDirection = Vector2.ClampMagnitude(dragDirection, maxDragDistance);

        Vector2 initialVelocity = dragDirection * launchPower;
        
        
        if(jumpsRemaining < 2)
        {
            initialVelocity = dragDirection * ((launchPower + 2) / 2);
        }

        Vector2 gravity = Physics2D.gravity * rb.gravityScale;

        lineRenderer.positionCount = linePoints;

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;

            Vector2 point =
                (Vector2)transform.position +
                (initialVelocity * time) +
                (0.5f * gravity * time * time);

            lineRenderer.SetPosition(i, point);
        }
    }

    Vector2 GetTouchWorldPosition()
    {
        Vector2 screenPosition = Vector2.zero;

        if (Touchscreen.current != null)
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screenPosition = Mouse.current.position.ReadValue();

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane)
        );

        return worldPoint;
    }

    void UpdateSprite(float xVelocity)
    {
        if (xVelocity > 0.1f)
            spriteRenderer.flipX = false;
        else if (xVelocity < -0.1f)
            spriteRenderer.flipX = true;
    }

    void SetFacingDirection(Transform wall)
    {
        if (wall == null) return;

        float dir = wall.position.x - transform.position.x;

        spriteRenderer.flipX = dir < 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        touchingSurface = true;

        animator.SetBool("isFalling", false);

        if (collision.gameObject.CompareTag("Wall"))
        {
            SetFacingDirection(collision.transform);

            animator.SetBool("isGrab", true);
            jumpsRemaining = 2;
        }
        if (collision.gameObject.CompareTag("Floor"))
        {
            jumpsRemaining = 2;
        }

    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        touchingSurface = true;

    }

    private void OnCollisionExit2D(Collision2D collision)
    {

        touchingSurface = false;
        if (collision.gameObject.CompareTag("Wall"))
        {
            animator.SetBool("isGrab", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lava"))
            Die();
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        gameoverText.enabled = true;
        continueText.enabled = true;
        animator.SetBool("isFalling", false);
        animator.SetBool("isGrab", false);

        animator.SetTrigger("isDead");


        touchingSurface = false;
        isDragging = false;
        Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>().Follow = null;
        bc.enabled = false;

        lineRenderer.enabled = false;

        inputActions.Disable();

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);

        rb.angularVelocity = 400f;
        
    }

    void RestartGame()
    {
        gameoverText.enabled = false;
        continueText.enabled = false;
        startText.enabled = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}