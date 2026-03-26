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

    [Header("States")]
    private bool awake_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;
    
    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load levels data
        loadLevelsData();
    }

    // LOAD / UNLOAD DATA
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
        awake_done = true;
    }

}