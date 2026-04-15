
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// this class handles few global things
/// of the application, such as a flag to know if the app is quitting or not.
/// It is situated on "/app" gameObject which is dontdestroy and is the first thing loaded when
/// launching the app
/// </summary>
public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }
    public bool IsQuitting = false;
    public int LoadedSceneCount = 0;


    [Header("Version Text Settings")]
    public string prototype = "none";
    private string versionPrefix = "subrunner version alpha ";
    private string version = "0";


    [Header("FPS & VSync Settings")]
    public bool UseVSync
    {
        get { return (QualitySettings.vSyncCount > 0); }
        set { QualitySettings.vSyncCount = (value) ? 1 : 0; }
    }
    public int TargetFrameRate
    {

        get { return Application.targetFrameRate; }
        set { Application.targetFrameRate = value; }
    }



    [Header("Logs")]
    public bool log = false;
    public bool log_stats = false;

    // AWAKE
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // on récupère la version de l'app
        version = Application.version;

        // on affiche les stats
        if (log_stats) { ProjectStats.AnalyzeProject(); }

        // si on est sur la scene subrunner-clean alors on trouve le UI_Manager pour lui assigner hud
        if (SceneManager.GetActiveScene().name == "subrunner-clean")
        {
            UI_Manager ui = GameObject.Find("/ui").GetComponent<UI_Manager>();
            UI_Pool hud = GameObject.Find("/ui/hud").GetComponent<UI_Pool>();
            ui.AssignStartPool(hud);
        }
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




    // MAIN CLICK FUNCTIONS
    public void Exit()
    {
        #if UNITY_EDITOR
        Debug.Log("exiting playmode...");
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }
    public void Fullscreen()
    {
        #if UNITY_EDITOR
        EditorWindow window = EditorWindow.focusedWindow;
        // Assume the game view is focused.
        window.maximized = !window.maximized;
        #else
        Screen.fullScreen = !Screen.fullScreen;
        #endif
    }
    public void Fullscreen(bool set_full)
    {
        #if UNITY_EDITOR
        EditorWindow window = EditorWindow.focusedWindow;
        // Assume the game view is focused.
        window.maximized = set_full;
        #else
        Screen.fullScreen = set_full;
        #endif
    }
    public void SetVSync(bool set_vsync)
    {
        UseVSync = set_vsync;
    }

    // APPLICATION QUIT
    private void OnApplicationQuit()
    {
        IsQuitting = true;

        // we save the settings prefs
        SettingsManager.Instance.SaveLocalSettings();

        if (log) { Debug.Log("(AppManager) Application is quitting"); }
    }



    // STATIC USEFUL FUNCTIONS
    /// <summary>
    /// Ensures that a folder exists at the given path. If it doesn't exist, it creates it.
    /// </summary>
    /// <param name="path">the path to check. should be reachable</param>
    public static void EnsureFolderExists(string path)
    {
        if (System.IO.Directory.Exists(path)) { return; }
        System.IO.Directory.CreateDirectory(path);
    }

}