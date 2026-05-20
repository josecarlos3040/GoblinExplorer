using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private GameObject coinPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnDistanceAboveCamera = 12f;
    [SerializeField] private float spawnIntervalY = 5f;

    [Header("X Range")]
    [SerializeField] private float xMin = -3f;
    [SerializeField] private float xMax = 3f;

    [Header("Anti-wall")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float checkRadius = 0.35f;
    [SerializeField] private int maxAttempts = 5;

    private float nextSpawnY;

    void Start()
    {
        nextSpawnY = cam.position.y + spawnDistanceAboveCamera;
    }

    void Update()
    {
        float camY = cam.position.y;

        while (nextSpawnY < camY + spawnDistanceAboveCamera)
        {
            SpawnCoin(nextSpawnY);
            nextSpawnY += spawnIntervalY;
        }
    }

    void SpawnCoin(float y)
    {
        Vector2 pos = Vector2.zero;


        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(xMin, xMax);
            pos = new Vector2(x, y);

            // se não colidir com wall, aceita
            if (!Physics2D.OverlapCircle(pos, checkRadius, wallMask))
            {

                break;
            }
        }

        // se não achou posição perfeita, ainda spawna mesmo assim
        Instantiate(coinPrefab, pos, Quaternion.identity);
    }
}