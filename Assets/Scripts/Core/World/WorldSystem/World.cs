using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class World : BSOD_System<World>
{
    public WorldLoadStatus load_status = WorldLoadStatus.NotLoaded;
    public static WorldLoadStatus Status => LazyInstance != null ? LazyInstance.load_status : WorldLoadStatus.NotLoaded;

    [Header("Current world")]
    public string world_to_load = ""; // we can set this in the inspector to load a specific world at start
    public string world_id => data == null ? "" : data.id;
    public static string ID => LazyInstance == null ? "" : LazyInstance.world_id;
    public WorldData data => save == null ? null : save.world;
    public WorldSaveData save;
    public static WorldSaveData Save => LazyInstance == null ? null : LazyInstance.save;
    public bool IsWorldLoaded => load_status == WorldLoadStatus.Loaded;
    public bool IsWorldLoadingOrUnloading => load_status != WorldLoadStatus.Loaded && load_status != WorldLoadStatus.NotLoaded;

    [Header("Spawn")]
    public Transform fallback_spawn_point; // if no player data were found on LoadWorld, we will spawn the player at this position

    [Header("Transform Parents")]
    private Transform _capables_parent;
    public Transform CapablesParent
    {
        get
        {
            if (_capables_parent == null)
            {
                _capables_parent = transform.Find("Capables");
                if (_capables_parent == null)
                {
                    GameObject go = new GameObject("Capables");
                    go.transform.SetParent(transform);
                    _capables_parent = go.transform;
                }
            }
            return _capables_parent;
        }
    }
    private Transform _movables_parent;
    public Transform MovablesParent
    {
        get
        {
            if (_movables_parent == null)
            {
                _movables_parent = transform.Find("Movables");
                if (_movables_parent == null)
                {
                    GameObject go = new GameObject("Movables");
                    go.transform.SetParent(transform);
                    _movables_parent = go.transform;
                }
            }
            return _movables_parent;
        }
    }
    private Transform _items_parent;
    public Transform ItemsParent
    {
        get
        {
            if (_items_parent == null)
            {
                _items_parent = transform.Find("Items");
                if (_items_parent == null)
                {
                    GameObject go = new GameObject("Items");
                    go.transform.SetParent(transform);
                    _items_parent = go.transform;
                }
            }
            return _items_parent;
        }
    }
    private Transform _level_parent;
    public Transform LevelParent
    {
        get
        {
            if (_level_parent == null)
            {
                _level_parent = transform.Find("Levels");
                if (_level_parent == null)
                {
                    GameObject go = new GameObject("Levels");
                    go.transform.SetParent(transform);
                    _level_parent = go.transform;
                }
            }
            return _level_parent;
        }
    }
    private Transform _room_parent;
    public Transform RoomParent
    {
        get
        {
            if (_room_parent == null)
            {
                _room_parent = transform.Find("Rooms");
                if (_room_parent == null)
                {
                    GameObject go = new GameObject("Rooms");
                    go.transform.SetParent(transform);
                    _room_parent = go.transform;
                }
            }
            return _room_parent;
        }
    }
    private Transform _chunk_parent;
    public Transform ChunkParent
    {
        get
        {
            if (_chunk_parent == null)
            {
                _chunk_parent = transform.Find("Chunks");
                if (_chunk_parent == null)
                {
                    GameObject go = new GameObject("Chunks");
                    go.transform.SetParent(transform);
                    _chunk_parent = go.transform;
                }
            }
            return _chunk_parent;
        }
    }


    [Header("Logs")]
    public bool log = false;
    public bool log_loading_extended = false;
    public bool log_id_generation = false;


    ///
    //
    /// MAIN ENTRY POINTS : WORLD LOADING / UNLOADING
    //
    ///

    // LOAD / UNLOAD WORLD
    public async Task LoadWorld(string world_id)
    {
        if (log) { Debug.Log($"(World) ----------------------------------- LOADING WORLD : {world_id}"); }
        float start_time = Time.realtimeSinceStartup;
        float phase_time = Time.realtimeSinceStartup;

        ///
        //  1. WE LOAD THE WORLD DATA
        /* */ load_status = WorldLoadStatus.LoadingWorld;
        ///

        save = SaveEngine.GetWorldSave(world_id);
        if (save == null)
        {
            if (log) { Debug.LogWarning($"(World) Failed to load world save data for world_id: {world_id} -- json is null."); }
            return;
        }

        /* string json = extract_save_json(world_id);
        save = JsonUtility.FromJson<WorldSaveData>(json);
        if (json == null) */

        // load the data inside the world save
        // data = JsonUtility.FromJson<WorldData>(json);
        data.id = world_id; // we set the world_id in the data for easier access to it later, even if it's not serialized
        if (log_loading_extended) { Debug.Log($"(World) Loaded world save data for world_id: {world_id}\n\n"); }


        ///
        //  2. WE LOAD ALL THE ENGINES WITH WORLD DATA (and their sub systems)
        /* */ load_status = WorldLoadStatus.LoadingEngines;
        ///

        // and then we init all the engines
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- LOADING ALL ENGINES : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;
        await LevelEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);

        // -> now we can get all the levels ids and put it in the world data for runtime access.
        data.levels_ids = new List<string>(LevelEngine.LazyInstance.GetWorldLevelsIDs());

        await RoomEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);
        await ChunkEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);
        await CapableEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);
        await CapacityEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);


        ///
        //  3. WE WAIT A FRAME SO THE LOADED DATA CAN SLEEP vite fait
        /* */ load_status = WorldLoadStatus.WaitingFrame;
        ///
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- WE WAIT A FRAME : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;
        await System.Threading.Tasks.Task.Yield();



        ///
        //  4. WE LOAD THE CONTROLLER
        /* */
        load_status = WorldLoadStatus.LoadingController;
        ///
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- LOADING CONTROLLER : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;
        await Controller.LazyInstance.LoadWorldData(world_id, log_loading_extended);
        if (fallback_spawn_point != null) { Controller.Capable.transform.position = fallback_spawn_point.position; } // debug only to tp quickly at launch
        
        // ok so now we have a Controller.Capable defined if everything went ok ! We can determine it to load the right level


        ///
        //  5. WE LOAD THE PLAYER LEVEL
        /* */
        load_status = WorldLoadStatus.LoadingPlayerLevel;
        ///
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- WE LOAD CURRENT PLAYER LEVEL : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;

        // we load the start level
        if (Controller.LazyInstance.data != null && !string.IsNullOrEmpty(Controller.LazyInstance.data.player_level))
        {
            string player_level_id = Controller.LazyInstance.data.player_level;
            if (log) { Debug.Log($"(World) STARTING WORLD: {world_id}  -- Player level from controller data: {player_level_id}"); }
            if (!string.IsNullOrEmpty(player_level_id)) { await LevelEngine.LazyInstance.LoadLevel(player_level_id); }
        }
        else if (data != null && data.levels_ids != null && data.levels_ids.Count > 0)
        {
            string start_level_id = data.levels_ids[0];
            if (log) { Debug.Log($"(World) STARTING WORLD: {world_id}  -- Level: {start_level_id}"); }
            if (!string.IsNullOrEmpty(start_level_id)) { await LevelEngine.LazyInstance.LoadLevel(start_level_id); }
        }
        else if (log) { Debug.Log($"(World) STARTING WORLD: {world_id} (!) {(data == null ? "DATA IS NULL" : "NO LEVELS FOUND")}"); }



        ///
        //  6. WE SUCCESSFULLY LOADED THE WORLD !
        /* */ load_status = WorldLoadStatus.Loaded;
        ///
        if (log)
        {
            Debug.Log($"(World) ----------------------------------- WORLD LOADED : (in {Time.realtimeSinceStartup - start_time}s{(!log_loading_extended ? ")" : $", previous phase duration: {Time.realtimeSinceStartup - phase_time}s)")}");
        }
        // we make sure the first chunk loaded are the one of the player
        /* string chunk_id = Controller.LazyInstance.data != null ? Controller.LazyInstance.data.player_chunk : null;
        if (string.IsNullOrEmpty(chunk_id))
        {
            if (log) { Debug.LogWarning($"(World) No player chunk defined in controller data, cannot init player chunk."); }
            return;
        }
        else { ChunkEngine.LazyInstance.InitPlayerChunk(chunk_id); } */

        // we wait for the controller capable to be loaded
        int frames_waited = 0;
        while (Controller.Capable == null || !Controller.Capable.Loaded) { await Task.Yield(); frames_waited++; }
        if (log) { Debug.Log($"(World) Player capable loaded after waiting {frames_waited} frames. Capable ID: {Controller.Capable.ID}"); }
        ChunkEngine.LazyInstance.RefreshPlayerChunk(Controller.Capable);
    }
    public async Task UnloadWorld()
    {
        if (log) { Debug.Log($"(World) ----------------------------------- UNLOADING WORLD : {world_id}"); }
        float start_time = Time.realtimeSinceStartup;
        /* */ load_status = WorldLoadStatus.Unloading;

        // we unload the controller first to let it do some cleanup if needed (like saving player data for example)
        await Controller.LazyInstance.UnloadWorldData(log_loading_extended);
        

        // we unload all the engines
        await LevelEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await RoomEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await ChunkEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await CapableEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await CapacityEngine.LazyInstance.UnloadWorldData(log_loading_extended);

        // we clear the world data
        save = null; // ? really useful to clear the save data here ? maybe we will reload it in like 2 seconds
        if (log)
        {
            Debug.Log($"(World) ----------------------------------- WORLD UNLOADED : (in {Time.realtimeSinceStartup - start_time}s)");
        }
        /* */ load_status = WorldLoadStatus.NotLoaded;
    }



    ///
    //
    /// OTHER HELPFUL METHODS
    //
    ///


    // ID GENERATION / REGISTRATION
    private Dictionary<string, int> generated_ids_counters = new Dictionary<string, int>();
    public void RegisterUniqueID(string id)
    {
        string prefix = get_id_prefix(id, out string suffix);
        if (!generated_ids_counters.ContainsKey(prefix))
        {
            generated_ids_counters[prefix] = 0;
            if (log_id_generation) { Debug.Log($"(World) Registered unique ID: {id} (prefix: {prefix}, suffix: {suffix}) -- new prefix, counter initialized to 0."); }
            // return; // ! we don't return here because maybe the suffix is greater than 0 so we need to update it
        }

        // otherwise we already have some ids with this prefix, we check if the suffix int is greater than the current max suffix for this prefix
        try
        {
            int suffix_int = int.Parse(suffix);
            if (suffix_int <= generated_ids_counters[prefix]) { return; } // already have a higher suffix for this prefix, we do nothing
            generated_ids_counters[prefix] = suffix_int;
            if (log_id_generation) { Debug.Log($"(World) Registered unique ID: {id} (prefix: {prefix}, suffix: {suffix}) -- counter updated to {suffix_int} for this prefix."); }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"(World) Failed to register unique ID: {id} -- invalid suffix: {suffix} -- exception: {e.Message}");
        }
    }
    public string GenerateUniqueID(string base_id)
    {
        string prefix = get_id_prefix(base_id);

        // construct final id
        string new_id = prefix + "-" + get_next_id_suffix(prefix);
        if (log_id_generation) { Debug.Log($"(World) Generated unique ID: {new_id} (base_id: {base_id}, prefix: {prefix}, max id for this prefix: {generated_ids_counters[prefix]})"); }
        return new_id;
    }
    public void ClearGeneratedIDs() { generated_ids_counters.Clear(); }
    public void UnregisterUniqueID(string id)
    {
        string prefix = get_id_prefix(id, out string suffix);
        if (!generated_ids_counters.ContainsKey(prefix))
        {

            if (log_id_generation) { Debug.Log($"(World) Unregistered unique ID: {id} (prefix: {prefix}, suffix: {suffix}) -- did not find prefix, did nothing (already unregistered)"); }
            return;
        }

        // otherwise we already have some ids with this prefix, we check if the suffix int is equal to the current max suffix for this prefix
        try
        {
            int suffix_int = int.Parse(suffix);
            if (suffix_int != generated_ids_counters[prefix]) { return; } // we can only unregister the id if it's the one with the highest suffix for this prefix, otherwise we do nothing (we don't want to mess with the suffixes order)
            generated_ids_counters[prefix]--;
            if (log_id_generation) { Debug.Log($"(World) Unregistered unique ID: {id} (prefix: {prefix}, suffix: {suffix}) -- counter updated to {generated_ids_counters[prefix]} for this prefix."); }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"(World) Failed to unregister unique ID: {id} -- invalid suffix: {suffix} -- exception: {e.Message}");
        }
    }
    private string get_id_prefix(string id)
    {
        string[] parts = id.Split('-');
        return parts[0];
    }
    private string get_id_prefix(string id, out string suffix)
    {
        string[] parts = id.Split('-');
        suffix = parts.Length > 1 ? parts[parts.Length - 1] : "";
        return parts[0];
    }
    private int get_next_id_suffix(string prefix)
    {
        if (!generated_ids_counters.ContainsKey(prefix))
        {
            generated_ids_counters[prefix] = 0;
        }
        else
        {
            generated_ids_counters[prefix]++;
        }
        return generated_ids_counters[prefix];
    }
    public string GetPrefix(string id) { return get_id_prefix(id); }
    public bool DoesIDMatchPrefix(string id, string prefix)
    {
        return get_id_prefix(id) == prefix;
    }



    ///
    //
    /// DATA MANAGEMENT
    //
    ///


    // GET STATIC DATA
    public WorldData GetStaticData()
    {
        WorldData new_data = new WorldData
        {
            levels_ids = get_static_levels_ids(),
            generated_ids_counters = data != null ? data.generated_ids_counters : new Dictionary<string, int>(),
            creation_date = data != null ? data.creation_date : string.Empty,
            last_update_date = data != null ? data.last_update_date : string.Empty,
            color = data != null ? data.color : Color.white,
            icon_path = data != null ? data.icon_path : "",
            icon_name = data != null ? data.icon_name : "",
            game_version = data != null ? data.game_version : Application.version
        };

        return new_data;
    }
    private List<string> get_static_levels_ids()
    {
        // 1. check if we have data we return the levels_ids stored in it
        if (data != null && data.levels_ids != null && data.levels_ids.Count > 0) { return data.levels_ids; }

        // 2. if not we go statically get the levels ids from the children levels (only active ones)
        Level[] levels = gameObject.GetComponentsInChildren<Level>();
        List<string> level_ids = new List<string>();
        foreach (Level level in levels) { level_ids.Add(level.ID); }
        return level_ids;
    }
    public Level[] GetStaticLevels()
    {
        // only return the levels in the children that are ALSO in the levels_ids
        List<string> levels_ids = get_static_levels_ids();
        Level[] levels = gameObject.GetComponentsInChildren<Level>(includeInactive: true);
        List<Level> filtered_levels = new List<Level>();
        foreach (Level level in levels)
        {
            if (!levels_ids.Contains(level.ID)) { continue; }
            filtered_levels.Add(level);
        }
        return filtered_levels.ToArray();
    }


}


[Serializable] public class WorldData
{
    [NonSerialized] public string id;
    [NonSerialized] public List<string> levels_ids; // no need to serialize it since we always get them from the "levels" folder -> more granular better
    [RuntimeOnly] public Dictionary<string, int> generated_ids_counters;


    // meta data
    public string game_version;
    public string creation_date;
    public string last_update_date;
    public string icon_path;
    public string icon_name;
    public Color color;

    public void UpdateTime(bool just_created = false)
    {
        game_version = Application.version;
        last_update_date = DateTime.Now.ToString();
        creation_date = (just_created || string.IsNullOrEmpty(creation_date)) ? last_update_date : creation_date;

        // check if we just created it or we don't have any icon, then we set random color and default icon
        if (just_created || string.IsNullOrEmpty(icon_path))
        {
            color = WorldManager.LazyInstance.GetRandomWorldColor();
            icon_path = WorldManager.LazyInstance.GetRandomIconPath(out string icon_name);
            this.icon_name = icon_name;
        }
    }
    public int CompareTime(WorldData other)
    {
        return CompareTime(this.last_update_date, other.last_update_date);
    }
    public static int CompareTime(string a_date, string b_date)
    {
        if (string.IsNullOrEmpty(a_date) || string.IsNullOrEmpty(b_date)) { return 0; }
        try
        {
            DateTime this_date = DateTime.ParseExact(a_date, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime other = DateTime.ParseExact(b_date, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            return this_date.CompareTo(other);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"(WorldData) Failed to compare dates: invalid date format -- this last_update_date: {a_date}, other_date: {b_date} -- exception: {e.Message}");
            return 0;
        }
    }

    public string GetDetails()
    {
        string details = "";
        details += "id : " + id + "\n";
        details += "game version : " + $"{game_version}\n";
        details += $"modified : " + $"{last_update_date}\n";
        details += $"created : " + $"{creation_date}\n";
        details += $"color : " + $"{color}\n";
        details += $"icon path : " + $"{icon_path}\n";
        details += $"icon name : " + $"{icon_name}\n";
        details += $"levels ids : " + $"{(levels_ids != null ? string.Join(", ", levels_ids) : "null")}\n";
        return details;
    }
}

[Serializable] public enum WorldLoadStatus
{
    NotLoaded,
    LoadingWorld,
    LoadingEngines,
    WaitingFrame,
    LoadingController,
    LoadingPlayerLevel,
    Loaded,
    Unloading
}