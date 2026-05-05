using System;
using System.Collections.Generic;
using System.IO;
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
            _static_loader = LazyInstance.transform.GetComponentInChildren<AIO_Loader>(includeInactive: true);
            if (_static_loader == null) { Debug.LogError("(SaveEngine) SaveEngine.AIO_Loader was not found"); }
            return _static_loader;
        }
    }


    [Header("Logs")]
    [SerializeField] private bool log = false;
    private static bool log_static => LazyInstance != null && LazyInstance.log;



    ///
    //
    /// SAVING OBJECTS -- MAIN ENTRY POINTS
    /// 
    ///

    public static void SaveAIOLevel(Level level, string world_id, bool save_rooms = true, bool save_capables = true)
    {
        // get the level data
        LevelData data = level.GetStaticData();
        SaveLevelData(data, world_id);

        // check if we need to save the rooms also
        if (save_rooms)
        {
            foreach (Room room in level.GetStaticRooms()) { SaveRoom(room, world_id); }
        }

        // check if we need to save the capables also
        if (save_capables)
        {
            foreach (Capable cap in level.GetStaticCapables()) { SaveCapable(cap, world_id); }
        }
    }
    public static void SaveRoom(Room room, string world_id)
    {
        RoomData data = room.GetStaticData();
        SaveRoomData(data, world_id);
    }
    public static void SaveCapable(Capable capable, string world_id, bool save_inventory = true, bool save_capacities = true)
    {
        CapableData data = (CapableData)capable.GetStaticData();
        SaveCapableData(data, world_id);

        if (save_capacities)
        {
            foreach (Capacity capa in capable.GetStaticCapacities()) { SaveCapacity(capa, world_id); }
        }

        if (save_inventory)
        {
            foreach (Item item in capable.Inventory.GetStaticItems()) { SaveCapable(item, world_id, save_inventory, save_capacities); }
        }
    }
    public static void SaveCapacity(Capacity capacity, string world_id)
    {
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
        AppManager.SaveJsonToWorldFolder(world_id, Path.Combine("levels", data.id + ".json"), json, log_static);
    }

    // SAVE ROOM DATA
    public static void SaveRoomData(RoomData data, string world_id)
    {
        // save the current RoomData to a json file
        string json = JsonUtility.ToJson(data, true);
        AppManager.SaveJsonToWorldFolder(world_id, Path.Combine("rooms", data.id + ".json"), json, log_static);
    }

    // SAVE CAPABLE DATA
    public static void SaveCapableData(CapableData data, string world_id)
    {
        // save the current CapableData to a json file
        string json = JsonUtility.ToJson(data, true);
        AppManager.SaveJsonToWorldFolder(world_id, Path.Combine("capables", data.id + ".json"), json, log_static);
    }

    // SAVE CAPACITY DATA
    public static void SaveCapacityData(CapacityData data, string world_id)
    {
        // save the current CapacityData to a json file
        string json = JsonUtility.ToJson(data, true);
        AppManager.SaveJsonToWorldFolder(world_id, Path.Combine("capacities", data.id + ".json"), json, log_static);
    }
}