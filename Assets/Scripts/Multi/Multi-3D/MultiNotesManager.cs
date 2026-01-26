using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class MultiNoteData {
    public int lpb;
    public int num;
    public int block;
    public int type;          // 0 = short, 4 = long
    public bool isAttack;
    public List<MultiNoteData> notes;
}

[Serializable]
public class MultiSongData {
    public string name;
    public float bpm;
    public List<List<MultiNoteData>> notes;
}

public class MultiNotesManager : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] private GameObject shortNotePrefab;
    [SerializeField] private GameObject longNotePrefab;

    [Header("Refs")]
    [SerializeField] private MultiJudge judge;

    [Header("Lane Settings")]
    public float laneWidth = 1f;
    public int laneCount = 4;

    [Header("Timing")]
    public float spawnOffset = 15f;
    private const float judgeZ = 0f;

    private float bpm = 120f;
    private float songStartTime;
    private bool started;

    // runtime lists
    private readonly List<GameObject> notesObj = new();
    private readonly List<float> notesTime = new();
    private readonly List<int> laneNum = new();

    
    // GAME FLOW
    public void StartGame()
    {
        Load(SongDataHolder.instance.SelectedSongName);
        songStartTime = Time.time;
        started = true;
    }

    
    // LOAD SONG
    void Load(string songName)
    {
        TextAsset json = Resources.Load<TextAsset>(songName);
        if(!json)
        {
            Debug.LogError("Song JSON not found: " + songName);
            return;
        }

        MultiSongData data = JsonConvert.DeserializeObject<MultiSongData>(json.text);
        bpm = data.bpm > 0 ? data.bpm : 120f;

        foreach(var lane in data.notes)
        {
            foreach(var note in lane)
            {
                float start = (note.num / (float)note.lpb) * (60f / bpm);
                float end = start;

                if(note.type == 4 && note.notes != null && note.notes.Count > 0)
                {
                    float last = note.notes[^1].num;
                    end = (last / (float)note.lpb) * (60f / bpm);
                }

                SpawnNote(note, start, end);
            }
        }

        Debug.Log($"[MultiNotesManager] Loaded {notesObj.Count} notes");
    }

    
    // SPAWN
    void SpawnNote(MultiNoteData data, float start, float end)
    {
        float x = (data.block - (laneCount - 1) / 2f) * laneWidth;

        if(data.type == 4)
            SpawnLong(data, start, end, x);
        else
            SpawnShort(data, start, x);
    }

    void SpawnShort(MultiNoteData data, float start, float x)
    {
        GameObject obj = Instantiate(
            shortNotePrefab,
            new Vector3(x, 0.5f, spawnOffset),
            Quaternion.identity
        );

        notesObj.Add(obj);
        notesTime.Add(start);
        laneNum.Add(data.block);
    }

    void SpawnLong(MultiNoteData data, float start, float end, float x)
    {
        GameObject obj = Instantiate(
            longNotePrefab,
            new Vector3(x, 0.5f, spawnOffset),
            Quaternion.identity
        );

        MultiLongNote ln = obj.GetComponent<MultiLongNote>();
        if(ln == null)
        {
            Debug.LogError("MultiLongNote component missing!");
            Destroy(obj);
            return;
        }

        ln.Init(
            data.block,
            start,
            end,
            judgeZ,
            spawnOffset,
            this,
            judge
        );

        notesObj.Add(obj);
        notesTime.Add(start);
        laneNum.Add(data.block);
    }

    
    // UPDATE
    void Update()
    {
        if(!started || MultiGameManager.instance == null)
            return;

        float songTime = Time.time - songStartTime;
        float speed = MultiGameManager.instance.noteSpeed;

        for(int i = notesObj.Count - 1; i >= 0; i--)
        {
            GameObject obj = notesObj[i];
            if(!obj)
            {
                RemoveNote(i);
                continue;
            }

            if(obj.GetComponent<MultiLongNote>() != null)
                continue;

            float t = notesTime[i] - songTime;
            float z = judgeZ + t * speed + spawnOffset;

            obj.transform.position = new Vector3(
                obj.transform.position.x,
                0.5f,
                z
            );

            if(z < judgeZ - 0.4f)
            {
                judge.OnMiss();
                Destroy(obj);
                RemoveNote(i);
            }
        }
    }

    public struct NoteHitResult {
        public bool hit;
        public MultiJudge.JudgeType type;
    }

    public NoteHitResult TryHitNote(int lane, float judgeZ)
    {
        for(int i = 0; i < notesObj.Count; i++)
        {
            if(laneNum[i] != lane)
                continue;

            GameObject note = notesObj[i];
            if(!note) continue;

            float distance = Mathf.Abs(note.transform.position.z - judgeZ);

            if(note.GetComponent<MultiLongNote>() != null)
                continue;

            if(distance <= 0.2f)
            {
                Destroy(note);
                RemoveNote(i);
                return new NoteHitResult { hit = true, type = MultiJudge.JudgeType.Perfect };
            }
            else if(distance <= 0.4f)
            {
                Destroy(note);
                RemoveNote(i);
                return new NoteHitResult { hit = true, type = MultiJudge.JudgeType.Great };
            }
            else if(distance <= 0.6f)
            {
                Destroy(note);
                RemoveNote(i);
                return new NoteHitResult { hit = true, type = MultiJudge.JudgeType.Bad };
            }
        }

        return new NoteHitResult { hit = false };
    }

    // CALLED FROM LONG NOTE

    public void ShowMissFromLong()
    {
        judge?.ShowMissEffect();
    }

    
    // CLEANUP
    void RemoveNote(int index)
    {
        notesObj.RemoveAt(index);
        notesTime.RemoveAt(index);
        laneNum.RemoveAt(index);
    }
}
