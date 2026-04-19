using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class World : BSOD_System<World>
{
    [Header("Data")]
    private static string worlds_path = "worlds";
    public static string WorldDataPath => Path.Combine(Application.persistentDataPath, worlds_path);
    public static string CurrentStaticWorldDataPath
    {
        get
        {
            // check if we have a world instance and if it has a world_id
            if (string.IsNullOrEmpty(StaticInstance.world_id))
            {
                Debug.LogError("(World) Cannot get current static world data path: world_id is null or empty.");
                return null;
            }
            return Path.Combine(WorldDataPath, StaticInstance.world_id);
        }
    }
    /* private static World _instance;
    public static World StaticInstance
    {
        get
        {
            if (_instance == null)
            {
                // find the World instance in the scene
                _instance = FindFirstObjectByType<World>();
            }
            if (_instance == null) { Debug.LogError("(World) No World instance found in the scene. Please add one to the scene."); }
            return _instance;
        }
    } */

    [Header("Current world")]
    public string world_id;
    public WorldData data;
    public string CurrentWorldDataPath => Path.Combine(WorldDataPath, world_id);

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



    [Header("Logs")]
    public bool log = false;
    public bool log_id_generation = false;

    // LOAD / UNLOAD WORLD
    public void LoadWorld(string world_id)
    {
        string json = extract_world_json(world_id);
        if (json == null) { return; }

        data = JsonUtility.FromJson<WorldData>(json);

        // load the data inside the world
        this.world_id = world_id;
        if (log) { Debug.Log($"(World) Loaded world data for world_id: {world_id}\n\n{json}"); }

        // and then we init all the engines
        LevelEngine.StaticInstance.Init();
        RoomEngine.StaticInstance.Init();
        CapableSystem.StaticInstance.Init();
        CapacityEngine.StaticInstance.Init();
    }
    private string extract_world_json(string world_id)
    {
        AppManager.EnsureFolderExists(WorldDataPath);
        string world_path = Path.Combine(WorldDataPath, world_id);
        
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

    // START
    private void Start()
    {
        // we load the start level
        if (data == null || data.levels_ids == null || data.levels_ids.Count == 0)
        {
            if (log) { Debug.Log($"(World) STARTING WORLD: {world_id} (!) {(data == null ? "DATA IS NULL" : "NO LEVELS FOUND")}"); }
            return;
        }
        string start_level_id = data.levels_ids[0];
        if (log) { Debug.Log($"(World) STARTING WORLD: {world_id}  -- Level: {start_level_id}"); }
        if (!string.IsNullOrEmpty(start_level_id)) { LevelEngine.Instance.LoadLevel(start_level_id); }

        // we tp the player to the fallback spawn point while loading the world
        if (fallback_spawn_point != null) { Controller.Instance.Capable.transform.position = fallback_spawn_point.position; }
    }

    // WORLD SAVING
    public void EnsureWorldDataHierarchy()
    {
        if (string.IsNullOrEmpty(world_id))
        {
            if (log) { Debug.LogWarning("(World) Cannot ensure world data hierarchy : world_id is null or empty."); }
            return;
        }

        // create the worlds folder if it doesn't exist
        AppManager.EnsureFolderExists(WorldDataPath);

        // then we do the same for the current world folder
        string world_path = CurrentWorldDataPath;
        AppManager.EnsureFolderExists(world_path);

        // then we ensure that the data folder hierarchy is correct (create them if they don't exist)
        // world folder hierarchy is :
        // - worlds/
        //     - world_id/
        //         - world_data.json
        //         - levels/
        //         - rooms/
        //         - capables/
        //         - capacities/

        AppManager.EnsureFolderExists(Path.Combine(world_path, "levels"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "rooms"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "capables"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "capacities"));

        // we save the world data to a json file in the current world folder
        // string world_data_json_path = Path.Combine(world_path, "world_data.json");
    }

    // STATIC DATA EXTRACTION
    public WorldData GetStaticData()
    {
        WorldData new_data = new WorldData
        {
            levels_ids = get_static_levels_ids(),
            generated_ids_counters = data != null ? data.generated_ids_counters : new Dictionary<string, int>()
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
        foreach (Level level in levels) { level_ids.Add(level.name); }
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
            if (!levels_ids.Contains(level.name)) { continue; }
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
        // todo if we have perf issues when spawning capables this can be the issue
        // then we just need to have a static int that we increment so it's faster
        // or have a dict with max ids per prefix

        // we check if the base_id can be splitted with "_"
        // string[] parts = base_id.Split('-');
        // string suffix = parts.Length > 1 ? parts[parts.Length - 1] : "";
        // string prefix = base_id.Substring(0, base_id.Length - suffix.Length);
        // string prefix = parts[0];
        string prefix = get_id_prefix(base_id);
        // if (suffix == "") { prefix += "-"; } // if we got no suffix, we add a - to the prefix so it will be alrgiht next time

        // we go through all generated_ids and memorize all the ids that have the same prefix and check the suffix int is greater or not
        // int max_suffix = get_next_id_suffix(prefix);
        // int max_suffix = 0;
        /* foreach (string id in generated_ids)
        {
            if (id.StartsWith(prefix))
            {
                string id_suffix = id.Substring(prefix.Length);
                if (int.TryParse(id_suffix, out int id_suffix_int))
                {
                    if (id_suffix_int > max_suffix)
                    {
                        max_suffix = id_suffix_int;
                    }
                }
            }
        } */

        // construct final id
        string new_id = prefix + "-" + get_next_id_suffix(prefix);
        if (log_id_generation) { Debug.Log($"(World) Generated unique ID: {new_id} (base_id: {base_id}, prefix: {prefix}, max id for this prefix: {generated_ids_counters[prefix]})"); }
        return new_id;
    }
    public void ClearGeneratedIDs() { generated_ids_counters.Clear(); }
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
    public List<string> levels_ids;
    public Dictionary<string, int> generated_ids_counters;
}