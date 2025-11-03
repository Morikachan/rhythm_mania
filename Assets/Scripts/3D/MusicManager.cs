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

    void Update()
    {
        if(played && !audioSource.isPlaying && audioSource.time > 0)
        {
            StartCoroutine(EndGame());
        }
    }

    IEnumerator EndGame()
    {
        finishText.SetActive(true);
        yield return new WaitForSeconds(1);

        SceneManager.LoadScene("ResultScene");
    }
}