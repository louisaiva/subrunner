using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{

    [Header("Room data")]
    public RoomData data;
    public bool Loaded { get { return data != null; } }

    [Header("Room collider")]
    public PolygonCollider2D room_collider;

    [Header("Tilemaps")]
    public Tilemap ceiling_tilemap;
    public Tilemap walls_tilemap;
    public Tilemap carpet_tilemap;
    public Tilemap ground_tilemap;

    // LOAD / UNLOAD
    public void LoadData(RoomData data)
    {
        this.data = data;
        this.name = data.id;
        this.transform.position = data.position;

        // load the colliders in the composite collider
        room_collider.SetPath(0, data.collider_points.ToArray());

        // load the tilemaps
        set_tilemaps();
    }
    public void UnloadData()
    {
        this.data = null;

        // here we can save the data if the tilemaps changed ?
    }


    // loading tilemaps low level
    protected void set_tilemaps()
    {
        // ceiling
        set_tilemap(ceiling_tilemap, data.ceiling_tiles, data.ceiling_bounds);
        // walls
        set_tilemap(walls_tilemap, data.walls_tiles, data.walls_bounds);
        // carpet
        set_tilemap(carpet_tilemap, data.carpet_tiles, data.carpet_bounds);
        // ground
        set_tilemap(ground_tilemap, data.ground_tiles, data.ground_bounds);
    }
    protected void set_tilemap(Tilemap tilemap, TileBase[] tiles, BoundsInt bounds)
    {
        tilemap.ClearAllTiles();
        tilemap.SetTilesBlock(bounds, tiles);
        tilemap.ResizeBounds();
        tilemap.CompressBounds();
    }


    // SAVE CURRENT DATA
    public RoomData UpdateData()
    {
        // we take current data and we write it down inside this.data
        // ex : when we changed a tilemap we need to update the data equivalent
        // otherwise it will erase all modifications on load

        if (data == null) { this.data = new RoomData(); }

        // set base data things
        data.id = this.name;
        data.position = this.transform.position;

        // set collider data
        data.collider_points = new List<Vector2>(room_collider.GetPath(0));

        // set tilemaps data
        data.ceiling_tiles = get_tilemap(ceiling_tilemap, out data.ceiling_bounds);
        data.walls_tiles = get_tilemap(walls_tilemap, out data.walls_bounds);
        data.carpet_tiles = get_tilemap(carpet_tilemap, out data.carpet_bounds);
        data.ground_tiles = get_tilemap(ground_tilemap, out data.ground_bounds);
        return data;
    }
    protected TileBase[] get_tilemap(Tilemap tilemap, out BoundsInt bounds)
    {
        tilemap.CompressBounds();
        bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);
        /* for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = tiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
                else
                {
                    Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        } */
        return tiles;
    }

}