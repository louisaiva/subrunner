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
    private static World _instance;
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
    }

    [Header("Current world")]
    public string world_id;
    public WorldData data;
    public string CurrentWorldDataPath => Path.Combine(WorldDataPath, world_id);


    [Header("Spawn")]
    public Transform fallback_spawn_point; // if no player data were found on LoadWorld, we will spawn the player at this position

    [Header("Logs")]
    public bool log = false;

    // LOAD / UNLOAD WORLD
    public void LoadWorld(string world_id)
    {
        string json = extract_world_json(world_id);
        if (json == null) { return; }

        data = JsonUtility.FromJson<WorldData>(json);

        // load the data inside the world
        this.world_id = world_id;
        if (log) { Debug.Log($"(World) Loaded world data for world_id: {world_id}\n\n{json}"); }
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

    public WorldData GetStaticData()
    {
        WorldData new_data = new WorldData
        {
            levels_ids = get_static_levels_ids()
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

}


[Serializable] public class WorldData
{
    public List<string> levels_ids;
}