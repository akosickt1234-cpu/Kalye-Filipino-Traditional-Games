using System.Collections;
using UnityEngine;

public sealed class ApiRunner : MonoBehaviour
{
    const string HOST_NAME = "api_runner";

    static ApiRunner instance;

    public static Coroutine run(IEnumerator routine)
    {
        ensure();
        return instance.StartCoroutine(routine);
    }

    static void ensure()
    {
        if (instance != null) return;

        var host = new GameObject(HOST_NAME);
        DontDestroyOnLoad(host);
        instance = host.AddComponent<ApiRunner>();
    }
}
