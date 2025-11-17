using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class NoteData
{
    public int lpb;
    public int num;
    public int block;
    public int type;
    public bool isAttack;
    public List<NoteData> notes; // For long notes
}

[Serializable]
public class SongData
{
    public string name;
    public float bpm;
    public List<List<NoteData>> notes;
}

public class NotesManager : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Judge judge;

    public List<int> LaneNum = new List<int>();
    public List<float> NotesTime = new List<float>();
    public List<GameObject> NotesObj = new List<GameObject>();

    private float bpm = 120f;
    private float songStartTime;
    private bool started = false;

    private const float spawnZ = 20f;
    private const float judgeZ = -1.5f;

    public void StartGame()
    {
        Load("Blank-Kytt-RSPN");

        started = true;
        songStartTime = Time.time;
    }

    void Load(string songName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(songName);
        if (jsonFile == null)
        {
            Debug.LogError("JSON file not found: " + songName);
            return;
        }

        SongData data = JsonConvert.DeserializeObject<SongData>(jsonFile.text);
        if (data == null || data.notes == null)
        {
            Debug.LogError("Invalid or empty JSON structure");
            return;
        }

        bpm = data.bpm > 0 ? data.bpm : 120f;

        foreach (var lane in data.notes)
        {
            if (lane == null) continue;

            foreach (var note in lane)
            {
                if (note == null) continue;
                if (note.lpb <= 0) note.lpb = 4;

                float startTime = (note.num / (float)note.lpb) * (60f / bpm);
                float endTime = startTime;

                // Long note support
                if(note.notes != null && note.notes.Count > 0)
                {
                    float lastNum = note.notes[note.notes.Count - 1].num;
                    endTime = (lastNum / (float)note.lpb) * (60f / bpm);
                }

                NotesTime.Add(startTime);
                LaneNum.Add(note.block);

                GameObject obj = CreateNoteObject(note, startTime, endTime);
                NotesObj.Add(obj);
            }
        }

        Debug.Log($"Loaded {NotesTime.Count} notes");
    }

    GameObject CreateNoteObject(NoteData note, float startTime, float endTime)
    {
        float laneWidth = 1f;
        float laneCount = 4f;
        float x = (note.block - (laneCount - 1) / 2f) * laneWidth;
        float y = 0.5f;

        GameObject obj;

        if(note.type == 4) // long note
        {
            obj = Instantiate(Resources.Load<GameObject>("LongNote"));
            obj.transform.position = new Vector3(x, y, spawnZ);

            var ln = obj.GetComponent<LongNote>();
            ln.Init(note.block, startTime, endTime);
        } else
        {
            obj = Instantiate(notePrefab, new Vector3(x, y, spawnZ), Quaternion.identity);
        }

        return obj;
    }

    void Update()
    {
        if (!started) return;

        float songTime = Time.time - songStartTime;

        for (int i = NotesObj.Count - 1; i >= 0; i--)
        {
            GameObject note = NotesObj[i];
            float noteTime = NotesTime[i];
            float timeToJudge = noteTime - songTime;

            float z = judgeZ + timeToJudge * GameManager.instance.noteSpeed;

            note.transform.position = new Vector3(
                note.transform.position.x,
                0.5f,
                z
            );

            // MISS
            if (z < judgeZ - 0.4f)
            {
                GameManager.instance.miss++;
                GameManager.instance.ResetCombo();

                if (judge) judge.ShowMissEffect();

                Destroy(note);
                RemoveNoteData(i);
            }
        }
    }

    public void RemoveNoteData(int index)
    {
        NotesObj.RemoveAt(index);
        LaneNum.RemoveAt(index);
        NotesTime.RemoveAt(index);
    }
}
