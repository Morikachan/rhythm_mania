using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class RecommendButton : MonoBehaviour {
    public void OnClickRecommend()
    {
        Hashtable props = new Hashtable
        {
            { "SelectState", "Recommend" }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
