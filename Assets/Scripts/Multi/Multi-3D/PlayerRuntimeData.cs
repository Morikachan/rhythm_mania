using UnityEngine;

[System.Serializable]
public class PlayerRuntimeData
{
    public int actorNumber;

    public int score;
    public int combo;

    public int perfect;
    public int great;
    public int bad;
    public int miss;

    public int maxHP = 1000;
    public int hp;

    public float multiplier = 1f;

    public float Accuracy
    {
        get
        {
            int total = perfect + great + bad + miss;
            if (total == 0) return 0f;

            float hit = perfect + great * 0.7f;
            return hit / total;
        }
    }
}
