using Photon.Pun;
using UnityEngine;

public class RecommendButton : MonoBehaviour {
    public RoomPhaseManager phaseManager;

    public void OnClickRecommend()
    {
        ExitGames.Client.Photon.Hashtable props =
            new ExitGames.Client.Photon.Hashtable
            {
                { "SelectState", "Recommend" }
            };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        phaseManager.ShowRouletteLocally();
    }
}