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
    private string versionPrefix = "subrunner version alpha ";
    private string version = "0";


    [Header("FPS & VSync Settings")]
    public bool useVSync
    {
        get { return (QualitySettings.vSyncCount > 0); }
        set { QualitySettings.vSyncCount = (value) ? 1 : 0; }
    }
    public int targetFrameRate
    {

        get { return Application.targetFrameRate; }
        set { Application.targetFrameRate = value; }
    }



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