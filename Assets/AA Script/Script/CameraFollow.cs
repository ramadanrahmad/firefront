using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform target; // Kolom untuk memasukkan objek Player

    private Vector3 offset; // Menyimpan jarak antara kamera dan Player

    void Start()
    {
        // Secara otomatis menghitung jarak awal antara kamera dan Player saat game dimulai
        // Pastikan posisi kamera di scene (sebelum di-play) sudah sesuai dengan sudut pandang yang kamu inginkan
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    // Menggunakan LateUpdate sangat penting untuk kamera
    // LateUpdate dieksekusi SETELAH Update/FixedUpdate milik Player selesai
    // Ini memastikan kamera bergerak setelah karakter benar-benar sudah pindah posisi (mencegah kamera patah-patah/jitter)
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}