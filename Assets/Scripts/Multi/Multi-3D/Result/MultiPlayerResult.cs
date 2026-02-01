using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Photon.Pun;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using Photon.Realtime;

public class MultiPlayerResult : MonoBehaviour {
    [Header("UI")]
    public Text scoreText;
    public Text perfectText;
    public Text greatText;
    public Text badText;
    public Text missText;
    public Text rankText;
    public Image userSprite;

    private int score;
    private int perfect;
    private int great;
    private int bad;
    private int miss;
    private string rank;

    private bool sent = false;

    private const string USER_ID_KEY = "UserID";
    private const string HOME_CARD_ID_KEY = "HomeCardID";
    public string url = "http://153.126.183.193/student/k248010/rhythm_mania_db/update_user_song.php";

    private const string CARD_SPRITES_PATH =
        @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_sprites\";

    [System.Serializable]
    class UserData {
        public string user_id;
        public int song_id;
        public string combo;
        public int score;
    }


    [System.Serializable]
    public class ServerResponse {
        public string status;
        public string message;
        public bool combo_isNew;
        public bool score_isNew;
    }

    void Start()
    {
        LoadLocalPlayerData();
        rank = CalculateRankFromStats();
        UpdateUI();
    }

    void LoadLocalPlayerData()
    {
        Player p = PhotonNetwork.LocalPlayer;
        var props = p.CustomProperties;

        if(props.ContainsKey("Score"))
        {
            score = (int)props["Score"];
            perfect = (int)props["Perfect"];
            great = (int)props["Great"];
            bad = (int)props["Bad"];
            miss = (int)props["Miss"];
        }
        else
        {
            score = perfect = great = bad = miss = 0;
        }
    }

    string CalculateRankFromStats()
    {
        int total = perfect + great + bad + miss;
        if(total == 0) return "D";

        float hit = perfect + great * 0.7f;
        float percent = (hit / total) * 100f;

        if(bad == 0 && miss == 0) return "SS";
        if(percent >= 95f) return "S";
        if(percent >= 75f) return "A";
        if(percent >= 60f) return "B";
        if(percent >= 40f) return "C";
        return "D";
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
        perfectText.text = perfect.ToString();
        greatText.text = great.ToString();
        badText.text = bad.ToString();
        missText.text = miss.ToString();
        rankText.text = rank;

        if(PlayerPrefs.HasKey(HOME_CARD_ID_KEY))
        {
            int cardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY);

            if(CardLoader.Instance != null)
            {
                // sprite
                string fileNameSprite = $"card_sprite_{cardId}.png";
                CardLoader.Instance.LoadCardIllustration(userSprite, CARD_SPRITES_PATH, fileNameSprite);
            }
        };
    }

    public void SendResultToServer()
    {
        if(sent) return;
        sent = true;

        StartCoroutine(SendJsonData());
    }

    IEnumerator SendJsonData()
    {
        yield return new WaitForSeconds(0.5f);
        UserData dataToSend = null;

        if(!PlayerPrefs.HasKey(USER_ID_KEY))
            yield break;

        int songId = SongDataHolder.instance != null
            ? SongDataHolder.instance.SelectedSongId
            : 1;

        if(PlayerPrefs.HasKey(USER_ID_KEY))
        {
            dataToSend = new UserData
            {
                user_id = PlayerPrefs.GetString(USER_ID_KEY),
                song_id = songId,
                combo = rank,
                score = score,
            };
        };

        if(dataToSend != null)
        {
            string jsonString = JsonUtility.ToJson(dataToSend);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

            Debug.Log("[SEND RESULT JSON] " + jsonString);

            using(UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if(request.result == UnityWebRequest.Result.Success)
                {
                    string rawJson = request.downloadHandler.text.Trim();
                    Debug.Log("Clean JSON: " + rawJson);

                    ServerResponse response = JsonUtility.FromJson<ServerResponse>(rawJson);

                    if(response == null || string.IsNullOrEmpty(response.status))
                    {
                        Debug.LogError("JSON parsed, but response is NULL or empty");
                        yield break;
                    }

                    if(response.status == "success")
                    {
                        Debug.LogWarning("Result Set: " + response.message);
                    }
                    else
                    {
                        Debug.LogWarning("Server error: " + response.message);
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
}