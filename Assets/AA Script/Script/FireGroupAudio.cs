using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireGroupAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private int totalApiDiGrup;
    private int apiPadamDiGrup = 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Menghitung berapa banyak objek api di dalam parent ini (misal: 2 api di dapur)
        Fire[] semuaApi = GetComponentsInChildren<Fire>(true);
        totalApiDiGrup = semuaApi.Length;
    }

    // Mendengarkan event dari script Fire buatan teman Anda
    void OnEnable()
    {
        Fire.OnFireExtinguished += HandleApiPadam;
    }

    void OnDisable()
    {
        Fire.OnFireExtinguished -= HandleApiPadam;
    }

    void HandleApiPadam(Fire apiYangPadam)
    {
        // Mengecek apakah api yang baru saja mati adalah anggota dari ruangan/grup ini
        if (apiYangPadam.transform.IsChildOf(this.transform))
        {
            apiPadamDiGrup++;

            // Jika semua api di ruangan ini sudah padam, matikan suara gemuruhnya
            if (apiPadamDiGrup >= totalApiDiGrup)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log("Semua api di " + gameObject.name + " berhasil dipadamkan. Suara dihentikan.");
                }
            }
        }
    }
}