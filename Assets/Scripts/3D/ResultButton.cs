using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultButton : MonoBehaviour
{
    public int buttonType;

    public void ChangeScene()
    {   
        switch (buttonType)
        {
            case 1:
                Debug.Log("Return");
                SceneManager.LoadScene("HomeScreen");
                break;
            case 2:
                Debug.Log("SongSelection");
                SceneManager.LoadScene("SelectSongScene");
                break;
            default:
                Debug.Log("Return");
                SceneManager.LoadScene("HomeScreen");
                break;
        }
    }
}
