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
}