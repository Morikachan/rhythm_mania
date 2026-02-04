using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectButtons : MonoBehaviour
{
    public void GoToSolo()
    {
        SceneManager.LoadScene("SelectSongScene");
    }

    public void GoToMulti()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Photon is not connected yet!");
            return;
        }

        SceneManager.LoadScene("MultiLobby");
    }

    public void CreateOwnRoom()
    {
        // Create Room
        SceneManager.LoadScene("");
    }

    public void CodeConnectToRoom()
    {
        // Enter Code
        SceneManager.LoadScene("");
    }
}
