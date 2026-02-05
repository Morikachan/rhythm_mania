using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankingUserItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;      // RankingText
    public Image userPhoto;               // UserPhoto
    public TextMeshProUGUI usernameText;  // Username

    [Header("Score Info")]
    public TextMeshProUGUI bestScoreText; // SongScore -> SongBestScoreText
    public TextMeshProUGUI comboText;     // SongScore -> BestComboValue

    public void Setup(int rank, string username, int score, string combo, int homeCardId)
    {
        rankText.text = rank.ToString();
        usernameText.text = username;
        bestScoreText.text = score.ToString();
        comboText.text = combo;

        string fileName = $"game_icon_{homeCardId}.png";

        if (PlayerCardIllustLoader.instance != null)
        {
            if(userPhoto != null)
            {
                Color tempColor = userPhoto.color;
                tempColor.a = 0.0f;
                userPhoto.color = tempColor;
            }

            PlayerCardIllustLoader.instance.LoadPlayerIllustration(userPhoto, fileName, () =>
            {
                if(userPhoto != null)
                {
                    Color finalColor = userPhoto.color;
                    finalColor.a = 1.0f;
                    userPhoto.color = finalColor;
                }
            });
        }
        else
        {
            Debug.LogWarning("PlayerCardIllustLoader instance is null. Make sure it exists in the scene.");
        }
    }
}