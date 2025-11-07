using Unity.VisualScripting;
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
                Debug.Log("Game");
                break;
            case 2:
                Debug.Log("Gacha");
                SceneManager.LoadScene("GachaScreen");
                break;
            case 3:
                Debug.Log("Cards");
                SceneManager.LoadScene("CardsScreen");
                break;
            default:
                Debug.Log("Team");
                SceneManager.LoadScene("TeamScreen");
                break;
        }
    }
}
