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
    private Grid grid { get { return LevelBuilder.StaticInstance.Grid; } }
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
    public WorldNodeVisualizer NodeA;
    public WorldNodeVisualizer NodeB;

    // SET CELLS
    public void SetSecondCell(WorldNodeVisualizer node)
    {
        unregister_callbacks();
        NodeB = node;
        line_renderer.SetPosition(1, node.WorldPosition);
        register_callbacks();
    }
    public void SetCells(WorldNodeVisualizer node_a, WorldNodeVisualizer node_b)
    {
        unregister_callbacks();

        NodeA = node_a;
        NodeB = node_b;
        line_renderer.SetPosition(0, node_a.WorldPosition);
        line_renderer.SetPosition(1, node_b.WorldPosition);

        // register to ondestroy of nodes to destroy this link if one of the nodes is destroyed
        register_callbacks();
    }

    // callbacks
    private void register_callbacks()
    {
        if (NodeA != null)
        {
            NodeA.OnRemoved += remove_ourself;
            NodeA.OnMoved += on_nodes_moved;
        }
        if (NodeB != null) 
        { 
            NodeB.OnRemoved += remove_ourself; 
            NodeB.OnMoved += on_nodes_moved;
        }
    }
    private void unregister_callbacks()
    {
        if (NodeA != null) { NodeA.OnRemoved -= remove_ourself; NodeA.OnMoved -= on_nodes_moved; }
        if (NodeB != null) { NodeB.OnRemoved -= remove_ourself; NodeB.OnMoved -= on_nodes_moved; }
    }
    private void remove_ourself(WorldCellVisualizer node)
    {
        Destroy(gameObject);
    }

    // CELLS MOVED
    private void on_nodes_moved(WorldCellVisualizer node)
    {
        if (NodeA != null) { line_renderer.SetPosition(0, NodeA.WorldPosition); }
        if (NodeB != null) { line_renderer.SetPosition(1, NodeB.WorldPosition); }
    }

    void OnDestroy()
    {
        unregister_callbacks();
    }
}