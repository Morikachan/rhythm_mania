using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectButtons : MonoBehaviour
{
    public int buttonType;

    public void ChangeScene()
    {
        switch (buttonType)
        {
            case 1:
                // Solo Live
                SceneManager.LoadScene("SelectSongScene");
                break;
            case 2:
                // Multi Live
                SceneManager.LoadScene("SelectSongScene");
                break;
            case 3:
                // Create Room
                SceneManager.LoadScene("");
                break;
            case 4:
                // Enter Code
                SceneManager.LoadScene("");
                break;
            default:
                // Home
                SceneManager.LoadScene("HomeScreen");
                break;
        }
    }
}
