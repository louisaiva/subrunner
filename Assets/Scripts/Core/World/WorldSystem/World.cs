using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class World : BSOD_System<World>
{
    [Header("Current world")]
    public string world_id;
    public WorldData data;
    [field:SerializeField] public bool IsWorldLoaded { get; private set; } = false;
    [field:SerializeField] public bool IsWorldLoadingOrUnloading { get; private set; } = false;

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


    [Header("Logs")]
    public bool log = false;
    public bool log_loading_extended = false;
    public bool log_id_generation = false;

    // LOAD / UNLOAD WORLD
    public async Task LoadWorld(string world_id)
    {
        IsWorldLoadingOrUnloading = true;
        if (log) { Debug.Log($"(World) ----------------------------------- LOADING WORLD : {world_id}"); }
        float start_time = Time.realtimeSinceStartup;
        float phase_time = Time.realtimeSinceStartup;

        ///
        //  1. WE LOAD THE WORLD DATA
        ///

        string json = extract_world_json(world_id);
        if (json == null)
        {
            if (log) { Debug.LogWarning($"(World) Failed to load world data for world_id: {world_id} -- json is null."); }
            return;
        }

        // load the data inside the world
        data = JsonUtility.FromJson<WorldData>(json);
        this.world_id = world_id;
        data.id = world_id; // we set the world_id in the data for easier access to it later, even if it's not serialized
        if (log_loading_extended) { Debug.Log($"(World) Loaded world data for world_id: {world_id}\n\n{json}"); }



        ///
        //  2. WE LOAD ALL THE ENGINES WITH WORLD DATA (and their sub systems)
        ///

        // and then we init all the engines
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- LOADING ALL ENGINES : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;
        await LevelEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);

        // -> now we can get all the levels ids and put it in the world data for runtime access.
        data.levels_ids = new List<string>(LevelEngine.LazyInstance.GetWorldLevelsIDs());

        await RoomEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);
        await CapableEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);
        await CapacityEngine.LazyInstance.LoadWorldData(world_id, log_loading_extended);


        ///
        //  3. WE WAIT A FRAME SO THE LOADED DATA CAN SLEEP vite fait
        ///
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- WE WAIT A FRAME : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;
        await System.Threading.Tasks.Task.Yield();



        ///
        //  4. WE LOAD THE PLAYER LEVEL
        ///
        if (log_loading_extended) { Debug.Log($"(World) ----------------------------------- WE LOAD CURRENT PLAYER LEVEL : (previous phase duration: {Time.realtimeSinceStartup - phase_time}s)"); }
        phase_time = Time.realtimeSinceStartup;

        // we load the start level
        if (data != null && data.levels_ids != null && data.levels_ids.Count > 0)
        {
            string start_level_id = data.levels_ids[0];
            if (log) { Debug.Log($"(World) STARTING WORLD: {world_id}  -- Level: {start_level_id}"); }
            if (!string.IsNullOrEmpty(start_level_id)) { await LevelEngine.LazyInstance.LoadLevel(start_level_id); }

            // we tp the player to the fallback spawn point while loading the world
            if (fallback_spawn_point != null) { Controller.LazyInstance.Capable.transform.position = fallback_spawn_point.position; }
        }
        else if (log) { Debug.Log($"(World) STARTING WORLD: {world_id} (!) {(data == null ? "DATA IS NULL" : "NO LEVELS FOUND")}"); }



        ///
        //  5. WE SUCCESSFULLY LOADED THE WORLD !
        ///
        IsWorldLoaded = true;
        IsWorldLoadingOrUnloading = false;
        if (log)
        {
            Debug.Log($"(World) ----------------------------------- WORLD LOADED : (in {Time.realtimeSinceStartup - start_time}s{(!log_loading_extended ? ")" : $", previous phase duration: {Time.realtimeSinceStartup - phase_time}s)")}");
        }
    }
    private string extract_world_json(string world_id)
    {
        AppManager.EnsureFolderExists(WorldManager.WorldsDataPath);
        string world_path = Path.Combine(WorldManager.WorldsDataPath, world_id);
        
        // check if the current world folder exists in the worlds folder.
        if (!System.IO.Directory.Exists(world_path))
        {
            if (log) { Debug.LogWarning($"(World) World folder not found: {world_path}"); }
            return null;
        }


        // check if the world data json exists in the worlds folder.
        string world_data_json_path = Path.Combine(world_path, "world_data.json");
        if (!System.IO.File.Exists(world_data_json_path))
        {
            if (log) { Debug.LogWarning($"(World) World file not found: {world_data_json_path}"); }
            return null;
        }

        // we load the world from Application.persistentDataPath + worlds_path
        try
        {
            using (FileStream stream = new FileStream(world_data_json_path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch (Exception e)
        {
            if (log) { Debug.LogWarning($"(World) Failed to extract world data json in {world_data_json_path}: {e.Message}"); }
            return null;
        }
    }
    public async Task UnloadWorld()
    {
        IsWorldLoadingOrUnloading = true;
        if (log) { Debug.Log($"(World) ----------------------------------- UNLOADING WORLD : {world_id}"); }
        float start_time = Time.realtimeSinceStartup;

        // we unload all the engines
        await LevelEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await RoomEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await CapableEngine.LazyInstance.UnloadWorldData(log_loading_extended);
        await CapacityEngine.LazyInstance.UnloadWorldData(log_loading_extended);

        // we clear the world data
        data = null;
        world_id = null;
        if (log)
        {
            Debug.Log($"(World) ----------------------------------- WORLD UNLOADED : (in {Time.realtimeSinceStartup - start_time}s)");
        }
        IsWorldLoaded = false;
        IsWorldLoadingOrUnloading = false;
    }



    // STATIC DATA EXTRACTION
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

    // ID GENERATION
    private Dictionary<string, int> generated_ids_counters = new Dictionary<string, int>();
    public void RegisterUniqueID(string id)
    {
        string prefix = get_id_prefix(id, out string suffix);
        if (!generated_ids_counters.ContainsKey(prefix))
        {
            generated_ids_counters[prefix] = 0;
            if (log_id_generation) { Debug.Log($"(World) Registered unique ID: {id} (prefix: {prefix}, suffix: {suffix}) -- new prefix, counter initialized to 0."); }
            return;
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
}


[Serializable] public class WorldData
{
    [NonSerialized] public string id;
    [NonSerialized] public List<string> levels_ids; // no need to serialize it since we always get them from the "levels" folder -> more granular better
    public Dictionary<string, int> generated_ids_counters;


    // meta data
    public string game_version;
    public string creation_date;
    public string last_update_date;
    public string icon_path;
    public string icon_name;
    public Color color;
}