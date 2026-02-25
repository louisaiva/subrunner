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

    [Header("Room neighbours")]
    public List<string> neighbours = new List<string>();

    [Header("Tilemaps")]
    public Tilemap ceiling_tilemap;
    public Tilemap walls_tilemap;
    public Tilemap carpet_tilemap;
    public Tilemap ground_tilemap;

    [Header("Logs")]
    private bool log_tilemaps_loading = false;
    private bool log_colliders = true;

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

        // and neighbours (just for debug)
        neighbours = data.neighbours_ids;
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
        if (log_tilemaps_loading) { Debug.Log("(Room) Loading tilemap: " + tilemap.name + " with bounds: " + bounds + " and tiles count: " + tiles.Length); }

        // we count how many tiles we have in the data
        string tile_count_log = "\n\nTiles :";
        int non_null_tiles = 0;
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = tiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    tile_count_log += "\n   - x:" + x + " y:" + y + " tile:" + tile.name;
                    non_null_tiles++;
                }
                else
                {
                    tile_count_log += "\n   - x:" + x + " y:" + y + " tile: (null)";
                }
            }
        }

        tilemap.ClearAllTiles();
        tilemap.SetTilesBlock(bounds, tiles);
        tilemap.ResizeBounds();
        tilemap.CompressBounds();

        // we count how many tiles we have in the object now
        tile_count_log = "\n\nTiles :";
        non_null_tiles = 0;
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = tiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    tile_count_log += "\n   - x:" + x + " y:" + y + " tile:" + tile.name;
                    non_null_tiles++;
                }
                else
                {
                    tile_count_log += "\n   - x:" + x + " y:" + y + " tile: (null)";
                }
            }
        }


        if (log_tilemaps_loading) { Debug.Log("(Room) Tilemap loaded: " + tilemap.name + " with bounds: " + tilemap.cellBounds + " and " + non_null_tiles + " non-null tiles" + tile_count_log); }
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

        // set neighbours data
        data.neighbours_ids = new List<string>(neighbours);
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


    // COLLIDERS EVENTS
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Capable capable = collision.transform.parent.GetComponent<Capable>();
        if (capable == null) { return; }

        // check some bools
        bool in_movables = data.movables_ids.Contains(capable.ID);
        bool in_out_movables = data.OUT_movables_ids.Contains(capable.ID);

        if (in_movables && !in_out_movables)
        {
            // if the capable is already in the room and has not gone out of the room, it means it teleported (happens on awake)
            if (log_colliders) { Debug.Log($"(Room - {this.name}) Ignored IN - " + capable.ID + " (should be in awake otherwise it s weird)"); }
            return;
        }
        if (in_movables && in_out_movables)
        {
            // if the capable is in the OUT list and in the movables one it means it went out, did not find any other room to go to, and came back to main room,
            // so we simply remove both in and out for this capable
            data.OUT_movables_ids.Remove(capable.ID);
            data.IN_movables_ids.Remove(capable.ID);
            if (log_colliders) { Debug.Log($"(Room - {this.name}) Ignored OUT then IN - " + capable.ID); }
            return;
        }

        // capable enters !
        data.IN_movables_ids.Add(capable.ID);
        if (log_colliders) { Debug.Log($"(Room - {this.name}) IN - " + capable.ID); }
    }
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        Capable capable = collision.transform.parent.GetComponent<Capable>();
        if (capable == null) { return; }

        // check some bools
        // bool in_movables = data.movables_ids.Contains(capable.ID);
        bool in_out_movables = data.IN_movables_ids.Contains(capable.ID);
        if (in_out_movables)
        {
            // if the capable is in the OUT list and in the movables one it means it went out, did not find any other room to go to, and came back to main room,
            // so we simply remove both in and out for this capable
            data.IN_movables_ids.Remove(capable.ID);
            data.OUT_movables_ids.Remove(capable.ID);
            if (log_colliders) { Debug.Log($"(Room - {this.name}) Ignored IN then OUT - " + capable.ID); }
            return;
        }

        // capable exits !
        data.OUT_movables_ids.Add(capable.ID);
        if (log_colliders) { Debug.Log($"(Room - {this.name}) OUT - " + capable.ID); }
    }
}