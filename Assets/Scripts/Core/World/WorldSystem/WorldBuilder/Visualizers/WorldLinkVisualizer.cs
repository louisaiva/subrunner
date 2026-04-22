using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class WorldLinkVisualizer : MonoBehaviour
{
    [Header("Components")]
    private LineRenderer _line_renderer;
    private LineRenderer line_renderer
    {
        get
        {
            if (_line_renderer == null) { _line_renderer = GetComponent<LineRenderer>(); }
            return _line_renderer;
        }
    }
    private Grid grid { get { return WorldBuilder.StaticInstance.Grid; } }
    public Color Color
    {
        get { return line_renderer.startColor; }
        set { line_renderer.startColor = value; line_renderer.endColor = value; }
    }
    public void SetSize(float size)
    {
        transform.localScale = new Vector3(.2f, .2f, .2f) * size;
    }
    [Header("Data")]
    public WorldCellVisualizer CellA;
    public WorldCellVisualizer CellB;

    // SET CELLS
    public void SetSecondCell(WorldCellVisualizer cell)
    {
        unregister_callbacks();
        CellB = cell;
        line_renderer.SetPosition(1, cell.WorldPosition);
        register_callbacks();
    }
    public void SetCells(WorldCellVisualizer cell_a, WorldCellVisualizer cell_b)
    {
        unregister_callbacks();

        CellA = cell_a;
        CellB = cell_b;
        line_renderer.SetPosition(0, cell_a.WorldPosition);
        line_renderer.SetPosition(1, cell_b.WorldPosition);

        // register to ondestroy of cells to destroy this link if one of the cells is destroyed
        register_callbacks();
    }

    // callbacks
    private void register_callbacks()
    {
        if (CellA != null)
        {
            CellA.OnRemoved += remove_ourself;
            CellA.OnMoved += on_cells_moved;
        }
        if (CellB != null) 
        { 
            CellB.OnRemoved += remove_ourself; 
            CellB.OnMoved += on_cells_moved;
        }
    }
    private void unregister_callbacks()
    {
        if (CellA != null) { CellA.OnRemoved -= remove_ourself; CellA.OnMoved -= on_cells_moved; }
        if (CellB != null) { CellB.OnRemoved -= remove_ourself; CellB.OnMoved -= on_cells_moved; }
    }
    private void remove_ourself(WorldCellVisualizer cell_visu)
    {
        Destroy(gameObject);
    }

    // CELLS MOVED
    private void on_cells_moved(WorldCellVisualizer cell_visu)
    {
        if (CellA != null) { line_renderer.SetPosition(0, CellA.WorldPosition); }
        if (CellB != null) { line_renderer.SetPosition(1, CellB.WorldPosition); }
    }

    void OnDestroy()
    {
        unregister_callbacks();
    }
}