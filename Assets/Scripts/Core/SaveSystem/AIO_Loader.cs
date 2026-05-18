using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AIO_Loader : MonoBehaviour
{
    
    [Header("Prefabs & Parents")]
    [SerializeField] private Transform level_parent;
    [SerializeField] private Level level_prefab;
    [SerializeField] private Room room_prefab;

    [Header("Neighbours nodes")]
    public bool add_roomgraph_neighbour_node = true;
    public RoomNodeEditor neighbour_node_prefab;

    [Header("RTO Loaded AIO Levels")]
    private Dictionary<string, Level> loaded_levels = new Dictionary<string, Level>(); // key is level_id, value is the loaded level

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool hide_no_level_warning = false;

    public async Task<Level> LoadAIO_Level_NoWorldLoaded(string world_id, string level_id, bool navmesh_only = false)
    {
        if (log) { Debug.Log($"(AIO_Loader) Loading level '{level_id}' for world '{world_id}' (No World Loaded)"); }

        // CLEAR OLD DATA
        await ClearCache_NoWorldLoaded();

        // grab the level data from the world data
        LevelData level_data = LevelEngine.LoadWorldLevelData(world_id, level_id);
        if (level_data == null)
        {
            if (!hide_no_level_warning) { Debug.LogWarning($"(AIO_Loader) No level data found for level '{level_id}' in world '{world_id}'"); }
            return null;
        }

        // load the level
        Level aio_level = load_level(level_data);


        // BEFORE loading the rooms, we need to load the capables data & capacity data,
        // otherwise the CapableEngine & CapacityEngine won't be able to 
        // load the room's capables & their capacities
        CapableEngine.Instance.LoadWorldCapablesData(world_id);
        CapacityEngine.Instance.LoadWorldCapacitiesData(world_id);
        List<RoomData> rooms_data = RoomEngine.LoadRoomsData(world_id, level_data.rooms_ids);

        // load the rooms
        List<Room> rooms = load_rooms(rooms_data, aio_level.transform, navmesh_only : navmesh_only);
        if (rooms.Count == 0) { Debug.LogWarning($"(AIO_Loader) No rooms loaded for level '{level_id}' in world '{world_id}'"); }

        return aio_level;
    }
    public Level LoadAIO_Level(string world_id, string level_id, bool navmesh_only = false)
    {
        if (log) { Debug.Log($"(AIO_Loader) Loading level '{level_id}' for world '{world_id}' (World Loaded)"); }

        // CLEAR OLD DATA
        ClearCache();

        // grab the level data from the world data
        LevelData level_data = LevelEngine.LoadWorldLevelData(world_id, level_id);
        if (level_data == null)
        {
            if (!hide_no_level_warning) { Debug.LogWarning($"(AIO_Loader) No level data found for level '{level_id}' in world '{world_id}'"); }
            return null;
        }

        // load the level
        Level aio_level = load_level(level_data);

        // BECAUSE we are in world loaded mode, rooms & capables & capacities data
        // should already be loaded so we don't load them again

        // load the rooms
        List<Room> rooms = load_rooms(RoomEngine.Instance.GetRoomsDataFromIDs(level_data.rooms_ids), aio_level.transform, navmesh_only);
        if (rooms.Count == 0) { Debug.LogWarning($"(AIO_Loader) No rooms loaded for level '{level_id}' in world '{world_id}'"); }

        return aio_level;
    }

    // low level level loading
    private Level load_level(LevelData data)
    {
        // we instanciate a new level and assign the data to it
        Level new_level = Instantiate(level_prefab, level_parent);
        new_level.data = data;
        new_level.gameObject.name = data.id;
        if (log) { Debug.Log($"(AIO_Loader) Level '{new_level.ID}' loaded"); }
        loaded_levels[data.id] = new_level;
        return new_level;
    }

    // low level rooms loading
    private HashSet<string> loaded_rooms = new HashSet<string>();
    private List<Room> load_rooms(List<RoomData> rooms_data, Transform rooms_parent, bool navmesh_only = false)
    {
        // grab the room data from the world data
        List<Room> rooms = new List<Room>();
        foreach (RoomData data in rooms_data)
        {
            // load the room
            Room room = load_room(data, rooms_parent);
            if (room == null) { continue; }
            rooms.Add(room);

            load_capables(data.capables_ids, rooms_parent.Find("Capables"));
            if (navmesh_only) { continue; } // we don't load movables if we are in navmesh only mode
            load_capables(data.movables_ids, rooms_parent.Find("Movables"));
        }


        return rooms;
    }
    private Room load_room(RoomData data, Transform parent)
    {
        // we instanciate a new room and assign the data to it
        Room new_room = Instantiate(room_prefab, parent);

        // load the data
        new_room.data = data;
        new_room.gameObject.name = data.id;
        new_room.transform.position = data.position;

        // load the colliders in the composite collider
        new_room.RoomCollider.SetPath(0, data.collider_points.ToArray());
        new_room.RoomCollider.enabled = true;

        // build the tilemaps
        RoomEngine.Instance.TilemapEngine.BuildTilemapsForAIO_Room(new_room);

        // load the lights
        RoomEngine.Instance.LightsEngine.LoadLights_AIO(data.lights_data, data.id, new_room.LightsParent);

        // if add_roomgraph_neighbour_node is true, we add a node for each neighbour of the room in the roomgraph
        if (add_roomgraph_neighbour_node) { Instantiate(neighbour_node_prefab, new_room.transform); }

        loaded_rooms.Add(data.id);
        return new_room;
    }


    // low level capables loading
    private HashSet<string> loaded_capables = new HashSet<string>();
    private void load_capables(List<string> capables_ids, Transform parent)
    {
        foreach (string id in capables_ids)
        {
            if (loaded_capables.Contains(id)) { continue; }
            Capable capable = CapableEngine.Instance.LoadCapableInstantly(id);
            if (capable == null)
            {
                Debug.LogWarning($"(AIO_Loader) Capable with id '{id}' not found, skipping it");
                continue;
            }
            loaded_capables.Add(id);
            capable.AnimPlayer.Show();
            capable.transform.SetParent(parent);
        }
    }

    // CLEAR CACHE

    /// <summary>
    /// this method is used to clear the cache when no world is loaded.
    /// If you already have a world loaded / a world loading, please use the
    /// world loaded equivalent
    /// </summary>
    /// <returns></returns>
    public async Task ClearCache_NoWorldLoaded()
    {
        // we clear the loaded levels
        foreach (Level level in loaded_levels.Values)
        {
            Destroy(level.gameObject);
        }
        loaded_levels.Clear();
        loaded_capables.Clear();
        loaded_rooms.Clear();

        // we clear the capable & capacity engines cache
        await CapableEngine.LazyInstance.UnloadWorldData(log: true);
        await CapacityEngine.LazyInstance.UnloadWorldData(log: true);

        // we clear the subsystems
        // RoomEngine.Instance.TilemapEngine.ClearTilemaps(log: true);
        // RoomEngine.Instance.LightsEngine.ClearLights(log: true);
    }

    /// <summary>
    /// this method is used to clear the cache of this class.
    /// This is NOT clearing the capable & capacity engines cache, nor their subsystems cache.
    /// Useful when a world is loaded or loading and you don't want to clear everything.
    /// </summary>
    public async Task ClearCache()
    {
        // we ask capable engine to unload all loaded capables, which will pool objects & capacities
        for (int i = 0; i < loaded_capables.Count; i++)
        {
            string capable_id = loaded_capables.ElementAt(i);
            CapableEngine.Instance.UnloadCapableInstantly(capable_id);
        }

        // we clear the loaded levels
        foreach (Level level in loaded_levels.Values)
        {
            Destroy(level.gameObject);
        }
        loaded_levels.Clear();
        loaded_capables.Clear();
        loaded_rooms.Clear();

        await Task.Yield();
    }
}