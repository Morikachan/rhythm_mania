using UnityEngine;

public class LongNote : MonoBehaviour {
    [Header("Note Data")]
    public int lane;
    public float startTime;
    public float endTime;

    [Header("Refs")]
    private Judge judge;
    private NotesManager notes;

    [Header("Parts")]
    private Transform startPart;
    private Transform bodyPart;
    private Transform endPart;

    [Header("State")]
    private bool holding = false;
    private bool completed = false;
    private bool releasedEarly = false;

    [Header("Tick")]
    [SerializeField] private float tickInterval = 0.15f;
    private float nextTickTime;

    [Header("Visual")]
    [SerializeField] private float visualEndGap = 0.05f;

    private float judgeZ;
    private float spawnOffset;
    private float speed => GameManager.instance.noteSpeed;

    public void Init(
        int lane,
        float start,
        float end,
        float judgeZ,
        float spawnOffset,
        NotesManager nm,
        Judge j)
    {
        this.lane = lane;
        startTime = start;
        endTime = end;
        this.judgeZ = judgeZ;
        this.spawnOffset = spawnOffset;

        notes = nm;
        judge = j;

        startPart = transform.Find("Start");
        bodyPart = transform.Find("Body");
        endPart = transform.Find("End");
    }

    void Update()
    {
        if(!GameManager.instance.gameStarted || completed)
            return;

        float songTime = Time.time - GameManager.instance.startTime;

        float zStart = judgeZ + (startTime - songTime) * speed + spawnOffset;
        float zEnd = judgeZ + (endTime - songTime) * speed + spawnOffset;

        UpdateVisual(zStart, zEnd);

        if(!holding)
        {
            TryStartHold(zStart);
        }
        else
        {
            HandleTickScore();
            CheckEarlyRelease();
            TryEndHold(zEnd);
        }

        if(!holding && zEnd < judgeZ - 0.4f)
            Miss();
    }

    void UpdateVisual(float zStart, float zEnd)
    {
        float length;

        if(!holding)
        {
            length = zEnd - zStart;
            if(length <= 0) return;

            transform.position = new Vector3(LaneToX(lane), 0.5f, zStart);
        }
        else
        {
            length = Mathf.Max(0, zEnd - judgeZ);
            transform.position = new Vector3(LaneToX(lane), 0.5f, judgeZ);
        }

        // bodyPart.localScale = new Vector3(1, 0.01f, length);
        bodyPart.localScale = new Vector3(1, 1, length);
        bodyPart.localPosition = new Vector3(0, 0, length * 0.5f);

        endPart.localPosition = new Vector3(0, 0, length + visualEndGap);
    }

    void TryStartHold(float zStart)
    {
        if(!IsKeyJustPressed())
            return;

        float dist = Mathf.Abs(zStart - judgeZ);

        if(dist <= 0.2f)
            StartHold(true);
        else if(dist <= 0.4f)
            StartHold(false);
    }

    void StartHold(bool perfect)
    {
        holding = true;
        nextTickTime = Time.time + tickInterval;

        GameManager.instance.AddScore(perfect ? 1000 : 300);
        judge.ShowJudge(perfect ? 0 : 1);
    }

    // TICK SCORE
    void HandleTickScore()
    {
        if(releasedEarly) return;

        if(Time.time >= nextTickTime)
        {
            GameManager.instance.AddScore(50);
            nextTickTime += tickInterval;
        }
    }

    // EARLY RELEASE -> Bad
    void CheckEarlyRelease()
    {
        if(releasedEarly) return;

        if(!IsKeyPressed())
        {
            releasedEarly = true;
            judge.ShowJudge(2);
            HPManager.instance.ApplyJudge(Judge.JudgeType.Bad);
        }
    }

    // END HOLD
    void TryEndHold(float zEnd)
    {
        if(zEnd <= judgeZ)
            EndHold();
    }

    void EndHold()
    {
        completed = true;

        if(!releasedEarly)
        {
            GameManager.instance.AddScore(500);
            judge.ShowJudge(0);
        }

        Destroy(gameObject, 0.05f);
    }

    void Miss()
    {
        completed = true;

        GameManager.instance.miss++;
        GameManager.instance.ResetCombo();
        HPManager.instance.ApplyJudge(Judge.JudgeType.Miss);

        if(notes)
            notes.ShowMissFromLong();

        Destroy(gameObject);
    }

    // INPUT
    float LaneToX(int lane) => lane - 1.5f;

    bool IsKeyJustPressed()
    {
        return lane switch
        {
            0 => Input.GetKeyDown(KeyCode.D),
            1 => Input.GetKeyDown(KeyCode.F),
            2 => Input.GetKeyDown(KeyCode.J),
            3 => Input.GetKeyDown(KeyCode.K),
            _ => false
        };
    }

    bool IsKeyPressed()
    {
        return lane switch
        {
            0 => Input.GetKey(KeyCode.D),
            1 => Input.GetKey(KeyCode.F),
            2 => Input.GetKey(KeyCode.J),
            3 => Input.GetKey(KeyCode.K),
            _ => false
        };
    }
}
