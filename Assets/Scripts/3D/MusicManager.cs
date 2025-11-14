using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;
    public bool played = false;

    public GameObject finishText;

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
            played = true;
            audioSource.Play();
            Debug.Log("Music started!");
        }
        else Debug.LogError("Music clip not found in Resources/Musics/");
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

    void Update()
    {
        if (played && audioSource.isPlaying)
        {
            if (audioSource.time >= audioSource.clip.length - 0.1f)
            {
                played = false;
                StartCoroutine(EndGame());
            }
        }
    }

    IEnumerator EndGame()
    {
        finishText.SetActive(true);
        yield return new WaitForSeconds(1);

        SceneManager.LoadScene("ResultScene");
    }
}