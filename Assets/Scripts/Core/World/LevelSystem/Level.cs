using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Level : MonoBehaviour
{
    [Header("Data")]
    public LevelData data;


    // START
    private void Start()
    {
        // for now we are a dummy we only tell the RoomSystem to
        // load all the rooms based on their data ids
        RoomSystem.Instance?.LoadRooms(data.rooms_ids.ToArray());
    }



    // GET STATIC DATA

    /// <summary>
    /// just as other GetStaticData() methods (ie Capable's one), this method
    /// is not meant to be run in a BUILD !!! IT WON T WORK because it does not
    /// update this.data . it creates a new data based from actual static
    /// variables states of the object. if run inside a build, it could overwrite
    /// some data such as tilebases_used paths which would break the save.
    /// </summary>
    /// <returns></returns>
    public LevelData GetStaticData()
    {
        LevelData new_data = new LevelData
        {
            // set base data things
            id = get_static_id(),
            rooms_ids = get_static_rooms_ids(),
            navmesh_data = get_static_navmesh_data()
        };

        return new_data;
    }
    protected string get_static_id()
    {
        string id = this.name;
        if (this.data == null) { return id; }
        if (string.IsNullOrEmpty(this.data.id)) { return id; }
        return this.data.id;
    }
    private NavMeshData get_static_navmesh_data()
    {
        if (data == null || data.navmesh_data == null)
        {
            Debug.LogWarning($"(Level) No navmesh data found for level '{get_static_id()}' when getting static data, returning null");
            return null;
        }
        return data.navmesh_data;
    }
    private List<string> get_static_rooms_ids()
    {
        if (data == null) { return new List<string>(); }
        if (data.rooms_ids != null && data.rooms_ids.Count > 0) { return data.rooms_ids; }

        // else we go statically get the rooms ids from the children rooms
        Room[] rooms = GetComponentsInChildren<Room>(includeInactive: true);
        List<string> rooms_ids = new List<string>();
        foreach (Room room in rooms)
        {
            string room_id = room.GetStaticData().id;
            if (!string.IsNullOrEmpty(room_id))
            {
                rooms_ids.Add(room_id);
            }
        }
        return rooms_ids;
    }
}