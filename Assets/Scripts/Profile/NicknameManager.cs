using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class NicknameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_InputField nickInput;        // The InputField inside the popup
    public TextMeshProUGUI userNickValue;
    public Button userNamePopupOpenButton;
    public Button userNamePopupConfirmButton;

    [Header("Configuration")]
    private const string BASE_URL = "http://153.126.183.193/student/k248010/rhythm_mania_db/";
    private const string USER_ID_KEY = "UserID";
    private const string USER_NAME_KEY = "UserName";

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

        StartCoroutine(SendNickChangeRequest(newNick));
    }

    IEnumerator SendNickChangeRequest(string newNick)
    {
        string userId = PlayerPrefs.GetString(USER_ID_KEY, "0");
        string url = BASE_URL + "update_user_name.php";

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", newNick);

        // Debug.Log($"Sending Request to {url}: ID={userId}, NewName={newNick}");

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error changing nick: " + www.error);
                // TODO: Show an error message popup here
            }
            else
            {
                // --- SUCCESS ---
                PlayerPrefs.SetString(USER_NAME_KEY, newNick);
                PlayerPrefs.Save();

                userNickValue.text = newNick;

                // Close edit popup
                popupPanel.SetActive(false);
            }
        }
    }
}