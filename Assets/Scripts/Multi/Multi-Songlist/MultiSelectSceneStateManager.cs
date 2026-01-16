using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MultiSelectSceneStateManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject songListUI;              // SongList + info
    public GameObject multiplayerRouletteUI;  // Roulette + statuses
    public GameObject preparationUI;           // Ready / song info

    private const string PROP_STATE = "SelectState";
    private const string PROP_ROOM_PHASE = "RoomPhase";

    private const string PROP_SONG = "SongID";
    private const string PROP_PHASE = "RoomPhase";
    private const string PROP_ROULETTE = "RouletteType";

    void Start()
    {
        InitLocalPlayer();
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
        TryResolveState();
    }

    // INIT
    void InitLocalPlayer()
    {
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PROP_STATE))
        {
            SetMyState("Selecting", -1);
        }
    }

    // BUTTON API
    public void OnClickSelect()
    {
        SetMyState("Selected", SongDataHolder.instance.SelectedSongId);
    }

    public void OnClickRecommend()
    {
        SetMyState("Recommend", SongDataHolder.instance.SelectedSongId);
    }

    // CORE LOGIC
    void TryResolveState()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (GetRoomPhase() != "Selecting") return;

        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length < 2) return;

        string s1 = GetState(players[0]);
        string s2 = GetState(players[1]);

        // auto Recommend
        if (s1 == "Selecting" || s2 == "Selecting")
        {
            ForceAutoRecommend(players);
            return;
        }

        // resolve roulette type
        if (s1 == "Selected" && s2 == "Selected")
        {
            SetRoomPhase("Roulette", "Normal");
        }
        else if (
            (s1 == "Selected" && s2 == "Recommend") ||
            (s1 == "Recommend" && s2 == "Selected")
        )
        {
            SetRoomPhase("Roulette", "Fast");
        }
        else if (s1 == "Recommend" && s2 == "Recommend")
        {
            ForceRandomSong(players);
            SetRoomPhase("Roulette", "Random");
        }
    }

    // HELPERS
    void SetMyState(string state, int songId)
    {
        Hashtable props = new Hashtable
        {
            { PROP_STATE, state },
            { PROP_SONG, songId }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    string GetState(Player p)
    {
        if (p.CustomProperties.TryGetValue(PROP_STATE, out object v))
            return v.ToString();
        return "Selecting";
    }

    string GetRoomPhase()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties
            .TryGetValue(PROP_PHASE, out object v))
            return v.ToString();
        return "Selecting";
    }

    void SetRoomPhase(string phase, string rouletteType)
    {
        Hashtable props = new Hashtable
        {
            { PROP_PHASE, phase },
            { PROP_ROULETTE, rouletteType }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    // SPECIAL CASES
    void ForceAutoRecommend(Player[] players)
    {
        foreach (Player p in players)
        {
            if (GetState(p) == "Selecting")
            {
                Hashtable props = new Hashtable
                {
                    { PROP_STATE, "Recommend" },
                    { PROP_SONG, GetRandomSongId() }
                };
                p.SetCustomProperties(props);
            }
        }
    }

    void ForceRandomSong(Player[] players)
    {
        int randomSong = GetRandomSongId();
        foreach (Player p in players)
        {
            Hashtable props = new Hashtable
            {
                { PROP_SONG, randomSong }
            };
            p.SetCustomProperties(props);
        }
    }

    int GetRandomSongId()
    {
        return Random.Range(1, 10);
    }

    // UI LOGIC (LOCAL)
    void UpdateUI()
    {
        Player me = PhotonNetwork.LocalPlayer;
        string myState = GetPlayerState(me);
        string roomPhase = GetRoomPhase();

        // --- Song Selection Phase ---
        if (roomPhase == "Selecting")
        {
            songListUI.SetActive(myState == "Selecting");
            multiplayerRouletteUI.SetActive(myState == "Selected");
            preparationUI.SetActive(false);
        }

        // --- Roulette Phase ---
        else if (roomPhase == "Roulette")
        {
            songListUI.SetActive(false);
            multiplayerRouletteUI.SetActive(true);
            preparationUI.SetActive(false);
        }

        // --- Preparation Phase ---
        else if (roomPhase == "Preparation")
        {
            songListUI.SetActive(false);
            multiplayerRouletteUI.SetActive(false);
            preparationUI.SetActive(true);
        }
    }

    // GLOBAL PHASE LOGIC
    void TryChangeRoomPhase()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        string roomPhase = GetRoomPhase();

        if (roomPhase == "Selecting" && AreAllPlayersSelected())
        {
            SetRoomPhase("Roulette");
        }
    }

    bool AreAllPlayersSelected()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (GetPlayerState(p) != "Selected")
                return false;
        }
        return true;
    }

    // HELPERS
    string GetPlayerState(Player p)
    {
        if (p.CustomProperties.TryGetValue(PROP_STATE, out object value))
            return value.ToString();

        return "Selecting";
    }

    void SetRoomPhase(string phase)
    {
        Hashtable props = new Hashtable
        {
            { PROP_ROOM_PHASE, phase }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}
