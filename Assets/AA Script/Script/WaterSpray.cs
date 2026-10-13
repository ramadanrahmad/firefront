using UnityEngine;
using System.Collections.Generic;

public class WaterSpray : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem waterSpray;

    // --- AUDIO AIR ---
    [Header("Audio Settings")]
    public AudioSource sfxSemprotan;
    public AudioClip suaraAir;

    [Header("Settings")]
    public float sprayDistance = 15f;
    public float damagePerSecond = 35f;
    public float sprayRadius = 0.6f;

    [Header("Feel")]
    public float aimSmoothSpeed = 10f;
    public float spreadAmount = 0.015f;
    public float gravityBias = 0.005f;

    private Vector3 currentDirection;
    private bool isSpraying;
    private readonly HashSet<Fire> damagedFiresThisFrame = new HashSet<Fire>();

    void Start()
    {
        if (Camera.main != null)
            currentDirection = Camera.main.transform.forward;

        if (waterSpray != null)
            waterSpray.Stop();

        // --- SETUP AUDIO SAAT GAME MULAI ---
        if (sfxSemprotan != null && suaraAir != null)
        {
            sfxSemprotan.clip = suaraAir;
            sfxSemprotan.loop = true;
            sfxSemprotan.Stop();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (isSpraying)
            {
                isSpraying = false;
                if (waterSpray != null && waterSpray.isPlaying) waterSpray.Stop();
                if (sfxSemprotan != null && sfxSemprotan.isPlaying) sfxSemprotan.Stop();
            }
            return;
        }

        HandleInput();

        if (!isSpraying)
            return;

        HandleSpray();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isSpraying = true;
            if (waterSpray != null) waterSpray.Play();

            if (sfxSemprotan != null && !sfxSemprotan.isPlaying)
            {
                sfxSemprotan.Play();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSpraying = false;
            if (waterSpray != null) waterSpray.Stop();

            if (sfxSemprotan != null && sfxSemprotan.isPlaying)
            {
                sfxSemprotan.Stop();
            }
        }
    }

    void HandleSpray()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. Target arah dari kamera
        Vector3 targetDirection = cam.transform.forward;

        // 2. Inersia semprotan selang pemadam
        currentDirection = Vector3.Slerp(
            currentDirection,
            targetDirection,
            aimSmoothSpeed * Time.deltaTime
        );

        // 3. Tambahkan sedikit dinamika aliran air
        Vector3 finalDirection = currentDirection;
        if (spreadAmount > 0f) finalDirection += Random.insideUnitSphere * spreadAmount;
        if (gravityBias > 0f) finalDirection += Vector3.down * gravityBias;

        Ray ray = new Ray(cam.transform.position, finalDirection.normalized);

        Debug.DrawRay(ray.origin, ray.direction * sprayDistance, Color.cyan);

        damagedFiresThisFrame.Clear();

        // Pastikan damage minimum 35f/s agar pemadaman api terasa responsif dan adil
        float effectiveDps = Mathf.Max(damagePerSecond, 35f);

        // 4. Deteksi semprotan air bervolume (SphereCastAll menembus rintangan sofa/meja dan mengenai Trigger collider api)
        RaycastHit[] hits = Physics.SphereCastAll(ray, sprayRadius, sprayDistance, ~0, QueryTriggerInteraction.Collide);

        foreach (var hit in hits)
        {
            Fire fire = hit.collider.GetComponentInParent<Fire>();
            if (fire != null && !damagedFiresThisFrame.Contains(fire))
            {
                damagedFiresThisFrame.Add(fire);

                // Distance falloff yang wajar (tidak terlalu drastis)
                float distanceFactor = 1f - (hit.distance / sprayDistance) * 0.4f;
                distanceFactor = Mathf.Clamp(distanceFactor, 0.6f, 1f);

                fire.DamageFire(effectiveDps * distanceFactor * Time.deltaTime);
            }
        }

        // 5. Area bantuan jarak dekat (jika pemain berdiri tepat di samping/depan api)
        Collider[] closeColliders = Physics.OverlapSphere(cam.transform.position + cam.transform.forward * 1.8f, 1.2f, ~0, QueryTriggerInteraction.Collide);
        foreach (var col in closeColliders)
        {
            Fire fire = col.GetComponentInParent<Fire>();
            if (fire != null && !damagedFiresThisFrame.Contains(fire))
            {
                damagedFiresThisFrame.Add(fire);
                fire.DamageFire(effectiveDps * Time.deltaTime);
            }
        }
    }
}