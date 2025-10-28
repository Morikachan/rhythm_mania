using UnityEngine;

public class Notes : MonoBehaviour
{
    int NoteSpeed = 5;
    void Update()
    {
        transform.position -= transform.forward * Time.deltaTime * NoteSpeed;
    }
}
