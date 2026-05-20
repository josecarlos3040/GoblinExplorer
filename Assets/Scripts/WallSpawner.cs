using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [Header("Referência")]
    [SerializeField] private Transform cam;

    [Header("Prefab")]
    [SerializeField] private GameObject wallPrefab;

    [Header("Spawn Y")]
    [SerializeField] private float spawnDistanceAboveCamera = 15f;
    [SerializeField] private float spawnIntervalY = 8f;

    [Header("Spawn X")]
    [SerializeField] private float xMin = -3f;
    [SerializeField] private float xMax = 3f;

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
            SpawnWall(nextSpawnY);
            nextSpawnY += spawnIntervalY;
        }
    }

    void SpawnWall(float y)
    {
        float randomX = Random.Range(xMin, xMax);

        Vector3 pos = new Vector3(randomX, y, 0f);

        Instantiate(wallPrefab, pos, Quaternion.identity);
    }
}