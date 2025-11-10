using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeButtons : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuPop;
    public void ToPauseGame()
    {
        pauseMenuPop.SetActive(true);
        Time.timeScale = 0;
    }
    public void ToResumeGame()
    {
        pauseMenuPop.SetActive(false);
        Time.timeScale = 1;
    }
    public void ToExitGame()
    {
        SceneManager.LoadScene("HomeScreen");
    }
}
