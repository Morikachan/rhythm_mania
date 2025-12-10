using UnityEngine;

public class Notes : MonoBehaviour
{
    float NoteSpeed = 5;

    void Start()
    {
        NoteSpeed = GameManager.instance.noteSpeed;
    }
    void Update()
    {
        transform.position -= transform.forward * Time.deltaTime * NoteSpeed;
    }
}