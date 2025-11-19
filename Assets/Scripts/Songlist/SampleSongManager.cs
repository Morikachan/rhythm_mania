using UnityEngine;

public class SampleSongManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic(string songName)
    {
        audioSource.clip = Resources.Load<AudioClip>("Musics/" + songName);

        if (audioSource.clip)
        {
            audioSource.Play();
        }
    }
}
