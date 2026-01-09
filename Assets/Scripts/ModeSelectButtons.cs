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

        // „R„‚„p„x„… „„u„‚„u„‡„€„t„y„} „r „|„€„q„q„y-„ƒ„ˆ„u„~„… („~„u „w„t„v„} callback)
        SceneManager.LoadScene("MultiLobby");
    }

    public void CreateOwnRoom()
    {
        // Create Room
        // POPUP
        SceneManager.LoadScene("");
    }

    public void CodeConnectToRoom()
    {
        // Enter Code
        // POPUP
        SceneManager.LoadScene("");
    }
}
