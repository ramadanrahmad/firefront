using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 12, -8);
    public float smoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // ALWAYS LOOK NORTH (world forward)
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}