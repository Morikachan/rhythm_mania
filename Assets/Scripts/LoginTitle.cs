using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class LoginTitle : MonoBehaviour {

    private const string USER_ID_KEY = "UserID";

    [Header("Server")]
    [SerializeField] private string loginUrl = "http://153.126.183.193/student/k248010/rhythm_mania_db/user_login.php";

    [Header("UI Login Popup Button")]
    public Button openLoginPopupButton;

    [Header("UI Login Popup")]
    public GameObject loginPopupPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button submitLoginButton;
    public Button closeLoginButton;

    [Header("UI Error Popup")]
    public GameObject errorPopupPanel;
    public Button closeErrorButton;


    [System.Serializable]
    public class LoginData {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class LoginResponse {
        public string status;
        public string message;
        public string user_id;
    }

    private void Start()
    {
        openLoginPopupButton.onClick.AddListener(OnClickOpenLoginPopup);
        submitLoginButton.onClick.AddListener(OnClickCheckLogin);

        closeLoginButton.onClick.AddListener(() => loginPopupPanel.SetActive(false));
        closeErrorButton.onClick.AddListener(() => errorPopupPanel.SetActive(false));

        loginPopupPanel.SetActive(false);
        errorPopupPanel.SetActive(false);
    }

    void OnClickOpenLoginPopup()
    {
        loginPopupPanel.SetActive(true);
        emailInput.text = "";
        passwordInput.text = "";
    }

    void OnClickCheckLogin()
    {
        string email = emailInput.text;
        string pass = passwordInput.text;

        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            ShowError("Fill Email & Password");
            return;
        }

        StartCoroutine(SendLoginRequest(email, pass));
    }

    IEnumerator SendLoginRequest(string email, string password)
    {
        submitLoginButton.interactable = false;

        LoginData dataToSend = new LoginData { email = email, password = password };
        string jsonToSend = JsonUtility.ToJson(dataToSend);

        using(UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToSend);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                ShowError("Server Error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                try
                {
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                    if(response.status == "success")
                    {
                        OnLoginSuccess(response.user_id);
                    }
                    else
                    {
                        ShowError(response.message);
                    }
                }
                catch(System.Exception e)
                {
                    ShowError("Error: " + e.Message);
                }
            }
        }

        submitLoginButton.interactable = true;
    }

    void OnLoginSuccess(string userId)
    {
        loginPopupPanel.SetActive(false);
        PlayerPrefs.SetString(USER_ID_KEY, userId);
    }

    void ShowError(string message)
    {
        //errorMessageText.text = message;
        errorPopupPanel.SetActive(true);
    }
}