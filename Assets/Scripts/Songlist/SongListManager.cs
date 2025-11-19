using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class SongListManager : MonoBehaviour
{
    public GameObject songPanelPrefab;
    public Transform contentParent;

    public Image songIllustImage;

    [Header("Info Panel References")]
    public TextMeshProUGUI infoSongCombo;
    public TextMeshProUGUI infoSongScore;


    private SongPanelController currentSelectedPanel;
    public SampleSongManager sampleSongManager;

    private const string USER_ID_KEY = "UserID";

    public string receiveUrl = "http://localhost/rhythm_mania/Database/get-songlist.php";

    [System.Serializable]
    public class UserData
    {
        public string user_id;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
        public List<Song> song_list;
    }

    [System.Serializable]
    public class Song
    {
        public int song_id;
        public string song_name;
        public int song_level;
        public int song_bpm;
        public int best_score;
        public string best_combo;
    };

    void Start()
    {
        StartCoroutine(GetJsonData());
    }

    IEnumerator GetJsonData()
    {
        UserData dataToSend = null;

        if (PlayerPrefs.HasKey(USER_ID_KEY))
        {
            dataToSend = new UserData
            {
                user_id = PlayerPrefs.GetString(USER_ID_KEY),
            };
        };

        if (dataToSend != null)
        {
            string jsonString = JsonUtility.ToJson(dataToSend);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);


            using (UnityWebRequest request = UnityWebRequest.Get(receiveUrl))
            {

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("Received JSON: " + jsonResponse);

                    ServerResponse receivedData = JsonUtility.FromJson<ServerResponse>(jsonResponse);

                    // Create the songs' panels
                    CreateSongPanels(receivedData.song_list);
                }
                else
                {
                    Debug.LogError("Error receiving JSON: " + request.error);
                }
            }
        }
    }
    public void SelectSong(SongPanelController newPanelController, Song song)
    {
        // 1. DESELECT the previous panel
        if(currentSelectedPanel != null)
        {
            currentSelectedPanel.SetSelected(false);
        }

        // 2. SELECT the new panel
        currentSelectedPanel = newPanelController;
        currentSelectedPanel.SetSelected(true);

        if(SongDataHolder.instance != null)
        {
            SongDataHolder.instance.SetSelectedSong(song);
        }

        // Update the right-side information panel
         infoSongCombo.text = song.best_combo;
         infoSongScore.text = song.best_score.ToString();
        // TODO Image load here
        DisplaySongIllust(song.song_id);

        sampleSongManager.PlayMusic(song.song_name);
    }

    public void DisplaySongIllust(int songId)
    {
        if (songIllustImage == null) return;

        string fileName = $"song_illust_{songId}.png";
        SongIllustLoader.instance.LoadSongIllustration(songIllustImage, fileName);
    }

    private void CreateSongPanels(List<Song> songs)
    {
        if (songPanelPrefab == null || contentParent == null)
        {
            Debug.LogError("Prefab or Content Parent not assigned in SongListManager");
            return;
        }

        SongPanelController firstPanelController = null;
        Song firstSongData = null;

        foreach (Song song in songs)
        {
            // 1. Instantiate the prefab inside the Content
            GameObject newPanelObj = Instantiate(songPanelPrefab, contentParent);

            // 2. Get the controller script attached to the new panel
            SongPanelController controller = newPanelObj.GetComponent<SongPanelController>();

            if (controller != null)
            {
                controller.Setup(song, this);

                if (firstPanelController == null)
                {
                    firstPanelController = controller;
                    firstSongData = song;
                }
            }

            if (firstPanelController != null)
            {
                SelectSong(firstPanelController, firstSongData);
            }
        }
    }
}