using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float baseSpeed = 1.5f;
    [SerializeField] private float distanceMultiplier = 0.05f;
    [SerializeField] private float maxSpeed = 12f;

    void Update()
    {
        if (Goblin.instance == null || !Goblin.instance.gameStarted)
            return;

        float distance = Mathf.Max(0, player.position.y - transform.position.y);

        float speed = baseSpeed + (distance * distanceMultiplier);
        speed = Mathf.Clamp(speed, baseSpeed, maxSpeed);

        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    public void ResetForRevive(float playerY)
    {
        transform.position = new Vector3(
            transform.position.x,
            playerY - 50f,
            transform.position.z
        );

        Debug.Log("Lava resetada para Y: " + transform.position.y);
    }
}