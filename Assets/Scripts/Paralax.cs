using UnityEngine;

public class Paralax : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private float speed = 0.3f;

    private Vector3 startPos;
    private float startCamY;

    private void Start()
    {
        startPos = transform.position;
        startCamY = cam.position.y;
    }

    private void LateUpdate()
    {
        float y = startPos.y + ((cam.position.y - startCamY) * speed);

        transform.position = new Vector3(
            startPos.x,
            y,
            startPos.z
        );
    }
}