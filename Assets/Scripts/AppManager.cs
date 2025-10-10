using UnityEngine;
/// <summary>
/// this class handles few global things
/// of the application, such as a flag to know if the app is quitting or not
/// </summary>
public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }
    public bool IsQuitting = false;


    [Header("Version Text Settings")]
    public string prototype = "none";
    [SerializeField] private string versionPrefix = "Version: ";
    private string version = "0";

    [Header("Logs")]
    public bool log = false;

    // AWAKE
    private void Awake()
    {
        if (Instance == null) { Instance = this; }

        // on récupère la version de l'app
        version = Application.version;

        // on affiche les stats
        ProjectStats.AnalyzeProject();
    }


    // VERSION
    public string GetVersion()
    {
        return version;
    }
    public string GetFullVersion(bool includePrototype = true)
    {
        string full = versionPrefix + version;

        if (includePrototype && !string.IsNullOrEmpty(prototype))
        {
            full += "\n(prototype " + prototype + ")";
        }

        return full;
    }



    // APPLICATION QUIT
    private void OnApplicationQuit()
    {
        IsQuitting = true;
        if (log) { Debug.Log("(AppManager) Application is quitting"); }
    }


}