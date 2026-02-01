using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    private const string HOME_CARD_ID_KEY = "HomeCardID";
    public Image userSprite;

    private const string CARD_SPRITES_PATH =
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

            if(CardLoader.Instance != null)
            {
                string fileNameSprite = $"card_sprite_{cardId}.png";
                CardLoader.Instance.LoadCardIllustration(userSprite, CARD_SPRITES_PATH, fileNameSprite);
            }
        };
    }
}