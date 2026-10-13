using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }

    [Header("Audio SFX")]
    public AudioSource sfxSource;
    public AudioClip clickClip;
    public AudioClip hoverClip;

    [Header("Level 2 State")]
    public bool isForestUnlocked = false;

    private GameObject level2CardObj;
    private Button level2Button;
    private TMP_Text level2ButtonText;
    private TMP_Text level2StatusBadgeText;
    private Image level2StatusBadgeImg;
    private GameObject level2LockOverlay;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Baca status unlock dari PlayerPrefs (1 = terbuka, 0 = terkunci)
        isForestUnlocked = PlayerPrefs.GetInt("ForestLevelUnlocked", 0) == 1;

        InitAudio();
        BuildUIIfNeeded();
        RefreshLevel2UI();
    }

    private void Update()
    {
        // Tombol ESC untuk kembali ke Main Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            KembaliKeMainMenu();
        }

        // Cheat shortcut (Tekan U untuk toggle unlock Forest Level saat testing)
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleCheatUnlock();
        }
    }

    private void InitAudio()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (clickClip == null)
        {
            clickClip = Resources.Load<AudioClip>("Audio/Click");
        }
        if (hoverClip == null)
        {
            hoverClip = Resources.Load<AudioClip>("Audio/Hover");
        }
    }

    public void PlayHouseLevel()
    {
        PlayClickSfx();
        Debug.Log("🏠 [LevelSelect] Memulai Misi 1: Rumah Terbakar (Map)...");
        SceneManager.LoadScene("Map");
    }

    public void PlayForestLevel()
    {
        PlayClickSfx();
        if (!isForestUnlocked)
        {
            Debug.LogWarning("🔒 [LevelSelect] Misi 2 (Hutan) masih terkunci! Selesaikan Misi 1 terlebih dahulu.");
            return;
        }

        Debug.Log("🌲 [LevelSelect] Memulai Misi 2: Kebakaran Hutan (ForestLevel)...");
        SceneManager.LoadScene("ForestLevel");
    }

    public void KembaliKeMainMenu()
    {
        PlayClickSfx();
        Debug.Log("⬅️ [LevelSelect] Kembali ke Menu Utama...");
        SceneManager.LoadScene("Main Menu");
    }

    public void ToggleCheatUnlock()
    {
        isForestUnlocked = !isForestUnlocked;
        PlayerPrefs.SetInt("ForestLevelUnlocked", isForestUnlocked ? 1 : 0);
        PlayerPrefs.Save();
        PlayClickSfx();
        Debug.Log($"🔑 [LevelSelect Cheat] Status Hutan sekarang: {(isForestUnlocked ? "TERBUKA" : "TERKUNCI")}");
        RefreshLevel2UI();
    }

    public void PlayClickSfx()
    {
        if (sfxSource != null && clickClip != null)
        {
            sfxSource.PlayOneShot(clickClip);
        }
    }

    public void PlayHoverSfx()
    {
        if (sfxSource != null && hoverClip != null)
        {
            sfxSource.PlayOneShot(hoverClip);
        }
    }

    // ==========================================
    // PROCEDURAL UI GENERATION
    // ==========================================

    private void BuildUIIfNeeded()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("LevelSelectCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Pastikan EventSystem tersedia
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Cek jika sudah dibangun
        if (canvas.transform.Find("LevelSelect_Root") != null)
        {
            return;
        }

        GameObject root = new GameObject("LevelSelect_Root", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // 1. Background Gelap Tematik Pemadam Kebakaran
        GameObject bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.08f, 0.12f, 1f); // Deep firefighter midnight slate
        bgImg.raycastTarget = false;

        // 2. Header (Judul & Subjudul)
        GameObject headerObj = new GameObject("Header", typeof(RectTransform));
        headerObj.transform.SetParent(root.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.5f, 1f);
        headerRt.anchorMax = new Vector2(0.5f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0f, -40f);
        headerRt.sizeDelta = new Vector2(1000f, 120f);

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, -10f);
        titleRt.sizeDelta = new Vector2(900f, 60f);
        TMP_Text titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.fontSize = 46;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(1f, 0.76f, 0.25f); // Emas menyala
        titleTmp.text = "PILIH MISI PEMADAM KEBAKARAN";
        titleTmp.raycastTarget = false;

        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform));
        subObj.transform.SetParent(headerObj.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0f, -65f);
        subRt.sizeDelta = new Vector2(900f, 40f);
        TMP_Text subTmp = subObj.AddComponent<TextMeshProUGUI>();
        subTmp.fontSize = 20;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(0.75f, 0.82f, 0.90f);
        subTmp.text = "Pilih lokasi darurat untuk memulai operasi pemadaman!";
        subTmp.raycastTarget = false;

        // 3. Container Kartu Level (2 Kartu Bersebelahan)
        Sprite houseSprite = LoadSpriteResourceOrFile("Level_House");
        Sprite forestSprite = LoadSpriteResourceOrFile("Level_Forest");

        // Kartu Level 1: Rumah Terbakar
        BuildLevelCard(root.transform,
            new Vector2(-330f, -25f),
            "MISI 01",
            "PEMUKIMAN WARGA",
            "Rumah Tinggal Berlantai Dua",
            "Padamkan kobaran api yang melalap rumah warga sebelum struktur bangunan runtuh!",
            "🔥 Bahaya: Sedang  |  ⏱️ Waktu: 5 Menit",
            houseSprite,
            true,
            PlayHouseLevel);

        // Kartu Level 2: Kebakaran Hutan
        BuildLevel2Card(root.transform,
            new Vector2(330f, -25f),
            "MISI 02",
            "HUTAN TROPIS",
            "Kebakaran Hutan Hebat",
            "Padamkan titik api yang membakar pepohonan sebelum hutan hangus menjadi abu!",
            "🔥 Bahaya: EKSTREM  |  ⏱️ Waktu: 4 Menit",
            forestSprite,
            PlayForestLevel);

        // 4. Tombol Kembali di Bawah Kiri
        GameObject backBtnObj = new GameObject("Btn_Kembali", typeof(RectTransform));
        backBtnObj.transform.SetParent(root.transform, false);
        RectTransform backRt = backBtnObj.GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0f, 0f);
        backRt.anchorMax = new Vector2(0f, 0f);
        backRt.pivot = new Vector2(0f, 0f);
        backRt.anchoredPosition = new Vector2(70f, 50f);
        backRt.sizeDelta = new Vector2(250f, 54f);

        Image backImg = backBtnObj.AddComponent<Image>();
        backImg.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);
        backImg.raycastTarget = true;

        Button backBtn = backBtnObj.AddComponent<Button>();
        backBtn.targetGraphic = backImg;
        ColorBlock cbb = backBtn.colors;
        cbb.normalColor = new Color(0.18f, 0.22f, 0.30f, 0.95f);
        cbb.highlightedColor = new Color(0.28f, 0.35f, 0.48f, 1f);
        cbb.pressedColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        backBtn.colors = cbb;
        backBtn.onClick.AddListener(KembaliKeMainMenu);
        AttachHoverSfx(backBtnObj);

        GameObject backTxtObj = new GameObject("Text", typeof(RectTransform));
        backTxtObj.transform.SetParent(backBtnObj.transform, false);
        RectTransform backTxtRt = backTxtObj.GetComponent<RectTransform>();
        backTxtRt.anchorMin = Vector2.zero;
        backTxtRt.anchorMax = Vector2.one;
        backTxtRt.offsetMin = Vector2.zero;
        backTxtRt.offsetMax = Vector2.zero;
        TMP_Text backTmp = backTxtObj.AddComponent<TextMeshProUGUI>();
        backTmp.fontSize = 20;
        backTmp.fontStyle = FontStyles.Bold;
        backTmp.alignment = TextAlignmentOptions.Center;
        backTmp.color = Color.white;
        backTmp.text = "⬅️ KEMBALI KE MENU";
        backTmp.raycastTarget = false;

        // 5. Hint Tekan U (Cheat/Tester) di Bawah Kanan
        GameObject hintObj = new GameObject("CheatHint", typeof(RectTransform));
        hintObj.transform.SetParent(root.transform, false);
        RectTransform hintRt = hintObj.GetComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(1f, 0f);
        hintRt.anchorMax = new Vector2(1f, 0f);
        hintRt.pivot = new Vector2(1f, 0f);
        hintRt.anchoredPosition = new Vector2(-70f, 55f);
        hintRt.sizeDelta = new Vector2(400f, 40f);
        TMP_Text hintTmp = hintObj.AddComponent<TextMeshProUGUI>();
        hintTmp.fontSize = 15;
        hintTmp.alignment = TextAlignmentOptions.Right;
        hintTmp.color = new Color(0.5f, 0.55f, 0.65f, 0.7f);
        hintTmp.text = "[Tips: Tekan 'U' di keyboard untuk uji coba Kunci / Buka Level Hutan]";
        hintTmp.raycastTarget = false;
    }

    private void BuildLevelCard(Transform parent, Vector2 pos, string tag, string title, string subtitle, string desc, string stats, Sprite thumb, bool isUnlocked, System.Action onPlay)
    {
        GameObject card = new GameObject("Card_" + title, typeof(RectTransform));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.anchoredPosition = pos;
        cardRt.sizeDelta = new Vector2(560f, 660f);

        // Background Kartu
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.10f, 0.14f, 0.20f, 0.95f);
        cardImg.raycastTarget = false;

        // Badge Nomor Misi
        GameObject badgeObj = new GameObject("Badge", typeof(RectTransform));
        badgeObj.transform.SetParent(card.transform, false);
        RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
        badgeRt.anchoredPosition = new Vector2(-160f, 300f);
        badgeRt.sizeDelta = new Vector2(170f, 36f);
        Image badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.color = new Color(0.95f, 0.45f, 0.15f); // Orange ember
        badgeImg.raycastTarget = false;

        GameObject badgeTxtObj = new GameObject("Text", typeof(RectTransform));
        badgeTxtObj.transform.SetParent(badgeObj.transform, false);
        RectTransform badgeTxtRt = badgeTxtObj.GetComponent<RectTransform>();
        badgeTxtRt.anchorMin = Vector2.zero;
        badgeTxtRt.anchorMax = Vector2.one;
        badgeTxtRt.offsetMin = Vector2.zero;
        badgeTxtRt.offsetMax = Vector2.zero;
        TMP_Text badgeTmp = badgeTxtObj.AddComponent<TextMeshProUGUI>();
        badgeTmp.fontSize = 17;
        badgeTmp.fontStyle = FontStyles.Bold;
        badgeTmp.alignment = TextAlignmentOptions.Center;
        badgeTmp.color = Color.white;
        badgeTmp.text = tag;
        badgeTmp.raycastTarget = false;

        // Status Badge (Terbuka)
        GameObject statusBadgeObj = new GameObject("StatusBadge", typeof(RectTransform));
        statusBadgeObj.transform.SetParent(card.transform, false);
        RectTransform sbRt = statusBadgeObj.GetComponent<RectTransform>();
        sbRt.anchoredPosition = new Vector2(175f, 300f);
        sbRt.sizeDelta = new Vector2(140f, 36f);
        Image sbImg = statusBadgeObj.AddComponent<Image>();
        sbImg.color = new Color(0.12f, 0.65f, 0.32f); // Hijau aktif
        sbImg.raycastTarget = false;

        GameObject sbTxtObj = new GameObject("Text", typeof(RectTransform));
        sbTxtObj.transform.SetParent(statusBadgeObj.transform, false);
        RectTransform sbTxtRt = sbTxtObj.GetComponent<RectTransform>();
        sbTxtRt.anchorMin = Vector2.zero;
        sbTxtRt.anchorMax = Vector2.one;
        sbTxtRt.offsetMin = Vector2.zero;
        sbTxtRt.offsetMax = Vector2.zero;
        TMP_Text sbTmp = sbTxtObj.AddComponent<TextMeshProUGUI>();
        sbTmp.fontSize = 16;
        sbTmp.fontStyle = FontStyles.Bold;
        sbTmp.alignment = TextAlignmentOptions.Center;
        sbTmp.color = Color.white;
        sbTmp.text = "TERBUKA";
        sbTmp.raycastTarget = false;

        // Thumbnail Gambar Level
        GameObject thumbObj = new GameObject("Thumbnail", typeof(RectTransform));
        thumbObj.transform.SetParent(card.transform, false);
        RectTransform thumbRt = thumbObj.GetComponent<RectTransform>();
        thumbRt.anchoredPosition = new Vector2(0f, 130f);
        thumbRt.sizeDelta = new Vector2(500f, 260f);
        Image tImg = thumbObj.AddComponent<Image>();
        if (thumb != null)
        {
            tImg.sprite = thumb;
            tImg.color = Color.white;
        }
        else
        {
            tImg.color = new Color(0.2f, 0.25f, 0.35f);
        }
        tImg.raycastTarget = false;

        // Judul Misi
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(card.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(500f, 40f);
        TMP_Text tTmp = titleObj.AddComponent<TextMeshProUGUI>();
        tTmp.fontSize = 26;
        tTmp.fontStyle = FontStyles.Bold;
        tTmp.alignment = TextAlignmentOptions.Left;
        tTmp.color = Color.white;
        tTmp.text = title;
        tTmp.raycastTarget = false;

        // Subjudul
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform));
        subObj.transform.SetParent(card.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0f, -62f);
        subRt.sizeDelta = new Vector2(500f, 28f);
        TMP_Text stTmp = subObj.AddComponent<TextMeshProUGUI>();
        stTmp.fontSize = 17;
        stTmp.fontStyle = FontStyles.Italic;
        stTmp.alignment = TextAlignmentOptions.Left;
        stTmp.color = new Color(1f, 0.65f, 0.25f);
        stTmp.text = subtitle;
        stTmp.raycastTarget = false;

        // Deskripsi Misi
        GameObject descObj = new GameObject("Desc", typeof(RectTransform));
        descObj.transform.SetParent(card.transform, false);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchoredPosition = new Vector2(0f, -115f);
        descRt.sizeDelta = new Vector2(500f, 65f);
        TMP_Text dTmp = descObj.AddComponent<TextMeshProUGUI>();
        dTmp.fontSize = 16;
        dTmp.alignment = TextAlignmentOptions.Left;
        dTmp.color = new Color(0.80f, 0.85f, 0.90f);
        dTmp.text = desc;
        dTmp.raycastTarget = false;

        // Statistik / Info
        GameObject statsObj = new GameObject("Stats", typeof(RectTransform));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform statsRt = statsObj.GetComponent<RectTransform>();
        statsRt.anchoredPosition = new Vector2(0f, -165f);
        statsRt.sizeDelta = new Vector2(500f, 30f);
        TMP_Text statTmp = statsObj.AddComponent<TextMeshProUGUI>();
        statTmp.fontSize = 15;
        statTmp.alignment = TextAlignmentOptions.Left;
        statTmp.color = new Color(0.95f, 0.75f, 0.35f);
        statTmp.text = stats;
        statTmp.raycastTarget = false;

        // Tombol Mainkan
        GameObject btnObj = new GameObject("PlayBtn", typeof(RectTransform));
        btnObj.transform.SetParent(card.transform, false);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchoredPosition = new Vector2(0f, -235f);
        btnRt.sizeDelta = new Vector2(500f, 58f);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.30f, 0.15f); // Warna api membara
        btnImg.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.85f, 0.30f, 0.15f);
        cb.highlightedColor = new Color(1f, 0.45f, 0.25f);
        cb.pressedColor = new Color(0.65f, 0.20f, 0.08f);
        btn.colors = cb;
        btn.onClick.AddListener(() => onPlay?.Invoke());
        AttachHoverSfx(btnObj);

        GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero;
        btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero;
        btnTxtRt.offsetMax = Vector2.zero;
        TMP_Text btnTmp = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTmp.fontSize = 22;
        btnTmp.fontStyle = FontStyles.Bold;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.white;
        btnTmp.text = "MAINKAN SEKARANG ▶";
        btnTmp.raycastTarget = false;
    }

    private void BuildLevel2Card(Transform parent, Vector2 pos, string tag, string title, string subtitle, string desc, string stats, Sprite thumb, System.Action onPlay)
    {
        level2CardObj = new GameObject("Card_" + title, typeof(RectTransform));
        level2CardObj.transform.SetParent(parent, false);
        RectTransform cardRt = level2CardObj.GetComponent<RectTransform>();
        cardRt.anchoredPosition = pos;
        cardRt.sizeDelta = new Vector2(560f, 660f);

        Image cardImg = level2CardObj.AddComponent<Image>();
        cardImg.color = new Color(0.10f, 0.14f, 0.20f, 0.95f);
        cardImg.raycastTarget = false;

        // Badge Nomor Misi
        GameObject badgeObj = new GameObject("Badge", typeof(RectTransform));
        badgeObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
        badgeRt.anchoredPosition = new Vector2(-160f, 300f);
        badgeRt.sizeDelta = new Vector2(170f, 36f);
        Image badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.color = new Color(0.18f, 0.55f, 0.35f);
        badgeImg.raycastTarget = false;

        GameObject badgeTxtObj = new GameObject("Text", typeof(RectTransform));
        badgeTxtObj.transform.SetParent(badgeObj.transform, false);
        RectTransform badgeTxtRt = badgeTxtObj.GetComponent<RectTransform>();
        badgeTxtRt.anchorMin = Vector2.zero;
        badgeTxtRt.anchorMax = Vector2.one;
        badgeTxtRt.offsetMin = Vector2.zero;
        badgeTxtRt.offsetMax = Vector2.zero;
        TMP_Text badgeTmp = badgeTxtObj.AddComponent<TextMeshProUGUI>();
        badgeTmp.fontSize = 17;
        badgeTmp.fontStyle = FontStyles.Bold;
        badgeTmp.alignment = TextAlignmentOptions.Center;
        badgeTmp.color = Color.white;
        badgeTmp.text = tag;
        badgeTmp.raycastTarget = false;

        // Status Badge (Akan diubah sesuai status lock)
        GameObject statusBadgeObj = new GameObject("StatusBadge", typeof(RectTransform));
        statusBadgeObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform sbRt = statusBadgeObj.GetComponent<RectTransform>();
        sbRt.anchoredPosition = new Vector2(175f, 300f);
        sbRt.sizeDelta = new Vector2(140f, 36f);
        level2StatusBadgeImg = statusBadgeObj.AddComponent<Image>();
        level2StatusBadgeImg.raycastTarget = false;

        GameObject sbTxtObj = new GameObject("Text", typeof(RectTransform));
        sbTxtObj.transform.SetParent(statusBadgeObj.transform, false);
        RectTransform sbTxtRt = sbTxtObj.GetComponent<RectTransform>();
        sbTxtRt.anchorMin = Vector2.zero;
        sbTxtRt.anchorMax = Vector2.one;
        sbTxtRt.offsetMin = Vector2.zero;
        sbTxtRt.offsetMax = Vector2.zero;
        level2StatusBadgeText = sbTxtObj.AddComponent<TextMeshProUGUI>();
        level2StatusBadgeText.fontSize = 16;
        level2StatusBadgeText.fontStyle = FontStyles.Bold;
        level2StatusBadgeText.alignment = TextAlignmentOptions.Center;
        level2StatusBadgeText.color = Color.white;
        level2StatusBadgeText.raycastTarget = false;

        // Thumbnail
        GameObject thumbObj = new GameObject("Thumbnail", typeof(RectTransform));
        thumbObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform thumbRt = thumbObj.GetComponent<RectTransform>();
        thumbRt.anchoredPosition = new Vector2(0f, 130f);
        thumbRt.sizeDelta = new Vector2(500f, 260f);
        Image tImg = thumbObj.AddComponent<Image>();
        if (thumb != null)
        {
            tImg.sprite = thumb;
            tImg.color = Color.white;
        }
        else
        {
            tImg.color = new Color(0.2f, 0.25f, 0.35f);
        }
        tImg.raycastTarget = false;

        // Lock Overlay di atas Thumbnail
        level2LockOverlay = new GameObject("LockOverlay", typeof(RectTransform));
        level2LockOverlay.transform.SetParent(thumbObj.transform, false);
        RectTransform lockRt = level2LockOverlay.GetComponent<RectTransform>();
        lockRt.anchorMin = Vector2.zero;
        lockRt.anchorMax = Vector2.one;
        lockRt.offsetMin = Vector2.zero;
        lockRt.offsetMax = Vector2.zero;
        Image lockBg = level2LockOverlay.AddComponent<Image>();
        lockBg.color = new Color(0.04f, 0.06f, 0.08f, 0.75f);
        lockBg.raycastTarget = false;

        GameObject lockIconObj = new GameObject("LockIcon", typeof(RectTransform));
        lockIconObj.transform.SetParent(level2LockOverlay.transform, false);
        RectTransform lockIconRt = lockIconObj.GetComponent<RectTransform>();
        lockIconRt.anchorMin = Vector2.zero;
        lockIconRt.anchorMax = Vector2.one;
        lockIconRt.offsetMin = Vector2.zero;
        lockIconRt.offsetMax = Vector2.zero;
        TMP_Text lockIconTmp = lockIconObj.AddComponent<TextMeshProUGUI>();
        lockIconTmp.fontSize = 54;
        lockIconTmp.alignment = TextAlignmentOptions.Center;
        lockIconTmp.color = new Color(1f, 0.85f, 0.4f);
        lockIconTmp.text = "🔒\n<size=20><color=#e2e8f0>Selesaikan Misi 1 Dahulu</color></size>";
        lockIconTmp.raycastTarget = false;

        // Judul Misi
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(500f, 40f);
        TMP_Text tTmp = titleObj.AddComponent<TextMeshProUGUI>();
        tTmp.fontSize = 26;
        tTmp.fontStyle = FontStyles.Bold;
        tTmp.alignment = TextAlignmentOptions.Left;
        tTmp.color = Color.white;
        tTmp.text = title;
        tTmp.raycastTarget = false;

        // Subjudul
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform));
        subObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0f, -62f);
        subRt.sizeDelta = new Vector2(500f, 28f);
        TMP_Text stTmp = subObj.AddComponent<TextMeshProUGUI>();
        stTmp.fontSize = 17;
        stTmp.fontStyle = FontStyles.Italic;
        stTmp.alignment = TextAlignmentOptions.Left;
        stTmp.color = new Color(0.35f, 0.85f, 0.55f);
        stTmp.text = subtitle;
        stTmp.raycastTarget = false;

        // Deskripsi Misi
        GameObject descObj = new GameObject("Desc", typeof(RectTransform));
        descObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchoredPosition = new Vector2(0f, -115f);
        descRt.sizeDelta = new Vector2(500f, 65f);
        TMP_Text dTmp = descObj.AddComponent<TextMeshProUGUI>();
        dTmp.fontSize = 16;
        dTmp.alignment = TextAlignmentOptions.Left;
        dTmp.color = new Color(0.80f, 0.85f, 0.90f);
        dTmp.text = desc;
        dTmp.raycastTarget = false;

        // Statistik / Info
        GameObject statsObj = new GameObject("Stats", typeof(RectTransform));
        statsObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform statsRt = statsObj.GetComponent<RectTransform>();
        statsRt.anchoredPosition = new Vector2(0f, -165f);
        statsRt.sizeDelta = new Vector2(500f, 30f);
        TMP_Text statTmp = statsObj.AddComponent<TextMeshProUGUI>();
        statTmp.fontSize = 15;
        statTmp.alignment = TextAlignmentOptions.Left;
        statTmp.color = new Color(0.95f, 0.75f, 0.35f);
        statTmp.text = stats;
        statTmp.raycastTarget = false;

        // Tombol Mainkan
        GameObject btnObj = new GameObject("PlayBtn", typeof(RectTransform));
        btnObj.transform.SetParent(level2CardObj.transform, false);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchoredPosition = new Vector2(0f, -235f);
        btnRt.sizeDelta = new Vector2(500f, 58f);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.raycastTarget = true;

        level2Button = btnObj.AddComponent<Button>();
        level2Button.targetGraphic = btnImg;
        level2Button.onClick.AddListener(() => onPlay?.Invoke());
        AttachHoverSfx(btnObj);

        GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero;
        btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero;
        btnTxtRt.offsetMax = Vector2.zero;
        level2ButtonText = btnTxtObj.AddComponent<TextMeshProUGUI>();
        level2ButtonText.fontSize = 22;
        level2ButtonText.fontStyle = FontStyles.Bold;
        level2ButtonText.alignment = TextAlignmentOptions.Center;
        level2ButtonText.raycastTarget = false;
    }

    private void RefreshLevel2UI()
    {
        if (level2Button == null) return;

        if (isForestUnlocked)
        {
            // STATUS: TERBUKA
            if (level2LockOverlay != null) level2LockOverlay.SetActive(false);

            if (level2StatusBadgeImg != null)
                level2StatusBadgeImg.color = new Color(0.12f, 0.65f, 0.32f);

            if (level2StatusBadgeText != null)
                level2StatusBadgeText.text = "TERBUKA";

            level2Button.interactable = true;
            Image btnImg = level2Button.GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.15f, 0.65f, 0.35f);

            ColorBlock cb = level2Button.colors;
            cb.normalColor = new Color(0.15f, 0.65f, 0.35f);
            cb.highlightedColor = new Color(0.22f, 0.85f, 0.45f);
            cb.pressedColor = new Color(0.10f, 0.45f, 0.22f);
            level2Button.colors = cb;

            if (level2ButtonText != null)
            {
                level2ButtonText.color = Color.white;
                level2ButtonText.text = "MAINKAN SEKARANG ▶";
            }
        }
        else
        {
            // STATUS: TERKUNCI
            if (level2LockOverlay != null) level2LockOverlay.SetActive(true);

            if (level2StatusBadgeImg != null)
                level2StatusBadgeImg.color = new Color(0.65f, 0.20f, 0.20f);

            if (level2StatusBadgeText != null)
                level2StatusBadgeText.text = "TERKUNCI";

            level2Button.interactable = false;
            Image btnImg = level2Button.GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.25f, 0.28f, 0.35f);

            ColorBlock cb = level2Button.colors;
            cb.normalColor = new Color(0.25f, 0.28f, 0.35f);
            cb.disabledColor = new Color(0.25f, 0.28f, 0.35f, 0.8f);
            level2Button.colors = cb;

            if (level2ButtonText != null)
            {
                level2ButtonText.color = new Color(0.6f, 0.65f, 0.72f);
                level2ButtonText.text = "🔒 TERKUNCI";
            }
        }
    }

    private void AttachHoverSfx(GameObject obj)
    {
        UnityEngine.EventSystems.EventTrigger trigger = obj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSfx(); });
        trigger.triggers.Add(entry);
    }

    private Sprite LoadSpriteResourceOrFile(string resourceName)
    {
        Sprite sp = Resources.Load<Sprite>(resourceName);
        if (sp != null) return sp;

        string path = Path.Combine(Application.dataPath, "Resources", resourceName + ".jpg");
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        return null;
    }
}
