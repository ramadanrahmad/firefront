using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Firesomething.Forest
{
    public class ForestHUDManager : MonoBehaviour
    {
        [Header("Text Objective & Timer")]
        public TMP_Text timerText;
        public TMP_Text mainObjectiveText;
        public TMP_Text optionalObjectiveText;
        public TMP_Text fireDistanceText;

        [Header("Water Supply UI")]
        public Slider waterSupplySlider;
        public Image waterFillImage;
        public TMP_Text waterSupplyText;

        [Header("Health UI")]
        public Slider healthSlider;
        public Image heatVignetteImage;

        [Header("Banner Pengumuman")]
        public GameObject announcementBanner;
        public TMP_Text announcementText;
        public float bannerDuration = 3f;
        private float bannerTimer = 0f;

        [Header("Panel Hasil Akhir")]
        public GameObject victoryPanel;
        public GameObject defeatPanel;
        public TMP_Text victoryScoreText;
        public TMP_Text defeatReasonText;

        [Header("Pause UI")]
        public GameObject pausePanel;
        public bool isPaused = false;

        [Header("Audio UI")]
        public AudioSource uiAudioSource;
        public AudioClip clickSfx;
        public AudioClip winSfx;
        public AudioClip lossSfx;

        private void OnEnable()
        {
            ForestLevelManager.OnTimerTick += UpdateTimerDisplay;
            ForestLevelManager.OnFireProgressChanged += UpdateFireProgress;
            ForestLevelManager.OnWaterSupplyChanged += UpdateWaterSupply;
            ForestLevelManager.OnCatRescueStatusChanged += UpdateCatRescue;
            ForestLevelManager.OnMissionEnded += HandleMissionEnd;
        }

        private void OnDisable()
        {
            ForestLevelManager.OnTimerTick -= UpdateTimerDisplay;
            ForestLevelManager.OnFireProgressChanged -= UpdateFireProgress;
            ForestLevelManager.OnWaterSupplyChanged -= UpdateWaterSupply;
            ForestLevelManager.OnCatRescueStatusChanged -= UpdateCatRescue;
            ForestLevelManager.OnMissionEnded -= HandleMissionEnd;
        }

        private void Start()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            if (announcementBanner != null) announcementBanner.SetActive(false);

            ShowBanner("🔥 PADAMKAN SELURUH KEBAKARAN HUTAN!");
        }

        private void Update()
        {
            // Banner timer
            if (bannerTimer > 0f)
            {
                bannerTimer -= Time.unscaledDeltaTime;
                if (bannerTimer <= 0f && announcementBanner != null)
                {
                    announcementBanner.SetActive(false);
                }
            }

            // Input Pause (ESC atau P)
            if (ForestLevelManager.Instance == null || !ForestLevelManager.Instance.isGameOver)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    if (isPaused)
                    {
                        ResumeGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
            }
        }

        public void ShowBanner(string message)
        {
            if (announcementBanner != null && announcementText != null)
            {
                announcementText.text = message;
                announcementBanner.SetActive(true);
                bannerTimer = bannerDuration;
            }
        }

        private void UpdateTimerDisplay(float seconds)
        {
            if (timerText == null) return;
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            timerText.text = $"{mins:00}:{secs:00}";

            if (seconds <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        private void UpdateFireProgress(int padam, int total)
        {
            if (mainObjectiveText == null) return;
            float percent = total > 0 ? ((float)padam / total) * 100f : 100f;
            mainObjectiveText.text = $"PADAMKAN API: {padam}/{total} ({percent:F0}%)";
            mainObjectiveText.color = new Color(1f, 0.28f, 0.22f);
            
            if (padam > 0 && padam < total)
            {
                ShowBanner($"💧 Api Berhasil Dipadamkan! ({padam}/{total})");
            }
        }

        private void UpdateWaterSupply(float current, float max)
        {
            if (waterSupplySlider != null)
            {
                waterSupplySlider.maxValue = max;
                waterSupplySlider.value = current;
            }

            if (waterSupplyText != null)
            {
                float percent = (current / max) * 100f;
                waterSupplyText.text = $"PASOKAN AIR: {percent:F0}%";
                
                if (percent <= 20f)
                {
                    waterSupplyText.color = Color.red;
                }
                else
                {
                    waterSupplyText.color = new Color(0.25f, 0.85f, 1f);
                }
            }
        }

        private void UpdateCatRescue(bool isRescued)
        {
            // Objektif kucing dihapus sesuai permintaan pengguna agar fokus murni pada pemadaman kebakaran
            if (optionalObjectiveText != null)
            {
                optionalObjectiveText.gameObject.SetActive(false);
            }
        }

        private void UpdateNearestFireDistance()
        {
            // Digantikan oleh timer digital besar di bagian atas tengah layar
            if (fireDistanceText != null)
            {
                fireDistanceText.text = "";
                fireDistanceText.gameObject.SetActive(false);
            }
        }

        private void HandleMissionEnd(bool isWin, float score)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (isWin)
            {
                if (victoryPanel != null)
                {
                    victoryPanel.SetActive(true);
                }
                else
                {
                    ShowMissionEndModal(true, score);
                }

                if (victoryScoreText != null)
                {
                    victoryScoreText.text = $"Hutan Terselamatkan!\nEfisiensi Pemadaman: {score:F0}%";
                }
                if (uiAudioSource != null && winSfx != null) uiAudioSource.PlayOneShot(winSfx);
            }
            else
            {
                if (defeatPanel != null)
                {
                    defeatPanel.SetActive(true);
                }
                else
                {
                    ShowMissionEndModal(false, score);
                }

                if (defeatReasonText != null)
                {
                    defeatReasonText.text = $"Waktu Habis!\nHanya {score:F0}% api yang berhasil dipadamkan.\nHutan telah habis terbakar.";
                }
                if (uiAudioSource != null && lossSfx != null) uiAudioSource.PlayOneShot(lossSfx);
            }
        }

        private void ShowMissionEndModal(bool isWin, float score)
        {
            GameObject modal = new GameObject(isWin ? "VictoryModal" : "DefeatModal", typeof(RectTransform));
            modal.transform.SetParent(transform, false);

            RectTransform rt = modal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background gelap
            UnityEngine.UI.Image bgImg = modal.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.05f, 0.08f, 0.12f, 0.88f);

            // Container box tengah
            GameObject box = new GameObject("CenterBox", typeof(RectTransform));
            box.transform.SetParent(modal.transform, false);
            RectTransform boxRt = box.GetComponent<RectTransform>();
            boxRt.sizeDelta = new Vector2(580f, 320f);
            UnityEngine.UI.Image boxImg = box.AddComponent<UnityEngine.UI.Image>();
            boxImg.color = new Color(0.10f, 0.14f, 0.20f, 0.96f);
            boxImg.raycastTarget = false; // Jangan blokir klik tombol anak

            // Judul
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
            titleObj.transform.SetParent(box.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 95f);
            titleRt.sizeDelta = new Vector2(520f, 55f);
            TMP_Text titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 30;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = isWin ? new Color(1f, 0.84f, 0.2f) : new Color(1f, 0.35f, 0.35f);
            titleTmp.text = isWin ? "MISI BERHASIL!" : "MISI GAGAL";
            titleTmp.raycastTarget = false;

            // Deskripsi Hasil
            GameObject descObj = new GameObject("DescText", typeof(RectTransform));
            descObj.transform.SetParent(box.transform, false);
            RectTransform descRt = descObj.GetComponent<RectTransform>();
            descRt.anchoredPosition = new Vector2(0f, 25f);
            descRt.sizeDelta = new Vector2(520f, 75f);
            TMP_Text descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 19;
            descTmp.alignment = TextAlignmentOptions.Center;
            descTmp.color = new Color(0.9f, 0.92f, 0.95f);
            descTmp.text = isWin 
                ? $"Semua titik kebakaran hutan berhasil dipadamkan!\nEfisiensi Pemadaman: {score:F0}%"
                : $"Waktu habis sebelum api sempat dipadamkan.\nHasil: {score:F0}% api padam.";
            descTmp.raycastTarget = false;

            // 1. Tombol Main Lagi (Restart)
            GameObject restartBtnObj = new GameObject("RestartBtn", typeof(RectTransform));
            restartBtnObj.transform.SetParent(box.transform, false);
            RectTransform restartBtnRt = restartBtnObj.GetComponent<RectTransform>();
            restartBtnRt.anchoredPosition = new Vector2(-130f, -80f);
            restartBtnRt.sizeDelta = new Vector2(230f, 50f);
            UnityEngine.UI.Image restartBtnImg = restartBtnObj.AddComponent<UnityEngine.UI.Image>();
            restartBtnImg.color = isWin ? new Color(0.18f, 0.65f, 0.35f) : new Color(0.85f, 0.35f, 0.2f);
            restartBtnImg.raycastTarget = true;

            UnityEngine.UI.Button restartBtn = restartBtnObj.AddComponent<UnityEngine.UI.Button>();
            restartBtn.targetGraphic = restartBtnImg;
            UnityEngine.UI.ColorBlock cbR = restartBtn.colors;
            cbR.normalColor = restartBtnImg.color;
            cbR.highlightedColor = new Color(0.25f, 0.85f, 0.45f);
            cbR.pressedColor = new Color(0.12f, 0.50f, 0.25f);
            restartBtn.colors = cbR;
            restartBtn.onClick.AddListener(OnClickRestart);

            GameObject restartTextObj = new GameObject("RestartText", typeof(RectTransform));
            restartTextObj.transform.SetParent(restartBtnObj.transform, false);
            RectTransform restartTextRt = restartTextObj.GetComponent<RectTransform>();
            restartTextRt.anchorMin = Vector2.zero;
            restartTextRt.anchorMax = Vector2.one;
            restartTextRt.offsetMin = Vector2.zero;
            restartTextRt.offsetMax = Vector2.zero;
            TMP_Text restartTmp = restartTextObj.AddComponent<TextMeshProUGUI>();
            restartTmp.fontSize = 19;
            restartTmp.fontStyle = FontStyles.Bold;
            restartTmp.alignment = TextAlignmentOptions.Center;
            restartTmp.color = Color.white;
            restartTmp.text = "Main Lagi";
            restartTmp.raycastTarget = false; // Jangan blokir klik tombol

            // 2. Tombol Main Menu
            GameObject menuBtnObj = new GameObject("MainMenuBtn", typeof(RectTransform));
            menuBtnObj.transform.SetParent(box.transform, false);
            RectTransform menuBtnRt = menuBtnObj.GetComponent<RectTransform>();
            menuBtnRt.anchoredPosition = new Vector2(130f, -80f);
            menuBtnRt.sizeDelta = new Vector2(230f, 50f);
            UnityEngine.UI.Image menuBtnImg = menuBtnObj.AddComponent<UnityEngine.UI.Image>();
            menuBtnImg.color = new Color(0.25f, 0.45f, 0.85f);
            menuBtnImg.raycastTarget = true;

            UnityEngine.UI.Button menuBtn = menuBtnObj.AddComponent<UnityEngine.UI.Button>();
            menuBtn.targetGraphic = menuBtnImg;
            UnityEngine.UI.ColorBlock cbM = menuBtn.colors;
            cbM.normalColor = menuBtnImg.color;
            cbM.highlightedColor = new Color(0.35f, 0.60f, 1.0f);
            cbM.pressedColor = new Color(0.15f, 0.30f, 0.65f);
            menuBtn.colors = cbM;
            menuBtn.onClick.AddListener(OnClickMainMenu);

            GameObject menuTextObj = new GameObject("MenuText", typeof(RectTransform));
            menuTextObj.transform.SetParent(menuBtnObj.transform, false);
            RectTransform menuTextRt = menuTextObj.GetComponent<RectTransform>();
            menuTextRt.anchorMin = Vector2.zero;
            menuTextRt.anchorMax = Vector2.one;
            menuTextRt.offsetMin = Vector2.zero;
            menuTextRt.offsetMax = Vector2.zero;
            TMP_Text menuTmp = menuTextObj.AddComponent<TextMeshProUGUI>();
            menuTmp.fontSize = 19;
            menuTmp.fontStyle = FontStyles.Bold;
            menuTmp.alignment = TextAlignmentOptions.Center;
            menuTmp.color = Color.white;
            menuTmp.text = "Main Menu";
            menuTmp.raycastTarget = false; // Jangan blokir klik tombol

            // Pastikan EventSystem aktif dan GraphicRaycaster terpasang
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject es = new GameObject("RuntimeEventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            UnityEngine.UI.GraphicRaycaster gr = GetComponentInParent<UnityEngine.UI.GraphicRaycaster>();
            if (gr != null) gr.enabled = true;

            if (isWin) victoryPanel = modal; else defeatPanel = modal;
        }

        public void PauseGame()
        {
            if (ForestLevelManager.Instance != null && ForestLevelManager.Instance.isGameOver) return;

            isPaused = true;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            else
            {
                ShowPauseModal();
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void ShowPauseModal()
        {
            GameObject modal = new GameObject("PauseModal", typeof(RectTransform));
            modal.transform.SetParent(transform, false);

            RectTransform rt = modal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background gelap transparan
            UnityEngine.UI.Image bgImg = modal.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.04f, 0.07f, 0.12f, 0.85f);
            bgImg.raycastTarget = true;

            // Box tengah
            GameObject box = new GameObject("PauseBox", typeof(RectTransform));
            box.transform.SetParent(modal.transform, false);
            RectTransform boxRt = box.GetComponent<RectTransform>();
            boxRt.sizeDelta = new Vector2(480f, 320f);
            UnityEngine.UI.Image boxImg = box.AddComponent<UnityEngine.UI.Image>();
            boxImg.color = new Color(0.11f, 0.15f, 0.22f, 0.98f);
            boxImg.raycastTarget = false;

            // Judul
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
            titleObj.transform.SetParent(box.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 95f);
            titleRt.sizeDelta = new Vector2(440f, 50f);
            TMP_Text titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 32;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;
            titleTmp.text = "GAME DIJEDA";
            titleTmp.raycastTarget = false;

            // Subtitle instruksi
            GameObject subObj = new GameObject("SubText", typeof(RectTransform));
            subObj.transform.SetParent(box.transform, false);
            RectTransform subRt = subObj.GetComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0f, 45f);
            subRt.sizeDelta = new Vector2(440f, 35f);
            TMP_Text subTmp = subObj.AddComponent<TextMeshProUGUI>();
            subTmp.fontSize = 16;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.7f, 0.75f, 0.82f);
            subTmp.text = "Tekan ESC atau klik tombol di bawah";
            subTmp.raycastTarget = false;

            // 1. Tombol Lanjutkan
            CreatePauseButton(box.transform, new Vector2(0f, -10f), new Vector2(360f, 48f),
                "Lanjutkan", new Color(0.18f, 0.65f, 0.35f), () => ResumeGame());

            // 2. Tombol Main Lagi
            CreatePauseButton(box.transform, new Vector2(0f, -65f), new Vector2(360f, 48f),
                "Main Lagi", new Color(0.85f, 0.45f, 0.15f), () => OnClickRestart());

            // 3. Tombol Main Menu
            CreatePauseButton(box.transform, new Vector2(0f, -120f), new Vector2(360f, 48f),
                "Main Menu", new Color(0.22f, 0.42f, 0.75f), () => OnClickMainMenu());

            // EventSystem & Raycaster
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject es = new GameObject("RuntimeEventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            UnityEngine.UI.GraphicRaycaster gr = GetComponentInParent<UnityEngine.UI.GraphicRaycaster>();
            if (gr != null) gr.enabled = true;

            pausePanel = modal;
        }

        private void CreatePauseButton(Transform parent, Vector2 pos, Vector2 size, string text, Color color, System.Action onClick)
        {
            GameObject btnObj = new GameObject(text + "_Btn", typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchoredPosition = pos;
            btnRt.sizeDelta = size;

            UnityEngine.UI.Image btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = color;
            btnImg.raycastTarget = true;

            UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = btnImg;
            UnityEngine.UI.ColorBlock cb = btn.colors;
            cb.normalColor = color;
            cb.highlightedColor = color * 1.2f;
            cb.pressedColor = color * 0.8f;
            btn.colors = cb;
            btn.onClick.AddListener(() =>
            {
                PlayClickSfx();
                onClick?.Invoke();
            });

            GameObject txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 19;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = text;
            tmp.raycastTarget = false;
        }

        public void OnClickRestart()
        {
            PlayClickSfx();
            Time.timeScale = 1f;
            if (ForestLevelManager.Instance != null)
            {
                ForestLevelManager.Instance.RestartLevel();
            }
        }

        public void OnClickMainMenu()
        {
            PlayClickSfx();
            Time.timeScale = 1f;
            if (ForestLevelManager.Instance != null)
            {
                ForestLevelManager.Instance.KembaliKeMainMenu();
            }
        }

        private void PlayClickSfx()
        {
            if (uiAudioSource != null && clickSfx != null)
            {
                uiAudioSource.PlayOneShot(clickSfx);
            }
        }
    }
}
