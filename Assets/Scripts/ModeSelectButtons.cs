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
                Debug.Log("Solo Live");
                SceneManager.LoadScene("SelectSongScene");
                break;
            case 2:
                Debug.Log("Multi Live");
                SceneManager.LoadScene("SelectSongScene");
                break;
            case 3:
                Debug.Log("Create Room");
                SceneManager.LoadScene("");
                break;
            case 4:
                Debug.Log("Enter Code");
                SceneManager.LoadScene("");
                break;
            default:
                Debug.Log("Home");
                SceneManager.LoadScene("HomeScreen");
                break;
        }
    }
}
