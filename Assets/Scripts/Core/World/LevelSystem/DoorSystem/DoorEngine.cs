using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DoorEngine : MonoBehaviour
{
    private DoorGraph door_graph;
    private HashSet<Door> loaded_doors = new HashSet<Door>();
    [SerializeField] private HashSet<RoomData> visible_rooms = new HashSet<RoomData>();


    [Header("Logs")]
    public bool log_graph = false;
    public bool log_accessible_rooms = false;
    public bool log_visibility_update = false;
    public bool log_callbacks = false;
    public bool log_visibility = false;

    // LOAD / UNLOAD WORLD DATA
    public async Task LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(DoorEngine) Loading world data for world_id: {world_id}"); }

        List<DoorData> doors = CapableEngine.Instance.GetWorldDoorsData();
        if (log) { Debug.Log($"(DoorEngine) Gathered {doors.Count} doors data from CapableSystem"); }
        createDoorGraph(doors);
        if (log) { Debug.Log($"(DoorEngine) Created door graph : {door_graph.GetDoorGraphDetails()}"); }

        // register to capable bank loading/unloading events to know when doors are loaded/unloaded
        CapableBank.Instance.OnCapableLoaded += HandleDoorLoaded;
        CapableBank.Instance.OnCapableUnloading += HandleDoorUnloaded;

        // register to room engine on capable added to room
        ChunkEngine.Instance.OnCapableAddedToRoom += on_capable_enter_chunk;
        ChunkEngine.Instance.OnPlayerChunkChange += UpdateRoomsVisibility;
        if (log) { Debug.Log($"(DoorEngine) Registered callbacks to CapableBank and RoomEngine events"); }

        UpdateRoomsVisibility();
        if (log) { Debug.Log($"(DoorEngine) Updated rooms visibility"); }

        if (log) { Debug.Log($"(DoorEngine) DOOR ENGINE SUCCESSFULLY LOADED : {world_id}"); }
    }
    public async Task UnloadWorldData(bool log)
    {
        CapableBank.Instance.OnCapableLoaded -= HandleDoorLoaded;
        CapableBank.Instance.OnCapableUnloading -= HandleDoorUnloaded;
        ChunkEngine.Instance.OnCapableAddedToRoom -= on_capable_enter_chunk;
        ChunkEngine.Instance.OnPlayerChunkChange -= UpdateRoomsVisibility;

        door_graph = null;
        loaded_doors.Clear();
        visible_rooms.Clear();
        if (log) { Debug.Log($"(DoorEngine) DOOR ENGINE SUCCESSFULLY UNLOADED"); }
    }


    // GRAPH CREATION
    private void createDoorGraph(List<DoorData> doors)
    {
        if (log_graph) { Debug.Log($"(DoorEngine) Creating door graph with {doors.Count} doors"); }
        door_graph = new DoorGraph();
        List<string> added_rooms = new List<string>();

        // we grab all the doors in the world and create a graph from them
        foreach (DoorData door in doors) { create_door_link(door, ref added_rooms); }
    }
    private void create_door_link(DoorData door_data, ref List<string> added_rooms)
    {
        if (log_graph) { Debug.Log($"(DoorEngine) Creating link for door id: {door_data.id} between chunk {door_data.room1_id} and chunk {door_data.room2_id} (is open: {door_data.is_open})"); }

        // convert to their room data
        RoomData room_1 = RoomEngine.Instance.GetRoomDataFromID(door_data.room1_id);
        if (room_1 == null) { Debug.LogError($"(DoorEngine) Could not find RoomData for room id: {door_data.room1_id}"); return; }
        RoomData room_2 = RoomEngine.Instance.GetRoomDataFromID(door_data.room2_id);
        if (room_2 == null) { Debug.LogError($"(DoorEngine) Could not find RoomData for room id: {door_data.room2_id}"); return; }

        // we create the nodes if they don't exist yet
        if (!added_rooms.Contains(room_1.id)) { create_room_node(room_1); added_rooms.Add(room_1.id); }
        if (!added_rooms.Contains(room_2.id)) { create_room_node(room_2); added_rooms.Add(room_2.id); }

        // we create the link
        RoomLink link = new RoomLink()
        {
            room1 = door_graph.GetRoomNode(room_1.id),
            room2 = door_graph.GetRoomNode(room_2.id),
            state = door_data.is_open ? LinkState.Open : LinkState.RequireInteraction,
            door_id = door_data.id,
        };

        // we add the link to the graph
        door_graph.links.Add(link);
    }
    private void create_room_node(RoomData room_data)
    {
        RoomNode node = new RoomNode() { data = room_data };
        door_graph.rooms.Add(node);
        if (log_graph) { Debug.Log($"(DoorEngine) Created node for room : {room_data.id}"); }
    }


    // DOORS CALLBACKS
    private void HandleDoorLoaded(Capable capable)
    {
        if (capable is not Door door) { return; }
        loaded_doors.Add(door);
        register_door_callbacks(door);
    }
    private void HandleDoorUnloaded(Capable capable)
    {
        if (capable is not Door door) { return; }
        loaded_doors.Remove(door);
        unregister_door_callbacks(door);
    }
    private void register_door_callbacks(Door door)
    {
        door.OnDoorOpen += OnDoorOpen;
        door.OnDoorClose += OnDoorClose;
        if (log_callbacks) { Debug.Log($"(DoorEngine) Registered callbacks for door id: {door.ID}"); }
    }
    private void unregister_door_callbacks(Door door)
    {
        door.OnDoorOpen -= OnDoorOpen;
        door.OnDoorClose -= OnDoorClose;
        if (log_callbacks) { Debug.Log($"(DoorEngine) Unregistered callbacks for door id: {door.ID}"); }
    }



    // DOOR CLOSE OPEN HANDLERS
    private void OnDoorOpen(Door door)
    {
        // we mark the link as open in the graph
        RoomLink link = door_graph.GetDoorLink(door.ID);
        if (link == null) { Debug.LogError($"(DoorEngine) Could not find link for door id: {door.ID}"); return; }
        link.state = LinkState.Open;

        UpdateRoomsVisibility();
    }
    private void OnDoorClose(Door door)
    {
        // we mark the link as closed in the graph
        RoomLink link = door_graph.GetDoorLink(door.ID);
        if (link == null) { Debug.LogError($"(DoorEngine) Could not find link for door id: {door.ID}"); return; }
        link.state = LinkState.RequireInteraction;

        UpdateRoomsVisibility();
    }


    // called in 2 situations :
    // - when a door is open/closed
    // - when we enter a room (to update the masks of the doors of the room)
    private readonly List<RoomData> rooms_to_show = new List<RoomData>();
    private readonly List<RoomData> rooms_to_hide = new List<RoomData>();
    private HashSet<RoomNode> accessible_rooms = new HashSet<RoomNode>();
    private readonly HashSet<RoomData> accessible_rooms_data = new HashSet<RoomData>();
    private void UpdateRoomsVisibility(ChunkData room) => UpdateRoomsVisibility();
    private void UpdateRoomsVisibility()
    {
        // get the current room
        ChunkData current_chunk = ChunkEngine.Instance.PlayerChunkData;
        if (current_chunk == null || string.IsNullOrEmpty(current_chunk.id)) { Debug.LogWarning($"(DoorEngine) Could not find current chunk data"); return; }
        else if (string.IsNullOrEmpty(current_chunk.room_id)) { Debug.LogError($"(DoorEngine) Current chunk data ({current_chunk.id}) does not have a room id"); return; }
        RoomData current_room = RoomEngine.Instance.GetRoomDataFromID(current_chunk.room_id);
        if (current_room == null) { Debug.LogError($"(DoorEngine) Could not find current room data"); return; }

        if (visible_rooms is null) { visible_rooms = new HashSet<RoomData>(); }

        // we get the accessible rooms from the current room
        accessible_rooms.Clear();
        accessible_rooms_data.Clear();
        door_graph.GetAccessibleNeighbourNodes(current_room.id, ref accessible_rooms);
        accessible_rooms_data.UnionWith(accessible_rooms.Select(node => node.data));

        if (log_accessible_rooms)
        {
            string accessible_log = string.Join(", ", accessible_rooms_data.Select(room => room.id));
            Debug.Log($"(DoorEngine) Accessible rooms from current room {current_room.id}: {accessible_log}");
        }

        // we gather the rooms to update
        rooms_to_show.Clear();
        rooms_to_hide.Clear();
        foreach (RoomData room in accessible_rooms_data)
        {
            if (visible_rooms.Contains(room)) { continue; }
            rooms_to_show.Add(room);
        }
        foreach (RoomData data in visible_rooms)
        {
            if (accessible_rooms_data.Contains(data)) { continue; }
            rooms_to_hide.Add(data);
        }

        if (log_visibility_update)
        {
            string show_log = string.Join(", ", rooms_to_show.Select(room => room.id));
            string hide_log = string.Join(", ", rooms_to_hide.Select(room => room.id));
            Debug.Log($"(DoorEngine) Updating room visibility. Rooms to show: {show_log}. Rooms to hide: {hide_log}");
        }

        // we update the visible rooms list
        visible_rooms.Clear();
        visible_rooms.UnionWith(accessible_rooms_data);

        // we show the rooms to show and hide the rooms to hide
        // todo : THIS METHOD SHOWS ALL TILEMAPS IF WE HAVE SOME, even if the room is not loaded !
        // todo not harmful rn (since it only affects the tilemaps and does not trigger the capable/room loading) but may introduce future bugs
        foreach (RoomData room in rooms_to_show) { ShowRoom(room); }
        foreach (RoomData room in rooms_to_hide) { HideRoom(room); }
    }


    // ROOM SHOW / HIDE

    /// <summary>
    /// these 2 methods are NOT supposed to modify visible_rooms list.
    /// visible_rooms is the only truth, and so it must be checked BEFORE
    /// calling these methods
    /// </summary>
    /// <param name="room_data"></param>
    public void ShowRoom(string room_id)
    {
        RoomData room_data = RoomEngine.Instance.GetRoomDataFromID(room_id);
        if (room_data == null) { Debug.LogError($"(DoorEngine) Could not find RoomData for room id: {room_id}"); return; }
        ShowRoom(room_data);
    }
    public void ShowRoom(RoomData room_data)
    {
        RoomEngine.Instance.ShowTilemaps(room_data);

        // show the lights
        ChunkEngine.Instance.LightsEngine.ShowLights(room_data.chunks_ids);

        // if none of the chunks of the room are loaded, we can't show the capable anyway so we return early
        // ! maybe we still need to show the doors even if the chunk is not loaded ???
        if (!RoomEngine.Instance.IsRoomLoaded(room_data)) { return; }

        // show all the capables
        List<CapableData> capables_data = RoomEngine.Instance.GetCapablesDataInRoom(room_data);
        foreach (CapableData data in capables_data)
        {
            if (data.Capable == null || data.Capable.AnimPlayer == null)
            {
                if (log_visibility) { Debug.LogWarning($"(DoorEngine) Capable {data.id} in room {room_data.id} {(data.Capable != null ? "has null AnimPlayer" : "is not loaded, we don't show it")}"); }
                continue;
            }
            if (log_visibility) { Debug.Log($"(DoorEngine) Showing capable {data.id} in room {room_data.id}"); }
            data.Capable.AnimPlayer.Show();
        }

        // show all the doors
        List<Door> doors = GetRoomDoors(room_data);
        foreach (Door door in doors) { door.AnimPlayer.Show(); }
    }

    /// <summary>
    /// these 2 methods are NOT supposed to modify visible_rooms list.
    /// visible_rooms is the only truth, and so it must be checked BEFORE
    /// calling these methods
    /// </summary>
    /// <param name="room_data"></param>
    public void HideRoom(string room_id)
    {
        RoomData room_data = RoomEngine.Instance.GetRoomDataFromID(room_id);
        if (room_data == null) { Debug.LogError($"(DoorEngine) Could not find RoomData for room id: {room_id}"); return; }
        HideRoom(room_data);
    }
    public void HideRoom(RoomData room_data)
    {
        RoomEngine.Instance.HideTilemaps(room_data); // hide the tilemaps

        // hide the lights
        ChunkEngine.Instance.LightsEngine.HideLights(room_data.chunks_ids);

        // if none of the chunks of the room are loaded, no need to hide the capables since they are not loaded either
        if (!RoomEngine.Instance.IsRoomLoaded(room_data)) { return; } 

        // hide all the capables
        List<CapableData> capables_data = RoomEngine.Instance.GetCapablesDataInRoom(room_data);
        foreach (CapableData data in capables_data)
        {
            // skip the doors bcz we do it manually after
            if (data is DoorData) { continue; }
            if (data.Capable == null || data.Capable.AnimPlayer == null) { continue; }
            data.Capable.AnimPlayer.Hide();
        }

        // hide the doors linked to the room if the other room linked to the door is not visible
        List<Door> doors = GetRoomDoors(room_data);
        foreach (Door door in doors)
        {
            RoomLink link = door_graph.GetDoorLink(door.ID);
            if (link == null) { Debug.LogError($"(DoorEngine) Could not find link for door id: {door.ID}"); continue; }
            RoomNode other_room = link.room1.ID == room_data.id ? link.room2 : link.room1;
            if (visible_rooms.Contains(other_room.data)) { continue; }

            door.AnimPlayer.Hide();
        }
    }


    // CAPABLES ADDED/REMOVED FROM ROOMS HANDLERS
    private void on_capable_enter_chunk(string capid, ChunkData chunk_data)
    {
        CapableData capable_data = CapableEngine.Instance.GetCapableDataFromID(capid);
        if (capable_data == null) { return; }
        Capable capable = capable_data.Capable;
        if (capable == null) { return; }
        RoomData room_data = RoomEngine.Instance.GetRoomDataFromID(chunk_data.room_id);
        if (room_data == null) { return; }
        
        bool capable_visible = capable.AnimPlayer.IsVisible();
        bool room_visible = visible_rooms.Contains(room_data);
        if (capable_visible && !room_visible) { capable.AnimPlayer.Hide(); }
        else if (!capable_visible && room_visible) { capable.AnimPlayer.Show(); }
    }


    // GETTERS
    public bool IsRoomVisible(string room_id)
    {
        RoomData room_data = RoomEngine.Instance.GetRoomDataFromID(room_id);
        if (room_data == null) { return false; }
        return IsRoomVisible(room_data);
    }
    public bool IsRoomVisible(RoomData room_data)
    {
        if (visible_rooms is null) { return true; } // not loaded yet, we return true so all rooms are shown on world loading
        return visible_rooms.Contains(room_data);
    }
    public List<Door> GetRoomDoors(RoomData room_data)
    {
        List<string> door_ids = door_graph.GetDoorIDsLinkedToRoom(room_data.id);
        List<Door> doors = new List<Door>();
        foreach (string door_id in door_ids)
        {
            Door door = loaded_doors.FirstOrDefault(d => d.ID == door_id);
            if (door != null) { doors.Add(door); }
        }
        return doors;
    }




    // INTERNAL CLASSES
    private class DoorGraph
    {
        public List<RoomNode> rooms;
        public List<RoomLink> links;
        private bool hide_log_room_not_found = false;

        // constructor
        public DoorGraph()
        {
            rooms = new List<RoomNode>();
            links = new List<RoomLink>();
        }

        // get room/link
        public RoomNode GetRoomNode(string room_id)
        {
            return rooms.Find(node => node.ID == room_id);
        }
        public RoomLink GetDoorLink(string door_id)
        {
            return links.Find(link => link.ID == door_id);
        }

        // get all doors linked to a room
        public List<string> GetDoorIDsLinkedToRoom(string room_id)
        {
            List<string> door_ids = new List<string>();
            foreach (RoomLink link in links)
            {
                if (link.room1.ID == room_id || link.room2.ID == room_id) { door_ids.Add(link.ID); }
            }
            return door_ids;
        }

        // get neighbours
        public List<RoomNode> GetNeighbourNodes(string room_id)
        {
            List<RoomNode> neighbours = new List<RoomNode>();
            foreach (RoomLink link in links)
            {
                if (link.room1.ID == room_id) { neighbours.Add(link.room2); }
                else if (link.room2.ID == room_id) { neighbours.Add(link.room1); }
            }
            return neighbours;
        }
        public List<RoomNode> GetOpenNeighbourNodes(string room_id)
        {
            List<RoomNode> neighbours = new List<RoomNode>();
            foreach (RoomLink link in links)
            {
                if (link.state != LinkState.Open) { continue; }
                if (link.room1.ID == room_id) { neighbours.Add(link.room2); }
                else if (link.room2.ID == room_id) { neighbours.Add(link.room1); }
            }
            return neighbours;
        }
        public void GetAccessibleNeighbourNodes(string room_id, ref HashSet<RoomNode> accessible_rooms)
        {
            RoomNode base_node = GetRoomNode(room_id);
            if (base_node == null)
            {
                if (!hide_log_room_not_found) { Debug.LogWarning($"(DoorGraph) Could not find room node for room id: {room_id}"); }
                return;
            }

            accessible_rooms.Clear();
            accessible_rooms.Add(base_node);
            List<RoomNode> visited = new List<RoomNode>();
            Queue<RoomNode> queue = new Queue<RoomNode>();
            queue.Enqueue(base_node);

            while (queue.Count > 0)
            {
                RoomNode current = queue.Dequeue();
                visited.Add(current);

                List<RoomNode> accessible_neighbours = GetOpenNeighbourNodes(current.ID);
                foreach (RoomNode neighbour in accessible_neighbours)
                {
                    if (visited.Contains(neighbour)) { continue; }
                    if (queue.Contains(neighbour)) { continue; }
                    accessible_rooms.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }
        }
    
        // get details
        public string GetDoorGraphDetails()
        {
            string details = $"DoorGraph with {rooms.Count} rooms and {links.Count} links\n";
            details += "Rooms:\n";
            foreach (RoomNode node in rooms)
            {
                details += $"- Room id: {node.ID}\n";
            }
            details += "Links:\n";
            foreach (RoomLink link in links)
            {
                if (link == null) { details += "- Link is null\n"; continue; }
                if (link.room1 == null) { details += $"- Link id: {link.ID} has null room1\n"; continue; }
                if (link.room2 == null) { details += $"- Link id: {link.ID} has null room2\n"; continue; }
                details += $"- Link id: {link.ID}, between room {link.room1.ID} and room {link.room2.ID}, state: {link.state}\n";
            }
            return details;
        }
    }
    private class RoomNode
    {
        public RoomData data;
        public string ID => data.id;
    }
    private class RoomLink
    {
        public RoomNode room1;
        public RoomNode room2;
        public LinkState state; // todo update the state dynamically
        public string door_id;
        public string ID => door_id;
    }
    private enum LinkState
    {
        Open,
        RequireInteraction,
    }
}