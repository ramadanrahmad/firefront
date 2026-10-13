#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Firesomething.Forest
{
    public static class ForestSceneBuilderEditor
    {
        private const string FOREST_SCENE_PATH = "Assets/Scenes/ForestLevel.unity";

        [MenuItem("Tools/Firesomething/Buat Scene Forest Wildfire Baru", false, 1)]
        public static void GenerateForestScene()
        {
            // 1. Buat Scene baru
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 2. Tambahkan Environment Builder GameObject
            GameObject builderObj = new GameObject("--- FOREST_ENVIRONMENT_GENERATOR ---");
            ForestEnvironmentBuilder builder = builderObj.AddComponent<ForestEnvironmentBuilder>();
            builder.generateOnStart = true;

            // 3. Bangun seluruh lingkungan hutan secara instan
            builder.BuildEntireForest();

            // 4. Pastikan folder Assets/Scenes ada
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            // 5. Simpan Scene ke Assets/Scenes/ForestLevel.unity
            bool saveSuccess = EditorSceneManager.SaveScene(newScene, FOREST_SCENE_PATH);
            if (saveSuccess)
            {
                Debug.Log($"🎉 [ForestSceneBuilderEditor] Scene berhasil disimpan ke: {FOREST_SCENE_PATH}");
                RegisterSceneInBuildSettings(FOREST_SCENE_PATH);
            }
            else
            {
                Debug.LogError("❌ [ForestSceneBuilderEditor] Gagal menyimpan scene.");
            }

            AssetDatabase.Refresh();
        }

        public static void RegisterSceneInBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Cek apakah scene sudah terdaftar
            bool alreadyRegistered = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    alreadyRegistered = true;
                    s.enabled = true;
                    break;
                }
            }

            if (!alreadyRegistered)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"📌 [ForestSceneBuilderEditor] Scene {scenePath} berhasil didaftarkan ke Build Settings!");
            }
        }
    }
}
#endif
