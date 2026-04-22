using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class WorldRoomVisualizer : MonoBehaviour
{
    private List<WorldNodeVisualizer> nodes = new List<WorldNodeVisualizer>();
    private List<WorldLinkVisualizer> links = new List<WorldLinkVisualizer>();
    public List<WorldNodeVisualizer> Nodes { get { return nodes; } }
    public List<WorldLinkVisualizer> Links { get { return links; } }

    [SerializeField] private List<WorldDoorVisualizer> doors = new List<WorldDoorVisualizer>();
    public List<WorldDoorVisualizer> Doors { get { return doors; } }

    private PolygonCollider2D _collider;
    private PolygonCollider2D polygon_collider
    {
        get
        {
            if (_collider == null) { _collider = GetComponent<PolygonCollider2D>(); }
            return _collider;
        }
    }
    public PolygonCollider2D PolygonCollider { get { return polygon_collider; } }

    private Material _mat;
    private Material material
    {
        get
        {
            if (_mat == null) { _mat = GetComponent<MeshRenderer>().material; }
            return _mat;
        }
    }
    public Color Color
    {
        get { return material.color; }
        set { material.color = value; }
    }



    [Header("Logs")]
    [SerializeField] private bool log_callbacks = false;
    [SerializeField] private bool log_collides = false;


    // CREATE ROOM
    private static int room_count = 0;
    public static string NextRoomName { get { return $"room_{room_count}"; } }
    public void CreateRoom(List<WorldNodeVisualizer> nodes, List<WorldLinkVisualizer> links, string id = "")
    {
        // unregister from previous nodes if there is any
        unregister_callbacks();
        reset_color();

        this.nodes = new List<WorldNodeVisualizer>(nodes);
        this.links = new List<WorldLinkVisualizer>(links);

        // set color of nodes and links
        set_color();

        // register to new nodes
        register_callbacks();


        // create the visu
        polygon_collider.pathCount = 1;
        polygon_collider.SetPath(0, GetPath());

        // we ask for a name and we set it
        if (string.IsNullOrEmpty(id))
        {
            id = $"room_{room_count}";
            room_count++;
        }
        else if (id.StartsWith("room_"))
        {
            int.TryParse(id.Substring(5), out int parsed_count);
            if (parsed_count >= room_count) { room_count = parsed_count + 1; }
        }
        name = id;
    }

    // colors
    private void set_color()
    {
        foreach (var n in nodes) { n.Color = WorldBuilder.StaticInstance.LinkedColor; }
        foreach (var l in links) { l.Color = WorldBuilder.StaticInstance.LinkedColor; }
    }
    private void reset_color()
    {
        foreach (var n in nodes)
        {
            if (n == null) { continue; }
            if (n.IsPartOfRoom()) { n.Color = WorldBuilder.StaticInstance.LinkedColor; }
            else { n.Color = WorldBuilder.StaticInstance.WaitingColor; }
        }
        foreach (var l in links)
        {
            if (l == null) { continue; }
            if (l.NodeA.IsPartOfRoom() && l.NodeB.IsPartOfRoom()) { l.Color = WorldBuilder.StaticInstance.LinkedColor; }
            else { l.Color = WorldBuilder.StaticInstance.WaitingColor; }
        }
    }

    // callbacks
    private void register_callbacks()
    {
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) registering to all cells"); }
        foreach (var n in nodes) { register_node_callback(n); }
        foreach (var d in doors) { register_door(d); }
    }
    private void unregister_callbacks()
    {
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) unregistering from all cells"); }
        foreach (var n in nodes) { unregister_node_callback(n); }
        foreach (var d in doors) { unregister_door(d); }
    }
    
    
    // doors callbacks
    private void register_door(WorldDoorVisualizer door)
    {
        if (door == null) { return; }
        door.OnRemoved += RemoveDoor;
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) registered to door {door}"); }
    }
    private void unregister_door(WorldDoorVisualizer door)
    {
        if (door == null) { return; }
        door.OnRemoved -= RemoveDoor;
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) unregistered from door {door}"); }
    }


    // nodes callbacks
    private void register_node_callback(WorldNodeVisualizer node)
    {
        if (node == null) { return; }
        node.OnRemoved += remove_ourself;
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) registered to cell {node}"); }
    }
    private void unregister_node_callback(WorldNodeVisualizer node)
    {
        if (node == null) { return; }
        node.OnRemoved -= remove_ourself;
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) unregistered from cell {node}"); }
    }
    private void remove_ourself(WorldCellVisualizer node)
    {
        going_to_be_destroyed = true;
        reset_color();
        Destroy(gameObject);
    }
    private bool going_to_be_destroyed = false;
    private void OnDestroy()
    {
        unregister_callbacks();
    }

    // GETTERS
    public bool HasNode(WorldNodeVisualizer node)
    {
        if (going_to_be_destroyed) { return false; }
        return nodes.Contains(node);
    }
    public bool IsEqualTo(List<WorldNodeVisualizer> nodes, List<WorldLinkVisualizer> links)
    {
        return this.nodes.Intersect(nodes).Count() == this.nodes.Count
            && this.links.Intersect(links).Count() == this.links.Count;
    }
    public List<Vector3Int> GetLoopCells()
    {
        return nodes.Select(n => n.Cell).ToList();
    }
    public Vector2[] GetPath()
    {
        return nodes.Select(n => (Vector2)n.transform.position).ToArray();
    }
    public Vector2[] GetWorldPath()
    {
        return nodes.Select(n => (Vector2)n.transform.position - (Vector2)transform.position).ToArray();
    }
    
    private static float circle_cast_radius = 0.35f;
    public bool CollideWithCell(Vector3Int cell_pos)
    {
        if (log_collides) { Debug.Log($"(WorldRoomVisualizer) checking collision with cell {cell_pos}"); }

        // do a circle cast with small radius to check if the cell collides
        Vector2 world_pos = WorldBuilder.StaticInstance.Grid.CellToWorld(cell_pos);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(world_pos, circle_cast_radius, Vector2.zero, 0f, LayerMask.GetMask("WorldBuilder"));
        if (log_collides) { Debug.Log($"(WorldRoomVisualizer) found {hits.Length} hits : \n - {string.Join("\n - ", hits.Select(h => h.collider.name))}"); }
        foreach (var hit in hits)
        {
            if (hit.collider != polygon_collider) { continue; }            
            return true;
        }
        return false;
    }

    // DOORS
    public void AddDoor(WorldDoorVisualizer door)
    {
        if (doors.Contains(door)) { return; }
        doors.Add(door);
        register_door(door);
    }
    public void RemoveDoor(WorldCellVisualizer door) { RemoveDoor(door as WorldDoorVisualizer); }
    public void RemoveDoor(WorldDoorVisualizer door)
    {
        if (!doors.Contains(door)) { return; }
        doors.Remove(door);
        unregister_door(door);
    }
}