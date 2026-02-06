using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfileSettingsManager : MonoBehaviour
{
    public static ProfileSettingsManager instance = null;

    public CardSelectPopup cardPopup;
    public Button changeCardButton;
    public CardInventoryService inventoryService;

    [Header("Player References")]
    private const string USER_ID_KEY = "UserID";
    private const string USER_NAME_KEY = "UserName";
    private const string HOME_CARD_ID_KEY = "HomeCardID";

    [Header("Profile UI")]
    public TextMeshProUGUI userIDValue;
    public TextMeshProUGUI userNameValue;
    public Image profileIcon;
    public Image profileSprite;

    [Header("Header Panel")]
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text starsValue;
    [SerializeField] TMP_Text coinsValue;
    [SerializeField] TMP_Text levelValue;

    [System.Serializable]
    public class UserData {
        public string user_id;
    }

    [System.Serializable]
    public class ServerResponse {
        public string status;
        public string message;
        public UserInfo user_info;
        public UserInventory user_inventory;
    }

    [System.Serializable]
    public class UserInfo {
        public string user_name;
        public int user_lvl;
        public int user_exp;
        public int next_lvl_value;
        public int next_lvl_percent;
        public int home_card_id;
    };

    [System.Serializable]
    public class UserInventory {
        public int free_gems;
        public int paid_gems;
        public int coins;
    };

    public string receiveUrl = "http://153.126.183.193/student/k248010/rhythm_mania_db/user-home-info.php";

    private const string CARD_ICONS_PATH =
        //"http://153.126.183.193/student/k248010/ps_game/src/cards/card_icons/";
        @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_icons\";

    private const string CARD_SPRITES_PATH =
        //"http://153.126.183.193/student/k248010/ps_game/src/cards/card_sprites/";
        @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_sprites\";

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        changeCardButton.onClick.AddListener(OnChangeCardClicked);

        SetProfileUIText();
        SetProfileUIImage();

        GetJsonData();
    }
    public void OnChangeCardClicked()
    {
        cardPopup.Open(inventoryService.AllCards);
    }

    public void SetProfileUIText()
    {
        if(PlayerPrefs.HasKey(USER_ID_KEY))
        {
           userIDValue.text = PlayerPrefs.GetString(USER_ID_KEY);
        };

        if(PlayerPrefs.HasKey(USER_NAME_KEY))
        {
            userNameValue.text = PlayerPrefs.GetString(USER_NAME_KEY);
        };
    }

    public void SetProfileUIImage()
    {
        if(PlayerPrefs.HasKey(HOME_CARD_ID_KEY))
        {
            int cardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY);

            if(CardLoader.Instance != null)
            // if(CardLoaderOnline.Instance != null)
            {
                // sprite
                string fileNameSprite = $"card_sprite_{cardId}.png";

                Color tempColor = profileSprite.color;
                tempColor.a = 0.0f;
                profileSprite.color = tempColor;

                CardLoader.Instance.LoadCardIllustration(
                //CardLoaderOnline.Instance.LoadCardIllustration(
                    profileSprite,
                    CARD_SPRITES_PATH,
                    fileNameSprite,
                    () => {
                        Color tempColor = profileSprite.color;
                        tempColor.a = 1.0f;
                        profileSprite.color = tempColor;
                    }
                );

                // icon
                string fileNameIcon = $"card_icon_{cardId}.png";
                CardLoader.Instance.LoadCardIllustration(profileIcon, CARD_ICONS_PATH, fileNameIcon);
                //CardLoaderOnline.Instance.LoadCardIllustration(profileIcon, CARD_ICONS_PATH, fileNameIcon);
            }
        };
    }

    // TODO: Header own class
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
                //Debug.Log("Raw response: " + request.downloadHandler.text);

                if(request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;

                    ServerResponse receivedData = JsonUtility.FromJson<ServerResponse>(jsonResponse);

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

    private void UpdateProgress(int exp)
    {
        slider.value = exp;
    }
}
