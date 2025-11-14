using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using System.Text;
//using UnityEditor.PackageManager.Requests;

public class SongListManager : MonoBehaviour
{
    public GameObject songPanelPrefab;
    public Transform contentParent;

    [Header("Info Panel References")]
    //public TextMeshProUGUI infoSongName;
    //public TextMeshProUGUI infoLevel;
    //public TextMeshProUGUI infoBPM;

    private SongPanelController currentSelectedPanel;

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

                    //CURRENT_CARD_ID = receivedData.user_info.home_card_id;

                    //StartCoroutine(WaitForCardLoaderAndDisplay());
                    //UpdateProgress(receivedData.user_info.next_lvl_percent);

                    //starsValue.text = (receivedData.user_inventory.paid_gems + receivedData.user_inventory.free_gems).ToString();
                    //coinsValue.text = receivedData.user_inventory.coins.ToString();
                    //levelValue.text = "Level " + receivedData.user_info.user_lvl;
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
        // selectedSong = song;
        print("manager: SelectSong");
        // 1. DESELECT the previous panel
        if(currentSelectedPanel != null)
        {
            currentSelectedPanel.SetSelected(false);
        }

        // 2. SELECT the new panel
        currentSelectedPanel = newPanelController;
        currentSelectedPanel.SetSelected(true);

        // Update the right-side information panel
        // infoSongName.text = song.song_name;
        // infoLevel.text = "Level " + song.song_level.ToString();
        // infoBPM.text = "BPM: " + song.song_bpm.ToString();
        // TODO Image load here
    }

    private void CreateSongPanels(List<Song> songs)
    {
        if (songPanelPrefab == null || contentParent == null)
        {
            Debug.LogError("Prefab or Content Parent not assigned in SongListManager!");
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