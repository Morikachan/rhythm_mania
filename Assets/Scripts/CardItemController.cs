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
        @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_illust\";

    public void Setup(int id, CardSelectPopup owner, bool selected)
    {
        cardId = id;
        popup = owner;

        if (CardLoader.Instance != null)
        {
            string fileName = $"card_{cardId}.jpg";
            CardLoader.Instance.LoadCardIllustration(cardImage, CARD_ICONS_PATH, fileName);
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
