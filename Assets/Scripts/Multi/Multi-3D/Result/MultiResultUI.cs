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

    void Start()
    {
        var data = MultiResultDataHolder.instance.results;
        var players = PhotonNetwork.PlayerList;

        var sorted = data.Values
            .OrderByDescending(p => p.score)
            .ThenByDescending(p => p.Accuracy)
            .ToList();

        Setup(sorted[0], WinnerResultImage, WinnerResultNick);
        Setup(sorted[1], SecondResultImage, SecondResultNick);
    }

    void Setup(PlayerRuntimeData d, Image img, TextMeshProUGUI nick)
    {
        Player p = PhotonNetwork.PlayerList
            .First(x => x.ActorNumber == d.actorNumber);

        nick.text = p.CustomProperties["UserName"].ToString();

        PlayerCardIllustLoader.instance.LoadPlayerIllustration(
            img,
            $"game_icon_{(int)p.CustomProperties["CardID"]}.png"
        );
    }
}
