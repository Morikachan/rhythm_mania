using Photon.Pun;
using UnityEngine;

public class SelectButton : MonoBehaviour {
    public RoomPhaseManager phaseManager;

    public void OnClickSelect()
    {
        if(SongDataHolder.instance == null) return;

        var song = SongDataHolder.instance;

        ExitGames.Client.Photon.Hashtable props =
            new ExitGames.Client.Photon.Hashtable
            {
                { "SelectState", "Selected" },
                { "SongID", song.SelectedSongId },
                { "SongName", song.SelectedSongName },
                { "SongLevel", song.SelectedSongLevel },
                { "SongBPM", song.SelectedSongBPM }
            };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        phaseManager.ShowRouletteLocally();
    }
}