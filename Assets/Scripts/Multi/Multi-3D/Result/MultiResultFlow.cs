using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class MultiResultFlow : MonoBehaviourPunCallbacks {
    public GameObject multiResult;
    public GameObject playerResult;
    public MultiPlayerResult playerResultScript;


    private bool showedPlayer = false;
    private bool manuallyLeaving = false;

    private const string USER_ID_KEY = "UserID";
    public string url = "http://153.126.183.193/student/k248010/rhythm_mania_db/update_user_song.php";

    private int score, perfect, great, bad, miss;
    private string rank;
    private bool dataSent = false;

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
    }

    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
        }

        if(!dataSent)
        {
            LoadLocalPlayerData();
            rank = CalculateRank();
            StartCoroutine(SendJsonData());
            dataSent = true;
        }
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

    string CalculateRank()
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

    public void OnNext()
    {
        if(!showedPlayer)
        {
            multiResult.SetActive(false);
            playerResult.SetActive(true);

            showedPlayer = true;
        }
        else
        {
            manuallyLeaving = true;
            LeaveAndReset();
        }
    }

    //public void LeaveAndReset()
    //{
    //    ExitGames.Client.Photon.Hashtable props =
    //        new ExitGames.Client.Photon.Hashtable
    //        {
    //            { "Ready", false },
    //            { "SelectState", "Selecting" },
    //            { "SongID", -1 }
    //        };

    //    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

    //    if(MultiResultDataHolder.instance != null)
    //        Destroy(MultiResultDataHolder.instance.gameObject);

    //    if(PhotonNetwork.InRoom)
    //    {
    //        SongDataHolder.instance.SetMultiLive(false);
    //        PhotonNetwork.LeaveRoom();
    //    }
    //    else
    //        SceneManager.LoadScene("HomeScreen");
    //}

    public void LeaveAndReset()
    {
        Time.timeScale = 1;

        // Reset PLAYER props
        ExitGames.Client.Photon.Hashtable playerProps =
            new ExitGames.Client.Photon.Hashtable
            {
            { "Ready", false },
            { "SelectState", "Selecting" },
            { "SongID", -1 },
            { "SongName", "" },
            { "SongLevel", "" },
            { "SongBPM", "" },

            { "Score", 0 },
            { "Perfect", 0 },
            { "Great", 0 },
            { "Bad", 0 },
            { "Miss", 0 },
            { "Combo", 0 },
            { "HP", 0 }
            };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        // Reset ROOM props (ONLY MASTER)
        if(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            ExitGames.Client.Photon.Hashtable roomProps =
                new ExitGames.Client.Photon.Hashtable
                {
                { "FinalSongID", -1 },
                { "FinalSongName", "" },
                { "WinnerIndex", -1 }
                };

            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }

        // Destroy data holders
        if(MultiResultDataHolder.instance != null)
            Destroy(MultiResultDataHolder.instance.gameObject);

        if(SongDataHolder.instance != null)
        {
            SongDataHolder.instance.SetMultiLive(false);
        }


        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("HomeScreen");
        }
    }

    public override void OnLeftRoom()
    {
        if(!manuallyLeaving)
        {
            return; // DO NOTHING
        }

        SceneManager.LoadScene("HomeScreen");
    }

    IEnumerator SendJsonData()
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
            song_id = songId,
            combo = rank,
            score = score
        };

        string jsonString = JsonUtility.ToJson(dataToSend);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        using(UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if(request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending result: " + request.error);
            }
        }
    }
}
