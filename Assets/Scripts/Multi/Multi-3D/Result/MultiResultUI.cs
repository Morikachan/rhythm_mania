using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class MultiResultUI : MonoBehaviour {

    [Header("Winner UI")]
    public Image WinnerResultImage;
    public Image WinnerBadgeBg;

    public TextMeshProUGUI WinnerResultNick;
    public TextMeshProUGUI WinnerResultScore;
    public TextMeshProUGUI WinnerBadgeText;

    public Image SecondResultImage;
    public Image SecondBadgeBg;

    public TextMeshProUGUI SecondResultNick;
    public TextMeshProUGUI SecondResultScore;
    public TextMeshProUGUI SecondBadgeText;

    void Start()
    {
        var sortedPlayers = MultiGameDataStorage.CachedPlayers.Values
            .OrderByDescending(p => p.Score)
            .ToList();

        if(sortedPlayers.Count > 0)
            SetupResultSlot(sortedPlayers[0], WinnerResultImage, WinnerResultNick, WinnerResultScore, WinnerBadgeText, WinnerBadgeBg);

        if(sortedPlayers.Count > 1)
            SetupResultSlot(sortedPlayers[1], SecondResultImage, SecondResultNick, SecondResultScore, SecondBadgeText, SecondBadgeBg);
    }

    void SetupResultSlot(MultiGameDataStorage.PlayerInfo p, Image img, TextMeshProUGUI nick, TextMeshProUGUI score, TextMeshProUGUI badge, Image badgeBg)
    {
        nick.text = p.Nickname;
        score.text = p.Score.ToString();
        PlayerCardIllustLoader.instance.LoadPlayerIllustration(img, $"game_icon_{p.CardID}.png");

        if(p.InitialPosition == 1)
        {
            badge.text = "P1";
            badgeBg.color = HexToColor("FF0046");
        }
        else
        {
            badge.text = "P2";
            badgeBg.color = HexToColor("B600FF");
        }
    }

    Color HexToColor(string hex)
    {
        Color col;
        ColorUtility.TryParseHtmlString("#" + hex, out col);
        return col;
    }
}
