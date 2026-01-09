using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using IEnumerator = System.Collections.IEnumerator;

public class MultiLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Room")]
    [SerializeField] private byte maxPlayers = 2;
    [SerializeField] private string songSelectionSceneName = "MultiSelectSongScene";
    [SerializeField] private float sceneLoadDelay = 2f;

    [Header("Player Slots")]
    [SerializeField] private Image player1IllustImage;
    [SerializeField] private Image player2IllustImage;
    [SerializeField] private TextMeshProUGUI player1UsernameText;
    [SerializeField] private TextMeshProUGUI player2UsernameText;
    [SerializeField] private Sprite defaultAvatarSprite;

    [Header("Photon CustomProperties keys")]
    private const string PROP_USERNAME = "UserName";
    private const string PROP_CARD_ID = "CardID";

    private Coroutine loadSceneCoroutine;

    private void Start()
    {
        ResetAllSlots();

        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Photon is NOT connected. MultiLobby expects connection before scene.");
            return;
        }

        if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            UpdateLobbyUI();
        }
    }

    // PHOTON CALLBACKS

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        UpdateLobbyUI();
        TryStartSceneTransition();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No free rooms, creating one");

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
        UpdateLobbyUI();
        TryStartSceneTransition();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
        CancelSceneTransition();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateLobbyUI();
    }

    // SCENE TRANSITION

    private void TryStartSceneTransition()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            if (loadSceneCoroutine == null)
            {
                loadSceneCoroutine = StartCoroutine(LoadSongSelectionWithDelay());
            }
        }
    }

    private void CancelSceneTransition()
    {
        if (loadSceneCoroutine != null)
        {
            StopCoroutine(loadSceneCoroutine);
            loadSceneCoroutine = null;
        }
    }

    private IEnumerator LoadSongSelectionWithDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        PhotonNetwork.LoadLevel(songSelectionSceneName);
    }

    // UI CHANGES

    private void ResetAllSlots()
    {
        ResetSlot(player1UsernameText, player1IllustImage);
        ResetSlot(player2UsernameText, player2IllustImage);
    }

    private void UpdateLobbyUI()
    {
        ResetAllSlots();

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            if (i == 0)
                FillSlot(players[i], player1UsernameText, player1IllustImage);
            else if (i == 1)
                FillSlot(players[i], player2UsernameText, player2IllustImage);
        }
    }

    private void FillSlot(Player player, TextMeshProUGUI nameText, Image avatar)
    {
        if (player.CustomProperties.TryGetValue(PROP_USERNAME, out object username))
        {
            nameText.text = username.ToString();
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