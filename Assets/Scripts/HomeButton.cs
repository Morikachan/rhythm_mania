using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    public int buttonType;

    public void ChangeScene()
    {   
        switch (buttonType)
        {
            case 1:
                SceneManager.LoadScene("GameModeSelection");
                break;
            case 2:
                SceneManager.LoadScene("GachaScreen");
                break;
            case 3:
                SceneManager.LoadScene("CardsScreen");
                break;
            case 4:
                SceneManager.LoadScene("TeamScreen");
                break;
            default:
                SceneManager.LoadScene("HomeScreen");
                break;
        }
    }
}
