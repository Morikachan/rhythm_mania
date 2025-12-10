using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySelect : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene("3D-game");
    }
}
