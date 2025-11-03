using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Result : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text perfectText;
    [SerializeField] Text greatText;
    [SerializeField] Text badText;
    [SerializeField] Text misseText;

    void Start()
    {
        scoreText.text = GameManager.instance.score.ToString();
        perfectText.text = GameManager.instance.perfect.ToString();
        greatText.text = GameManager.instance.great.ToString();
        badText.text = GameManager.instance.bad.ToString();
        misseText.text = GameManager.instance.miss.ToString();
    }

    public void Retry()
    {
        GameManager.instance.score = 0;
        GameManager.instance.perfect = 0;
        GameManager.instance.great = 0;
        GameManager.instance.bad = 0;
        GameManager.instance.miss = 0;
        GameManager.instance.combo = 0;
        GameManager.instance.maxScore = 0;
        GameManager.instance.ratioScore = 0;

    }
}