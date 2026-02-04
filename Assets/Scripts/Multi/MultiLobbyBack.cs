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

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("GameModeSelection");
    }
}
