using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class MultiNoteData
{
    public int lpb;
    public int num;
    public int block;
    public int type;
    public bool isAttack;
    public List<MultiNoteData> notes;
}

[Serializable]
public class MultiSongData
{
    public string name;
    public float bpm;
    public List<List<MultiNoteData>> notes;
}

public class MultiNotesManager : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private GameObject noteLongPrefab;
    [SerializeField] private MultiJudge judge;

    public List<int> LaneNum = new();
    public List<float> NotesTime = new();
    public List<GameObject> NotesObj = new();

    public float spawnOffset = 15f;

    private float bpm = 120f;
    private float songStartTime;
    public bool started = false;

    private const float judgeZ = 0f;

    [Header("Settings")]
    public float laneWidth = 1f;
    public float laneCount = 4f;

    public void StartGame()
    {
        Load(SongDataHolder.instance.SelectedSongName);
        started = true;
        songStartTime = Time.time;
    }

    void Load(string songName)
    {
        TextAsset json = Resources.Load<TextAsset>(songName);
        if (!json)
        {
            Debug.LogError("Song JSON not found!");
            return;
        }

        MultiSongData data = JsonConvert.DeserializeObject<MultiSongData>(json.text);
        bpm = data.bpm > 0 ? data.bpm : 120f;

        foreach (var lane in data.notes)
        {
            foreach (var note in lane)
            {
                float start = (note.num / (float)note.lpb) * (60f / bpm);
                float end = start;

                if (note.notes != null && note.notes.Count > 0)
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
    }

    GameObject CreateNote(MultiNoteData data, float start, float end)
    {
        float x = (data.block - (laneCount - 1) / 2f) * laneWidth;

        if (data.type == 4)
        {
            GameObject obj = Instantiate(noteLongPrefab);
            obj.transform.position = new Vector3(x, 0.5f, spawnOffset);

            MultiLongNote ln = obj.GetComponent<MultiLongNote>();
            ln.Init(
                data.block,
                start,
                end,
                judgeZ,
                spawnOffset,
                this,
                judge
            );

            return obj;
        }
        else
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
        if (!started) return;

        float songTime = Time.time - songStartTime;

        for (int i = NotesObj.Count - 1; i >= 0; i--)
        {
            GameObject obj = NotesObj[i];
            if (!obj)
            {
                RemoveNoteData(i);
                continue;
            }

            if (obj.GetComponent<MultiLongNote>() != null)
                continue;

            float t = NotesTime[i] - songTime;
            float z = judgeZ + t * MultiGameManager.instance.noteSpeed + spawnOffset;

            obj.transform.position = new Vector3(obj.transform.position.x, 0.5f, z);

            if (z < judgeZ - 0.4f)
            {
                judge.OnMiss();
                Destroy(obj);
                RemoveNoteData(i);
            }
        }
    }

    public void ShowMissFromLong()
    {
        if (judge != null)
            judge.ShowMissEffect();
    }

    public void RemoveNoteData(int index)
    {
        NotesObj.RemoveAt(index);
        LaneNum.RemoveAt(index);
        NotesTime.RemoveAt(index);
    }
}
