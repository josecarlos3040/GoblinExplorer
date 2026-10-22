using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] public TextMeshProUGUI distanceText;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Save System")]
    [SerializeField] private GoblinSaveSystem saveSystem;

    public static int coins = 0;
    public static int distanceGoblin = 0;

    private float startY;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        startY = player.position.y;

        // Carrega as moedas salvas
        if (saveSystem != null)
        {
            coins = saveSystem.Coins;
        }
        else
        {
            Debug.LogError("SaveManager não configurado no GameUI!");
        }

        // Reseta apenas a distância da partida
        distanceGoblin = 0;

        UpdateCoinsUI();
        UpdateDistanceUI();
    }

    void Update()
    {
        UpdateDistanceUI();
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        UpdateCoinsUI();
    }

    public void UpdateCoinsUI()
    {
        coinsText.text = " " + coins;

        // Salva sempre que o saldo mudar
        if (saveSystem != null && saveSystem.Coins != coins)
        {
            saveSystem.SaveCoins(coins);
        }
    }

    void UpdateDistanceUI()
    {
        float distance = Mathf.Max(
            0,
            player.position.y - startY
        );

        int currentDistance = Mathf.FloorToInt(distance);

        if (currentDistance > distanceGoblin)
        {
            distanceGoblin = currentDistance;
        }

        distanceText.text = currentDistance + "m";
    }
}