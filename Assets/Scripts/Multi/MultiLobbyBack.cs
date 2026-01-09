using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiLobbyBack : MonoBehaviourPunCallbacks
{
    public void LeaveCurrentRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Not in room");
            return;
        }

        Debug.Log("Leaving room...");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room callback received");
        SceneManager.LoadScene("GameModeSelection");
    }
}
