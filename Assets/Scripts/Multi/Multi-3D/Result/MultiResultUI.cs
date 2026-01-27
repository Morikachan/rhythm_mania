using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class MultiResultUI : MonoBehaviour {
    public Image WinnerResultImage;
    public Image SecondResultImage;

    public TextMeshProUGUI WinnerResultNick;
    public TextMeshProUGUI SecondResultNick;

    public TextMeshProUGUI WinnerResultScore;
    public TextMeshProUGUI SecondResultScore;

    //void Start()
    //{
    //    var data = MultiResultDataHolder.instance.results;
    //    var players = PhotonNetwork.PlayerList;

    //    var sorted = data.Values
    //        .OrderByDescending(p => p.score)
    //        .ThenByDescending(p => p.Accuracy)
    //        .ToList();

    //    Setup(sorted[0], WinnerResultImage, WinnerResultNick, WinnerResultScore);
    //    Setup(sorted[1], SecondResultImage, SecondResultNick, SecondResultScore);
    //}

    //void Setup(PlayerRuntimeData d, Image img, TextMeshProUGUI nick, TextMeshProUGUI score)
    //{
    //    Player p = PhotonNetwork.PlayerList
    //        .First(x => x.ActorNumber == d.actorNumber);

    //    nick.text = p.CustomProperties["UserName"].ToString();

    //    score.text = d.score.ToString();

    //    PlayerCardIllustLoader.instance.LoadPlayerIllustration(
    //        img,
    //        $"game_icon_{(int)p.CustomProperties["CardID"]}.png"
    //    );
    //}

    void Start()
    {
        var players = PhotonNetwork.PlayerList.OrderByDescending(p =>
            p.CustomProperties.ContainsKey("Score") ? (int)p.CustomProperties["Score"] : 0
        ).ToList();

        if(players.Count > 0)
            SetupFromPhoton(players[0], WinnerResultImage, WinnerResultNick, WinnerResultScore);

        if(players.Count > 1)
            SetupFromPhoton(players[1], SecondResultImage, SecondResultNick, SecondResultScore);
    }

    void SetupFromPhoton(Player p, Image img, TextMeshProUGUI nick, TextMeshProUGUI score)
    {
        nick.text = p.CustomProperties["UserName"].ToString();
        score.text = p.CustomProperties.ContainsKey("Score") ? p.CustomProperties["Score"].ToString() : "0";

        int cardId = p.CustomProperties.ContainsKey("CardID") ? (int)p.CustomProperties["CardID"] : 0;
        PlayerCardIllustLoader.instance.LoadPlayerIllustration(img, $"game_icon_{cardId}.png");
    }
}
