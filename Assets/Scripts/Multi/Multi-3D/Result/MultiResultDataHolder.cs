using UnityEngine;
using System.Collections.Generic;

public class MultiResultDataHolder : MonoBehaviour {
    public static MultiResultDataHolder instance;

    public Dictionary<int, PlayerRuntimeData> results;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SetResults(Dictionary<int, PlayerRuntimeData> data)
    {
        results = data;
    }
}
