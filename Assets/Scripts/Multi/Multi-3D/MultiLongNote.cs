using UnityEngine;
using Photon.Pun;

public class MultiLongNote : MonoBehaviour {
    [Header("Note Data")]
    public int lane;
    public float startTime;
    public float endTime;

    [Header("Refs")]
    private MultiJudge judge;
    private MultiNotesManager notes;
    private MultiGameManager gm;

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
    private float speed => MultiGameManager.instance.noteSpeed;

    private int localActor;

    //  INIT 
    public void Init(
        int lane,
        float start,
        float end,
        float judgeZ,
        float spawnOffset,
        MultiNotesManager nm,
        MultiJudge j)
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

    void Start()
    {
        gm = MultiGameManager.instance;
        localActor = gm.localActor;
    }

    void Update()
    {
        if(completed || !gm.musicManager.played)
            return;

        float songTime = gm.musicManager.GetMusicTime();

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

    //  VISUAL 
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

        bodyPart.localScale = new Vector3(1, 1, length);
        bodyPart.localPosition = new Vector3(0, 0, length * 0.5f);
        endPart.localPosition = new Vector3(0, 0, length + visualEndGap);
    }

    //  HOLD START 
    void TryStartHold(float zStart)
    {
        if(!IsKeyJustPressed())
            return;

        float dist = Mathf.Abs(zStart - judgeZ);

        if(dist <= 0.2f)
            StartHold(MultiJudge.JudgeType.Perfect);
        else if(dist <= 0.4f)
            StartHold(MultiJudge.JudgeType.Great);
    }

    void StartHold(MultiJudge.JudgeType type)
    {
        holding = true;
        nextTickTime = Time.time + tickInterval;

        judge.ShowJudge(type);
        gm.AddScore(localActor, type == MultiJudge.JudgeType.Perfect ? 1000 : 300);
    }

    //  TICK 
    void HandleTickScore()
    {
        if(releasedEarly) return;

        if(Time.time >= nextTickTime)
        {
            gm.AddScore(localActor, 50);
            nextTickTime += tickInterval;
        }
    }

    //  EARLY RELEASE 
    void CheckEarlyRelease()
    {
        if(releasedEarly) return;

        if(!IsKeyPressed())
        {
            releasedEarly = true;
            judge.ShowJudge(MultiJudge.JudgeType.Bad);
            gm.ResetCombo(localActor);
            gm.Damage(localActor, 50);
        }
    }

    //  END HOLD 
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
            gm.AddScore(localActor, 500);
            judge.ShowJudge(MultiJudge.JudgeType.Perfect);
        }

        Destroy(gameObject, 0.05f);
    }

    //  MISS 
    void Miss()
    {
        completed = true;

        gm.ResetCombo(localActor);
        gm.Damage(localActor, 100);
        judge.ShowMissEffect();

        if(notes)
            notes.ShowMissFromLong();

        Destroy(gameObject);
    }

    //  INPUT 
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
