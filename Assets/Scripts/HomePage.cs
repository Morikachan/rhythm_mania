using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HomePage : MonoBehaviour
{
    public Image homeImage;

    private const int CURRENT_CARD_ID = 1;

    [System.Serializable]
    public class ServerData
    {
        public string user_id;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
    }

    void Awake()
    {
        StartCoroutine(WaitForCardLoaderAndDisplay());
    }

    //    [System.Serializable]
    //    public class MyReceivedData {
    //        public string message;
    //        public int status;
    //    }

    //    public string receiveUrl = "http://localhost/rhythm_mania/Database/user-home-info.php";
    //    void Start()
    //    {
    //        //StartCoroutine(GetJsonData());
    //    }


    //    IEnumerator GetJsonData()
    //    {
    //        using(UnityWebRequest request = UnityWebRequest.Get(receiveUrl))
    //        {
    //            yield return request.SendWebRequest();

    //            if(request.result == UnityWebRequest.Result.Success)
    //            {
    //                string jsonResponse = request.downloadHandler.text;
    //                Debug.Log("Received JSON: " + jsonResponse);

    //                MyReceivedData receivedData = JsonUtility.FromJson<MyReceivedData>(jsonResponse);
    //                Debug.Log($"Message: {receivedData.message}, Status: {receivedData.status}");
    //            }
    //            else
    //            {
    //                Debug.LogError("Error receiving JSON: " + request.error);
    //            }
    //        }
    //    }

    IEnumerator WaitForCardLoaderAndDisplay()
    {
        if (homeImage == null)
        {
            Debug.LogError("HomePage: Target Image (homeImage) is not assigned in the Inspector. Please assign it!");
            yield break;
        }

        while (CardLoader.Instance == null)
        {
            Debug.LogWarning("HomePage is waiting for CardLoader to initialize...");
            yield return null;
        }

        DisplayCard(CURRENT_CARD_ID);
    }

    public void DisplayCard(int newCardID)
    {
        if (homeImage == null) return;

        Debug.Log($"HomePage: CardLoader is ready. Loading Card ID {newCardID} into homeImage.");
        CardLoader.Instance.LoadCardIllustration(newCardID, homeImage);
    }
}


//using UnityEngine;
//using UnityEngine.Networking; // For HTTP-call
//using System.Collections;
//using UnityEngine.UI;

//public class HomePage : MonoBehaviour
//{
//    public Image homeImage;
//    private int currentCardID = 1;

//    [System.Serializable]
//    public class MyReceivedData {
//        public string message;
//        public int status;
//    }

//    public string receiveUrl = "http://localhost/rhythm_mania/Database/user-home-info.php";
//    void Start()
//    {
//        //StartCoroutine(GetJsonData());
//    }

//    void Awake()
//    {
//        if (CardLoader.Instance != null)
//        {
//            DisplayCard(currentCardID);
//        }
//        else
//        {
//            StartCoroutine(WaitForCardLoader());
//        }
//    }

//    IEnumerator WaitForCardLoader()
//    {
//        yield return new WaitForEndOfFrame();

//        if (CardLoader.Instance != null)
//        {
//            DisplayCard(currentCardID);
//        }
//        else
//        {
//            Debug.LogError("CardLoader Singleton could not be found!");
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    IEnumerator GetJsonData()
//    {
//        using(UnityWebRequest request = UnityWebRequest.Get(receiveUrl))
//        {
//            yield return request.SendWebRequest();

//            if(request.result == UnityWebRequest.Result.Success)
//            {
//                string jsonResponse = request.downloadHandler.text;
//                Debug.Log("Received JSON: " + jsonResponse);

//                MyReceivedData receivedData = JsonUtility.FromJson<MyReceivedData>(jsonResponse);
//                Debug.Log($"Message: {receivedData.message}, Status: {receivedData.status}");
//            }
//            else
//            {
//                Debug.LogError("Error receiving JSON: " + request.error);
//            }
//        }
//    }
//}
