using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class RoomData : IData
{
    [field: SerializeField] public string id { get; set; }
    public Vector2 position;

    // colliders
    public List<Vector2> collider_points;

    // neighbours data
    public List<string> neighbours_ids; // list of the rooms that are directly connected to this one, used for loading/unloading logic

    // Capable management
    public List<string> capables_ids;
    public List<string> movables_ids;
    public int TotalCapablesCount { get { return capables_ids.Count + movables_ids.Count; } }
    public int TotalCapacitiesCount
    {
        get
        {
            // we get the capable data for each capable in the room and we sum their capacities count
            List<CapableData> capables_data = CapableSystem.Instance.GetCapablesDataFromIDs(capables_ids);
            capables_data.AddRange(CapableSystem.Instance.GetCapablesDataFromIDs(movables_ids));
            int count = 0;
            foreach (CapableData capable_data in capables_data)
            {
                count += capable_data.TotalCapacitiesCount();
            }
            return count;
        }
    }
    

    // lights management
    public List<LightData> lights_data;


    // tilemaps data
    public string[] tilebase_paths_used;
    public BoundsInt ceiling_bounds;
    public int[] ceiling_tiles;
    public BoundsInt walls_bounds;
    public int[] walls_tiles;
    public BoundsInt carpet_bounds;
    public int[] carpet_tiles;
    public BoundsInt ground_bounds;
    public int[] ground_tiles;


    // GETTERS
    protected int calculate_tilemap_non_null_tiles(int[] tiles)
    {
        int count = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != -1) { count++; }
        }
        return count;
    }
    public string GetDetails()
    {
        string details = $"Room {id} :\n";
        details += $"  - position : {position}\n";
        details += $"  - neighbours : {(neighbours_ids != null ? neighbours_ids.Count : 0)} rooms\n";
        details += $"  - capables : {(capables_ids != null ? capables_ids.Count : 0)} capables\n";
        details += $"  - movables : {(movables_ids != null ? movables_ids.Count : 0)} movables\n";
        details += $"  - colliders : {(collider_points != null ? collider_points.Count : 0)} points\n";
        details += $"  - lights : {(lights_data != null ? lights_data.Count : 0)} lights\n";
        details += $"  - tilemaps :\n";
        details += $"    - ceiling : {calculate_tilemap_non_null_tiles(ceiling_tiles)} tiles\n";
        details += $"    - walls : {calculate_tilemap_non_null_tiles(walls_tiles)} tiles\n";
        details += $"    - carpet : {calculate_tilemap_non_null_tiles(carpet_tiles)} tiles\n";
        details += $"    - ground : {calculate_tilemap_non_null_tiles(ground_tiles)} tiles\n";
        return details;
    }
    public bool HasTiles(string tilemap_type)
    {
        switch (tilemap_type)
        {
            case "ceiling":
                return ceiling_tiles != null && ceiling_tiles.Length > 0;
            case "walls":
                return walls_tiles != null && walls_tiles.Length > 0;
            case "carpet":
                return carpet_tiles != null && carpet_tiles.Length > 0;
            case "ground":
                return ground_tiles != null && ground_tiles.Length > 0;
            default:
                Debug.LogError($"(RoomData - HasTiles) Invalid tilemap type: {tilemap_type}");
                return false;
        }
    }
}


[Serializable] public class LightData
{
    // public string id;
    public Vector2 position;
    public Color color;
    public float intensity;
    public Vector2 radius; // inner & outer radius for the light falloff
    public float falloff; // how fast the light decreases
}