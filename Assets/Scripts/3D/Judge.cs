using UnityEngine;

public class Judge : MonoBehaviour
{
    [SerializeField] private GameObject[] MessageObj;
    [SerializeField] private NotesManager notesManager;
    [SerializeField] private AudioClip hitSound;

    private AudioSource audioSource;
    private const float judgeZ = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!GameManager.instance.started) return;
        if (notesManager.NotesObj.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.D)) CheckHit(0);
        if (Input.GetKeyDown(KeyCode.F)) CheckHit(1);
        if (Input.GetKeyDown(KeyCode.J)) CheckHit(2);
        if (Input.GetKeyDown(KeyCode.K)) CheckHit(3);
    }

    void CheckHit(int lane)
    {
        for (int i = 0; i < notesManager.NotesObj.Count; i++)
        {
            if (notesManager.LaneNum[i] != lane) continue;

            GameObject note = notesManager.NotesObj[i];
            float distance = Mathf.Abs(note.transform.position.z - judgeZ);

            if (distance <= 0.6f)
                audioSource.PlayOneShot(hitSound);

            if (distance <= 0.2f)
            {
                ShowJudge(0); // Perfect
                GameManager.instance.perfect++;
                GameManager.instance.AddScore(1000);
                DeleteNoteAt(i);
                return;
            }
            else if (distance <= 0.4f)
            {
                ShowJudge(1); // Great
                GameManager.instance.great++;
                GameManager.instance.AddScore(700);
                DeleteNoteAt(i);
                return;
            }
            else if (distance <= 0.6f)
            {
                ShowJudge(2); // Bad
                GameManager.instance.bad++;
                GameManager.instance.ResetCombo();
                DeleteNoteAt(i);
                return;
            }
        }
    }

    void DeleteNoteAt(int index)
    {
        Destroy(notesManager.NotesObj[index]);
        notesManager.RemoveNoteData(index);
    }

    void ShowJudge(int type)
    {
        Instantiate(MessageObj[type], new Vector3(0, 0.8f, judgeZ), Quaternion.identity);
    }

    public void ShowMissEffect()
    {
        if (MessageObj.Length > 3 && MessageObj[3] != null)
            Instantiate(MessageObj[3], new Vector3(0, 0.8f, judgeZ), Quaternion.identity);
    }
}
