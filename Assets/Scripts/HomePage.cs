using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using TMPro;

public class HomePage : MonoBehaviour
{
    public Image homeImage;

    private int CURRENT_CARD_ID = 1;

    private const string USER_ID_KEY = "UserID";

    [SerializeField] Slider slider;
    [SerializeField] TMP_Text starsValue;
    [SerializeField] TMP_Text coinsValue;
    [SerializeField] TMP_Text levelValue;

    [System.Serializable]
    public class UserData
    {
        public string user_id;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
        public UserInfo user_info;
        public UserInventory user_inventory;
    }

    [System.Serializable]
    public class UserInfo
    {
        public string user_name;
        public int user_lvl;
        public int user_exp;
        public int next_lvl_value;
        public int next_lvl_percent;
        public int home_card_id;
    };

    [System.Serializable]
    public class UserInventory
    {
        public int free_gems;
        public int paid_gems;
        public int coins;
    };

    public string receiveUrl = "http://localhost/rhythm_mania/Database/user-home-info.php";

    void Start()
    {
        StartCoroutine(GetJsonData());
    }


    IEnumerator GetJsonData()
    {
        UserData dataToSend = null;

        if (PlayerPrefs.HasKey(USER_ID_KEY))
        {
            dataToSend = new UserData
            {
                user_id = PlayerPrefs.GetString(USER_ID_KEY),
            };
        };

        if (dataToSend != null)
        {
            string jsonString = JsonUtility.ToJson(dataToSend);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);


            //using (UnityWebRequest request = UnityWebRequest.Get(receiveUrl))
            using (UnityWebRequest request = new UnityWebRequest(receiveUrl, "POST"))
            {

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("Received JSON: " + jsonResponse);

                    ServerResponse receivedData = JsonUtility.FromJson<ServerResponse>(jsonResponse);

                    CURRENT_CARD_ID = receivedData.user_info.home_card_id;

                    StartCoroutine(WaitForCardLoaderAndDisplay());
                    UpdateProgress(receivedData.user_info.next_lvl_percent);

                    starsValue.text = (receivedData.user_inventory.paid_gems + receivedData.user_inventory.free_gems).ToString();
                    coinsValue.text = receivedData.user_inventory.coins.ToString();
                    levelValue.text = "Level " + receivedData.user_info.user_lvl;
                }
                else
                {
                    Debug.LogError("Error receiving JSON: " + request.error);
                }
            }
        } else
        {
            Debug.LogError("User ID not found. Cannot send data.");
        }
    }

    IEnumerator WaitForCardLoaderAndDisplay()
    {
        if (homeImage == null)
        {
            Debug.LogError("HomePage: Target Image (homeImage) is not assigned in the Inspector. Please assign it!");
            yield break;
        }

        while (CardLoader.Instance == null)
        {
            Debug.LogWarning("HomePage is waiting for CardLoader to initialize...");
            yield return null;
        }

        DisplayCard(CURRENT_CARD_ID);
    }

    public void DisplayCard(int newCardID)
    {
        if (homeImage == null) return;

        Debug.Log($"HomePage: CardLoader is ready. Loading Card ID {newCardID} into homeImage.");
        CardLoader.Instance.LoadCardIllustration(newCardID, homeImage);
    }

    private void UpdateProgress(int exp)
    {
        slider.value = exp;
    }
}