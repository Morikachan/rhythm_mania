using UnityEngine;

public class SongDataHolder : MonoBehaviour
{
    public static SongDataHolder instance;

    public string SelectedSongName {  get; private set; }
    public int SelectedSongId { get; private set; }

    //public SongListManager.Song FullSelectedSongData { get; private set; }

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
        SelectedSongName = song.song_name;
        SelectedSongId = song.song_id;
        //FullSelectedSongData = song;
    }
}
