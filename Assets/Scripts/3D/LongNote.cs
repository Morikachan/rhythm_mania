using UnityEngine;

public class LongNote : MonoBehaviour
{
    public int lane;
    public float startTime;
    public float endTime;

    private Transform startPart;
    private Transform bodyPart;
    private Transform endPart;

    private const float judgeZ = -1.5f;
    private float speed => GameManager.instance.noteSpeed;

    public void Init(int lane, float start, float end)
    {
        this.lane = lane;
        this.startTime = start;
        this.endTime = end;

        startPart = transform.Find("Start");
        bodyPart = transform.Find("Body");
        endPart = transform.Find("End");
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.instance.started) return;

        float t = Time.time - GameManager.instance.startTime;
        float zStart = judgeZ + (startTime - t) * speed;
        float zEnd = judgeZ + (endTime - t) * speed;

        Vector3 pos = transform.position;
        pos.x = (lane - 1.5f);
        pos.z = (zStart + zEnd) / 2f;
        transform.position = pos;

        float length = Mathf.Abs(zEnd - zStart);
        bodyPart.localScale = new Vector3(1, 1, length);
        bodyPart.localPosition = Vector3.zero;
    }
}
