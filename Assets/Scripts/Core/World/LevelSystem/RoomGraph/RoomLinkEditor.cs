
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
public class RoomLinkEditor : MonoBehaviour
{

    [Header("References")]
    public RoomNodeEditor node_a;
    public RoomNodeEditor node_b;
    private LineRenderer _line_renderer;
    public LineRenderer LineRenderer
    {
        get
        {
            if (_line_renderer == null) { _line_renderer = GetComponent<LineRenderer>(); }
            return _line_renderer;
        }
    }
#if UNITY_EDITOR

    // INIT
    public void Init(RoomNodeEditor node_a, RoomNodeEditor node_b)
    {
        this.node_a = node_a;
        this.node_b = node_b;
        UpdateLinkPosition();
        node_a.OnNodeUpdated += handle_node_updated;
        node_b.OnNodeUpdated += handle_node_updated;

        // we also add this link to the nodes list of links
        node_a.Links.Add(this);
        node_b.Links.Add(this);

        // and finally we set the nodes' rooms as neighbors of each other
        ConnectRooms();
    }
    public void ConnectRooms()
    {
        if (node_a == null || node_b == null) { return; }
        Room room_a = node_a.Room;
        Room room_b = node_b.Room;
        if (room_a == null || room_b == null) { return; }

        // we add the rooms as neighbors of each other
        room_a.AddStaticNeighbor(room_b);
        room_b.AddStaticNeighbor(room_a);

        // we warn the editor that the rooms have been updated
        UnityEditor.EditorUtility.SetDirty(room_a);
        UnityEditor.EditorUtility.SetDirty(room_b);
    }
    private void OnDestroy()
    {
        if (node_a != null) { node_a.OnNodeUpdated -= handle_node_updated; }
        if (node_b != null) { node_b.OnNodeUpdated -= handle_node_updated; }

        // we also remove this link from the nodes list of links
        if (node_a != null) { node_a.Links.Remove(this); }
        if (node_b != null) { node_b.Links.Remove(this); }

        // and finally we remove the nodes' rooms as neighbors of each other
        Room room_a = node_a != null ? node_a.Room : null;
        Room room_b = node_b != null ? node_b.Room : null;
        if (room_a != null && room_b != null)
        {
            room_a.RemoveStaticNeighbor(room_b);
            room_b.RemoveStaticNeighbor(room_a);

            // we warn the editor that the rooms have been updated
            UnityEditor.EditorUtility.SetDirty(room_a);
            UnityEditor.EditorUtility.SetDirty(room_b);
        }
    }

    // UPDATE NODE
    private void UpdateLinkPosition()
    {
        if (node_a == null || node_b == null) { return; }
        LineRenderer.SetPosition(0, node_a.transform.position);
        LineRenderer.SetPosition(1, node_b.transform.position);
    }
    private void handle_node_updated(RoomNodeEditor node) { UpdateLinkPosition(); }
#endif
}
