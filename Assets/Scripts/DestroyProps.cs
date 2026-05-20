using UnityEngine;

public class DestroyProps : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float destroyDistance = 25f;

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


    }
}
