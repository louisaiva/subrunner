#pragma warning disable 1998

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

public class ChunkEngine : BSOD_System<ChunkEngine>
{

    // SUB SYSTEMS
    private LightsEngine _lights_engine;
    public LightsEngine LightsEngine
    {
        get
        {
            if (_lights_engine == null) { _lights_engine = GetComponentInChildren<LightsEngine>(includeInactive: true); }
            return _lights_engine;
        }
    }

    [SerializeField] private ChunkLoader loader = new ChunkLoader();
    public ChunkLoader Loader { get => loader; }



    // CHUNKS DATA

    [Header("Rooms data")]
    public Dictionary<string, ChunkData> chunks_data = new Dictionary<string, ChunkData>();
    private Dictionary<string, int> chunks_hashs_by_ids = new Dictionary<string, int>();
    private Dictionary<int, string> chunks_ids_by_hash = new Dictionary<int, string>();
    private int next_room_hash = 1;
    // public Dictionary<string, ChunkData> loaded_chunks_data = new Dictionary<string, ChunkData>();
    public ChunkData PlayerChunkData; // the main room is the one where the perso is, we need to keep track of it to know which room to load when the perso changes room
    public Action<ChunkData> OnPlayerChunkChange = delegate { };


    // CAPABLES PER CHUNKS

    private Dictionary<string, string> chunkByCapableID = new Dictionary<string, string>(); // we keep track of the room of the STATIC CAPABLES (NOT MOVABLES)
    private Dictionary<string, string> chunkByMovableID = new Dictionary<string, string>(); // we keep track of the room of the MOVABLES
    private Dictionary<string, HashSet<string>> chunkByCapableIDs = new Dictionary<string, HashSet<string>>(); // we keep track of all capables in each room (capables + movables)
    private Dictionary<string, int> dirtyCapablesIDs = new Dictionary<string, int>(); // capables that don't have any room assigned / just changed rooms, waiting for new assignment. the int is a priority flag
    private Dictionary<string, Dictionary<string, ScoreBiasState>> chunk_score_biases = new Dictionary<string, Dictionary<string, ScoreBiasState>>();
    public Action<string, ChunkData> OnCapableAddedToRoom = delegate { };
    public Action<string, ChunkData> OnCapableRemovedFromRoom = delegate { };

    // CAPABLES ATTACHING
    private Dictionary<string, float> capables_attach_times = new Dictionary<string, float>();
    private float no_trigger_after_attach_duration = 0.08f;


    // PARAMETERS & LOGS


    [Header("Tick parameters")]
    public int frames_between_ticks = 1;
    public int dirty_capables_handled_per_tick = 10;
    private bool ticking = false;

    [Header("Loading parameters")]
    public int ticks_between_loading_chunks = 2;
    [SerializeField, Range(1, 10)] public int chunk_distance = 1;
    public void SetChunkDistance(int distance)
    {
        chunk_distance = distance;
        if (log_chunk_distance) { Debug.Log($"(ChunkEngine) Set chunk distance to {chunk_distance}"); }
    }
    public void SetChunkDistance(Setting setting) => SetChunkDistance((int) setting.Value);

    [Header("Logs awakening")]
    public bool log_awake_data = false;
    // public bool log_init = false;

    /* [Header("Logs loading")]
    public bool log_loading = false;
    public bool hide_already_loaded = false;
    public bool hide_data_not_found = false; */

    [Header("Log ticks")]
    public bool log_ticks = false;
    public bool log_chunk_transfers = false;
    public bool log_loaded_area_transfers = false;
    public bool log_best_match_calcul = false;
    public bool log_spatial_queries = false;
    public bool log_neighbours = false;
    public bool hide_log_outsider_created = false;

    [Header("Logs spawning")]
    public bool log_dynamic_room_assignement = false;
    public bool hide_log_no_room_of_capable_found = false;

    [Header("Specific logs")]
    public bool log_chunk_distance = false;

    [Header("Room Logs")]
    public bool log_tilemaps_loading = false;
    public bool log_colliders = false;
    public bool log_enter_exit = false;

    ///
    //
    /// 1. AWAKE & DATA LOADING + 2D SPATIAL CELL CHUNKS
    //
    ///
    private void Start()
    {
        // on register certains callbacks directement
        SettingsManager.Instance.RegisterCallback("chunk_distance", SetChunkDistance);
    }
    public override void OnDestroy()
    {
        // we unregister the callback
        SettingsManager.Instance.UnregisterCallback("chunk_distance", SetChunkDistance);

        base.OnDestroy();
    }


    // LOAD / UNLOAD WORLD DATA
    public override async Task LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(ChunkEngine) Loading world data for world_id: {world_id}"); }

        // load chunks data
        loadChunksData(world_id);
        if (log) { Debug.Log($"(ChunkEngine) Loaded {chunks_data.Count} chunks data"); }


        // DoorEngine.initorsomething() <-- we don't do this since door engine need the doors data to be loaded -> means it is CapableEngine that calls it

        // start ticking
        ticking = true;

        await Task.Yield();

        // we register to CapableSystem.OnCapableAppear so we can assign rooms to the new capable
        CapableEngine.Instance.OnCapableAppear += AttachCapable;
        CapableEngine.Instance.OnCapableDisappear += FreeCapable;
        if (log) { Debug.Log($"(ChunkEngine) Registered to CapableSystem events"); }

        // we generate the spatial maps for the levels
        generateLevels2DSpatialCells();
        if (log) { Debug.Log($"(ChunkEngine) Generated 2D spatial maps for levels : \n  -{string.Join("\n  -", spatial_maps_by_level_id.Keys)}"); }
        if (log) { Debug.Log($"(ChunkEngine) CHUNK ENGINE SUCCESSFULLY LOADED : {world_id}"); }
    }
    public override async Task UnloadWorldData(bool log)
    {
        // we stop ticking
        ticking = false;

        // we remove all the events
        CapableEngine.Instance.OnCapableAppear -= AttachCapable;
        CapableEngine.Instance.OnCapableDisappear -= FreeCapable;

        // clear the spatial maps
        spatial_maps_by_level_id.Clear();

        // we destroy all the chunks gameobjects
        loader.Clear();
        ChunkBank.Instance.DestroyAllChunksInstantly();

        // clear all the rooms data
        chunks_data.Clear();
        chunks_hashs_by_ids.Clear();
        chunks_ids_by_hash.Clear();
        next_room_hash = 1;

        // and the rooms data per capables
        chunkByCapableID.Clear();
        chunkByMovableID.Clear();
        chunkByCapableIDs.Clear();
        dirtyCapablesIDs.Clear();
        chunk_score_biases.Clear();
        capables_attach_times.Clear();

        // clear sub systems caches
        // TilemapEngine.ClearTilemaps(log);
        LightsEngine.ClearLights(log);
        // await DoorEngine.UnloadWorldData(log);

        if (log) { Debug.Log($"(ChunkEngine) CHUNK ENGINE SUCCESSFULLY UNLOADED"); }
    }

    // LOAD / UNLOAD DATA
    protected void loadChunksData(string world_id)
    {
        // we empty the rooms_data and runtime ids
        chunks_data = new Dictionary<string, ChunkData>();
        chunks_hashs_by_ids = new Dictionary<string, int>();
        chunks_ids_by_hash = new Dictionary<int, string>();
        next_room_hash = 1;
        string log_rooms_details = "\n\n";

        // we load all the json files in the data path and convert them to RoomData objects
        string[] files = AppManager.LoadJsonsFromWorldFolder(world_id, "chunks");
        foreach (string file in files)
        {
            ChunkData data = JsonUtility.FromJson<ChunkData>(file);
            chunks_data.Add(data.id, data);
            generate_runtime_chunk_id(data.id);
            log_rooms_details += data.GetDetails() + "\n";

            // we call addCapableToRoom for each capable in the room to set the capable-room links in the RoomEngine
            foreach (string capable_id in data.capables_ids) { addCapableToChunk(capable_id, data, false); }
            foreach (string movable_id in data.movables_ids) { addCapableToChunk(movable_id, data, true); }
        }

        if (log_awake_data) { Debug.Log("(ChunkEngine) CHUNKS DATA LOADED : " + chunks_data.Count + log_rooms_details); }
        // awake_done = true;
    }
    private int generate_runtime_chunk_id(string id)
    {
        if (string.IsNullOrEmpty(id)) { return 0; }
        if (chunks_hashs_by_ids.TryGetValue(id, out int existing)) { return existing; }

        int new_hash = next_room_hash++;
        chunks_hashs_by_ids[id] = new_hash;
        chunks_ids_by_hash[new_hash] = id;
        return new_hash;
    }


    // 2D SPATIAL CELL CHUNKS
    private Dictionary<string, LevelSpatialMap2D> spatial_maps_by_level_id = new Dictionary<string, LevelSpatialMap2D>(); // ? should be on LevelEngine ???
    protected void generateLevels2DSpatialCells()
    {
        spatial_maps_by_level_id.Clear();

        // get all levels from the LevelEngine
        Level[] levels = LevelEngine.Instance.GetWorldLevels();
        foreach (Level level in levels)
        {
            // we create a spatial map for the level
            string level_id = level.data.id;
            spatial_maps_by_level_id[level_id] = new LevelSpatialMap2D();
            LevelSpatialMap2D spatial_map = spatial_maps_by_level_id[level_id];

            // get the rooms of the level
            List<ChunkData> rooms = LevelEngine.Instance.GetChunksDataOfLevel(level_id);
            spatial_map.ComputeSpatialMap(rooms);
        }
    }
    protected ChunkData GetChunkAtPosition(Vector2 world_position, string level_id=null)
    {
        // if level_id is null then we get the current level id from the LevelEngine
        if (string.IsNullOrEmpty(level_id)) { level_id = LevelEngine.Instance.CurrentLevelID; }
        if (string.IsNullOrEmpty(level_id)) { return null; }

        // we check if we have a spatial map for the level
        if (!spatial_maps_by_level_id.TryGetValue(level_id, out LevelSpatialMap2D spatial_map))
        {
            if (log_spatial_queries) { Debug.LogWarning("(ChunkEngine - GetRoomAtPosition) No spatial map found for level id: " + level_id); }
            return null;
        }

        // we query the spatial map for the rooms at the position
        string room_id = spatial_map.GetRoomAtPosition(world_position);
        if (string.IsNullOrEmpty(room_id)) { return null; }
        if (!chunks_data.TryGetValue(room_id, out ChunkData room)) { return null; }
        return room;
    }
    protected bool IsPositionInsideRoom(Vector2 world_position, string room_id, string level_id = null)
    {
        // if level_id is null then we get the current level id from the LevelEngine
        if (string.IsNullOrEmpty(level_id)) { level_id = LevelEngine.Instance.CurrentLevelID; }

        // we check if we have a spatial map for the level
        if (!spatial_maps_by_level_id.TryGetValue(level_id, out LevelSpatialMap2D spatial_map))
        {
            if (log_spatial_queries) { Debug.LogWarning("(ChunkEngine - IsPositionInsideRoom) No spatial map found for level id: " + level_id); }
            return false;
        }

        // we query the spatial map for the rooms at the position
        return spatial_map.IsPositionInsideRoom(world_position, room_id);
    }
    protected LevelSpatialMap2D CurrentSpatialMap
    {
        get
        {
            string level_id = LevelEngine.Instance.CurrentLevelID;
            if (string.IsNullOrEmpty(level_id)) { return null; }
            if (!spatial_maps_by_level_id.TryGetValue(level_id, out LevelSpatialMap2D spatial_map)) { return null; }
            return spatial_map;
        }
    }

    ///
    //
    /// 2. ASSIGNING CHUNKS TO CAPABLES
    //
    ///


    // ADD / REMOVE CAPABLE TO / FROM CHUNK

    /// <summary>
    /// this method adds the capable id to the room data and to the room engine dicts.
    /// No check if the capable is already in a room or if the room is loaded, so it
    /// may reproduce duplicated ownership of capables by rooms if not used carefully.
    /// Call removeCapableFromRoom before using it if your capable may already be in a room. Main
    /// method we use to add capables to rooms since it has internal dict management.
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="room"></param>
    /// <param name="is_movable"></param>
    protected void addCapableToChunk(string entity_id, ChunkData room, bool is_movable)
    {
        // we add the capable to our dictionaries
        if (!is_movable) { chunkByCapableID[entity_id] = room.id; }
        else { chunkByMovableID[entity_id] = room.id; }
        if (!chunkByCapableIDs.ContainsKey(room.id)) { chunkByCapableIDs[room.id] = new HashSet<string>(); }
        if (!chunkByCapableIDs[room.id].Contains(entity_id)) { chunkByCapableIDs[room.id].Add(entity_id); }

        // we add the capable to the room's data
        if (!is_movable && !room.capables_ids.Contains(entity_id)) { room.capables_ids.Add(entity_id); }
        else if (is_movable && !room.movables_ids.Contains(entity_id)) { room.movables_ids.Add(entity_id); }

        // we invoke the event
        OnCapableAddedToRoom?.Invoke(entity_id, room);
    }
    /// <summary>
    /// this method removes the capable id from the room data and from the room engine dicts.
    /// May free the capable from any room, and so create a RoomEngine outsider. Main
    /// method we use to free capables from rooms since it has internal dict management.
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="room"></param>
    protected void removeCapableFromChunk(string entity_id, ChunkData room)
    {
        // we remove the capable from our dictionaries
        if (chunkByCapableID.ContainsKey(entity_id)) { chunkByCapableID.Remove(entity_id); }
        if (chunkByMovableID.ContainsKey(entity_id)) { chunkByMovableID.Remove(entity_id); }
        if (chunkByCapableIDs.ContainsKey(room.id) && chunkByCapableIDs[room.id].Contains(entity_id)) { chunkByCapableIDs[room.id].Remove(entity_id); }

        // we remove the capable from the room's data
        if (room.capables_ids.Contains(entity_id)) { room.capables_ids.Remove(entity_id); }
        if (room.movables_ids.Contains(entity_id)) { room.movables_ids.Remove(entity_id); }

        // we invoke the event
        OnCapableRemovedFromRoom?.Invoke(entity_id, room);
    }




    // ON CHUNK ENTER
    public void OnChunkEnter(ChunkData room, Capable capable, int priority = 0) => OnChunkEnter(room.id, capable.data.id, priority);
    public void OnChunkEnter(string room_id, string entity_id, int priority = 0)
    {

        if (log_enter_exit) { Debug.Log($"(ChunkEngine) OnRoomEnter : {entity_id} entered {room_id} with priority {priority}"); }

        if (!dirtyCapablesIDs.ContainsKey(entity_id)) { dirtyCapablesIDs[entity_id] = priority; }
        else
        {
            int new_priority = dirtyCapablesIDs[entity_id] + priority + 1;
            dirtyCapablesIDs[entity_id] = new_priority > 1000 ? 1000 : new_priority; // we cap the priority to avoid overflow and keep it manageable
        }

        set_only_bias(entity_id, room_id, ScoreBiasType.RoomEnter); // we set the bias to RoomEnter for the room the capable just entered, and clear it for other rooms to prefer the new room for next room switch resolution

    }
    // ON CHUNK EXIT
    public void OnChunkExit(ChunkData room, Capable capable, int priority = 0) => OnChunkExit(room.id, capable.data.id, priority);
    public void OnChunkExit(string room_id, string entity_id, int priority = 0)
    {
        if (log_enter_exit) { Debug.Log($"(ChunkEngine) OnRoomExit : {entity_id} exited {room_id} with priority {priority}"); }

        if (!dirtyCapablesIDs.ContainsKey(entity_id)) { dirtyCapablesIDs[entity_id] = priority; }
        else
        {
            int new_priority = dirtyCapablesIDs[entity_id] + priority + 1;
            dirtyCapablesIDs[entity_id] = new_priority > 1000 ? 1000 : new_priority; // we cap the priority to avoid overflow and keep it manageable
        }
        set_only_bias(entity_id, room_id, ScoreBiasType.RoomExit); // we set the bias to RoomExit for the room the capable just exited, and clear it for other rooms to prefer the new room for next room switch resolution
    }
    private void set_only_bias(string entity_id, string room_id, ScoreBiasType bias)
    {
        // we add a bias if not exists
        if (!chunk_score_biases.ContainsKey(entity_id)) { chunk_score_biases[entity_id] = new Dictionary<string, ScoreBiasState>(); }

        // add a new room bias if not exists / set bias if already exists
        if (!chunk_score_biases[entity_id].ContainsKey(room_id)) { chunk_score_biases[entity_id][room_id] = new ScoreBiasState(bias); }
        else { chunk_score_biases[entity_id][room_id].Bias = bias; }

        // we clear other biases of same type to keep only one bias like this for this capable
        for (int i = 0; i < chunk_score_biases[entity_id].Keys.Count; i++)
        {
            string other_room_id = chunk_score_biases[entity_id].Keys.ElementAt(i);
            if (other_room_id == room_id) { continue; }
            ScoreBiasState other = chunk_score_biases[entity_id][other_room_id];

            // we clear other biases of same type to keep only one bias like this !
            if (other.Bias != bias) { continue; }
            chunk_score_biases[entity_id][other_room_id].Clear();
        }

    }

    // CAPABLE ATTACHING
    public void AttachCapable(CapableData capable_data)
    {
        // double check that the capable is not a grabbed item
        if (capable_data is ItemData item_data && item_data.is_grabbed)
        {
            if (this.log_dynamic_room_assignement) { Debug.Log($"(ChunkEngine) Capable {capable_data.id} is a grabbed item, we don't attach it to any room for now"); }
            return;
        }
        AttachCapable(capable_data.id);
    }
    public void AttachCapable(string id)
    {
        // basically we add the capable to the dirty list to let the tick handle its room assignment
        if (log_enter_exit) { Debug.Log($"(ChunkEngine) AttachCapable : {id} is going to be attached"); }
        dirtyCapablesIDs[id] = 1000; // highest prio for new attached capables

        // we don't set any bias bcz it just attached

        // but we save the id and time of attach to prevent the calling of room triggers
        capables_attach_times[id] = Time.time;
    }
    // CAPABLE FREEING
    public void FreeCapable(CapableData capable_data) => FreeCapable(capable_data.id);
    public void FreeCapable(string id)
    {
        if (log_enter_exit) { Debug.Log($"(ChunkEngine) FreeCapable : {id} is going to be freed"); }

        // we free the capable from any room, it may be destroyed or else
        ChunkData room = GetCapableChunk(id);
        if (room == null) { return; }
        removeCapableFromChunk(id, room);

        // we save the id and time of detach to prevent the calling of room triggers
        capables_attach_times[id] = Time.time;

        if (log_chunk_transfers) { Debug.Log($"(ChunkEngine) [{room.id}] >> {id} >> [none]         -- was freed !!"); }
        if (log_loaded_area_transfers) { Debug.Log($"(ChunkEngine) [{room.id}] >> {id} >> [none]         -- was freed !!"); }
    }
    public bool ShouldIgnoreRoomTrigger(string id)
    {
        if (!capables_attach_times.TryGetValue(id, out float attach_time)) { return false; }
        if (Time.time - attach_time > no_trigger_after_attach_duration) { return false; }
        capables_attach_times.Remove(id);
        return true;
    }


    ///
    //
    ///  3. DYNAMIC CHUNK OF CAPABLE MANAGEMENT (UPDATE)
    //
    ///


    // UPDATE
    private int frames_since_last_tick = 0;
    private int tick_since_last_loading = 0;
    protected virtual void Update()
    {
        if (!ticking) { return; }

        frames_since_last_tick++;
        if (frames_since_last_tick < frames_between_ticks) { return; }
        frames_since_last_tick = 0;

        // we tick !
        Tick();

        tick_since_last_loading++;
        if (tick_since_last_loading < ticks_between_loading_chunks) { return; }
        tick_since_last_loading = 0;

        // we run the loader, which will make it load & unload 1 chunk per time
        loader.RunTask(); 
    }

    // TICK
    private List<string> dirty_capables_ids = new List<string>();
    private List<ChunkData> chunk_candidates = new List<ChunkData>();
    private List<string> capables_to_unload = new List<string>();
    protected void Tick()
    {
        // . we get the highest priority dirty capables
        dirty_capables_ids.Clear();
        dirty_capables_ids = dirtyCapablesIDs.OrderByDescending(kv => kv.Value).Take(dirty_capables_handled_per_tick).Select(kv => kv.Key).ToList();
        if (dirty_capables_ids.Count == 0) { return; }

        string controlled_id = Controller.LazyInstance.ID;

        string log_tick = "";
        if (log_ticks) { log_tick += $"(ChunkEngine - Tick) Handling {dirty_capables_ids.Count} dirty capables : "; }

        capables_to_unload.Clear();

        // . we handle them
        foreach (string capable_id in dirty_capables_ids)
        {
            if (log_ticks) { log_tick += $"\n  - {capable_id} : \n"; }

            // we get the current room of the capable, if it has one
            ChunkData current_chunk = GetCapableChunk(capable_id);
            if (log_ticks) { log_tick += $"    - current room : {(current_chunk != null ? current_chunk.id : "none")}\n"; }

            // we get the rooms candidates for the capable
            chunk_candidates.Clear();
            if (current_chunk != null) { get_chunk_neighbours(current_chunk.id, ref chunk_candidates); }
            else { get_chunks_candidates_from_position(capable_id, ref chunk_candidates); }
            if (log_ticks) { log_tick += $"    - room candidates : {chunk_candidates.Count} ({string.Join(", ", chunk_candidates.Select(r => r.id))})\n"; }

            // check if we have candidates
            if (chunk_candidates.Count == 0)
            {
                if (!hide_log_outsider_created) { Debug.LogWarning($"(ChunkEngine - Tick) No room candidates found for capable {capable_id}, it may become an outsider of the RoomEngine."); }
                continue;
            }

            // we resolve the best room
            ChunkData best_chunk = get_best_chunk_for_capable(capable_id, chunk_candidates);
            if (log_ticks) { log_tick += $"    - best chunk : {(best_chunk != null ? best_chunk.id : "none")}\n"; }
            if (best_chunk == null) { continue; }
            if (current_chunk != null && best_chunk.id == current_chunk.id) { continue; }

            // check if perso changed room
            if (capable_id == controlled_id)
            {
                if (log_ticks) { log_tick += $"    - controlled capable changed chunk, handling it... \n"; }
                handle_perso_changed_chunk(current_chunk, best_chunk);
            }

            // we assign the capable to the best room
            string out_chunk_id = current_chunk != null ? current_chunk.id : "none";
            string in_chunk_id = best_chunk != null ? best_chunk.id : "none";
            if (log_chunk_transfers) { Debug.Log($"(ChunkEngine) [{out_chunk_id}] >> {capable_id} >> [{in_chunk_id}]"); }
            if (current_chunk != null) { removeCapableFromChunk(capable_id, current_chunk); }
            addCapableToChunk(capable_id, best_chunk, CapableEngine.Instance.IsMovable(capable_id));
            if (log_ticks) { log_tick += $"    - TRANSFERED TO NEW CHUNK !!! : {best_chunk.id}\n"; }

            // check if the new chunk is unloading (or unloaded and not loading) and if yes we need to unload the entity as well
            if (loader.WillChunkBeUnloaded(best_chunk.id))
            {
                if (log_ticks) { log_tick += $"    - new chunk is not loaded, adding entity to unload list... \n"; }
                if (log_loaded_area_transfers) { Debug.Log($"(ChunkEngine) [{out_chunk_id}] >> {capable_id} >> [{in_chunk_id}]      (quit loaded area)"); }
                capables_to_unload.Add(capable_id);
            }
        }

        // we unload the entities that need to be unloaded
        CapableEngine.Instance.UnloadCapables(capables_to_unload);

        // . we remove the handled capables from the dirty list
        foreach (string capable_id in dirty_capables_ids)
        {
            // verify that the capable has a chunk, otherwise we log an error/warning because it means the capable is an outsider of the chunk engine system D:
            if (!IsInAChunk(capable_id) && !hide_log_no_room_of_capable_found)
            {
                string log = $"(ChunkEngine - Tick) Capable {capable_id} could not be assigned to any chunk. Is now a ChunkEngine outsider.";
                bool is_capable_outsider = CapableEngine.Instance.IsOutsider(capable_id);
                if (!is_capable_outsider) { Debug.LogError(log + " (Insider of the CapableSystem, critical issue...)"); }
                else { Debug.LogWarning(log + " (Outsider of the CapableSystem as well so may be ok)"); }
            }

            // we remove the capable from the dirty list
            dirtyCapablesIDs.Remove(capable_id);
        }

        if (log_ticks) { Debug.Log(log_tick); }
    }
    
    public void StartTicking() { ticking = true; }
    public void StopTicking() { ticking = false; }

    // get rooms candidates for capable
    private void get_chunks_candidates_from_position(string capable_id, ref List<ChunkData> room_candidates)
    {
        Vector2 capable_position = CapableEngine.Instance.GetCapablePosition(capable_id);
        ChunkData position_candidate = GetChunkAtPosition(capable_position);
        if (position_candidate == null) { return; }
        get_chunk_neighbours(position_candidate.id, ref room_candidates);
    }
    private void get_chunk_neighbours(string room_id, ref List<ChunkData> room_candidates)
    {
        if (!chunks_data.ContainsKey(room_id)) { return; }
        ChunkData room = chunks_data[room_id];
        room_candidates.Add(room);
        room_candidates.AddRange(GetNeighboursData(room));
    }

    // resolve best room for capable among candidates
    private ChunkData get_best_chunk_for_capable(string capable_id, List<ChunkData> room_candidates)
    {
        if (room_candidates.Count == 0) { return null; }
        LevelSpatialMap2D spatial_map = CurrentSpatialMap;
        if (spatial_map == null) { return null; }

        string log = "";
        if (log_best_match_calcul) { log += $"(ChunkEngine - get_best_room_for_capable) Resolving best room for {capable_id} among {room_candidates.Count} candidates : \n"; }

        // keep best score
        ChunkData best_room = null;
        float best_score = float.MinValue;
        float best_distance = float.MaxValue;

        // we iterate candidates and score them
        foreach (ChunkData rdata in room_candidates)
        {
            if (log_best_match_calcul) { log += $"  - candidate room {rdata.id} : \n"; }
            float score = 0f;

            // check if the capable is inside the AABB
            Vector2 capable_position = CapableEngine.Instance.GetCapablePosition(capable_id);
            Bounds2D bounds = spatial_map.GetRoomBounds(rdata.id);

            // if inside bounds we give +100 score
            if (bounds.Contains(capable_position))
            {
                score += 100f;
                if (log_best_match_calcul) { log += $"    - inside bounds, score is now {score} \n"; }
            }


            float distance = Vector2.Distance(bounds.center, capable_position);
            score -= distance; // we prefer closer rooms
            if (log_best_match_calcul) { log += $"    - distance applied, score is now {score} \n"; }

            // we check if we have any score bias for this capable and this room
            if (chunk_score_biases.TryGetValue(capable_id, out Dictionary<string, ScoreBiasState> biases_by_room) &&
                biases_by_room.TryGetValue(rdata.id, out ScoreBiasState bias_state))
            {
                bias_state.Tick();
                if (!bias_state.IsExpired)
                {
                    score += bias_state.Score;
                    if (log_best_match_calcul) { log += $"    - bias applied, score is now {score} ({bias_state.Score} / {bias_state.Bias}) \n"; }
                }
            }

            if (score < best_score) { continue; }
            if (score > best_score) { best_score = score; best_room = rdata; best_distance = distance; continue; }

            // if tie, we check multiple things
            
            // 1. keep current capable room if tie
            ChunkData current_capable_room = GetCapableChunk(capable_id);
            if (current_capable_room != null &&
                (current_capable_room.id == rdata.id)) { best_room = current_capable_room; best_distance = distance; continue; } // we prefer the current room if tie with a neighbour
        
            // 2. prefer the smallest distance
            if (distance < best_distance) { best_room = rdata; best_distance = distance; continue; }

            // 3. continue (we keep the first one in case of tie)        
        }

        if (log_best_match_calcul) { Debug.Log(log + $"\n\n => best room is {best_room.id} with score {best_score} and distance {best_distance} \n"); }

        return best_room;
    }

    // handle perso changed room
    private void handle_perso_changed_chunk(ChunkData from_chunk, ChunkData to_chunk)
    {
        // . find rooms to load / unload based on new controlled room neighbours.
        Stack<string> chunks_to_unload = new Stack<string>();
        Stack<string> chunks_to_load = new Stack<string>();
        List<string> new_neighbours_ids = GetNeighboursIDs(to_chunk, depth: chunk_distance);

        // check which of the currently loaded rooms we need to unload
        foreach (string loaded_chunk_id in loader.LoadedChunkIDs)
        {
            if (loaded_chunk_id == to_chunk.id) { continue; }
            if (!new_neighbours_ids.Contains(loaded_chunk_id)) { chunks_to_unload.Push(loaded_chunk_id); }
        }

        // check which of the new neighbours we need to load (that are not already loaded)
        for (int i = 0; i < new_neighbours_ids.Count; i++)
        {
            string neighbour_id = new_neighbours_ids[i];
            // here we keep the to_chunk.id bcz if we tp the to_chunk may not be loaded but we want it always loaded
            if (!loader.WillChunkBeLoaded(neighbour_id)) { chunks_to_load.Push(neighbour_id); }
        }

        PlayerChunkData = to_chunk;
        OnPlayerChunkChange.Invoke(to_chunk);

        // . load the new rooms and unload old ones.
        _ = LoadChunks(chunks_to_load.ToArray());
        _ = UnloadChunks(chunks_to_unload.ToArray());
    }


    ///
    //
    ///  4. DYNAMIC CHUNK LOADING & UNLOADING
    //
    ///



    // LOAD CHUNKS
    public async Task LoadChunks(string[] rooms_ids)
    {
        ChunkLoadTask task = loader.AskLoadChunks(rooms_ids);
        while (!task.IsDone) { await Task.Yield(); }
    }

    // UNLOAD CHUNKS
    public async Task UnloadAllChunks() => await UnloadChunks(loader.LoadedChunkIDs);
    public async Task UnloadChunks(string[] rooms_ids)
    {
        ChunkUnloadTask task = loader.AskUnloadChunks(rooms_ids);
        while (!task.IsDone) { await Task.Yield(); }
    }
    /* private async Task unloadChunks(ICollection<string> rooms_ids)
    {
        foreach (string id in rooms_ids)
        {
            unload_chunk(id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loading_rooms; i++) { await Task.Yield(); }
        }
    }
    private void unload_chunk(string id)
    {
        if (!loaded_chunks_data.ContainsKey(id)) { Debug.LogWarning("(ChunkEngine - Unload) Loaded room data not found for id: " + id); return; }
        ChunkData data = loaded_chunks_data[id];
        ChunkBank.Instance.Unload(data);
        loaded_chunks_data.Remove(id);
        if (log_loading) { Debug.Log("(ChunkEngine) Unloaded " + id); }
    } */




    ///
    //
    /// 5. GETTERS & OTHERS
    //
    ///



    // NEIGHBOURS MANAGEMENT
    private List<ChunkData> neighbours_cache = new List<ChunkData>();
    public List<string> GetNeighboursIDs(ChunkData rdata, int depth = 1)
    {
        List<string> neighbours_ids = new List<string>();

        // we get the recursive neighbours
        neighbours_cache.Clear();
        get_neighbours_recursively(rdata, ref neighbours_cache, ref neighbours_ids, depth);

        if (log_neighbours)
        {
            string log = "(ChunkEngine) Neighbours of " + rdata.id + " (with depth " + depth + ") : " + neighbours_ids.Count + "\n";
            for (int i = 0; i < neighbours_ids.Count; i++)
            {
                log += " - " + neighbours_ids[i] + "\n";
            }
            Debug.Log(log);
        }
        return neighbours_ids;
    }
    /* public void get_neighbours_recursively(ChunkData room, ref List<ChunkData> neighbours, ref List<string> neighbours_ids, int depth = 1)
    {
        if (depth < 0) { return; }

        // we add the room to the neighbours list if not already in it
        if (neighbours.Contains(room)) { return; }
        neighbours.Add(room);
        neighbours_ids.Add(room.id);

        // we get the direct neighbours of the room
        List<ChunkData> direct_neighbours = GetNeighboursData(room);
        foreach (ChunkData neighbour in direct_neighbours)
        {
            // we get the neighbours of the neighbour recursively with depth - 1
            get_neighbours_recursively(neighbour, ref neighbours, ref neighbours_ids, depth - 1);
        }
    } */
    public void get_neighbours_recursively(ChunkData room, ref List<ChunkData> neighbours, ref List<string> neighbours_ids, int depth = 1)
    {
        if (depth < 1) { return; }

        // we get the direct neighbours of the room
        List<ChunkData> direct_neighbours = GetNeighboursData(room);
        for (int i = direct_neighbours.Count - 1; i >= 0; i--)
        {
            if (neighbours_ids.Contains(direct_neighbours[i].id)) { direct_neighbours.RemoveAt(i); continue; }

            // we add the neighbour to the neighbours list if not already in it
            neighbours.Add(direct_neighbours[i]);
            neighbours_ids.Add(direct_neighbours[i].id);
        }

        if (depth == 1) { return; }

        // then we make all direct neighbour grab their ones recursively with depth - 1
        foreach (ChunkData neighbour in direct_neighbours)
        {
            get_neighbours_recursively(neighbour, ref neighbours, ref neighbours_ids, depth - 1);
        }
    }
    public List<ChunkData> GetNeighboursData(ChunkData room)
    {
        List<ChunkData> neigh_datas = new List<ChunkData>();

        string log = "";
        for (int i = 0; i < room.neighbours_ids.Count; i++)
        {
            string neighbour_id = room.neighbours_ids[i];
            if (!chunks_data.ContainsKey(neighbour_id))
            {
                if (log_neighbours) { log += $"  - {neighbour_id} (data was not found)\n"; }
                continue;
            }

            if (log_neighbours) { log += $"  - {neighbour_id}\n"; }
            neigh_datas.Add(chunks_data[neighbour_id]);
        }

        if (log_neighbours) { Debug.Log($"(ChunkEngine) Neighbours of {room.id} : {neigh_datas.Count}\n{log}"); }
        return neigh_datas;
    }


    // GETTERS
    public int GetChunkHashFromID(string id)
    {
        return chunks_hashs_by_ids.TryGetValue(id, out int hash) ? hash : 0;
    }
    public string GetChunkIDFromHash(int hash)
    {
        return chunks_ids_by_hash.TryGetValue(hash, out string id) ? id : null;
    }
    private ChunkData GetCapableChunk(string capable_id)
    {
        // check if we have the capable in our dictionaries
        string room_id;
        if (chunkByCapableID.TryGetValue(capable_id, out string id_1)) { room_id = id_1; }
        else if (chunkByMovableID.TryGetValue(capable_id, out string id_2)) { room_id = id_2; }
        else { return null; }
        return chunks_data.TryGetValue(room_id, out ChunkData room) ? room : null;
    }
    public bool IsInAChunk(string capable_id)
    {
        return GetCapableChunk(capable_id) != null;
    }
    public bool TryGetCapableChunk(string capable_id, out ChunkData room)
    {
        room = GetCapableChunk(capable_id);
        return room != null;
    }
    public List<ChunkData> GetChunksDataFromIDs(ICollection<string> rooms_ids)
    {
        List<ChunkData> rooms_datas = new List<ChunkData>();
        foreach (string room_id in rooms_ids)
        {
            if (!chunks_data.ContainsKey(room_id)) { continue; }
            rooms_datas.Add(chunks_data[room_id]);
        }
        return rooms_datas;
    }
    public ChunkData GetChunkDataFromID(string room_id)
    {
        return chunks_data.TryGetValue(room_id, out ChunkData room) ? room : null;
    }
    public bool IsAnyChunkLoaded(List<string> chunks_ids)
    {
        foreach (string chunk_id in chunks_ids)
        {
            if (loader.IsChunkLoaded(chunk_id)) { return true; }
        }
        return false;
    }


    // STATIC GETTERS
    public static List<ChunkData> LoadWorldChunksData(string world_id, List<string> chunks_ids)
    {
        List<ChunkData> chunks_data = new List<ChunkData>();

        // we load all the json files in the data path and convert them to RoomData objects
        string[] files = AppManager.LoadSpecificJsonsFromWorldFolder(world_id, "chunks", chunks_ids);
        foreach (string file in files)
        {
            // if (!chunks_ids.Contains(data.id)) { continue; }
            ChunkData data = JsonUtility.FromJson<ChunkData>(file);
            chunks_data.Add(data);
        }
        return chunks_data;
    }
    public static List<ChunkData> GetOrLoadChunksDataFromIDs(List<string> chunks_ids)
    {
        // check if we have a lazy instance that has the data we need, if yes we use it to avoid loading json files
        List<ChunkData> chunks_data = new List<ChunkData>();
        if (LazyInstance != null)
        {
            bool has_all_data = true;
            foreach (string chunk_id in chunks_ids)
            {
                if (!LazyInstance.chunks_data.TryGetValue(chunk_id, out ChunkData data))
                {
                    has_all_data = false;
                    break;
                }
                chunks_data.Add(data);
            }
            if (has_all_data) { return chunks_data; }
            chunks_data.Clear(); // if we don't have all the data in the lazy instance, we clear the list to load it from json files
        }

        // we have no data :// we load it directly from the json files (try to check world at first)
        string world_id = WorldManager.StaticSelectedWorld;
        if (string.IsNullOrEmpty(world_id)) { Debug.LogError("(ChunkEngine - GetChunksDataFromIDs) No world selected, can't load chunks data"); return null; }

        return LoadWorldChunksData(world_id, chunks_ids);
    }
    public static void MakeChunksGrabCapables<T>(Chunk[] chunks, bool only_capables = false, Loggable<T> grab_log = null) where T : MonoBehaviour
    {
        grab_log?.Log($"[ChunkEngine] Making {chunks.Length} chunks grab capables... (only_capables = {only_capables})");

        List<Capable> overlapping_capables = new List<Capable>();
        List<string> added_capable_ids = new List<string>();

        string log = "";
        foreach (Chunk chunk in chunks)
        {
            log += $"   - Chunk {chunk.ID} :\n";
            overlapping_capables.Clear();
            overlapping_capables.AddRange(chunk.GetStaticOverlappingCapables(grab_log));

            // . clear the capables & movables ids room data
            chunk.data.capables_ids = new List<string>();
            if (!only_capables) { chunk.data.movables_ids = new List<string>(); }

            // we try to add the capable ids to the room data
            foreach (Capable capable in overlapping_capables)
            {
                string capable_id = capable.GetStaticID();
                if (added_capable_ids.Contains(capable_id)) { continue; } // already added somewhere

                // . verify not Perso
                if (capable is Perso) { continue; }

                // . verify if not grabbed item
                if (capable is Item item && item.GetStaticGrabbed()) { continue; }

                // . check if movable or capable
                if (capable is Movable)
                {
                    if (only_capables) { continue; }
                    if (chunk.data.movables_ids == null) { chunk.data.movables_ids = new List<string>(); }
                    if (!chunk.data.movables_ids.Contains(capable_id)) { chunk.data.movables_ids.Add(capable_id); }
                    log += $"     - Movable '{capable_id}' (at {capable.transform.position})\n";
                }
                else
                {
                    if (chunk.data.capables_ids == null) { chunk.data.capables_ids = new List<string>(); }
                    if (!chunk.data.capables_ids.Contains(capable_id)) { chunk.data.capables_ids.Add(capable_id); }
                    log += $"     - Capable '{capable_id}' (at {capable.transform.position})\n";
                }

                // . memorize we added this capable to a room
                added_capable_ids.Add(capable_id);
            }
            log += "\n";
        }
        log += "\n";

        grab_log?.Log($"[ChunkEngine] Total Capables grabbed : {added_capable_ids.Count}\n{log}");
    }

}

// LEVEL SPATIAL MAP 2D
public class LevelSpatialMap2D
{
    private float cell_size = 10f; // size of one cell in world units
    private Dictionary<string, Bounds2D> roomBoundsCellsByID = new Dictionary<string, Bounds2D>(); // we keep track of the bounds of each room for spatial queries
    private Dictionary<Vector2Int, HashSet<string>> roomIDsByCell = new Dictionary<Vector2Int, HashSet<string>>(); // we keep track of the rooms in each cell for spatial queries

    /// <summary>
    /// Creates the spatial map from the rooms data of the level. And more precisely
    /// from their collider's bounds.
    /// </summary>
    /// <param name="chunks"></param>
    public void ComputeSpatialMap(List<ChunkData> chunks)
    {
        roomBoundsCellsByID.Clear();
        roomIDsByCell.Clear();

        foreach (ChunkData room in chunks)
        {
            // we get the bounds of the room from its collider
            Bounds2D bounds = create_world_bounds_from_roomdata(room);
            roomBoundsCellsByID[room.id] = bounds;

            // we get the cells covered by the bounds and we add the room id to these cells
            Vector2Int min_cell = WorldToCell(bounds.min);
            Vector2Int max_cell = WorldToCell(bounds.max);
            for (int x = min_cell.x; x <= max_cell.x; x++)
            {
                for (int y = min_cell.y; y <= max_cell.y; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!roomIDsByCell.ContainsKey(cell)) { roomIDsByCell[cell] = new HashSet<string>(); }
                    roomIDsByCell[cell].Add(room.id);
                }
            }
        }
    }
    private Bounds2D create_world_bounds_from_roomdata(ChunkData rdata)
    {
        // we get the points of the collider
        List<Vector2> points = rdata.collider_points;
        if (points == null || points.Count == 0) { return new Bounds2D(); }

        // we create a bounds from the world points
        Vector2 min_cell = new Vector2(int.MaxValue, int.MaxValue);
        Vector2 max_cell = new Vector2(int.MinValue, int.MinValue);
        foreach (Vector2 world_point in points)
        {
            min_cell.x = Mathf.Min(min_cell.x, world_point.x + rdata.position.x);
            min_cell.y = Mathf.Min(min_cell.y, world_point.y + rdata.position.y);
            max_cell.x = Mathf.Max(max_cell.x, world_point.x + rdata.position.x);
            max_cell.y = Mathf.Max(max_cell.y, world_point.y + rdata.position.y);
        }
        return new Bounds2D(min_cell, max_cell);
    }


    // 3*3 query offset to check the cell of the position and its 8 neighbours cells
    private List<Vector2Int> query_offsets = new List<Vector2Int>()
    {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    /// <summary>
    /// retrieves the best matching room at the given world position using the spatial map.
    /// If multiple, it returns the room with the closest bounds center to the position.
    /// If still tie, returns the first one. Returns null if no room found at the position.
    /// </summary>
    /// <param name="world_position"></param>
    public string GetRoomAtPosition(Vector2 world_position)
    {
        string best_room = null;
        float best_distance = float.MaxValue;

        // gather all the rooms candidates in the query
        Vector2Int base_cell = WorldToCell(world_position);
        HashSet<string> room_ids = new HashSet<string>();
        foreach (Vector2Int offset in query_offsets)
        {
            Vector2Int cell = base_cell + offset;
            if (!roomIDsByCell.TryGetValue(cell, out HashSet<string> rids)) { continue; }
            room_ids.UnionWith(rids);
        }

        // get the closest room among the candidates
        foreach (string room_id in room_ids)
        {
            if (!roomBoundsCellsByID.TryGetValue(room_id, out Bounds2D bounds)) { continue; }
            if (!bounds.Contains(world_position)) { continue; }
            float distance = Vector2.Distance(bounds.center, world_position);
            if (distance < best_distance)
            {
                best_distance = distance;
                best_room = room_id;
            }
        }

        if (ChunkEngine.Instance.log_spatial_queries) { Debug.Log($"(LevelSpatialMap - GetRoomAtPosition) Queried position {world_position}, found room {best_room} at distance {best_distance}"); }
        return best_room;
    }



    /// <summary>
    /// check if position is inside room
    /// </summary>
    public bool IsPositionInsideRoom(Vector2 world_position, string room_id)
    {
        if (!roomBoundsCellsByID.TryGetValue(room_id, out Bounds2D bounds)) { return false; }
        return bounds.Contains(world_position);
    }



    // GETTERS & OTHERS
    private Vector2Int WorldToCell(Vector2 world_position)
    {
        int x = Mathf.FloorToInt(world_position.x / cell_size);
        int y = Mathf.FloorToInt(world_position.y / cell_size);
        return new Vector2Int(x, y);
    }
    public Bounds2D GetRoomBounds(string room_id)
    {
        return roomBoundsCellsByID.TryGetValue(room_id, out Bounds2D bounds) ? bounds : new Bounds2D();
    }
}
public struct Bounds2D
{
    public Vector2 center;
    public float width => max.x - min.x;
    public float height => max.y - min.y;
    public float area => width * height;
    public Vector2 min;
    public Vector2 max;

    public Bounds2D(Vector2 min, Vector2 max)
    {
        this.center = (min + max) / 2f;
        // this.size = max - min;
        this.min = min;
        this.max = max;
    }

    public bool Contains(Vector2 point)
    {
        return point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;
    }
}


// SCORE BIAS
public class ScoreBiasState
{
    private ScoreBiasType bias_type;
    public ScoreBiasType Bias { get { return bias_type; } set  { bias_type = value; ttl = default_ttl; } }
    public float Score => GetBiasFromType(bias_type);

    public float ttl;
    public bool IsExpired => ttl <= 0f;
    private float last_tick_time;
    private static float default_ttl = 15f;

    // CONSTRUCTORS
    public ScoreBiasState(ScoreBiasType bias_type)
    {
        this.bias_type = bias_type;
        this.ttl = default_ttl;
        this.last_tick_time = Time.time;
    }

    // TICK
    public void Tick()
    {
        ttl -= (Time.time - last_tick_time); // reduce ttl by the time elapsed since last tick
        last_tick_time = Time.time;
        if (ttl > 0f) { return; }
        
        // reset the bias when expired
        Clear();
    }
    public void Clear()
    {
        bias_type = ScoreBiasType.None;
        ttl = 0f;
    }

    // GETTERS & OTHERS
    private static float GetBiasFromType(ScoreBiasType bias_type)
    {
        switch (bias_type)
        {
            case ScoreBiasType.None: return 0f;
            case ScoreBiasType.RoomEnter: return 200f;
            case ScoreBiasType.RoomExit: return -200f;
            default: return 0f;
        }
    }
}
public enum ScoreBiasType
{
    None,
    RoomEnter,
    RoomExit,
    RoomFreed
}


