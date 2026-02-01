using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private string songName;
    public AudioSource audioSource;
    public bool played = false;

    public bool isMulti = false;

    public GameObject finishText;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (SongDataHolder.instance != null)
        {
            songName = SongDataHolder.instance.SelectedSongName;
        }
        else
        {
            Debug.LogError("No song selected! Starting with default song.");
            SceneManager.LoadScene("HomeScreen");
        }
    }

    void Update()
    {
        if(played && audioSource.isPlaying)
        {
            if(audioSource.time >= audioSource.clip.length - 0.1f && !isMulti)
            {
                played = false;
                StartCoroutine(EndGame());
            }
        }
    }

    public void PlayMusic(float delay)
    {
        if (audioSource.clip == null)
        {
            audioSource.clip = Resources.Load<AudioClip>("Musics/" + songName);
        }

        if(audioSource.clip)
        {
            StartCoroutine(PlayDelayedRoutine(delay));
        }
        else Debug.LogError("Music clip not found in Resources/Musics/");
    }

    IEnumerator PlayDelayedRoutine(float delay)
    {
        // Wait exactly Offset
        yield return new WaitForSeconds(delay);

        played = true;
        audioSource.Play();
        Debug.Log($"Music started after {delay} sec delay!");
    }

    public void PauseAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    public void ResumeAudio()
    {
        if (audioSource != null && !audioSource.isPlaying && played)
        {
            audioSource.UnPause();
        }
    }

    public void ResetMusic()
    {
        StopAllCoroutines();

        if(audioSource.clip == null)
        {
            audioSource.clip = Resources.Load<AudioClip>("Musics/" + songName);
        }

        audioSource.Stop();
        audioSource.time = 0f;
        played = false;
    }

    public void SetSongByName(string newSongName)
    {
        songName = newSongName;

        audioSource.Stop();
        audioSource.clip = null;
        played = false;

        AudioClip clip = Resources.Load<AudioClip>("Musics/" + songName);

        if(clip != null)
        {
            audioSource.clip = clip;
            Debug.Log($"[MUSIC] Song set to: {songName}");
        }
        else
        {
            Debug.LogError($"[MUSIC] Clip not found: Musics/{songName}");
        }
    }

    public float GetMusicTime()
    {
        return audioSource.time;
    }

    IEnumerator EndGame()
    {
        finishText.SetActive(true);
        yield return new WaitForSeconds(1);

        SceneManager.LoadScene("ResultScene");
    }
}