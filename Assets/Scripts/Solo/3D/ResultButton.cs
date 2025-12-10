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
                SceneManager.LoadScene("HomeScreen");
                break;
            case 2:
                SceneManager.LoadScene("SelectSongScene");
                break;
            default:
                SceneManager.LoadScene("HomeScreen");
                break;
        }
    }
}
