
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;






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
    /// <returns>returns true if the folder was just created, false if it already existed</returns>
    public static bool EnsureFolderExists(string path)
    {
        if (Directory.Exists(path)) { return false; }
        Directory.CreateDirectory(path);
        return true;
    }

    // JSON DATA LOADING FROM ASSETS
    public static string[] LoadJsonsFromAssets(string data_folder)
    {
        TextAsset[] json_assets = Resources.LoadAll<TextAsset>(data_folder);
        string[] jsons = new string[json_assets.Length];
        for (int i = 0; i < json_assets.Length; i++)
        {
            jsons[i] = json_assets[i].text;
        }
        return jsons;
    }
    public static string LoadJsonFromAsset(string path, FileNotFound log_type = FileNotFound.Log)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            if (log_type == FileNotFound.DontLog) { return null; }
            string log = $"(AppManager) Failed to load json from asset at path: {path}";
            if (log_type == FileNotFound.LogWarning) { Debug.LogWarning(log); }
            if (log_type == FileNotFound.LogError) { Debug.LogError(log); }
            if (log_type == FileNotFound.Log) { Debug.Log(log); }
            return null;
        }
        return textAsset.text;
    }
    public static string LoadJsonFromAsset<T>(string path, out T data)
    {
        string json = Resources.Load<TextAsset>(path).text;
        data = JsonUtility.FromJson<T>(json);
        return json;
    }


    // JSON DATA LOADING FROM WORLD DATA PATH
    public static string[] LoadJsonsFromWorldFolder(string world_id, string data_folder)
    {
        // loads jsons from the current world data path (which is in the persistent data path) instead of the assets
        // data_folder should be like "levels" for levels or "capables"
        string jsons_path = Path.Combine(WorldManager.WorldsDataPath, world_id, data_folder);

        // we get all the json files in the data folder and load them as strings
        string[] file_paths = Directory.GetFiles(jsons_path, "*.json");
        string[] jsons = new string[file_paths.Length];
        for (int i = 0; i < file_paths.Length; i++)
        {
            jsons[i] = System.IO.File.ReadAllText(file_paths[i]);
        }
        return jsons;
    }
    public static string[] LoadSpecificJsonsFromWorldFolder(string world_id, string data_folder, List<string> file_names)
    {
        // loads jsons from the current world data path (which is in the persistent data path) instead of the assets
        // data_folder should be like "levels" for levels or "capables"
        string jsons_path = Path.Combine(WorldManager.WorldsDataPath, world_id, data_folder);

        // we get all the json files in the data folder and load them as strings
        string[] file_paths = Directory.GetFiles(jsons_path, "*.json");
        List<string> jsons = new List<string>();
        for (int i = 0; i < file_paths.Length; i++)
        {
            string file_name = Path.GetFileNameWithoutExtension(file_paths[i]);
            if (!file_names.Contains(file_name)) { continue; }
            jsons.Add(System.IO.File.ReadAllText(file_paths[i]));
        }
        return jsons.ToArray();
    }
    public static string LoadJsonFromWorldFolder(string world_id, string path)
    {
        // load json from the current world data path (which is in the persistent data path) instead of the assets
        // path should contain the world name like this : "world_id/levels/level_id.json"
        string json_path = Path.Combine(WorldManager.WorldsDataPath, world_id, path);
        if (!System.IO.File.Exists(json_path))
        {
            Debug.LogWarning($"(AppManager) Failed to load json from persistent data path: {json_path} because the file was not found.");
            return null;
        }
        return System.IO.File.ReadAllText(json_path);
    }
    public static string LoadJsonFromPersistentDataPath(string json_path, FileNotFound log_type = FileNotFound.Log)
    {
        string path = Path.Combine(Application.persistentDataPath, json_path);
        if (!System.IO.File.Exists(path))
        {
            if (log_type == FileNotFound.DontLog) { return null; }
            string log = $"(AppManager) Failed to load json from persistent data path: {path} because the file was not found.";
            if (log_type == FileNotFound.LogWarning) { Debug.LogWarning(log); }
            else if (log_type == FileNotFound.LogError) { Debug.LogError(log); }
            else if (log_type == FileNotFound.Log) { Debug.Log(log); }
            return null;
        }
        return System.IO.File.ReadAllText(path);
    }
    public static void OpenWorldsFolder()
    {
        string worlds_folder_path = Path.Combine(Application.persistentDataPath, "worlds");
        EnsureFolderExists(worlds_folder_path);
        Application.OpenURL(worlds_folder_path);
    }
    public static void OpenFolderInWorlds(string path_in_worlds)
    {
        string path = Path.Combine(Application.persistentDataPath, "worlds", path_in_worlds);
        EnsureFolderExists(path);
        Application.OpenURL(path);
    }

    // JSON DATA SAVING TO WORLD DATA PATH
    public static void SaveJsonToWorldFolder(string world_id, string path, string json, bool log = false)
    {
        string json_path = Path.Combine(WorldManager.WorldsDataPath, world_id, path);
        System.IO.File.WriteAllText(json_path, json);
        if (log) { Debug.Log($"(AppManager) Saved json to {world_id} world folder: {json_path}\n{json}"); }
    }
    public static void SaveJsonToAsset(string path, string json, Verbosity verbose)
    {
        if (!path.StartsWith("Assets/Resources/")) { path = Path.Combine("Assets/Resources/", path + ".json"); }
        try
        {
            System.IO.File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"(AppManager) Failed to save json to asset: {path}\n{json}\nError: {e.Message}");
            return;
        }
        if (verbose >= Verbosity.Normal) { Debug.Log($"(AppManager) Saved json to asset: {path}\n{json}"); }
    }


    // FILE MANAGEMENT
    public static string[] GetFilesPathsInWorldFolder(string world_id, string data_folder, string search_pattern = "*.json")
    {
        string folder_path = Path.Combine(WorldManager.WorldsDataPath, world_id, data_folder);
        if (!Directory.Exists(folder_path))
        {
            Debug.LogWarning($"(AppManager) Failed to get files paths in world folder because the folder was not found: {folder_path}");
            return new string[0];
        }
        return Directory.GetFiles(folder_path, search_pattern);
    }
    public static void DeleteFile(string path, bool log = false)
    {
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            if (log) { Debug.Log($"(AppManager) Deleted file : {path}"); }
        }
        else if (log)
        {
            Debug.LogWarning($"(AppManager) Failed to delete file: {path} because the file was not found.");
        }
    }
}

public enum FileNotFound
{
    DontLog,
    Log,
    LogWarning,
    LogError,
}