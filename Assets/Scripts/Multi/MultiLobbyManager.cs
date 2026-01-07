using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MultiLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Room")]
    [SerializeField] private byte maxPlayers = 2;
    [SerializeField] private string songSelectionSceneName = "MultiSelectSongScene";

    [Header("Player Slots")]
    [SerializeField] private GameObject player1Panel;
    [SerializeField] private GameObject player2Panel;

    [SerializeField] private Image player1IllustImage;
    [SerializeField] private Image player2IllustImage;

    [SerializeField] private TextMeshProUGUI player1UsernameText;
    [SerializeField] private TextMeshProUGUI player2UsernameText;

    [SerializeField] private Sprite defaultAvatarSprite;

    [Header("PlayerPrefs keys")]
    private const string USER_NAME_KEY = "UserName";
    private const string HOME_CARD_ID_KEY = "HomeCardID";

    [Header("Photon CustomProperties keys")]
    private const string PROP_USERNAME = "UserName";
    private const string PROP_CARD_ID = "CardID";

    private void Start()
    {
        Debug.Log("Start at MultiLobbyManager");
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = "0.0.1";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // ~~~~~~~~~~~~~~ PHOTON ~~~~~~~~~~~~~~

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        string userName = PlayerPrefs.GetString(USER_NAME_KEY, "Player");
        int cardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY, 1);

        Debug.Log(userName + "+ " + cardId.ToString());

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { PROP_USERNAME, userName },
            { PROP_CARD_ID, cardId }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // TODO: To instance or to call it every time when connecting to room
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        UpdateLobbyUI();
        TryLoadSongSelection();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No free rooms, creating new one...");

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player entered: {newPlayer.ActorNumber}");
        UpdateLobbyUI();
        TryLoadSongSelection();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player left: {otherPlayer.ActorNumber}");
        UpdateLobbyUI();
    }

    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true
        };

        PhotonNetwork.CreateRoom(null, options);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    // ~~~~~~~~~~~~~~ SCENE ~~~~~~~~~~~~~~
    private void TryLoadSongSelection()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            PhotonNetwork.LoadLevel(songSelectionSceneName);
        }
    }

    // ~~~~~~~~~~~~~~ UI ~~~~~~~~~~~~~~

    private void UpdateLobbyUI()
    {
        ResetSlot(player1UsernameText, player1IllustImage);
        ResetSlot(player2UsernameText, player2IllustImage);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == 1)
            {
                FillSlot(player, player1UsernameText, player1IllustImage);
            }
            else if (player.ActorNumber == 2)
            {
                FillSlot(player, player2UsernameText, player2IllustImage);
            }
        }
    }

    private void FillSlot(Player player, TextMeshProUGUI nameText, Image avatar)
    {
        if (player.CustomProperties.TryGetValue(PROP_USERNAME, out object name))
        {
            nameText.text = name.ToString();
        }

        if (player.CustomProperties.TryGetValue(PROP_CARD_ID, out object cardId))
        {
            DisplayPlayerIllust(avatar, (int)cardId);
        }
    }

    private void ResetSlot(TextMeshProUGUI nameText, Image avatar)
    {
        nameText.text = "Waiting...";
        avatar.sprite = defaultAvatarSprite;
    }

    private void DisplayPlayerIllust(Image targetImage, int cardId)
    {
        if (targetImage == null) return;

        string fileName = $"game_icon_{cardId}.png";
        PlayerCardIllustLoader.instance.LoadPlayerIllustration(targetImage, fileName);
    }
}