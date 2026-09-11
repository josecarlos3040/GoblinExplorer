using TMPro;
using Unity.Cinemachine;
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

    [Header("Revive")]
    [SerializeField] private float wallSafeOffset = 0.3f;
    [SerializeField] private float dangerCheckRadius = 1.5f;
    [SerializeField] private LayerMask dangerLayers;

    private Vector2 lastSafeWallPosition;
    private bool hasSafeWallPosition = false;

    [SerializeField] private TextMeshProUGUI textReviveCoin;
    [SerializeField] public int coinCostForRevive = 50;

    [Header("Componentes")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D bc;
    [SerializeField] TextMeshProUGUI startText;
    [SerializeField] GameObject gameoverPanel;
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

        gameoverPanel.SetActive(false);
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
            
            //if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            //    (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
            //{
            //    RestartGame();
            //}
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

    void SetFacingDirection2(Transform wall)
    {
        if (wall == null) return;

        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    private void SaveSafeWallPosition(Collision2D collision)
    {
        if (collision == null)
            return;

        if (!collision.gameObject.CompareTag("Wall"))
            return;

        // Nunca salva posição em uma parede mortal
        if (collision.gameObject.CompareTag("Lava"))
            return;

        ContactPoint2D contact = collision.GetContact(0);

        Vector2 safePosition = contact.point + contact.normal * wallSafeOffset;

        Collider2D danger = Physics2D.OverlapCircle(
            safePosition,
            dangerCheckRadius,
            dangerLayers
        );

        if (danger != null)
        {
            Debug.Log("Posição de revive ignorada. Perigo próximo: " + danger.name);
            return;
        }

        lastSafeWallPosition = safePosition;
        hasSafeWallPosition = true;

        Debug.Log("Posição segura salva: " + lastSafeWallPosition);
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

            SaveSafeWallPosition(collision);
        }

        if (collision.gameObject.CompareTag("Wall2"))
        {
            SetFacingDirection2(collision.transform);

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

        if (collision.gameObject.CompareTag("Wall"))
        {
            SaveSafeWallPosition(collision);
        }
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

    [System.Obsolete]
    public void Revive()
    {
        if (!isDead)
            return;

        if (!hasSafeWallPosition)
        {
            Debug.LogWarning("Não existe uma posição segura para reviver.");
            return;
        }

        Debug.Log("REVIVENDO O GOBLIN!");

        isDead = false;

        gameoverPanel.SetActive(false);
        gameoverText.enabled = false;
        continueText.enabled = false;

        // Para completamente a física
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Para qualquer estado anterior da animação
        animator.Rebind();
        animator.Update(0f);

        // Reposiciona o jogador na última parede segura
        bc.enabled = false;
        transform.position = lastSafeWallPosition;
        bc.enabled = true;

        // Reseta estados do jogador
        touchingSurface = true;
        isDragging = false;
        jumpsRemaining = 2;

        // Reseta a animação
        animator.SetBool("isFalling", false);
        animator.SetBool("isGrab", true);

        lineRenderer.enabled = false;

        // Reativa os controles
        inputActions.Enable();

        // Reativa a câmera
        Unity.Cinemachine.CinemachineCamera camera =
            Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();

        if (camera != null)
        {
            camera.Follow = transform;
        }

        // Coloca a lava 100 unidades abaixo do jogador
        Lava lava = Object.FindFirstObjectByType<Lava>();

        if (lava != null)
        {
            lava.ResetForRevive(transform.position.y);
        }

        rb.linearVelocity = Vector2.zero;

        Debug.Log("Goblin revivido na posição: " + transform.position);
    }

    public void ReviveWithCoin()
    {
        if (GameUI.coins >= coinCostForRevive)
        {
            GameUI.coins -= coinCostForRevive;

            Revive();

            coinCostForRevive *= 2;

            textReviveCoin.text = coinCostForRevive + " coins";
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        gameoverPanel.SetActive(true);
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

    public void RestartGame()
    {
        gameoverPanel.SetActive(false);
        gameoverText.enabled = false;
        continueText.enabled = false;
        startText.enabled = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}