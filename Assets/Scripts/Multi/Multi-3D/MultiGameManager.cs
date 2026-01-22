using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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

    public Slider localPlayerHP;

    [Header("Game Settings")]
    public float noteSpeed = 5f;
    public float startDelay = 2f; // Delay before start

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
        Debug.Log("MultiGameManager started");
        InitPlayers();

        StartCoroutine(StartGameAfterDelay());
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

        musicManager.ResetMusic();

        float musicDelay = notesManager.spawnOffset / noteSpeed;
        musicManager.PlayMusic(musicDelay);

        notesManager.StartGame();

        Debug.Log("MULTI GAME STARTED");
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

    //  SCORE 

    public void AddScore(int actor, int baseScore)
    {
        var data = players[actor];
        int add = Mathf.RoundToInt(baseScore * data.multiplier);
        data.score += add;
        data.combo++;

        UpdateScoreAndComboUI();
        UpdateLocalHP();
    }

    public void ResetCombo(int actor)
    {
        players[actor].combo = 0;

        UpdateScoreAndComboUI();
        UpdateLocalHP();
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

        if(comboText != null)
            comboText.text = localData.combo.ToString();

        if(scoreText != null)
            scoreText.text = localData.score.ToString();

        // BOTH PLAYERS COMBO (TOP UI)
        Player[] p = PhotonNetwork.PlayerList;
        if(p.Length < 2) return;

        player1Combo.text = players[p[0].ActorNumber].combo.ToString();
        player2Combo.text = players[p[1].ActorNumber].combo.ToString();
    }

    //  END 

    public void EndGame()
    {
        MultiResultDataHolder.instance.SetResults(players);
        PhotonNetwork.LoadLevel("MultiResultScene");
    }
}