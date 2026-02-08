using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using IEnumerator = System.Collections.IEnumerator;

public class MultiLobbyManager : MonoBehaviourPunCallbacks {
    [Header("Room Settings")]
    [SerializeField] private byte maxPlayers = 2;
    [SerializeField] private string songSelectionSceneName = "MultiSelectSongScene";
    [SerializeField] private float sceneLoadDelay = 2f;

    [Header("UI Slots")]
    [SerializeField] private Image player1IllustImage;
    [SerializeField] private Image player2IllustImage;
    [SerializeField] private TextMeshProUGUI player1UsernameText;
    [SerializeField] private TextMeshProUGUI player2UsernameText;
    [SerializeField] private Sprite defaultAvatarSprite;

    private const string PROP_USERNAME = "UserName";
    private const string PROP_CARD_ID = "CardID";

    private Coroutine loadSceneCoroutine;
    private bool isRetryingToCreate = false;

    private void Start()
    {
        ResetAllSlots();
        Time.timeScale = 1;

        SyncPlayerPropertiesWithPrefs();

        if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScreen");
            return;
        }

        if(PhotonNetwork.InRoom)
        {
            OnJoinedRoom();
        }
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        if(!PhotonNetwork.CurrentRoom.IsOpen || !PhotonNetwork.CurrentRoom.IsVisible)
        {
            isRetryingToCreate = true;
            PhotonNetwork.LeaveRoom();
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;

        ResetMyPlayerProps();

        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            CleanupRoomProperties();
        }

        UpdateLobbyUI();
        TryStartSceneTransition();
    }

    public override void OnConnectedToMaster()
    {
        if(isRetryingToCreate)
        {
            isRetryingToCreate = false;
            CreateRoom();
        }
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    private void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CleanupCacheOnLeave = true,
            EmptyRoomTtl = 0
        };

        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLobbyUI();
        TryStartSceneTransition();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
        CancelSceneTransition();

        if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            CleanupRoomProperties();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateLobbyUI();
    }

    private void CleanupRoomProperties()
    {
        if(!PhotonNetwork.IsMasterClient) return;

        Hashtable cleanProps = new Hashtable
        {
            { "FinalSongID", -1 },
            { "FinalSongName", "" },
            { "WinnerIndex", -1 }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(cleanProps);
    }

    private void TryStartSceneTransition()
    {
        if(!PhotonNetwork.IsMasterClient) return;

        if(PhotonNetwork.CurrentRoom.PlayerCount >= maxPlayers)
        {
            if(loadSceneCoroutine == null)
            {
                loadSceneCoroutine = StartCoroutine(LoadSongSelectionWithDelay());
            }
        }
    }

    private void CancelSceneTransition()
    {
        if(loadSceneCoroutine != null && PhotonNetwork.CurrentRoom.PlayerCount < maxPlayers)
        {
            StopCoroutine(loadSceneCoroutine);
            loadSceneCoroutine = null;
        }
    }

    private IEnumerator LoadSongSelectionWithDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= maxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel(songSelectionSceneName);
        }
        else
        {
            loadSceneCoroutine = null;
        }
    }

    public void SyncPlayerPropertiesWithPrefs()
    {
        if (PhotonNetwork.IsConnected)
        {
            string userName = PlayerPrefs.GetString("UserName", "Player");
            int cardId = PlayerPrefs.GetInt("HomeCardID", 1);

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "UserName", userName },
            { "CardID", cardId }
        };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            PhotonNetwork.LocalPlayer.NickName = userName;
        }
    }
    private void ResetMyPlayerProps()
    {
        Hashtable props = new Hashtable
        {
            { "Ready", false },
            { "SelectState", "Selecting" },
            { "SongID", -1 },
            { "SongName", "" },
            { "SongLevel", "" },
            { "SongBPM", "" },
            { "Score", 0 },
            { "Combo", 0 }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void ResetAllSlots()
    {
        ResetSlot(player1UsernameText, player1IllustImage);
        ResetSlot(player2UsernameText, player2IllustImage);
    }

    private void UpdateLobbyUI()
    {
        ResetAllSlots();

        Player[] players = PhotonNetwork.PlayerList;
        // System.Array.Sort(players, (p1, p2) => p1.ActorNumber.CompareTo(p2.ActorNumber));

        for(int i = 0; i < players.Length; i++)
        {
            if(i == 0) FillSlot(players[i], player1UsernameText, player1IllustImage);
            else if(i == 1) FillSlot(players[i], player2UsernameText, player2IllustImage);
        }
    }

    private void FillSlot(Player player, TextMeshProUGUI nameText, Image avatar)
    {
        if(player.CustomProperties.TryGetValue(PROP_USERNAME, out object username))
        {
            nameText.text = username.ToString();
        }
        else
        {
            nameText.text = !string.IsNullOrEmpty(player.NickName) ? player.NickName : "Loading...";
        }

        if(player.CustomProperties.TryGetValue(PROP_CARD_ID, out object cardId))
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
        if(targetImage == null) return;
        string fileName = $"game_icon_{cardId}.png";

        if(PlayerCardIllustLoader.instance != null)
            PlayerCardIllustLoader.instance.LoadPlayerIllustration(targetImage, fileName);
    }
}