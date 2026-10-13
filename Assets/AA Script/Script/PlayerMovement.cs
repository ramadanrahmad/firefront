using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Kecepatan gerak karakter

    private Rigidbody rb;
    private Vector3 movement;

    void Start()
    {
        // Mengambil komponen Rigidbody yang ada pada Cube
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. Mengambil Input (WASD atau Arrow Keys)
        // GetAxisRaw membuat pergerakan lebih responsif (langsung berhenti saat tombol dilepas)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // 2. Menyimpan arah pergerakan ke dalam Vector3
        // .normalized mencegah karakter bergerak lebih cepat saat berjalan menyilang (diagonal)
        movement = (transform.right * moveX + transform.forward * moveZ).normalized;
    }

    void FixedUpdate()
    {
        // 3. Mengeksekusi pergerakan menggunakan sistem fisika
        MoveCharacter();
    }

    void MoveCharacter()
    {
        // Menghitung posisi baru berdasarkan posisi saat ini + (arah gerak * kecepatan * waktu fisika)
        Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // Memindahkan Rigidbody ke posisi baru
        rb.MovePosition(newPosition);
    }
}