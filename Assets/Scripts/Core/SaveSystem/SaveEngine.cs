using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class SaveEngine : MonoBehaviour
{
    // STATIC SINGLETON
    public static SaveEngine Instance { get; private set; }
    public static SaveEngine LazyInstance
    {
        get
        {
            if (Instance != null) { return Instance; }
            Instance = FindFirstObjectByType<SaveEngine>();
            if (Instance == null)
            {
                Debug.LogError("No instance of SaveManager found in the scene. Please make sure to add a SaveManager component to a game object in the scene.");
            }
            return Instance;
        }
    }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }
    }


    // SUBSYSTEMS
    private static AIO_Loader _static_loader;
    public static AIO_Loader AIO_Loader
    {
        get
        {
            if (_static_loader != null) { return _static_loader; }
            if (LevelEngine.LazyInstance == null) { return null; }
            _static_loader = LevelEngine.LazyInstance.transform.GetComponentInChildren<AIO_Loader>(includeInactive: true);
            if (_static_loader == null) { Debug.LogError("(SaveEngine) SaveEngine.AIO_Loader was not found"); }
            return _static_loader;
        }
    }


    [Header("Save Parameters")]
    [SerializeField] private bool save_as_one_file = true; // if true, saves the whole world save data in a single json file, if false, saves the world save data in multiple json files in a folder structure
    private static JsonSerializerSettings one_file_settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        ContractResolver = new UnityValueTypeContractResolver()
        /* ,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects */
    };
    /* private static JsonTextWriter Writer = new JsonTextWriter(new StringWriter())
    {
        Formatting = Formatting.Indented,
        Indentation = 4,
        IndentChar = ' '
    }; */
    
    [Header("Logs")]
    [SerializeField] private Loggable<SaveEngine> log;
    private static Loggable<SaveEngine> slog => LazyInstance != null ? LazyInstance.log : null;
    private static bool log_static => LazyInstance != null && LazyInstance.log.Verbose >= Verbosity.Extended;
    [SerializeField] private bool log_capable_data_kind_checking = false;
    private static bool _log_capable_data_kind_checking => LazyInstance != null ? LazyInstance.log_capable_data_kind_checking : false;
    [SerializeField] private Loggable<SaveEngine> log_rooms;
    private static Loggable<SaveEngine> s_log_rooms => LazyInstance != null ? LazyInstance.log_rooms : null;
    [SerializeField] private Loggable<SaveEngine> log_capables;
    private static Loggable<SaveEngine> s_log_capables => LazyInstance != null ? LazyInstance.log_capables : null;
    [SerializeField] private Loggable<SaveEngine> log_clean;
    private static Loggable<SaveEngine> slog_clean => LazyInstance != null ? LazyInstance.log_clean : null;

    [Header("GetStaticData Logs")]
    public Loggable<Capable> log_gsd_capable;


    ///
    //
    /// DYNAMIC WORLD SAVE
    //
    ///


    /// <summary>
    /// this method is the main saving method at
    /// runtime. For now it only saves all data to their files,
    /// but in the future we can split the json saving through a
    /// specific pipeline that only saves the variables with custom attributes
    /// that differ from the template one :D -> faster saving time + smaller save file
    /// </summary>
    public static void SaveDynamicWorld()
    {
        WorldSaveData save = generate_save_from_dynamic_world();
        if (save == null)
        {
            slog?.Error("Failed to generate world save data. Aborting saving.");
            return;
        }

        SaveWorldSaveData(save);
        slog?.Log($"Finished dynamic saving of world '{save.world.id}' !");

        // notif
        UI_Manager.Instance.GetPool<UI_HUD>()?.Notifier?.Notify("game saved !");
    }
    private static WorldSaveData generate_save_from_dynamic_world()
    {
        string world_id = WorldManager.StaticSelectedWorld;
        if (string.IsNullOrEmpty(world_id))
        {
            slog?.Error("No world selected, cannot generate save data from dynamic world.");
            return null;
        }
        slog?.Log($"Saving Dynamic World '{world_id}'");
        WorldSaveData save = new WorldSaveData();

        // saving the world data
        save.world = World.LazyInstance.data;
        slog?.Log($"Gathered world data : '{world_id}'");

        // controller
        ControllerData controller_data = Controller.LazyInstance.data;
        controller_data.UpdatePlayerLevelRoomChunk(slog);
        save.controller = controller_data;
        slog?.Log($"Gathered controller data : '{controller_data.controlled_capable_id}'");

        // levels
        List<string> rooms_ids = new List<string>();
        int levels_count = save.world.levels_ids != null ? save.world.levels_ids.Count : 0;
        string tmp_log = "\n";
        for (int i = 0; i < levels_count; i++)
        {
            LevelData level_data = LevelEngine.LazyInstance.GetLevelDataFromID(save.world.levels_ids[i]);
            save.levels.Add(level_data);
            // SaveLevelData(level_data, world_id);
            tmp_log += $"- '{level_data.id}' -------------- {level_data.rooms_ids.Count} rooms\n";
            rooms_ids.AddRange(level_data.rooms_ids);
        }
        slog?.Log($"Gathered {levels_count} levels :{tmp_log}");

        // rooms
        List<string> chunks_ids = new List<string>();
        List<RoomData> rooms_data = RoomEngine.LazyInstance.GetRoomsDataFromIDs(rooms_ids);
        tmp_log = "\n";
        for (int i = 0; i < rooms_data.Count; i++)
        {
            save.rooms.Add(rooms_data[i]);
            // SaveRoomData(rooms_data[i], world_id);
            tmp_log += $"- '{rooms_data[i].id}' -------------- {rooms_data[i].chunks_ids.Count} chunks\n";
            chunks_ids.AddRange(rooms_data[i].chunks_ids);
        }
        slog?.Log($"Gathered {rooms_data.Count} rooms :{tmp_log}");

        // chunks
        List<string> capables_ids = new List<string>();
        List<ChunkData> chunks_data = ChunkEngine.LazyInstance.GetChunksDataFromIDs(chunks_ids);
        tmp_log = "\n";
        for (int i = 0; i < chunks_data.Count; i++)
        {
            save.chunks.Add(chunks_data[i]);
            // SaveChunkData(chunks_data[i], world_id);
            tmp_log += $"- '{chunks_data[i].id}' -------------- {chunks_data[i].capables_ids.Count} capables  /  {chunks_data[i].movables_ids.Count} movables\n";
            capables_ids.AddRange(chunks_data[i].capables_ids);
            capables_ids.AddRange(chunks_data[i].movables_ids);
        }
        slog?.Log($"Gathered {chunks_data.Count} chunks :{tmp_log}");


        // now we save the dynamic data of capables & capacities
        // so the data we are saving is up to date
        slog?.Log($"Saving dynamic data of loaded capables and capacities...");
        CapableEngine.LazyInstance.SaveLoadedCapablesDynamicData();
        CapacityEngine.LazyInstance.SaveLoadedCapacitiesDynamicData();
        slog?.Log($"Dynamic data of loaded capables / capacities updated :D");

        // capables
        // we need to do a while loop until the capables_ids list is real empty
        // bcz there can be capable in a capable in a capable in a capable etc etc etc
        // so the max_iterations of the while loop is the highest intrication depth of
        // capables in the world, but we limit it to 10 just in case (should never happen)
        List<string> capacities_ids = new List<string>();
        int iterations = 0;
        int total_capables = 0;
        while (capables_ids.Count > 0 && iterations < 10)
        {
            iterations++;
            tmp_log = "\n";
            List<CapableData> capables_data = CapableEngine.LazyInstance.GetCapablesDataFromIDs(capables_ids);
            capables_ids.Clear();
            for (int i = 0; i < capables_data.Count; i++)
            {
                save.capables.Add(capables_data[i]);
                // SaveCapableData(capables_data[i], world_id);
                tmp_log += $"- '{capables_data[i].id}' -------------- {(capables_data[i].capacities_ids != null ? capables_data[i].capacities_ids.Count : 0)} capacities  /  {(capables_data[i].inventory != null ? capables_data[i].inventory.ItemsCount() : 0)} items\n";
                if (capables_data[i].capacities_ids != null) { capacities_ids.AddRange(capables_data[i].capacities_ids); }
                if (capables_data[i].inventory != null) { capables_ids.AddRange(capables_data[i].inventory.GetAllItemsIds()); }

                // also check for containers
                if (capables_data[i] is ContainerData container_data && container_data.contained_capable_ids != null)
                {
                    capables_ids.AddRange(container_data.contained_capable_ids);
                }
            }
            slog?.Log($"Gathered {capables_data.Count} capables ----- iteration {iterations} :{tmp_log}");
            total_capables += capables_data.Count;
        }
        slog?.Log($"Gathered total {total_capables} capables in {iterations} iterations.");

        // capacities
        List<CapacityData> capacities_data = CapacityEngine.LazyInstance.GetCapacitiesDataFromIDs(capacities_ids);
        tmp_log = "\n";
        for (int i = 0; i < capacities_data.Count; i++)
        {
            save.capacities.Add(capacities_data[i]);
            // SaveCapacityData(capacities_data[i], world_id);
            tmp_log += $"- '{capacities_data[i].id}'\n";
        }
        slog?.Log($"Gathered {capacities_data.Count} capacities :{tmp_log}");

        return save;
    }


    ///
    //
    /// STATIC WORLD SAVE
    //
    ///

    /// <summary>
    /// This method is different from the dynamic save method because
    /// it also updates the static data of rooms & chunks. this is
    /// mainly (if not only) used by the World/LevelBuilder to save
    /// the built levels and rooms and chunks to their static data files.
    /// </summary>
    public static void SaveAIOLevel(Level level, string world_id)
    {
        bool folder = !AppManager.IsSaveASingleFile(world_id);
        slog?.Log($"Saving AIO level '{level.ID}' of world '{world_id}' (save is a {(folder ? "folder structure" : "single file")})");

        if (folder)
        {
            save_static_level_to_folder(level, world_id, save_rooms: true, save_capables: true);
            return;
        }

        // else it is a file save
        save_static_level_to_file(level, world_id, save_rooms: true, save_capables: true);
    }
    private static void save_static_level_to_file(Level level, string world_id, bool save_rooms = true, bool save_capables = true)
    {
        // we load the WSD
        WorldSaveData save = GetWorldSave(world_id);

        // replace the level data
        LevelData data = level.GetStaticData();
        bool found = false;
        for (int i = 0; i < save.levels.Count; i++)
        {
            if (save.levels[i].id != data.id) { continue; }
            save.levels[i] = data;
            found = true;
            break;
        }
        if (!found) { save.levels.Add(data); }

        // replace rooms data
        if (save_rooms)
        {
            List<Room> rooms = level.GetStaticRooms().ToList();
            slog?.Log($"Saving {rooms.Count} rooms of level '{level.ID}'");
            foreach (Room room in rooms)
            {
                // save the room data
                s_log_rooms?.Log($"Saving room '{room.ID}' in world '{world_id}'");
                RoomData rdata = room.GetStaticData();
                found = false;
                for (int i = 0; i < save.rooms.Count; i++)
                {
                    if (save.rooms[i].id != rdata.id) { continue; }
                    save.rooms[i] = rdata;
                    found = true;
                    break;
                }
                if (!found) { save.rooms.Add(rdata); }

                // and the chunks
                List<Chunk> chunks = room.GetStaticChunks().ToList();
                slog?.Log($"Saving {chunks.Count} chunks of room '{room.ID}'");
                foreach (Chunk chunk in chunks)
                {
                    s_log_rooms?.Log($"Saving chunk '{chunk.ID}' in world '{world_id}'");
                    ChunkData cdata = chunk.GetStaticData();
                    found = false;
                    for (int i = 0; i < save.chunks.Count; i++)
                    {
                        if (save.chunks[i].id != cdata.id) { continue; }
                        save.chunks[i] = cdata;
                        found = true;
                        break;
                    }
                    if (!found) { save.chunks.Add(cdata); }
                }
            }
        }

        // replace capables data
        if (save_capables)
        {
            List<Capable> capables = level.GetStaticCapables().ToList();
            List<string> done_ids = new List<string>();
            slog?.Log($"Saving {capables.Count} capables of level '{level.ID}'");
            s_log_capables?.LogExtended($"(SaveEngine - AIO) Level GetStaticCapables gathered {capables.Count} capables of level '{level.ID}' : {string.Join(", ", capables.Select(c => c.ID))}");
            foreach (Capable cap in capables)
            {
                if (log_static) { Debug.Log($"(SaveEngine) Saving capable '{cap.ID}' of level '{level.ID}'"); }
                save_static_capable_to_wsd(cap, save, ref done_ids);
            }
        }

        slog?.Log($"Constructed the WSD for saving AIO level '{level.ID}' of world '{world_id}' into single file. WSD is :" + save.GetDetails());

        SaveWorldSaveData(save);

        slog?.Log($"Finished saving AIO level '{level.ID}' of world '{world_id}' into single file !");

    }
    private static void save_static_level_to_folder(Level level, string world_id, bool save_rooms = true, bool save_capables = true)
    {
        LevelData data = level.GetStaticData();
        SaveLevelDataInFolder(data, world_id);

        // check if we need to save the rooms also
        if (save_rooms)
        {
            List<Room> rooms = level.GetStaticRooms().ToList();
            slog?.Log($"Saving {rooms.Count} rooms of level '{level.ID}'");
            foreach (Room room in rooms)
            {
                // save the room data
                s_log_rooms?.Log($"Saving room '{room.ID}' in world '{world_id}'");
                RoomData rdata = room.GetStaticData();
                SaveRoomDataInFolder(rdata, world_id);

                // and the chunks
                List<Chunk> chunks = room.GetStaticChunks().ToList();
                slog?.Log($"Saving {chunks.Count} chunks of room '{room.ID}'");
                foreach (Chunk chunk in chunks)
                {
                    s_log_rooms?.Log($"Saving chunk '{chunk.ID}' in world '{world_id}'");
                    ChunkData cdata = chunk.GetStaticData();
                    SaveChunkDataInFolder(cdata, world_id);
                }
            }
        }

        // check if we need to save the capables also
        if (save_capables)
        {
            List<Capable> capables = level.GetStaticCapables().ToList();
            slog?.Log($"Saving {capables.Count} capables of level '{level.ID}'");
            // Debug.Log($"(SaveEngine) Saving {capables.Count} capables of level '{level.ID}' : {string.Join(", ", capables.Select(c => c.ID))}");
            foreach (Capable cap in capables)
            {
                if (log_static) { Debug.Log($"(SaveEngine) Saving capable '{cap.ID}' of level '{level.ID}'"); }
                save_static_capable_to_folder(cap, world_id);
            }
        }

        slog?.Log($"Finished saving AIO level '{level.ID}' of world '{world_id}' into folder structure !");
    }
    private static void save_static_capable_to_folder(Capable capable, string world_id, bool save_inventory = true, bool save_capacities = true)
    {
        s_log_capables?.Log($"Saving capable '{capable.ID}' in world '{world_id}' (save_inventory: {save_inventory}, save_capacities: {save_capacities})");

        s_log_capables?.LogSpecific($"Getting capable data '{capable.ID}'");
        CapableData data = (CapableData)capable.GetStaticData();

        s_log_capables?.LogSpecific($"Saving capable data for '{capable.ID}'");
        s_log_capables?.LogSpecific($"Capable data is {(data != null ? "not null" : "null")} \n{(data != null ? JsonUtility.ToJson(data, true) : "")}");
        SaveCapableDataInFolder(data, world_id);

        if (save_capacities)
        {
            List<Capacity> capacities = capable.GetStaticCapacities();
            s_log_capables?.LogExtended($"Saving {capacities.Count} capacities of capable '{capable.ID}'");
            foreach (Capacity capa in capacities)
            {
                s_log_capables?.Log($"Saving capacity '{capa.ID}' in world '{world_id}'");
                CapacityData cdata = capa.GetStaticData();
                SaveCapacityDataInFolder(cdata, world_id);
            }
        }

        if (save_inventory && capable.Inventory != null)
        {
            List<Item> items = capable.Inventory.GetStaticItems();
            s_log_capables?.LogExtended($"Saving {items.Count} items of capable '{capable.ID}'");
            foreach (Item item in items) { save_static_capable_to_folder(item, world_id, save_inventory, save_capacities); }
        }
    }
    private static void save_static_capable_to_wsd(Capable capable, WorldSaveData wsd, ref List<string> done_ids, bool save_inventory = true, bool save_capacities = true)
    {
        if (done_ids.Contains(capable.ID))
        {
            s_log_capables?.Log($"Capable '{capable.ID}' already saved in WorldSaveData '{wsd.ID}', skipping...");
            return;
        }

        s_log_capables?.Log($"Saving capable '{capable.ID}' in WorldSaveData '{wsd.ID}' (save_inventory: {save_inventory}, save_capacities: {save_capacities})");

        s_log_capables?.LogSpecific($"Getting capable data '{capable.ID}'");
        CapableData data = (CapableData)capable.GetStaticData();

        s_log_capables?.LogSpecific($"Saving capable data for '{capable.ID}' to wsd '{wsd.ID}'");
        s_log_capables?.LogSpecific($"Capable data is {(data != null ? "not null" : "null")} \n{(data != null ? JsonUtility.ToJson(data, true) : "")}");
        bool found = false;
        for (int i = 0; i < wsd.capables.Count; i++)
        {
            if (wsd.capables[i].id != data.id) { continue; }
            wsd.capables[i] = data;
            found = true;
            break;
        }
        if (!found) { wsd.capables.Add(data); }
        done_ids.Add(capable.ID);

        if (save_capacities)
        {
            List<Capacity> capacities = capable.GetStaticCapacities();
            s_log_capables?.LogExtended($"Saving {capacities.Count} capacities of capable '{capable.ID}'");
            foreach (Capacity capa in capacities)
            {
                s_log_capables?.Log($"Saving capacity '{capa.ID}' in world '{wsd.ID}'");
                CapacityData cdata = capa.GetStaticData();
                found = false;
                for (int i = 0; i < wsd.capacities.Count; i++)
                {
                    if (wsd.capacities[i].id != cdata.id) { continue; }
                    wsd.capacities[i] = cdata;
                    found = true;
                    break;
                }
                if (!found) { wsd.capacities.Add(cdata); }
            }
        }

        if (save_inventory && capable.Inventory != null)
        {
            List<Item> items = capable.Inventory.GetStaticItems();
            s_log_capables?.LogExtended($"Saving {items.Count} items of capable '{capable.ID}'");
            foreach (Item item in items) { save_static_capable_to_wsd(item, wsd, ref done_ids, save_inventory, save_capacities); }
        }
    }



    ///
    //
    /// LOADING SAVES
    //
    ///

    private static WorldSaveData _loaded_save = null; // only one big world save data loaded at a time
    public static WorldSaveData LoadWorldSaveFromJson(string json, bool log=true)
    {
        if (string.IsNullOrEmpty(json))
        {
            if (log) { Debug.LogWarning($"(SaveEngine - LoadWorldSaveFromJson) json is null or empty: {json}"); }
            return null;
        }
        _loaded_save = JsonConvert.DeserializeObject<WorldSaveData>(json, one_file_settings);
        return _loaded_save;
    }
    public static WorldSaveData GetWorldSave(string world_id)
    {        
        if (string.IsNullOrEmpty(world_id)) { Debug.LogWarning($"(SaveEngine) Invalid world_id : '{world_id}'"); return null; }

        // Debug.Log($"(SaveEngine) Loading world save data for world '{world_id}' from folder or file..., save is null: {save == null}");
        if (_loaded_save != null && _loaded_save.world.id == world_id) { return _loaded_save; } // we already have one loaded, we return it
        _loaded_save = new WorldSaveData();

        // then we have no save load
        // Debug.Log($"(SaveEngine) Loading world save data for world '{world_id}' from folder or file..., save is null: {save == null}");

        AppManager.EnsureFolderExists(WorldManager.WorldsDataPath);
        if (load_world_save_from_folder(world_id, ref _loaded_save)) { return _loaded_save; }

        // here we have no folder for the world, we try to load it from the single file save
        if (load_world_save_from_file(world_id, ref _loaded_save)) { return _loaded_save; }

        // if we reach this point, we failed to load the world save data
        Debug.LogError($"(SaveEngine) Failed to load world save data for world '{world_id}' from both folder and file.");
        return null;
    }
    public static void ClearWorldSave() { _loaded_save = null; }
    private static bool load_world_save_from_file(string id, ref WorldSaveData save, bool log = true)
    {
        // string world_file_path = Path.Combine(WorldManager.WorldsDataPath, id + ".json");

        // get the json
        string json = AppManager.LoadJsonFromWorldFolder(id, "save");
        if (string.IsNullOrEmpty(json))
        {
            if (log) { Debug.LogWarning($"(SaveEngine - Load World Save) World Save Data file not found: {id}.json"); }
            return false;
        }
        save = JsonConvert.DeserializeObject<WorldSaveData>(json, one_file_settings);
        save.world.id = id; // ensure the world id is set correctly
        return true;
    }
    private static bool load_world_save_from_folder(string id, ref WorldSaveData save, bool log = true)
    {
        string world_path = Path.Combine(WorldManager.WorldsDataPath, id);

        // check if the current world folder exists in the worlds folder.
        if (!Directory.Exists(world_path))
        {
            if (log) { Debug.LogWarning($"(World) World folder not found: {world_path}"); }
            return false;
        }

        // now we load the world save data from the folder structure
        string json = "";
        
        // first, world data
        json = AppManager.LoadJsonFromWorldFolder(id, "world_data.json");
        if (!string.IsNullOrEmpty(json)) { save.world = JsonUtility.FromJson<WorldData>(json); }
        else
        {
            if (log) { Debug.LogWarning($"(World) World data file not found: {Path.Combine(world_path, "world_data.json")}"); }
            return false;
        }

        // controller data
        json = AppManager.LoadJsonFromWorldFolder(id, "controller.json");
        if (!string.IsNullOrEmpty(json)) { save.controller = JsonUtility.FromJson<ControllerData>(json); }
        else
        {
            if (log) { Debug.LogWarning($"(World) Controller data file not found: {Path.Combine(world_path, "controller.json")}"); }
            return false;
        }

        // levels data
        string[] jsons = AppManager.LoadJsonsFromWorldFolder(id, "levels");
        if (jsons != null && jsons.Length > 0)
        {
            foreach (string level_json in jsons)
            {
                LevelData level_data = JsonUtility.FromJson<LevelData>(level_json);
                save.levels.Add(level_data);
            }
        }
        else if (log) { Debug.LogWarning($"(World) No levels data files found in folder: {Path.Combine(world_path, "levels")}"); }

        // rooms data
        jsons = AppManager.LoadJsonsFromWorldFolder(id, "rooms");
        if (jsons != null && jsons.Length > 0)
        {
            foreach (string room_json in jsons)
            {
                RoomData room_data = JsonUtility.FromJson<RoomData>(room_json);
                save.rooms.Add(room_data);
            }
        }
        else if (log) { Debug.LogWarning($"(World) No rooms data files found in folder: {Path.Combine(world_path, "rooms")}"); }

        // chunks data
        jsons = AppManager.LoadJsonsFromWorldFolder(id, "chunks");
        if (jsons != null && jsons.Length > 0)
        {
            foreach (string chunk_json in jsons)
            {
                ChunkData chunk_data = JsonUtility.FromJson<ChunkData>(chunk_json);
                save.chunks.Add(chunk_data);
            }
        }
        else if (log) { Debug.LogWarning($"(World) No chunks data files found in folder: {Path.Combine(world_path, "chunks")}"); }

        // capables data
        jsons = AppManager.LoadJsonsFromWorldFolder(id, "capables");
        if (jsons != null && jsons.Length > 0)
        {
            foreach (string capable_json in jsons)
            {
                CapableData capable_data = LoadCapableDataWithGoodKind(capable_json);
                save.capables.Add(capable_data);
            }
        }
        else if (log) { Debug.LogWarning($"(World) No capables data files found in folder: {Path.Combine(world_path, "capables")}"); }

        // capacities data
        jsons = AppManager.LoadJsonsFromWorldFolder(id, "capacities");
        if (jsons != null && jsons.Length > 0)
        {
            foreach (string capacity_json in jsons)
            {
                CapacityData capacity_data = LoadCapacityDataWithGoodKind(capacity_json);
                save.capacities.Add(capacity_data);
            }
        }
        else if (log) { Debug.LogWarning($"(World) No capacities data files found in folder: {Path.Combine(world_path, "capacities")}"); }

        // if we reach this point, our save is complete and we return true
        save.world.id = id; // ensure the world id is set correctly
        return true;
    }

    public static CapableData LoadCapableDataWithGoodKind(string json)
    {
        // gather the kind of the capacity from the json
        CapableData tmp_data = JsonUtility.FromJson<CapableData>(json);
        string kind = tmp_data.kind;
        if (_log_capable_data_kind_checking) { Debug.Log($"(SaveEngine) Loading capable data '{tmp_data.id}' with kind '{kind}'"); }

        // first we check if we have a data for this precise kind
        Type data_type = Type.GetType(kind + "Data");
        if (data_type != null)
        {
            if (_log_capable_data_kind_checking) { Debug.Log($"(SaveEngine) Found precise data type for capable kind '{kind}' : {data_type}"); }
            return JsonUtility.FromJson(json, data_type) as CapableData;
        }

        // we found no precise data type ://
        // we check if we have an intermediary type
        // ex : ItemData, DoorData
        // (insert in the list below)
        Type capable_type = Type.GetType(kind);
        if (_log_capable_data_kind_checking) { Debug.Log($"(SaveEngine) No precise data type for capable kind '{kind}' was found. Checking for intermediary types..."); }

        // PersoData
        if (GameManager.IsKind(capable_type, typeof(Perso))) { data_type = typeof(PersoData); }

        // ItemData
        else if (GameManager.IsKind(capable_type, typeof(Food))) { data_type = typeof(FoodData); }

        // ItemData
        else if (GameManager.IsKind(capable_type, typeof(Item))) { data_type = typeof(ItemData); }

        // IAData
        else if (GameManager.IsKind(capable_type, typeof(IA))) { data_type = typeof(IAData); }

        // DoorData
        else if (GameManager.IsKind(capable_type, typeof(Door))) { data_type = typeof(DoorData); }

        // no intermediary type -> we give a CapableData, basic
        else { data_type = typeof(CapableData); }

        if (_log_capable_data_kind_checking) { Debug.Log($"(SaveEngine) Final data type for capable kind '{kind}' is '{data_type}'"); }

        // we finally extract the data
        return JsonUtility.FromJson(json, data_type) as CapableData;
    }
    public static CapacityData LoadCapacityDataWithGoodKind(string json)
    {
        // gather the kind of the capacity from the json
        string kind = JsonUtility.FromJson<CapacityData>(json).kind;
        Type type = Type.GetType(kind + "Data");
        if (type == null) { type = Type.GetType(kind.Replace("Capacity", "Data")); }
        if (type == null) { type = typeof(CapacityData); }
        return JsonUtility.FromJson(json, type) as CapacityData;
    }

    private static Dictionary<string, WorldDataHelper> _world_data_helpers = new Dictionary<string, WorldDataHelper>();
    public static WorldDataHelper GetWorldData(string world_id)
    {
        if (string.IsNullOrEmpty(world_id)) { Debug.LogWarning($"(SaveEngine) Invalid world_id : '{world_id}'"); return null; }

        if (_world_data_helpers.TryGetValue(world_id, out WorldDataHelper w_data)) { return w_data; }
        w_data = new WorldDataHelper();

        // then we have no cached world data, we try to load it from the world folder or file        
        AppManager.EnsureFolderExists(WorldManager.WorldsDataPath);
        if (load_world_data_helper_from_folder(world_id, ref w_data)) { return w_data; }

        // here we have no folder for the world, we try to load it from the single file save
        if (load_world_data_helper_from_file(world_id, ref w_data)) { return w_data; }

        // if we reach this point, we failed to load the world save data
        Debug.LogWarning($"(SaveEngine) Failed to load world save data for world '{world_id}' from both folder and file.");
        return null;
    }
    private static bool load_world_data_helper_from_file(string id, ref WorldDataHelper helper, bool log = true)
    {
        // get the json
        string json = AppManager.LoadJsonFromWorldFolder(id, "save");
        if (string.IsNullOrEmpty(json))
        {
            if (log) { Debug.LogWarning($"(SaveEngine - Load World Data Helper) World Data Helper file not found: {id}.json"); }
            return false;
        }
        helper = JsonConvert.DeserializeObject<WorldDataHelper>(json, one_file_settings);
        helper.is_one_file = true;
        return true;
    }
    private static bool load_world_data_helper_from_folder(string id, ref WorldDataHelper helper, bool log = true)
    {
        // load the world data
        string json = AppManager.LoadJsonFromWorldFolder(id, "world_data.json");
        if (string.IsNullOrEmpty(json)) { return false; }
        WorldData data = JsonUtility.FromJson<WorldData>(json);

        // and controller
        json = AppManager.LoadJsonFromWorldFolder(id, "controller.json");
        if (string.IsNullOrEmpty(json)) { return false; }
        ControllerData controller_data = JsonUtility.FromJson<ControllerData>(json);

        // if we reach this point, our save is complete and we return true
        helper = new WorldDataHelper
        {
            world = data,
            controller = controller_data,
            is_one_file = false
        };
        return true;
    }



    ///
    //
    /// SAVING DATA TO DISK
    //
    ///

    public static void SaveWorldSaveData(WorldSaveData data)
    {
        if (LazyInstance.save_as_one_file) { SaveWorldSaveDataAsFile(data); }
        else { SaveWorldSaveDataAsFolder(data); }
    }
    public static void SaveWorldSaveDataAsFile(WorldSaveData data)
    {
        // saves the world save data to a single file in the worlds/world_id/save path
        WorldManager.EnsureWorldDataFolderExists(data.world.id);

        // create file
        bool just_created = !WorldManager.DoesWorldSaveDataExists(data.world.id);
        data.world.UpdateTime(just_created);

        // prepare for serializing into json, we want to keep to
        // good casting for everything (SofaData =/= CapableData)
        // so we use a specific json contract
        slog?.Log($"Converting WORLD SAVE data to json (single file)... using custom json contract to keep the good casting for all data types");
        string json = "";
        try
        {
            json = JsonConvert.SerializeObject(data, one_file_settings);
            slog?.Log($"Serialized {json.Length} chars");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        // and we save the world save data to a single json file
        slog?.Log($"Saving WORLD SAVE data (single file) to path: {Path.Combine(WorldManager.GetWorldDataPath(data.world.id), "save")}\n{json}");
        AppManager.SaveJsonToWorldFolder(data.world.id, "save", json, log_static);
    }
    public static void SaveWorldSaveDataAsFolder(WorldSaveData data)
    {
        // splits the save into small json data files into an unique world folder
        SaveWorldDataInFolder(data.world);
        SaveControllerDataInFolder(data.controller, data.world.id);

        foreach (LevelData level_data in data.levels) { SaveLevelDataInFolder(level_data, data.world.id); }
        foreach (RoomData room_data in data.rooms) { SaveRoomDataInFolder(room_data, data.world.id); }
        foreach (ChunkData chunk_data in data.chunks) { SaveChunkDataInFolder(chunk_data, data.world.id); }
        foreach (CapableData capable_data in data.capables) { SaveCapableDataInFolder(capable_data, data.world.id); }
        foreach (CapacityData capacity_data in data.capacities) { SaveCapacityDataInFolder(capacity_data, data.world.id); }
    }

    /// <important>
    /// all the following methods are used ONLY for saving data files into a FOLDER structure.
    /// if you need to save a specific data into a FILE structure, you need to :
    /// - (1.) load the world save data from the file
    /// - (2.) update the specific data in the world save data
    /// - (3.) save the world save data back to the file
    /// this should NOT be done using the methods below. please do it in a higher level method
    /// </important>

    public static void SaveWorldDataInFolder(WorldData data)
    {
        bool just_created = WorldManager.EnsureWorldDataHierarchy(data.id);
        data.UpdateTime(just_created);

        // save the current WorldData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = "world_data.json";
        s_log_rooms?.Log($"Saving WORLD data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(data.id, path, json, log_static);
    }
    public static void SaveControllerDataInFolder(ControllerData data, string world_id)
    {
        // updating controller with the player level/room/chunk before saving
        // data.UpdatePlayerLevelRoomChunk(slog);

        // save the current ControllerData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = "controller.json";
        s_log_rooms?.Log($"Saving CONTROLLER data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveLevelDataInFolder(LevelData data, string world_id)
    {
        // save the current LevelData to a json file in the folder structure
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("levels", data.id + ".json");
        s_log_rooms?.Log($"Saving LEVEL data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
        return;
    }
    public static void SaveRoomDataInFolder(RoomData data, string world_id)
    {
        // save the current RoomData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("rooms", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving ROOM data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveChunkDataInFolder(ChunkData data, string world_id)
    {
        // save the current ChunkData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("chunks", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CHUNK data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveCapableDataInFolder(CapableData data, string world_id)
    {
        // save the current CapableData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("capables", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CAPABLE data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveCapacityDataInFolder(CapacityData data, string world_id)
    {
        // save the current CapacityData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("capacities", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CAPACITY data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }


    ///
    //
    /// CLEANING SAVE FILES
    //
    ///

    /// <summary>
    /// delete all the rooms/chunks/capables/capacities save files that are not in any levels.
    /// careful, it does not make any backup, so make sure you know what you are doing
    /// </summary>
    /// <param name="world_id"></param>
    public static void CleanWorldSave(string world_id)
    {
        bool folder = !AppManager.IsSaveASingleFile(world_id);
        slog_clean?.Log($"Cleaning save for world '{world_id}' (save is a {(folder ? "folder structure" : "single file")})");
        if (folder) { clean_world_save_in_folder(world_id); return; }

        try
        {
            clean_world_save_in_file(world_id);
        }
        catch (Exception ex)
        {
            Debug.LogError($"(SaveEngine) Exception while cleaning world save for world '{world_id}': {ex.Message}\n{ex.StackTrace}");
        }
    }
    private static void clean_world_save_in_file(string world_id)
    {
        ClearWorldSave(); // we potentially just saved things, so we clear so the wsd is up to date

        // we load the world save data
        WorldSaveData wsd = GetWorldSave(world_id);
        if (wsd == null)
        {
            slog_clean?.Warning($"Failed to load world save data for world '{world_id}' while trying to clean save. Aborting.");
            return;
        }

        // we get the controller id
        string controller_id = wsd.controller != null ? wsd.controller.controlled_capable_id : null;

        // levels
        if (wsd.levels == null || wsd.levels.Count == 0)
        {
            slog_clean?.Warning($"No levels found in world '{world_id}' while trying to clean save. Aborting.");
            return;
        }

        slog_clean?.Log($"Cleaning save for world '{world_id}' (file structure)... Found {wsd.levels.Count} levels in the world. Gathering all rooms/chunks/capables/capacities in these levels");

        // rooms
        HashSet<string> rooms_in_levels = new HashSet<string>();
        foreach (LevelData level_data in wsd.levels)
        {
            foreach (string room_id in level_data.rooms_ids) { rooms_in_levels.Add(room_id); }
        }
        List<RoomData> rooms_data = wsd.rooms.Where(r => rooms_in_levels.Contains(r.id)).ToList();
        slog_clean?.LogExtended($"Found {rooms_data.Count} rooms in the levels of the world.");

        // chunks
        HashSet<string> chunks_in_levels = new HashSet<string>();
        foreach (RoomData room_data in rooms_data)
        {
            foreach (string chunk_id in room_data.chunks_ids) { chunks_in_levels.Add(chunk_id); }
        }
        List<ChunkData> chunks_data = wsd.chunks.Where(c => chunks_in_levels.Contains(c.id)).ToList();
        slog_clean?.LogExtended($"Found {chunks_data.Count} chunks in the levels of the world.");

        // capables
        List<string> capables_in_levels = new List<string>();
        foreach (ChunkData chunk_data in chunks_data)
        {
            foreach (string capable_id in chunk_data.capables_ids) { capables_in_levels.Add(capable_id); }
            foreach (string movable_id in chunk_data.movables_ids) { capables_in_levels.Add(movable_id); }
        }

        // check if the controlled capable is not here we add it 
        bool controlled_capable_missing = false;
        if (!string.IsNullOrEmpty(controller_id) && !capables_in_levels.Contains(controller_id))
        {
            slog_clean?.Warning($"Controlled capable '{controller_id}' is not in the list of capables in the levels of the world. Adding it to the list to avoid deleting it.");
            capables_in_levels.Add(controller_id);
            controlled_capable_missing = true;
        }

        // then we gather all CapableData
        List<CapableData> capables_data = gather_all_capables_from_wsd_that_are_listed(wsd, capables_in_levels, out List<string> capacities_in_levels);
        if (controlled_capable_missing) // and reset position if needed
        {
            CapableData controlled_capable = capables_data.FirstOrDefault(c => c.id == controller_id);
            if (controlled_capable != null)
            {
                controlled_capable.position = Vector2.zero;
                slog_clean?.Warning($"Reset position of controlled capable '{controller_id}' to 0,0 instead of deleting it.");
            }
        }
        capables_in_levels = capables_data.Select(c => c.id).ToList();


        // then we can gather all CapacityData
        List<CapacityData> capacities_data = wsd.capacities.Where(c => capacities_in_levels.Contains(c.id)).ToList();
        slog_clean?.LogExtended($"Found {capables_data.Count} capables and {capacities_in_levels.Count} capacities in the levels of the world (including inventories).");
        slog_clean?.Log($"World has {rooms_in_levels.Count} rooms, {chunks_in_levels.Count} chunks, {capables_in_levels.Count} capables and {capacities_in_levels.Count} capacities in its levels. Now deleting all save files that are not in these lists...");


        List<RoomData> rooms_to_delete = new List<RoomData>();
        List<ChunkData> chunks_to_delete = new List<ChunkData>();
        List<CapableData> capables_to_delete = new List<CapableData>();
        List<CapacityData> capacities_to_delete = new List<CapacityData>();
        if (slog_clean.Verbose >= Verbosity.Normal)
        {
            // now we go through the lists to log which ones are going to be deleted.
            rooms_to_delete = wsd.rooms.Where(r => !rooms_in_levels.Contains(r.id)).ToList();
            chunks_to_delete = wsd.chunks.Where(c => !chunks_in_levels.Contains(c.id)).ToList();
            capables_to_delete = wsd.capables.Where(c => !capables_in_levels.Contains(c.id)).ToList();
            capacities_to_delete = wsd.capacities.Where(c => !capacities_in_levels.Contains(c.id)).ToList();
            foreach (RoomData room in rooms_to_delete) { slog_clean?.LogExtended($"Room '{room.id}' will be deleted from the world save data."); }
            foreach (ChunkData chunk in chunks_to_delete) { slog_clean?.LogExtended($"Chunk '{chunk.id}' will be deleted from the world save data."); }
            foreach (CapableData capable in capables_to_delete) { slog_clean?.LogExtended($"Capable '{capable.id}' will be deleted from the world save data."); }
            foreach (CapacityData capacity in capacities_to_delete) { slog_clean?.LogExtended($"Capacity '{capacity.id}' will be deleted from the world save data."); }
        }

        // now we simply replace the lists in the world save data with the filtered ones
        wsd.rooms = rooms_data;
        wsd.chunks = chunks_data;
        wsd.capables = capables_data;
        wsd.capacities = capacities_data;

        // and we save the world save data back to the file
        SaveWorldSaveDataAsFile(wsd);
        slog_clean?.Log($"Finished cleaning save for world '{world_id}' (single file)." + $" DELETED :    {rooms_to_delete.Count} rooms    ///    {chunks_to_delete.Count} chunks    ///    {capables_to_delete.Count} capables    ///    {capacities_to_delete.Count} capacities.");
    }
    private static void clean_world_save_in_folder(string world_id)
    {
        // we get the controller id
        ControllerData controller_data = Controller.LoadWorldControllerData(world_id);
        string controller_id = controller_data != null ? controller_data.controlled_capable_id : null;

        // we load all the levels of the world
        List<LevelData> levels_data = LevelEngine.LoadWorldLevelsData(world_id);
        if (levels_data == null || levels_data.Count == 0)
        {
            slog_clean?.Warning($"No levels found in world '{world_id}' while trying to clean save. Aborting.");
            return;
        }

        slog_clean?.Log($"Cleaning save for world '{world_id}' (folder structure)... Found {levels_data.Count} levels in the world. Gathering all rooms/chunks/capables/capacities in these levels");

        // gather all the rooms
        HashSet<string> rooms_in_levels = new HashSet<string>();
        foreach (LevelData level_data in levels_data)
        {
            foreach (string room_id in level_data.rooms_ids) { rooms_in_levels.Add(room_id); }
        }
        List<RoomData> rooms_data = RoomEngine.LoadWorldRoomsData(world_id, rooms_in_levels.ToList());
        slog_clean?.LogExtended($"Found {rooms_data.Count} rooms in the levels of the world.");
        // gather all the chunks
        HashSet<string> chunks_in_levels = new HashSet<string>();
        foreach (RoomData room_data in rooms_data)
        {
            foreach (string chunk_id in room_data.chunks_ids) { chunks_in_levels.Add(chunk_id); }
        }
        List<ChunkData> chunks_data = ChunkEngine.LoadWorldChunksData(world_id, chunks_in_levels.ToList());
        slog_clean?.LogExtended($"Found {chunks_data.Count} chunks in the levels of the world.");
        // gather all the capables
        HashSet<string> capables_in_levels = new HashSet<string>();
        foreach (ChunkData chunk_data in chunks_data)
        {
            foreach (string capable_id in chunk_data.capables_ids) { capables_in_levels.Add(capable_id); }
            foreach (string movable_id in chunk_data.movables_ids) { capables_in_levels.Add(movable_id); }
        }
        List<CapableData> capables_data = CapableEngine.LoadWorldCapablesData(world_id, capables_in_levels.ToList());
        // gather the capacities + add all their inventories' items too
        HashSet<string> capacities_in_levels = new HashSet<string>();
        foreach (CapableData capable_data in capables_data)
        {
            gather_all_capable_and_capacities_in_capable_recursive(world_id, capable_data, ref capables_in_levels, ref capacities_in_levels);
        }
        slog_clean?.LogExtended($"Found {capables_data.Count} capables and {capacities_in_levels.Count} capacities in the levels of the world (including inventories).");
        slog_clean?.Log($"World has {rooms_in_levels.Count} rooms, {chunks_in_levels.Count} chunks, {capables_in_levels.Count} capables and {capacities_in_levels.Count} capacities in its levels. Now deleting all save files that are not in these lists...");

        // now, we have all the rooms/chunks/capables/capacities that are in the levels, we can delete all the ones that are not in these lists
        string[] rooms_paths = AppManager.GetFilesPathsInWorldFolder(world_id, "rooms");
        string[] chunks_paths = AppManager.GetFilesPathsInWorldFolder(world_id, "chunks");
        string[] capables_paths = AppManager.GetFilesPathsInWorldFolder(world_id, "capables");
        string[] capacities_paths = AppManager.GetFilesPathsInWorldFolder(world_id, "capacities");
        int deleted_rooms = 0;
        int deleted_chunks = 0;
        int deleted_capables = 0;
        int deleted_capacities = 0;

        foreach (string path in rooms_paths)
        {
            string file_name = Path.GetFileNameWithoutExtension(path);
            if (rooms_in_levels.Contains(file_name)) { continue; }
            AppManager.DeleteFile(path, log_static);
            deleted_rooms++;
            slog_clean?.LogExtended($"Deleted '{file_name}' room save file: {path}");
        }
        foreach (string path in chunks_paths)
        {
            string file_name = Path.GetFileNameWithoutExtension(path);
            if (chunks_in_levels.Contains(file_name)) { continue; }
            AppManager.DeleteFile(path, log_static);
            deleted_chunks++;
            slog_clean?.LogExtended($"Deleted '{file_name}' chunk save file: {path}");
        }
        List<string> controlled_capacities_ids = new List<string>();
        foreach (string path in capables_paths)
        {
            string file_name = Path.GetFileNameWithoutExtension(path);
            if (capables_in_levels.Contains(file_name)) { continue; }

            // verify that we are not deleting the controlled capable, and if yes reset its position to 0,0
            if (!string.IsNullOrEmpty(controller_id) && file_name == controller_id)
            {
                string json = AppManager.ReadFile(path, log_static);
                CapableData data = JsonUtility.FromJson<CapableData>(json);
                if (data != null)
                {
                    // reset position
                    data.position = Vector2.zero;
                    string new_json = JsonUtility.ToJson(data, true);
                    SaveCapableDataInFolder(data, world_id);
                    slog_clean?.Warning($"Reset position of controlled capable '{file_name}' to 0,0 instead of deleting it.");

                    // gather its capacities to not delete them
                    if (data.capacities_ids != null) { controlled_capacities_ids.AddRange(data.capacities_ids); }
                    continue;
                }
            }

            AppManager.DeleteFile(path, log_static);
            deleted_capables++;
            slog_clean?.LogExtended($"Deleted '{file_name}' capable save file: {path}");
        }
        foreach (string path in capacities_paths)
        {
            string file_name = Path.GetFileNameWithoutExtension(path);
            if (capacities_in_levels.Contains(file_name)) { continue; }
            if (controlled_capacities_ids.Contains(file_name)) { continue; }
            AppManager.DeleteFile(path, log_static);
            deleted_capacities++;
            slog_clean?.LogExtended($"Deleted '{file_name}' capacity save file: {path}");
        }

        slog_clean?.Log($"Finished cleaning save for world '{world_id}' (folder structure). DELETED : {deleted_rooms} rooms, {deleted_chunks} chunks, {deleted_capables} capables and {deleted_capacities} capacities.");

    }
    
    // ! does it work with containers ??? i don't think so
    // todo : make this work with containes as well
    private static void gather_all_capable_and_capacities_in_capable_recursive(string world_id, CapableData capable_data, ref HashSet<string> items_ids, ref HashSet<string> capacities_ids)
    {
        // we take the opportunity to gather the capacities ids too
        if (capable_data.capacities_ids != null)
        {
            foreach (string capa_id in capable_data.capacities_ids)
            {
                if (!capacities_ids.Contains(capa_id)) { capacities_ids.Add(capa_id); }
            }
        }

        // and we recursively gather the items in the inventory of the capable
        if (capable_data.inventory == null) { return; }
        List<string> inv_items_ids = capable_data.inventory.GetAllItemsIds();
        items_ids.UnionWith(inv_items_ids);

        // and we gather the items in the inventories of the items in the inventory, and so on recursively
        foreach (string item_id in inv_items_ids)
        {
            CapableData item_data = CapableEngine.LoadWorldCapableData(world_id, item_id);
            if (item_data == null) { continue; }
            gather_all_capable_and_capacities_in_capable_recursive(world_id, item_data, ref items_ids, ref capacities_ids);
        }
    }


    // this method works well, should become the norm overall
    private static List<CapableData> gather_all_capables_from_wsd_that_are_listed(WorldSaveData wsd, List<string> capables_ids, out List<string> capacities_ids)
    {
        string tmp_log = "gathering capables for save cleaning...\n";
        capacities_ids = new List<string>();

        // capables
        // we need to do a while loop until the capables_ids list is real empty
        // bcz there can be capable in a capable in a capable in a capable etc etc etc
        // so the max_iterations of the while loop is the highest intrication depth of
        // capables in the world, but we limit it to 10 just in case (should never happen)
        int iterations = 0;
        int total_capables = 0;
        List<CapableData> gathered_capables = new List<CapableData>();
        List<string> added_capables_ids = new List<string>();
        while (capables_ids.Count > 0 && iterations < 10)
        {
            iterations++;
            tmp_log = "\n";
            List<CapableData> capables_data = get_listed_capables_data_in_wsd(wsd, capables_ids);
            capables_ids.Clear();
            for (int i = 0; i < capables_data.Count; i++)
            {
                if (added_capables_ids.Contains(capables_data[i].id)) { continue; }
                added_capables_ids.Add(capables_data[i].id);
                gathered_capables.Add(capables_data[i]);

                slog_clean?.LogOMGThatsVeryVerySpecific($"- '{capables_data[i].id}' -------------- {(capables_data[i].capacities_ids != null ? capables_data[i].capacities_ids.Count : 0)} capacities  /  {(capables_data[i].inventory != null ? capables_data[i].inventory.ItemsCount() : 0)} items\n");
                if (capables_data[i].capacities_ids != null) { capacities_ids.AddRange(capables_data[i].capacities_ids); }
                if (capables_data[i].inventory != null) { capables_ids.AddRange(capables_data[i].inventory.GetAllItemsIds()); }

                // also check for containers
                if (capables_data[i] is ContainerData container_data && container_data.contained_capable_ids != null)
                {
                    capables_ids.AddRange(container_data.contained_capable_ids);
                }
            }
            slog_clean?.LogVerySpecific($"Gathered {capables_data.Count} capables ----- iteration {iterations} :{tmp_log}");
            total_capables += capables_data.Count;
        }
        slog_clean?.LogSpecific($"Gathered total {total_capables} capables in {iterations} iterations.");

        return gathered_capables;
    }
    private static List<CapableData> get_listed_capables_data_in_wsd(WorldSaveData wsd, List<string> capables_ids)
    {
        List<CapableData> gathered_capables = new List<CapableData>();
        foreach (string capable_id in capables_ids)
        {
            CapableData capable_data = wsd.capables.FirstOrDefault(c => c.id == capable_id);
            if (capable_data != null) { gathered_capables.Add(capable_data); }
        }
        return gathered_capables;
    }

}