using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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


    [Header("Logs")]
    [SerializeField] private Loggable<SaveEngine> log;
    private static Loggable<SaveEngine> slog => LazyInstance != null ? LazyInstance.log : null;
    private static bool log_static => LazyInstance != null && LazyInstance.log.Verbose >= Verbosity.Extended;
    [SerializeField] private Loggable<SaveEngine> log_capa;
    private static Loggable<SaveEngine> s_log_capa => LazyInstance != null ? LazyInstance.log_capa : null;
    [SerializeField] private Loggable<SaveEngine> log_rooms;
    private static Loggable<SaveEngine> s_log_rooms => LazyInstance != null ? LazyInstance.log_rooms : null;
    [SerializeField] private Loggable<SaveEngine> log_clean;
    private static Loggable<SaveEngine> slog_clean => LazyInstance != null ? LazyInstance.log_clean : null;


    [Header("GetStaticData Logs")]
    public Loggable<Capable> log_gsd_capable;
    // public Loggable<Capable> log_gsd_capacity;


    ///
    //
    /// SAVING STATIC OBJECTS
    //
    ///

    public static void SaveAIOLevel(Level level, string world_id, bool save_rooms = true, bool save_capables = true)
    {
        // get the level data
        slog?.Log($"Saving AIO level '{level.ID}' of world '{world_id}' (save_rooms: {save_rooms}, save_capables: {save_capables})");
        LevelData data = level.GetStaticData();
        SaveLevelData(data, world_id);

        // check if we need to save the rooms also
        if (save_rooms)
        {
            List<Room> rooms = level.GetStaticRooms().ToList();
            slog?.Log($"Saving {rooms.Count} rooms of level '{level.ID}'");
            foreach (Room room in rooms)
            {
                SaveRoom(room, world_id);
                List<Chunk> chunks = room.GetStaticChunks().ToList();
                slog?.Log($"Saving {chunks.Count} chunks of room '{room.ID}'");
                foreach (Chunk chunk in chunks) { SaveChunk(chunk, world_id); }
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
                SaveCapable(cap, world_id);
            }
        }

        slog?.Log($"Finished saving AIO level '{level.ID}' of world '{world_id}'");
    }
    public static void SaveRoom(Room room, string world_id)
    {
        s_log_rooms?.Log($"Saving room '{room.ID}' in world '{world_id}'");
        RoomData data = room.GetStaticData();
        SaveRoomData(data, world_id);
    }
    public static void SaveChunk(Chunk chunk, string world_id)
    {
        s_log_rooms?.Log($"Saving chunk '{chunk.ID}' in world '{world_id}'");
        ChunkData data = chunk.GetStaticData();
        SaveChunkData(data, world_id);
    }
    public static void SaveCapable(Capable capable, string world_id, bool save_inventory = true, bool save_capacities = true)
    {
        s_log_rooms?.Log($"Saving capable '{capable.ID}' in world '{world_id}' (save_inventory: {save_inventory}, save_capacities: {save_capacities})");

        s_log_rooms?.LogSpecific($"Getting capable data '{capable.ID}'");
        CapableData data = (CapableData)capable.GetStaticData();

        s_log_rooms?.LogSpecific($"Saving capable data for '{capable.ID}'");
        s_log_rooms?.LogSpecific($"Capable data is {(data != null ? "not null" : "null")} \n{(data != null ? JsonUtility.ToJson(data, true) : "")}");
        SaveCapableData(data, world_id);

        if (save_capacities)
        {
            List<Capacity> capacities = capable.GetStaticCapacities();
            s_log_rooms?.LogExtended($"Saving {capacities.Count} capacities of capable '{capable.ID}'");
            foreach (Capacity capa in capacities) { SaveCapacity(capa, world_id); }
        }

        if (save_inventory && capable.Inventory != null)
        {
            List<Item> items = capable.Inventory.GetStaticItems();
            s_log_rooms?.LogExtended($"Saving {items.Count} items of capable '{capable.ID}'");
            foreach (Item item in items) { SaveCapable(item, world_id, save_inventory, save_capacities); }
        }
    }
    public static void SaveCapacity(Capacity capacity, string world_id)
    {
        s_log_rooms?.Log($"Saving capacity '{capacity.ID}' in world '{world_id}'");
        CapacityData data = capacity.GetStaticData();
        SaveCapacityData(data, world_id);
    }




    ///
    //
    /// SAVING DATA TO FILES
    //
    ///

    public static void SaveWorldData(WorldData data)
    {
        bool just_created = WorldManager.EnsureWorldDataHierarchy(data.id);

        data.game_version = Application.version;
        data.last_update_date = DateTime.Now.ToString();
        data.creation_date = (just_created || string.IsNullOrEmpty(data.creation_date)) ? data.last_update_date : data.creation_date;

        // check if we just created it or we don't have any icon, then we set random color and default icon
        if (just_created || string.IsNullOrEmpty(data.icon_path))
        {
            data.color = WorldManager.LazyInstance.GetRandomWorldColor();
            data.icon_path = WorldManager.LazyInstance.GetRandomIconPath(out string icon_name);
            data.icon_name = icon_name;
        }

        // save the current WorldData to a json file
        string json = JsonUtility.ToJson(data, true);
        AppManager.SaveJsonToWorldFolder(data.id, "world_data.json", json, log_static);
    }
    public static void SaveControllerData(ControllerData data, string world_id)
    {
        // save the current ControllerData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = "controller.json";
        s_log_rooms?.Log($"Saving CONTROLLER data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveLevelData(LevelData data, string world_id)
    {
        // save the current LevelData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("levels", data.id + ".json");
        s_log_rooms?.Log($"Saving LEVEL data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveRoomData(RoomData data, string world_id)
    {
        // save the current RoomData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("rooms", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving ROOM data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveChunkData(ChunkData data, string world_id)
    {
        // save the current ChunkData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("chunks", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CHUNK data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveCapableData(CapableData data, string world_id)
    {
        // save the current CapableData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("capables", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CAPABLE data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
    public static void SaveCapacityData(CapacityData data, string world_id)
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

        // we load all the levels of the world
        List<LevelData> levels_data = LevelEngine.LoadWorldLevelsData(world_id);
        if (levels_data == null || levels_data.Count == 0)
        {
            slog_clean?.Warning($"No levels found in world '{world_id}' while trying to clean save. Aborting.");
            return;
        }

        slog_clean?.Log($"Cleaning save for world '{world_id}'... Found {levels_data.Count} levels in the world. Gathering all rooms/chunks/capables/capacities in these levels");

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
        foreach (string path in capables_paths)
        {
            string file_name = Path.GetFileNameWithoutExtension(path);
            if (capables_in_levels.Contains(file_name)) { continue; }
            AppManager.DeleteFile(path, log_static);
            deleted_capables++;
            slog_clean?.LogExtended($"Deleted '{file_name}' capable save file: {path}");
        }
        foreach (string path in capacities_paths)
        {
            string file_name = Path.GetFileNameWithoutExtension(path);
            if (capacities_in_levels.Contains(file_name)) { continue; }
            AppManager.DeleteFile(path, log_static);
            deleted_capacities++;
            slog_clean?.LogExtended($"Deleted '{file_name}' capacity save file: {path}");
        }
    
        slog_clean?.Log($"Finished cleaning save for world '{world_id}'. Deleted {deleted_rooms} rooms, {deleted_chunks} chunks, {deleted_capables} capables and {deleted_capacities} capacities.");
    }
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
}