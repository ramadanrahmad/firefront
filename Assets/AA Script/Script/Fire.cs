using System;
using System.Collections;
using UnityEngine;

public class Fire : MonoBehaviour
{
    // EVENT → memberi tahu GameplayManager kalau api padam
    public static event Action<Fire> OnFireExtinguished;

    [Header("Fire Objects")]
    public GameObject fireObject;
    public GameObject embers;

    [Header("Effects")]
    public GameObject smokeEffect;
    public GameObject steamEffect;

    [Header("Settings")]
    public float maxHealth = 100f;
    public float burnSpeed = 10f;

    private float fireHealth;
    private bool fireDead = false;

    private Vector3 initialScale;

    void Start()
    {
        fireHealth = maxHealth;

        // simpan scale awal api
        if (fireObject != null)
        {
            initialScale = fireObject.transform.localScale;
            fireObject.SetActive(true);
        }

        if (embers != null)
            embers.SetActive(true);

        // matikan smoke awal
        if (smokeEffect != null)
        {
            smokeEffect.SetActive(false);

            ParticleSystem smokePS = smokeEffect.GetComponent<ParticleSystem>();

            if (smokePS != null)
            {
                smokePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // matikan steam awal
        if (steamEffect != null)
        {
            steamEffect.SetActive(false);

            ParticleSystem steamPS = steamEffect.GetComponent<ParticleSystem>();

            if (steamPS != null)
            {
                steamPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    public void DamageFire(float damage)
    {
        if (fireDead)
            return;

        fireHealth -= damage;

        fireHealth = Mathf.Clamp(fireHealth, 0f, maxHealth);

        // scale api mengecil
        float scalePercent = fireHealth / maxHealth;

        if (fireObject != null)
        {
            fireObject.transform.localScale = initialScale * scalePercent;
        }

        Debug.Log("🔥 FIRE HP: " + Mathf.RoundToInt(fireHealth));

        // api padam
        if (fireHealth <= 0f && !fireDead)
        {
            fireDead = true;

            // kirim sinyal ke GameplayManager
            OnFireExtinguished?.Invoke(this);

            FireOut();
        }
    }

    private void FireOut()
    {
        // matikan api utama
        if (fireObject != null)
            fireObject.SetActive(false);

        // matikan ember
        if (embers != null)
            embers.SetActive(false);

        // pastikan efek asap dan uap tetap mati total agar laptop tidak lag
        if (smokeEffect != null)
            smokeEffect.SetActive(false);

        if (steamEffect != null)
            steamEffect.SetActive(false);

        // matikan collider agar tidak membebani physics
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("💨 FIRE EXTINGUISHED (Efek asap dimatikan untuk performa)");
    }
}