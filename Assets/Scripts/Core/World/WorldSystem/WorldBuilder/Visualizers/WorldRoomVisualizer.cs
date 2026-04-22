using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class WorldRoomVisualizer : MonoBehaviour
{
    private List<WorldCellVisualizer> cells = new List<WorldCellVisualizer>();
    private List<WorldLinkVisualizer> links = new List<WorldLinkVisualizer>();
    public List<WorldCellVisualizer> Cells { get { return cells; } }
    public List<WorldLinkVisualizer> Links { get { return links; } }

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


    // CREATE ROOM
    private static int room_count = 0;
    public static string NextRoomName { get { return $"room_{room_count}"; } }
    public void CreateRoom(List<WorldCellVisualizer> cells, List<WorldLinkVisualizer> links, string id = "")
    {
        // unregister from previous cells if there is any
        unregister_callbacks();
        reset_color();

        this.cells = new List<WorldCellVisualizer>(cells);
        this.links = new List<WorldLinkVisualizer>(links);

        // set color of cells and links
        set_color();

        // register to new cells
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
        foreach (var c in cells) { c.Color = WorldBuilder.StaticInstance.LinkedColor; }
        foreach (var l in links) { l.Color = WorldBuilder.StaticInstance.LinkedColor; }
    }
    private void reset_color()
    {
        foreach (var c in cells)
        {
            if (c == null) { continue; }
            if (c.IsPartOfRoom()) { c.Color = WorldBuilder.StaticInstance.LinkedColor; }
            else { c.Color = WorldBuilder.StaticInstance.WaitingColor; }
        }
        foreach (var l in links)
        {
            if (l == null) { continue; }
            if (l.CellA.IsPartOfRoom() && l.CellB.IsPartOfRoom()) { l.Color = WorldBuilder.StaticInstance.LinkedColor; }
            else { l.Color = WorldBuilder.StaticInstance.WaitingColor; }
        }
    }

    // callbacks
    private void register_callback(WorldCellVisualizer cell)
    {
        if (cell == null) { return; }
        cell.OnRemoved += remove_ourself;
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) registered to cell {cell}"); }
    }
    private void register_callbacks()
    {
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) registering to all cells"); }
        foreach (var c in cells) { register_callback(c); }
    }
    private void unregister_callback(WorldCellVisualizer cell)
    {
        if (cell == null) { return; }
        cell.OnRemoved -= remove_ourself;
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) unregistered from cell {cell}"); }
    }
    private void unregister_callbacks()
    {
        if (log_callbacks) { Debug.Log($"(WorldRoomVisualizer) unregistering from all cells"); }
        foreach (var c in cells) { unregister_callback(c); }
    }
    private void remove_ourself(WorldCellVisualizer cell_visu)
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
    public bool HasCell(WorldCellVisualizer cell)
    {
        if (going_to_be_destroyed) { return false; }
        return cells.Contains(cell);
    }
    public bool IsEqualTo(List<WorldCellVisualizer> cells, List<WorldLinkVisualizer> links)
    {
        return this.cells.Intersect(cells).Count() == this.cells.Count
            && this.links.Intersect(links).Count() == this.links.Count;
    }
    public List<Vector3Int> GetLoopCells()
    {
        return cells.Select(c => c.CurrentCell).ToList();
    }
    public Vector2[] GetPath()
    {
        return cells.Select(c => (Vector2)c.transform.position).ToArray();
    }
    public Vector2[] GetWorldPath()
    {
        return cells.Select(c => (Vector2)c.transform.position - (Vector2)transform.position).ToArray();
    }
}