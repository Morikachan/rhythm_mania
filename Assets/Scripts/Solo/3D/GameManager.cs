using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public float maxScore;
    public float ratioScore;

    //public int songID;
    [Header("Game Settings")]
    public float noteSpeed = 5f;
    public float startDelay = 2f; // Delay before start

    public bool gameStarted;
    public bool gameEnded;

    public float startTime;

    [Header("Score Info")]
    public int combo;
    public int score;

    public int perfect;
    public int great;
    public int good;
    public int bad;
    public int miss;

    [Header("References")]
    [SerializeField] public TextMeshProUGUI comboText;
    [SerializeField] public TextMeshProUGUI scoreText;
    public NotesManager notesManager;
    public MusicManager musicManager;

    public GameObject keyLine;
    [SerializeField] GameObject gameOverPopup;

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        HPManager.instance.ResetHP();
        StartCoroutine(StartGameAfterDelay());
    }

    IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        if(keyLine != null)
        {
            keyLine.SetActive(false);
        }

        gameStarted = false;
        gameEnded = false;

        startTime = Time.time;

        musicManager.ResetMusic();

        float musicDelay = notesManager.spawnOffset / noteSpeed;
        musicManager.PlayMusic(musicDelay);


        gameStarted = true;

        notesManager.StartGame();
    }


    public void GameOver()
    {
        if(gameEnded) return;

        gameEnded = true;
        gameStarted = false;

        musicManager.PauseAudio();

        Time.timeScale = 0;

        gameOverPopup.SetActive(true);
    }

    public void ResetGame()
    {
        score = 0;
        perfect = 0;
        great = 0;
        bad = 0;
        miss = 0;
        combo = 0;
        maxScore = 0;
        ratioScore = 0;

        gameStarted = false;
        gameEnded = false;

        notesManager.started = false;

        Time.timeScale = 1;

        musicManager.PauseAudio();
        musicManager.ResetMusic();
        HPManager.instance.ResetHP();
    }

    public void AddScore(int value)
    {
        score += value;
        combo++;
        UpdateUI();
    }

    public void ResetCombo()
    {
        combo = 0;
        UpdateUI();
    }
    public void UpdateUI()
    {
        if (comboText) comboText.text = combo.ToString();
        if (scoreText) scoreText.text = score.ToString();
    }
}
