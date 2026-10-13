using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Firesomething.Forest
{
    public class ForestLevelManager : MonoBehaviour
    {
        public static ForestLevelManager Instance { get; private set; }

        [Header("Status Misi")]
        public float waktuMisi = 180f; // 3 Menit untuk menyelamatkan hutan
        public float waktuTersisa;
        public bool isGameOver = false;
        public bool isVictory = false;

        [Header("Penyelamatan Satwa")]
        public bool kucingDiselamatkan = false;

        [Header("Persentase Api")]
        public int totalTitikApi = 0;
        public int apiPadam = 0;

        [Header("Water Supply")]
        public float maxWaterSupply = 100f;
        public float currentWaterSupply = 100f;
        public float waterConsumptionRate = 12f; // Konsumsi per detik saat menyemprot
        public float waterRefillRate = 25f; // Pengisian ulang di dekat Fire Truck

        // Event listener
        public static event Action<float> OnTimerTick;
        public static event Action<int, int> OnFireProgressChanged;
        public static event Action<float, float> OnWaterSupplyChanged;
        public static event Action<bool> OnCatRescueStatusChanged;
        public static event Action<bool, float> OnMissionEnded; // (isWin, scorePercentage)

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            Fire.OnFireExtinguished += HandleFireExtinguished;
        }

        private void OnDisable()
        {
            Fire.OnFireExtinguished -= HandleFireExtinguished;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            waktuTersisa = waktuMisi;
            currentWaterSupply = maxWaterSupply;
            isGameOver = false;
            isVictory = false;
            kucingDiselamatkan = false;

            // Hitung semua objek Fire yang ada di scene
            HitungTotalApi();
            
            // Sync status awal dengan sistem lama (GameplayManager)
            GameplayManager.pintuSudahTerbuka = true;
            GameplayManager.punyaKunci = true;
            GameplayManager.punyaSelang = true;

            NotifyProgress();
        }

        private System.Collections.Generic.HashSet<GameObject> padamSet = new System.Collections.Generic.HashSet<GameObject>();

        public void HitungTotalApi()
        {
            padamSet.Clear();
            ForestFireHazard[] semuaHazard = FindObjectsOfType<ForestFireHazard>();
            if (semuaHazard != null && semuaHazard.Length > 0)
            {
                totalTitikApi = semuaHazard.Length;
            }
            else
            {
                Fire[] semuaApi = FindObjectsOfType<Fire>();
                totalTitikApi = semuaApi != null ? semuaApi.Length : 0;
            }
            apiPadam = 0;
            Debug.Log($"🌲 [ForestLevelManager] Terdeteksi {totalTitikApi} titik kebakaran hutan.");
            NotifyProgress();
        }

        private void Update()
        {
            if (isGameOver) return;

            // Pengurangan Waktu
            if (waktuTersisa > 0f)
            {
                waktuTersisa -= Time.deltaTime;
                if (waktuTersisa < 0f) waktuTersisa = 0f;
                OnTimerTick?.Invoke(waktuTersisa);
            }
            else
            {
                EvaluasiHasilMisi();
            }
        }

        public void GunakanAir(float deltaTime)
        {
            if (currentWaterSupply > 0f)
            {
                currentWaterSupply -= waterConsumptionRate * deltaTime;
                if (currentWaterSupply < 0f) currentWaterSupply = 0f;
                OnWaterSupplyChanged?.Invoke(currentWaterSupply, maxWaterSupply);
            }
        }

        public bool PunyaAirCukup()
        {
            return currentWaterSupply > 0.5f;
        }

        public void IsiUlangAir(float deltaTime)
        {
            if (currentWaterSupply < maxWaterSupply)
            {
                currentWaterSupply += waterRefillRate * deltaTime;
                if (currentWaterSupply > maxWaterSupply) currentWaterSupply = maxWaterSupply;
                OnWaterSupplyChanged?.Invoke(currentWaterSupply, maxWaterSupply);
            }
        }

        public void CatatApiPadam(GameObject fireObj = null)
        {
            if (fireObj != null)
            {
                if (padamSet.Contains(fireObj)) return;
                padamSet.Add(fireObj);
            }

            apiPadam++;
            Debug.Log($"🔥 [ForestLevelManager] Api padam: {apiPadam}/{totalTitikApi}");
            NotifyProgress();

            if (apiPadam >= totalTitikApi && totalTitikApi > 0)
            {
                KemenanganPenuh();
            }
        }

        private void HandleFireExtinguished(Fire fire)
        {
            CatatApiPadam(fire != null ? fire.gameObject : null);
        }

        public void SelamatkanKucingHutan()
        {
            if (!kucingDiselamatkan)
            {
                kucingDiselamatkan = true;
                Debug.Log("🐾 [ForestLevelManager] Kucing hutan berhasil diamankan ke basecamp!");
                OnCatRescueStatusChanged?.Invoke(true);

                // Integrasi dengan GameplayManager jika aktif
                GameplayManager gm = FindObjectOfType<GameplayManager>();
                if (gm != null) gm.SelamatkanKucing();
            }
        }

        private void NotifyProgress()
        {
            OnFireProgressChanged?.Invoke(apiPadam, totalTitikApi);
            OnWaterSupplyChanged?.Invoke(currentWaterSupply, maxWaterSupply);
            OnCatRescueStatusChanged?.Invoke(kucingDiselamatkan);
            OnTimerTick?.Invoke(waktuTersisa);
        }

        public float HitungPersentasePadam()
        {
            if (totalTitikApi <= 0) return 100f;
            return ((float)apiPadam / totalTitikApi) * 100f;
        }

        private void KemenanganPenuh()
        {
            isGameOver = true;
            isVictory = true;
            Time.timeScale = 1.0f; // Tetap pada kecepatan normal (tidak slow-mo)
            Debug.Log("🏆 [ForestLevelManager] SEMUA API HUTAN BERHASIL DIPADAMKAN!");
            OnMissionEnded?.Invoke(true, 100f);
        }

        private void EvaluasiHasilMisi()
        {
            isGameOver = true;
            float persentase = HitungPersentasePadam();

            if (persentase >= 75f)
            {
                isVictory = true;
                Debug.Log($"🎉 [ForestLevelManager] Misi Berhasil dengan pemadaman {persentase:F0}%");
                OnMissionEnded?.Invoke(true, persentase);
            }
            else
            {
                isVictory = false;
                Debug.Log($"💀 [ForestLevelManager] Misi Gagal! Hutan hangus terbakar. Hasil: {persentase:F0}%");
                OnMissionEnded?.Invoke(false, persentase);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "ForestLevel";
            SceneManager.LoadScene(sceneName);
        }

        public void KembaliKeMainMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("Main Menu");
        }
    }
}
