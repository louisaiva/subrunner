using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveEngine : MonoBehaviour
{
    // STATIC SINGLETON
    public static SaveEngine Instance { get; private set; }
    public static SaveEngine LazyInstance
    {
        get
        {
            if (Instance != null) { return Instance; }
            Instance = FindFirstObjectByType<SaveEngine>();
            if (Instance == null)
            {
                Debug.LogError("No instance of SaveManager found in the scene. Please make sure to add a SaveManager component to a game object in the scene.");
            }
            return Instance;
        }
    }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }
    }


    // SUBSYSTEMS
    private static AIO_Loader _static_loader;
    public static AIO_Loader AIO_Loader
    {
        get
        {
            if (_static_loader != null) { return _static_loader; }
            _static_loader = LazyInstance.transform.GetComponentInChildren<AIO_Loader>(includeInactive: true);
            if (_static_loader == null) { Debug.LogError("(SaveEngine) SaveEngine.AIO_Loader was not found"); }
            return _static_loader;
        }
    }


    [Header("Logs")]
    [SerializeField] private bool log = false;
    private static bool log_static => LazyInstance != null && LazyInstance.log;


    // SAVE WORLD DATA
    public static void SaveWorldData(WorldData data)
    {
        bool just_created = WorldManager.EnsureWorldDataHierarchy(data.id);

        data.game_version = Application.version;
        data.last_update_date = DateTime.Now.ToString();
        data.creation_date = (just_created || string.IsNullOrEmpty(data.creation_date)) ? data.last_update_date : data.creation_date;

        // check if we just created it or we don't have any icon, then we set random color and default icon
        if (just_created || string.IsNullOrEmpty(data.icon_path))
        {
            data.color = WorldManager.LazyInstance.GetRandomWorldColor();
            data.icon_path = WorldManager.LazyInstance.GetRandomIconPath(out string icon_name);
            data.icon_name = icon_name;
        }

        // save the current WorldData to a json file
        string json = JsonUtility.ToJson(data, true);
        AppManager.SaveJsonToWorldFolder(data.id, "world_data.json", json, log_static);
    }

    // SAVE LEVEL DATA
    public static void SaveLevelData(LevelData data, string world_id)
    {
        // save the current LevelData to a json file
        string json = JsonUtility.ToJson(data, true);
        AppManager.SaveJsonToWorldFolder(world_id, Path.Combine("levels", data.id + ".json") , json, log_static);
    }
}