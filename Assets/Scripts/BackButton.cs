using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public string sceneName;
    public void OnClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
