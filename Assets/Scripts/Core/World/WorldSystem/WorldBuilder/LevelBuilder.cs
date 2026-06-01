using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
public class LevelBuilder : MonoBehaviour
{


    [Header("Current Targeted World & Level")]
    [SerializeField] private string level_id = "";
    private string world_id => WorldBuilder.StaticTargetedWorld;
    public string TargetedLevel => level_id;


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


    [Header("Selected Tool Cell")]
    public WorldCellVisualizer selected_cell_visualizer;
    public string tool_type = "node"; // "node", "door_ver", "door_hor", "light"
    private Dictionary<string, Sprite> tools_icons = new Dictionary<string, Sprite>();


    [Header("Zoom")]
    private float min_zoom = 5f;
    private float max_zoom = 30f;
    [Range(5f, 30f)] public float Zoom = 5f;
    public void SetZoom(Setting setting)
    {
        // setting is a percentage
        Zoom = Mathf.Lerp(min_zoom, max_zoom, setting.GetPercentage());
    }


    [Header("Nodes Visualizers")]
    public WorldNodeVisualizer node_prefab;
    public WorldNodeVisualizer node_tool;
    public Transform node_parent;
    private List<WorldNodeVisualizer> node_visualizers = new List<WorldNodeVisualizer>();
    private WorldNodeVisualizer last_added_node = null;

    [Header("Link Visualizers")]
    public WorldLinkVisualizer link_prefab;
    public Transform link_parent;
    private List<WorldLinkVisualizer> link_visualizers = new List<WorldLinkVisualizer>();
    private WorldLinkVisualizer selecting_link = null;


    [Header("Room Visualizers")]
    public WorldChunkVisualizer room_prefab;
    public Transform room_parent;
    private List<WorldChunkVisualizer> room_visualizers = new List<WorldChunkVisualizer>();

    [Header("Colors")]
    public Color WaitingColor = Color.orange;
    public Color LinkedColor = Color.lightBlue;
    public List<Color> RoomColors = new List<Color> { Color.lightPink, Color.lightGreen, Color.paleTurquoise, Color.cyan, Color.magenta };
    private List<Color> used_colors = new List<Color>();

    [Header("Door Visualizers")]
    public WorldDoorVisualizer door_ver_prefab;
    public WorldDoorVisualizer door_hor_prefab;
    public WorldDoorVisualizer door_ver_tool;
    public WorldDoorVisualizer door_hor_tool;
    public Transform door_parent;
    private List<WorldDoorVisualizer> door_visualizers = new List<WorldDoorVisualizer>();
    [SerializeField] private Color WrongDoorColor = Color.darkRed;
    [SerializeField] private Color ConnectedDoorColor = Color.lightSeaGreen;

    [Header("Light Visualizers")]
    public WorldLightVisualizer light_prefab;
    public WorldLightVisualizer light_tool;
    public Transform light_parent;
    private List<WorldLightVisualizer> light_visualizers = new List<WorldLightVisualizer>();
    [SerializeField] private Color LightColor = Color.yellowNice;



    [Header("Builders")]
    public CarpetBuilder carpet_builder;
    public GroundBuilder ground_builder;
    public WallsBuilder walls_builder;
    public CeilingBuilder ceiling_builder;
    public EdgesBuilder edges_builder;


    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_cycles = false;
    [SerializeField] private bool log_get_room = false;
    [SerializeField] private bool log_room_grab = false;
    [SerializeField] private bool log_data = false;
    [SerializeField] private bool log_building = false;
    [SerializeField] private bool log_tool = false;



    // EVENTS
    private bool fire_modified = true; // simple toggle so we can prevent firing the event when loading the schematic
    public System.Action<string> OnLevelModified = delegate { };
    public System.Action<string> OnLevelBuilding = delegate { };



    ///
    // 
    ///  AWAKE & MAIN ENTRY POINTS
    //
    ///

    // Awake
    public void Awake()
    {
        grid_material.SetFloat("_CellSize", Grid.cellSize.x);

        used_colors = new List<Color>(RoomColors);

        // we get the icons for the tools
        tools_icons["node"] = node_prefab.GetComponent<SpriteRenderer>().sprite;
        tools_icons["door_ver"] = door_ver_prefab.GetComponent<SpriteRenderer>().sprite;
        tools_icons["door_hor"] = door_hor_prefab.GetComponent<SpriteRenderer>().sprite;
        tools_icons["light"] = light_prefab.GetComponent<SpriteRenderer>().sprite;
    }
    private void Start()
    {
        SettingsManager.Instance.RegisterCallback("level_builder_zoom", SetZoom);
    }
    private void OnDestroy()
    {
        SettingsManager.Instance?.UnregisterCallback("level_builder_zoom", SetZoom);
    }

    // ON ENABLE / DISABLE
    private void OnEnable()
    {
        if (log) { Debug.Log("(LevelBuilder) OnEnable, targeted world: " + world_id + ", targeted level: " + level_id); }

        // we try to grab all the doors & lights
        make_rooms_grab_all_doors();
        make_rooms_grab_all_lights();

        CameraFollow.Instance.ResetSimpleControllerToCenter();
        CameraFollow.Instance.EnableSimpleController();
    }
    private void OnDisable()
    {
        SaveCurrentLevelSchematic();
        Erase();
        try
        {
            CameraFollow.Instance.ResetSize();
            CameraFollow.Instance.DisableSimpleController();
        }
        catch (Exception) { }
    }


    public void EditLevel(string world, string level)
    {
        level_id = level;
        if (!LoadLevelSchematic(world, level))
        {
            if (log) { Debug.Log("(LevelBuilder) New Level schematic : " + level + " for world: " + world); }
            return;
        }
        if (log) { Debug.Log("(LevelBuilder) Loaded Level schematic : " + level + " for world: " + world); }
    }


    ///
    // 
    ///  UPDATE & INPUTS
    //
    ///




    // UPDATE
    private void Update()
    {
        CameraFollow.Instance.SetSize(Zoom);

        // check if we are on the right ui_pool
        if (UI_Manager.Instance.CurrentPool != "dev_level_builder")
        {
            if (selected_cell_visualizer.gameObject.activeSelf) { selected_cell_visualizer.gameObject.SetActive(false); }
            return;
        }

        // check if we have a navigator and if it has a hovered ui element
        if (UI_Navigator.Instance.IsHoveringSlot)
        {
            if (selected_cell_visualizer.gameObject.activeSelf) { selected_cell_visualizer.gameObject.SetActive(false); }
            return;
        }
        if (!selected_cell_visualizer.gameObject.activeSelf) { selected_cell_visualizer.gameObject.SetActive(true); }


        // check if we have some null or missing visualizers in our lists and remove them
        node_visualizers.RemoveAll(v => v == null);
        door_visualizers.RemoveAll(v => v == null);
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

        // update zoom setting
        float scroll = InputManager.Instance.inputs.UI.scroll.ReadValue<float>();
        if (scroll != 0)
        {
            // we get the setting
            StepSetting stepSetting = (StepSetting) SettingsManager.Instance.GetSetting("level_builder_zoom");
            stepSetting.Scroll((int) -scroll);
        }

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


    // TOOL SWITCH
    public void SelectTool(string new_tool)
    {
        if (new_tool == tool_type) { return; }

        // remove the old tool
        if (tool_type == "node" && selecting_link != null)
        {
            Destroy(selecting_link.gameObject);
            selecting_link = null;
        }

        tool_type = new_tool;

        // switch the tool
        selected_cell_visualizer.gameObject.SetActive(false);
        selected_cell_visualizer = tool_type switch
        {
            "node" => node_tool,
            "door_ver" => door_ver_tool,
            "door_hor" => door_hor_tool,
            "light" => light_tool,
            _ => selected_cell_visualizer
        };
        selected_cell_visualizer.SetIcon(tools_icons[tool_type]);
        selected_cell_visualizer.gameObject.SetActive(true);
        if (log_tool) { Debug.Log("(LevelBuilder) selected tool : " + tool_type); }
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
            click_at_selected();
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
    private void click_at_selected()
    {
        // check the tool type
        if (tool_type == "node") { add_node_at(SelectedCell); }
        else if (tool_type == "door_ver") { create_door_at(SelectedCell, true); }
        else if (tool_type == "door_hor") { create_door_at(SelectedCell, false); }
        else if (tool_type == "light") { create_light_at(SelectedCell); }
    }


    ///
    // 
    ///  CELLS & NODES & LINKS & ROOMS VISUs
    //
    ///

    // REMOVE CELL
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
        if (cell_to_remove is WorldNodeVisualizer node_to_remove) { node_visualizers.Remove(node_to_remove); }
        else if (cell_to_remove is WorldDoorVisualizer door_to_remove) { door_visualizers.Remove(door_to_remove); }
        Destroy(cell_to_remove.gameObject);

        // fire the event
        if (fire_modified) { OnLevelModified?.Invoke(level_id); }
    }

    // NODES / LINKS MANAGEMENT
    private void add_node_at(Vector3Int cell_pos)
    {
        WorldCellVisualizer new_cell_visu = GetCellAt(cell_pos);
        if (new_cell_visu != null && new_cell_visu is not WorldNodeVisualizer) { return; }

        // we add a new cell if not already here
        WorldNodeVisualizer new_node_visu = new_cell_visu as WorldNodeVisualizer;
        if (new_cell_visu == null) { new_node_visu = create_node_at(cell_pos); }
        
        // we connect the on-going link to the new cell
        if (selecting_link != null)
        {
            if (GetLinkBetween(selecting_link.NodeA, new_node_visu) != null)
            {
                // if there is already a link between the selecting cell and the new cell it means we want to remove this link instead of creating a cycle
                Destroy(selecting_link.gameObject);
                selecting_link = null;
            }
            else
            {
                // it means we already have a cell selected and we want to link it to the new cell
                selecting_link.SetSecondCell(new_node_visu);
                selecting_link.Color = WaitingColor;

                // we check if this new link creates a cycle
                handle_potential_cycle_creation(selecting_link);
                selecting_link = null;
            }
        }
        last_added_node = new_node_visu;

        // we create a new link visu to link this cell to the next one that will be created if we click on another cell
        selecting_link = create_link_between(new_node_visu, selected_cell_visualizer as WorldNodeVisualizer, Color.white);
    }
    private WorldNodeVisualizer create_node_at(Vector3Int cell_pos)
    {
        WorldNodeVisualizer new_cell_visu = Instantiate(node_prefab, node_parent);
        new_cell_visu.SetCell(cell_pos);
        new_cell_visu.Color = WaitingColor;
        node_visualizers.Add(new_cell_visu);

        // fire the event
        if (fire_modified) { OnLevelModified?.Invoke(level_id); }
        return new_cell_visu;
    }
    private WorldLinkVisualizer create_link_between(WorldNodeVisualizer c1, WorldNodeVisualizer c2, Color? color = null)
    {
        WorldLinkVisualizer new_link_visu = Instantiate(link_prefab, link_parent);
        new_link_visu.SetCells(c1, c2);
        new_link_visu.Color = color ?? WaitingColor;
        link_visualizers.Add(new_link_visu);
        // fire the event
        if (fire_modified) { OnLevelModified?.Invoke(level_id); }
        return new_link_visu;
    }

    // cells to rooms
    private void handle_potential_cycle_creation(WorldLinkVisualizer new_link)
    {
        bool is_a_cycle_created = try_get_cycle_created_by_edge(new_link, out List<WorldNodeVisualizer> cycle);

        if (!is_a_cycle_created) { return; }
        if (cycle.Count <= 4) { return; } // we need at least 5 cells to create a room (so we remove the small dirty shit)
        if (log_cycles) { Debug.Log("(LevelBuilder) cycle created with " + cycle.Count + " cells : " + string.Join(", ", cycle)); }

        // we gather the links of this cycle
        List<WorldLinkVisualizer> links = gather_links_of_cycle(cycle);

        // check if we already have a room with the same cycle
        foreach (var r in room_visualizers)
        {
            if (r.IsEqualTo(cycle, links))
            {
                if (log_cycles) { Debug.Log("(LevelBuilder) but this cycle already exists in room " + r.name); }
                return;
            }
        }

        // else we create a new room
        cells_waiting_for_a_room = cycle;
        UI_Manager.Instance.OpenInputPopup("enter room name", WorldChunkVisualizer.NextRoomName, FinishRoomCreationWithName);
    }
    private List<WorldNodeVisualizer> cells_waiting_for_a_room = new List<WorldNodeVisualizer>();
    public void FinishRoomCreationWithName(string room_name)
    {
        if (cells_waiting_for_a_room.Count == 0) { return; }
        create_room_with_cells(cells_waiting_for_a_room, room_name);
        cells_waiting_for_a_room.Clear();
    }

    // ROOM (LOOPING NODES) MANAGEMENT
    private WorldChunkVisualizer create_room_with_cells(List<WorldNodeVisualizer> cells, string room_name = "")
    {
        List<WorldLinkVisualizer> links = gather_links_of_cycle(cells);

        WorldChunkVisualizer new_room_visu = Instantiate(room_prefab, room_parent);
        new_room_visu.CreateRoom(cells, links, room_name);

        // pick a random color
        if (used_colors.Count == 0) { used_colors = new List<Color>(RoomColors); }
        Color color = used_colors[UnityEngine.Random.Range(0, used_colors.Count)];
        used_colors.Remove(color);
        new_room_visu.Color = color;

        // add to list
        room_visualizers.Add(new_room_visu);

        // we refresh all lights & doors
        make_rooms_grab_all_doors();
        make_rooms_grab_all_lights();

        // fire the event
        if (fire_modified) { OnLevelModified?.Invoke(level_id); }
        return new_room_visu;
    }
    private List<WorldLinkVisualizer> gather_links_of_cycle(List<WorldNodeVisualizer> cycle)
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
    private bool try_get_cycle_created_by_edge(WorldLinkVisualizer new_link, out List<WorldNodeVisualizer> cycle)
    {
        cycle = null;
        if (new_link.NodeA == null || new_link.NodeB == null) { return false; }
        WorldNodeVisualizer a = new_link.NodeA;
        WorldNodeVisualizer b = new_link.NodeB;

        // 1) adjacency from existing finalized links
        var adj = new Dictionary<WorldNodeVisualizer, List<WorldNodeVisualizer>>();
        foreach (var l in link_visualizers)
        {
            if (l == null || l.NodeA == null || l.NodeB == null) { continue; }
            if (l == new_link) { continue; } // prevent the new link to be in the adjacency otherwise we will always have a cycle of 2 nodes
            mark_as_adjacents(adj, l.NodeA, l.NodeB);
        }
        if (log_cycles)
        {
            Debug.Log("(LevelBuilder) adjacency : " + string.Join("\n", adj.Select(kv => $"{kv.Key} -> {string.Join(", ", kv.Value)}")));
        }

        // 2) BFS from a to b
        var q = new Queue<WorldNodeVisualizer>();
        var parent = new Dictionary<WorldNodeVisualizer, WorldNodeVisualizer>();
        var visited = new HashSet<WorldNodeVisualizer>();

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
        var path = new List<WorldNodeVisualizer>();
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
    private void mark_as_adjacents(Dictionary<WorldNodeVisualizer, List<WorldNodeVisualizer>> adj, WorldNodeVisualizer u, WorldNodeVisualizer v)
    {
        if (!adj.TryGetValue(u, out var lu)) { lu = new List<WorldNodeVisualizer>(); adj[u] = lu; }
        if (!adj.TryGetValue(v, out var lv)) { lv = new List<WorldNodeVisualizer>(); adj[v] = lv; }
        if (!lu.Contains(v)) lu.Add(v);
        if (!lv.Contains(u)) lv.Add(u);
    }



    ///
    // 
    ///  DOORs & LIGHTs
    //
    ///

    // DOORS MANAGEMENT
    private WorldDoorVisualizer create_door_at(Vector3Int cell_pos, bool vertical = true)
    {
        WorldDoorVisualizer prefab = vertical ? door_ver_prefab : door_hor_prefab;
        WorldDoorVisualizer new_door_visu = Instantiate(prefab, door_parent);
        new_door_visu.SetCell(cell_pos);
        new_door_visu.Color = ConnectedDoorColor;
        new_door_visu.name = $"{prefab.name}_{cell_pos.x}_{cell_pos.y}";
        door_visualizers.Add(new_door_visu);
        assign_door_to_rooms(new_door_visu);

        // fire the event
        if (fire_modified) { OnLevelModified?.Invoke(level_id); }
        return new_door_visu;
    }
    private void make_rooms_grab_all_doors()
    {
        if (log_room_grab) { Debug.Log("(LevelBuilder) making rooms grab all doors"); }
        foreach (var d in door_visualizers) { assign_door_to_rooms(d); }
    }
    private void assign_door_to_rooms(WorldDoorVisualizer door)
    {
        foreach (var r in room_visualizers)
        {
            bool grabbed = false;
            if (r.CollideWithCell(door.Cell)) { r.AddDoor(door); grabbed = true; }
            else if (r.CollideWithCell(door.OtherCell)) { r.AddDoor(door); grabbed = true; }
            if (!grabbed) { continue; }
            if (log_room_grab) { Debug.Log($"(LevelBuilder) Door {door.name} assigned to room {r.name}"); }
        }
    }

    // LIGHTS
    private WorldLightVisualizer create_light_at(Vector3Int cell_pos)
    {
        WorldLightVisualizer new_light_visu = Instantiate(light_prefab, light_parent);
        new_light_visu.SetCell(cell_pos);
        new_light_visu.Color = LightColor;
        light_visualizers.Add(new_light_visu);
        assign_light_to_rooms(new_light_visu);

        // fire the event
        if (fire_modified) { OnLevelModified?.Invoke(level_id); }
        return new_light_visu;
    }
    private void make_rooms_grab_all_lights()
    {
        if (log_room_grab) { Debug.Log("(LevelBuilder) making rooms grab all lights"); }
        foreach (var l in light_visualizers) { assign_light_to_rooms(l); }
    }
    private void assign_light_to_rooms(WorldLightVisualizer light)
    {
        foreach (var r in room_visualizers)
        {
            bool grabbed = false;
            if (r.CollideWithCell(light.Cell)) { r.AddLight(light); grabbed = true;}
            if (!grabbed) { continue; }
            if (log_room_grab) { Debug.Log($"(LevelBuilder) Light {light.name} assigned to room {r.name}"); }
            return; // one light can only be in one room
        }
    }





    ///
    //
    ///  BUILD
    //
    ///


    // BUILDER
    public Action<BuiltLevelData> OnLevelBuilt = delegate { };
    public BuiltLevelData Build(string world, string level)
    {
        OnLevelBuilding?.Invoke(level);

        LoadLevelSchematic(world, level);
        BuiltLevelData built_world = new BuiltLevelData()
        {
            world = world_id,
            level = level,
            Chunks = new List<WorldChunkVisualizer>(room_visualizers)
        };

        foreach (var r in room_visualizers)
        {
            if (!r.isActiveAndEnabled) { continue; }
            if (log_building) { Debug.Log($"(LevelBuilder) Building tilemaps for {r.name}"); }
            built_world.Tilemaps[r.name] = build_room(r);
        }

        if (log_building) { Debug.Log($"(LevelBuilder) Level {level} on world {world_id} built"); }
        OnLevelBuilt?.Invoke(built_world);

        return built_world;
    }
    public void Build()
    {
        if (string.IsNullOrEmpty(world_id) || string.IsNullOrEmpty(level_id))
        {
            if (log_building) { Debug.Log("(LevelBuilder) No world or level targeted, cannot build"); }
            return;
        }

        if (log_building) { Debug.Log("(LevelBuilder) Building the level : " + level_id + $" (world : {world_id})"); }
        OnLevelBuilding?.Invoke(level_id);

        BuiltLevelData built_world = new BuiltLevelData()
        {
            world = world_id,
            level = level_id,
            Chunks = new List<WorldChunkVisualizer>(room_visualizers)
        };

        foreach (var r in room_visualizers)
        {
            if (!r.isActiveAndEnabled) { continue; }
            if (log_building) { Debug.Log($"(LevelBuilder) Building tilemaps for {r.name}"); }
            built_world.Tilemaps[r.name] = build_room(r);
        }

        if (log_building) { Debug.Log("(LevelBuilder) World built"); }
        OnLevelBuilt?.Invoke(built_world);
    }
    public void Build(string builder)
    {
        List<string> builders = new List<string>();

        // check some things
        if (builder == "ceiling") { builders.Add("walls"); } // we need walls to build the ceiling
        if (builders.Contains("walls")) { builders.Add("edges"); } // for real walls we need edges
        builders.Add(builder);

        foreach (var r in room_visualizers)
        {
            if (!r.isActiveAndEnabled) { continue; }
            build_room(r, builders);
        }
    }
    private List<string> default_builders = new List<string> { "carpet", "ground", "walls", "edges", "ceiling" };
    private Dictionary<string, Tilemap> build_room(WorldChunkVisualizer room, List<string> builders = null)
    {
        if (builders == null) { builders = default_builders; }
        Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
        foreach (var b in builders)
        {
            if (b == "carpet" && carpet_builder != null)
            {
                if (log_building) { Debug.Log("(LevelBuilder) Building carpet for " + room.name); }
                tilemaps["carpet"] = carpet_builder.Build(room);
            }
            if (b == "ground" && ground_builder != null)
            {
                if (log_building) { Debug.Log("(LevelBuilder) Building ground for " + room.name); }
                tilemaps["ground"] = ground_builder.Build(room);
            }
            if (b == "walls" && walls_builder != null)
            {
                if (log_building) { Debug.Log("(LevelBuilder) Building walls for " + room.name); }
                tilemaps["walls"] = walls_builder.Build(room);
            }
            if (b == "edges" && edges_builder != null)
            {
                if (log_building) { Debug.Log("(LevelBuilder) Building edges for " + room.name); }
                tilemaps["edges"] = edges_builder.Build(room);
            }
            if (b == "ceiling" && ceiling_builder != null)
            {
                if (log_building) { Debug.Log("(LevelBuilder) Building ceiling for " + room.name); }
                tilemaps["ceiling"] = ceiling_builder.Build(room);
            }
        }
        return tilemaps;
    }




    ///
    //
    ///  CLEAR CACHE
    //
    ///

    public void Erase()
    {
        // we remove all the visualizers
        ClearVisus();

        // we clear the tilemaps
        ClearTilemaps();
    }
    public void ClearVisus()
    {
        // we destroy all the visualizers and clear the lists
        for (int i = 0; i < node_visualizers.Count; i++) { if (node_visualizers[i] != null) { Destroy(node_visualizers[i].gameObject); } }
        for (int i = 0; i < door_visualizers.Count; i++) { if (door_visualizers[i] != null) { Destroy(door_visualizers[i].gameObject); } }
        for (int i = 0; i < link_visualizers.Count; i++) { if (link_visualizers[i] != null) { Destroy(link_visualizers[i].gameObject); } }
        for (int i = 0; i < room_visualizers.Count; i++) { if (room_visualizers[i] != null) { Destroy(room_visualizers[i].gameObject); } }
        for (int i = 0; i < light_visualizers.Count; i++) { if (light_visualizers[i] != null) { Destroy(light_visualizers[i].gameObject); } }

        node_visualizers.Clear();
        door_visualizers.Clear();
        link_visualizers.Clear();
        room_visualizers.Clear();
        light_visualizers.Clear();
    }
    public void ClearTilemaps()
    {
        carpet_builder.Clear();
        ground_builder.Clear();
        walls_builder.Clear();
        ceiling_builder.Clear();
        edges_builder.Clear();
    }






    ///
    //
    ///  GETTERS & OTHERS
    //
    ///





    // GETTERS
    public Vector3Int SelectedCell => selected_cell_visualizer.Cell;
    public WorldCellVisualizer GetCellAt(Vector3Int cell_pos)
    {
        WorldCellVisualizer cell = GetNodeAt(cell_pos) as WorldCellVisualizer
                                ?? GetDoorAt(cell_pos) as WorldCellVisualizer
                                ?? GetLightAt(cell_pos);
        return cell;
    }
    public WorldNodeVisualizer GetNodeAt(Vector3Int cell_pos)
    {
        for (int i = 0; i < node_visualizers.Count; i++)
        {
            if (node_visualizers[i].Cell == cell_pos)
            {
                return node_visualizers[i];
            }
        }
        return null;
    }
    public WorldDoorVisualizer GetDoorAt(Vector3Int cell_pos)
    {
        for (int i = 0; i < door_visualizers.Count; i++)
        {
            if (door_visualizers[i].Cell == cell_pos)
            {
                return door_visualizers[i];
            }
        }
        return null;
    }
    public WorldLightVisualizer GetLightAt(Vector3Int cell_pos)
    {
        for (int i = 0; i < light_visualizers.Count; i++)
        {
            if (light_visualizers[i].Cell == cell_pos)
            {
                return light_visualizers[i];
            }
        }
        return null;
    }
    public WorldLinkVisualizer GetLinkBetween(WorldNodeVisualizer c1, WorldNodeVisualizer c2)
    {
        return link_visualizers.FirstOrDefault(l => (l.NodeA == c1 && l.NodeB == c2) || (l.NodeA == c2 && l.NodeB == c1));
    }
    public WorldChunkVisualizer GetRoomOfNode(WorldNodeVisualizer cell)
    {
        for (int i = 0; i < room_visualizers.Count; i++)
        {
            if (!room_visualizers[i].HasNode(cell)) { continue; }
            if (log_get_room) { Debug.Log("(LevelBuilder) cell " + cell + " is part of room " + room_visualizers[i].name); }
            return room_visualizers[i];
        }
        if (log_get_room) { Debug.Log("(LevelBuilder) cell " + cell + " is not part of any room"); }
        return null;
    }


    ///
    //
    ///  DATA MANAGEMENT
    //
    ///

    // SAVE
    public void SaveCurrentLevelSchematic()
    {
        if (string.IsNullOrEmpty(world_id) || string.IsNullOrEmpty(level_id))
        {
            if (log_data) { Debug.LogWarning("(LevelBuilder) Cannot save schematic : world_id or level_id is empty"); }
            return;
        }

        // we make sure we have a world folder hierarchy for the world & level
        WorldManager.EnsureWorldDataHierarchy(world_id);

        var data = new LevelSchematic();

        // create cells
        for (int i = 0; i < node_visualizers.Count; i++)
        {
            if (node_visualizers[i] == null) { continue; }
            data.Cells.Add(node_visualizers[i].Cell);
        }

        // create doors
        for (int i = 0; i < door_visualizers.Count; i++)
        {
            if (door_visualizers[i] == null) { continue; }
            if (door_visualizers[i].is_vertical) { data.VerDoors.Add(door_visualizers[i].Cell); }
            else { data.HorDoors.Add(door_visualizers[i].Cell); }
        }

        // create links
        for (int i = 0; i < link_visualizers.Count; i++)
        {
            if (link_visualizers[i] == null) { continue; }
            if (link_visualizers[i].NodeA == null) { continue; }
            if (link_visualizers[i].NodeB == null) { continue; }
            data.Links.Add(new LinkSchematic { CellA = link_visualizers[i].NodeA.Cell, CellB = link_visualizers[i].NodeB.Cell });
        }

        // create rooms
        for (int i = 0; i < room_visualizers.Count; i++)
        {
            if (room_visualizers[i] == null) { continue; }
            RoomSchematic room_data = new RoomSchematic
            {
                Name = room_visualizers[i].name.Replace("-0", ""),
                Cells = room_visualizers[i].GetLoopCells()
            };
            data.Rooms.Add(room_data);
        }
        
        // create lights
        for (int i = 0; i < light_visualizers.Count; i++)
        {
            if (light_visualizers[i] == null) { continue; }
            data.Lights.Add(light_visualizers[i].Cell);
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);


        // we save the json in a file in assets/data/world_building.json
        string path = Path.Combine("levels", level_id + ".schematic");
        AppManager.SaveJsonToWorldFolder(world_id, path, json);
        if (log_data) { Debug.Log("(LevelBuilder) Saved schematic for " + level_id + $"({world_id}) at {path} :\n" + json); }
    }
    
    // LOAD
    public bool LoadLevelSchematic(string world, string level)
    {
        string path = Path.Combine("levels", level + ".schematic");
        string json = AppManager.LoadJsonFromWorldFolder(world, path);
        if (string.IsNullOrEmpty(json))
        {
            if (log_data) { Debug.LogWarning($"(LevelBuilder) No schematic found at path : {path}"); }
            return false;
        }
        load_data(json);
        if (log_data) { Debug.Log("(LevelBuilder) Loaded schematic for " + level_id + $"({world_id}) from {path} :\n" + json); }
        return true;
    }
    private void load_data(string json)
    {
        var data = JsonUtility.FromJson<LevelSchematic>(json);

        fire_modified = false; // we prevent firing the modified event while loading the schematic

        // Load cells
        foreach (var cell in data.Cells) { create_node_at(cell); }

        // Load links
        foreach (var link in data.Links)
        {
            WorldNodeVisualizer cA = GetNodeAt(link.CellA);
            WorldNodeVisualizer cB = GetNodeAt(link.CellB);
            if (cA == null || cB == null) { continue; }
            create_link_between(cA, cB);
        }

        // Load rooms
        foreach (var room in data.Rooms)
        {
            // gather cells of this room
            List<WorldNodeVisualizer> room_cells = new List<WorldNodeVisualizer>();
            foreach (var cell_pos in room.Cells)
            {
                WorldNodeVisualizer c = GetNodeAt(cell_pos);
                if (c != null) { room_cells.Add(c); }
            }

            // create a room with these cells
            create_room_with_cells(room_cells, room.Name);
        }

        // Load doors
        foreach (var door in data.VerDoors) { create_door_at(door, vertical: true); }
        foreach (var door in data.HorDoors) { create_door_at(door, vertical: false); }

        // Load lights
        foreach (var light in data.Lights) { create_light_at(light); }

        // we are done loading, we can allow firing the modified event again
        fire_modified = true;
    }
}


// RTO DATA CLASS -> for sending data to the LevelTranslator
public class BuiltLevelData
{
    public string world;
    public string level;

    // cells links chunks visu
    public List<WorldChunkVisualizer> Chunks = new List<WorldChunkVisualizer>();

    // tilemaps
    public Dictionary<string, Dictionary<string, Tilemap>> Tilemaps = new Dictionary<string, Dictionary<string, Tilemap>>();
    // like this :
    // -room_0
    //     -carpet -> tilemap
    //     -ground -> tilemap
    //     -...
    // -room_1
    //     -carpet -> tilemap
    //     -ground -> tilemap
    //     -...
    // -...

    // big rooms splitted in small chunks
    public Dictionary<string, List<string>> RoomChunks = new Dictionary<string, List<string>>();

    public string GetRoomOfChunk(string chunk_name)
    {
        foreach (var kv in RoomChunks)
        {
            if (kv.Value.Contains(chunk_name)) { return kv.Key; }
        }
        return null;
    }
}


// USEFUL SAVING DATA CLASSES -> for saving the current schematic
[Serializable] public class LevelSchematic
{
    public List<Vector3Int> Cells = new List<Vector3Int>();
    public List<Vector3Int> VerDoors = new List<Vector3Int>();
    public List<Vector3Int> HorDoors = new List<Vector3Int>();
    public List<Vector3Int> Lights = new List<Vector3Int>();
    public List<LinkSchematic> Links = new List<LinkSchematic>();
    public List<RoomSchematic> Rooms = new List<RoomSchematic>();
}
[Serializable] public class LinkSchematic
{
    public Vector3Int CellA;
    public Vector3Int CellB;
}
[Serializable] public class RoomSchematic
{
    public string Name;
    public List<Vector3Int> Cells = new List<Vector3Int>();
}
