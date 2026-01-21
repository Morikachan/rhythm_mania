using UnityEngine;

public class MultiResultFlow : MonoBehaviour {
    public GameObject multiResult;
    public GameObject playerResult;
    public MultiPlayerResult playerResultScript;

    private bool showedPlayer = false;

    public void OnNext()
    {
        if(!showedPlayer)
        {
            multiResult.SetActive(false);
            playerResult.SetActive(true);

            playerResultScript.SendResultToServer();

            showedPlayer = true;
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager
                .LoadScene("ModeSelectionScene");
        }
    }
}
