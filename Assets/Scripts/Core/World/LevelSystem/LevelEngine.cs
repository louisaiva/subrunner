using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LevelEngine : BSOD_System<LevelEngine>
{

    [Header("Level data")]
    public Dictionary<string, LevelData> levels_data = new Dictionary<string, LevelData>();

    [Header("Current level")]
    // public string start_level_id; // level id to load at the start // todo : remove this when it is in WorldData based on player's level
    public Level current_level;
    public string CurrentLevelID
    {
        get
        {
            if (current_level == null || current_level.data == null) { return null; }
            return current_level.data.id;
        }
    }
    public Action<LevelData> OnLevelChange = delegate { };

    [Header("World levels")]
    public Dictionary<string, Level> world_levels = new Dictionary<string, Level>();
    public Level level_prefab;
    public Transform level_parent;

    [Header("States")]
    // private bool awake_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;
    public bool log_loading = false;
    public bool hide_no_level_warning = false;
    
    // LOAD / UNLOAD WORLD DATA
    public override async Task LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(LevelEngine) Loading world data for world_id: {world_id}"); }

        // load levels data
        loadLevelsData(world_id);
        if (log) { Debug.Log($"(LevelEngine) Loaded {levels_data.Count} levels data"); }

        // create levels
        createLevels();
        if (log) { Debug.Log($"(LevelEngine) Created {world_levels.Count} levels"); }
        if (log) { Debug.Log($"(LevelEngine) LEVEL ENGINE SUCCESSFULLY LOADED : {world_id}"); }
    }
    public override async Task UnloadWorldData(bool log)
    {
        // we clear the levels data and destroy the levels gameobjects
        levels_data.Clear();
        foreach (var level in world_levels.Values) { Destroy(level.gameObject); }
        world_levels.Clear();
        if (log) { Debug.Log($"(LevelEngine) LEVEL ENGINE SUCCESSFULLY UNLOADED"); }
    }


    // LOAD LEVELS DATA & CREATE LEVELS
    protected void loadLevelsData(string world_id)
    {
        // we empty the levels_data
        levels_data = new Dictionary<string, LevelData>();
        string log_levels_details = "\n\n";

        // we load all the json files in the data path and convert them to LevelData objects
        string[] files = AppManager.LoadJsonsFromWorldFolder(world_id, "levels");
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
        world_levels = new Dictionary<string, Level>();
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
        world_levels.Add(data.id, new_level);
        return new_level;
    }


    // LOAD LEVEL
    public Action<Level> OnLevelLoaded = delegate { };
    public void LoadLevel(string level_id)
    {
        if (!world_levels.ContainsKey(level_id)) { if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - Load) Level data not found for id: " + level_id); } return; }
        Level new_level = world_levels[level_id];

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
        OnLevelLoaded?.Invoke(current_level);
        OnLevelChange?.Invoke(current_level.data);
    }
    public void UnloadLevel()
    {
        if (current_level == null) { if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - Unload) No level currently loaded, skipping unload"); } return; }
        current_level.Unload();
        if (log_loading) { Debug.Log($"(LevelEngine) Level '{current_level.data.id}' unloaded"); }
        current_level = null;
    }

    // GETTERS
    public List<RoomData> GetRoomsDataOfLevel(string level_id)
    {
        if (!levels_data.ContainsKey(level_id))
        {
            if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - GetRoomsDataOfLevel) Level data not found for id: " + level_id); }
            return new List<RoomData>();
        }
        List<string> room_ids = levels_data[level_id].rooms_ids;
        List<RoomData> room_datas = RoomEngine.Instance.GetRoomsDataFromIDs(room_ids);
        return room_datas;
    }
    public List<CapableData> GetCapablesDataOfLevel(string level_id)
    {
        return GetCapablesDataOfLevel(level_id, out List<RoomData> _);
    }
    public List<CapableData> GetCapablesDataOfLevel(string level_id, out List<RoomData> rooms_data)
    {
        // get the rooms data of the level
        rooms_data = GetRoomsDataOfLevel(level_id);
        if (rooms_data.Count == 0) { return new List<CapableData>(); }

        // we get the capable ids inside the rooms data and return the corresponding capable data
        List<string> capable_ids = new List<string>();
        foreach (RoomData rdata in rooms_data)
        {
            if (rdata.capables_ids != null && rdata.capables_ids.Count > 0)
            {
                capable_ids.AddRange(rdata.capables_ids);
            }
            if (rdata.movables_ids != null && rdata.movables_ids.Count > 0)
            {
                capable_ids.AddRange(rdata.movables_ids);
            }
        }

        // we get the capable data from the ids
        List<CapableData> capable_datas = CapableEngine.Instance.GetCapablesDataFromIDs(capable_ids);
        return capable_datas;
    }
    public Level[] GetWorldLevels()
    {
        return world_levels.Values.ToArray();
    }
    public Level GetLevelOfRoom(string room_id)
    {
        foreach (var kvp in world_levels)
        {
            if (kvp.Value.data.rooms_ids.Contains(room_id))
            {
                return kvp.Value;
            }
        }
        if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - GetLevelOfRoom) Level not found for room id: " + room_id); }
        return null;
    }
}