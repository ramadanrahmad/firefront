using UnityEngine;

namespace Firesomething.Forest
{
    [RequireComponent(typeof(CharacterController))]
    public class ForestPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 8.5f;
        public float jumpHeight = 1.6f;
        public float gravity = -20f;

        [Header("Camera & Look")]
        public Transform playerCamera;
        public float mouseSensitivity = 2.5f;
        public float minPitch = -85f;
        public float maxPitch = 85f;

        [Header("Head Bobbing")]
        public bool enableHeadBob = true;
        public float bobSpeed = 12f;
        public float bobAmount = 0.05f;

        [Header("Hose & Water Spray")]
        public ParticleSystem waterParticle;
        public AudioSource sprayAudioSource;
        public AudioClip sprayAudioClip;
        public float sprayRange = 20f;
        public float waterDamagePerSecond = 28f;
        public Transform nozzleTransform;

        [Header("Interaction")]
        public float interactDistance = 3.5f;
        public LayerMask interactableLayers = ~0;

        [Header("Kesehatan & Suhu")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public float heatDamageRate = 15f;
        private bool isInFireHazard = false;

        // Internal State
        private CharacterController characterController;
        private Vector3 velocity;
        private bool isGrounded;
        private float pitch = 0f;
        private float defaultCameraPosY = 0.8f;
        private float bobTimer = 0f;
        private bool isSpraying = false;
        private Vector3 defaultNozzleLocalPos;
        private bool hasDefaultNozzlePos = false;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.height = 1.8f;
                characterController.center = new Vector3(0, 0.9f, 0);
                characterController.radius = 0.4f;
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCamera != null)
            {
                defaultCameraPosY = playerCamera.localPosition.y;
            }

            if (nozzleTransform != null)
            {
                defaultNozzleLocalPos = nozzleTransform.localPosition;
                hasDefaultNozzlePos = true;
            }

            if (sprayAudioSource != null && sprayAudioClip != null)
            {
                sprayAudioSource.clip = sprayAudioClip;
                sprayAudioSource.loop = true;
                sprayAudioSource.Stop();
            }

            if (waterParticle != null)
            {
                waterParticle.Stop();
            }
        }

        private void Update()
        {
            // Jika misi telah selesai (Menang atau Kalah), hentikan gerakan dan penyemprotan air
            if (ForestLevelManager.Instance != null && ForestLevelManager.Instance.isGameOver)
            {
                if (isSpraying)
                {
                    isSpraying = false;
                    if (waterParticle != null && waterParticle.isPlaying) waterParticle.Stop();
                    if (sprayAudioSource != null && sprayAudioSource.isPlaying) sprayAudioSource.Stop();
                }

                if (nozzleTransform != null && hasDefaultNozzlePos)
                {
                    nozzleTransform.localPosition = defaultNozzleLocalPos;
                }

                // Bebaskan kursor mouse agar pemain dapat mengklik tombol pop up
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                return;
            }

            // Jika game sedang dijeda (Pause), hentikan input dan penyemprotan
            if (Time.timeScale == 0f)
            {
                if (isSpraying)
                {
                    isSpraying = false;
                    if (waterParticle != null && waterParticle.isPlaying) waterParticle.Stop();
                    if (sprayAudioSource != null && sprayAudioSource.isPlaying) sprayAudioSource.Stop();
                }
                return;
            }

            HandleMouseLook();
            HandleMovement();
            HandleSpraying();
            HandleNozzleFeedback();
            HandleInteraction();
            HandleHeatHazard();
        }

        private void HandleMouseLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 60f * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 60f * Time.deltaTime;

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleMovement()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Keep grounded firmly
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && z > 0;
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            Vector3 move = (transform.right * x + transform.forward * z).normalized;
            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Head Bobbing
            if (enableHeadBob && playerCamera != null)
            {
                if (move.sqrMagnitude > 0.01f && isGrounded)
                {
                    float speedMultiplier = isSprinting ? 1.4f : 1f;
                    bobTimer += Time.deltaTime * bobSpeed * speedMultiplier;
                    float newY = defaultCameraPosY + Mathf.Sin(bobTimer) * bobAmount;
                    playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, newY, playerCamera.localPosition.z);
                }
                else
                {
                    bobTimer = 0f;
                    playerCamera.localPosition = new Vector3(
                        playerCamera.localPosition.x,
                        Mathf.Lerp(playerCamera.localPosition.y, defaultCameraPosY, Time.deltaTime * 6f),
                        playerCamera.localPosition.z
                    );
                }
            }
        }

        private void HandleSpraying()
        {
            bool hasWater = ForestLevelManager.Instance != null ? ForestLevelManager.Instance.PunyaAirCukup() : true;
            bool isPressingFire = Input.GetMouseButton(0);

            if (isPressingFire && hasWater)
            {
                if (!isSpraying)
                {
                    isSpraying = true;
                    if (waterParticle != null && !waterParticle.isPlaying) waterParticle.Play();
                    if (sprayAudioSource != null && !sprayAudioSource.isPlaying) sprayAudioSource.Play();
                }

                // Konsumsi supply air di LevelManager
                if (ForestLevelManager.Instance != null)
                {
                    ForestLevelManager.Instance.GunakanAir(Time.deltaTime);
                }

                // Eksekusi pemadaman api via Raycast
                ExecuteWaterRaycast();
            }
            else
            {
                if (isSpraying)
                {
                    isSpraying = false;
                    if (waterParticle != null && waterParticle.isPlaying) waterParticle.Stop();
                    if (sprayAudioSource != null && sprayAudioSource.isPlaying) sprayAudioSource.Stop();
                }
            }
        }

        private void HandleNozzleFeedback()
        {
            if (nozzleTransform == null) return;

            if (!hasDefaultNozzlePos)
            {
                defaultNozzleLocalPos = nozzleTransform.localPosition;
                hasDefaultNozzlePos = true;
            }

            if (isSpraying)
            {
                // Hentakan dan getaran dinamis ketika air bertekanan tinggi menyembur keluar
                Vector3 jitter = new Vector3(
                    Random.Range(-0.002f, 0.002f),
                    Random.Range(-0.002f, 0.002f),
                    Random.Range(-0.006f, -0.002f)
                );
                nozzleTransform.localPosition = Vector3.Lerp(nozzleTransform.localPosition, defaultNozzleLocalPos + jitter, Time.deltaTime * 30f);
            }
            else
            {
                nozzleTransform.localPosition = Vector3.Lerp(nozzleTransform.localPosition, defaultNozzleLocalPos, Time.deltaTime * 12f);
            }
        }

        private void ExecuteWaterRaycast()
        {
            Ray ray;
            if (playerCamera != null)
            {
                ray = new Ray(playerCamera.position, playerCamera.forward);
            }
            else
            {
                ray = new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
            }

            // SphereCast memberikan area jangkauan semprotan yang lebih realistis dibanding garis tipis tunggal
            RaycastHit[] hits = Physics.SphereCastAll(ray, 0.75f, sprayRange);

            foreach (var hit in hits)
            {
                // Cek komponen Fire langsung
                Fire fire = hit.collider.GetComponentInParent<Fire>();
                if (fire != null)
                {
                    fire.DamageFire(waterDamagePerSecond * Time.deltaTime);
                }

                // Cek komponen ForestFireHazard jika ada
                ForestFireHazard hazard = hit.collider.GetComponentInParent<ForestFireHazard>();
                if (hazard != null)
                {
                    hazard.ExtinguishWithWater(waterDamagePerSecond * Time.deltaTime);
                }
            }
        }

        private void HandleInteraction()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            {
                Ray ray = new Ray(playerCamera != null ? playerCamera.position : transform.position, 
                                  playerCamera != null ? playerCamera.forward : transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayers))
                {
                    // Interaksi Kucing
                    CatCompanion cat = hit.collider.GetComponentInParent<CatCompanion>();
                    if (cat != null)
                    {
                        if (ForestLevelManager.Instance != null)
                        {
                            ForestLevelManager.Instance.SelamatkanKucingHutan();
                        }
                    }

                    // Interaksi TriggerInteraksi lama
                    TriggerInteraksi trigger = hit.collider.GetComponent<TriggerInteraksi>();
                    if (trigger != null)
                    {
                        // Trigger interaksi secara langsung
                        trigger.SendMessage("SelesaikanInteraksi", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        private void HandleHeatHazard()
        {
            if (isInFireHazard)
            {
                currentHealth -= heatDamageRate * Time.deltaTime;
                if (currentHealth < 0f) currentHealth = 0f;
            }
            else
            {
                if (currentHealth < maxHealth)
                {
                    currentHealth += 5f * Time.deltaTime;
                }
            }
        }

        public void SetInFireHazard(bool inHazard)
        {
            isInFireHazard = inHazard;
        }

        private void OnTriggerStay(Collider other)
        {
            // Jika berada di dekat area tangki mobil damkar, isi ulang air
            if (other.CompareTag("Respawn") || other.name.ToLower().Contains("truck") || other.name.ToLower().Contains("tank"))
            {
                if (ForestLevelManager.Instance != null)
                {
                    ForestLevelManager.Instance.IsiUlangAir(Time.deltaTime);
                }
            }
        }
    }
}
