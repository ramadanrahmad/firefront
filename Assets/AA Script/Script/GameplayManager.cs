using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    public static bool pintuSudahTerbuka = false;
    public static bool punyaKunci = false;
    public static bool punyaSelang = false; //

    [Header("Pengaturan UI Objective")]
    public TMP_Text textTimer;
    public TMP_Text textMainObjective;
    public TMP_Text textOptionalObjective;

    [Header("Panel Win/Loss")]
    public GameObject panelWin;
    public GameObject panelLoss;
    public TMP_Text textStatusMenang;

    [Header("Panel Pause")]
    public GameObject panelPause;
    private bool isPaused = false;

    [Header("Pengaturan Audio UI")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Sistem Waktu")]
    public float waktuTersisa = 30f;

    private bool gameSelesai = false;

    [Header("Fire System")]
    private int totalApiAwal = 0;
    private int apiPadam = 0;

    [Header("Sistem Kucing")]
    private bool kucingDiselamatkan = false;

    // =========================
    // EVENT LISTENER
    // =========================

    void OnEnable()
    {
        Fire.OnFireExtinguished += HandleFireExtinguished;
    }

    void OnDisable()
    {
        Fire.OnFireExtinguished -= HandleFireExtinguished;
    }

    // =========================
    // START
    // =========================

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        gameSelesai = false;

        // Pastikan kursor terkunci dan fokus ke game saat mulai
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        pintuSudahTerbuka = false;
        punyaKunci = false;
        punyaSelang = false;

        // cari semua fire dalam scene
        Fire[] semuaApi = FindObjectsOfType<Fire>();

        totalApiAwal = semuaApi.Length;

        Debug.Log("🔥 TOTAL FIRE: " + totalApiAwal);

        if (panelWin != null) panelWin.SetActive(false);
        if (panelLoss != null) panelLoss.SetActive(false);

        UpdateObjectiveUI();
        BuatTombolPauseHUD();
    }

    // =========================
    // UPDATE
    // =========================

    void Update()
    {
        if (gameSelesai)
            return;

        HandlePauseInput();

        if (isPaused)
            return;

        HandleTimer();
    }

    // =========================
    // TIMER
    // =========================

    void HandleTimer()
    {
        if (waktuTersisa > 0)
        {
            waktuTersisa -= Time.deltaTime;

            UpdateDisplayTimer(waktuTersisa);
        }
        else
        {
            waktuTersisa = 0;

            CekKondisiGameSelesai();
        }
    }

    void UpdateDisplayTimer(float waktu)
    {
        float menit = Mathf.FloorToInt(waktu / 60);
        float detik = Mathf.FloorToInt(waktu % 60);

        textTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
    }

    // =========================
    // FIRE SYSTEM
    // =========================

    void HandleFireExtinguished(Fire fire)
    {
        apiPadam++;

        Debug.Log("🔥 FIRE PADAM: " + apiPadam + "/" + totalApiAwal);

        UpdateObjectiveUI();

        // semua api padam
        if (apiPadam >= totalApiAwal)
        {
            KondisiMenang(100f);
        }
    }

    float HitungPersentasePadam()
    {
        if (totalApiAwal <= 0)
            return 0f;

        return ((float)apiPadam / totalApiAwal) * 100f;
    }

    // =========================
    // OBJECTIVE UI
    // =========================

    void UpdateObjectiveUI()
    {
        float persen = HitungPersentasePadam();

        textMainObjective.text =
            "Main Objective:\nExtinguish Fire (" +
            Mathf.RoundToInt(persen) +
            "%)";

        string statusKucing =
            kucingDiselamatkan ? "1/1" : "0/1";

        textOptionalObjective.text =
            "Optional:\nRescue Pet (" +
            statusKucing +
            ")";

        textOptionalObjective.color =
            kucingDiselamatkan ? Color.green : Color.white;
    }

    // =========================
    // CAT SYSTEM
    // =========================

    public void SelamatkanKucing()
    {
        if (!kucingDiselamatkan)
        {
            kucingDiselamatkan = true;

            Debug.Log("🐱 Kucing diselamatkan!");

            UpdateObjectiveUI();
        }
    }

    // =========================
    // GAME END
    // =========================

    void CekKondisiGameSelesai()
    {
        gameSelesai = true;

        float persentaseAkhir = HitungPersentasePadam();

        if (persentaseAkhir >= 80f)
        {
            KondisiMenang(persentaseAkhir);
        }
        else
        {
            CekKondisiKalah();
        }
    }

    void KondisiMenang(float skorAkhir = 100f)
    {
        gameSelesai = true;

        // Buka akses ke Misi 2 (Kebakaran Hutan)
        PlayerPrefs.SetInt("ForestLevelUnlocked", 1);
        PlayerPrefs.Save();
        Debug.Log("🔓 [GameplayManager] Level 2 (Kebakaran Hutan) Berhasil Dibuka!");

        SembunyikanTeksHUD();

        // Pastikan popup bawaan lama dimatikan
        if (panelWin != null) panelWin.SetActive(false);
        if (panelLoss != null) panelLoss.SetActive(false);

        BukaKursor();

        // Tampilkan Modal UI Modern baru (3 Tombol: Main Menu, Ulang, Lanjut)
        ShowModernMissionEndModal(true, skorAkhir);
    }

    void CekKondisiKalah()
    {
        gameSelesai = true;

        SembunyikanTeksHUD();

        // Pastikan popup bawaan lama dimatikan
        if (panelWin != null) panelWin.SetActive(false);
        if (panelLoss != null) panelLoss.SetActive(false);

        BukaKursor();

        // Tampilkan Modal UI Modern baru (2 Tombol: Ulang, Main Menu)
        ShowModernMissionEndModal(false, HitungPersentasePadam());
    }

    private void ShowModernMissionEndModal(bool isWin, float score)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Cegah duplikasi modal
        Transform existing = canvas.transform.Find("ModernMissionEndModal");
        if (existing != null) Destroy(existing.gameObject);

        GameObject modal = new GameObject("ModernMissionEndModal", typeof(RectTransform));
        modal.transform.SetParent(canvas.transform, false);

        RectTransform rt = modal.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Background gelap transparan menutup seluruh layar
        UnityEngine.UI.Image bgImg = modal.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.05f, 0.08f, 0.12f, 0.88f);
        bgImg.raycastTarget = true; // Blokir input mouse ke scene di belakang

        // Kotak dialog tengah
        GameObject box = new GameObject("CenterBox", typeof(RectTransform));
        box.transform.SetParent(modal.transform, false);
        RectTransform boxRt = box.GetComponent<RectTransform>();
        boxRt.sizeDelta = isWin ? new Vector2(680f, 330f) : new Vector2(560f, 320f);
        UnityEngine.UI.Image boxImg = box.AddComponent<UnityEngine.UI.Image>();
        boxImg.color = new Color(0.10f, 0.14f, 0.20f, 0.98f);
        boxImg.raycastTarget = false;

        // Judul Modal
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(box.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, 95f);
        titleRt.sizeDelta = new Vector2(boxRt.sizeDelta.x - 40f, 55f);
        TMP_Text titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.fontSize = 32;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = isWin ? new Color(1f, 0.84f, 0.2f) : new Color(1f, 0.35f, 0.35f);
        titleTmp.text = isWin ? "MISI SELESAI!" : "MISI GAGAL";
        titleTmp.raycastTarget = false;

        // Deskripsi Hasil
        GameObject descObj = new GameObject("DescText", typeof(RectTransform));
        descObj.transform.SetParent(box.transform, false);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchoredPosition = new Vector2(0f, 25f);
        descRt.sizeDelta = new Vector2(boxRt.sizeDelta.x - 60f, 75f);
        TMP_Text descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.fontSize = 19;
        descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.color = new Color(0.9f, 0.92f, 0.95f);
        descTmp.text = isWin 
            ? $"Semua titik kebakaran di rumah berhasil dipadamkan!\nSkor Akhir: {Mathf.RoundToInt(score)}%"
            : $"Waktu habis sebelum api sempat dipadamkan.\nHasil: {Mathf.RoundToInt(score)}% api padam.";
        descTmp.raycastTarget = false;

        if (isWin)
        {
            // --- 3 TOMBOL KETIKA MENANG ---
            // 1. Main Menu
            CreateModalButton(box.transform, new Vector2(-215f, -80f), new Vector2(190f, 52f),
                "Main Menu", new Color(0.22f, 0.42f, 0.75f), () => TombolMainMenu());

            // 2. Ulang
            CreateModalButton(box.transform, new Vector2(0f, -80f), new Vector2(190f, 52f),
                "Ulang", new Color(0.85f, 0.45f, 0.15f), () => TombolRetry());

            // 3. Lanjut
            CreateModalButton(box.transform, new Vector2(215f, -80f), new Vector2(190f, 52f),
                "Lanjut ➔", new Color(0.16f, 0.68f, 0.32f), () => TombolLanjut());
        }
        else
        {
            // --- 2 TOMBOL KETIKA KALAH ---
            // 1. Ulang
            CreateModalButton(box.transform, new Vector2(-130f, -80f), new Vector2(230f, 52f),
                "Ulang", new Color(0.85f, 0.35f, 0.2f), () => TombolRetry());

            // 2. Main Menu
            CreateModalButton(box.transform, new Vector2(130f, -80f), new Vector2(230f, 52f),
                "Main Menu", new Color(0.22f, 0.42f, 0.75f), () => TombolMainMenu());
        }

        // Pastikan EventSystem aktif
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("RuntimeEventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private void CreateModalButton(Transform parent, Vector2 pos, Vector2 size, string text, Color color, System.Action onClick)
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
        cb.highlightedColor = color * 1.25f;
        cb.pressedColor = color * 0.8f;
        btn.colors = cb;
        btn.onClick.AddListener(() =>
        {
            PlayClickSound();
            onClick?.Invoke();
        });

        // Hover sound trigger
        UnityEngine.EventSystems.EventTrigger trigger = btnObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.text = text;
        tmp.raycastTarget = false;
    }

    // =========================
    // UI / CURSOR
    // =========================

    void BukaKursor()
    {
        Cursor.visible = true;

        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }

    void SembunyikanTeksHUD()
    {
        if (textTimer != null)
            textTimer.gameObject.SetActive(false);

        if (textMainObjective != null)
            textMainObjective.gameObject.SetActive(false);

        if (textOptionalObjective != null)
            textOptionalObjective.gameObject.SetActive(false);
    }

    // =========================
    // BUTTONS & PAUSE
    // =========================

    void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                LanjutkanGame();
            }
            else
            {
                JedaGame();
            }
        }
    }

    public void JedaGame()
    {
        if (gameSelesai) return;

        isPaused = true;
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (panelPause != null)
        {
            panelPause.SetActive(true);
        }
        else
        {
            BuatPanelPause();
        }
    }

    public void LanjutkanGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (panelPause != null)
        {
            panelPause.SetActive(false);
        }
    }

    private void BuatPanelPause()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject modal = new GameObject("PausePanel_Runtime", typeof(RectTransform));
        modal.transform.SetParent(canvas.transform, false);

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
        BuatTombolPause(box.transform, new Vector2(0f, -10f), new Vector2(360f, 48f),
            "Lanjutkan", new Color(0.18f, 0.65f, 0.35f), () => LanjutkanGame());

        // 2. Tombol Main Lagi
        BuatTombolPause(box.transform, new Vector2(0f, -65f), new Vector2(360f, 48f),
            "Main Lagi", new Color(0.85f, 0.45f, 0.15f), () => TombolRetry());

        // 3. Tombol Main Menu
        BuatTombolPause(box.transform, new Vector2(0f, -120f), new Vector2(360f, 48f),
            "Main Menu", new Color(0.22f, 0.42f, 0.75f), () => TombolMainMenu());

        panelPause = modal;
    }

    private void BuatTombolPause(Transform parent, Vector2 pos, Vector2 size, string text, Color color, System.Action onClick)
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
            PlayClickSound();
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

    private void BuatTombolPauseHUD()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        if (canvas.transform.Find("HUD_PauseBtn") != null) return;

        GameObject pauseBtnObj = new GameObject("HUD_PauseBtn", typeof(RectTransform));
        pauseBtnObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = pauseBtnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-25f, -25f);
        rt.sizeDelta = new Vector2(110f, 42f);

        UnityEngine.UI.Image img = pauseBtnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.12f, 0.16f, 0.24f, 0.85f);
        img.raycastTarget = true;

        UnityEngine.UI.Button btn = pauseBtnObj.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            PlayClickSound();
            JedaGame();
        });

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(pauseBtnObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 17;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.text = "⏸️ JEDA";
        tmp.raycastTarget = false;
    }

    public void TombolMainMenu()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        isPaused = false;
        gameSelesai = false;
        punyaSelang = false;
        punyaKunci = false;
        SceneManager.LoadScene("Main Menu");
    }

    public void TombolRetry()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        isPaused = false;
        gameSelesai = false;
        punyaSelang = false;
        punyaKunci = false;
        SceneManager.LoadScene("Map");
    }

    public void TombolLanjut()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene("ForestLevel");
    }

    // =========================
    // AUDIO UI
    // =========================

    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}