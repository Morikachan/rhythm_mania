using UnityEngine;
using UnityEngine.UI;
//using Photon.Pun;
//using Photon.Realtime;

public class MultiLobbyManager : MonoBehaviour
{
    public Image playerIllust;
    //[SerializeField] private byte maxPlayer = 2;
    //[SerializeField] private GameObject playerPrefub;
    //private void Start()
    //{
    //    PhotonNetwork.AutomaticallySyncScene = true;
    //    PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);

    //    Debug.Log("Connecting to server...");
    //    PhotonNetwork.ConnectUsingSettings();
    //    PhotonNetwork.GameVersion = "0.0.1";
    //}

    //public void CreatingRoom()
    //{
    //    RoomOptions roomOptions = new RoomOptions();
    //    roomOptions.MaxPlayers = maxPlayer;
    //    roomOptions.IsVisible = true;
    //    PhotonNetwork.CreateRoom(null, roomOptions);
    //    //PhotonNetwork.JoinOrCreateRoom("asd", roomOptions, TypedLobby.Default);
    //}

    //public override void OnCreatedRoom()
    //{
    //    Debug.Log("OnCreatedRoom");
    //}

    //public override void OnJoinedRoom()
    //{
    //    Debug.Log("OnJoinedRoom");

    //    PhotonNetwork.Instantiate(playerPrefub.name, new Vector3(0.4f, 1, 0), Quaternion.identity);
    //    //base.OnJoinedRoom();
    //}
    //public void JoiningRoom()
    //{
    //    PhotonNetwork.JoinRandomRoom();

    //    //string roomName = "asd";
    //    //PhotonNetwork.JoinRoom(roomName);
    //}

    //public override void OnConnectedToMaster()
    //{
    //    Debug.Log("Connected to Master");
    //    // base.OnConnectedToMaster();
    //}

    //public override void OnDisconnected(DisconnectCause cause)
    //{
    //    base.OnDisconnected(cause);
    //    Debug.Log("Disconnected from server for reason: " + cause.ToString());
    //}

    public void DisplayPlayerIllust(int cardId)
    {
        if (playerIllust == null) return;

        string fileName = $"game_icon_{cardId}.png";
        PlayerCardIllustLoader.instance.LoadPlayerIllustration(playerIllust, fileName);
    }
}
