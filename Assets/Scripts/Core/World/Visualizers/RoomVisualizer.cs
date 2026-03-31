using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

/// <summary>
/// this class draws the border of every room in a Level. It is
/// connected to the RoomSystem to know all rooms.
/// </summary>
public class RoomVisualizer : MonoBehaviour
{

    [Header("Line Visu Prefab")]
    [SerializeField] private GameObject ui_line_visu_prefab; // a line renderer
    private HashSet<UILineRenderer> room_liners = new();

    [Header("Logs")]
    public bool log_no_instance_found_at_start = false; // log if no RoomSystem instance is found at start

    // CALCULATE WORLD OFFSET
    public Vector2 CalculateWorldCenter(out Vector2 extents)
    {
        // grab all the rooms data in the RoomSystem
        List<RoomData> rooms = RoomEngine.Instance.rooms_data.Values.ToList();

        // compute the world bounds of all rooms to set the size of our canvas accordingly
        Bounds world_bounds = compute_world_bounds(rooms);
        extents = world_bounds.extents;
        return world_bounds.center;
    }
    private Bounds compute_world_bounds(IEnumerable<RoomData> rooms)
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

    // VISUALS CREATION
    public void ClearVisuals()
    {
        foreach (UILineRenderer liner in room_liners) { if (liner != null) { Destroy(liner.gameObject); } }
        room_liners.Clear();
    }
    public void CreateVisuals(string level_id = null)
    {
        // if level id is null, we get the current level id from the LevelEngine
        if (level_id == null) { level_id = LevelEngine.Instance.CurrentLevelID; }
        if (level_id == null) { if (log_no_instance_found_at_start) { Debug.LogWarning($"(RoomVisualizer) Can't find the current level ID"); } return; }

        // grab the rooms data of the level from the LevelEngine
        List<RoomData> rooms = LevelEngine.Instance.GetRoomsDataOfLevel(level_id);

        // and build a visual for each room
        foreach (RoomData room in rooms) { create_visu_for_room(room); }
    }
    private void create_visu_for_room(RoomData rdata)
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
            // Vector2 canvas_point = UI_Manager.WorldToCanvasLocal(world_point, mapRoot, liner.canvas, Camera.main);
            // Vector2 final_ui_point = (canvas_point + worldOffset + UI_DevMap.global_offset)* UI_DevMap.global_zoom;
            Vector2 final_ui_point = UI_DevMap.GetUIPositionFromWorldPosition(world_point);
            points.Add(final_ui_point);
        }
        points.Add(points[0]); // we close the loop by adding the first point at the end
        liner.Points = points.ToArray();

        // 3. add the liner to our hashset
        room_liners.Add(liner);
    }

}