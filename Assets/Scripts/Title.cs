using UnityEngine;
using UnityEngine.Networking; // For HTTP-call
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Title : MonoBehaviour {
    private const string USER_ID_KEY = "UserID";

    private string currentUserID;

    [SerializeField] GameObject createUserPop;

    public Button ConfirmButton;
    public TMP_InputField UsernameInputField;
    public TMP_InputField PasswordInputField;

    [System.Serializable]
    public class NewUserData {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class ServerResponse {
        public string status;
        public string message;
        public string user_id;
    }

    public string url = "http://localhost/rhythm_mania/Database/create-user.php";

    void Start()
    {
        ConfirmButton.onClick.AddListener(ConfirmButtonClick);
    }

    public void ConfirmButtonClick()
    {
        Debug.Log("Button Clicked!");
        StartCoroutine(CreateNewUser());
    }

    public void OnScreenClicked()
    {
        if(PlayerPrefs.HasKey(USER_ID_KEY))
        {
            currentUserID = PlayerPrefs.GetString(USER_ID_KEY);
            Debug.Log("User ID: " + currentUserID);
            SceneManager.LoadScene("ResultScene");
        }
        else
        {
            Debug.Log("Local User ID нot found. Creating new user.");
            createUserPop.SetActive(true);
        }
    }

    IEnumerator CreateNewUser()
    {
        NewUserData dataToSend = new NewUserData
        {
            username = UsernameInputField.text,
            password = PasswordInputField.text
        };

        string jsonString = JsonUtility.ToJson(dataToSend);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        using(UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);

                try
                {
                    ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

                    if(response.status == "success")
                    {
                        PlayerPrefs.SetString(USER_ID_KEY, response.user_id);
                        PlayerPrefs.Save();
                        Debug.Log("User created successfully. ID: " + response.user_id);
                        SceneManager.LoadScene("ResultScene");
                    }
                    else
                    {
                        Debug.LogWarning("Server error: " + response.message);
                    }
                }
                catch
                {
                    Debug.LogError("Invalid JSON format from server: " + request.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError("Network error: " + request.error);
            }
        }
    }

    void OnDestroy()
    {
        ConfirmButton.onClick.RemoveListener(ConfirmButtonClick);
    }
}   
