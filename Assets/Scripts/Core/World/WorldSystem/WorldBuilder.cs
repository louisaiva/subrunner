using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[RequireComponent(typeof(Grid))]
public class WorldBuilder : Singleton<WorldBuilder>
{
    [SerializeField] private string data_path = "Assets/Resources/data/world_builder/";

    [Header("Grid & Grid Visualizers")]
    private Grid _grid;
    public Grid Grid
    {
        get
        {
            if (_grid == null) { _grid = GetComponent<Grid>(); }
            return _grid;
        }
    }
    private Material _grid_material;
    private Material grid_material
    {
        get
        {
            if (_grid_material == null) { _grid_material = transform.Find("grid_visu").GetComponent<SpriteRenderer>().material; }
            return _grid_material;
        }
    }

    [Header("Cell Visualizers")]
    public WorldCellVisualizer selected_cell_visualizer;
    public WorldCellVisualizer cell_prefab;
    public Transform cell_parent;
    [SerializeField] private List<WorldCellVisualizer> cell_visualizers = new List<WorldCellVisualizer>();
    private WorldCellVisualizer last_added_cell = null;

    [Header("Link Visualizers")]
    public WorldLinkVisualizer link_prefab;
    public Transform link_parent;
    [SerializeField] private List<WorldLinkVisualizer> link_visualizers = new List<WorldLinkVisualizer>();
    private WorldLinkVisualizer selecting_link = null;

    [Header("Room Visualizers")]
    public WorldRoomVisualizer room_prefab;
    public Transform room_parent;
    [SerializeField] private List<WorldRoomVisualizer> room_visualizers = new List<WorldRoomVisualizer>();

    [Header("Colors")]
    public Color WaitingColor = Color.orange;
    public Color LinkedColor = Color.lightBlue;
    public List<Color> RoomColors = new List<Color> { Color.lightPink, Color.lightGreen, Color.paleTurquoise, Color.cyan, Color.magenta };
    private List<Color> used_colors = new List<Color>();


    [Header("Builders")]
    public CarpetBuilder carpet_builder;


    [Header("Logs")]
    [SerializeField] private bool log_cycles = false;
    [SerializeField] private bool log_get_room = false;
    [SerializeField] private bool log_data = false;
    [SerializeField] private bool log_building = false;



    // AWAKE
    protected override void Awake()
    {
        base.Awake();
        grid_material.SetFloat("_CellSize", Grid.cellSize.x);

        used_colors = new List<Color>(RoomColors);

        
        #if UNITY_EDITOR
        LoadData();
        #endif
    }

    // START
    private void Start()
    {
        UI_Manager.Instance.SwitchTo("dev_world_builder");
    }





    // UPDATE
    private void Update()
    {
        // check if we have a navigator and if it has a hovered ui element
        if (UI_Navigator.Instance.IsHoveringSlot)
        {
            if (selected_cell_visualizer.gameObject.activeSelf) { selected_cell_visualizer.gameObject.SetActive(false); }
            return;
        }
        if (!selected_cell_visualizer.gameObject.activeSelf) { selected_cell_visualizer.gameObject.SetActive(true); }


        // check if we have some null or missing visualizers in our lists and remove them
        cell_visualizers.RemoveAll(v => v == null);
        link_visualizers.RemoveAll(v => v == null);
        room_visualizers.RemoveAll(v => v == null);

        // missing
        foreach (var l in link_visualizers)
        {
            try { var pos = l.transform.position; }
            catch (MissingReferenceException)
            {
                // if we have a missing reference exception it means the link has been destroyed but not removed from the list, we remove it from the list
                link_visualizers.Remove(l);
                break;
            }
        }
        foreach (var r in room_visualizers)
        {
            try { var pos = r.transform.position; }
            catch (MissingReferenceException)
            {
                // if we have a missing reference exception it means the room has been destroyed but not removed from the list, we remove it from the list
                room_visualizers.Remove(r);
                break;
            }
        }

        UpdateInputs();
    }
    private void UpdateInputs()
    {
        update_mouse_pos();

        // update clicks, buttons
        update_click();
    }
    private void update_mouse_pos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 world_mouse = Camera.main.ScreenToWorldPoint(mousePos);
        selected_cell_visualizer.SetCell(Grid.WorldToCell(world_mouse));
    }

    // clicks
    private bool holding_left_click = false;
    private bool holding_right_click = false;
    private void update_click()
    {

        // LEFT CLICK (add cell)
        if (Input.GetMouseButtonDown(0) && !holding_right_click)
        {
            selected_cell_visualizer.Color = Color.yellow;
            holding_left_click = true;
        }
        if (Input.GetMouseButtonUp(0) && holding_left_click)
        {
            selected_cell_visualizer.Color = Color.white;
            holding_left_click = false;
            add_cell_at_selected_cell();
        }

        // RIGHT CLICK (remove cell)
        if (Input.GetMouseButtonDown(1) && !holding_left_click)
        {
            selected_cell_visualizer.Color = Color.red;
            holding_right_click = true;
        }
        if (Input.GetMouseButtonUp(1) && holding_right_click)
        {
            selected_cell_visualizer.Color = Color.white;
            holding_right_click = false;
            remove_cell_at_selected_cell();
        }
    }


    // CELLS MANAGEMENT
    private void add_cell_at_selected_cell()
    {
        add_cell_at(SelectedCell);
    }
    private void add_cell_at(Vector3Int cell_pos)
    {

        // we add a new cell if not already here
        WorldCellVisualizer new_cell_visu = GetCellAt(cell_pos);
        if (new_cell_visu == null) { new_cell_visu = create_cell_at(cell_pos); }
        
        // we connect the on-going link to the new cell
        if (selecting_link != null)
        {
            if (GetLinkBetween(selecting_link.CellA, new_cell_visu) != null)
            {
                // if there is already a link between the selecting cell and the new cell it means we want to remove this link instead of creating a cycle
                Destroy(selecting_link.gameObject);
                selecting_link = null;
            }
            else
            {
                // it means we already have a cell selected and we want to link it to the new cell
                selecting_link.SetSecondCell(new_cell_visu);
                selecting_link.Color = WaitingColor;

                // we check if this new link creates a cycle
                handle_potential_cycle_creation(selecting_link);
                selecting_link = null;
            }
        }
        last_added_cell = new_cell_visu;

        // we create a new link visu to link this cell to the next one that will be created if we click on another cell
        selecting_link = create_link_between(new_cell_visu, selected_cell_visualizer, Color.white);
    }
    private WorldCellVisualizer create_cell_at(Vector3Int cell_pos)
    {
        WorldCellVisualizer new_cell_visu = Instantiate(cell_prefab, cell_parent);
        new_cell_visu.SetCell(cell_pos);
        new_cell_visu.Color = WaitingColor;
        cell_visualizers.Add(new_cell_visu);
        return new_cell_visu;
    }
    private void remove_cell_at_selected_cell()
    {
        // check if we have a select link we remove it
        if (selecting_link != null)
        {
            Destroy(selecting_link.gameObject);
            selecting_link = null;
        }

        WorldCellVisualizer cell_to_remove = GetCellAt(SelectedCell);
        if (cell_to_remove == null) { return; }
        
        // remove the cell
        cell_visualizers.Remove(cell_to_remove);
        Destroy(cell_to_remove.gameObject);
    }

    // LINKS MANAGEMENT
    private WorldLinkVisualizer create_link_between(WorldCellVisualizer c1, WorldCellVisualizer c2, Color? color = null)
    {
        WorldLinkVisualizer new_link_visu = Instantiate(link_prefab, link_parent);
        new_link_visu.SetCells(c1, c2);
        new_link_visu.Color = color ?? WaitingColor;
        link_visualizers.Add(new_link_visu);
        return new_link_visu;
    }



    // ROOM (LOOPING NODES) MANAGEMENT
    private WorldRoomVisualizer create_room_with_cells(List<WorldCellVisualizer> cells)
    {
        List<WorldLinkVisualizer> links = gather_links_of_cycle(cells);

        WorldRoomVisualizer new_room_visu = Instantiate(room_prefab, room_parent);
        new_room_visu.CreateRoom(cells, links);

        // pick a random color
        if (used_colors.Count == 0) { used_colors = new List<Color>(RoomColors); }
        Color color = used_colors[UnityEngine.Random.Range(0, used_colors.Count)];
        used_colors.Remove(color);
        new_room_visu.Color = color;

        // add to list
        room_visualizers.Add(new_room_visu);
        return new_room_visu;
    }
    private List<WorldLinkVisualizer> gather_links_of_cycle(List<WorldCellVisualizer> cycle)
    {
        var links = new List<WorldLinkVisualizer>();
        for (int i = 0; i < cycle.Count; i++)
        {
            var c1 = cycle[i];
            var c2 = cycle[(i + 1) % cycle.Count];
            var l = GetLinkBetween(c1, c2);
            if (l != null) { links.Add(l); }
        }
        return links;
    }
    private void handle_potential_cycle_creation(WorldLinkVisualizer new_link)
    {
        bool is_a_cycle_created = try_get_cycle_created_by_edge(new_link, out List<WorldCellVisualizer> cycle);

        if (!is_a_cycle_created) { return; }
        if (log_cycles) { Debug.Log("(WorldBuilder) cycle created with " + cycle.Count + " cells : " + string.Join(", ", cycle)); }

        // we gather the links of this cycle
        List<WorldLinkVisualizer> links = gather_links_of_cycle(cycle);

        // check if we already have a room with the same cycle
        foreach (var r in room_visualizers)
        {
            if (r.IsEqualTo(cycle, links))
            {
                if (log_cycles) { Debug.Log("(WorldBuilder) but this cycle already exists in room " + r.name); }
                return;
            }
        }

        // else we create a new room
        create_room_with_cells(cycle);
    }
    private bool try_get_cycle_created_by_edge(WorldLinkVisualizer new_link, out List<WorldCellVisualizer> cycle)
    {
        cycle = null;
        if (new_link.CellA == null || new_link.CellB == null) { return false; }
        WorldCellVisualizer a = new_link.CellA;
        WorldCellVisualizer b = new_link.CellB;

        // 1) adjacency from existing finalized links
        var adj = new Dictionary<WorldCellVisualizer, List<WorldCellVisualizer>>();
        foreach (var l in link_visualizers)
        {
            if (l == null || l.CellA == null || l.CellB == null) { continue; }
            if (l == new_link) { continue; } // prevent the new link to be in the adjacency otherwise we will always have a cycle of 2 nodes
            mark_as_adjacents(adj, l.CellA, l.CellB);
        }
        if (log_cycles)
        {
            Debug.Log("(WorldBuilder) adjacency : " + string.Join("\n", adj.Select(kv => $"{kv.Key} -> {string.Join(", ", kv.Value)}")));
        }

        // 2) BFS from a to b
        var q = new Queue<WorldCellVisualizer>();
        var parent = new Dictionary<WorldCellVisualizer, WorldCellVisualizer>();
        var visited = new HashSet<WorldCellVisualizer>();

        q.Enqueue(a);
        visited.Add(a);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == b) break;
            if (!adj.TryGetValue(cur, out var neigh)) { continue; }

            for (int i = 0; i < neigh.Count; i++)
            {
                var n = neigh[i];
                if (visited.Contains(n)) { continue; }
                visited.Add(n);
                parent[n] = cur;
                q.Enqueue(n);
            }
        }

        if (!visited.Contains(b)) { return false; }

        // 3) reconstruct path a..b
        var path = new List<WorldCellVisualizer>();
        var node = b;
        path.Add(node);
        while (node != a)
        {
            node = parent[node];
            path.Add(node);
        }
        path.Reverse();

        // 4) close cycle with new edge b->a
        // path.Add(a);
        cycle = path;
        return true;
    }
    private void mark_as_adjacents(Dictionary<WorldCellVisualizer, List<WorldCellVisualizer>> adj, WorldCellVisualizer u, WorldCellVisualizer v)
    {
        if (!adj.TryGetValue(u, out var lu)) { lu = new List<WorldCellVisualizer>(); adj[u] = lu; }
        if (!adj.TryGetValue(v, out var lv)) { lv = new List<WorldCellVisualizer>(); adj[v] = lv; }
        if (!lu.Contains(v)) lu.Add(v);
        if (!lv.Contains(u)) lv.Add(u);
    }


    // GETTERS
    public Vector3Int SelectedCell => selected_cell_visualizer.CurrentCell;
    public WorldCellVisualizer GetCellAt(Vector3Int cell_pos)
    {
        for (int i = 0; i < cell_visualizers.Count; i++)
        {
            if (cell_visualizers[i].CurrentCell == cell_pos)
            {
                return cell_visualizers[i];
            }
        }
        return null;
    }
    public WorldLinkVisualizer GetLinkBetween(WorldCellVisualizer c1, WorldCellVisualizer c2)
    {
        return link_visualizers.FirstOrDefault(l => (l.CellA == c1 && l.CellB == c2) || (l.CellA == c2 && l.CellB == c1));
    }
    public WorldRoomVisualizer GetRoomOfCell(WorldCellVisualizer cell)
    {
        for (int i = 0; i < room_visualizers.Count; i++)
        {
            if (!room_visualizers[i].HasCell(cell)) { continue; }
            if (log_get_room) { Debug.Log("(WorldBuilder) cell " + cell + " is part of room " + room_visualizers[i].name); }
            return room_visualizers[i];
        }
        if (log_get_room) { Debug.Log("(WorldBuilder) cell " + cell + " is not part of any room"); }
        return null;
    }

    // BUILDER
    public void Build()
    {
        if (log_building) { Debug.Log("(WorldBuilder) Building the world..."); }

        // build carpet
        if (carpet_builder != null)
        {
            if (log_building) { Debug.Log("(WorldBuilder) Building carpet"); }
            foreach (var r in room_visualizers)
            {
                if (log_building) { Debug.Log("(WorldBuilder) Building carpet for " + r.name); }
                carpet_builder.Build(r);
            }
        }
    }

    // SAVE DATA
    public void SaveData()
    {
        var data = new WorldBuilderData();

        // create cells
        for (int i = 0; i < cell_visualizers.Count; i++)
        {
            if (cell_visualizers[i] == null) { continue; }
            data.Cells.Add(cell_visualizers[i].CurrentCell);
        }

        // create links
        for (int i = 0; i < link_visualizers.Count; i++)
        {
            if (link_visualizers[i] == null) { continue; }
            if (link_visualizers[i].CellA == null) { continue; }
            if (link_visualizers[i].CellB == null) { continue; }
            data.Links.Add(new WorldLinkData { CellA = link_visualizers[i].CellA.CurrentCell, CellB = link_visualizers[i].CellB.CurrentCell });
        }

        // create rooms
        for (int i = 0; i < room_visualizers.Count; i++)
        {
            if (room_visualizers[i] == null) { continue; }
            data.Rooms.Add(new WorldRoomData { Cells = room_visualizers[i].GetLoopCells() });
        }
        // data.Cells = cell_visualizers.Select(c => c.CurrentCell).ToList();
        // data.Links = link_visualizers.Where(l => l.CellA != null && l.CellB != null).Select(l => new WorldLinkData { CellA = l.CellA.CurrentCell, CellB = l.CellB.CurrentCell }).ToList();
        // data.Rooms = room_visualizers.Select(r => new WorldRoomData { Cells = r.GetLoopCells() }).ToList();

        string json = JsonUtility.ToJson(data, prettyPrint: true);

        if (log_data) { Debug.Log("(WorldBuilder) Saving WorldBuilder data :\n" + json); }

        // we save the json in a file in assets/data/world_building.json
        string path = data_path + "working_world.json";
        System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
    }
    public void LoadData()
    {
        string path = data_path + "working_world.json";
        if (!System.IO.File.Exists(path)) { return; }

        string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
        var data = JsonUtility.FromJson<WorldBuilderData>(json);

        // Load cells
        foreach (var cell in data.Cells)
        {
            create_cell_at(cell);
        }

        // Load links
        foreach (var link in data.Links)
        {
            WorldCellVisualizer cA = GetCellAt(link.CellA);
            WorldCellVisualizer cB = GetCellAt(link.CellB);
            if (cA == null || cB == null) { continue; }
            create_link_between(cA, cB);
        }

        // Load rooms
        foreach (var room in data.Rooms)
        {
            // gather cells of this room
            List<WorldCellVisualizer> room_cells = new List<WorldCellVisualizer>();
            foreach (var cell_pos in room.Cells)
            {
                WorldCellVisualizer c = GetCellAt(cell_pos);
                if (c != null) { room_cells.Add(c); }
            }

            // create a room with these cells
            create_room_with_cells(room_cells);
        }
    }
    private void OnDestroy()
    {
        #if UNITY_EDITOR
        SaveData();
        #endif
    }
}

[Serializable] public class WorldBuilderData
{
    public List<Vector3Int> Cells = new List<Vector3Int>();
    public List<WorldLinkData> Links = new List<WorldLinkData>();
    public List<WorldRoomData> Rooms = new List<WorldRoomData>();
}
[Serializable] public class WorldLinkData
{
    public Vector3Int CellA;
    public Vector3Int CellB;
}
[Serializable] public class WorldRoomData
{
    public List<Vector3Int> Cells = new List<Vector3Int>();
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(WorldBuilder))]
public class WorldBuilderEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Save Data"))
        {
            ((WorldBuilder)target).SaveData();
        }
        DrawDefaultInspector();
    }
}
#endif