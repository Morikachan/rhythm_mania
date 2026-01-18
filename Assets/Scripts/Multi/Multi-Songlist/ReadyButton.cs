using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class ReadyButton : MonoBehaviour {
    public void OnClickReady()
    {
        Hashtable props = new Hashtable
        {
            { "Ready", true }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
