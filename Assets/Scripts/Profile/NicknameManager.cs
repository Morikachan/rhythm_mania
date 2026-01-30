using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class NicknameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;           // The 'EditNickPopup' GameObject
    public TMP_InputField nickInput;        // The InputField inside the popup
    public TextMeshProUGUI userNickValue;   // The text on the main screen (UserNickValue)

    [Header("Configuration")]
    private const string BASE_URL = "http://localhost/rhythm_mania/";
    private const string USER_ID_KEY = "UserID";
    private const string USER_NAME_KEY = "UserName";

    void Start()
    {
        // Ensure popup is hidden at start
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void OpenEditPopup()
    {
        nickInput.text = userNickValue.text;
        popupPanel.SetActive(true);
    }

    public void CloseEditPopup()
    {
        popupPanel.SetActive(false);
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
        string url = BASE_URL + "change_username.php";

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", newNick);

        Debug.Log($"Sending Request to {url}: ID={userId}, NewName={newNick}");

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
                Debug.Log("Server Response: " + www.downloadHandler.text);

                // --- SUCCESS ---
                PlayerPrefs.SetString(USER_NAME_KEY, newNick);
                PlayerPrefs.Save();

                userNickValue.text = newNick;

                CloseEditPopup();
            }
        }
    }
}