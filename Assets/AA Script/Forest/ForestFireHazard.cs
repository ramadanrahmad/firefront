using UnityEngine;

namespace Firesomething.Forest
{
    public class ForestFireHazard : MonoBehaviour
    {
        [Header("Pengaturan Api")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public float heatDamageRadius = 5.5f;
        public bool isExtinguished = false;

        [Header("Pencahayaan Api")]
        public Light fireLight;
        private float baseLightIntensity = 4.8f;

        [Header("Audio Api 3D")]
        public AudioSource fireAudioSource;
        public AudioClip fireLoopClip;

        // Particle systems referensi internal
        private ParticleSystem[] allParticleSystems;
        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;

            // Kumpulkan semua ParticleSystem di dalam prefab api (WildFire)
            allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

            // Pastikan semua partikel api aktif dan mulai bermain
            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    main.playOnAwake = true;
                    main.loop = true;
                    if (!ps.isPlaying) ps.Play();
                }
            }

            // Setup Collider Trigger untuk menerima tembakan semprotan air
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0, 1.8f, 0);
            col.size = new Vector3(3.5f, 3.8f, 3.5f);

            // Setup Lampu Kobaran Api (Point Light)
            if (fireLight == null)
            {
                GameObject lightObj = new GameObject("FlamePointLight");
                lightObj.transform.SetParent(transform, false);
                lightObj.transform.localPosition = new Vector3(0, 1.8f, 0);

                fireLight = lightObj.AddComponent<Light>();
                fireLight.type = LightType.Point;
                fireLight.color = new Color(1.0f, 0.45f, 0.1f);
                fireLight.range = 22f;
                fireLight.intensity = baseLightIntensity;
                fireLight.shadows = LightShadows.None;
            }

            // Setup Audio Api 3D
            if (fireAudioSource == null)
            {
                fireAudioSource = gameObject.AddComponent<AudioSource>();
                fireAudioSource.spatialBlend = 1.0f;
                fireAudioSource.minDistance = 3f;
                fireAudioSource.maxDistance = 35f;
                fireAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                fireAudioSource.loop = true;
                fireAudioSource.playOnAwake = false;
            }

            if (fireLoopClip != null)
            {
                fireAudioSource.clip = fireLoopClip;
                fireAudioSource.volume = 0.9f;
                fireAudioSource.Play();
            }
        }

        private void Start()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (isExtinguished) return;

            // Efek kedip cahaya api alami (Natural flickering light)
            if (fireLight != null)
            {
                float flicker = Mathf.PerlinNoise(Time.time * 8.5f, transform.position.x * 3f);
                fireLight.intensity = baseLightIntensity * (0.8f + flicker * 0.4f);
            }

            // Cek bahaya panas pada pemain jika terlalu dekat
            CheckPlayerHeatProximity();
        }

        public void ExtinguishWithWater(float damage)
        {
            if (isExtinguished) return;

            currentHealth -= damage;
            if (currentHealth < 0f) currentHealth = 0f;

            // Mengecilkan partikel api seiring semprotan air
            float scalePercent = Mathf.Clamp01(currentHealth / maxHealth);
            transform.localScale = initialScale * (0.3f + 0.7f * scalePercent);

            if (fireLight != null)
            {
                baseLightIntensity = 4.8f * scalePercent;
            }

            if (currentHealth <= 0f)
            {
                MatikanApi();
            }
        }

        public void MatikanApi()
        {
            if (isExtinguished) return;
            isExtinguished = true;

            // Hentikan emisi partikel api secara halus (agar partikel yang sedang melayang hilang natural)
            if (allParticleSystems != null)
            {
                foreach (var ps in allParticleSystems)
                {
                    if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            // Matikan cahaya
            if (fireLight != null)
            {
                fireLight.enabled = false;
            }

            // Hentikan suara api
            if (fireAudioSource != null && fireAudioSource.isPlaying)
            {
                fireAudioSource.Stop();
            }

            // Matikan collider agar tidak menerima tembakan lagi
            Collider c = GetComponent<Collider>();
            if (c != null) c.enabled = false;

            // Catat progres ke Level Manager
            if (ForestLevelManager.Instance != null)
            {
                ForestLevelManager.Instance.CatatApiPadam(gameObject);
            }

            Debug.Log($"🔥 [ForestFireHazard] Api pada {gameObject.name} berhasil dipadamkan!");
        }

        private void CheckPlayerHeatProximity()
        {
            ForestPlayerController player = FindObjectOfType<ForestPlayerController>();
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= heatDamageRadius)
                {
                    player.SetInFireHazard(true);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, heatDamageRadius);
        }
    }
}
