using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using static CardSelectPopup;

public class HomePage : MonoBehaviour
{
    public Image homeImage;

    private int CURRENT_CARD_ID = 1;

    [Header("Player References")]
    private const string USER_ID_KEY = "UserID";
    private const string USER_NAME_KEY = "UserName";
    private const string HOME_CARD_ID_KEY = "HomeCardID";

    private const string CARD_ICONS_PATH = @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_illust\";

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
        GetJsonData();
    }

    void OnEnable()
    {
        HomeCardEvents.OnHomeCardChanged += OnHomeCardChanged;
    }

    void OnDisable()
    {
        HomeCardEvents.OnHomeCardChanged -= OnHomeCardChanged;
    }

    async void GetJsonData()
    {
        UserData dataToSend = null;

        if(PlayerPrefs.HasKey(USER_ID_KEY))
        {
            dataToSend = new UserData
            {
                user_id = PlayerPrefs.GetString(USER_ID_KEY),
            };
        };

        if(dataToSend != null)
        {
            string jsonString = JsonUtility.ToJson(dataToSend);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);


            using(UnityWebRequest request = new UnityWebRequest(receiveUrl, "POST"))
            {

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                try
                {
                    await request.SendWebRequest();
                }
                catch(System.Exception e)
                {
                    Debug.LogError("SYSTEM ERROR: " + e.Message);
                    return;
                }
                //Debug.Log("HTTP Status: " + request.responseCode);
                //Debug.Log("Raw response: " + request.downloadHandler.text);

                if(request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;

                    ServerResponse receivedData = JsonUtility.FromJson<ServerResponse>(jsonResponse);

                    //Debug.Log("Parsed Object: " + JsonUtility.ToJson(receivedData, true));

                    CURRENT_CARD_ID = receivedData.user_info.home_card_id;

                    PlayerPrefs.SetString(USER_NAME_KEY, receivedData.user_info.user_name);
                    PlayerPrefs.SetInt(HOME_CARD_ID_KEY, CURRENT_CARD_ID);
                    PlayerPrefs.Save();

                    StartCoroutine(WaitForCardLoaderAndDisplay());
                    UpdateProgress(receivedData.user_info.next_lvl_percent);

                    starsValue.text = (receivedData.user_inventory.paid_gems + receivedData.user_inventory.free_gems).ToString();
                    coinsValue.text = receivedData.user_inventory.coins.ToString();
                    levelValue.text = "Level " + receivedData.user_info.user_lvl;
                }
                else
                {
                    Debug.LogError("UnityWebRequest Error: " + request.error);
                }
            }
        }
        else
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
            yield return null;
        }

        DisplayCard(CURRENT_CARD_ID);
    }

    public void DisplayCard(int newCardID)
    {
        if (homeImage == null) return;

        string fileName = $"card_{newCardID}.jpg";
        CardLoader.Instance.LoadCardIllustration(homeImage, CARD_ICONS_PATH, fileName);
    }

    private void OnHomeCardChanged(int newCardId)
    {
        CURRENT_CARD_ID = newCardId;
        PlayerPrefs.SetInt(HOME_CARD_ID_KEY, newCardId);
        PlayerPrefs.Save();

        DisplayCard(newCardId);
    }

    private void UpdateProgress(int exp)
    {
        slider.value = exp;
    }
}