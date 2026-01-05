using UnityEngine;
using static Judge;

public class HPManager : MonoBehaviour
{
    public static HPManager instance;

    public int maxHP = 1000;
    public int currentHP;

    public HPBarUI hpUI;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void ResetHP()
    {
        currentHP = maxHP;
        hpUI.UpdateHP();
    }

    public void ApplyJudge(JudgeType judge)
    {
        switch(judge)
        {
            case JudgeType.Bad:
                Damage(50);
                break;

            case JudgeType.Miss:
                Damage(100);
                break;
        }
    }

    private void Damage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(0, currentHP);

        hpUI.UpdateHP();

        if(currentHP <= 0)
        {
            GameManager.instance.GameOver();
        }
    }
}
