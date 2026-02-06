using UnityEngine;
using UnityEngine.UI;

public class CardItemController : MonoBehaviour
{
    public Image cardImage;
    public Image checkMark;
    public Button button;

    private int cardId;
    private CardSelectPopup popup;

    private const string CARD_ICONS_PATH =
        // "http://153.126.183.193/student/k248010/ps_game/src/cards/card_icons/";
        @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_icons\";

    public void Setup(int id, CardSelectPopup owner, bool selected)
    {
        cardId = id;
        popup = owner;

        // if (CardLoaderOnline.Instance != null)
        if (CardLoader.Instance != null)
        {
            string fileName = $"card_icon_{cardId}.png";
            CardLoader.Instance.LoadCardIllustration(cardImage, CARD_ICONS_PATH, fileName);
            // CardLoaderOnline.Instance.LoadCardIllustration(cardImage, CARD_ICONS_PATH, fileName);
        }

        checkMark.gameObject.SetActive(selected);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        popup.OnCardSelected(cardId);
    }

    public void SetSelected(bool value)
    {
        checkMark.gameObject.SetActive(value);
    }

    public int GetCardId() => cardId;
}
