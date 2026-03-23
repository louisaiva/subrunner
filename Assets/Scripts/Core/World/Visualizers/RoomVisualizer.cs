using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// this class draws the border of every room in a Level. It is
/// connected to the RoomSystem to know all rooms.
/// </summary>
public class RoomVisualizer : MonoBehaviour
{

    [Header("Line Visu Prefab")]
    [SerializeField] private GameObject line_visu_prefab; // a line renderer

    private HashSet<LineRenderer> room_liners = new();

    // START
    private void Start()
    {
        // grab all the rooms data in the RoomSystem and build a visual for each room
        List<RoomData> rooms = RoomSystem.Instance.rooms_data.Values.ToList();
        foreach (RoomData room in rooms)
        {
            // Debug.Log("(RoomVisualizer) Creating visual for room: " + room.id);
            create_visu_for_room(room);
        }
    }

    private void create_visu_for_room(RoomData rdata)
    {
        // 1. instanciate a visu
        GameObject go = Instantiate(line_visu_prefab, transform);
        go.transform.position = rdata.position;
        go.name = rdata.id;

        // 2. set the line points
        LineRenderer liner = go.GetComponent<LineRenderer>();
        liner.positionCount = rdata.collider_points.Count;
        for (int i = 0; i < rdata.collider_points.Count; i++)
        {
            liner.SetPosition(i, rdata.collider_points[i] + rdata.position);
        }

        // 3. add the liner to our hashset
        room_liners.Add(liner);
    }

}