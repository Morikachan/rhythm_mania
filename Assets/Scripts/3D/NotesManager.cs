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
    [SerializeField] private GameObject noteLongPrefab;
    [SerializeField] private Judge judge;

    public List<int> LaneNum = new List<int>();
    public List<float> NotesTime = new List<float>();
    public List<GameObject> NotesObj = new List<GameObject>();

    public float spawnOffset = 15f;

    private float bpm = 120f;
    private float songStartTime;
    private bool started = false;

    private const float judgeZ = -1.5f;

    [Header("Settings")]
    public float laneWidth = 1f;
    public float laneCount = 4f;

    public void StartGame()
    {
        if (SongDataHolder.instance != null)
        {
            Load(SongDataHolder.instance.SelectedSongName);
        }
        else
        {
            Load("Blank-Kytt-RSPN");
            Debug.LogError("No song selected! Starting with default song.");
        }

        started = true;
        songStartTime = Time.time;
    }

    void Load(string songName)
    {
        TextAsset json = Resources.Load<TextAsset>(songName);
        if(!json)
        {
            Debug.LogError("Song JSON not found!");
            return;
        }
        SongData data = JsonConvert.DeserializeObject<SongData>(json.text);
        bpm = data.bpm > 0 ? data.bpm : 120f;

        foreach(var lane in data.notes)
        {
            foreach(var note in lane)
            {
                float start = (note.num / (float)note.lpb) * (60f / bpm);
                float end = start;

                if(note.notes != null && note.notes.Count > 0)
                {
                    float last = note.notes[^1].num;
                    end = (last / (float)note.lpb) * (60f / bpm);
                }

                GameObject obj = CreateNote(note, start, end);
                NotesObj.Add(obj);
                NotesTime.Add(start);
                LaneNum.Add(note.block);
            }
        }

        Debug.Log($"Loaded {NotesTime.Count} notes");
    }

    GameObject CreateNote(NoteData data, float start, float end)
    {
        float x = (data.block - (laneCount - 1) / 2f) * laneWidth;

        if(data.type == 4) // long note
        {
            GameObject obj = Instantiate(noteLongPrefab);
            obj.transform.position = new Vector3(x, 0.5f, spawnOffset);

            LongNote ln = obj.GetComponent<LongNote>();
            ln.Init(data.block, start, end, judgeZ, spawnOffset, this, judge);

            return obj;
        }
        else // short note
        {
            return Instantiate(
                notePrefab,
                new Vector3(x, 0.5f, spawnOffset),
                Quaternion.identity
            );
        }
    }

    void Update()
    {
        if(!started) return;

        float songTime = Time.time - songStartTime;

        for(int i = NotesObj.Count - 1; i >= 0; i--)
        {
            GameObject obj = NotesObj[i];

            if(!obj)
            {
                RemoveNoteData(i);
                continue;
            }

            // LongNotes move themselves → skip them
            if(obj.GetComponent<LongNote>() != null)
                continue;

            float noteTime = NotesTime[i];
            float t = noteTime - songTime;

            float z = judgeZ + t * GameManager.instance.noteSpeed + spawnOffset;

            obj.transform.position = new Vector3(obj.transform.position.x, 0.5f, z);

            // MISS
            if(z < judgeZ - 0.4f)
            {
                GameManager.instance.miss++;
                GameManager.instance.ResetCombo();

                if(judge) judge.ShowMissEffect();

                Destroy(obj);
                RemoveNoteData(i);
            }
        }
    }

    public void ShowMissFromLong()
    {
        if(judge) judge.ShowMissEffect();
    }
    public void ShowHitFromLong(int type)
    {
        if(judge) judge.ShowJudge(type);
    }

    public void RemoveNoteData(int index) 
    { 
        NotesObj.RemoveAt(index);
        LaneNum.RemoveAt(index);
        NotesTime.RemoveAt(index);
    }
}
