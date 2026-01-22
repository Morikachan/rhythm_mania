using UnityEngine;

public class Notes : MonoBehaviour
{
    private INoteSpeedProvider speedProvider;
    private float noteSpeed;

    void Awake()
    {
        TryFindProvider();
    }

    void TryFindProvider()
    {
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb is INoteSpeedProvider provider)
            {
                speedProvider = provider;
                noteSpeed = provider.GetNoteSpeed();
                return;
            }
        }

        Debug.LogError("INoteSpeedProvider not found on scene!");
    }

    void Update()
    {
        if (speedProvider == null) return;

        transform.position -= transform.forward * Time.deltaTime * noteSpeed;
    }
}