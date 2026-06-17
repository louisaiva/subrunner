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
    public static WorldManager LazyInstance
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

        // refresh existing worlds
        RefreshExistingWorldsData();

        // and existing world templates
        GatherExistingWorldsTemplates();
    }



    [Header("Selected World")]
    private WorldData selected_world_data;
    public WorldData SelectedWorldData { get { return selected_world_data; } }
    public string SelectedWorld { get { return selected_world_data != null ? selected_world_data.id : ""; } }
    public static string LazyWorld { get { return LazyInstance.SelectedWorld; } }


    [Header("World Creation")]
    [SerializeField] private List<Color> world_colors = new List<Color>();
    [SerializeField] private List<Sprite> world_icons = new List<Sprite>();
    // [SerializeField] private bool auto_load_on_creation = false;
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


    // EVENTS
    public event Action OnWorldLoading;
    public event Action OnWorldLoaded;
    public event Action OnWorldUnloading;
    public event Action OnWorldUnloaded;


    ///
    //
    /// LOADING EXISTING WORLD DATA & SELECTING WORLDS
    //
    ///

    // CHECK EXISTING WORLDS
    private Dictionary<string, WorldDataHelper> existing_worlds_data = new Dictionary<string, WorldDataHelper>();
    public List<WorldDataHelper> ExistingWorlds { get { return new List<WorldDataHelper>(existing_worlds_data.Values); } }
    public int ExistingWorldsCount { get { return existing_worlds_data.Count; } }
    public void RefreshExistingWorldsData()
    {
        existing_worlds_data.Clear();

        // we check if the worlds data folder exists
        if (!Directory.Exists(WorldsDataPath)) { return; }

        // we get all the world folders in the worlds data folder
        string[] world_folders = Directory.GetDirectories(WorldsDataPath);
        foreach (string world_folder in world_folders)
        {
            // we get the world_id from the folder name
            string world_id = Path.GetFileName(world_folder);
            if (existing_worlds_data.ContainsKey(world_id)) { continue; }

            // we try to load the WorldDataHelper from the SaveEngine
            WorldDataHelper world_data_helper = SaveEngine.GetWorldData(world_id);
            if (world_data_helper == null || world_data_helper.world == null)
            {
                if (!hide_files_not_found) { Debug.LogWarning($"(WorldManager) Failed to load world data for world_id: {world_id} from folder: {world_folder}"); }
                continue;
            }
            add_world_data_to_existing(world_data_helper, world_id);

            if (log_world_data_loading_on_awake) { Debug.Log($"(WorldManager) Loaded world data for world_id: {world_id} from folder {world_folder}\n{world_data_helper.GetDetails()}"); }
        }
    }
    private void add_world_data_to_existing(WorldDataHelper whelper, string id)
    {
        whelper.world.id = id; // we set the id in the data for easier access to it later, even if it's not serialized

        // we check if it has no icon path
        if (string.IsNullOrEmpty(whelper.world.icon_path))
        {
            whelper.world.color = GetRandomWorldColor();
            whelper.world.icon_path = GetRandomIconPath(out string icon_name);
            whelper.world.icon_name = icon_name;
        }

        // we add the loaded data to the existing worlds data dictionary
        existing_worlds_data.Add(id, whelper);
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
        if (!existing_worlds_data.TryGetValue(world_id, out WorldDataHelper w_helper))
        {
            if (log_selecting_world) { Debug.LogWarning($"(WorldManager) Failed to select world with world_id: {world_id} because it does not exist in the existing worlds data."); }
            return;
        }

        SelectWorld(w_helper.world);
    }



    ///
    //
    /// WORLD TEMPLATES
    //
    ///
    private string world_templates_path = "data/world_templates"; // in this folder there are worldsavedata.json files
    private Dictionary<string, string> existing_worlds_templates = new Dictionary<string, string>(); // id, json
    public void GatherExistingWorldsTemplates()
    {
        // we clear the existing worlds templates list
        existing_worlds_templates.Clear();

        // we load the world templates from the json files in the data world templates folder
        TextAsset[] json_assets = Resources.LoadAll<TextAsset>(world_templates_path);
        string debug = "";
        foreach (TextAsset json_asset in json_assets)
        {
            existing_worlds_templates.Add(json_asset.name, json_asset.text);
            debug += " - " + json_asset.name + "\n";
        }
        if (log_world_data_loading_on_awake) { Debug.Log($"(WorldManager) Gathered {existing_worlds_templates.Count} existing world templates from path: {world_templates_path}\n{debug}"); }
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
            if (string.IsNullOrEmpty(LazyInstance.SelectedWorld))
            {
                Debug.LogError("(WorldManager) Cannot get current static world data path: SelectedWorld is null or empty.");
                return null;
            }
            return Path.Combine(WorldsDataPath, LazyInstance.SelectedWorld);
        }
    }
    public static string StaticSelectedWorld => LazyInstance?.SelectedWorld;

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
        //         - chunks/
        //         - capables/
        //         - capacities/

        AppManager.EnsureFolderExists(Path.Combine(world_path, "levels"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "rooms"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "chunks"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "capables"));
        AppManager.EnsureFolderExists(Path.Combine(world_path, "capacities"));

        // we save the world data to a json file in the current world folder
        return world_folder_created;
    }
    public static bool EnsureWorldDataFolderExists(string world_id)
    {
        if (string.IsNullOrEmpty(world_id))
        {
            if (log) { Debug.LogWarning("(WorldManager) Cannot ensure world data folder exists : world_id is null or empty."); }
            return false;
        }

        // create the /worlds folder if it doesn't exist
        AppManager.EnsureFolderExists(WorldsDataPath);

        // then we do the same for the current world folder
        string world_path = GetWorldDataPath(world_id);
        bool world_folder_created = AppManager.EnsureFolderExists(world_path);

        return world_folder_created;
    }
    public static bool EnsureSchematicHierarchy(string world_id)
    {
        if (string.IsNullOrEmpty(world_id))
        {
            if (log) { Debug.LogWarning("(WorldManager) Cannot ensure schematic hierarchy : world_id is null or empty."); }
            return false;
        }
        // first we ensure that the world data folder exists
        EnsureWorldDataFolderExists(world_id);

        // then we ensure that there is a "schematics" folder in the world data folder
        string schematics_path = Path.Combine(GetWorldDataPath(world_id), "schematics");
        bool schematics_folder_created = AppManager.EnsureFolderExists(schematics_path);
        return schematics_folder_created;
    }
    public static bool DoesWorldSaveDataExists(string world_id)
    {
        if (string.IsNullOrEmpty(world_id))
        {
            if (log) { Debug.LogWarning("(WorldManager) Cannot check if world save data exists : world_id is null or empty."); }
            return false;
        }

        string world_data_path = Path.Combine(WorldsDataPath, world_id, "save");
        return System.IO.File.Exists(world_data_path);
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
            // so we use World.LazyInstance which will find the world instance with FindObjectByType
            _world = World.LazyInstance;
            return _world;
        }
    }


    // LOAD / UNLOAD WORLD
    private bool still_loading_awaitable = false;
    public async Awaitable LoadSelectedWorldAwaitable()
    {
        still_loading_awaitable = true;
        LoadSelectedWorld();
        while (still_loading_awaitable)
        {
            await Task.Delay(100);
        }
    }
    public async void LoadSelectedWorld()
    {
        // first we check that a world is not already loaded
        while (world.IsWorldLoadingOrUnloading) { await Task.Delay(100); }
        if (world.IsWorldLoaded) { await UnloadCurrentWorld(); }

        // we load the good world.
        // for this we have multiple choices :
        // 1. if SelectedWorld is defined, we load it (this is set by the world selector in the main menu)
        // 3. else if the world instance has a world_id defined, we load it

        if (!string.IsNullOrEmpty(SelectedWorld)) { } // we do nothing, we are good !
        else if (!string.IsNullOrEmpty(world.world_to_load))
        {
            // we select the world from the world instance world_to_load
            SelectWorld(world.world_to_load);
        }

        OnWorldLoading?.Invoke();
        await world.LoadWorld(SelectedWorld);
        OnWorldLoaded?.Invoke();
        still_loading_awaitable = false;
    }
    public async Task UnloadCurrentWorld()
    {
        // first we check that a world is not already loaded
        while (world.IsWorldLoadingOrUnloading)
        {
            if (world == null) { return; }
            await Task.Delay(100);
            if (world == null) { return; }
        }
        if (!world.IsWorldLoaded) { return; }
        OnWorldUnloading?.Invoke();
        await world.UnloadWorld();
        OnWorldUnloaded?.Invoke();
    }

    // CREATE WORLD
    public void CreateNewWorld()
    {
        // we ask a popup to enter the world name
        UI_Manager.Instance.OpenInputPopup("Enter world name", "world", CreateNewWorldWithName, dont_auto_close: true); // we do manual closing after it is done
    }
    public void CreateNewWorldWithName(string world_name = "_____//-!!-_o_o__")
    {
        CreateNewWorldWithName(world_name, "demo");
    }
    public void CreateNewWorldWithName(string world_name = "_____//-!!-_o_o__", bool auto_load = false)
    {
        CreateNewWorldWithName(world_name, "demo", auto_load);
    }
    public void CreateNewWorldWithName(string world_name, string world_template, bool auto_load = false)
    {
        // we check if the world name is valid
        if (string.IsNullOrEmpty(world_name))
        {
            if (log_create) { Debug.LogWarning("(WorldManager) Failed to create new world because the world name is null or empty."); }
            return;
        }
        else if (world_name == "_____//-!!-_o_o__") { world_name = get_first_not_existing_world_name_with_prefix(world_template); }

        // and we check if the world name is not already taken
        if (existing_worlds_data.ContainsKey(world_name))
        {
            // we add a random number to the name and try again
            string new_world_name = world_name + "_" + UnityEngine.Random.Range(0, 1000);
            if (log_create) { Debug.LogWarning($"(WorldManager) Failed to create new world because the world name '{world_name}' is already taken. Trying with new name: '{new_world_name}'"); }
            CreateNewWorldWithName(new_world_name, world_template);
            return;
        }

        // we then create the world data and save it (which will create the hierarchy folders if needed)
        WorldSaveData new_world = DuplicateTemplate(world_template, world_name);
        new_world.controller.play_intro_cinematic = true;
        new_world.controller.UpdatePlayerLevelRoomChunk();
        existing_worlds_data.Add(world_name, new_world.ToWorldDataHelper());


        SaveEngine.SaveWorldSaveData(new_world);

        if (log_create) { Debug.Log($"(WorldManager) Created new world with world_id: {world_name}"); }
        UI_Manager.Instance.CloseInputPopup();

        if (!auto_load) { return; }
        // we select & load the new world
        SelectWorld(new_world.world);
        SceneLoader.Instance.LoadGame();
    }
    private WorldSaveData DuplicateTemplate(string world_template, string world_name)
    {
        if (string.IsNullOrEmpty(world_template) || world_template == "default")
        {
            // we create a new world save data with default values
            WorldSaveData new_world = new WorldSaveData()
            {
                world = new WorldData()
                {
                    id = world_name
                },
                controller = new ControllerData()
                {
                    capable_template = "bob",
                    stack_capable_ids = new List<string>(),
                },
            };
            return new_world;
        }


        // find the good template
        foreach (var kvp in existing_worlds_templates)
        {
            if (kvp.Key != world_template) { continue; }
            WorldSaveData template = SaveEngine.LoadWorldSaveFromJson(kvp.Value);
            template.world.id = world_name; // we set the new world name
            if (log_create) { Debug.Log($"(WorldManager) Loaded world template '{world_template}' to create new world '{world_name}'"); }
            return template;
        }

        Debug.LogWarning($"(WorldManager) Failed to find world template with name: {world_template}. Creating a default world instead.");
        return DuplicateTemplate("default", world_name);
    }
    private string get_first_not_existing_world_name_with_prefix(string prefix)
    {
        int index = 0;
        foreach (var kvp in existing_worlds_data)
        {
            if (!kvp.Key.StartsWith(prefix)) { continue; }
            string suffix = kvp.Key.Substring(prefix.Length);
            if (!int.TryParse(suffix, out int suffix_index)) { continue; }
            index = Math.Max(index, suffix_index + 1);
        }
        return $"{prefix}{index}";
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
        SaveEngine.SaveLevelDataInFolder(new_level_data, SelectedWorld);
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