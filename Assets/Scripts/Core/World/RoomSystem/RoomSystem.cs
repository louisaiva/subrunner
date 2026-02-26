using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSystem : BSOD_System<RoomSystem>
{
    [Header("Rooms data")]
    private string data_path = "Assets/Resources/data/rooms/";
    public List<RoomData> rooms_data = new List<RoomData>();
    public List<RoomData> loaded_rooms_data = new List<RoomData>();
    public RoomData main_room_data; // the main room is the one where the perso is, we need to keep track of it to know which room to load when the perso changes room

    [Header("Loading parameters")]
    public bool awake_done = false;
    public bool start_loading_done = false;
    public Transform room_parent;
    public int frames_between_loaded_rooms = 10;
    public int frames_between_ticks = 1;

    [Header("Initialization parameters")]
    public bool init_doing = false;
    public bool init_done = false;
    public int frames_between_room_overlap_checks = 10;

    [Header("Logs")]
    public bool log_init = false;
    public bool log_loading = false;
    public bool log_neighbours = false;
    public bool log_room_transfers = false;
    public bool log_ticks = false;


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
        // we empty the rooms_data
        rooms_data = new List<RoomData>();

        // we load all the json files in the data path and convert them to RoomData objects
        string[] files = System.IO.Directory.GetFiles(data_path, "*.json");
        foreach (string file in files)
        {
            string json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
            RoomData data = JsonUtility.FromJson<RoomData>(json);
            rooms_data.Add(data);
        }

        if (log_loading) { Debug.Log("(RoomSystem) Rooms data loaded: " + rooms_data.Count); }
        awake_done = true;
    }

    // LOAD ROOMS
    public async void LoadRooms(string[] rooms_ids)
    {
        // for each room id we need to find its data and load it
        foreach (string room_id in rooms_ids)
        {
            RoomData data = rooms_data.Find(r => r.id == room_id);
            if (data == null) { Debug.LogWarning("(RoomSystem) Room data not found for id: " + room_id); continue; }

            // we load the room
            load_room(data);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
        start_loading_done = true;
    }
    private async void loadRooms(RoomData[] rooms_data)
    {
        foreach (RoomData data in rooms_data)
        {
            load_room(data);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    private void load_room(RoomData data)
    {
        Room room = RoomBank.Instance.Load(data);
        room.transform.SetParent(room_parent);
        loaded_rooms_data.Add(data);
        if (log_loading) { Debug.Log("(RoomSystem) Room loaded: " + data.id); }
    }

    // UNLOAD ROOMS
    public void UnloadAllRooms()
    {
        unloadRooms(loaded_rooms_data.ToArray());
    }
    private async void unloadRooms(RoomData[] rooms_data)
    {
        foreach (RoomData data in rooms_data)
        {
            unload_room(data);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    private void unload_room(RoomData data)
    {
        Room room = room_parent.GetComponentsInChildren<Room>().ToList().Find(r => r.data == data);
        if (room == null) { Debug.LogWarning("(RoomSystem) Room not found for data: " + data.id); return; }
        RoomBank.Instance.Unload(room);
        loaded_rooms_data.Remove(data);

        if (log_loading) { Debug.Log("(RoomSystem) Room unloaded: " + data.id); }
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

            // todo should we wait at some point ? if we have perf issues yes (drop of fps 1s after game start)
            for (int j = 0; j < frames_between_room_overlap_checks; j++) { await System.Threading.Tasks.Task.Yield(); }
        }

        init_done = true;
        if (log_init) { Debug.Log("(RoomSystem) Init done and started ticking"); }
    }

    // TICK
    protected virtual void Tick()
    {

        // if (log_ticks) { Debug.Log("(RoomSystem) Tick called"); }

        // 1. find all capables that changes room
        RoomData room;
        Dictionary<RoomData, string> movables_IN = new Dictionary<RoomData, string>(); // RoomData, CapableID
        Dictionary<RoomData, string> movables_OUT = new Dictionary<RoomData, string>(); // RoomData, CapableID
        for (int i = 0; i < rooms_data.Count; i++)
        {
            room = rooms_data[i];
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
        for (int i = 0; i < rooms_data.Count; i++)
        {
            room = rooms_data[i];
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


        // store si le perso a été found + next room principale
        bool perso_changed_room = false;
        RoomData perso_new_room = null;

        // 4. prepare the lists for capable loading info to transmit to CapableSystem
        List<string> capables_to_load = new List<string>();
        List<string> capables_to_unload = new List<string>();
        for (int i=0; i<capable_ids.Count; i++)
        {
            string capable_id = capable_ids[i];

            // check if is perso
            if (Controller.Instance.ControlledID == capable_id)
            {
                perso_changed_room = true;
                perso_new_room = in_rooms[i];
                continue;
            }

            // check if need to load/unload
            bool in_room_loaded = loaded_rooms_data.Contains(in_rooms[i]);
            bool out_room_loaded = loaded_rooms_data.Contains(out_rooms[i]);

            if (in_room_loaded && !out_room_loaded)
            {
                capables_to_load.Add(capable_id);
            }
            else if (!in_room_loaded && out_room_loaded)
            {
                capables_to_unload.Add(capable_id);
            }
        }

        // 5. Sens load unload to CapableSystem
        // CapableSystem.Load(capables_to_load)
        // CapableSystem.Unload(capables_to_unload)
        // (for now we log)

        // 6. Handle when perso changed room
        if (!perso_changed_room) { return; }
        List<RoomData> rooms_to_unload = new List<RoomData>();
        List<RoomData> rooms_to_load = new List<RoomData>();
        List<RoomData> new_neighbours = GetNeighbours(perso_new_room);
        for (int i = 0; i < loaded_rooms_data.Count; i++)
        {
            RoomData data = loaded_rooms_data[i];
            if (data == perso_new_room) { continue; }

            // we check if the room is in the new neighbours
            if (!new_neighbours.Contains(data)) { rooms_to_unload.Add(data); }
        }
        for (int i = 0; i < new_neighbours.Count; i++)
        {
            RoomData data = new_neighbours[i];
            if (data == perso_new_room) { continue; }

            // we check if the room is already loaded
            if (!loaded_rooms_data.Contains(data)) { rooms_to_load.Add(data); }
        }

        main_room_data = perso_new_room;

        // 7. We load the new rooms and unload the old ones
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

    // NEIGHBOURS MANAGEMENT
    private List<RoomData> GetNeighbours(RoomData perso_new_room)
    {
        List<RoomData> neighbours = new List<RoomData>();
        for (int i = 0; i < perso_new_room.neighbours_ids.Count; i++)
        {
            string neighbour_id = perso_new_room.neighbours_ids[i];
            RoomData neighbour = rooms_data.Find(r => r.id == neighbour_id);
            if (neighbour != null) { neighbours.Add(neighbour); }
            else if (log_neighbours) { Debug.LogWarning("(RoomSystem) Neighbour room data not found for id: " + neighbour_id); }
        }
        if (log_neighbours)
        {
            string log = "(RoomSystem) Neighbours of " + perso_new_room.id + ": ";
            for (int i = 0; i < neighbours.Count; i++)
            {
                log += neighbours[i].id + " ";
            }
            Debug.Log(log);
        }
        return neighbours;
    }
}