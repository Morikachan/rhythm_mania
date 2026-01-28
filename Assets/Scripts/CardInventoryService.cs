using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class CardInventoryService : MonoBehaviour
{
    [System.Serializable]
    public class CardListResponse
    {
        public string status;
        public string message;
        public List<CardData> cardlist;
    }

    [System.Serializable]
    class UserIdWrapper
    {
        public string user_id;
    }

    public static CardInventoryService Instance;

    public List<CardData> AllCards { get; private set; } = new();

    private const string USER_ID_KEY = "UserID";
    private const string URL =
        "http://localhost/rhythm_mania/Database/core/getCardList.php";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        LoadCards();
    }

    public void LoadCards()
    {
        if (!PlayerPrefs.HasKey(USER_ID_KEY))
        {
            Debug.LogError("UserID not found");
            return;
        }

        StartCoroutine(LoadCardsCoroutine(PlayerPrefs.GetString(USER_ID_KEY)));
    }

    IEnumerator LoadCardsCoroutine(string userId)
    {
        var body = JsonUtility.ToJson(new UserIdWrapper { user_id = userId });
        var bodyRaw = Encoding.UTF8.GetBytes(body);

        using var request = new UnityWebRequest(URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        var response =
            JsonUtility.FromJson<CardListResponse>(request.downloadHandler.text);

        AllCards = response.cardlist ?? new List<CardData>();

        Debug.Log($"Loaded cards: {AllCards.Count}");
    }
}

