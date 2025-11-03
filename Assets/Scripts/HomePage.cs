using UnityEngine;
using UnityEngine.Networking; // For HTTP-call
using System.Collections;

public class HomePage : MonoBehaviour
{
    [System.Serializable]
    public class MyReceivedData {
        public string message;
        public int status;
    }

    public string receiveUrl = "http://localhost/rhythm_mania/Database/user-home-info.php";

    IEnumerator GetJsonData()
    {
        using(UnityWebRequest request = UnityWebRequest.Get(receiveUrl))
        {
            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Received JSON: " + jsonResponse);

                MyReceivedData receivedData = JsonUtility.FromJson<MyReceivedData>(jsonResponse);
                Debug.Log($"Message: {receivedData.message}, Status: {receivedData.status}");
            }
            else
            {
                Debug.LogError("Error receiving JSON: " + request.error);
            }
        }
    }

    void Start()
    {
        StartCoroutine(GetJsonData());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
