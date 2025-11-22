
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public bool log_stats = false;

    // AWAKE
    private void Awake()
    {
        if (Instance == null) { Instance = this; }

        // on récupère la version de l'app
        version = Application.version;

        // on affiche les stats
        if (log_stats) { ProjectStats.AnalyzeProject(); }
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
    public void play() => UI_Manager.Instance.SwitchToHUD();
    public void hud() => UI_Manager.Instance.SwitchToHUD();
    public void exit()
    {
        #if UNITY_EDITOR
        Debug.Log("exiting playmode...");
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }
    public void fullscreen()
    {
        #if UNITY_EDITOR
        EditorWindow window = EditorWindow.focusedWindow;
        // Assume the game view is focused.
        window.maximized = !window.maximized;
        #else
        Screen.fullScreen = !Screen.fullScreen;
        #endif
    }
    public void fullscreen(bool set_full)
    {
        #if UNITY_EDITOR
        EditorWindow window = EditorWindow.focusedWindow;
        // Assume the game view is focused.
        window.maximized = set_full;
        #else
        Screen.fullScreen = set_full;
        #endif
    }
    public void ghost_mode()
    {
        if (Perso.Instance == null) { return; }
        Perso.Instance.ToggleGhost();
    }
    public void metamorph()
    {
        if (Perso.Instance == null) { return; }
        Perso.Instance.Metamorph();
    }
    public void heal()
    {
        if (Perso.Instance == null) { return; }
        Perso.Instance.healMax();
    }
    public void toggle_vsync()
    {
        useVSync = !useVSync;
    }
    public void credits()
    {
        UI_Manager.Instance.SwitchTo("credits");
    }
    public void settings()
    {
        UI_Manager.Instance.SwitchTo("settings");
    }
    public void home()
    {
        UI_Manager.Instance.SwitchTo("home");
    }
    public async void back_to_main_menu()
    {
        if (SceneLoader.Instance == null) { exit(); return; }
        await SceneLoader.Instance.GoBackToMainMenu();
    }

    // APPLICATION QUIT
    private void OnApplicationQuit()
    {
        IsQuitting = true;

        // we save the settings prefs
        SettingsManager.Instance.SaveLocalSettings();

        if (log) { Debug.Log("(AppManager) Application is quitting"); }
    }
}