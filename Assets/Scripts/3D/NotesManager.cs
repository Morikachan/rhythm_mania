using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class NoteData {
    public int lpb;
    public int num;
    public int block;
    public int type;
    public bool isAttack;
    public List<NoteData> notes;
}

[Serializable]
public class SongData {
    public string name;
    public float bpm;
    public int maxBlock;
    public int offset;
    public List<List<NoteData>> notes;
}

public class NotesManager : MonoBehaviour {
    public int noteNum;
    private string songName;

    public List<int> LaneNum = new List<int>();
    public List<int> NoteType = new List<int>();
    public List<float> NotesTime = new List<float>();
    public List<GameObject> NotesObj = new List<GameObject>();

    [SerializeField] private float NotesSpeed = 1f;
    [SerializeField] private GameObject noteObj;

    void OnEnable()
    {
        noteNum = 0;
        songName = "warm-nights"; // !UPDATE idから出す
        Load(songName);
    }

    void Load(string SongName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(SongName);
        if(jsonFile == null)
        {
            Debug.LogError("❌ JSON file not found in Resources: " + SongName);
            return;
        }

        SongData data;
        try
        {
            data = JsonConvert.DeserializeObject<SongData>(jsonFile.text);
        }
        catch(Exception ex)
        {
            Debug.LogError("❌ JSON parse error: " + ex.Message);
            return;
        }

        if(data == null || data.notes == null)
        {
            Debug.LogError("❌ Invalid or empty JSON structure");
            return;
        }

        float bpm = data.bpm > 0 ? data.bpm : 120f;

        foreach(var lane in data.notes)
        {
            if(lane == null) continue;

            foreach(var note in lane)
            {
                if(note == null) continue;
                if(note.lpb <= 0) note.lpb = 4;

                float time = (note.num / (float)note.lpb) * (60f / bpm);
                if(float.IsNaN(time) || float.IsInfinity(time)) continue;

                NotesTime.Add(time);
                LaneNum.Add(note.block);
                NoteType.Add(note.type);
            }
        }

        noteNum = NotesTime.Count;

        for(int i = 0; i < noteNum; i++)
        {
            float z = NotesTime[i] * NotesSpeed + 25;

            float laneWidth = 1.0f;
            float laneCount = 4f;
            float x = (LaneNum[i] - (laneCount - 1) / 2f) * laneWidth;

            float y = 0.5f;

            if(float.IsNaN(z) || float.IsInfinity(z))
            {
                continue;
            }

            GameObject newNote = Instantiate(noteObj, new Vector3(x, y, z), Quaternion.identity);
            NotesObj.Add(newNote);
        }
    }
}
