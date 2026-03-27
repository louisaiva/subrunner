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