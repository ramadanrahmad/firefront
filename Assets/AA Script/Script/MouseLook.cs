using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Pengaturan Mouse")]
    public float mouseSensitivity = 300f; // Kecepatan putaran mouse
    public Transform playerBody; // Target badan karakter yang akan diputar ke kiri/kanan

    private float xRotation = 0f;

    void Start()
    {
        // Menyembunyikan dan mengunci kursor mouse di tengah layar saat game dimainkan
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        // Auto-lock jika pemain klik layar dan kursor belum terkunci
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 1. Mengambil input pergerakan mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 2. Menghitung rotasi Atas/Bawah (Sumbu X)
        xRotation -= mouseY;
        // Membatasi rotasi agar karakter tidak bisa melihat terlalu jauh ke belakang (kayang)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 3. Menerapkan rotasi Atas/Bawah HANYA pada Kamera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 4. Menerapkan rotasi Kiri/Kanan (Sumbu Y) pada SELURUH BADAN Player
        playerBody.Rotate(Vector3.up * mouseX);
    }
}