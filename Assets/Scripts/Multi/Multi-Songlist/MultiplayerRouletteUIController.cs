using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class MultiplayerRouletteUIController : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI p1Status;
    public TextMeshProUGUI p2Status;

    private bool rouletteStarted = false;

    void Update()
    {
        UpdateStatusUI();
        TryStartRoulette();
    }

    void UpdateStatusUI()
    {
        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length < 2) return;

        p1Status.text = GetState(players[0]);
        p2Status.text = GetState(players[1]);
    }

    string GetState(Player p)
    {
        if (!p.CustomProperties.ContainsKey("SelectState"))
            return "Selecting...";

        return p.CustomProperties["SelectState"].ToString();
    }

    void TryStartRoulette()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (rouletteStarted) return;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("SelectState")) return;
            if (p.CustomProperties["SelectState"].ToString() == "Selecting")
                return;
        }

        rouletteStarted = true;
        photonView.RPC(nameof(StartRoulette), RpcTarget.All);
    }

    [PunRPC]
    void StartRoulette()
    {
        Debug.Log("Start Roulette!");
    }
}