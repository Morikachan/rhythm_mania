using UnityEngine;
using UnityEngine.UI;

public class CardItemController : MonoBehaviour {
    public Image cardImage;
    public Image checkMark;
    public Button button;

    private int cardId;
    private CardSelectPopup popup;

    public void Setup(int id, Sprite sprite, CardSelectPopup owner, bool selected)
    {
        cardId = id;
        popup = owner;

        cardImage.sprite = sprite;
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