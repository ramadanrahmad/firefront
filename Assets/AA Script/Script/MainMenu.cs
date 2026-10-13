using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Pengaturan UI")]
    public GameObject panelSettings;
    public Slider sliderSuara;

    [Header("Pengaturan Audio UI")]
    public AudioSource sfxSource;   // Pemutar khusus efek suara (bukan BGM)
    public AudioClip hoverSound;    // File suara saat mouse lewat
    public AudioClip clickSound;    // File suara saat diklik

    [Header("Pengaturan Level")]
    public GameObject panelLevelSelect;

    private void Start()
    {
        float volumeTersimpan = PlayerPrefs.GetFloat("VolumeGame", 1f);
        AudioListener.volume = volumeTersimpan;
        if (sliderSuara != null) sliderSuara.value = volumeTersimpan;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void PlayUrbanLevel()
    {
        SceneManager.LoadScene("Map");
    }

    public void PlayForestLevel()
    {
        SceneManager.LoadScene("ForestLevel");
    }

    public void CloseLevelSelect()
    {
        if (panelLevelSelect != null)
        {
            panelLevelSelect.SetActive(false);
        }
    }

    public void OpenOptions()
    {
        panelSettings.SetActive(true);
    }

    public void CloseOptions()
    {
        panelSettings.SetActive(false);
    }

    public void AturVolume(float nilaiVolume)
    {
        AudioListener.volume = nilaiVolume;
        PlayerPrefs.SetFloat("VolumeGame", nilaiVolume);
    }

    public void QuitGame()
    {
        Debug.Log("Game Ditutup!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // FUNGSI BARU UNTUK EFEK SUARA
    public void PlayHoverSound()
    {
        sfxSource.PlayOneShot(hoverSound); // PlayOneShot agar suara bisa menumpuk jika digeser cepat
    }

    public void PlayClickSound()
    {
        sfxSource.PlayOneShot(clickSound);
    }
}