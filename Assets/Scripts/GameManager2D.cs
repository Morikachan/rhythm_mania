using UnityEngine;
using UnityEngine.UI;

public class GameManager2D : MonoBehaviour
{
    public AudioSource theMusic;
    public bool startPlaying;
    public BeatScroler theBS;

    public static GameManager2D instance;

    public int currentScore;
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;

    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThresholds;

    public Text scoreText;
    public Text multiText;

    public float totalNotes;
    public float normalHits;
    public float goodHits;
    public float perfectHits;
    public float missedHits;

    public GameObject resultScreen;
    public Text percentHitText, normalsText, goodsText, perfectsText, missesText, rankText, finalScoreText;

    void Start()
    {
        instance = this;

        currentMultiplier = 1;
        scoreText.text = "Score: 0";

        totalNotes = FindObjectsOfType<NoteObject>().Length;
    }

    void Update()
    {
        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                startPlaying = true;
                theBS.hasStarted = true;

                theMusic.Play();
            }
        }
        else
        {
            if (!theMusic.isPlaying && resultScreen.activeInHierarchy)
            {
                resultScreen.SetActive(true);

                normalsText.text = "" + normalHits;
                goodsText.text = "" + goodHits;
                percentHitText.text = "" + perfectHits;
                missesText.text = "" + missesText;

                finalScoreText.text = currentScore.ToString();

                float totalHit = normalHits + goodHits + perfectHits;
                float percentHit = (totalHit / totalNotes) * 100;
                percentHitText.text = "" + Mathf.Floor(percentHit) + "%";

                string rankVal = "F";
                if(percentHit > 40)
                {
                    rankVal = "D";
                } else if (percentHit > 55)
                {
                    rankVal = "C";
                } else if (percentHit > 70)
                {
                    rankVal = "B";
                } else if (percentHit > 85)
                {
                    rankVal = "A";
                } else if (percentHit > 95 && percentHit < 100)
                {
                    rankVal = "S";
                } else
                {
                    rankVal = "SS";
                }

                rankText.text = rankVal;
            }
        }
    }

    public void NormalHit()
    {
        currentScore += scorePerNote * currentMultiplier;
        NoteHit();
        normalHits++;
    }

    public void GoodHit()
    {
        currentScore += scorePerGoodNote * currentMultiplier;
        NoteHit();
        goodHits++;
    }

    public void PerfectHit()
    {
        currentScore += scorePerPerfectNote * currentMultiplier;
        NoteHit();
        perfectHits++;
    }

    public void NoteHit()
    {
        if (currentMultiplier - 1 < multiplierThresholds.Length) {
            multiplierTracker++;

            if (multiplierThresholds[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
            }
        }

        multiText.text = "Multiplier: x" + currentMultiplier;

        scoreText.text = "Score: " + currentScore;
    }

    public void NoteMissed()
    {
        currentMultiplier = 1;
        multiplierTracker = 0;
        multiText.text = "Multiplier: x" + currentMultiplier;
        missedHits++;
    }
}
