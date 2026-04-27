using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorEngine : MonoBehaviour
{
    private DoorGraph door_graph;
    private List<Door> loaded_doors = new List<Door>();
    [SerializeField] private List<RoomData> visible_rooms = new List<RoomData>();


    [Header("Logs")]
    public bool log_graph = false;
    public bool log_accessible_rooms = false;
    public bool log_masks = false;
    public bool log_callbacks = false;

    // START
    public void Start()
    {
        List<DoorData> doors = CapableSystem.Instance.GetWorldDoorsData();
        createDoorGraph(doors);

        // register to capable bank loading/unloading events to know when doors are loaded/unloaded
        CapableBank.Instance.OnCapableLoaded += HandleDoorLoaded;
        CapableBank.Instance.OnCapableUnloading += HandleDoorUnloaded;

        // first visible rooms update
        UpdateRoomMasks();
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
        if (log_graph) { Debug.Log($"(DoorEngine) Creating link for door id: {door_data.id} between room {door_data.room1_id} and room {door_data.room2_id} (is open: {door_data.is_open})"); }

        // we get the rooms linked to the door
        string room1_id = door_data.room1_id;
        string room2_id = door_data.room2_id;

        // we create the nodes if they don't exist yet
        if (!added_rooms.Contains(room1_id)) { create_room_node(room1_id); added_rooms.Add(room1_id); }
        if (!added_rooms.Contains(room2_id)) { create_room_node(room2_id); added_rooms.Add(room2_id); }

        // we create the link
        RoomLink link = new RoomLink()
        {
            room1 = door_graph.GetRoomNode(room1_id),
            room2 = door_graph.GetRoomNode(room2_id),
            state = door_data.is_open ? LinkState.Open : LinkState.RequireInteraction,
            door_id = door_data.id,
        };

        // we add the link to the graph
        door_graph.links.Add(link);
    }
    private void create_room_node(string room_id)
    {
        if (log_graph) { Debug.Log($"(DoorEngine) Creating node for room id: {room_id}"); }

        // we get the room data
        RoomData data = RoomEngine.Instance.GetRoomDataFromID(room_id);
        if (data == null) { Debug.LogError($"(DoorEngine) Could not find RoomData for room id: {room_id}"); return; }

        RoomNode node = new RoomNode() { data = data };
        door_graph.rooms.Add(node);
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









    // called in 2 situations :
    // - when a door is open/closed
    // - when we enter a room (to update the masks of the doors of the room)
    private void UpdateRoomMasks()
    {
        // get the current room
        RoomData current_room = RoomEngine.Instance.PlayerRoomData;
        if (current_room == null) { Debug.LogError($"(DoorEngine) Could not find current room data"); return; }

        // we get the accessible rooms from the current room
        HashSet<RoomNode> accessible_rooms = door_graph.GetAccessibleNeighbourNodes(current_room.id);
        HashSet<RoomData> accessible_rooms_data = new HashSet<RoomData>(accessible_rooms.Select(node => node.data));

        if (log_accessible_rooms)
        {
            string accessible_log = string.Join(", ", accessible_rooms_data.Select(room => room.id));
            Debug.Log($"(DoorEngine) Accessible rooms from current room {current_room.id}: {accessible_log}");
        }

        // we gather the rooms to update
        List<RoomData> rooms_to_show = new List<RoomData>();
        List<RoomData> rooms_to_hide = new List<RoomData>();
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

        if (log_masks)
        {
            string show_log = string.Join(", ", rooms_to_show.Select(room => room.id));
            string hide_log = string.Join(", ", rooms_to_hide.Select(room => room.id));
            Debug.Log($"(DoorEngine) Updating room masks. Rooms to show: {show_log}. Rooms to hide: {hide_log}");
        }

        // we show the rooms to show and hide the rooms to hide
        foreach (RoomData room in rooms_to_show) { RoomEngine.Instance.TilemapEngine.HideMask(room); }
        foreach (RoomData room in rooms_to_hide) { RoomEngine.Instance.TilemapEngine.ShowMask(room); }

        // we update the visible rooms list
        visible_rooms = accessible_rooms_data.ToList();
    }

    // DOOR CLOSE OPEN HANDLERS
    private void OnDoorOpen(Door door)
    {
        // we mark the link as open in the graph
        RoomLink link = door_graph.GetDoorLink(door.ID);
        if (link == null) { Debug.LogError($"(DoorEngine) Could not find link for door id: {door.ID}"); return; }
        link.state = LinkState.Open;

        UpdateRoomMasks();
    }
    private void OnDoorClose(Door door)
    {
        // we mark the link as closed in the graph
        RoomLink link = door_graph.GetDoorLink(door.ID);
        if (link == null) { Debug.LogError($"(DoorEngine) Could not find link for door id: {door.ID}"); return; }
        link.state = LinkState.RequireInteraction;

        UpdateRoomMasks();
    }



    // INTERNAL CLASSES
    private class DoorGraph
    {
        public List<RoomNode> rooms;
        public List<RoomLink> links;
        private bool hide_log_room_not_found = false;

        public DoorGraph()
        {
            rooms = new List<RoomNode>();
            links = new List<RoomLink>();
        }

        public RoomNode GetRoomNode(string room_id)
        {
            return rooms.Find(node => node.ID == room_id);
        }
        public RoomLink GetDoorLink(string door_id)
        {
            return links.Find(link => link.ID == door_id);
        }

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
    
        public HashSet<RoomNode> GetAccessibleNeighbourNodes(string room_id)
        {
            RoomNode base_node = GetRoomNode(room_id);
            if (base_node == null)
            {
                if (!hide_log_room_not_found) { Debug.LogWarning($"(DoorGraph) Could not find room node for room id: {room_id}"); }
                return new HashSet<RoomNode>();
            }

            HashSet<RoomNode> accessible_rooms = new HashSet<RoomNode>() { base_node };
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
            return accessible_rooms;
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