using UnityEngine;

public class SideStructure : MonoBehaviour
{
    [Header("Referência")]
    [SerializeField] private Transform cam;

    [Header("Prefabs")]
    [SerializeField] private GameObject leftPrefab;
    [SerializeField] private GameObject rightPrefab;

    [Header("Spawn Y")]
    [SerializeField] private float spawnDistanceAboveCamera = 15f;
    [SerializeField] private float spawnIntervalY = 6f;

    [Header("X fixo")]
    [SerializeField] private float leftX = -2.6f;
    [SerializeField] private float rightX = 3.16f;

    private float nextSpawnY;
    private bool spawnLeft = true;

    void Start()
    {
        nextSpawnY = cam.position.y + spawnDistanceAboveCamera;
    }

    void Update()
    {
        float camY = cam.position.y;

        while (nextSpawnY < camY + spawnDistanceAboveCamera)
        {
            Spawn(nextSpawnY);
            nextSpawnY += spawnIntervalY;
        }
    }

    void Spawn(float y)
    {
        if (spawnLeft)
        {
            Vector3 pos = new Vector3(leftX, y, 0f);
            Instantiate(leftPrefab, pos, Quaternion.identity);
        }
        else
        {
            Vector3 pos = new Vector3(rightX, y, 0f);

            GameObject obj = Instantiate(rightPrefab, pos, Quaternion.identity);

            // FLIP via SpriteRenderer
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.flipX = true;
            }
        }

        spawnLeft = !spawnLeft;
    }
}