using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class RankingManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rankingPanel;
    public Transform contentParent;
    public GameObject rankingUserPrefab;

    public Button openRankingButton;
    public Button closeRankingButton;

    [Header("Settings")]
    public string receiveUrl = "http://localhost/rhythm_mania/Database/get-song-ranking.php";

    [System.Serializable]
    public class RequestData
    {
        public int song_id;
    }

    [System.Serializable]
    public class RankingResponse
    {
        public string status;
        public string message;
        public List<RankingUser> ranking;
    }

    [System.Serializable]
    public class RankingUser
    {
        public string username;
        public string user_id;
        public int best_score;
        public string best_combo;
        public int rank;
        public int home_card_id;
    }

    private void Start()
    {
        if (openRankingButton != null)
        {
            openRankingButton.onClick.AddListener(OnRankingButtonClicked);
        }
        else
        {
            Debug.LogWarning("Ranking Button Ñ~Ñu ÑÅÑÇÑyÑrÑëÑxÑpÑ~Ñp Ñr ÑyÑ~ÑÉÑÅÑuÑ{ÑÑÑÄÑÇÑu RankingManager!");
        }

        if (closeRankingButton != null)
        {
            closeRankingButton.onClick.AddListener(CloseRanking);
        }
        else
        {
            Debug.LogWarning("Ranking Button Ñ~Ñu ÑÅÑÇÑyÑrÑëÑxÑpÑ~Ñp Ñr ÑyÑ~ÑÉÑÅÑuÑ{ÑÑÑÄÑÇÑu RankingManager!");
        }
    }

    public void OnRankingButtonClicked()
    {
        if (SongDataHolder.instance == null)
        {
            Debug.LogError("SongDataHolder instance not found!");
            return;
        }

        int currentSongId = SongDataHolder.instance.SelectedSongId;

        if (currentSongId != 0)
        {
            OpenRanking(currentSongId);
        }
        else
        {
            Debug.LogWarning("No song selected (SelectedSongId is 0). Please select a song first.");
        }
    }

    public async void OpenRanking(int songId)
    {
        rankingPanel.SetActive(true);
        ClearList();
        await GetRankingData(songId);
    }

    public void CloseRanking()
    {
        rankingPanel.SetActive(false);
    }

    private void ClearList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private async Task GetRankingData(int songId)
    {
        RequestData dataToSend = new RequestData { song_id = songId };
        string jsonString = JsonUtility.ToJson(dataToSend);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest request = new UnityWebRequest(receiveUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                try
                {
                    RankingResponse response = JsonUtility.FromJson<RankingResponse>(jsonResponse);

                    if (response != null && response.status == "success")
                    {
                        CreateRankingList(response.ranking);
                    }
                    else
                    {
                        Debug.Log("Server message: " + (response != null ? response.message : "No response"));
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parse Error: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Network Error: " + request.error);
            }
        }
    }

    private void CreateRankingList(List<RankingUser> rankingList)
    {
        if (rankingList == null || rankingList.Count == 0) return;

        foreach (var user in rankingList)
        {
            GameObject newItem = Instantiate(rankingUserPrefab, contentParent);
            RankingUserItem itemScript = newItem.GetComponent<RankingUserItem>();

            if (itemScript != null)
            {
                itemScript.Setup(
                    user.rank,
                    user.username,
                    user.best_score,
                    user.best_combo,
                    user.home_card_id
                );
            }
        }
    }
}