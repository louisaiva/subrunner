using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    // SINGLETON LOGIC
    private static WorldBuilder _lazy_instance;
    public static WorldBuilder LazyInstance
    {
        get
        {
            if (_lazy_instance == null)
            {
                _lazy_instance = FindFirstObjectByType<WorldBuilder>(FindObjectsInactive.Include);
                if (_lazy_instance == null) { Debug.LogError("No (WorldBuilder) instance found in the scene."); }
            }
            return _lazy_instance;
        }
    }

    // SUBSYSTEMS
    private static LevelBuilder _level_builder;
    public static LevelBuilder LevelBuilder
    {
        get
        {
            if (_level_builder == null) { _level_builder = LazyInstance.GetComponent<LevelBuilder>(); }
            if (_level_builder == null) { Debug.LogError("(WorldBuilder) No LevelBuilder found on WorldBuilder"); }
            return _level_builder;
        }
    }
    private static LevelTranslator _level_translator;
    public static LevelTranslator LevelTranslator
    {
        get
        {
            if (_level_translator == null) { _level_translator = LazyInstance.GetComponent<LevelTranslator>(); }
            if (_level_translator == null) { Debug.LogError("(WorldBuilder) No LevelTranslator found on WorldBuilder"); }
            return _level_translator;
        }
    }


    // LEVELS STATUS
    // private string world_id;
    public static string StaticTargetedWorld => WorldManager.LazyWorld;
    public Dictionary<string, LevelBuildStatus> build_status = new Dictionary<string, LevelBuildStatus>();
    public Dictionary<string, LevelSaveStatus> save_status = new Dictionary<string, LevelSaveStatus>();


    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_callbacks = false;

    public void RegisterCallbacks()
    {
        if (log_callbacks) { Debug.Log("(WorldBuilder) Registering callbacks"); }

        // we subscribe to the level builder events
        LevelBuilder.OnLevelModified += on_level_modified;
        LevelBuilder.OnLevelBuilding += on_level_started_building;
        LevelBuilder.OnLevelBuilt += on_level_built;

        // and translator ones
        LevelTranslator.OnLevelTranslated += on_level_translated;
    }
    public void RemoveCallbacks()
    {
        if (log_callbacks) { Debug.Log("(WorldBuilder) Removing callbacks"); }

        // we unsubscribe to the level builder events
        LevelBuilder.OnLevelModified -= on_level_modified;
        LevelBuilder.OnLevelBuilding -= on_level_started_building;
        LevelBuilder.OnLevelBuilt -= on_level_built;

        // and translator ones
        LevelTranslator.OnLevelTranslated -= on_level_translated;
    }


    // MAIN ENTRY POINT TO EDIT / CREATE A LEVEL
    public static void EditLevel(string level)
    {
        if (LazyInstance.log) { Debug.Log($"(WorldBuilder) EditLevel called for level '{level}'"); }
        LevelBuilder.EditLevel(StaticTargetedWorld, level);
    }
    public static void BuildLevel(string level)
    {
        if (LazyInstance.log) { Debug.Log($"(WorldBuilder) BuildLevel called for level '{level}'"); }
        
        // we first enable LevelBuilder
        LevelBuilder.gameObject.SetActive(true);
        BuiltLevelData built_data = LevelBuilder.Build(StaticTargetedWorld, level); // will build then translate
    }
    public static void SaveLevel(string level)
    {
        if (LazyInstance.log) { Debug.Log($"(WorldBuilder) SaveLevel called for level '{level}'"); }
        // LevelBuilder.SaveLevel(StaticTargetedWorld, level);
    }

    // CALLBACKS HANDLERS
    private void on_level_modified(string level_id)
    {
        if (log_callbacks) { Debug.Log($"(WorldBuilder) Level '{level_id}' modified, marking it as needing build"); }
        build_status[level_id] = LevelBuildStatus.BuildRequested;
    }
    private void on_level_started_building(string level_id)
    {
        if (log_callbacks) { Debug.Log($"(WorldBuilder) Level '{level_id}' started building"); }
        build_status[level_id] = LevelBuildStatus.Building;
    }
    private void on_level_built(BuiltLevelData built_data)
    {
        if (log_callbacks) { Debug.Log($"(WorldBuilder) Level '{built_data.level}' built, starting translation"); }
        build_status[built_data.level] = LevelBuildStatus.Translating;
        save_status[built_data.level] = LevelSaveStatus.SaveRequested;
        LevelBuilder.gameObject.SetActive(false);
        LevelTranslator.Translate(built_data);
    }
    private void on_level_translated(Level level)
    {
        if (log_callbacks) { Debug.Log($"(WorldBuilder) Level '{level.ID}' translated, marking it as needing save"); }
        build_status[level.ID] = LevelBuildStatus.Built;
        save_status[level.ID] = LevelSaveStatus.SaveRequested;
    }



    // CLEAN CACHE
    public void ClearCache()
    {
        // we clear all potentials AIO leftovers
        SaveEngine.AIO_Loader.ClearCache();

        build_status.Clear();
        save_status.Clear();
    }
}

public enum LevelBuildStatus
{
    BuildRequested,
    Building,
    Translating,
    Built,
}
public enum LevelSaveStatus
{
    SaveRequested,
    Saving,
    UpToData,
}