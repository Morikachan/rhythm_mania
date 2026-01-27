using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CardSelectPopup : MonoBehaviour {
    [Header("UI")]
    public Transform gridParent;
    public GameObject cardItemPrefab;
    public Button selectButton;

    private int currentHomeCardId;
    private int selectedCardId;

    private List<CardItemController> items = new();

    private const string USER_ID_KEY = "UserID";
    private const string HOME_CARD_ID_KEY = "HomeCardID";

    private string setHomeUrl = "http://localhost/rhythm_mania/Database/change-home-char.php";

    public void Open(List<CardData> cards)
    {
        gameObject.SetActive(true);

        currentHomeCardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY);
        selectedCardId = currentHomeCardId;

        BuildGrid(cards);

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelectClicked);
    }

    void BuildGrid(List<CardData> cards)
    {
        foreach(Transform c in gridParent)
            Destroy(c.gameObject);

        items.Clear();

        foreach(var card in cards)
        {
            GameObject obj = Instantiate(cardItemPrefab, gridParent);
            var ctrl = obj.GetComponent<CardItemController>();

            bool isSelected = card.card_id == selectedCardId;

            ctrl.Setup(
                card.card_id,
                card.sprite,
                this,
                isSelected
            );

            items.Add(ctrl);
        }
    }

    public void OnCardSelected(int cardId)
    {
        selectedCardId = cardId;

        foreach(var item in items)
            item.SetSelected(item.GetCardId() == selectedCardId);
    }

    void OnSelectClicked()
    {
        if(selectedCardId == currentHomeCardId)
        {
            Close();
            return;
        }

        StartCoroutine(SendSelectRequest());
    }

    public static class HomeCardEvents {
        public static System.Action<int> OnHomeCardChanged;
    }

    IEnumerator SendSelectRequest()
    {
        var payload = new
        {
            user_id = PlayerPrefs.GetString(USER_ID_KEY),
            card_id = selectedCardId
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(setHomeUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if(req.result == UnityWebRequest.Result.Success)
        {
            PlayerPrefs.SetInt(HOME_CARD_ID_KEY, selectedCardId);
            PlayerPrefs.Save();

            HomeCardEvents.OnHomeCardChanged?.Invoke(selectedCardId);

            Close();
        }
        else
        {
            Debug.LogError(req.error);
        }
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}
