using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SocialPlatforms.Impl;

public class NicknameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_InputField nickInput;        // The InputField inside the popup
    public TextMeshProUGUI userNickValue;
    public Button userNamePopupOpenButton;
    public Button userNamePopupConfirmButton;

    [Header("Configuration")]
    private const string BASE_URL = "http://153.126.183.193/student/k248010/rhythm_mania_db/update_user_name.php";
    private const string USER_ID_KEY = "UserID";
    private const string USER_NAME_KEY = "UserName";

    [System.Serializable]
    class UserData {
        public string user_id;
        public string username;
    }

    [System.Serializable]
    public class ServerResponse {
        public string status;
        public string message;
    }

    void Start()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        userNamePopupOpenButton.onClick.AddListener(OpenEditPopup);
        userNamePopupConfirmButton.onClick.AddListener(OnConfirmClicked);
    }

    public void OpenEditPopup()
    {
        if(PlayerPrefs.HasKey(USER_NAME_KEY))
        {
            nickInput.text = PlayerPrefs.GetString(USER_NAME_KEY);
        }
        else
        {
            nickInput.text = "User";
        }

        nickInput.ForceLabelUpdate();
        nickInput.MoveTextEnd(false);

        popupPanel.SetActive(true);
    }

    public void OnConfirmClicked()
    {
        string newNick = nickInput.text;

        if (string.IsNullOrEmpty(newNick))
        {
            Debug.LogWarning("Nickname cannot be empty!");
            return;
        }

        StartCoroutine(SendJsonDataNewName(newNick));
    }

    IEnumerator SendJsonDataNewName(string newNick)
    {
        yield return null;

        if(!PlayerPrefs.HasKey(USER_ID_KEY))
        {
            Debug.LogError("No UserID found, cannot send result.");
            yield break;
        }

        int songId = SongDataHolder.instance != null ? SongDataHolder.instance.SelectedSongId : 1;

        UserData dataToSend = new UserData
        {
            user_id = PlayerPrefs.GetString(USER_ID_KEY),
            username = newNick,
        };

        string jsonString = JsonUtility.ToJson(dataToSend);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        Debug.Log("Sending Result: " + jsonString);

        using(UnityWebRequest request = new UnityWebRequest(BASE_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Server Response: " + request.downloadHandler.text);

                // --- SUCCESS ---
                PlayerPrefs.SetString(USER_NAME_KEY, newNick);
                PlayerPrefs.Save();

                userNickValue.text = newNick;

                // Close edit popup
                popupPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("Error sending result: " + request.error);
            }
        }
    }
}