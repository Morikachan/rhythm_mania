using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public enum RoomPhase {
    SongList,
    Roulette,
    Preparation
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
    public Image Player1RulletImage;
    public Image Player2RulletImage;
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

    [Header("Colors")]
    public Color readyPink;

    private RoomPhase currentPhase = RoomPhase.SongList;

    private const string PROP_USERNAME = "UserName";
    private const string PROP_CARD_ID = "CardID";

    private void Start()
    {
        SetPhase(RoomPhase.SongList);
    }

    // PHASE

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
            RefreshPreparationUI();
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

        OnTimerEnded();
    }

    void OnTimerEnded()
    {
        if(currentPhase == RoomPhase.SongList)
            AutoRecommend();

        if(currentPhase == RoomPhase.Preparation)
            AutoReady();
    }

    // PHOTON

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(currentPhase == RoomPhase.SongList && AllPlayersSelected())
        {
            SetPhase(RoomPhase.Roulette);
            if(PhotonNetwork.IsMasterClient)
                ChooseFinalSong();
        }

        if(currentPhase == RoomPhase.Preparation && AllPlayersReady())
        {
            PhotonNetwork.LoadLevel("Multi_3D_Game");
        }

        if(currentPhase == RoomPhase.Preparation)
            RefreshPreparationUI();
    }

    // LOGIC

    bool AllPlayersSelected()
    {
        foreach(var p in PhotonNetwork.PlayerList)
        {
            if(!p.CustomProperties.ContainsKey("SelectState"))
                return false;

            string state = p.CustomProperties["SelectState"].ToString();
            if(state == "Selecting")
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
        ExitGames.Client.Photon.Hashtable props =
        new ExitGames.Client.Photon.Hashtable { { "SelectState", "Recommend" } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void AutoReady()
    {
        ExitGames.Client.Photon.Hashtable props =
        new ExitGames.Client.Photon.Hashtable { { "Ready", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void ChooseFinalSong()
    {
        Player[] players = PhotonNetwork.PlayerList;
        Player chosen = players[Random.Range(0, players.Length)];

        photonView.RPC(nameof(GoPreparation), RpcTarget.All,
            chosen.CustomProperties["SongName"].ToString(),
            chosen.CustomProperties["SongLevel"].ToString(),
            chosen.CustomProperties["SongBPM"].ToString()
        );
    }

    [PunRPC]
    void GoPreparation(string name, string level, string bpm)
    {
        songName.text = name;
        songLevel.text = level;
        songBPM.text = bpm;

        SetPhase(RoomPhase.Preparation);
    }

    // UI

    void RefreshRouletteUI()
    {
        Player[] p = PhotonNetwork.PlayerList;
        SetupRoulettePlayer(p[0], Player1RulletNick, Player1RulletStatus, Player1RulletImage);
        SetupRoulettePlayer(p[1], Player2RulletNick, Player2RulletStatus, Player2RulletImage);
    }

    void RefreshPreparationUI()
    {
        Player[] p = PhotonNetwork.PlayerList;

        SetupPrepPlayer(p[0], Player1PrepNick, Player1PrepImage, Player1PrepStatusPlate, Player1PrepStatusText);
        SetupPrepPlayer(p[1], Player2PrepNick, Player2PrepImage, Player2PrepStatusPlate, Player2PrepStatusText);
    }

    void SetupRoulettePlayer(Player p, TextMeshProUGUI nick, TextMeshProUGUI status, Image img)
    {
        nick.text = p.CustomProperties[PROP_USERNAME].ToString();
        status.text = p.CustomProperties["SelectState"].ToString();

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            img, $"game_icon_{(int)p.CustomProperties[PROP_CARD_ID]}.png");
    }

    void SetupPrepPlayer(Player p, TextMeshProUGUI nick, Image img, Image plate, TextMeshProUGUI status)
    {
        nick.text = p.CustomProperties[PROP_USERNAME].ToString();

        bool ready = p.CustomProperties.ContainsKey("Ready") && (bool)p.CustomProperties["Ready"];
        status.text = ready ? "Ready" : "Preparing...";
        if(ready) plate.color = readyPink;

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            img, $"game_icon_{(int)p.CustomProperties[PROP_CARD_ID]}.png");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left room → return to GameModeSelection");

        if(PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("GameModeSelection");
    }
}
