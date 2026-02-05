using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MultiPlayerResult : MonoBehaviour {
    [Header("UI")]
    public Text scoreText;
    public Text perfectText;
    public Text greatText;
    public Text badText;
    public Text missText;
    public Text rankText;
    public Image userSprite;

    private int score, perfect, great, bad, miss;
    private string rank;

    private const string HOME_CARD_ID_KEY = "HomeCardID";
    private const string CARD_SPRITES_PATH = "http://153.126.183.193/student/k248010/ps_game/src/cards/card_sprites/";

    void Start()
    {
        LoadLocalPlayerData();
        rank = CalculateRankFromStats();
        UpdateUI();
    }

    void LoadLocalPlayerData()
    {
        Player p = PhotonNetwork.LocalPlayer;
        var props = p.CustomProperties;

        if(props.ContainsKey("Score"))
        {
            score = (int)props["Score"];
            perfect = (int)props["Perfect"];
            great = (int)props["Great"];
            bad = (int)props["Bad"];
            miss = (int)props["Miss"];
        }
    }

    string CalculateRankFromStats()
    {
        int total = perfect + great + bad + miss;
        if(total == 0) return "D";

        float hit = perfect + great * 0.7f;
        float percent = (hit / total) * 100f;

        if(bad == 0 && miss == 0) return "SS";
        if(percent >= 95f) return "S";
        if(percent >= 75f) return "A";
        if(percent >= 60f) return "B";
        if(percent >= 40f) return "C";
        return "D";
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
        perfectText.text = perfect.ToString();
        greatText.text = great.ToString();
        badText.text = bad.ToString();
        missText.text = miss.ToString();
        rankText.text = rank;

        if(PlayerPrefs.HasKey(HOME_CARD_ID_KEY))
        {
            int cardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY);
            if(CardLoaderOnline.Instance != null)
            {
                string fileNameSprite = $"card_sprite_{cardId}.png";
                CardLoaderOnline.Instance.LoadCardIllustration(userSprite, CARD_SPRITES_PATH, fileNameSprite);
            }
        };
    }
}