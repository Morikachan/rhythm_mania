using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PhotonBootstrap : MonoBehaviourPunCallbacks
{
    private static PhotonBootstrap instance;

    private const string USER_NAME_KEY = "UserName";
    private const string HOME_CARD_ID_KEY = "HomeCardID";

    private const string PROP_USERNAME = "UserName";
    private const string PROP_CARD_ID = "CardID";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
      
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = "0.0.1";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon connected (global)");

        string userName = PlayerPrefs.GetString(USER_NAME_KEY, "Player");
        int cardId = PlayerPrefs.GetInt(HOME_CARD_ID_KEY, 1);

        Hashtable props = new Hashtable
        {
            { PROP_USERNAME, userName },
            { PROP_CARD_ID, cardId }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}