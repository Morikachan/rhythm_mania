using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeButtons : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuPop;

    // Reference to the MusicManager
    private MusicManager musicManager;

    void Start()
    {
        musicManager = FindObjectOfType<MusicManager>();

        if (musicManager == null)
        {
            Debug.LogError("MusicManager not found in the scene! Cannot control audio.");
        }
    }

    public void ToPauseGame()
    {
        pauseMenuPop.SetActive(true);
        Time.timeScale = 0;

        if (musicManager != null)
        {
            musicManager.PauseAudio();
        }
    }
    public void ToResumeGame()
    {
        pauseMenuPop.SetActive(false);
        Time.timeScale = 1;

        if (musicManager != null)
        {
            musicManager.ResumeAudio();
        }
    }
    public void ToExitGame()
    {
        pauseMenuPop.SetActive(false);
        Time.timeScale = 1;

        SceneManager.LoadScene("HomeScreen");
    }
}
