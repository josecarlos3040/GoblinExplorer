using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI distanceText;

    [Header("Player")]
    [SerializeField] private Transform player;

    public static int coins = 0;
    private float startY;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        startY = player.position.y;
        UpdateCoinsUI();
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

    void UpdateCoinsUI()
    {
        coinsText.text = "Coins: " + coins;
    }

    void UpdateDistanceUI()
    {
        float distance = Mathf.Max(0, player.position.y - startY);
        distanceText.text = "Distance: " + Mathf.FloorToInt(distance) + "m";
    }
}