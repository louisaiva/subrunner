using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AIO_Loader : MonoBehaviour
{
    [SerializeField] private AIO_Status _status = AIO_Status.NotWorking;
    public static AIO_Status Status
    {
        get
        {
            if (SaveEngine.AIO_Loader == null) { return AIO_Status.NotWorking; }
            return SaveEngine.AIO_Loader._status;
        }
    }

    [Header("Prefabs & Parents")]
    [SerializeField] private Transform level_parent;
    [SerializeField] private Level level_prefab;
    [SerializeField] private Chunk chunk_prefab;
    [SerializeField] private Room room_prefab;

    [Header("Neighbours nodes")]
    public bool add_roomgraph_neighbour_node = true;
    public RoomNodeEditor neighbour_node_prefab;

    [Header("RTO Loaded AIO Levels")]
    private Dictionary<string, Level> loaded_levels = new Dictionary<string, Level>(); // key is level_id, value is the loaded level

    [Header("Logs")]
    // [SerializeField] private bool log = false;
    [SerializeField] private bool hide_no_level_warning = false;
    public Loggable<AIO_Loader> log_loading;

    public async Task<Level> LoadAIO_Level_NoWorldLoaded(string world_id, string level_id, bool navmesh_only = false)
    {
        log_loading.Log($"(AIO_Loader) Loading level '{level_id}' for world '{world_id}' (No World Loaded)");

        // CLEAR OLD DATA
        await ClearCache_NoWorldLoaded();
        _status = AIO_Status.LoadingNoWorldLoaded;

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

        log_loading.LogExtended($"Loading Rooms & Chunks datas into the engines from world + level");
        List<RoomData> rooms_data = RoomEngine.Instance.LoadRoomsData(world_id, level_data.rooms_ids);

        // load the rooms
        List<Chunk> chunks = load_rooms_and_chunks(rooms_data, aio_level.transform, navmesh_only : navmesh_only);
        if (chunks.Count == 0) { log_loading.Warning($"No chunks loaded for level '{level_id}' in world '{world_id}'"); }

        return aio_level;
    }
    public Level LoadAIO_Level(string world_id, string level_id, bool navmesh_only = false)
    {

        log_loading.Log($"Loading level '{level_id}' for world '{world_id}' (World Loaded)");
        // if (log) { Debug.Log($"(AIO_Loader) Loading level '{level_id}' for world '{world_id}' (World Loaded)"); }

        // CLEAR OLD DATA
        _ = ClearCache();

        _status = AIO_Status.LoadingWorldLoaded;

        // grab the level data from the world data
        LevelData level_data = LevelEngine.LoadWorldLevelData(world_id, level_id);
        if (level_data == null)
        {
            log_loading.Warning($"No level data found for level '{level_id}' in world '{world_id}'");
            // if (!hide_no_level_warning) { Debug.LogWarning($"(AIO_Loader) No level data found for level '{level_id}' in world '{world_id}'"); }
            return null;
        }

        // load the level
        log_loading.LogExtended($"Loading level");
        Level aio_level = load_level(level_data);
        log_loading.LogExtended($"Level loaded with success !");

        // BECAUSE we are in world loaded mode, rooms & capables & capacities data
        // should already be loaded so we don't load them again

        // load the rooms
        log_loading.LogExtended($"Getting rooms data from level data");
        List<RoomData> rooms_data = RoomEngine.Instance.GetRoomsDataFromIDs(level_data.rooms_ids);
        log_loading.LogExtended($"{rooms_data.Count} rooms data found, loading rooms");
        List<Chunk> chunks = load_rooms_and_chunks(rooms_data, aio_level.transform, navmesh_only);
        if (chunks.Count == 0) { log_loading.Warning($"No chunks loaded for level '{level_id}' in world '{world_id}'"); }
        log_loading.LogExtended($"Level & Rooms & Chunks loaded with success !");

        return aio_level;
    }

    // low level level loading
    private Level load_level(LevelData data)
    {
        // we instanciate a new level and assign the data to it
        Level new_level = Instantiate(level_prefab, level_parent);
        new_level.data = data;
        new_level.gameObject.name = data.id;
        log_loading.Log($"(AIO_Loader) Level '{new_level.ID}' loaded");
        loaded_levels[data.id] = new_level;
        return new_level;
    }

    // low level rooms loading
    private HashSet<string> loaded_rooms = new HashSet<string>();
    private List<Chunk> load_rooms_and_chunks(List<RoomData> rooms_data, Transform rooms_parent, bool navmesh_only = false)
    {
        // grab the room data from the world data
        List<Chunk> chunks = new List<Chunk>();
        foreach (RoomData data in rooms_data)
        {
            // load the room
            log_loading.LogSpecific($"Loading room '{data.id}'");
            Room room = load_room(data, rooms_parent, ref chunks, navmesh_only : navmesh_only);
            if (room == null)
            {
                log_loading.Error($"Failed to load room with id '{data.id}'");
                continue;
            }
            log_loading.LogSpecific($"Room '{data.id}' loaded successfully");
        }


        return chunks;
    }
    private Room load_room(RoomData data, Transform level_transform, ref List<Chunk> loaded_chunks, bool navmesh_only = false)
    {
        // we instanciate a new room and assign the data to it
        Room new_room = Instantiate(room_prefab, level_transform);

        // load the data
        new_room.data = data;
        new_room.gameObject.name = data.id;
        log_loading.LogVerySpecific($"Room '{data.id}' basic data loaded");

        // load the lights
        log_loading.LogVerySpecific($"Loading lights");
        ChunkEngine.Instance.LightsEngine.LoadLights_AIO(data.lights_data, data.id, new_room.LightsParent);

        // build the tilemaps
        log_loading.LogVerySpecific($"Loading its tilemaps");
        RoomEngine.Instance.TilemapEngine.BuildTilemapsForAIO_Room(new_room);

        // and get the chunks of the room
        List<ChunkData> chunks_data = ChunkEngine.GetOrLoadChunksDataFromIDs(data.chunks_ids); // this method is a weird one bcz it works with both loaded engine & non loaded engine.
        foreach (ChunkData cdata in chunks_data)
        {
            // load the chunk
            log_loading.LogVerySpecific($"Loading chunk '{cdata.id}'");
            Chunk chunk = load_chunk(cdata, new_room.transform);
            if (chunk == null)
            {
                log_loading.Error($"Failed to load chunk with id '{cdata.id}'");
                continue;
            }
            log_loading.LogVerySpecific($"Chunk '{cdata.id}' loaded successfully");
            loaded_chunks.Add(chunk);

            log_loading.LogVerySpecific($"Now Loading its capables");
            load_capables(cdata.capables_ids, level_transform.Find("Capables"));
            if (navmesh_only) { continue; } // we don't load movables if we are in navmesh only mode
            log_loading.LogVerySpecific($"Now Loading its movables");
            load_capables(cdata.movables_ids, level_transform.Find("Movables"));
        }

        log_loading.LogVerySpecific($"Room '{data.id}' loaded successfully with its tilemaps !");
        loaded_rooms.Add(data.id);
        return new_room;
    }
    private Chunk load_chunk(ChunkData data, Transform parent)
    {
        // we instanciate a new room and assign the data to it
        Chunk new_chunk = Instantiate(chunk_prefab, parent);

        // load the data
        new_chunk.data = data;
        new_chunk.gameObject.name = data.id;
        new_chunk.transform.position = data.position;

        // load the colliders in the composite collider
        new_chunk.ChunkCollider.SetPath(0, data.collider_points.ToArray());
        new_chunk.ChunkCollider.enabled = true;

        log_loading.LogVerySpecific($"Chunk '{data.id}' basic data loaded + colliders");

        // if add_roomgraph_neighbour_node is true, we add a node for each neighbour of the room in the roomgraph
        log_loading.LogVerySpecific(add_roomgraph_neighbour_node, $"Adding neighbour node");
        if (add_roomgraph_neighbour_node) { Instantiate(neighbour_node_prefab, new_chunk.transform); }

        log_loading.LogVerySpecific($"Chunk '{data.id}' loaded successfully with its tilemaps and lights !");
        loaded_rooms.Add(data.id);
        return new_chunk;
    }


    // low level capables loading
    private HashSet<string> loaded_capables = new HashSet<string>();
    private void load_capables(List<string> capables_ids, Transform parent)
    {
        foreach (string id in capables_ids)
        {
            if (loaded_capables.Contains(id)) { continue; }
            if (Controller.Capable != null && id == Controller.Capable.ID)
            {
                log_loading.Warning($"Capable with id '{id}' is the currently controlled capable, skipping it to avoid conflicts");
                continue;
            }

            // check if we are building (level builder) and this is a door, we don't load it, so the LevelTranslator will
            // recreate a door
            if (GameManager.State == GameState.Building)
            {
                Type type = CapableEngine.Instance.GetCapableType(id);
                if (type != null && GameManager.IsKind(type, typeof(Door)))
                {
                    log_loading.LogExtended($"Capable with id '{id}' is a door and we are in LevelBuilder, skipping it");
                    continue;
                }
            }

            Capable capable = CapableEngine.Instance.LoadCapableInstantly(id);
            if (capable == null)
            {
                log_loading.Warning($"Capable with id '{id}' not found, skipping it");
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
        _status = AIO_Status.ClearingCache;

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

        _status = AIO_Status.NotWorking;
    }

    /// <summary>
    /// this method is used to clear the cache of this class.
    /// This is NOT clearing the capable & capacity engines cache, nor their subsystems cache.
    /// Useful when a world is loaded or loading and you don't want to clear everything.
    /// </summary>
    public async Task ClearCache()
    {
        _status = AIO_Status.ClearingCache;

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

        _status = AIO_Status.NotWorking;
    }
}

public enum AIO_Status
{
    NotWorking,
    LoadingWorldLoaded,
    LoadingNoWorldLoaded,
    ClearingCache,
}