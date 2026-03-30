using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IData
{
    public string id { get; set; }
    string GetDetails();
}

[Serializable] public class LevelData : IData
{
    [field: SerializeField] public string id { get; set; }

    // rooms ids
    public List<string> rooms_ids; // list of the rooms that are part of this level
    public int TotalRoomsCount { get { return rooms_ids != null ? rooms_ids.Count : 0; } }
    public int TotalCapablesCount
    {
        get
        {
            List<RoomData> rooms_data = RoomEngine.Instance.GetRoomsDataFromIDs(rooms_ids);
            int count = 0;
            foreach (RoomData room_data in rooms_data)
            {
                count += room_data.TotalCapablesCount;
            }
            return count;
        }
    }
    public int TotalCapacitiesCount
    {
        get
        {
            List<RoomData> rooms_data = RoomEngine.Instance.GetRoomsDataFromIDs(rooms_ids);
            int count = 0;
            foreach (RoomData room_data in rooms_data)
            {
                count += room_data.TotalCapacitiesCount;
            }
            return count;
        }
    }

    // navmesh data
    public List<string> navmesh_data_paths;

    public string GetDetails()
    {
        string details = $"Level {id} :\n";
        details += $"  - rooms : {rooms_ids.Count} rooms\n  -{string.Join("\n  -", rooms_ids)}\n";
        details += $"  - navmesh data paths : {navmesh_data_paths.Count} paths\n  -{string.Join("\n  -", navmesh_data_paths)}\n";
        return details;
    }
}