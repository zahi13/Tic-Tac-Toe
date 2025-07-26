using UnityEngine;
using System;

public class ApplicationEventsHandler : MonoBehaviour
{
    public static event Action<bool> OnApplicationPauseEvent;
    public static event Action OnApplicationQuitEvent;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        OnApplicationPauseEvent?.Invoke(pauseStatus);
    }

    void OnApplicationQuit()
    {
        OnApplicationQuitEvent?.Invoke();
    }
}