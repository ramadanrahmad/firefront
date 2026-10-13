using UnityEngine;

public class EquipHose : MonoBehaviour
{
    public GameObject hoseHeld;

    private bool playerNear = false;

    void Update()
    {
        // 1. Jika selang sudah diambil lewat TriggerInteraksi, sinkronkan segera
        if (GameplayManager.punyaSelang)
        {
            if (hoseHeld != null && !hoseHeld.activeSelf)
            {
                hoseHeld.SetActive(true);
            }
            gameObject.SetActive(false);
            return;
        }

        // 2. Fallback jarak jika player berdiri dekat rak selang truk
        if (!playerNear)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= 3.5f)
            {
                playerNear = true;
            }
        }

        // 3. Input F atau E untuk mengambil selang
        if (playerNear && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E)))
        {
            AmbilSelang();
        }
    }

    public void AmbilSelang()
    {
        GameplayManager.punyaSelang = true;

        if (hoseHeld != null)
        {
            hoseHeld.SetActive(true);
        }

        // Sinkronkan ke TriggerInteraksi
        TriggerInteraksi.AktifkanSelangPemain();

        Debug.Log("🚒 [EquipHose] Selang diambil melalui EquipHose!");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            Debug.Log("Tekan F untuk mengambil selang");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}
