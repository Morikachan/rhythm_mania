using Photon.Pun;
using UnityEngine;
using static MultiNotesManager;

public class MultiJudge : MonoBehaviour
{
    [SerializeField] private GameObject[] messageObj;
    [SerializeField] private AudioClip hitSound;

    private AudioSource audioSource;
    private MultiNotesManager notesManager;

    private const float judgeZ = 0f;
    private int localActor;

    public enum JudgeType { Perfect = 0, Great = 1, Bad = 2, Miss = 3 }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        localActor = PhotonNetwork.LocalPlayer.ActorNumber;

        notesManager = FindObjectOfType<MultiNotesManager>();
        if(!notesManager)
            Debug.LogError("MultiNotesManager NOT FOUND");
    }

    void Update()
    {
        if(!MultiGameManager.instance) return;
        if(!MultiGameManager.instance.musicManager) return;
        if(!MultiGameManager.instance.musicManager.played) return;
        if(!notesManager) return;

        if(Input.GetKeyDown(KeyCode.D)) TryHit(0);
        if(Input.GetKeyDown(KeyCode.F)) TryHit(1);
        if(Input.GetKeyDown(KeyCode.J)) TryHit(2);
        if(Input.GetKeyDown(KeyCode.K)) TryHit(3);
    }

    void TryHit(int lane)
    {
        NoteHitResult result = notesManager.TryHitNote(lane, judgeZ);

        if(!result.hit)
            return;

        if(hitSound)
            audioSource.PlayOneShot(hitSound);

        switch(result.type)
        {
            case JudgeType.Perfect:
                MultiGameManager.instance.players[localActor].perfect++;
                MultiGameManager.instance.AddScore(localActor, 1000);
                break;

            case JudgeType.Great:
                MultiGameManager.instance.players[localActor].great++;
                MultiGameManager.instance.AddScore(localActor, 700);
                break;

            case JudgeType.Bad:
                MultiGameManager.instance.players[localActor].bad++;
                MultiGameManager.instance.ResetCombo(localActor);
                MultiGameManager.instance.Damage(localActor, 50);
                break;
        }

        ShowJudge(result.type);
    }

    public void OnMiss()
    {
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;

        MultiGameManager.instance.players[actor].miss++;
        MultiGameManager.instance.ResetCombo(actor);
        MultiGameManager.instance.Damage(actor, 100);

        ShowMissEffect();
    }

    public void ShowJudge(JudgeType type)
    {
        Instantiate(
            messageObj[(int)type],
            new Vector3(0, 0.8f, judgeZ),
            Quaternion.identity
        );
    }

    public void ShowMissEffect()
    {
        if(messageObj.Length > 3 && messageObj[3] != null)
        {
            Instantiate(
                messageObj[3],
                new Vector3(0, 0.8f, judgeZ),
                Quaternion.identity
            );
        }
    }
}
