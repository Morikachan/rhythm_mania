using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class SelectButton : MonoBehaviour {
    public void OnClickSelect()
    {
        if(SongDataHolder.instance == null)
        {
            Debug.LogError("SongDataHolder missing");
            return;
        }

        var song = SongDataHolder.instance;

        Hashtable props = new Hashtable
        {
            { "SelectState", "Selected" },
            { "SongID", song.SelectedSongId },
            { "SongName", song.SelectedSongName },
            { "SongLevel", song.SelectedSongLevel },
            { "SongBPM", song.SelectedSongBPM }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
