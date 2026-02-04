using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MultiGameManager : MonoBehaviourPunCallbacks, INoteSpeedProvider
{
    public static MultiGameManager instance;

    [Header("Players")]
    public Dictionary<int, PlayerRuntimeData> players =
        new Dictionary<int, PlayerRuntimeData>();

    public int localActor;

    [Header("UI")]
    public Image player1Icon;
    public Image player2Icon;

    public TextMeshProUGUI player1Combo;
    public TextMeshProUGUI player2Combo;
    public GameObject player1YouPanel;
    public GameObject player2YouPanel;

    public Slider localPlayerHP;

    public GameObject finishText;

    [Header("Game Settings")]
    public float noteSpeed = 5f;
    public float startDelay = 2f; // Delay before start
    public float startTime;

    private bool finishSent = false;
    private double songEndDspTime = -1;

    [Header("References")]
    [SerializeField] public TextMeshProUGUI comboText;
    [SerializeField] public TextMeshProUGUI scoreText;
    public MultiNotesManager notesManager;
    public MusicManager musicManager;

    public GameObject keyLine;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SongDataHolder.instance.SetMultiLive(true);

        bool synced = SongDataHolder.instance.SyncFromRoom();

        musicManager.SetSongByName(
            SongDataHolder.instance.SelectedSongName
        );

        if(!synced)
        {
            Debug.LogError("SONG SYNC FAILED: GAME ABORTED");
            return;
        }

        InitPlayers();

        StartCoroutine(StartGameAfterDelay());
    }

    void Update()
    {
        if(!PhotonNetwork.IsMasterClient) return;

        if(finishSent) return;

        if(songEndDspTime < 0)
            return;

        if(AudioSettings.dspTime >= songEndDspTime)
        {
            finishSent = true;
            photonView.RPC(nameof(RPC_FinishGame), RpcTarget.All);
        }
    }

    public float GetNoteSpeed()
    {
        return noteSpeed;
    }

    IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        if (keyLine != null)
            keyLine.SetActive(false);

        startTime = Time.time;

        musicManager.ResetMusic();

        float musicDelay = notesManager.spawnOffset / noteSpeed;
        musicManager.PlayMusic(musicDelay);

        if(PhotonNetwork.IsMasterClient)
        {
            double startDsp = AudioSettings.dspTime + musicDelay;
            songEndDspTime = startDsp + musicManager.audioSource.clip.length;
        }

        notesManager.StartGame();
    }

    void InitPlayers()
    {
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            players[p.ActorNumber] = new PlayerRuntimeData
            {
                actorNumber = p.ActorNumber,
                hp = 1000
            };
        }

        localActor = PhotonNetwork.LocalPlayer.ActorNumber;

        SetupPlayerIcons();
        UpdateLocalHP();
        SetupComboVisibility();
    }

    void SetupPlayerIcons()
    {
        Player[] p = PhotonNetwork.PlayerList;

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            player1Icon,
            $"game_icon_{(int)p[0].CustomProperties["CardID"]}.png"
        );

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            player2Icon,
            $"game_icon_{(int)p[1].CustomProperties["CardID"]}.png"
        );
    }

    public void ResetCombo(int actor)
    {
        players[actor].combo = 0;

        UpdateScoreAndComboUI();
    }

    //  HP 

    public void Damage(int actor, int value)
    {
        var data = players[actor];
        data.hp -= value;
        data.hp = Mathf.Max(0, data.hp);

        if(data.hp == 0)
            data.multiplier = 0;

        UpdateLocalHP();
    }

    //  UI 

    void UpdateLocalHP()
    {
        var data = players[localActor];

        localPlayerHP.maxValue = data.maxHP;
        localPlayerHP.value = data.hp;
    }

    void UpdateScoreAndComboUI()
    {
        // LOCAL PLAYER
        var localData = players[localActor];

        comboText.text = localData.combo.ToString();
        scoreText.text = localData.score.ToString();

        // TOP UI
        foreach(var pair in players)
        {
            int actor = pair.Key;
            int combo = pair.Value.combo;

            if(actor == PhotonNetwork.PlayerList[0].ActorNumber)
                player1Combo.text = combo.ToString();
            else if(actor == PhotonNetwork.PlayerList[1].ActorNumber)
                player2Combo.text = combo.ToString();
        }
    }

    bool IsLocalPlayerIndex(int index)
    {
        Player[] p = PhotonNetwork.PlayerList;
        if(p.Length <= index) return false;

        return p[index].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
    }

    void SetupComboVisibility()
    {
        // Player1 -> player1Combo
        if(IsLocalPlayerIndex(0))
        {
            player1YouPanel.SetActive(true);
            player2YouPanel.SetActive(false);

            player1Combo.gameObject.SetActive(false);
            player2Combo.gameObject.SetActive(true);
        }
        // Player2 -> player2Combo
        else if(IsLocalPlayerIndex(1))
        {
            player1YouPanel.SetActive(false);
            player2YouPanel.SetActive(true);

            player1Combo.gameObject.SetActive(true);
            player2Combo.gameObject.SetActive(false);
        }
    }

    // RPC
    [PunRPC]
    void RPC_FinishGame()
    {
        if(musicManager.finishText != null)
            musicManager.finishText.SetActive(true);

        StartCoroutine(FinishRoutine());
    }

    IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(1f);
        EndGame();
    }

    void AddScoreInternal(int actor, int baseScore)
    {
        var data = players[actor];
        int add = Mathf.RoundToInt(baseScore * data.multiplier);
        data.score += add;
    }

    [PunRPC]
    void RPC_ApplyJudge(int actor, int judgeType, int baseScore)
    {
        var p = players[actor];
        var type = (MultiJudge.JudgeType)judgeType;

        switch(type)
        {
            case MultiJudge.JudgeType.Perfect:
                p.perfect++;
                p.combo++;
                break;

            case MultiJudge.JudgeType.Great:
                p.great++;
                p.combo++;
                break;

            case MultiJudge.JudgeType.Bad:
                p.bad++;
                p.combo = 0;
                Damage(actor, 50);
                break;

            case MultiJudge.JudgeType.Miss:
                p.miss++;
                p.combo = 0;
                Damage(actor, 100);
                break;
        }

        if(baseScore > 0)
            AddScoreInternal(actor, baseScore);

        UpdateScoreAndComboUI();
        UpdateLocalHP();
    }

    public void SendJudge(int actor, MultiJudge.JudgeType type, int baseScore = 0)
    {
        photonView.RPC(
            nameof(RPC_ApplyJudge),
            RpcTarget.All,
            actor,
            (int)type,
            baseScore
        );
    }

    public void EndGame()
    {
        var myData = players[PhotonNetwork.LocalPlayer.ActorNumber];

        Hashtable props = new Hashtable
        {
            { "Score", myData.score },
            { "Perfect", myData.perfect },
            { "Great", myData.great },
            { "Bad", myData.bad },
            { "Miss", myData.miss },
            { "HP", myData.hp },
            { "Combo", myData.combo }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        finishText.SetActive(true);

        StartCoroutine(WaitAndLoad());
    }

    IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.LoadLevel("MultiResultScene");
    }
}