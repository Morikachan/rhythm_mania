using UnityEngine;

public class SampleSongManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null)
        {
            Debug.LogWarning("AudioSource missing, adding automatically");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayMusic(string songName)
    {
        if(audioSource == null)
            return;

        AudioClip clip = Resources.Load<AudioClip>("Musics/" + songName);

        if(clip == null)
        {
            Debug.LogWarning($"Music not found: {songName}");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }
}
