using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    // SINGLETON
    public static WorldManager Instance { get; private set; }
    public static WorldManager StaticInstance
    {
        get
        {
            if (Instance != null) { return Instance; }
            Instance = FindFirstObjectByType<WorldManager>();
            if (Instance == null)
            {
                Debug.LogError("No instance of WorldManager found in the scene. Please make sure to add a WorldManager component to a game object in the scene.");
            }
            return Instance;
        }
    }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }

        load_world_icons_paths();

        load_existing_worlds_data();
    }



    [Header("Selected World")]
    private WorldData selected_world_data;
    public WorldData SelectedWorldData { get { return selected_world_data; } }
    public string SelectedWorld { get { return selected_world_data != null ? selected_world_data.id : ""; } }


    [Header("World Creation")]
    [SerializeField] private List<Color> world_colors = new List<Color>();
    [SerializeField] private List<Sprite> world_icons = new List<Sprite>();
    [SerializeField] private bool auto_load_on_creation = false;
    private WorldIconsPath world_icons_path;
    private string world_icons_data_path = Path.Combine("data", "world_icons_path.json");
    protected void load_world_icons_paths()
    {
        // if unity editor, we find the paths for the world icons and we write them to a json data file
        #if UNITY_EDITOR
        this.world_icons_path = new WorldIconsPath();
        for (int i = 0; i < world_icons.Count; i++)
        {
            Sprite icon = world_icons[i];
            string path = UnityEditor.AssetDatabase.GetAssetPath(icon);
            world_icons_path.world_icons_paths.Add(path);
            world_icons_path.world_icons_names.Add(icon.name);
        }
        // save the current data to a json file
        string json = JsonUtility.ToJson(world_icons_path, true);
        string full_path = Path.Combine("Assets", "Resources", world_icons_data_path);
        System.IO.File.WriteAllText(full_path, json, System.Text.Encoding.UTF8);
        #else

        // if in build, we load the paths from the json data file
        AppManager.LoadJsonFromAsset("data/world_icons_path", out WorldIconsPath loaded_world_icons_path);
        this.world_icons_path = loaded_world_icons_path;
        #endif

        // normally at this point we should have 2 lists, one with the sprites and one with the paths !
        // and they should be synced
        if (world_icons.Count != world_icons_path.world_icons_paths.Count)
        {
            Debug.LogError($"(UI_WorldSelector) The number of world icons ({world_icons.Count}) does not match the number of world icon paths ({world_icons_path.world_icons_paths.Count}). Please check the world icons and paths.");
        }
        if (log_icons_paths)
        {
            string log = $"(WorldManager) Loaded world icons : {world_icons.Count}\n";
            for (int i = 0; i < world_icons_path.world_icons_paths.Count; i++)
            {
                log += $"Icon: {world_icons[i].name}, Path: {world_icons_path.world_icons_paths[i]}\n";
            }
            Debug.Log(log);
        }
    }
    

    public static bool log = true;

    [Header("Logs awake")]
    [SerializeField] private bool log_icons_paths = false;
    [SerializeField] private bool log_world_data_loading_on_awake = false;
    [SerializeField] private bool hide_files_not_found = false;

    [Header("Logs selecting world")]
    [SerializeField] private bool log_selecting_world = false;

    [Header("Logs creating world")]
    [SerializeField] private bool log_create = false;

    ///
    //
    /// LOADING EXISTING WORLD DATA & SELECTING WORLDS
    //
    ///

    // CHECK EXISTING WORLDS
    private Dictionary<string, WorldData> existing_worlds_data = new Dictionary<string, WorldData>();
    public List<WorldData> ExistingWorlds { get { return new List<WorldData>(existing_worlds_data.Values); } }
    private void load_existing_worlds_data()
    {
        existing_worlds_data.Clear();

        // we check if the worlds data folder exists
        if (!Directory.Exists(WorldManager.WorldsDataPath)) { return; }

        // we get all the world folders in the worlds data folder
        string[] world_folders = Directory.GetDirectories(WorldManager.WorldsDataPath);
        foreach (string world_folder in world_folders)
        {
            // we get the world_id from the folder name
            string world_id = Path.GetFileName(world_folder);

            // we load the world data from the json file in the world folder
            string world_data_json_path = Path.Combine(world_folder, "world_data.json");
            string json = AppManager.LoadJsonFromPersistentDataPath(world_data_json_path);
            if (string.IsNullOrEmpty(json))
            {
                if (!hide_files_not_found) { Debug.LogWarning($"(WorldManager) Failed to load world data for world_id: {world_id} from path: {world_data_json_path}"); }
                continue;
            }
            WorldData world_data = JsonUtility.FromJson<WorldData>(json);
            world_data.id = world_id; // we set the world_id in the data for easier access to it later, even if it's not serialized

            // we check if it has no icon path
            if (string.IsNullOrEmpty(world_data.icon_path))
            {
                world_data.color = GetRandomWorldColor();
                world_data.icon_path = GetRandomIconPath(out string icon_name);
                world_data.icon_name = icon_name;
            }

            // we add the loaded data to the existing worlds data dictionary
            existing_worlds_data.Add(world_id, world_data);
            if (log_world_data_loading_on_awake) { Debug.Log($"(WorldManager) Loaded world data for world_id: {world_id} from path: {world_data_json_path}\n\n{json}"); }
        }
    }

    // SELECT WORLD
    public void SelectWorld(WorldData world_data)
    {
        selected_world_data = world_data;
        if (log_selecting_world) { Debug.Log($"(WorldManager) Selected world with world_id: {SelectedWorld}"); }
    }
    public void SelectWorld(string world_id)
    {
        // we check if the world_id exists in the existing worlds data dictionary
        if (!existing_worlds_data.TryGetValue(world_id, out WorldData world_data))
        {
             if (log_selecting_world) { Debug.LogWarning($"(WorldManager) Failed to select world with world_id: {world_id} because it does not exist in the existing worlds data."); }
            return;
        }

        SelectWorld(world_data);
    }



    ///
    //
    /// SAVING WORLD
    //
    ///


    // WORLD SAVING
    private static string worlds_path = "worlds";
    public static string WorldsDataPath => Path.Combine(Application.persistentDataPath, worlds_path);
    public static string GetWorldDataPath(string world_id) => Path.Combine(WorldsDataPath, world_id);
    public static string CurrentStaticWorldDataPath
    {
        get
        {
            // check if we have a world instance and if it has a world_id
            if (string.IsNullOrEmpty(StaticInstance.SelectedWorld))
            {
                Debug.LogError("(WorldManager) Cannot get current static world data path: SelectedWorld is null or empty.");
                return null;
            }
            return Path.Combine(WorldsDataPath, StaticInstance.SelectedWorld);
        }
    }
    public static string StaticSelectedWorld => StaticInstance?.SelectedWorld;

    /// <summary>
    /// this ensures that all the world data hierarchy folders exists for
    /// properly saving the given world data
    /// </summary>
    /// <param name="world_id"></param>
    /// <returns>returns true if the hierarchy was just created !</returns>
    public static bool EnsureWorldDataHierarchy(string world_id)
    {
        if (string.IsNullOrEmpty(world_id))
        {
            if (log) { Debug.LogWarning("(WorldManager) Cannot ensure world data hierarchy : world_id is null or empty."); }
            return false;
        }

        // create the /worlds folder if it doesn't exist
        AppManager.EnsureFolderExists(WorldsDataPath);

        // then we do the same for the current world folder
        string world_path = GetWorldDataPath(world_id);
        bool world_folder_created = AppManager.EnsureFolderExists(world_path);

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
        return world_folder_created;
    }

    ///
    //
    /// CREATE / LOAD SELECTED WORLD
    //
    ///

    private World _world;
    private World world
    {
        get
        {
            if (_world != null) { return _world; }
            // we instantly load the world even if World.Instance is not defined yet.
            // so we use World.StaticInstance which will find the world instance with FindObjectByType
            _world = World.StaticInstance;
            return _world;
        }
    }
    
    
    // LOAD / UNLOAD WORLD
    public async Task LoadSelectedWorld()
    {

        // first we check that a world is not already loaded
        while (world.IsWorldLoadingOrUnloading) { await Task.Delay(100); }
        if (world.IsWorldLoaded) { await UnloadCurrentWorld(); }

        // we load the good world.
        // for this we have multiple choices :
        // 1. if SelectedWorld is defined, we load it (this is set by the world selector in the main menu)
        // 3. else if the world instance has a world_id defined, we load it

        if (!string.IsNullOrEmpty(SelectedWorld)) { } // we do nothing, we are good !
        else if (!string.IsNullOrEmpty(world.world_id))
        {
            // we select the world from the world instance world_id
            SelectWorld(world.world_id);
        }

        await world.LoadWorld(SelectedWorld);
    }
    public async Task UnloadCurrentWorld()
    {
        // first we check that a world is not already loaded
        while (world.IsWorldLoadingOrUnloading) { await Task.Delay(100); }
        if (!world.IsWorldLoaded) { return; }
        await world.UnloadWorld();
    }

    // CREATE WORLD
    public void CreateNewWorld()
    {
        // we ask a popup to enter the world name
        UI_Manager.Instance.OpenInputPopup("Enter world name", "world", CreateNewWorld);
    }
    public void CreateNewWorld(string world_name)
    {
        // we check if the world name is valid
        if (string.IsNullOrEmpty(world_name))
        {
            if (log_create) { Debug.LogWarning("(WorldManager) Failed to create new world because the world name is null or empty."); }
            return;
        }

        // and we check if the world name is not already taken
        if (existing_worlds_data.ContainsKey(world_name))
        {
            // we add a random number to the name and try again
            string new_world_name = world_name + "_" + UnityEngine.Random.Range(0, 1000);
            if (log_create) { Debug.LogWarning($"(WorldManager) Failed to create new world because the world name '{world_name}' is already taken. Trying with new name: '{new_world_name}'"); }
            CreateNewWorld(new_world_name);
            return;
        }

        // we then create the world data and save it (which will create the hierarchy folders if needed)
        WorldData new_data = new WorldData() { id = world_name };
        existing_worlds_data.Add(world_name, new_data);
        SaveEngine.SaveWorldData(new_data);
        if (log_create) { Debug.Log($"(WorldManager) Created new world with world_id: {world_name}"); }

        if (!auto_load_on_creation) { return; }
        // we select & load the new world
        SelectWorld(new_data);
        SceneLoader.Instance.LoadGame();
    }

    // CREATE LEVEL
    public void CreateNewLevel()
    {
        if (string.IsNullOrEmpty(SelectedWorld))
        {
            if (log_create) { Debug.LogWarning("(WorldManager) Failed to create new level because no world is selected."); }
            return;
        }

        // we ask a popup to enter the level name
        UI_Manager.Instance.OpenInputPopup("Enter level name", "level", CreateNewLevel);
    }
    public void CreateNewLevel(string level_id)
    {
        // we check if the level name is valid
        if (string.IsNullOrEmpty(level_id))
        {
            if (log_create) { Debug.LogWarning("(WorldManager) Failed to create new level because the level name is null or empty."); }
            return;
        }

        // and we check if the level name is not already taken in the current world
        List<LevelData> existing_levels_data_list = LevelEngine.LoadWorldLevelsData(SelectedWorld);
        List<string> existing_levels_names = existing_levels_data_list.Select(ld => ld.id).ToList();
        if (existing_levels_names.Contains(level_id))
        {
            // we add a random number to the name and try again
            string new_level_name = level_id + "_" + UnityEngine.Random.Range(0, 1000);
            if (log_create) { Debug.LogWarning($"(WorldManager) Failed to create new level because the level name '{level_id}' is already taken. Trying with new name: '{new_level_name}'"); }
            CreateNewLevel(new_level_name);
            return;
        }

        // we then create the level data and save it (which will create the hierarchy folders if needed)
        LevelData new_level_data = new LevelData() { id = level_id };
        SaveEngine.SaveLevelData(new_level_data, SelectedWorld);
        if (log_create) { Debug.Log($"(WorldManager) Created new level in world {SelectedWorld} with level_id: {level_id}"); }

        // then we need to mark the level as dirty ??
    }

    ///
    //
    /// GETTERS
    //
    ///


    // GETTERS
    public Color GetRandomWorldColor() { return world_colors[UnityEngine.Random.Range(0, world_colors.Count)]; }
    public string GetRandomIconPath(out string icon_name)
    {
        if (world_icons_path == null || world_icons_path.world_icons_paths == null || world_icons_path.world_icons_paths.Count == 0)
        {
            if (log_icons_paths) { Debug.LogWarning("(WorldManager) World icons paths was null when trying to get a random icon path. Reloading the world icons paths."); }
            load_world_icons_paths();
        }
        int index = UnityEngine.Random.Range(0, world_icons_path.world_icons_paths.Count);
        icon_name = world_icons_path.world_icons_names[index];
        return world_icons_path.world_icons_paths[index];
    }
    public Sprite GetIconSprite(string icon_path, string icon_name)
    {
        for (int i = 0; i < world_icons_path.world_icons_paths.Count; i++)
        {
            if (world_icons_path.world_icons_paths[i] == icon_path && world_icons_path.world_icons_names[i] == icon_name)
            {
                if (world_icons.Count <= i)
                {
                    Debug.LogError($"(WorldManager) Failed to get icon sprite for icon path: {icon_path} and icon name: {icon_name} because the index {i} is out of range for the world icons list (count: {world_icons.Count}).");
                    return null;
                }
                return world_icons[i];
            }
        }
        Debug.LogError($"(WorldManager) Failed to get icon sprite for icon path: {icon_path} and icon name: {icon_name} because it was not found in the world icons.");
        return null;
    }
}

[Serializable] public class WorldIconsPath
{
    public List<string> world_icons_paths = new List<string>();
    public List<string> world_icons_names = new List<string>();
}