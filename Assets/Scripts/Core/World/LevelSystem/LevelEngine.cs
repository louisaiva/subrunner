using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LevelEngine : BSOD_System<LevelEngine>
{




    // SUB SYSTEMS
    [Header("Sub systems")]
    private LevelNavBaker _nav_baker;
    public LevelNavBaker NavBaker
    {
        get
        {
            if (_nav_baker == null)
            {
                _nav_baker = GetComponentInChildren<LevelNavBaker>(includeInactive: true);
                if (_nav_baker == null) { Debug.LogError("(LevelEngine) No LevelNavBaker found in children"); }
            }
            return _nav_baker;
        }
    }



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
            return current_level.ID;
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
        foreach (KeyValuePair<string, LevelData> entry in levels_data) { create_level(entry.Value); }
    }
    private Level create_level(LevelData data)
    {
        // we instanciate a new level and assign the data to it
        Level new_level = Instantiate(level_prefab, level_parent);
        new_level.name = "Level_" + data.id;
        new_level.data = data;
        world_levels.Add(data.id, new_level);
        return new_level;
    }


    // LOAD LEVEL
    public Action<Level> OnLevelLoaded = delegate { };
    public async Task LoadLevel(string level_id)
    {
        if (!world_levels.ContainsKey(level_id)) { if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - Load) Level data not found for id: " + level_id); } return; }
        Level new_level = world_levels[level_id];

        // we unload the current level if there is one
        if (current_level != null)
        {
            if (current_level.data.id == level_id) { if (!hide_no_level_warning) { Debug.LogWarning($"(LevelEngine - Load) Level '{level_id}' is already loaded, skipping load"); } return; }
            await UnloadLevel();
        }

        // we load the navmesh data for the new level
        await NavBaker.LoadLevelNavMesh(new_level);

        // we load the new level
        new_level.Load();
        if (log_loading) { Debug.Log($"(LevelEngine) Level '{level_id}' loaded"); }
        current_level = new_level;
        OnLevelLoaded?.Invoke(current_level);
        OnLevelChange?.Invoke(current_level.data);
    }
    public async Task UnloadLevel()
    {
        if (current_level == null) { if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - Unload) No level currently loaded, skipping unload"); } return; }
        current_level.Unload();
        if (log_loading) { Debug.Log($"(LevelEngine) Level '{current_level.data.id}' unloaded"); }
        current_level = null;
    }

    // GETTERS
    public List<ChunkData> GetChunksDataOfLevel(string level_id)
    {
        if (!levels_data.ContainsKey(level_id))
        {
            if (!hide_no_level_warning) { Debug.LogWarning("(LevelEngine - GetChunksDataOfLevel) Level data not found for id: " + level_id); }
            return new List<ChunkData>();
        }
        List<string> chunk_ids = new List<string>();
        foreach (string room_id in levels_data[level_id].rooms_ids)
        {
            RoomData room_data = RoomEngine.Instance.GetRoomDataFromID(room_id);
            if (room_data == null) { Debug.LogWarning($"(LevelEngine - GetChunksDataOfLevel) Room data not found for id: {room_id} when trying to get chunks of level '{level_id}'"); continue; }
            if (room_data.chunks_ids != null && room_data.chunks_ids.Count > 0)
            {
                chunk_ids.AddRange(room_data.chunks_ids);
            }
        }
        List<ChunkData> chunk_datas = ChunkEngine.Instance.GetChunksDataFromIDs(chunk_ids);
        return chunk_datas;
    }
    public List<CapableData> GetCapablesDataOfLevel(string level_id)
    {
        return GetCapablesDataOfLevel(level_id, out List<ChunkData> _);
    }
    public List<CapableData> GetCapablesDataOfLevel(string level_id, out List<ChunkData> chunks_data)
    {
        // get the rooms data of the level
        chunks_data = GetChunksDataOfLevel(level_id);
        if (chunks_data.Count == 0) { return new List<CapableData>(); }

        // we get the capable ids inside the rooms data and return the corresponding capable data
        List<string> capable_ids = new List<string>();
        foreach (ChunkData rdata in chunks_data)
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
    public List<string> GetWorldLevelsIDs()
    {
        return world_levels.Keys.ToList();
    }

    // STATIC GETTERS
    public static List<LevelData> LoadWorldLevelsData(string world_id)
    {
        List<LevelData> levels_data = new List<LevelData>();

        // we load all the json files in the data path and convert them to LevelData objects
        string[] files = AppManager.LoadJsonsFromWorldFolder(world_id, "levels");
        foreach (string file in files)
        {
            LevelData data = JsonUtility.FromJson<LevelData>(file);
            levels_data.Add(data);
        }
        return levels_data;
    }
    public static LevelData LoadWorldLevelData(string world_id, string level_id)
    {
        // we load the json file for the specified level and convert it to a LevelData object
        string file = AppManager.LoadJsonFromWorldFolder(world_id, Path.Combine("levels", level_id + ".json"));
        if (string.IsNullOrEmpty(file))
        {
            Debug.LogError($"(LevelEngine) Level data not found for id: {level_id}");
            return null;
        }
        return JsonUtility.FromJson<LevelData>(file);
    }
}