using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerAimer : MonoBehaviour
{
    public float rotationSpeed = 720f;
    public ParticleSystem extinguisherVFX;

    private Camera _cam;
    private ThirdPersonController _controller;

    private bool isAiming;

    void Start()
    {
        _cam = Camera.main;
        _controller = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        isAiming = Mouse.current.leftButton.isPressed;

        if (isAiming)
            RotateToMouse();

        HandleShooting();
    }

    void RotateToMouse()
    {
        Debug.Log("RotateToMouse running");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mousePos);

        // Plane at player height
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        // 🔴 Draw ray (RED)
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);

            // 🟢 Draw hit point
            Debug.DrawLine(hit, hit + Vector3.up * 2f, Color.green);

            Vector3 dir = hit - transform.position;
            dir.y = 0;

            // 🔵 Draw direction from player
            Debug.DrawLine(transform.position, transform.position + dir, Color.blue);

            if (dir.sqrMagnitude > 0.01f)
            {
                Debug.Log("DIR: " + dir + " | magnitude: " + dir.magnitude);
                Debug.Log("HIT: " + hit);

                Debug.Log("CAM FORWARD: " + _cam.transform.forward);
                Debug.Log("RAY DIR: " + ray.direction);
                Debug.Log("PLAYER FORWARD: " + transform.forward);
                Debug.Log("DIR NORMALIZED: " + dir.normalized);
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    rot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
    void HandleShooting()
    {
        if (extinguisherVFX == null) return;

        bool holdingAim = Mouse.current.leftButton.isPressed;

        if (holdingAim)
        {
            if (!extinguisherVFX.isPlaying)
                extinguisherVFX.Play();
        }
        else
        {
            if (extinguisherVFX.isPlaying)
                extinguisherVFX.Stop();
        }
    }
}