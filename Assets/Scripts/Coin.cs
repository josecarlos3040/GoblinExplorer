using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float destroyDistance = 25f;

    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float checkRadius = 0.3f;

    [SerializeField] GameObject particula;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = player.position.y - transform.position.y;

        if (distance > destroyDistance)
        {
            Destroy(gameObject);
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, checkRadius, wallMask);

        if (hit != null)
        {
            float dir = Random.value > 0.5f ? 1f : -1f;

            transform.position += Vector3.right * dir * 0.2f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameUI.instance.AddCoin(5);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Instantiate(particula, transform.position, transform.rotation);
    }
}