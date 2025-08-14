using UnityEngine;
/// <summary>
/// this class handles few global things
/// of the application, such as a flag to know if the app is quitting or not
/// </summary>
public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }
    public bool IsQuitting = false;

    [Header("Logs")]
    public bool log = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    private void OnApplicationQuit()
    {
        IsQuitting = true;
        if (log) { Debug.Log("(AppManager) Application is quitting"); }
    }
}