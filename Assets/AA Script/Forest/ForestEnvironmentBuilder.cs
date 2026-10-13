using UnityEngine;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Firesomething.Forest
{
    [ExecuteInEditMode]
    public class ForestEnvironmentBuilder : MonoBehaviour
    {
        [Header("Pengaturan Pembangkitan Peta Kompak")]
        public bool generateOnStart = true;
        public int treeCount = 42; // Pohon merapat di area kompak
        public int bushCount = 28;
        public float forestRadius = 22f; // Map ringkas & fokus (radius 22 meter)
        public int seed = 42;

        [Header("Prefab Lingkungan")]
        public GameObject tree1Prefab;
        public GameObject tree2Prefab;
        public GameObject tree3Prefab;
        public GameObject bushPrefab;
        public GameObject fireTruckPrefab;
        public GameObject catPrefab;
        public GameObject fireAreaPrefab;

        [Header("Material")]
        public Material grassMaterial;
        public Material dirtMaterial;

        [Header("Audio Clips")]
        public AudioClip bgmMusicClip;
        public AudioClip fireSfxClip;
        public AudioClip waterSfxClip;
        public AudioClip catSfxClip;
        public AudioClip clickSfxClip;

        [Header("Nozzle & Semprotan Air (Sesuai Scene Map)")]
        public GameObject nozzlePrefab;
        public Material waterSplashMaterial;

        private void Start()
        {
            if (Application.isPlaying && generateOnStart)
            {
                if (transform.childCount == 0)
                {
                    BuildEntireForest();
                }
            }
        }

        [ContextMenu("Bangun Lingkungan Hutan Lengkap")]
        public void BuildEntireForest()
        {
            Debug.Log("🌲 [ForestEnvironmentBuilder] Membangun arena hutan kompak dengan pohon merapat...");

            LoadAssetsIfNull();

            // 1. Bersihkan objek lama
            ClearExistingChildren();

            // 2. Setup Player & Kamera di Posisi Basecamp
            Vector3 truckPos = new Vector3(0f, 0f, -16f);
            Vector3 playerSpawnPos = truckPos + new Vector3(0f, 0.4f, 3.5f); // Menghadap langsung ke arah pohon terbakar
            try
            {
                SetupPlayer(playerSpawnPos);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Peringatan saat setup player: {ex.Message}");
            }

            // 3. Setup Pencahayaan & Suasana Atmosferik
            SetupAtmosphere();

            // 4. Buat Tanah Hutan Kompak (48m x 48m)
            try
            {
                CreateForestGround();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Peringatan saat membuat tanah: {ex.Message}");
            }

            // 5. Bangun Basecamp Truk Damkar
            try
            {
                SetupBasecamp(truckPos);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Peringatan saat setup basecamp: {ex.Message}");
            }

            // 6. Bangun Pepohonan Merapat & Dinding Batas Hutan
            try
            {
                SpawnDenseTreesAndVegetation();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Peringatan saat tanam pohon: {ex.Message}");
            }

            // 7. Tempatkan 3 Titik Api Terfokus Saja
            try
            {
                SpawnFocusedFireSpots();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Peringatan saat spawn api: {ex.Message}");
            }

            // 8. Setup Level Manager & HUD
            try
            {
                SetupManagersAndUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Peringatan saat setup UI: {ex.Message}");
            }

            Debug.Log("✅ [ForestEnvironmentBuilder] Arena hutan kompak & rapat berhasil dibangun!");
        }

        public void LoadAssetsIfNull()
        {
#if UNITY_EDITOR
            tree1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Enviroment/Tree1.prefab");
            tree2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Enviroment/Tree2.prefab");
            tree3Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Enviroment/Tree3.prefab");
            bushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Enviroment/Bush1.prefab");
            fireTruckPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Vehicles/ForestFireTruck1.prefab");
            catPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ladymito/Free_cat/Prefabs/cat.prefab");
            fireAreaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UnityTechnologies/ParticlePack/EffectExamples/Fire & Explosion Effects/Prefabs/WildFire.prefab")
                          ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UnityTechnologies/ParticlePack/EffectExamples/Fire & Explosion Effects/Prefabs/LargeFlames.prefab")
                          ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AA Script/FireArea.prefab");

            grassMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/PBS Materials Variety Pack/Grass/Grass 2.mat");
            dirtMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/PBS Materials Variety Pack/Dirt/Dirt 3.mat");

            if (bgmMusicClip == null) bgmMusicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AA Script/Audio/Bg Music.mp3");
            if (fireSfxClip == null) fireSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AA Script/Audio/fire sound.mp3");
            if (waterSfxClip == null) waterSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AA Script/Audio/hose spray.mp3");
            if (catSfxClip == null) catSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AA Script/Audio/cat.mp3");
            if (clickSfxClip == null) clickSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AA Script/Audio/Click.mp3");

            if (nozzlePrefab == null) nozzlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Equipment/Nozzle.prefab");
            if (waterSplashMaterial == null) waterSplashMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/UnityTechnologies/ParticlePack/EffectExamples/Legacy Particles/Materials/WaterfallSplash.mat");
#endif
        }

        private void ClearExistingChildren()
        {
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                children.Add(transform.GetChild(i).gameObject);
            }

            foreach (var child in children)
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        private GameObject CreateForestGround()
        {
            GameObject ground = new GameObject("ForestGround_Compact");
            ground.transform.SetParent(transform);
            ground.transform.position = Vector3.zero;

            MeshFilter mf = ground.AddComponent<MeshFilter>();
            MeshRenderer mr = ground.AddComponent<MeshRenderer>();
            MeshCollider mc = ground.AddComponent<MeshCollider>();

            // Mesh tanah kompak (48m x 48m)
            int width = 24;
            int length = 24;
            float spacing = 2.0f;
            Vector3[] vertices = new Vector3[(width + 1) * (length + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[width * length * 6];

            float xOffset = -width * spacing * 0.5f;
            float zOffset = -length * spacing * 0.5f;

            for (int z = 0, i = 0; z <= length; z++)
            {
                for (int x = 0; x <= width; x++, i++)
                {
                    float worldX = xOffset + x * spacing;
                    float worldZ = zOffset + z * spacing;

                    float distFromCenter = Mathf.Sqrt(worldX * worldX + worldZ * worldZ);
                    // Datar di basecamp (z < -8), sedikit berbukit di area pohon
                    float hill = 0f;
                    if (worldZ > -8f)
                    {
                        float heightFactor = Mathf.Clamp01((distFromCenter - 6f) / 16f);
                        hill = Mathf.PerlinNoise(worldX * 0.08f + 10f, worldZ * 0.08f + 20f) * 2.8f * heightFactor;
                    }

                    // Tepi arena dinaikkan membentuk cekungan mangkuk alami
                    if (distFromCenter > 18f)
                    {
                        hill += (distFromCenter - 18f) * 0.7f;
                    }

                    vertices[i] = new Vector3(worldX, hill, worldZ);
                    uvs[i] = new Vector2((float)x / 1.5f, (float)z / 1.5f);
                }
            }

            for (int z = 0, ti = 0, vi = 0; z < length; z++, vi++)
            {
                for (int x = 0; x < width; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                    triangles[ti + 5] = vi + width + 2;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "CompactForestMesh";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;

            if (grassMaterial != null)
            {
                mr.sharedMaterial = grassMaterial;
            }
            else
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mr.sharedMaterial.color = new Color(0.22f, 0.42f, 0.15f);
            }

            ground.layer = 0;
            return ground;
        }

        private void SetupBasecamp(Vector3 truckPos)
        {
            GameObject basecampGroup = new GameObject("Basecamp");
            basecampGroup.transform.SetParent(transform);

            // Truk Pemadam Kebakaran
            if (fireTruckPrefab != null)
            {
                GameObject truck = Instantiate(fireTruckPrefab, truckPos, Quaternion.Euler(0, 180, 0), basecampGroup.transform);
                truck.name = "ForestFireTruck";

                BoxCollider refillTrigger = truck.AddComponent<BoxCollider>();
                refillTrigger.isTrigger = true;
                refillTrigger.size = new Vector3(8f, 4f, 10f);
                refillTrigger.center = new Vector3(0, 1f, 0);
                truck.tag = "Respawn";
            }

            // Lampu Penerangan Basecamp
            GameObject campLight = new GameObject("BasecampWorkLight");
            campLight.transform.SetParent(basecampGroup.transform);
            campLight.transform.position = truckPos + new Vector3(3f, 3.5f, 0f);
            Light pLight = campLight.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.range = 16f;
            pLight.color = new Color(1f, 0.9f, 0.7f);
            pLight.intensity = 2.2f;
        }

        private void SpawnDenseTreesAndVegetation()
        {
            GameObject foliageGroup = new GameObject("DenseForestFoliage");
            foliageGroup.transform.SetParent(transform);

            Random.InitState(seed);

            GameObject[] treePrefabs = new GameObject[] { tree1Prefab, tree2Prefab, tree3Prefab };
            List<GameObject> validTreePrefabs = new List<GameObject>();
            foreach (var tp in treePrefabs) if (tp != null) validTreePrefabs.Add(tp);

            // 1. Dinding Pohon Rapat di Sekeliling Batas Arena (Keliling Map)
            int boundaryTrees = 24;
            float boundaryRadius = 20.5f;
            for (int i = 0; i < boundaryTrees; i++)
            {
                float angle = (i / (float)boundaryTrees) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * boundaryRadius, 0, Mathf.Sin(angle) * boundaryRadius);
                if (pos.z < -17f) continue; // Jangan menutupi area jalan masuk basecamp

                pos.y = SampleHeightAt(pos.x, pos.z);
                SpawnSingleTree(pos, validTreePrefabs, foliageGroup.transform, 1.1f, 1.4f);
            }

            // 2. Pohon Rapat di Area Dalam (Dense Clusters)
            // Area Pohon Kiri (-8, 0, -2)
            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = new Vector3(-8f + Random.Range(-3f, 3f), 0, -2f + Random.Range(-3f, 3f));
                pos.y = SampleHeightAt(pos.x, pos.z);
                SpawnSingleTree(pos, validTreePrefabs, foliageGroup.transform, 0.9f, 1.2f);
            }

            // Area Pohon Depan (0, 0, -6)
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-2.5f, 2.5f), 0, -6f + Random.Range(-2.5f, 2.5f));
                pos.y = SampleHeightAt(pos.x, pos.z);
                SpawnSingleTree(pos, validTreePrefabs, foliageGroup.transform, 0.95f, 1.25f);
            }

            // Area Pohon Kanan (8, 0, 4)
            for (int i = 0; i < 7; i++)
            {
                Vector3 pos = new Vector3(8f + Random.Range(-3f, 3f), 0, 4f + Random.Range(-3f, 3f));
                pos.y = SampleHeightAt(pos.x, pos.z);
                SpawnSingleTree(pos, validTreePrefabs, foliageGroup.transform, 0.9f, 1.2f);
            }

            // Area Pohon Belakang Dalam (0, 0, 12)
            for (int i = 0; i < 8; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-7f, 7f), 0, 12f + Random.Range(-3f, 4f));
                pos.y = SampleHeightAt(pos.x, pos.z);
                SpawnSingleTree(pos, validTreePrefabs, foliageGroup.transform, 0.85f, 1.2f);
            }

            // 3. Tanam Semak-semak Rapat di Bawah Pohon
            if (bushPrefab != null)
            {
                for (int i = 0; i < bushCount; i++)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * (forestRadius * 0.75f);
                    if (randomCircle.y < -13f) continue;

                    Vector3 pos = new Vector3(randomCircle.x, 0f, randomCircle.y);
                    pos.y = SampleHeightAt(pos.x, pos.z);

                    GameObject bush = Instantiate(bushPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), foliageGroup.transform);
                    bush.transform.localScale = Vector3.one * Random.Range(0.8f, 1.3f);
                }
            }
        }

        private void SpawnSingleTree(Vector3 pos, List<GameObject> validTreePrefabs, Transform parent, float minScale, float maxScale)
        {
            if (validTreePrefabs.Count > 0)
            {
                GameObject chosenPrefab = validTreePrefabs[Random.Range(0, validTreePrefabs.Count)];
                GameObject tree = Instantiate(chosenPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
                tree.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);

                if (tree.GetComponent<Collider>() == null)
                {
                    CapsuleCollider col = tree.AddComponent<CapsuleCollider>();
                    col.height = 8f;
                    col.radius = 0.6f;
                    col.center = new Vector3(0, 4f, 0);
                }
            }
        }

        private float SampleHeightAt(float x, float z)
        {
            float distFromCenter = Mathf.Sqrt(x * x + z * z);
            float hill = 0f;
            if (z > -8f)
            {
                float heightFactor = Mathf.Clamp01((distFromCenter - 6f) / 16f);
                hill = Mathf.PerlinNoise(x * 0.08f + 10f, z * 0.08f + 20f) * 2.8f * heightFactor;
            }
            if (distFromCenter > 18f)
            {
                hill += (distFromCenter - 18f) * 0.7f;
            }
            return hill;
        }

        private void SpawnFocusedFireSpots()
        {
            GameObject firesGroup = new GameObject("FocusedWildfires");
            firesGroup.transform.SetParent(transform);

            // HANYA 3 TITIK API TERFOKUS & JELAS:
            // 1. Api Depan Truk (Hanya 7-8 meter tepat di depan mata pemain!)
            SpawnSingleFire(new Vector3(0f, 0f, -7.5f), firesGroup.transform, "Api_Sektor_1_DepanTruk");

            // 2. Api Sektor Kiri (12 meter di kiri, membakar gugusan pohon kiri)
            SpawnSingleFire(new Vector3(-7.5f, 0f, -1.5f), firesGroup.transform, "Api_Sektor_2_PohonKiri");

            // 3. Api Sektor Kanan (14 meter di kanan, dekat satwa kucing)
            SpawnSingleFire(new Vector3(7.5f, 0f, 3.5f), firesGroup.transform, "Api_Sektor_3_PohonKanan");
        }

        private void SpawnSingleFire(Vector3 pos, Transform parent, string spotName)
        {
            pos.y = SampleHeightAt(pos.x, pos.z) + 0.1f;

            GameObject fireRoot = null;
            if (fireAreaPrefab != null)
            {
                fireRoot = Instantiate(fireAreaPrefab, pos, Quaternion.identity, parent);
                
                // Api di Batang Tengah Pohon (Ketinggian 2.4m)
                GameObject midTrunkFire = Instantiate(fireAreaPrefab, pos + Vector3.up * 2.4f, Quaternion.identity, fireRoot.transform);
                midTrunkFire.name = "MidTrunkFire";
                midTrunkFire.transform.localScale = Vector3.one * 0.75f;

                // Api di Tajuk Dedaunan Atas (Ketinggian 4.6m)
                GameObject canopyFire = Instantiate(fireAreaPrefab, pos + Vector3.up * 4.6f, Quaternion.identity, fireRoot.transform);
                canopyFire.name = "CanopyFire";
                canopyFire.transform.localScale = Vector3.one * 0.85f;
            }
            else
            {
                fireRoot = new GameObject("WildfireObject");
                fireRoot.transform.SetParent(parent);
                fireRoot.transform.position = pos;
            }

            fireRoot.name = spotName;
            fireRoot.transform.localScale = Vector3.one * 2.2f; // Skala api membara alami

            // Pastikan semua sistem partikel aktif dan berputar (loop)
            foreach (var ps in fireRoot.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.playOnAwake = true;
                main.loop = true;
                if (!ps.isPlaying) ps.Play();
            }

            // Setup Collider Trigger setinggi seluruh pohon (7 meter) untuk menerima semprotan air
            BoxCollider col = fireRoot.GetComponent<BoxCollider>();
            if (col == null) col = fireRoot.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4.5f, 7.5f, 4.5f);
            col.center = new Vector3(0, 3.2f, 0);

            // Tambahkan komponen ForestFireHazard dengan visual lidah api 3D menyala terang
            ForestFireHazard hazard = fireRoot.GetComponent<ForestFireHazard>();
            if (hazard == null) hazard = fireRoot.AddComponent<ForestFireHazard>();
            hazard.heatDamageRadius = 6.0f;
            hazard.fireLoopClip = fireSfxClip;
        }

        private void SpawnTrappedCat()
        {
            GameObject catGroup = new GameObject("TrappedWildlife");
            catGroup.transform.SetParent(transform);

            // Kucing di dekat gugusan pohon kanan (dekat api 3)
            Vector3 catPos = new Vector3(9.5f, 0f, 7.5f);
            catPos.y = SampleHeightAt(catPos.x, catPos.z) + 0.1f;

            GameObject cat = null;
            if (catPrefab != null)
            {
                cat = Instantiate(catPrefab, catPos, Quaternion.Euler(0, 190f, 0), catGroup.transform);
            }
            else
            {
                cat = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                cat.transform.SetParent(catGroup.transform);
                cat.transform.position = catPos + Vector3.up * 0.3f;
                cat.transform.localScale = new Vector3(0.4f, 0.3f, 0.7f);
                cat.GetComponent<Renderer>().material.color = new Color(1f, 0.6f, 0.2f);
            }

            cat.name = "TrappedCat";

            SphereCollider sc = cat.GetComponent<SphereCollider>();
            if (sc == null) sc = cat.AddComponent<SphereCollider>();
            sc.radius = 2.2f;
            sc.isTrigger = true;

            // NavMeshAgent
            UnityEngine.AI.NavMeshAgent agent = cat.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent == null) agent = cat.AddComponent<UnityEngine.AI.NavMeshAgent>();

            CatCompanion companion = cat.GetComponent<CatCompanion>();
            if (companion == null) companion = cat.AddComponent<CatCompanion>();
            companion.intervalMeongMin = 3f;
            companion.intervalMeongMax = 7f;
            companion.player = FindObjectOfType<ForestPlayerController>()?.transform;

            AudioSource catAudio = cat.GetComponent<AudioSource>();
            if (catAudio == null) catAudio = cat.AddComponent<AudioSource>();
            catAudio.clip = catSfxClip;
            catAudio.spatialBlend = 1.0f;
            catAudio.minDistance = 3f;
            catAudio.maxDistance = 25f;
            catAudio.playOnAwake = false;
        }

        private void SetupAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.45f, 0.35f, 0.28f, 1f);

            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                Light[] lights = FindObjectsOfType<Light>();
                foreach (var l in lights)
                {
                    if (l.type == LightType.Directional) { sun = l; break; }
                }
            }

            if (sun == null)
            {
                GameObject sunObj = new GameObject("WildfireSun");
                sun = sunObj.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.transform.rotation = Quaternion.Euler(32f, -50f, 0f);
            sun.color = new Color(1f, 0.68f, 0.45f);
            sun.intensity = 1.25f;

            GameObject bgmObj = GameObject.Find("ForestBGM");
            if (bgmObj == null)
            {
                bgmObj = new GameObject("ForestBGM");
                bgmObj.transform.SetParent(transform);
                AudioSource audio = bgmObj.AddComponent<AudioSource>();
                audio.clip = bgmMusicClip;
                audio.loop = true;
                audio.volume = 0.45f;
                audio.playOnAwake = true;
                if (Application.isPlaying) audio.Play();
            }
        }

        private void SetupPlayer(Vector3 spawnPos)
        {
            ForestPlayerController existingPlayer = FindObjectOfType<ForestPlayerController>();
            if (existingPlayer != null) return;

            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.transform.position = spawnPos;

            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.radius = 0.4f;

            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(playerObj.transform);
            camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            camObj.tag = "MainCamera";

            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            camObj.AddComponent<AudioListener>();

            ForestPlayerController fpc = playerObj.AddComponent<ForestPlayerController>();
            fpc.playerCamera = camObj.transform;
            fpc.sprayAudioClip = waterSfxClip;

            AudioSource sprayAudio = camObj.AddComponent<AudioSource>();
            fpc.sprayAudioSource = sprayAudio;

            // 1. Instansiasi Model 3D Nozzle (Selang Pemadam) persis seperti pada scene Map
#if UNITY_EDITOR
            if (nozzlePrefab == null)
            {
                nozzlePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Prefabs/Equipment/Nozzle.prefab");
            }
            if (waterSplashMaterial == null)
            {
                waterSplashMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/UnityTechnologies/ParticlePack/EffectExamples/Legacy Particles/Materials/WaterfallSplash.mat");
            }
#endif

            GameObject nozzleObj = null;
            try
            {
                if (nozzlePrefab != null)
                {
                    nozzleObj = Instantiate(nozzlePrefab, camObj.transform);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ForestEnvironmentBuilder] Gagal instansiasi nozzlePrefab: {ex.Message}");
                nozzleObj = null;
            }

            if (nozzleObj == null)
            {
#if UNITY_EDITOR
                try
                {
                    GameObject fbxObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Jucho/FirefightersPack/Models/Exports/Equipment/Nozzle.fbx");
                    if (fbxObj != null)
                    {
                        nozzleObj = Instantiate(fbxObj, camObj.transform);
                    }
                }
                catch { }
#endif
            }

            if (nozzleObj == null)
            {
                nozzleObj = new GameObject("Nozzle");
                nozzleObj.transform.SetParent(camObj.transform);
            }
            nozzleObj.name = "Nozzle";

            // Posisi dan Rotasi Nozzle di sudut kanan bawah kamera (persis seperti scene Map)
            nozzleObj.transform.localPosition = new Vector3(0.32f, -0.18f, 0.5f);
            nozzleObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            nozzleObj.transform.localScale = Vector3.one;

            // Pastikan tidak ada collider pada nozzle yang mengganggu pergerakan atau raycast
            foreach (var col in nozzleObj.GetComponentsInChildren<Collider>())
            {
                Destroy(col);
            }

            fpc.nozzleTransform = nozzleObj.transform;

            // 2. Buat WaterSpawnPoint di ujung nozzle
            GameObject spawnPointObj = new GameObject("WaterSpawnPoint");
            spawnPointObj.transform.SetParent(nozzleObj.transform);
            spawnPointObj.transform.localPosition = Vector3.zero;
            spawnPointObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            spawnPointObj.transform.localScale = Vector3.one;

            // 3. Buat WaterSpray (Particle System semprotan air busa volumetrik persis scene Map)
            GameObject waterObj = new GameObject("WaterSpray");
            waterObj.tag = "Water";
            waterObj.transform.SetParent(spawnPointObj.transform);
            waterObj.transform.localPosition = Vector3.zero;
            waterObj.transform.localRotation = Quaternion.identity;
            waterObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            ParticleSystem ps = waterObj.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystemRenderer psRenderer = waterObj.GetComponent<ParticleSystemRenderer>();

            // Setup Renderer menggunakan material WaterfallSplash.mat
            if (waterSplashMaterial != null)
            {
                psRenderer.sharedMaterial = waterSplashMaterial;
            }
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.maxParticleSize = 0.5f;
            psRenderer.lengthScale = 2f;
            psRenderer.normalDirection = 1f;

            // Setup Main Module sesuai spesifikasi scene Map
            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = 1.0f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 50f);
            main.startSize = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startColor = Color.white;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.maxParticles = 1000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            // Setup Emission Module (200 partikel/detik untuk efek semburan deras dan tebal)
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 200f;

            // Setup Shape Module (Cone angle 5 derajat, radius 0.01)
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.01f;

            // Setup Size Over Lifetime (tumbuh membesar dari nosel ke kejauhan)
            var sizeModule = ps.sizeOverLifetime;
            sizeModule.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(new Keyframe(0f, 0f, 0f, 1f));
            sizeCurve.AddKey(new Keyframe(1f, 1f, 1f, 0f));
            sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Setup Color Over Lifetime (semi-transparan di awal, padat di tengah, memudar di akhir)
            var colorModule = ps.colorOverLifetime;
            colorModule.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(1f, 0.45f), new GradientAlphaKey(0f, 1f) }
            );
            colorModule.color = new ParticleSystem.MinMaxGradient(grad);

            fpc.waterParticle = ps;
        }

        private void SetupManagersAndUI()
        {
            if (FindObjectOfType<ForestLevelManager>() == null)
            {
                GameObject mgrObj = new GameObject("ForestLevelManager");
                mgrObj.AddComponent<ForestLevelManager>();
            }

            if (FindObjectOfType<ForestHUDManager>() == null)
            {
                GameObject canvasObj = new GameObject("ForestHUD_Canvas", typeof(RectTransform));
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                ForestHUDManager hud = canvasObj.AddComponent<ForestHUDManager>();
                hud.clickSfx = clickSfxClip;

                SetupUIElements(canvasObj.transform, hud);
            }
        }

        private void SetupUIElements(Transform canvasTransform, ForestHUDManager hud)
        {
            // 1. Timer Digital Besar di Atas Tengah Layar (Persis seperti Scene Map)
            GameObject timerObj = new GameObject("TimerText", typeof(RectTransform));
            timerObj.transform.SetParent(canvasTransform, false);
            RectTransform rtTimer = timerObj.GetComponent<RectTransform>();
            rtTimer.anchorMin = new Vector2(0.5f, 1f);
            rtTimer.anchorMax = new Vector2(0.5f, 1f);
            rtTimer.pivot = new Vector2(0.5f, 1f);
            rtTimer.anchoredPosition = new Vector2(0f, -20f);
            rtTimer.sizeDelta = new Vector2(300f, 60f);
            TMP_Text timerTmp = timerObj.AddComponent<TextMeshProUGUI>();
            timerTmp.fontSize = 46;
            timerTmp.fontStyle = FontStyles.Bold;
            timerTmp.alignment = TextAlignmentOptions.Center;
            timerTmp.color = Color.white;
            timerTmp.text = "03:00";
            hud.timerText = timerTmp;

            // 2. Container Informasi Misi di Kiri Atas (Teks Dibesarkan & Kontras Tinggi)
            GameObject objPanel = new GameObject("ObjectiveContainer", typeof(RectTransform));
            objPanel.transform.SetParent(canvasTransform, false);
            RectTransform rtObj = objPanel.GetComponent<RectTransform>();
            rtObj.anchorMin = new Vector2(0f, 1f);
            rtObj.anchorMax = new Vector2(0f, 1f);
            rtObj.pivot = new Vector2(0f, 1f);
            rtObj.anchoredPosition = new Vector2(30f, -25f);
            rtObj.sizeDelta = new Vector2(550f, 130f);

            // Tulisan Padamkan Api (Dibesarkan)
            GameObject mainObj = new GameObject("MainObjectiveText", typeof(RectTransform));
            mainObj.transform.SetParent(objPanel.transform, false);
            RectTransform rtMain = mainObj.GetComponent<RectTransform>();
            rtMain.anchorMin = new Vector2(0f, 1f);
            rtMain.anchorMax = new Vector2(0f, 1f);
            rtMain.pivot = new Vector2(0f, 1f);
            rtMain.anchoredPosition = Vector2.zero;
            rtMain.sizeDelta = new Vector2(550f, 45f);
            TMP_Text mainTmp = mainObj.AddComponent<TextMeshProUGUI>();
            mainTmp.fontSize = 30;
            mainTmp.fontStyle = FontStyles.Bold;
            mainTmp.color = new Color(1f, 0.28f, 0.22f);
            mainTmp.text = "PADAMKAN API: 0/3 (0%)";
            hud.mainObjectiveText = mainTmp;

            // Tulisan Pasokan Air (Dibesarkan)
            GameObject waterObj = new GameObject("WaterSupplyText", typeof(RectTransform));
            waterObj.transform.SetParent(objPanel.transform, false);
            RectTransform rtWater = waterObj.GetComponent<RectTransform>();
            rtWater.anchorMin = new Vector2(0f, 1f);
            rtWater.anchorMax = new Vector2(0f, 1f);
            rtWater.pivot = new Vector2(0f, 1f);
            rtWater.anchoredPosition = new Vector2(0f, -50f);
            rtWater.sizeDelta = new Vector2(550f, 40f);
            TMP_Text waterTmp = waterObj.AddComponent<TextMeshProUGUI>();
            waterTmp.fontSize = 26;
            waterTmp.fontStyle = FontStyles.Bold;
            waterTmp.color = new Color(0.25f, 0.85f, 1f);
            waterTmp.text = "PASOKAN AIR: 100%";
            hud.waterSupplyText = waterTmp;

            // 3. Tombol Pause di Kanan Atas Layar
            GameObject pauseBtnObj = new GameObject("HUD_PauseBtn", typeof(RectTransform));
            pauseBtnObj.transform.SetParent(canvasTransform, false);
            RectTransform rtPauseBtn = pauseBtnObj.GetComponent<RectTransform>();
            rtPauseBtn.anchorMin = new Vector2(1f, 1f);
            rtPauseBtn.anchorMax = new Vector2(1f, 1f);
            rtPauseBtn.pivot = new Vector2(1f, 1f);
            rtPauseBtn.anchoredPosition = new Vector2(-25f, -25f);
            rtPauseBtn.sizeDelta = new Vector2(110f, 42f);

            UnityEngine.UI.Image pauseBtnImg = pauseBtnObj.AddComponent<UnityEngine.UI.Image>();
            pauseBtnImg.color = new Color(0.12f, 0.16f, 0.24f, 0.85f);
            pauseBtnImg.raycastTarget = true;

            UnityEngine.UI.Button pauseBtn = pauseBtnObj.AddComponent<UnityEngine.UI.Button>();
            pauseBtn.targetGraphic = pauseBtnImg;
            pauseBtn.onClick.AddListener(() =>
            {
                hud.PauseGame();
            });

            GameObject pauseTxtObj = new GameObject("Text", typeof(RectTransform));
            pauseTxtObj.transform.SetParent(pauseBtnObj.transform, false);
            RectTransform pauseTxtRt = pauseTxtObj.GetComponent<RectTransform>();
            pauseTxtRt.anchorMin = Vector2.zero;
            pauseTxtRt.anchorMax = Vector2.one;
            pauseTxtRt.offsetMin = Vector2.zero;
            pauseTxtRt.offsetMax = Vector2.zero;
            TMP_Text pauseTmp = pauseTxtObj.AddComponent<TextMeshProUGUI>();
            pauseTmp.fontSize = 17;
            pauseTmp.fontStyle = FontStyles.Bold;
            pauseTmp.alignment = TextAlignmentOptions.Center;
            pauseTmp.color = Color.white;
            pauseTmp.text = "⏸️ JEDA";
            pauseTmp.raycastTarget = false;
        }
    }
}
