using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiResultFlow : MonoBehaviourPunCallbacks {
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
            LeaveAndReset();
        }
    }

    public void LeaveAndReset()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "Ready", false },
            { "SelectState", "Selecting" },
            { "SongID", -1 }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if(MultiResultDataHolder.instance != null)
        {
            Destroy(MultiResultDataHolder.instance.gameObject);
        }

        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("ModeSelectionScene");
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left the room. Returning to menu.");
        SceneManager.LoadScene("ModeSelectionScene");
    }
}
