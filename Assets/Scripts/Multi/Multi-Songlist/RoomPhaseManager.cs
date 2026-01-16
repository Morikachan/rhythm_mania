using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class RoomPhaseManager : MonoBehaviourPunCallbacks
{
    public enum RoomPhase
    {
        Selecting,
        Roulette,
        Preparation,
        Game
    }

    private const string PROP_ROOM_PHASE = "RoomPhase";
    private const string PROP_SELECT_END_TIME = "SelectEndTime";
    private const string PROP_READY = "Ready";

    [Header("Timer UI (SongListUI)")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private int selectDuration = 30;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "3D-Game";
    [SerializeField] private string backSceneName = "GameModeSelection";

    void Start()
    {
        InitRoom();
    }

    void Update()
    {
        UpdateSelectingTimer();
        HandlePreparationPhase();
    }

    // INIT
    void InitRoom()
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.IsMasterClient)
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(PROP_ROOM_PHASE))
            {
                double endTime = PhotonNetwork.Time + selectDuration;

                Hashtable props = new Hashtable
                {
                    { PROP_ROOM_PHASE, RoomPhase.Selecting.ToString() },
                    { PROP_SELECT_END_TIME, endTime }
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }

    // TIMER (Selecting)
    void UpdateSelectingTimer()
    {
        if (timerText == null) return;
        if (GetRoomPhase() != RoomPhase.Selecting) return;

        if (!PhotonNetwork.CurrentRoom.CustomProperties
            .TryGetValue(PROP_SELECT_END_TIME, out object endObj))
            return;

        double endTime = (double)endObj;
        double remaining = endTime - PhotonNetwork.Time;

        if (remaining < 0) remaining = 0;

        timerText.text = Mathf.CeilToInt((float)remaining).ToString();

        // time over → auto recommend
        if (remaining <= 0 && PhotonNetwork.IsMasterClient)
        {
            ForceAutoRecommendAll();
        }
    }

    void ForceAutoRecommendAll()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("SelectState"))
            {
                Hashtable props = new Hashtable
                {
                    { "SelectState", "Recommend" },
                    { "SongID", Random.Range(1, 20) }
                };
                p.SetCustomProperties(props);
            }
        }
    }

    // PREPARATION → GAME
    void HandlePreparationPhase()
    {
        if (GetRoomPhase() != RoomPhase.Preparation) return;
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!IsReady(p))
                return;
        }

        PhotonNetwork.LoadLevel(gameSceneName);
    }

    bool IsReady(Player p)
    {
        if (p.CustomProperties.TryGetValue(PROP_READY, out object value))
            return (bool)value;

        return false;
    }

    // GETTERS
    RoomPhase GetRoomPhase()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties
            .TryGetValue(PROP_ROOM_PHASE, out object value))
        {
            return (RoomPhase)System.Enum.Parse(typeof(RoomPhase), value.ToString());
        }

        return RoomPhase.Selecting;
    }

    // READY API
    public void SetLocalPlayerReady(bool ready)
    {
        Hashtable props = new Hashtable
        {
            { PROP_READY, ready }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // PLAYER DISCONNECT
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // ВСЕ ВЫХОДЯТ ИЗ КОМНАТЫ
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(backSceneName);
    }
}
