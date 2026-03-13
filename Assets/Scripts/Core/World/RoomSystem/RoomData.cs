using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IData
{
    string GetDetails();
}

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
    public List<string> IN_movables_ids; // movable waiting to go in, not stored in movables_ids yet
    public List<string> OUT_movables_ids; // movables that are going out (!) are still stored in movables_ids
    
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
        details += $"  - neighbours : {neighbours_ids.Count} rooms\n";
        details += $"  - capables : {capables_ids.Count} capables\n";
        details += $"  - movables : {movables_ids.Count} movables\n";
        details += $"  - colliders : {collider_points.Count} points\n";
        details += $"  - tilemaps :\n";
        details += $"    - ceiling : {calculate_tilemap_non_null_tiles(ceiling_tiles)} tiles\n";
        details += $"    - walls : {calculate_tilemap_non_null_tiles(walls_tiles)} tiles\n";
        details += $"    - carpet : {calculate_tilemap_non_null_tiles(carpet_tiles)} tiles\n";
        details += $"    - ground : {calculate_tilemap_non_null_tiles(ground_tiles)} tiles\n";
        return details;
    }
}