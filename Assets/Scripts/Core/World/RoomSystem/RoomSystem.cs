using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public class RoomSystem : BSOD_System<RoomSystem>
{
    [Header("Rooms data")]
    private string data_path = "data/rooms/";
    public Dictionary<string, RoomData> rooms_data = new Dictionary<string, RoomData>();
    private Dictionary<string, int> rooms_hashs_by_ids = new Dictionary<string, int>();
    private Dictionary<int, string> rooms_ids_by_hash = new Dictionary<int, string>();
    private int next_room_hash = 1;
    public Dictionary<string, RoomData> loaded_rooms_data = new Dictionary<string, RoomData>();
    public RoomData main_room_data; // the main room is the one where the perso is, we need to keep track of it to know which room to load when the perso changes room

    [Header("Loading parameters")]
    public bool awake_done = false;
    public bool start_loading_done = false;
    // public Transform room_parent;
    public int frames_between_loaded_rooms = 10;
    public int frames_between_ticks = 1;

    [Header("Initialization parameters")]
    public bool init_doing = false;
    public bool init_done = false;
    public int frames_between_room_overlap_checks = 10;

    [Header("Logs")]
    public bool log_awake_data = false;
    public bool log_init = false;
    public bool log_loading = false;
    public bool log_neighbours = false;
    public bool log_room_transfers = false;
    public bool log_ticks = false;
    public bool log_dynamic_room_assignement = false;
    public bool hide_log_no_room_of_capable_found = false;

    [Header("Room Logs")]
    public bool log_tilemaps_loading = false;
    public bool log_colliders = false;


    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load rooms data
        loadRoomsData();
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
        string[] files = GameManager.Instance.LoadJsons(data_path);
        foreach (string file in files)
        {
            RoomData data = JsonUtility.FromJson<RoomData>(file);
            rooms_data.Add(data.id, data);
            generate_room_hash(data.id);
            log_rooms_details += data.GetDetails() + "\n";
        }

        if (log_awake_data) { Debug.Log("(RoomSystem) ROOMS DATA LOADED : " + rooms_data.Count + log_rooms_details); }
        awake_done = true;
    }

    // START
    private void Start()
    {
        // we register to CapableSystem.OnCapableNeedRoom so we can assign rooms to the new capable
        CapableSystem.Instance.OnCapableNeedRoom += handleCapableNeedRoom;
        CapableSystem.Instance.OnCapableNeedFreedom += handleCapableNeedFreedom;
    }

    // LOAD ROOMS
    public async void LoadRooms(string[] rooms_ids)
    {
        // for each room id we need to find its data and load it
        foreach (string room_id in rooms_ids)
        {
            // we load the room
            load_room(room_id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
        start_loading_done = true;
    }
    private async void loadRooms(ICollection<string> rooms_ids)
    {
        foreach (string id in rooms_ids)
        {
            load_room(id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    private void load_room(string id)
    {
        if (!rooms_data.ContainsKey(id)) { Debug.LogWarning("(RoomSystem - Load) Room data not found for id: " + id); return; }
        RoomData data = rooms_data[id];
        RoomBank.Instance.Load(data);
        loaded_rooms_data.Add(id, data);
        if (log_loading) { Debug.Log("(RoomSystem) Loaded " + id); }
    }

    // UNLOAD ROOMS
    public void UnloadAllRooms()
    {
        unloadRooms(loaded_rooms_data.Keys as ICollection<string>);
    }
    private async void unloadRooms(ICollection<string> rooms_ids)
    {
        foreach (string id in rooms_ids)
        {
            unload_room(id);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    private void unload_room(string id)
    {
        if (!loaded_rooms_data.ContainsKey(id)) { Debug.LogWarning("(RoomSystem - Unload) Loaded room data not found for id: " + id); return; }
        RoomData data = loaded_rooms_data[id];
        RoomBank.Instance.Unload(data);
        loaded_rooms_data.Remove(id);
        if (log_loading) { Debug.Log("(RoomSystem) Unloaded " + id); }
    }

    // UPDATE
    private int frames_since_last_tick = 0;
    protected virtual void Update()
    {
        if (!awake_done || !start_loading_done) { return; }
        if (!init_done)
        {
            if (init_doing) { return; }
            Init();
        }

        frames_since_last_tick++;
        if (frames_since_last_tick < frames_between_ticks) { return; }
        
        // we tick !
        Tick();
        frames_since_last_tick = 0;
    }

    // INIT
    /// <summary>
    /// this method is called when first loading is done. its only aim is to
    /// discover all capables & movables that are inside the loaded rooms
    /// and that are not already affected to a room. Because of this, this method is
    /// only useful during development because when the save system will be up, there won't
    /// be any capable that is not loaded from WorldData, which means all capable will be loaded
    /// at awake through the help of the capablesystem, and so they will have a room. this method
    /// uses overlapping colliders checks anyway so it is not perfect, but we don't care it's
    /// temporary.
    /// </summary>
    private async void Init()
    {
        init_doing = true;

        // we get the loaded rooms
        List<Room> loaded_rooms = RoomBank.Instance.GetAllLoadedRooms();
        if (log_init) { Debug.Log("(RoomSystem) Init started with " + loaded_rooms.Count + " rooms"); }

        // we go through all rooms and do an overlap check to get all capables inside each room
        List<string> capables_added = new List<string>();
        for (int i = 0; i < loaded_rooms.Count; i++)
        {
            Room room = loaded_rooms[i];
            RoomData room_data = room.data;

            // we get all the inside capables & movables
            room.GetOverlappingCapablesIDs(out List<string> overlapping_capables, out List<string> overlapping_movables);
            if (log_init) { Debug.Log($"(RoomSystem) Init - [{room_data.id}] found :    {overlapping_capables.Count} Capable ||| {overlapping_movables.Count} Movable"); }

            // we add all capables first
            for (int j = 0; j < overlapping_capables.Count; j++)
            {
                string capable_id = overlapping_capables[j];

                // check if capable was already added to a room
                if (capables_added.Contains(capable_id)) { continue; }

                // verify that we have a stored data for this capable,
                // if not we skip it
                if (!CapableSystem.Instance.HasLoadedCapableData(capable_id)) { continue; }

                // check if capable is already in the room
                if (!room_data.capables_ids.Contains(capable_id))
                {
                    room_data.capables_ids.Add(capable_id);
                    if (log_init) { Debug.Log($"(RoomSystem) [{room_data.id}] added capable : {capable_id}"); }
                }
                else if (log_init) { Debug.Log($"(RoomSystem) [{room_data.id}] had already capable : {capable_id}"); }

                // we memorize we added the capable
                capables_added.Add(capable_id);
            }

            // we do the same for the movables
            capables_added.Clear();
            for (int j = 0; j < overlapping_movables.Count; j++)
            {
                string movable_id = overlapping_movables[j];

                // check if movable was already added to a room
                if (capables_added.Contains(movable_id)) { continue; }

                // verify that we have a stored data for this capable,
                // if not we skip it
                if (!CapableSystem.Instance.HasLoadedCapableData(movable_id)) { continue; }

                // check if movable is already in the room
                if (!room_data.movables_ids.Contains(movable_id))
                {
                    room_data.movables_ids.Add(movable_id);
                    if (log_init) { Debug.Log($"(RoomSystem) [{room_data.id}] added movable : {movable_id}"); }
                }
                else if (log_init) { Debug.Log($"(RoomSystem) [{room_data.id}] had already movable : {movable_id}"); }

                // we memorize we added the movable
                capables_added.Add(movable_id);
            }
            
            // wipe out the IN data
            room_data.IN_movables_ids.Clear();

            for (int j = 0; j < frames_between_room_overlap_checks; j++) { await System.Threading.Tasks.Task.Yield(); }
        }

        init_done = true;
        if (log_init) { Debug.Log("(RoomSystem) Init done and started ticking"); }
    }

    // OLD TICK
    /* protected virtual void Tick2()
    {

        // if (log_ticks) { Debug.Log("(RoomSystem) Tick called"); }

        // 1. find all capables that changes room
        RoomData room;
        Dictionary<RoomData, string> movables_IN = new Dictionary<RoomData, string>(); // RoomData, CapableID
        Dictionary<RoomData, string> movables_OUT = new Dictionary<RoomData, string>(); // RoomData, CapableID
        ICollection rooms_ids = rooms_data.Keys;
        foreach (string room_id in rooms_ids)
        {
            room = rooms_data[room_id];
            for (int j = 0; j < room.IN_movables_ids.Count; j++)
            {
                string capable_id = room.IN_movables_ids[j];
                movables_IN[room] = capable_id;
            }
            for (int j = 0; j < room.OUT_movables_ids.Count; j++)
            {
                string capable_id = room.OUT_movables_ids[j];
                movables_OUT[room] = capable_id;
            }
        }
        if (log_ticks && log_room_transfers) { log_in_out(movables_IN, movables_OUT); }

        // 2. find existing IN & OUT pairs 
        List<string> capable_ids = new List<string>(); // is it better to use Stack ?
        List<RoomData> out_rooms = new List<RoomData>();
        List<RoomData> in_rooms = new List<RoomData>();
        foreach (KeyValuePair<RoomData, string> pair in movables_OUT)
        {
            string capable_id = pair.Value;
            RoomData out_room = pair.Key;

            // we cycle through all the in rooms to see if we have a matching pair
            foreach (KeyValuePair<RoomData, string> pair2 in movables_IN)
            {
                // we make sure that the movable is not going from and to the same room
                if (pair2.Value == capable_id && pair2.Key != out_room)
                {
                    RoomData in_room = pair2.Key;
                    capable_ids.Add(capable_id);
                    out_rooms.Add(out_room);
                    in_rooms.Add(in_room);
                }
            }
        }
        if (log_room_transfers)
        {
            for (int i = 0; i < capable_ids.Count; i++)
            {
                string capable_id = capable_ids[i];
                RoomData out_room = out_rooms[i];
                RoomData in_room = in_rooms[i];
                Debug.Log($"(RoomSystem) {capable_id} exits {out_room.id} for {in_room.id}");
            }
        }

        // 3. update rooms data with the capable changes
        foreach (string room_id in rooms_ids)
        {
            room = rooms_data[room_id];
            if (out_rooms.Contains(room))
            {
                int index = out_rooms.IndexOf(room);
                string capable_id = capable_ids[index];
                room.movables_ids.Remove(capable_id);
                room.OUT_movables_ids.Remove(capable_id);
            }
            if (in_rooms.Contains(room))
            {
                int index = in_rooms.IndexOf(room);
                string capable_id = capable_ids[index];
                room.movables_ids.Add(capable_id);
                room.IN_movables_ids.Remove(capable_id);
            }
        }

        // 4. check if perso changed room if yes we need to load / unload some rooms
        bool perso_changed_room = false;
        RoomData perso_new_room = null;
        for (int i=0; i<capable_ids.Count; i++)
        {
            // check if is perso
            if (Controller.Instance.ControlledID != capable_ids[i]) { continue; }
            
            perso_changed_room = true;
            perso_new_room = in_rooms[i];
        }
        if (!perso_changed_room) { return; }

        // 5. Find rooms to Load / Unload
        Stack<string> rooms_to_unload = new Stack<string>();
        Stack<string> rooms_to_load = new Stack<string>();
        List<string> new_neighbours_ids = GetNeighboursIDs(perso_new_room);
        ICollection loaded_rooms_ids = loaded_rooms_data.Keys;
        foreach (string loaded_room_id in loaded_rooms_ids)
        {
            if (loaded_room_id == perso_new_room.id) { continue; }

            // we check if the room is in the new neighbours
            if (!new_neighbours_ids.Contains(loaded_room_id)) { rooms_to_unload.Push(loaded_room_id); }
        }
        for (int i = 0; i < new_neighbours_ids.Count; i++)
        {
            string neigh_id = new_neighbours_ids[i];
            if (neigh_id == perso_new_room.id) { continue; }

            // we check if the room is already loaded
            if (!loaded_rooms_data.ContainsKey(neigh_id)) { rooms_to_load.Push(neigh_id); }
        }

        main_room_data = perso_new_room;

        // 6. We load the new rooms and unload the old ones
        loadRooms(rooms_to_load.ToArray());
        unloadRooms(rooms_to_unload.ToArray());
    }
    protected void log_in_out(Dictionary<RoomData, string> into, Dictionary<RoomData, string> from)
    {
        string log = "(RoomSystem) IN/OUT : \n IN :";
        foreach (KeyValuePair<RoomData, string> pair in into)
        {
            log += $"\n   - {pair.Value} : {pair.Key.id}";
        }
        log += "\n\n OUT :";
        foreach (KeyValuePair<RoomData, string> pair in from)
        {
            log += $"\n   - {pair.Value} in {pair.Key.id}";
        }
        Debug.Log(log);
    }

    */

    // TICK
    protected virtual void Tick()
    {
        // 1. create Allocator.TempJob Collections to pass data to the job
        NativeParallelMultiHashMap<int, int> rooms_movables_IN = new NativeParallelMultiHashMap<int, int>(100, Allocator.TempJob); // room, List<movable>
        NativeParallelMultiHashMap<int, int> rooms_movables_OUT = new NativeParallelMultiHashMap<int, int>(100, Allocator.TempJob); // movable, List<room>
        NativeList<MovableRoomTransition> transitions = new NativeList<MovableRoomTransition>(100, Allocator.TempJob);


        // 2. populate the npmhms with the IN & OUT data from rooms datas
        CapableSystem capable_system = CapableSystem.Instance;
        RoomData data;
        int room_hash;
        int movable_hash;
        foreach (KeyValuePair<string, RoomData> pair in rooms_data)
        {
            room_hash = GetRoomHashFromID(pair.Key);
            if (room_hash == 0) { continue; }
            data = pair.Value;

            // we add range IN to IN
            for (int i=0; i<data.IN_movables_ids.Count; i++)
            {
                movable_hash = capable_system.GetCapableHashFromID(data.IN_movables_ids[i]);
                if (movable_hash == 0) { continue; }
                rooms_movables_IN.Add(room_hash, movable_hash);
            }

            // and OUT
            for (int i = 0; i < data.OUT_movables_ids.Count; i++)
            {
                movable_hash = capable_system.GetCapableHashFromID(data.OUT_movables_ids[i]);
                if (movable_hash == 0) { continue; }
                rooms_movables_OUT.Add(movable_hash, room_hash);
            }
        }

        // 3. create and execute the job :)
        HandleRoomTransitionsJob job = new HandleRoomTransitionsJob
        {
            movables_going_IN = rooms_movables_IN,
            movables_going_OUT = rooms_movables_OUT,
            transitions = transitions            
        };
        JobHandle handle = job.Schedule();
        handle.Complete();

        // 4. apply room transitions to managed room data
        bool perso_changed_room = false;
        RoomData perso_new_room = null;
        string controlled_id = Controller.Instance.ControlledID;
        int not_valid_transitions = 0;
        for (int i=0; i<transitions.Length; i++)
        {
            MovableRoomTransition transition = transitions[i];
            string movable_id = capable_system.GetCapableIDFromHash(transition.movable_hash);
            string out_room_id = GetRoomIDFromHash(transition.from_room_index);
            string in_room_id = GetRoomIDFromHash(transition.to_room_index);
            if (movable_id == null || out_room_id == null || in_room_id == null) { not_valid_transitions++; continue; }
            if (!rooms_data.TryGetValue(out_room_id, out RoomData out_room)) { not_valid_transitions++; continue; }
            if (!rooms_data.TryGetValue(in_room_id, out RoomData in_room)) { not_valid_transitions++; continue; }

            if (log_room_transfers) { Debug.Log($"(RoomSystem) [{out_room.id}] >> {movable_id} >> [{in_room.id}]"); }

            // OUT room: transition consumed, movable is no longer inside that room.
            out_room.movables_ids.Remove(movable_id);
            out_room.OUT_movables_ids.Remove(movable_id);

            // IN room: transition consumed, movable is now inside that room.
            if (!in_room.movables_ids.Contains(movable_id)) { in_room.movables_ids.Add(movable_id); }
            in_room.IN_movables_ids.Remove(movable_id);

            if (movable_id == controlled_id)
            {
                perso_changed_room = true;
                perso_new_room = in_room;
            }
        }
        if (log_room_transfers && not_valid_transitions > 0) { Debug.LogWarning($"(RoomSystem) {not_valid_transitions} transitions were not valid during this tick"); }

        // 5. dispose native collections
        rooms_movables_IN.Dispose();
        rooms_movables_OUT.Dispose();
        transitions.Dispose();

        // 6. find rooms to load / unload based on new controlled room neighbours.
        if (!perso_changed_room || perso_new_room == null) { return; }
        Stack<string> rooms_to_unload = new Stack<string>();
        Stack<string> rooms_to_load = new Stack<string>();
        List<string> new_neighbours_ids = GetNeighboursIDs(perso_new_room);

        foreach (string loaded_room_id in loaded_rooms_data.Keys)
        {
            if (loaded_room_id == perso_new_room.id) { continue; }
            if (!new_neighbours_ids.Contains(loaded_room_id)) { rooms_to_unload.Push(loaded_room_id); }
        }

        for (int i = 0; i < new_neighbours_ids.Count; i++)
        {
            string neighbour_id = new_neighbours_ids[i];
            if (neighbour_id == perso_new_room.id) { continue; }
            if (!loaded_rooms_data.ContainsKey(neighbour_id)) { rooms_to_load.Push(neighbour_id); }
        }

        main_room_data = perso_new_room;

        // 7. load the new rooms and unload old ones.
        loadRooms(rooms_to_load.ToArray());
        unloadRooms(rooms_to_unload.ToArray());
    }



    // NEIGHBOURS MANAGEMENT
    public List<string> GetNeighboursIDs(RoomData perso_new_room)
    {
        List<string> neighbours_ids = new List<string>();
        for (int i = 0; i < perso_new_room.neighbours_ids.Count; i++)
        {
            string neighbour_id = perso_new_room.neighbours_ids[i];
            neighbours_ids.Add(neighbour_id);
        }
        if (log_neighbours)
        {
            string log = "(RoomSystem) Neighbours of " + perso_new_room.id + ": ";
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
                log += $"  - {neighbour_id} (data was not found)\n";
                continue;
            }

            log += $"  - {neighbour_id}\n";
            neigh_datas.Add(rooms_data[neighbour_id] as RoomData);
        }

        if (log_neighbours) { Debug.Log($"(RoomSystem) Neighbours of {room.id} : {neigh_datas.Count}\n{log}"); }
        return neigh_datas;
    }

    // CAPABLE'S ROOM DYNAMIC MANAGEMENT
    protected void handleCapableNeedRoom(Capable entity, Capable spawner)
    {
        // we want the entity capabledata to be set inside the same room as the spawner.
        // we need to find in which room the spawner is, and set the entity capabledata in the same room

        // if (log_spawning) { Debug.Log($"(RoomSystem) Handling spawn of {entity.data.id} by spawner {spawner.data.id}"); }
        string id = entity.data.id;

        // 1. find spawner room
        RoomData spawner_room = GetCapableRoom(spawner.data.id);
        if (spawner_room == null)
        {
            if (!hide_log_no_room_of_capable_found) { Debug.LogWarning("(RoomSystem) Could not find spawner room for capable " + spawner.data.id); }
            return;
        }

        // 2. remove all apparitions of this capable in the room + their neighbours (to be sure)
        List<RoomData> rooms = GetNeighboursData(spawner_room);
        rooms.Add(spawner_room);
        foreach (RoomData room in rooms) { remove_all_apparitions_of_capable(room, id); }

        // 3. attach entity data to the room
        if (entity is Movable) { spawner_room.movables_ids.Add(id); }
        else { spawner_room.capables_ids.Add(id); }


        if (log_dynamic_room_assignement) { Debug.Log($"(RoomSystem) Assigned {id} to {spawner_room.id}"); }
    }
    protected void handleCapableNeedFreedom(Capable entity, Capable grabber)
    {
        // entity was probably grabbed by grabber, and so entity has no colliders
        // it means we want to take it out of the system otherwise entity may change
        // rooms even if no movement was detected by the RoomSystem

        // we get the room of entity (if it exists)
        string id = entity.data.id;

        // 1. find entity room
        RoomData room = GetCapableRoom(id);
        if (room == null)
        {
            if (!hide_log_no_room_of_capable_found) { Debug.LogWarning("(RoomSystem) Could not find room of capable " + id); }
            return;
        }

        // 2. remove all apparitions of this capable in the room + their neighbours (to be sure)
        List<RoomData> rooms = GetNeighboursData(room);
        rooms.Add(room);
        foreach (RoomData room_data in rooms) { remove_all_apparitions_of_capable(room_data, id); }

        // 2. detach entity data from the room
        /* if (entity is Movable) { room.movables_ids.Remove(id); }
        else { room.capables_ids.Remove(id); }

        // 3. check if the entity is somewhere else in the room
        if (room.OUT_movables_ids.Contains(id)) { room.OUT_movables_ids.Remove(id); }
        if (room.IN_movables_ids.Contains(id)) { room.IN_movables_ids.Remove(id); } */

        if (log_dynamic_room_assignement) { Debug.Log($"(RoomSystem) Detached {id} from {room.id}"); }
    }
    private void remove_all_apparitions_of_capable(RoomData room, string id)
    {
        room.movables_ids.RemoveAll(ID => ID == id);
        room.capables_ids.RemoveAll(ID => ID == id);
        room.IN_movables_ids.RemoveAll(ID => ID == id);
        room.OUT_movables_ids.RemoveAll(ID => ID == id);
    }

    // GETTERS
    private int generate_room_hash(string id)
    {
        if (string.IsNullOrEmpty(id)) { return 0; }
        if (rooms_hashs_by_ids.TryGetValue(id, out int existing)) { return existing; }

        int new_hash = next_room_hash++;
        rooms_hashs_by_ids[id] = new_hash;
        rooms_ids_by_hash[new_hash] = id;
        return new_hash;
    }
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
        ICollection rooms_ids = rooms_data.Keys;
        foreach (string room_id in rooms_ids)
        {
            // if (log_spawning) { Debug.Log($"(RoomSystem) Checking room {room_id} for spawner {spawner.data.id}"); }
            RoomData data = rooms_data[room_id] as RoomData;
            bool is_in_room = false;
            if (data.capables_ids.Contains(capable_id)) { is_in_room = true; }
            else if (data.movables_ids.Contains(capable_id)) { is_in_room = true; }
            if (!is_in_room) { continue; }

            // if (log_spawning) { Debug.Log($"(RoomSystem) Found spawner {spawner.data.id} in room {room_id}"); }
            return data;
        }
        return null;
    }
}
