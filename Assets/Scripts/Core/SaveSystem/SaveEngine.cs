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
    [SerializeField] private Loggable<SaveEngine> log_capa;
    private static Loggable<SaveEngine> s_log_capa => LazyInstance != null ? LazyInstance.log_capa : null;
    [SerializeField] private Loggable<SaveEngine> log_rooms;
    private static Loggable<SaveEngine> s_log_rooms => LazyInstance != null ? LazyInstance.log_rooms : null;
    private static bool log_static => LazyInstance != null && LazyInstance.log.Verbose >= Verbosity.Extended;


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
    /// SAVING DATA METHODS
    //
    ///


    // SAVE WORLD DATA
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

    // SAVE LEVEL DATA
    public static void SaveLevelData(LevelData data, string world_id)
    {
        // save the current LevelData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("levels", data.id + ".json");
        s_log_rooms?.Log($"Saving LEVEL data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }

    // SAVE ROOM DATA
    public static void SaveRoomData(RoomData data, string world_id)
    {
        // save the current RoomData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("rooms", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving ROOM data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }

    // SAVE CHUNK DATA
    public static void SaveChunkData(ChunkData data, string world_id)
    {
        // save the current ChunkData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("chunks", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CHUNK data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }

    // SAVE CAPABLE DATA
    public static void SaveCapableData(CapableData data, string world_id)
    {
        // save the current CapableData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("capables", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CAPABLE data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }

    // SAVE CAPACITY DATA
    public static void SaveCapacityData(CapacityData data, string world_id)
    {
        // save the current CapacityData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine("capacities", data.id + ".json");
        s_log_rooms?.LogVerySpecific($"Saving CAPACITY data to path: {path}\n{json}");
        AppManager.SaveJsonToWorldFolder(world_id, path, json, log_static);
    }
}