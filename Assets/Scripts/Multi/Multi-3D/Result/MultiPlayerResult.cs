using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Photon.Pun;
using System.Collections;
using System.Text;
using UnityEngine.UI;

public class MultiPlayerResult : MonoBehaviour {
    [Header("UI")]
    public Text scoreText;
    public Text perfectText;
    public Text greatText;
    public Text badText;
    public Text missText;
    public Text rankText;

    private int score;
    private int perfect;
    private int great;
    private int bad;
    private int miss;
    private string rank;

    private bool sent = false;

    private const string USER_ID_KEY = "UserID";
    public string url = "http://localhost/rhythm_mania/Database/update_user_song.php";

    void Start()
    {
        LoadLocalPlayerData();
        CalculateRank();
        UpdateUI();
    }

    void LoadLocalPlayerData()
    {
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;

        var data = MultiResultDataHolder.instance.results[actor];

        score = data.score;
        perfect = data.perfect;
        great = data.great;
        bad = data.bad;
        miss = data.miss;
    }

    void CalculateRank()
    {
        int total = perfect + great + bad + miss;
        float hit = perfect + great;
        float percent = (hit / total) * 100f;

        if(bad == 0 && miss == 0) rank = "SS";
        else if(percent > 95) rank = "S";
        else if(percent > 75) rank = "A";
        else if(percent > 60) rank = "B";
        else if(percent > 40) rank = "C";
        else rank = "D";
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
        perfectText.text = perfect.ToString();
        greatText.text = great.ToString();
        badText.text = bad.ToString();
        missText.text = miss.ToString();
        rankText.text = rank;
    }

    public void SendResultToServer()
    {
        if(sent) return;
        sent = true;

        StartCoroutine(SendJson());
    }

    IEnumerator SendJson()
    {
        if(!PlayerPrefs.HasKey(USER_ID_KEY))
            yield break;

        int songId = SongDataHolder.instance != null
            ? SongDataHolder.instance.SelectedSongId
            : 0;

        UserData data = new UserData
        {
            user_id = PlayerPrefs.GetString(USER_ID_KEY),
            song_id = songId,
            combo = rank,
            score = score
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using(UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if(req.result != UnityWebRequest.Result.Success)
                Debug.LogError(req.error);
        }
    }

    [System.Serializable]
    class UserData {
        public string user_id;
        public int song_id;
        public string combo;
        public int score;
    }
}