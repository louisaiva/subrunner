using System.Collections.Generic;
using UnityEngine;

public class LevelEngine : BSOD_System<LevelEngine>
{

    [Header("Level data")]
    private string level_data_path = "data/levels/";
    public Dictionary<string, LevelData> levels_data = new Dictionary<string, LevelData>();

    [Header("Current level")]
    public string start_level_id; // level id to load at the start // todo : remove this when it is in WorldData based on player's level
    public Level current_level;

    [Header("Pooled levels")]
    public Dictionary<string, Level> pooled_levels;
    public Level level_prefab;
    public Transform level_parent;

    [Header("States")]
    // private bool awake_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;
    public bool log_loading = false;
    public bool hide_no_level_warning = false;
    
    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load levels data
        loadLevelsData();

        // create levels
        createLevels();
    }

    // LOAD LEVELS DATA & CREATE LEVELS
    protected void loadLevelsData()
    {
        // we empty the levels_data
        levels_data = new Dictionary<string, LevelData>();
        string log_levels_details = "\n\n";

        // we load all the json files in the data path and convert them to LevelData objects
        string[] files = GameManager.Instance.LoadJsons(level_data_path);
        foreach (string file in files)
        {
            LevelData data = JsonUtility.FromJson<LevelData>(file);
            levels_data.Add(data.id, data);
            log_levels_details += data.GetDetails() + "\n";
        }

        if (log_awake_data) { Debug.Log("(LevelEngine) LEVELS DATA LOADED : " + levels_data.Count + log_levels_details); }
        // awake_done = true;
    }
    protected void createLevels()
    {
        // we empty the pooled levels
        pooled_levels = new Dictionary<string, Level>();
        foreach (KeyValuePair<string, LevelData> entry in levels_data)
        {
            Level level = create_level(entry.Value);
            level.LoadNavMeshesPath();
        }
    }
    private Level create_level(LevelData data)
    {
        // we instanciate a new level and assign the data to it
        Level new_level = Instantiate(level_prefab, level_parent);
        new_level.data = data;
        pooled_levels.Add(data.id, new_level);
        return new_level;
    }

    // START
    private void Start()
    {
        // we load the start level
        if (!string.IsNullOrEmpty(start_level_id)) { LoadLevel(start_level_id); }
    }

    // LOAD LEVEL
    public void LoadLevel(string level_id)
    {
        if (!pooled_levels.ContainsKey(level_id)) { if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - Load) Level data not found for id: " + level_id); } return; }
        Level new_level = pooled_levels[level_id];

        // we unload the current level if there is one
        if (current_level != null)
        {
            if (current_level.data.id == level_id) { if (!hide_no_level_warning) { Debug.LogWarning($"(LevelEngine - Load) Level '{level_id}' is already loaded, skipping load"); } return; }
            UnloadLevel();
        }

        // we load the new level
        new_level.Load();
        if (log_loading) { Debug.Log($"(LevelEngine) Level '{level_id}' loaded"); }
        current_level = new_level;
    }
    public void UnloadLevel()
    {
        if (current_level == null) { if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - Unload) No level currently loaded, skipping unload"); } return; }
        current_level.Unload();
        if (log_loading) { Debug.Log($"(LevelEngine) Level '{current_level.data.id}' unloaded"); }
        current_level = null;
    }
}