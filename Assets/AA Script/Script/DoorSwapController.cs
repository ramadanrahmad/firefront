using UnityEngine;

public class DoorSwapController : MonoBehaviour
{
    [Header("Referensi Objek Pintu")]
    public GameObject closedDoor; // Slot untuk pintu yang tertutup
    public GameObject openDoor;   // Slot untuk pintu yang terbuka

    private bool isOpen = false;
    private bool isPlayerNear = false;

    void Start()
    {
        // Memastikan saat game dimulai, pintu dalam keadaan tertutup
        closedDoor.SetActive(true);
        openDoor.SetActive(false);
    }

    void Update()
    {
        // Mengecek apakah pemain di dekat pintu dan menekan tombol 'E'
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen; // Ubah status (dari true ke false, atau sebaliknya)

            // Logika Menukar Objek:
            // Jika isOpen bernilai true, closedDoor mati, openDoor hidup.
            closedDoor.SetActive(!isOpen);
            openDoor.SetActive(isOpen);
        }
    }

    // Mendeteksi saat pemain masuk ke area pemicu
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    // Mendeteksi saat pemain keluar dari area pemicu
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}