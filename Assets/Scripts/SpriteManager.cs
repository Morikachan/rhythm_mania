using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    private const string HOME_CARD_ID_KEY = "HomeCardID";
    public Image userSprite;

    private const string CARD_SPRITES_PATH =
        //"http://153.126.183.193/student/k248010/ps_game/src/cards/card_sprites/";
        @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_sprites\";

    void Start()
    {
        SetProfileUIImage();
    }

    public void SetProfileUIImage()
    {
        if(PlayerPrefs.HasKey(HOME_CARD_ID_KEY))
        {
            int cardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY);

            if (CardLoader.Instance != null)
            //if (CardLoaderOnline.Instance != null)
            {
                string fileNameSprite = $"card_sprite_{cardId}.png";

                Color tempColor = userSprite.color;
                tempColor.a = 0.0f;
                userSprite.color = tempColor;

                CardLoader.Instance.LoadCardIllustration(
                //CardLoaderOnline.Instance.LoadCardIllustration(
                    userSprite,
                    CARD_SPRITES_PATH,
                    fileNameSprite,
                    () => {
                        Color tempColor = userSprite.color;
                        tempColor.a = 1.0f;
                        userSprite.color = tempColor;
                    }
                );
            }
        };
    }
}