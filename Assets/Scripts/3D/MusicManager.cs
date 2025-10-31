using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic()
    {
        if (audioSource.clip == null)
        {
            string songName = "Blank-Kytt-RSPN";
            audioSource.clip = Resources.Load<AudioClip>("Musics/" + songName);
        }

        if (audioSource.clip)
        {
            audioSource.Play();
            Debug.Log("Music started!");
        }
        else Debug.LogError("Music clip not found in Resources/Musics/");
    }
}