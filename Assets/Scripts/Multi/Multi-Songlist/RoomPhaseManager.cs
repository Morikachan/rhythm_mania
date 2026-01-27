using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using static SongListManager;
using System;
using System.Collections.Generic;

public enum RoomPhase {
    SongList,
    Roulette,
    Preparation
}

enum SelectResultType {
    TwoSelected,
    OneSelectedOneRecommend,
    TwoRecommend
}

public class RoomPhaseManager : MonoBehaviourPunCallbacks {
    [Header("UI Roots")]
    public GameObject songListUI;
    public GameObject rouletteUI;
    public GameObject preparationUI;

    [Header("Timers")]
    public TextMeshProUGUI songListTimerText;
    public TextMeshProUGUI preparationTimerText;
    private float timer = 30f;
    private Coroutine timerCoroutine;

    [Header("Roulette Player UI")]
    public GameObject Player1RulletPanel;
    public GameObject Player2RulletPanel;
    public Image Player1RulletCardImage;
    public Image Player2RulletCardImage;
    public Image Player1RulletSongImage;
    public Image Player2RulletSongImage;

    public TextMeshProUGUI Player1RulletNick;
    public TextMeshProUGUI Player2RulletNick;
    public TextMeshProUGUI Player1RulletStatus;
    public TextMeshProUGUI Player2RulletStatus;

    [Header("Preparation Player UI")]
    public Image Player1PrepImage;
    public Image Player2PrepImage;
    public Image Player1PrepStatusPlate;
    public Image Player2PrepStatusPlate;

    public TextMeshProUGUI Player1PrepNick;
    public TextMeshProUGUI Player2PrepNick;
    public TextMeshProUGUI Player1PrepStatusText;
    public TextMeshProUGUI Player2PrepStatusText;

    [Header("Preparation Song Info")]
    public TextMeshProUGUI songName;
    public TextMeshProUGUI songLevel;
    public TextMeshProUGUI songBPM;
    public Image songIllust;

    [Header("All Songs")]
    public List<string> allSongNames;

    [Header("Colors")]
    public Color readyPink;

    private RoomPhase currentPhase = RoomPhase.SongList;
    private SampleSongManager songManager;

    [Header("Song Source")]
    public SongListManager songListManager;

    private bool decisionMade = false;

    private const string PROP_USERNAME = "UserName";
    private const string PROP_CARD_ID = "CardID";

    void Start()
    {
        if(photonView == null)
            Debug.LogError("PhotonView STILL NULL");

        songManager = FindObjectOfType<SampleSongManager>();
        SetPhase(RoomPhase.SongList);

        RefreshRouletteUI();
        RefreshPreparationUI();
    }

    //  PHASE 

    void SetPhase(RoomPhase phase)
    {
        currentPhase = phase;

        songListUI.SetActive(phase == RoomPhase.SongList);
        rouletteUI.SetActive(phase == RoomPhase.Roulette);
        preparationUI.SetActive(phase == RoomPhase.Preparation);

        RestartTimer();

        if(phase == RoomPhase.Roulette)
            RefreshRouletteUI();

        if(phase == RoomPhase.Preparation)
        {
            RefreshPreparationUI();
        }
    }

    void RestartTimer()
    {
        if(timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timer = 30f;
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    IEnumerator TimerRoutine()
    {
        while(timer > 0)
        {
            timer -= Time.deltaTime;

            if(currentPhase == RoomPhase.SongList)
                songListTimerText.text = Mathf.CeilToInt(timer).ToString();

            if(currentPhase == RoomPhase.Preparation)
                preparationTimerText.text = Mathf.CeilToInt(timer).ToString();

            yield return null;
        }

        if(currentPhase == RoomPhase.SongList)
            AutoRecommend();

        if(currentPhase == RoomPhase.Preparation)
            AutoReady();
    }

    // PHOTON

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if(!gameObject.activeInHierarchy)
            return;

        if(currentPhase == RoomPhase.SongList || currentPhase == RoomPhase.Roulette)
            RefreshRouletteUI();

        if(currentPhase == RoomPhase.Preparation)
            RefreshPreparationUI();

        if(currentPhase == RoomPhase.SongList && AllPlayersSelected())
        {
            if(decisionMade)
                return;

            decisionMade = true;

            SetPhase(RoomPhase.Roulette);
            ShowRouletteLocally();

            if(!PhotonNetwork.IsMasterClient)
                return;

            DecideFinalSong();
        }

        if(currentPhase == RoomPhase.Preparation && AllPlayersReady())
            StartCoroutine(ChangeToGameSceneWithDalay());
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if(propertiesThatChanged.ContainsKey("FinalSongID"))
        {
            int songId = (int)propertiesThatChanged["FinalSongID"];
            StartCoroutine(ShowFinalSong(songId));
        }
    }

    IEnumerator ShowFinalSong(int songId)
    {
        yield return new WaitForSeconds(1f);

        Song song = songListManager.GetSongById(songId);

        photonView.RPC(
            nameof(RPC_ShowWinner),
            RpcTarget.All,
            -1,
            song.song_name,
            song.song_level.ToString(),
            song.song_bpm.ToString(),
            song.song_id
        );

        yield return new WaitForSeconds(2f);
        photonView.RPC(nameof(RPC_GoPreparation), RpcTarget.All);
    }

    //  ROULETTE FLOW

    void DecideFinalSong()
    {
        Player selectedPlayer;
        SelectResultType result = GetSelectResult(out selectedPlayer);

        PhotonHashtable roomProps = new PhotonHashtable();

        switch(result)
        {
            case SelectResultType.OneSelectedOneRecommend:
                roomProps["FinalSongID"] =
                    (int)selectedPlayer.CustomProperties["SongID"];
                break;

            case SelectResultType.TwoSelected:
                Player[] players = PhotonNetwork.PlayerList;
                Player winner = players[UnityEngine.Random.Range(0, players.Length)];
                roomProps["FinalSongID"] =
                    (int)winner.CustomProperties["SongID"];
                break;

            case SelectResultType.TwoRecommend:
                List<Song> allSongs = songListManager.GetAllSongs();
                Song randomSong = allSongs[UnityEngine.Random.Range(0, allSongs.Count)];
                roomProps["FinalSongID"] = randomSong.song_id;
                break;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    SelectResultType GetSelectResult(out Player selectedPlayer)
    {
        selectedPlayer = null;

        Player[] players = PhotonNetwork.PlayerList;
        if(players.Length < 2)
            return SelectResultType.TwoRecommend;

        int selectedCount = 0;
        int recommendCount = 0;

        foreach(var p in players)
        {
            string state = p.CustomProperties["SelectState"].ToString();
            if(state == "Selected")
            {
                selectedCount++;
                selectedPlayer = p;
            }
            else if(state == "Recommend")
                recommendCount++;
        }

        if(selectedCount == 2)
            return SelectResultType.TwoSelected;

        if(selectedCount == 1 && recommendCount == 1)
            return SelectResultType.OneSelectedOneRecommend;

        return SelectResultType.TwoRecommend;
    }

    IEnumerator RouletteFlow()
    {

        yield return new WaitForSeconds(1f);

        if(PhotonNetwork.PlayerList.Length < 2)
            yield break;

        if(photonView == null)
        {
            Debug.LogError("RoomPhaseManager: photonView == null");
            yield break;
        }

        photonView.RPC(nameof(RPC_StopMusic), RpcTarget.All);

        yield return new WaitForSeconds(2f);

        Player[] players = PhotonNetwork.PlayerList;
        int winnerIndex = UnityEngine.Random.Range(0, players.Length);
        Player winner = players[winnerIndex];

        if(winner == null)
            yield break;

        string name = winner.CustomProperties.ContainsKey("SongName")
            ? winner.CustomProperties["SongName"].ToString()
            : "Unknown";

        string level = winner.CustomProperties.ContainsKey("SongLevel")
            ? winner.CustomProperties["SongLevel"].ToString()
            : "-";

        string bpm = winner.CustomProperties.ContainsKey("SongBPM")
            ? winner.CustomProperties["SongBPM"].ToString()
            : "-";

        int songID = winner.CustomProperties.ContainsKey("SongID")
            ? Int32.Parse(winner.CustomProperties["SongID"].ToString())
            : 1;

        photonView.RPC(
            nameof(RPC_ShowWinner),
            RpcTarget.All,
            winnerIndex,
            name,
            level,
            bpm,
            songID
        );

        yield return new WaitForSeconds(2f);
        photonView.RPC(nameof(RPC_GoPreparation), RpcTarget.All);
    }

    IEnumerator DirectWinnerFlow(Player winner)
    {
        yield return new WaitForSeconds(0.5f);

        photonView.RPC(nameof(RPC_StopMusic), RpcTarget.All);

        yield return new WaitForSeconds(0.5f);

        int winnerIndex = Array.IndexOf(PhotonNetwork.PlayerList, winner);

        photonView.RPC(
            nameof(RPC_ShowWinner),
            RpcTarget.All,
            winnerIndex,
            winner.CustomProperties["SongName"].ToString(),
            winner.CustomProperties["SongLevel"].ToString(),
            winner.CustomProperties["SongBPM"].ToString(),
            Int32.Parse(winner.CustomProperties["SongID"].ToString())
        );

        yield return new WaitForSeconds(2f);
        photonView.RPC(nameof(RPC_GoPreparation), RpcTarget.All);
    }

    IEnumerator RandomFromAllSongsFlow()
    {
        yield return new WaitForSeconds(1f);

        photonView.RPC(nameof(RPC_StopMusic), RpcTarget.All);

        yield return new WaitForSeconds(0.5f);

        List<Song> allSongs = songListManager.GetAllSongs();

        if(allSongs == null || allSongs.Count == 0)
            yield break;

        //Song randomSong = allSongs[UnityEngine.Random.Range(0, allSongs.Count)];
        // TEST: ONLY 2 songs
        int max = Mathf.Min(2, allSongs.Count);
        Song randomSong = allSongs[UnityEngine.Random.Range(0, max)];

        photonView.RPC(
            nameof(RPC_ShowWinner),
            RpcTarget.All,
            -1, // not player
            randomSong.song_name,
            randomSong.song_level.ToString(),
            randomSong.song_bpm.ToString(),
            randomSong.song_id
        );

        yield return new WaitForSeconds(2f);
        photonView.RPC(nameof(RPC_GoPreparation), RpcTarget.All);
    }

    IEnumerator ChangeToGameSceneWithDalay()
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.LoadLevel("Multi_3D-game");
    }

    [PunRPC]
    void RPC_StopMusic()
    {
        if(songManager == null)
            songManager = FindObjectOfType<SampleSongManager>();

        if(songManager == null)
            return;

        AudioSource src = songManager.GetComponent<AudioSource>();
        if(src != null && src.isPlaying)
            src.Stop();
    }

    [PunRPC]
    void RPC_ShowWinner(int winnerIndex, string name, string level, string bpm, int songId)
    {
        Player1RulletPanel.gameObject.SetActive(winnerIndex == 0);
        Player2RulletPanel.gameObject.SetActive(winnerIndex == 1);

        songManager.PlayMusic(name);

        songName.text = name;
        songLevel.text = level;
        songBPM.text = bpm;
        DisplaySongIllust(songId, songIllust);
    }

    [PunRPC]
    void RPC_GoPreparation()
    {
        SetPhase(RoomPhase.Preparation);
    }

    //  LOGIC 

    bool AllPlayersSelected()
    {
        foreach(var p in PhotonNetwork.PlayerList)
        {
            if(!p.CustomProperties.ContainsKey("SelectState"))
                return false;

            if(p.CustomProperties["SelectState"].ToString() == "Selecting")
                return false;
        }
        return true;
    }

    bool AllPlayersReady()
    {
        foreach(var p in PhotonNetwork.PlayerList)
        {
            if(!p.CustomProperties.ContainsKey("Ready") || !(bool)p.CustomProperties["Ready"])
                return false;
        }
        return true;
    }

    void AutoRecommend()
    {
        PhotonHashtable props = new PhotonHashtable { { "SelectState", "Recommend" } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void AutoReady()
    {
        PhotonHashtable props = new PhotonHashtable { { "Ready", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    //  UI 

    public void ShowRouletteLocally()
    {
        songListUI.SetActive(false);
        rouletteUI.SetActive(true);
    }

    void RefreshRouletteUI()
    {
        Player[] players = PhotonNetwork.PlayerList;

        if(players.Length > 0)
            SetupRoulettePlayerSafe(players[0],
                Player1RulletNick, Player1RulletStatus, Player1RulletCardImage, Player1RulletSongImage);

        if(players.Length > 1)
            SetupRoulettePlayerSafe(players[1],
                Player2RulletNick, Player2RulletStatus, Player2RulletCardImage, Player2RulletSongImage);
    }

    void RefreshPreparationUI()
    {
        Player[] players = PhotonNetwork.PlayerList;

        if(players.Length > 0)
            SetupPrepPlayer(players[0], 
                Player1PrepNick, Player1PrepImage, Player1PrepStatusPlate, Player1PrepStatusText);

        if(players.Length > 1)
            SetupPrepPlayer(players[1], 
                Player2PrepNick, Player2PrepImage, Player2PrepStatusPlate, Player2PrepStatusText);
    }

    void SetupRoulettePlayerSafe(
    Player p,
    TextMeshProUGUI nick,
    TextMeshProUGUI status,
    Image cardImg,
    Image songImg)
    {
        if(p == null) return;

        if(p.CustomProperties.ContainsKey(PROP_USERNAME))
            nick.text = p.CustomProperties[PROP_USERNAME].ToString();
        else
            nick.text = "Player";

        string state = p.CustomProperties.ContainsKey("SelectState")
            ? p.CustomProperties["SelectState"].ToString()
            : "Selecting";

        if(state == "Selected" && p.CustomProperties.ContainsKey("SongName")) { 
            status.text = p.CustomProperties["SongName"].ToString();
            DisplaySongIllust(Int32.Parse(p.CustomProperties["SongID"].ToString()), songImg);
        }
        else if(state == "Recommend")
            status.text = "Recommended";
        else
            status.text = "Selecting...";

        if(p.CustomProperties.ContainsKey(PROP_CARD_ID))
        {
            PlayerCardIllustLoader.instance.LoadPlayerIllustration(
                cardImg,
                $"game_icon_{(int)p.CustomProperties[PROP_CARD_ID]}.png"
            );
        }
    }

    void SetupRoulettePlayer(Player p, TextMeshProUGUI nick, TextMeshProUGUI status, Image img)
    {
        nick.text = p.CustomProperties[PROP_USERNAME].ToString();

        string state = p.CustomProperties["SelectState"].ToString();

        if(state == "Selected" && p.CustomProperties.ContainsKey("SongName"))
            status.text = p.CustomProperties["SongName"].ToString();
        else if(state == "Recommend")
            status.text = "Recommended";
        else
            status.text = "Selecting...";

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            img, $"game_icon_{(int)p.CustomProperties[PROP_CARD_ID]}.png");
    }

    void SetupPrepPlayer(Player p, TextMeshProUGUI nick, Image img, Image plate, TextMeshProUGUI status)
    {
        nick.text = p.CustomProperties[PROP_USERNAME].ToString();
        bool ready = p.CustomProperties.ContainsKey("Ready") && (bool)p.CustomProperties["Ready"];
        status.text = ready ? "Ready" : "Preparing...";
        if(ready) plate.color = readyPink;
        if(ready) status.color = Color.white;

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            img, $"game_icon_{(int)p.CustomProperties[PROP_CARD_ID]}.png");
    }

    public void DisplaySongIllust(int songId, Image img)
    {
        if(img == null) return;

        string fileName = $"song_illust_{songId}.png";
        SongIllustLoader.instance.LoadSongIllustration(img, fileName);
    }

    //  LEAVE ROOM 

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("GameModeSelection");
    }
}
