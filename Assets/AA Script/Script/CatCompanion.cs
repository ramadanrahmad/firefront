using UnityEngine;
using UnityEngine.AI;

public class CatCompanion : MonoBehaviour
{
    [Header("Pengaturan Target")]
    public Transform player;
    public Transform titikA;
    public Transform titikB;

    [Header("Pengaturan Perilaku Kucing")]
    public float waktuTungguDuduk = 3f; // Kucing akan duduk jika diam selama 3 detik
    public float intervalMeongMin = 5f; // Jarak waktu meong paling cepat (detik)
    public float intervalMeongMax = 15f; // Jarak waktu meong paling lama (detik)

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioKucing;

    private Transform targetPatroli;
    private bool isFollowing = false;
    private bool isPlayerNear = false;

    // Variabel penghitung waktu
    private float timerMeong;
    private float timerDuduk;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioKucing = GetComponent<AudioSource>();

        targetPatroli = titikA;
        if (targetPatroli != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(targetPatroli.position);
        }

        // Menentukan kapan kucing meong untuk pertama kalinya
        timerMeong = Random.Range(intervalMeongMin, intervalMeongMax);
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            // Jika NavMesh belum dibake, tetap jalankan interaksi selamatkan kucing
            if (!isFollowing && isPlayerNear && Input.GetKeyDown(KeyCode.F))
            {
                isFollowing = true;
                if (audioKucing != null) audioKucing.Play();
            }
            return;
        }

        // 1. Logika Interaksi (Mulai Mengikuti)
        if (!isFollowing && isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            isFollowing = true;
            if (audioKucing != null) audioKucing.Play();
            if (anim != null) anim.SetTrigger("Meow"); // Kucing meong saat diselamatkan
        }

        // 2. Logika Mode Mengikuti vs Patroli
        if (isFollowing)
        {
            // Terus kejar pemain
            if (player != null) agent.SetDestination(player.position);

            // LOGIKA DUDUK: Mengecek apakah kucing berhenti (sudah dekat pemain)
            // velocity.magnitude < 0.1f artinya kucing sedang tidak berjalan
            if (agent.velocity.magnitude < 0.1f && agent.remainingDistance <= agent.stoppingDistance)
            {
                timerDuduk += Time.deltaTime; // Waktu mulai berjalan

                // Jika sudah diam melebihi waktu tunggu (3 detik)
                if (timerDuduk >= waktuTungguDuduk)
                {
                    if (anim != null) anim.SetBool("IsSitting", true);
                }
            }
            else
            {
                // Jika pemain bergerak menjauh dan kucing harus jalan lagi, reset duduknya
                timerDuduk = 0f;
                if (anim != null) anim.SetBool("IsSitting", false);
            }
        }
        else
        {
            if (anim != null) anim.SetBool("IsSitting", false);
            // Mode Patroli (Maju Mundur)
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (titikA != null && titikB != null)
                {
                    targetPatroli = targetPatroli == titikA ? titikB : titikA;
                    agent.SetDestination(targetPatroli.position);
                }
            }

            // LOGIKA MEONG ACAK SAAT PATROLI
            timerMeong -= Time.deltaTime; // Hitung mundur
            if (timerMeong <= 0f)
            {
                if (audioKucing != null) audioKucing.Play();
                if (anim != null) anim.SetTrigger("Meow");

                // Reset timer dengan angka acak baru agar meongnya tidak monoton
                timerMeong = Random.Range(intervalMeongMin, intervalMeongMax);
            }
        }

        // 3. Update kecepatan untuk animasi jalan
        if (anim != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}