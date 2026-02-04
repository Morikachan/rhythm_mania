using UnityEngine;

public class SampleSongManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }

        audioSource = GetComponent<AudioSource>();

        if(audioSource == null)
        {
            Debug.LogWarning("AudioSource missing, adding automatically");
            audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
        }
    }

    public void PlayMusic(string songName)
    {
        if(audioSource == null)
            return;

        audioSource.Stop();
        
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
