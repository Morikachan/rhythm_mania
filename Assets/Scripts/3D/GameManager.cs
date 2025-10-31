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
    public float startDelay = 3f; // Delay before start
    public bool started = false;
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
        StartCoroutine(StartGameAfterDelay());
    }

    IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        started = true;
        startTime = Time.time;

        notesManager.StartGame();
        musicManager.PlayMusic();
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
