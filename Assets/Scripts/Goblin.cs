using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goblin Explorer — codigo do personagem.
// Para o video: procure as regioes "ESTILINGUE" e "OUTRAS MECANICAS".
// Os comentarios explicam o que ja estava funcionando; nao alteram a logica.
public class Goblin : MonoBehaviour
{
    public static Goblin instance;

    public bool gameStarted = false;

    [Header("Pulo")]
    // Quanto maior esse numero, mais forte o primeiro lancamento.
    [SerializeField] float launchPower = 8f;
    // Quantos pulos ainda temos antes de encostar em outra superficie.
    [SerializeField] int jumpsRemaining;

    [Header("Estilingue")]
    // Limite para o jogador nao puxar o "estilingue" infinitamente.
    [SerializeField] float maxDragDistance = 4f;

    [Header("Wall Slide")]
    [SerializeField] float wallSlideSpeed = 1.5f;

    [Header("Trajetória")]
    // Desenha a linha pontilhada/curva que indica para onde vamos.
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int linePoints = 30;
    [SerializeField] float timeBetweenPoints = 0.05f;

    [Header("Revive")]
    [SerializeField] private float wallSafeOffset = 0.3f;
    [SerializeField] private float dangerCheckRadius = 1.5f;
    [SerializeField] private LayerMask dangerLayers;

    private Vector2 lastSafeWallPosition;
    private bool hasSafeWallPosition = false;
    private Pointer activePointer;

    [SerializeField] private TextMeshProUGUI textReviveCoin;
    [SerializeField] public int coinCostForRevive = 50;

    [Header("Componentes")]
    // O Rigidbody2D e a parte que recebe a velocidade e reage a gravidade.
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D bc;
    [SerializeField] TextMeshProUGUI startText;
    [SerializeField] GameObject gameoverPanel;
    [SerializeField] TextMeshProUGUI gameoverText;

    [SerializeField] private GoblinSaveSystem saveSystem;


    [SerializeField] TextMeshProUGUI continueText;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    // Reconhece os controles configurados no Input System da Unity.
    private PlayerControls inputActions;

    private bool touchingSurface;
    private bool isDragging;
    private bool isDead = false;

    // Vector2 e uma posicao com dois numeros: X e Y.
    // Guardamos onde o jogador comecou e terminou de arrastar.
    private Vector2 dragStart;
    private Vector2 dragEnd;




    void Awake()
    {
        instance = this;
        inputActions = new PlayerControls();
    }

    // Quando este script e ativado, comecamos a escutar os comandos.
    void OnEnable()
    {
        inputActions.Enable();

        // Pressionar = StartInput; soltar = ReleaseInput.
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

        if (spriteRenderer == null)
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


    // Update roda a cada quadro do jogo.
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
            // Enquanto arrastamos, atualizamos o mouse e redesenhamos a linha.
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



    // PASSO 1: O jogador aperta o mouse ou toca na tela.
    void StartInput(InputAction.CallbackContext context)
    {
        // Se morreu ou acabou os pulos, nao pode lancar.
        if (isDead || jumpsRemaining <= 0)
            return;

        // Descobrimos qual dispositivo o jogador esta usando (mouse ou toque).
        activePointer = context.control.device as Pointer;

        if (activePointer == null)
            return;

        // Dizemos ao jogo: "o jogador esta arrastando".
        isDragging = true;
        startText.enabled = false;
        gameStarted = true;

        // Guardamos o ponto inicial do arrasto.
        dragStart = GetTouchWorldPosition();
        dragEnd = dragStart;

        // Mostra a previsao do caminho do goblin.
        lineRenderer.enabled = true;

        Debug.Log("Dispositivo: " + activePointer.displayName);
    }

    // PASSO 2: O jogador solta o mouse ou tira o dedo da tela.
    void ReleaseInput(InputAction.CallbackContext context)
    {
        if (!isDragging)
            return;

        // Atualiza a posição antes de lançar
        dragEnd = GetTouchWorldPosition();

        Debug.Log(
            "Distância do drag: " +
            Vector2.Distance(dragStart, dragEnd)
        );

        // Paramos de desenhar a linha e preparamos o lancamento.
        isDragging = false;
        lineRenderer.enabled = false;

        // Executa o movimento do goblin.
        Launch();

        activePointer = null;
    }

    // PASSO 3: Calculamos a direcao e a forca do lancamento.
    void Launch()
    {

        touchingSurface = false;

        // Exemplo: puxou para baixo? O goblin vai na direcao contraria: para cima!
        Vector2 dragDirection = dragStart - dragEnd;

        // Limita quanto a puxada pode influenciar na velocidade.
        dragDirection = Vector2.ClampMagnitude(dragDirection, maxDragDistance);


        // Multiplicamos a direcao pela potencia do estilingue.
        Vector2 launchVelocity = dragDirection * launchPower;

        // Se for o segundo pulo, o lancamento fica mais fraco.
        if (jumpsRemaining < 2)
        {
            launchVelocity = dragDirection * ((launchPower + 2) / 2);
        }

        UpdateSprite(launchVelocity.x);

        // Esta linha faz o Rigidbody2D se mover de verdade.
        rb.linearVelocity = launchVelocity;
        // Gastamos um dos pulos disponiveis.
        jumpsRemaining--;




    }

    // PASSO 4: Desenhamos uma PREVISAO do trajeto antes de soltar.
    void DrawTrajectory()
    {
        Vector2 dragDirection = dragStart - dragEnd;

        dragDirection = Vector2.ClampMagnitude(dragDirection, maxDragDistance);

        Vector2 initialVelocity = dragDirection * launchPower;


        if (jumpsRemaining < 2)
        {
            initialVelocity = dragDirection * ((launchPower + 2) / 2);
        }

        // Usamos a gravidade para prever a curva do salto.
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;

        lineRenderer.positionCount = linePoints;

        // Desenhamos a trajetoria ponto por ponto.
        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;

            Vector2 point =
                (Vector2)transform.position +
                (initialVelocity * time) +
                (0.5f * gravity * time * time);

            // Coloca cada ponto na linha que aparece na tela.
            lineRenderer.SetPosition(i, point);
        }
    }

    // AJUDA: transforma a posicao do mouse na tela em uma posicao do mundo.
    Vector2 GetTouchWorldPosition()
    {
        if (activePointer == null)
            return transform.position;

        Vector2 screenPosition =
            activePointer.position.ReadValue();

        Camera cam = Camera.main;

        float cameraDistance = Mathf.Abs(
            cam.transform.position.z -
            transform.position.z
        );

        Vector3 worldPosition = cam.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                cameraDistance
            )
        );

        return new Vector2(
            worldPosition.x,
            worldPosition.y
        );
    }

 



    // Vira o sprite, cuida das paredes, colisoes, revive e game over.
    // Tudo abaixo continua igual ao seu script original.
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

        //Debug.Log("Posição segura salva: " + lastSafeWallPosition);
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
            GameUI.coins = GameUI.coins - coinCostForRevive;
            GameUI.instance.UpdateCoinsUI();

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
        saveSystem.SaveGameResult(GameUI.distanceGoblin);

    }

    public void RestartGame()
    {

        gameoverPanel.SetActive(false);
        gameoverText.enabled = false;
        continueText.enabled = false;
        startText.enabled = true;

        saveSystem.UpdateUI();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    
}