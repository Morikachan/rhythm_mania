using Photon.Pun;
using UnityEngine;

public class MultiJudge : MonoBehaviour
{
    [SerializeField] private GameObject[] MessageObj;
    [SerializeField] private NotesManager notesManager;
    [SerializeField] private AudioClip hitSound;

    private AudioSource audioSource;
    private const float judgeZ = 0f;

    private int localActor;

    public enum JudgeType { Perfect = 0, Great = 1, Bad = 2, Miss = 3 }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        localActor = PhotonNetwork.LocalPlayer.ActorNumber;
    }

    void Update()
    {
        if(!MultiGameManager.instance) return;
        if(!MultiGameManager.instance.musicManager.played) return;
        if(notesManager.NotesObj.Count == 0) return;

        if(Input.GetKeyDown(KeyCode.D)) CheckHit(0);
        if(Input.GetKeyDown(KeyCode.F)) CheckHit(1);
        if(Input.GetKeyDown(KeyCode.J)) CheckHit(2);
        if(Input.GetKeyDown(KeyCode.K)) CheckHit(3);
    }

    void CheckHit(int lane)
    {
        for(int i = 0; i < notesManager.NotesObj.Count; i++)
        {
            if(notesManager.LaneNum[i] != lane)
                continue;

            GameObject note = notesManager.NotesObj[i];
            float distance = Mathf.Abs(note.transform.position.z - judgeZ);

            if(distance <= 0.6f)
                audioSource.PlayOneShot(hitSound);

            if(note.GetComponent<LongNote>() != null)
                return;

            //  PERFECT 
            if(distance <= 0.2f)
            {
                ShowJudge(JudgeType.Perfect);

                MultiGameManager.instance.players[localActor].perfect++;
                MultiGameManager.instance.AddScore(localActor, 1000);

                DeleteNote(i);
                return;
            }
            //  GREAT 
            else if(distance <= 0.4f)
            {
                ShowJudge(JudgeType.Great);

                MultiGameManager.instance.players[localActor].great++;
                MultiGameManager.instance.AddScore(localActor, 700);

                DeleteNote(i);
                return;
            }
            //  BAD 
            else if(distance <= 0.6f)
            {
                ShowJudge(JudgeType.Bad);

                MultiGameManager.instance.players[localActor].bad++;
                MultiGameManager.instance.ResetCombo(localActor);
                MultiGameManager.instance.Damage(localActor, 50);

                DeleteNote(i);
                return;
            }
        }
    }

    void DeleteNote(int index)
    {
        Destroy(notesManager.NotesObj[index]);
        notesManager.RemoveNoteData(index);
    }

    public void ShowJudge(JudgeType type)
    {
        Instantiate(
            MessageObj[(int)type],
            new Vector3(0, 0.8f, judgeZ),
            Quaternion.identity
        );
    }

    public void ShowMissEffect()
    {
        if(MessageObj.Length > 3 && MessageObj[3] != null)
        {
            Instantiate(
                MessageObj[3],
                new Vector3(0, 0.8f, judgeZ),
                Quaternion.identity
            );
        }
    }
}
