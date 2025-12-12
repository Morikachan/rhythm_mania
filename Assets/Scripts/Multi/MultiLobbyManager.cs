using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Networking;
using TMPro;
using System.Text; // Needed for Encoding

public class MultiLobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private byte maxPlayer = 2;
    [SerializeField] private string SongSelectionSceneName = "SongSelectionScene";

    public GameObject player1Panel;
    public GameObject player2Panel;

    public Image player1IllustImage;
    public Image player2IllustImage;
    public Sprite defaultAvatarSprite;

    [SerializeField] private TextMeshProUGUI player1UsernameText;
    [SerializeField] private TextMeshProUGUI player2UsernameText;

    private const string USER_ID_KEY = "UserID";
    private string receiveUrl = "http://localhost/rhythm_mania/Database/user-home-info.php";

    [System.Serializable]
    public class UserData
    {
        public string user_id;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
        public UserInfo user_info;
        public UserInventory user_inventory;
    }

    [System.Serializable]
    public class UserInfo
    {
        public string user_name;
        public int user_lvl;
        public int user_exp;
        public int next_lvl_value;
        public int next_lvl_percent;
        public int home_card_id;
    };

    [System.Serializable]
    public class UserInventory
    {
        public int free_gems;
        public int paid_gems;
        public int coins;
    };

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        Debug.Log("Connecting to server...");
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = "0.0.1";
    }

    public void CreatingRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayer;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(null, roomOptions);
        //PhotonNetwork.JoinOrCreateRoom("asd", roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        //PhotonNetwork.Instantiate(playerPrefub.name, new Vector3(0.4f, 1, 0), Quaternion.identity);
        //base.OnJoinedRoom();

        Debug.Log("OnJoinedRoom. Current players: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Update the UI for the newly joined local player
        UpdatePlayerLobbyUI();

        // Check if the room is full to load the next scene
        if (PhotonNetwork.IsMasterClient)
        {
            CheckRoomFullAndLoadScene();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("Player {0} entered the room. Total players: {1}", newPlayer.NickName, PhotonNetwork.CurrentRoom.PlayerCount);

        // Update the UI to show the new player
        UpdatePlayerLobbyUI();

        if (PhotonNetwork.IsMasterClient)
        {
            CheckRoomFullAndLoadScene();
        }
    }

    private void CheckRoomFullAndLoadScene()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayer)
        {
            Debug.Log("Room is full! Loading " + SongSelectionSceneName);
            // Switch scenes for all players
            PhotonNetwork.LoadLevel(SongSelectionSceneName);
        }
    }

    private void UpdatePlayerLobbyUI()
    {
        // Reset all slots
        ResetPlayerSlot(player1Panel, player1IllustImage);
        ResetPlayerSlot(player2Panel, player2IllustImage);

        // Populate based on current players
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        if (players.Length >= 1)
        {
            #pragma warning disable 4014
                FetchAndDisplayUserInfo(players[0], player1Panel);
            #pragma warning restore 4014
        }

        if (players.Length >= 2)
        {
            // Player 2 setup
            #pragma warning disable 4014
                FetchAndDisplayUserInfo(players[1], player2Panel);
            #pragma warning restore 4014
        }
    }

    public void JoiningRoom()
    {
        PhotonNetwork.JoinRandomRoom();

        //string roomName = "asd";
        //PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        // base.OnConnectedToMaster();

        string userId = PlayerPrefs.GetString(USER_ID_KEY, string.Empty);

        if (!string.IsNullOrEmpty(userId))
        {
            // Prepare the custom properties dictionary
            ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
            customProps.Add("UserID", userId);

            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        }
    }

    private async void FetchAndDisplayUserInfo(Player photonPlayer, GameObject playerPanel)
    {
        // UserID from the player's custom properties
        string userId = null;
        if (photonPlayer.CustomProperties.ContainsKey("UserID"))
        {
            userId = photonPlayer.CustomProperties["UserID"] as string;
        }

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError($"UserID not found for player: {photonPlayer.NickName}");
            return;
        }

        UserData dataToSend = new UserData { user_id = userId };
        string jsonString = JsonUtility.ToJson(dataToSend);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest request = UnityWebRequest.Get(receiveUrl))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    ServerResponse receivedData = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

                    string userName = receivedData.user_info.user_name;
                    int cardId = receivedData.user_info.home_card_id;

                    Image playerIllustImage = playerPanel.GetComponentInChildren<Image>();
                    DisplayPlayerIllust(playerIllustImage, cardId);

                    TextMeshProUGUI usernameText = playerPanel.GetComponentInChildren<TextMeshProUGUI>();
                    if (usernameText != null)
                    {
                        usernameText.text = userName;
                    }
                    else
                    {
                        Debug.LogError($"Could not find TMPro component in {playerPanel.name} for username update.");
                    }
                }
            }
            catch (System.Exception e)
            {
                // Catch any unexpected exceptions during the await operation
                Debug.LogError($"Exception during web request for {photonPlayer.NickName}: {e.Message}");
            }
        }
    }

    private void ResetPlayerSlot(GameObject playerPanel, Image playerIllust)
    {
        // Reset Text
        TextMeshProUGUI usernameText = playerPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (usernameText != null)
        {
            usernameText.text = "Waiting...";
        }

        // Reset Picture
        if (playerIllust != null && defaultAvatarSprite != null)
        {
            playerIllust.sprite = defaultAvatarSprite;
        }
    }

    public void DisplayPlayerIllust(Image targetImage, int cardId)
    {
        if (targetImage == null) return;

        string fileName = $"game_icon_{cardId}.png";
        PlayerCardIllustLoader.instance.LoadPlayerIllustration(targetImage, fileName);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected from server for reason: " + cause.ToString());
    }
}
