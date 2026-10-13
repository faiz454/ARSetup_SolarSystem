using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Volume Controls")]
    [SerializeField] private Slider volumeSlider;

    private const string VolumePrefKey = "AppVolume";

    private void Start()
    {
        // Hide Settings Panel at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Load saved volume preference or default to 100%
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1.0f);
        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Called by Play Button to load the AR Scene
    public void PlayGame()
    {
        // Loads scene index 1 (Your AR Solar System Scene)
        SceneManager.LoadScene(1);
    }

    // Called by Settings Button to open Panel
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Called by Close Button inside Settings Panel
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Adjusts master volume for all planet/satellite narration and audio
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }
}