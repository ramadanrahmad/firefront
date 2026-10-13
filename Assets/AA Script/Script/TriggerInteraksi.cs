using UnityEngine;
using TMPro;

public class TriggerInteraksi : MonoBehaviour
{
    // TAMBAHAN BARU: Selang dan InfoPohon
    public enum TipeObjek { Pintu, Kucing, Kunci, PintuTerkunci, Selang, InfoPohon }

    [Header("Jenis Objek")]
    public TipeObjek tipeObjek;

    [Header("Pengaturan Prompt")]
    public string pesanTampil = "[E] Buka Pintu";
    public TMP_Text teksUI;

    [Header("Tombol Eksekusi")]
    public KeyCode tombolAksi = KeyCode.E;

    [Header("Pengaturan Suara")]
    public AudioSource audioSource;
    public AudioClip suaraBuka;

    [Header("Referensi Objek Pintu (Swap)")]
    public GameObject pintuTertutup;
    public GameObject pintuTerbuka;

    private bool sudahDipakai = false;
    private bool playerDiDalamArea = false;

    private void Update()
    {
        if (sudahDipakai) return;

        // Auto check jarak untuk tipe Selang agar selalu responsif dan tidak terhalang jitter collider
        if (tipeObjek == TipeObjek.Selang)
        {
            CekJarakSelang();
        }

        // Cek tombol interaksi
        bool tombolDitekan = Input.GetKeyDown(tombolAksi);
        if (tipeObjek == TipeObjek.Selang)
        {
            // Fleksibel: terima tombol yang disetting, atau F, atau E
            tombolDitekan = tombolDitekan || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E);
        }

        // Eksekusi interaksi saat tombol ditekan di dalam area
        if (playerDiDalamArea && tombolDitekan)
        {
            if (tipeObjek == TipeObjek.Pintu)
            {
                SelesaikanInteraksi();
            }
            else if (tipeObjek == TipeObjek.Kunci)
            {
                GameplayManager.punyaKunci = true;
                SelesaikanInteraksi();
            }
            else if (tipeObjek == TipeObjek.PintuTerkunci)
            {
                if (GameplayManager.punyaKunci)
                {
                    SelesaikanInteraksi();
                }
                else
                {
                    if (teksUI != null)
                    {
                        teksUI.text = "Pintu terkunci silakan cari kunci terlebih dahulu";
                        teksUI.gameObject.SetActive(true);
                    }
                    if (audioSource != null && suaraBuka != null) audioSource.PlayOneShot(suaraBuka);
                }
            }
            else if (tipeObjek == TipeObjek.Kucing)
            {
                SelesaikanInteraksi();
                GameplayManager manager = Object.FindAnyObjectByType<GameplayManager>();
                if (manager != null) manager.SelamatkanKucing();
            }
            // --- LOGIKA MENGAMBIL SELANG DI MOBIL ---
            else if (tipeObjek == TipeObjek.Selang)
            {
                AktifkanSelangPemain();
                SelesaikanInteraksi();
                Debug.Log("✅ [TriggerInteraksi] Selang berhasil diambil dan dipasang ke tangan pemain!");
            }
            // Catatan: TipeObjek.InfoPohon tidak butuh dipencet tombol, jadi tidak ada di sini.
        }
    }

    private void CekJarakSelang()
    {
        if (GameplayManager.punyaSelang)
        {
            SelesaikanInteraksi();
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            // Jarak wajar saat berdiri di belakang mobil (sekitar 3.5m)
            if (dist <= 3.5f)
            {
                playerDiDalamArea = true;
                if (teksUI != null && !teksUI.gameObject.activeSelf)
                {
                    teksUI.text = pesanTampil;
                    teksUI.gameObject.SetActive(true);
                }
            }
            else if (dist > 4.5f && playerDiDalamArea)
            {
                playerDiDalamArea = false;
                if (teksUI != null) teksUI.gameObject.SetActive(false);
            }
        }
    }

    public static void AktifkanSelangPemain()
    {
        GameplayManager.punyaSelang = true;

        // 1. Aktifkan via EquipHose (jika ada script EquipHose di mobil)
        EquipHose[] allEquip = Object.FindObjectsOfType<EquipHose>(true);
        foreach (var eq in allEquip)
        {
            if (eq != null)
            {
                if (eq.hoseHeld != null)
                {
                    eq.hoseHeld.SetActive(true);
                }
                eq.gameObject.SetActive(false);
            }
        }

        // 2. Aktifkan via WaterSpray (komponen semprotan air di GameObject FireHoseHeld)
        WaterSpray[] allSpray = Object.FindObjectsOfType<WaterSpray>(true);
        foreach (var ws in allSpray)
        {
            if (ws != null)
            {
                ws.gameObject.SetActive(true);
            }
        }

        // 3. Cari transform FireHoseHeld di Player atau Camera
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Transform[] allChilds = player.GetComponentsInChildren<Transform>(true);
            foreach (var t in allChilds)
            {
                if (t.name == "FireHoseHeld" || t.name == "HoseHoldPoint")
                {
                    t.gameObject.SetActive(true);
                    for (int i = 0; i < t.childCount; i++)
                    {
                        t.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
        }

        Debug.Log("🚒 [AktifkanSelangPemain] Selang pemadam berhasil diaktifkan!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sudahDipakai) return;

        if (other.CompareTag("Player"))
        {
            playerDiDalamArea = true;

            if (teksUI != null)
            {
                if (tipeObjek == TipeObjek.PintuTerkunci)
                {
                    if (GameplayManager.punyaKunci)
                        teksUI.text = pesanTampil;
                    else
                        teksUI.text = "Pintu terkunci silakan cari kunci terlebih dahulu";

                    teksUI.gameObject.SetActive(true);
                }
                // --- LOGIKA AREA PENGINGAT (POHON) ---
                else if (tipeObjek == TipeObjek.InfoPohon)
                {
                    // Teks hanya muncul JIKA pemain BELUM mengambil selang
                    if (!GameplayManager.punyaSelang)
                    {
                        teksUI.text = pesanTampil; // Muncul "Ambil selang di belakang mobil"
                        teksUI.gameObject.SetActive(true);
                    }
                }
                else
                {
                    teksUI.text = pesanTampil;
                    teksUI.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (sudahDipakai) return;

        if (other.CompareTag("Player"))
        {
            playerDiDalamArea = true;

            if (teksUI != null)
            {
                if (tipeObjek == TipeObjek.InfoPohon)
                {
                    if (!GameplayManager.punyaSelang)
                    {
                        teksUI.text = pesanTampil;
                        teksUI.gameObject.SetActive(true);
                    }
                    else
                    {
                        teksUI.gameObject.SetActive(false);
                    }
                }
                else if (!teksUI.gameObject.activeSelf)
                {
                    teksUI.text = pesanTampil;
                    teksUI.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDiDalamArea = false;
            if (teksUI != null) teksUI.gameObject.SetActive(false);
        }
    }

    private void SelesaikanInteraksi()
    {
        sudahDipakai = true;
        playerDiDalamArea = false;
        if (teksUI != null) teksUI.gameObject.SetActive(false);

        // --- SISTEM SUARA ANTI-TERPOTONG ---
        if (suaraBuka != null)
        {
            if (tipeObjek == TipeObjek.Kunci || tipeObjek == TipeObjek.Selang)
            {
                // Memutar suara di udara (speaker sementara) agar tidak mati saat objek hilang
                AudioSource.PlayClipAtPoint(suaraBuka, transform.position);
            }
            else if (audioSource != null)
            {
                // Untuk pintu, putar suara normal karena pintunya tidak hilang
                audioSource.PlayOneShot(suaraBuka);
            }
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Handling visual objek
        if (tipeObjek == TipeObjek.Kunci || tipeObjek == TipeObjek.Selang)
        {
            if (tipeObjek == TipeObjek.Selang)
            {
                AktifkanSelangPemain();
            }
            gameObject.SetActive(false);
        }
        else if (tipeObjek == TipeObjek.Pintu || tipeObjek == TipeObjek.PintuTerkunci)
        {
            if (pintuTertutup != null && pintuTerbuka != null)
            {
                pintuTertutup.SetActive(false);
                pintuTerbuka.SetActive(true);
            }
        }
    }
}