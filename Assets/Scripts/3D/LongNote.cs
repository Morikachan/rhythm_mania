using UnityEngine;

public class LongNote : MonoBehaviour {
    public int lane;
    public float startTime;
    public float endTime;

    private float judgeZ;
    private float spawnOffset;

    private Transform startPart;
    private Transform bodyPart;
    private Transform endPart;

    private bool startedHold = false;
    private bool completed = false;

    private float speed => GameManager.instance.noteSpeed;

    private Judge judge;
    private NotesManager notes;

    public void Init(
        int lane, float start, float end,
        float judgeZ, float spawnOffset,
        NotesManager nm, Judge j)
    {
        this.lane = lane;
        this.startTime = start;
        this.endTime = end;
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
        if(!GameManager.instance.started || completed)
            return;

        float t = Time.time - GameManager.instance.startTime;

        float zStart = judgeZ + (startTime - t) * speed + spawnOffset;
        float zEnd = judgeZ + (endTime - t) * speed + spawnOffset;

        if(!startedHold)
        {
            // Normal movement
            UpdateFull(zStart, zEnd);
            TryStartHold(zStart);
        }
        else
        {
            // Start is locked at judgeZ; tail moves normally.
            UpdateShrinking(zEnd);
            TryEndHold(zEnd);
        }

        // If tail went far past judge → miss
        if(!completed && zEnd < judgeZ - 0.4f)
            Miss();
    }

    // --- BEFORE HOLD START ---
    void UpdateFull(float zStart, float zEnd)
    {
        float center = (zStart + zEnd) * 0.5f;
        float len = Mathf.Abs(zEnd - zStart);

        transform.position = new Vector3(LaneToX(lane), 0.5f, center);

        bodyPart.localScale = new Vector3(1, 1, len);
        startPart.localPosition = new Vector3(0, 0, -len * 0.5f);
        endPart.localPosition = new Vector3(0, 0, len * 0.5f);
    }

    void TryStartHold(float zStart)
    {
        if(!IsKeyPressed()) return;

        if(Mathf.Abs(zStart - judgeZ) <= 0.3f)
            StartHold(true);
        else if(Mathf.Abs(zStart - judgeZ) <= 0.6f)
            StartHold(false);
    }

    void StartHold(bool good)
    {
        startedHold = true;

        GameManager.instance.combo++;
        GameManager.instance.AddScore(good ? 1000 : 300);

        if(judge)
            judge.ShowJudge(good ? 0 : 1); // Perfect or Great
    }

    // AFTER HOLD START
    void UpdateShrinking(float zEnd)
    {
        // Start stays fixed on the judge line.
        float remaining = Mathf.Abs(zEnd - judgeZ);

        // Keep the START at judgeZ
        float center = judgeZ + (zEnd - judgeZ) * 0.5f;

        transform.position = new Vector3(LaneToX(lane), 0.5f, center);

        // Shrink ONLY from the tail
        bodyPart.localScale = new Vector3(1, 1, remaining);
        startPart.localPosition = new Vector3(0, 0, -remaining * 0.5f);
        endPart.localPosition = new Vector3(0, 0, remaining * 0.5f);
    }

    void TryEndHold(float zEnd)
    {
        // END reached judge line
        if(Mathf.Abs(zEnd - judgeZ) <= 0.15f)
        {
            if(IsKeyPressed())
                EndHold(true);  // Perfect Release
            else
                EndHold(false); // Bad Release

            return;
        }
    }

    void EndHold(bool perfect)
    {
        completed = true;

        GameManager.instance.AddScore(perfect ? 1000 : 300);

        if(judge)
            judge.ShowJudge(perfect ? 0 : 2); // Perfect or Bad

        Destroy(gameObject, 0.05f);
    }

    void Miss()
    {
        completed = true;

        GameManager.instance.miss++;
        GameManager.instance.ResetCombo();

        if(notes)
            notes.ShowMissFromLong();

        Destroy(gameObject);
    }

    float LaneToX(int lane) => (lane - 1.5f);

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
