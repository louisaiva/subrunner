using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class WorldBuilder : Singleton<WorldBuilder>
{

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
    private List<WorldCellVisualizer> cell_visualizers = new List<WorldCellVisualizer>();
    [SerializeField] private Color waiting_cells_color = Color.orange;
    [SerializeField] private Color linked_cells_color = Color.navyBlue;
    private WorldCellVisualizer last_added_cell = null;

    [Header("Link Visualizers")]
    public WorldLinkVisualizer link_prefab;
    public Transform link_parent;
    private List<WorldLinkVisualizer> link_visualizers = new List<WorldLinkVisualizer>();
    [SerializeField] private Color waiting_links_color = Color.orange;
    [SerializeField] private Color linked_links_color = Color.navyBlue;
    private WorldLinkVisualizer selecting_link = null;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();
        grid_material.SetFloat("_CellSize", Grid.cellSize.x);
    }





    // UPDATE
    private void Update()
    {
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

        // we add a new cell if not already here
        WorldCellVisualizer new_cell_visu = GetCellAt(SelectedCell);
        if (new_cell_visu == null)
        {
            new_cell_visu = Instantiate(cell_prefab, cell_parent);
            new_cell_visu.SetCell(SelectedCell);
            new_cell_visu.Color = waiting_cells_color;
            cell_visualizers.Add(new_cell_visu);
        }
        
        // we connect the on-going link to the new cell
        if (selecting_link != null)
        {
            // it means we already have a cell selected and we want to link it to the new cell
            selecting_link.SetSecondCell(new_cell_visu);
            selecting_link.Color = waiting_links_color;
            selecting_link = null;
        }
        last_added_cell = new_cell_visu;

        // we create a new link visu to link this cell to the next one that will be created if we click on another cell
        selecting_link = Instantiate(link_prefab, link_parent);
        selecting_link.Color = Color.white;
        link_visualizers.Add(selecting_link);
        selecting_link.SetCells(new_cell_visu, selected_cell_visualizer);
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


    // BUILDER
    public void BuildTilemaps()
    {
        
    }
}