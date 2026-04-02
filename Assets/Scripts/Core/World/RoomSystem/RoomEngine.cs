using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RoomEngine : BSOD_System<RoomEngine>
{

    // ROOMS DATA

    [Header("Rooms data")]
    // private string data_path = "data/rooms/";
    public Dictionary<string, RoomData> rooms_data = new Dictionary<string, RoomData>();
    private Dictionary<string, int> rooms_hashs_by_ids = new Dictionary<string, int>();
    private Dictionary<int, string> rooms_ids_by_hash = new Dictionary<int, string>();
    private int next_room_hash = 1;
    public Dictionary<string, RoomData> loaded_rooms_data = new Dictionary<string, RoomData>();
    public RoomData main_room_data; // the main room is the one where the perso is, we need to keep track of it to know which room to load when the perso changes room
    public Action<RoomData> OnRoomChange = delegate { };


    // CAPABLES PER ROOMS

    private Dictionary<string, string> roomByCapableID = new Dictionary<string, string>(); // we keep track of the room of the STATIC CAPABLES (NOT MOVABLES)
    private Dictionary<string, string> roomByMovableID = new Dictionary<string, string>(); // we keep track of the room of the MOVABLES
    private Dictionary<string, HashSet<string>> roomByCapableIDs = new Dictionary<string, HashSet<string>>(); // we keep track of all capables in each room (capables + movables)
    private Dictionary<string, int> dirtyCapablesIDs = new Dictionary<string, int>(); // capables that don't have any room assigned / just changed rooms, waiting for new assignment. the int is a priority flag
    private Dictionary<string, Dictionary<string, ScoreBiasState>> room_score_biases = new Dictionary<string, Dictionary<string, ScoreBiasState>>();
    public Action<string, RoomData> OnCapableAddedToRoom = delegate { };
    public Action<string, RoomData> OnCapableRemovedFromRoom = delegate { };


    // PARAMETERS & LOGS

    [Header("Loading parameters")]
    public int frames_between_loading_rooms = 10;

    [Header("Tick parameters")]
    public int frames_between_ticks = 1;
    public int dirty_capables_handled_per_tick = 10;
    private bool ticking = false;

    [Header("Logs awakening")]
    public bool log_awake_data = false;
    // public bool log_init = false;

    [Header("Logs loading")]
    public bool log_loading = false;

    [Header("Log ticks")]
    public bool log_ticks = false;
    public bool log_room_transfers = false;
    public bool log_loaded_area_transfers = false;
    public bool log_best_match_calcul = false;
    public bool log_spatial_queries = false;
    public bool log_neighbours = false;
    public bool hide_log_outsider_created = false;

    [Header("Logs spawning")]
    public bool log_dynamic_room_assignement = false;
    public bool hide_log_no_room_of_capable_found = false;

    [Header("Room Logs")]
    public bool log_tilemaps_loading = false;
    public bool log_colliders = false;
    public bool log_enter_exit = false;

    /* -------------------------------------

     1. AWAKE & DATA LOADING + 2D SPATIAL CELL ROOMS

    ------------------------------------- */


    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load rooms data
        loadRoomsData();

        // start ticking
        ticking = true;
    }

    // LOAD / UNLOAD DATA
    protected void loadRoomsData()
    {
        // we empty the rooms_data and runtime ids
        rooms_data = new Dictionary<string, RoomData>();
        rooms_hashs_by_ids = new Dictionary<string, int>();
        rooms_ids_by_hash = new Dictionary<int, string>();
        next_room_hash = 1;
        string log_rooms_details = "\n\n";

        // we load all the json files in the data path and convert them to RoomData objects
        string[] files = GameManager.Instance.LoadJsonsFromWorldDataPath("rooms");
        foreach (string file in files)
        {
            RoomData data = JsonUtility.FromJson<RoomData>(file);
            rooms_data.Add(data.id, data);
            generate_runtime_room_id(data.id);
            log_rooms_details += data.GetDetails() + "\n";

            // we call addCapableToRoom for each capable in the room to set the capable-room links in the RoomEngine
            foreach (string capable_id in data.capables_ids) { addCapableToRoom(capable_id, data, false); }
            foreach (string movable_id in data.movables_ids) { addCapableToRoom(movable_id, data, true); }
        }

        if (log_awake_data) { Debug.Log("(RoomEngine) ROOMS DATA LOADED : " + rooms_data.Count + log_rooms_details); }
        // awake_done = true;
    }
    private int generate_runtime_room_id(string id)
    {
        if (string.IsNullOrEmpty(id)) { return 0; }
        if (rooms_hashs_by_ids.TryGetValue(id, out int existing)) { return existing; }

        int new_hash = next_room_hash++;
        rooms_hashs_by_ids[id] = new_hash;
        rooms_ids_by_hash[new_hash] = id;
        return new_hash;
    }


    // 2D SPATIAL CELL ROOMS
    private Dictionary<string, LevelSpatialMap2D> spatial_maps_by_level_id = new Dictionary<string, LevelSpatialMap2D>();
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
            List<RoomData> rooms = LevelEngine.Instance.GetRoomsDataOfLevel(level_id);
            spatial_map.ComputeSpatialMap(rooms);
        }
    }
    protected RoomData GetRoomAtPosition(Vector2 world_position, string level_id=null)
    {
        // if level_id is null then we get the current level id from the LevelEngine
        if (string.IsNullOrEmpty(level_id)) { level_id = LevelEngine.Instance.CurrentLevelID; }

        // we check if we have a spatial map for the level
        if (!spatial_maps_by_level_id.TryGetValue(level_id, out LevelSpatialMap2D spatial_map))
        {
            if (log_spatial_queries) { Debug.LogWarning("(RoomEngine - GetRoomAtPosition) No spatial map found for level id: " + level_id); }
            return null;
        }

        // we query the spatial map for the rooms at the position
        string room_id = spatial_map.GetRoomAtPosition(world_position);
        if (string.IsNullOrEmpty(room_id)) { return null; }
        if (!rooms_data.TryGetValue(room_id, out RoomData room)) { return null; }
        return room;
    }
    protected bool IsPositionInsideRoom(Vector2 world_position, string room_id, string level_id = null)
    {
        // if level_id is null then we get the current level id from the LevelEngine
        if (string.IsNullOrEmpty(level_id)) { level_id = LevelEngine.Instance.CurrentLevelID; }

        // we check if we have a spatial map for the level
        if (!spatial_maps_by_level_id.TryGetValue(level_id, out LevelSpatialMap2D spatial_map))
        {
            if (log_spatial_queries) { Debug.LogWarning("(RoomEngine - IsPositionInsideRoom) No spatial map found for level id: " + level_id); }
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


    /* -------------------------------------

     2. ASSIGNING ROOMS TO CAPABLES

    ------------------------------------- */

    // START
    private void Start()
    {
        // we register to CapableSystem.OnCapableNeedRoom so we can assign rooms to the new capable
        // CapableSystem.Instance.OnCapableNeedRoom += handleCapableNeedRoom;
        // CapableSystem.Instance.OnCapableNeedFreedom += removeCapableFromSystem;

        // we generate the spatial maps for the levels
        generateLevels2DSpatialCells();
    }



    // ADD / REMOVE CAPABLE TO / FROM ROOM

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
    protected void addCapableToRoom(string entity_id, RoomData room, bool is_movable)
    {
        // we add the capable to our dictionaries
        if (!is_movable) { roomByCapableID[entity_id] = room.id; }
        else { roomByMovableID[entity_id] = room.id; }
        if (!roomByCapableIDs.ContainsKey(room.id)) { roomByCapableIDs[room.id] = new HashSet<string>(); }
        if (!roomByCapableIDs[room.id].Contains(entity_id)) { roomByCapableIDs[room.id].Add(entity_id); }

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
    protected void removeCapableFromRoom(string entity_id, RoomData room)
    {
        // we remove the capable from our dictionaries
        if (roomByCapableID.ContainsKey(entity_id)) { roomByCapableID.Remove(entity_id); }
        if (roomByMovableID.ContainsKey(entity_id)) { roomByMovableID.Remove(entity_id); }
        if (roomByCapableIDs.ContainsKey(room.id) && roomByCapableIDs[room.id].Contains(entity_id)) { roomByCapableIDs[room.id].Remove(entity_id); }

        // we remove the capable from the room's data
        if (room.capables_ids.Contains(entity_id)) { room.capables_ids.Remove(entity_id); }
        if (room.movables_ids.Contains(entity_id)) { room.movables_ids.Remove(entity_id); }

        // we invoke the event
        OnCapableRemovedFromRoom?.Invoke(entity_id, room);
    }




    // ON ROOM ENTER
    public void OnRoomEnter(RoomData room, Capable capable, int priority = 0) => OnRoomEnter(room.id, capable.data.id, priority);
    public void OnRoomEnter(string room_id, string entity_id, int priority = 0)
    {

        if (log_enter_exit) { Debug.Log($"(RoomEngine) OnRoomEnter : {entity_id} entered {room_id} with priority {priority}"); }

        if (!dirtyCapablesIDs.ContainsKey(entity_id)) { dirtyCapablesIDs[entity_id] = priority; }
        else
        {
            int new_priority = dirtyCapablesIDs[entity_id] + priority + 1;
            dirtyCapablesIDs[entity_id] = new_priority > 1000 ? 1000 : new_priority; // we cap the priority to avoid overflow and keep it manageable
        }

        set_only_bias(entity_id, room_id, ScoreBiasType.RoomEnter); // we set the bias to RoomEnter for the room the capable just entered, and clear it for other rooms to prefer the new room for next room switch resolution

    }
    // ON ROOM EXIT
    public void OnRoomExit(RoomData room, Capable capable, int priority = 0) => OnRoomExit(room.id, capable.data.id, priority);
    public void OnRoomExit(string room_id, string entity_id, int priority = 0)
    {
        if (log_enter_exit) { Debug.Log($"(RoomEngine) OnRoomExit : {entity_id} exited {room_id} with priority {priority}"); }

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
        if (!room_score_biases.ContainsKey(entity_id)) { room_score_biases[entity_id] = new Dictionary<string, ScoreBiasState>(); }

        // add a new room bias if not exists / set bias if already exists
        if (!room_score_biases[entity_id].ContainsKey(room_id)) { room_score_biases[entity_id][room_id] = new ScoreBiasState(bias); }
        else { room_score_biases[entity_id][room_id].Bias = bias; }

        // we clear other biases of same type to keep only one bias like this for this capable
        for (int i = 0; i < room_score_biases[entity_id].Keys.Count; i++)
        {
            string other_room_id = room_score_biases[entity_id].Keys.ElementAt(i);
            if (other_room_id == room_id) { continue; }
            ScoreBiasState other = room_score_biases[entity_id][other_room_id];

            // we clear other biases of same type to keep only one bias like this !
            if (other.Bias != bias) { continue; }
            room_score_biases[entity_id][other_room_id].Clear();
        }

    }


    // CAPABLE FREEING
    public void FreeCapable(Capable capable) => FreeCapable(capable.data.id);
    public void FreeCapable(string id)
    {
        // we free the capable from any room, it may be destroyed or else
        RoomData room = GetCapableRoom(id);
        if (room == null) { return; }
        removeCapableFromRoom(id, room);

        if (log_room_transfers) { Debug.Log($"(RoomEngine) [{room.id}] >> {id} >> [none]         -- was freed !!"); }
        if (log_loaded_area_transfers) { Debug.Log($"(RoomEngine) [{room.id}] >> {id} >> [none]         -- was freed !!"); }
    }


    /* -------------------------------------

     3. DYNAMIC ROOM OF CAPABLE MANAGEMENT (UPDATE)

    ------------------------------------- */

    // UPDATE
    private int frames_since_last_tick = 0;
    protected virtual void Update()
    {
        if (!ticking) { return; }

        frames_since_last_tick++;
        if (frames_since_last_tick < frames_between_ticks) { return; }
        frames_since_last_tick = 0;

        // we tick !
        Tick();
    }

    // TICK
    private List<string> dirty_capables_ids = new List<string>();
    private List<RoomData> room_candidates = new List<RoomData>();
    private List<string> capables_to_unload = new List<string>();
    protected void Tick()
    {
        // . we get the highest priority dirty capables
        dirty_capables_ids.Clear();
        dirty_capables_ids = dirtyCapablesIDs.OrderByDescending(kv => kv.Value).Take(dirty_capables_handled_per_tick).Select(kv => kv.Key).ToList();
        if (dirty_capables_ids.Count == 0) { return; }

        string controlled_id = Controller.Instance.ControlledID;

        string log_tick = "";
        if (log_ticks) { log_tick += $"(RoomEngine - Tick) Handling {dirty_capables_ids.Count} dirty capables : "; }

        capables_to_unload.Clear();

        // . we handle them
        foreach (string capable_id in dirty_capables_ids)
        {
            if (log_ticks) { log_tick += $"\n  - {capable_id} : \n"; }

            // we get the current room of the capable, if it has one
            RoomData current_room = GetCapableRoom(capable_id);
            if (log_ticks) { log_tick += $"    - current room : {(current_room != null ? current_room.id : "none")}\n"; }

            // we get the rooms candidates for the capable
            room_candidates.Clear();
            if (current_room != null) { get_room_neighbours(current_room.id, ref room_candidates); }
            else { get_room_candidates_from_position(capable_id, ref room_candidates); }
            if (log_ticks) { log_tick += $"    - room candidates : {room_candidates.Count} ({string.Join(", ", room_candidates.Select(r => r.id))})\n"; }

            // check if we have candidates
            if (room_candidates.Count == 0)
            {
                if (!hide_log_outsider_created) { Debug.LogWarning($"(RoomEngine - Tick) No room candidates found for capable {capable_id}, it may become an outsider of the RoomEngine."); }
                continue;
            }

            // we resolve the best room
            RoomData best_room = get_best_room_for_capable(capable_id, room_candidates);
            if (log_ticks) { log_tick += $"    - best room : {(best_room != null ? best_room.id : "none")}\n"; }
            if (best_room == null) { continue; }
            if (current_room != null && best_room.id == current_room.id) { continue; }

            // check if perso changed room
            if (capable_id == controlled_id)
            {
                if (log_ticks) { log_tick += $"    - controlled capable changed room, handling it... \n"; }
                handle_perso_changed_room(current_room, best_room);
            }

            // we assign the capable to the best room
            string out_room_id = current_room != null ? current_room.id : "none";
            string in_room_id = best_room != null ? best_room.id : "none";
            if (log_room_transfers) { Debug.Log($"(RoomEngine) [{out_room_id}] >> {capable_id} >> [{in_room_id}]"); }
            if (current_room != null) { removeCapableFromRoom(capable_id, current_room); }
            addCapableToRoom(capable_id, best_room, CapableSystem.Instance.IsMovable(capable_id));
            if (log_ticks) { log_tick += $"    - TRANSFERED TO NEW ROOM !!! : {best_room.id}\n"; }

            // check if the new room is unloaded and if yes we need to unload the entity as well
            if (!loaded_rooms_data.ContainsKey(best_room.id))
            {
                if (log_ticks) { log_tick += $"    - new room is not loaded, adding entity to unload list... \n"; }
                if (log_loaded_area_transfers) { Debug.Log($"(RoomEngine) [{out_room_id}] >> {capable_id} >> [{in_room_id}]      (quit loaded area)"); }
                capables_to_unload.Add(capable_id);
            }
        }

        // we unload the entities that need to be unloaded
        CapableSystem.Instance.UnloadCapables(capables_to_unload);

        // . we remove the handled capables from the dirty list
        foreach (string capable_id in dirty_capables_ids)
        {
            // verify that the capable has a room, otherwise we log an error/warning because it means the capable is an outsider of the room engine system D:
            if (!IsInARoom(capable_id) && !hide_log_no_room_of_capable_found)
            {
                string log = $"(RoomEngine - Tick) Capable {capable_id} could not be assigned to any room. Is now a RoomEngine outsider.";
                bool is_capable_outsider = CapableSystem.Instance.IsOutsider(capable_id);
                if (!is_capable_outsider) { Debug.LogError(log + " (Insider of the CapableSystem, critical issue...)"); }
                else { Debug.LogWarning(log + " (Outsider of the CapableSystem as well so may be ok)"); }
            }

            // we remove the capable from the dirty list
            dirtyCapablesIDs.Remove(capable_id);
        }

        if (log_ticks) { Debug.Log(log_tick); }
    }
    
    // get rooms candidates for capable
    private void get_room_candidates_from_position(string capable_id, ref List<RoomData> room_candidates)
    {
        Vector2 capable_position = CapableSystem.Instance.GetCapablePosition(capable_id);
        RoomData position_candidate = GetRoomAtPosition(capable_position);
        if (position_candidate == null) { return; }
        get_room_neighbours(position_candidate.id, ref room_candidates);
    }
    private void get_room_neighbours(string room_id, ref List<RoomData> room_candidates)
    {
        if (!rooms_data.ContainsKey(room_id)) { return; }
        RoomData room = rooms_data[room_id];
        room_candidates.Add(room);
        room_candidates.AddRange(GetNeighboursData(room));
    }

    // resolve best room for capable among candidates
    private RoomData get_best_room_for_capable(string capable_id, List<RoomData> room_candidates)
    {
        if (room_candidates.Count == 0) { return null; }
        LevelSpatialMap2D spatial_map = CurrentSpatialMap;
        if (spatial_map == null) { return null; }

        string log = "";
        if (log_best_match_calcul) { log += $"(RoomEngine - get_best_room_for_capable) Resolving best room for {capable_id} among {room_candidates.Count} candidates : \n"; }

        // keep best score
        RoomData best_room = null;
        float best_score = float.MinValue;
        float best_distance = float.MaxValue;

        // we iterate candidates and score them
        foreach (RoomData rdata in room_candidates)
        {
            if (log_best_match_calcul) { log += $"  - candidate room {rdata.id} : \n"; }
            float score = 0f;

            // check if the capable is inside the AABB
            Vector2 capable_position = CapableSystem.Instance.GetCapablePosition(capable_id);
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
            if (room_score_biases.TryGetValue(capable_id, out Dictionary<string, ScoreBiasState> biases_by_room) &&
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
            RoomData current_capable_room = GetCapableRoom(capable_id);
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
    private void handle_perso_changed_room(RoomData from_room, RoomData to_room)
    {

        // . find rooms to load / unload based on new controlled room neighbours.
        Stack<string> rooms_to_unload = new Stack<string>();
        Stack<string> rooms_to_load = new Stack<string>();
        List<string> new_neighbours_ids = GetNeighboursIDs(to_room);

        // check which of the currently loaded rooms we need to unload
        // todo would it be better to find them from the from_room neighbours data ?
        foreach (string loaded_room_id in loaded_rooms_data.Keys)
        {
            if (loaded_room_id == to_room.id) { continue; }
            if (!new_neighbours_ids.Contains(loaded_room_id)) { rooms_to_unload.Push(loaded_room_id); }
        }

        // check which of the new neighbours we need to load (that are not already loaded)
        for (int i = 0; i < new_neighbours_ids.Count; i++)
        {
            string neighbour_id = new_neighbours_ids[i];
            if (neighbour_id == to_room.id) { continue; }
            if (!loaded_rooms_data.ContainsKey(neighbour_id)) { rooms_to_load.Push(neighbour_id); }
        }

        main_room_data = to_room;
        OnRoomChange.Invoke(to_room);

        // . load the new rooms and unload old ones.
        LoadRooms(rooms_to_load.ToArray());
        unloadRooms(rooms_to_unload.ToArray());
    }


    /* -------------------------------------

     4. DYNAMIC ROOM LOADING & UNLOADING

    ------------------------------------- */




    // LOAD ROOMS
    public async void LoadRooms(string[] rooms_ids)
    {
        // for each room id we need to find its data and load it
        foreach (string room_id in rooms_ids)
        {
            // we load the room
            load_room(room_id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loading_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    /* private async void loadRooms(ICollection<string> rooms_ids)
    {
        foreach (string id in rooms_ids)
        {
            load_room(id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loading_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    } */
    private void load_room(string id)
    {
        if (!rooms_data.ContainsKey(id)) { Debug.LogWarning("(RoomEngine - Load) Room data not found for id: " + id); return; }
        RoomData data = rooms_data[id];
        RoomBank.Instance.Load(data);
        loaded_rooms_data.Add(id, data);
        if (log_loading) { Debug.Log("(RoomEngine) Loaded " + id); }
    }

    // UNLOAD ROOMS
    public void UnloadAllRooms()
    {
        unloadRooms(loaded_rooms_data.Keys as ICollection<string>);
    }
    public void UnloadRooms(string[] rooms_ids)
    {
        unloadRooms(rooms_ids);
    }
    private async void unloadRooms(ICollection<string> rooms_ids)
    {
        foreach (string id in rooms_ids)
        {
            unload_room(id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loading_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    private void unload_room(string id)
    {
        if (!loaded_rooms_data.ContainsKey(id)) { Debug.LogWarning("(RoomEngine - Unload) Loaded room data not found for id: " + id); return; }
        RoomData data = loaded_rooms_data[id];
        RoomBank.Instance.Unload(data);
        loaded_rooms_data.Remove(id);
        if (log_loading) { Debug.Log("(RoomEngine) Unloaded " + id); }
    }






    /* -------------------------------------

     5. GETTERS & OTHERS

    ------------------------------------- */



    // NEIGHBOURS MANAGEMENT
    public List<string> GetNeighboursIDs(RoomData rdata)
    {
        List<string> neighbours_ids = rdata.neighbours_ids != null ? new List<string>(rdata.neighbours_ids) : new List<string>();
        if (log_neighbours)
        {
            string log = "(RoomEngine) Neighbours of " + rdata.id + ": ";
            for (int i = 0; i < neighbours_ids.Count; i++)
            {
                log += neighbours_ids[i] + " ";
            }
            Debug.Log(log);
        }
        return neighbours_ids;
    }
    public List<RoomData> GetNeighboursData(RoomData room)
    {
        List<RoomData> neigh_datas = new List<RoomData>();

        string log = "";
        for (int i = 0; i < room.neighbours_ids.Count; i++)
        {
            string neighbour_id = room.neighbours_ids[i];
            if (!rooms_data.ContainsKey(neighbour_id))
            {
                if (log_neighbours) { log += $"  - {neighbour_id} (data was not found)\n"; }
                continue;
            }

            if (log_neighbours) { log += $"  - {neighbour_id}\n"; }
            neigh_datas.Add(rooms_data[neighbour_id]);
        }

        if (log_neighbours) { Debug.Log($"(RoomEngine) Neighbours of {room.id} : {neigh_datas.Count}\n{log}"); }
        return neigh_datas;
    }


    // GETTERS
    public int GetRoomHashFromID(string id)
    {
        return rooms_hashs_by_ids.TryGetValue(id, out int hash) ? hash : 0;
    }
    public string GetRoomIDFromHash(int hash)
    {
        return rooms_ids_by_hash.TryGetValue(hash, out string id) ? id : null;
    }
    private RoomData GetCapableRoom(string capable_id)
    {
        // check if we have the capable in our dictionaries
        string room_id;
        if (roomByCapableID.TryGetValue(capable_id, out string id_1)) { room_id = id_1; }
        else if (roomByMovableID.TryGetValue(capable_id, out string id_2)) { room_id = id_2; }
        else { return null; }
        return rooms_data.TryGetValue(room_id, out RoomData room) ? room : null;
    }
    public bool IsInARoom(string capable_id)
    {
        return GetCapableRoom(capable_id) != null;
    }
    public bool TryGetCapableRoom(string capable_id, out RoomData room)
    {
        room = GetCapableRoom(capable_id);
        return room != null;
    }
    public List<RoomData> GetRoomsDataFromIDs(ICollection<string> rooms_ids)
    {
        List<RoomData> rooms_datas = new List<RoomData>();
        foreach (string room_id in rooms_ids)
        {
            if (!rooms_data.ContainsKey(room_id)) { continue; }
            rooms_datas.Add(rooms_data[room_id]);
        }
        return rooms_datas;
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
    /// <param name="rooms"></param>
    public void ComputeSpatialMap(List<RoomData> rooms)
    {
        roomBoundsCellsByID.Clear();
        roomIDsByCell.Clear();

        foreach (RoomData room in rooms)
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
    private Bounds2D create_world_bounds_from_roomdata(RoomData rdata)
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

        if (RoomEngine.Instance.log_spatial_queries) { Debug.Log($"(LevelSpatialMap - GetRoomAtPosition) Queried position {world_position}, found room {best_room} at distance {best_distance}"); }
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
    public Vector2 size;
    public Vector2 min;
    public Vector2 max;

    public Bounds2D(Vector2 min, Vector2 max)
    {
        this.center = (min + max) / 2f;
        this.size = max - min;
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