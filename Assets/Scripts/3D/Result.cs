using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class Result : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text perfectText;
    [SerializeField] Text greatText;
    [SerializeField] Text badText;
    [SerializeField] Text misseText;
    [SerializeField] Text rankText;

    private int scoreValue;
    private int perfectCount;
    private int greatCount;
    private int badCount;
    private int missCount;
    private string rank = "D";

    private int selected_song_id;
    private const string USER_ID_KEY = "UserID";

    [System.Serializable]
    public class UserData
    {
        public string user_id;
        public int song_id;
        public string combo;
        public int score;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
        public bool combo_isNew;
        public bool score_isNew;
    }

    public string url = "http://localhost/rhythm_mania/Database/update_user_song.php";

    void Start()
    {
        scoreValue = GameManager.instance.score;
        perfectCount = GameManager.instance.perfect;
        greatCount = GameManager.instance.great;
        badCount = GameManager.instance.bad;
        missCount = GameManager.instance.miss;
        SetRank();

        StartCoroutine(GetJsonData());
    }

    IEnumerator GetJsonData()
    {
        UserData dataToSend = null;

        if (SongDataHolder.instance != null)
        {
            selected_song_id = SongDataHolder.instance.SelectedSongId;
        }
        else
        {
            selected_song_id = 2;
        }

        if (PlayerPrefs.HasKey(USER_ID_KEY))
        {
            dataToSend = new UserData
            {
                user_id = PlayerPrefs.GetString(USER_ID_KEY),
                song_id = selected_song_id,
                combo = rank,
                score = scoreValue,
            };
        };

        if (dataToSend != null)
        {
            string jsonString = JsonUtility.ToJson(dataToSend);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

                        if (response.status == "success")
                        {
                            SetResultText();
                            ClearData();
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
        else
        {
            Debug.LogError("User ID not found. Cannot send data.");
        }
    }

    public void SetRank()
    {
        int totalNotes = perfectCount + greatCount + badCount + missCount;
        float totalHit = perfectCount + greatCount;
        float percentHit = (totalHit / totalNotes) * 100;

        if (badCount == 0 && missCount == 0)
        {
            rank = "SS";
        }
        else if (percentHit > 95)
        {
            rank = "S";
        }
        else if (percentHit > 75)
        {
            rank = "A";
        }
        else if (percentHit > 60)
        {
            rank = "B";
        }
        else if(percentHit > 40)
        {
            rank = "C";
        }
        else
        {
            rank = "D";
        }
    }

    public void SetResultText()
    {
        scoreText.text = scoreValue.ToString();
        perfectText.text = perfectCount.ToString();
        greatText.text = greatCount.ToString();
        badText.text = badCount.ToString();
        misseText.text = missCount.ToString();
        rankText.text = rank;
    }

    public void ClearData()
    {
        GameManager.instance.score = 0;
        GameManager.instance.perfect = 0;
        GameManager.instance.great = 0;
        GameManager.instance.bad = 0;
        GameManager.instance.miss = 0;
        GameManager.instance.combo = 0;
        GameManager.instance.maxScore = 0;
        GameManager.instance.ratioScore = 0;
    }
}