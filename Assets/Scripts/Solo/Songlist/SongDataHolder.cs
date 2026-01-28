using Photon.Pun;
using UnityEngine;

public class SongDataHolder : MonoBehaviour
{
    public static SongDataHolder instance;

    public string SelectedSongName {  get; private set; }
    public int SelectedSongId { get; private set; }
    public int SelectedSongLevel { get; private set; }
    public int SelectedSongBPM { get; private set; }

    public bool IsMultiLive { get; private set; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Important for persistence
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }

    public void SetSelectedSong(SongListManager.Song song)
    {
        if(IsMultiLive)
        {
            Debug.LogWarning("[SongDataHolder] SetSelectedSong ignored in Multi Live");
            return;
        }

        ApplySong(song);
    }

    public void SetMultiLive()
    {
        IsMultiLive = true;
    }

    public bool SyncFromRoom()
    {
        if(!IsMultiLive)
            return false;

        if(PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("[SongDataHolder] Room is NULL");
            return false;
        }

        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        if(!props.ContainsKey("FinalSongID") || !props.ContainsKey("FinalSongName"))
        {
            Debug.LogError("[SongDataHolder] FinalSong data NOT FOUND");
            return false;
        }

        SelectedSongId = (int)props["FinalSongID"];
        SelectedSongName = props["FinalSongName"].ToString();

        Debug.Log($"[SongDataHolder][MULTI] Synced from Room: {SelectedSongId}");
        return true;
    }

    private void ApplySong(SongListManager.Song song)
    {
        SelectedSongName = song.song_name;
        SelectedSongId = song.song_id;
        SelectedSongLevel = song.song_level;
        SelectedSongBPM = song.song_bpm;
    }

    public void SetMultiLive(bool value)
    {
        IsMultiLive = value;
    }
}
