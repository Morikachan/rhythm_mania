using UnityEngine;

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

    [Header("Tick Settings")]
    private float tickInterval;

    private int totalTicks;
    private int processedTicks;

    private bool tickStarted = false;
    private float tickStartTime;
    private bool completed = false;

    [Header("Visual")]
    [SerializeField] private float visualEndGap = 0.05f;

    private float judgeZ;
    private float spawnOffset;
    private float speed => MultiGameManager.instance.noteSpeed;

    private int localActor;

    // INIT

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

        totalTicks = 5;

        tickInterval = (endTime - startTime) / totalTicks;
        tickInterval = Mathf.Max(0.05f, tickInterval);
    }

    void Start()
    {
        gm = MultiGameManager.instance;
        localActor = gm.localActor;
    }

    // UPDATE

    void Update()
    {
        if(completed || !gm.musicManager.played)
            return;

        float songTime = Time.time - gm.startTime;

        float zStart = judgeZ + (startTime - songTime) * speed + spawnOffset;
        float zEnd = judgeZ + (endTime - songTime) * speed + spawnOffset;

        UpdateVisual(zStart, zEnd);

        TryStartTicks(zStart);
        HandleTicks();

        if(zEnd < judgeZ - 0.4f)
            FinishNote();
    }

    // VISUAL

    void UpdateVisual(float zStart, float zEnd)
    {
        float length;

        if(!tickStarted)
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

        bodyPart.localScale = new Vector3(1, 0.01f, length);
        bodyPart.localPosition = new Vector3(0, 0, length * 0.5f);
        endPart.localPosition = new Vector3(0, 0, length + visualEndGap);
    }

    // START TICKS

    void TryStartTicks(float zStart)
    {
        if(tickStarted) return;

        if(zStart <= judgeZ)
        {
            tickStarted = true;
            tickStartTime = Time.time;
        }
    }

    // TICKS

    void HandleTicks()
    {
        if(!tickStarted) return;

        while(processedTicks < totalTicks &&
               Time.time >= tickStartTime + processedTicks * tickInterval)
        {
            ProcessTick();
            processedTicks++;
        }
    }

    void ProcessTick()
    {
        if(IsKeyPressed())
        {
            gm.SendJudge(localActor, MultiJudge.JudgeType.Perfect, 50);
            judge.ShowJudge(MultiJudge.JudgeType.Perfect);
        }
        else
        {
            gm.SendJudge(localActor, MultiJudge.JudgeType.Miss);
            judge.ShowJudge(MultiJudge.JudgeType.Miss);

            if(notes)
                notes.ShowMissFromLong();
        }
    }

    // END

    void FinishNote()
    {
        completed = true;
        Destroy(gameObject, 0.05f);
    }

    // INPUT

    float LaneToX(int lane) => lane - 1.5f;

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
