using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    public string sceneName;

    public void ChangeScene()
    {   
        SceneManager.LoadScene(sceneName);
    }
}
