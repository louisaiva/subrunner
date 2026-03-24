using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

/// <summary>
/// this class draws the border of every room in a Level. It is
/// connected to the RoomSystem to know all rooms.
/// </summary>
public class RoomVisualizer : MonoBehaviour, Startable
{

    [Header("Line Visu Prefab")]
    [SerializeField] private GameObject ui_line_visu_prefab; // a line renderer
    private HashSet<UILineRenderer> room_liners = new();

    private RectTransform _mapRoot;
    private RectTransform mapRoot
    {
        get
        {
            if (_mapRoot == null) { _mapRoot = transform.parent.GetComponent<RectTransform>(); }
            return _mapRoot;
        }
    }

    private RectTransform _scaler;
    private RectTransform scaler
    {
        get
        {
            if (_scaler == null) { _scaler = mapRoot.parent.GetComponent<RectTransform>(); }
            return _scaler;
        }
    }

    private Canvas _canvas;
    private Canvas canvas
    {
        get
        {
            if (_canvas == null) { _canvas = GetComponentInParent<Canvas>(includeInactive: true); }
            return _canvas;
        }
    }

    // START
    public void InitStart()
    {
        // ensure the scaler is at scale 1,1,1
        float scaler_scale = scaler.localScale.x;
        if (scaler_scale != 1f) { scaler.localScale = Vector3.one; }

        Canvas.ForceUpdateCanvases();

        // grab all the rooms data in the RoomSystem
        List<RoomData> rooms = RoomSystem.Instance.rooms_data.Values.ToList();

        // compute the world bounds of all rooms to set the size of our canvas accordingly
        Bounds world_bounds = ComputeRoomsWorldBounds(rooms);
        Vector2 centerLocal = UI_Manager.WorldToCanvasLocal(world_bounds.center, mapRoot, canvas, Camera.main);
        Vector2 globalOffset = -centerLocal;

        // and build a visual for each room
        foreach (RoomData room in rooms)
        {
            // Debug.Log("(RoomVisualizer) Creating visual for room: " + room.id);
            create_visu_for_room(room, globalOffset);
        }

        // then call creation of CapableVisualizerManager to create visuals for the capables, now that we have the right canvas size
        CapableVisualizerManager capable_visu_manager = transform.parent.GetComponentInChildren<CapableVisualizerManager>(includeInactive: true);
        capable_visu_manager.CreateVisuals(mapRoot, globalOffset);

        // finally we re set the scaler to initial scale
        scaler.localScale = Vector3.one * scaler_scale;
    }

    private void create_visu_for_room(RoomData rdata, Vector2 globalOffset)
    {
        // 1. instanciate a visu
        GameObject go = Instantiate(ui_line_visu_prefab, transform);
        go.name = rdata.id;

        // 2. set the line points
        UILineRenderer liner = go.GetComponent<UILineRenderer>();
        List<Vector2> points = new List<Vector2>();
        foreach (Vector2 point in rdata.collider_points)
        {
            // points.Add(point + rdata.position);
            Vector2 world_point = point + rdata.position;
            Vector2 canvas_point = UI_Manager.WorldToCanvasLocal(world_point, mapRoot, liner.canvas, Camera.main);
            points.Add(canvas_point + globalOffset);
        }
        points.Add(points[0]); // we close the loop by adding the first point at the end
        liner.Points = points.ToArray();

        // 3. add the liner to our hashset
        room_liners.Add(liner);
    }


    // COMPUTE WORLD BOUNDS
    private Bounds ComputeRoomsWorldBounds(IEnumerable<RoomData> rooms)
    {
        bool hasPoint = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (RoomData room in rooms)
        {
            if (room == null || room.collider_points == null) { continue; }

            for (int i = 0; i < room.collider_points.Count; i++)
            {
                Vector3 wp = (Vector3)(room.position + room.collider_points[i]);
                if (!hasPoint)
                {
                    bounds = new Bounds(wp, Vector3.zero);
                    hasPoint = true;
                }
                else
                {
                    bounds.Encapsulate(wp);
                }
            }
        }

        return bounds;
    }
}