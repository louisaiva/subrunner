using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] public class RoomData
{
    public string id;
    public Vector2 position;
    public List<Vector2> collider_points;

    // tilemaps data
    public BoundsInt ceiling_bounds;
    public TileBase[] ceiling_tiles;
    public BoundsInt walls_bounds;
    public TileBase[] walls_tiles;
    public BoundsInt carpet_bounds;
    public TileBase[] carpet_tiles;
    public BoundsInt ground_bounds;
    public TileBase[] ground_tiles;

    // neighbours data
    public List<string> neighbours_ids; // list of the rooms that are directly connected to this one, used for loading/unloading logic

    // Capable management
    public List<string> capables_ids;
    public List<string> movables_ids;
    public List<string> IN_movables_ids; // movable waiting to go in, not stored in movables_ids yet
    public List<string> OUT_movables_ids; // movables that are going out (!) are still stored in movables_ids
}