using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{

    [Header("Room data")]
    public RoomData data;
    public bool Loaded { get { return data != null; } }

    [Header("Room shown")]
    public bool shown = false; // if the room is currently shown. =/= is the room loaded. shown is only about visibility

    [Header("Room collider")]
    public PolygonCollider2D room_collider;

    [Header("Room neighbours")]
    public List<string> neighbours = new List<string>();

    [Header("Tilemaps")]
    public Tilemap ceiling_tilemap;
    private TilemapRenderer ceiling_renderer;
    public Tilemap walls_tilemap;
    private TilemapRenderer walls_renderer;
    public Tilemap carpet_tilemap;
    private TilemapRenderer carpet_renderer;
    public Tilemap ground_tilemap;
    private TilemapRenderer ground_renderer;

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

        // here we need to load all the capables that we hold in data.capables_ids
        if ((data.capables_ids == null || data.capables_ids.Count == 0)
        && (data.movables_ids == null || data.movables_ids.Count == 0)) { return; }
        if (CapableSystem.Instance != null)
        {
            CapableSystem.Instance.LoadCapables(data.capables_ids);
            CapableSystem.Instance.LoadCapables(data.movables_ids);
        }
    }
    public void UnloadData()
    {
        // here we need to unload all the capables that we hold
        // -> interacts with CapableSystem
        if (CapableSystem.Instance != null)
        {
            CapableSystem.Instance.UnloadCapables(data.capables_ids);
            CapableSystem.Instance.UnloadCapables(data.movables_ids);
        }

        this.data = null;
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

        // set capables data
        data.capables_ids = data.capables_ids ?? new List<string>();
        data.movables_ids = data.movables_ids ?? new List<string>();

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






    // SHOW / HIDE
    public void Show()
    {
        /// this should NOT disable the gameobject since we want all the logic to keep logiking
        /// we only want to disable all renderers + lights etc any visible thing

        // enable tilemaps renderer
        associate_renderers();
        ceiling_renderer.enabled = true;
        walls_renderer.enabled = true;
        carpet_renderer.enabled = true;
        ground_renderer.enabled = true;

        // show capables that we have

        // show lights

        // finally we are shown
        shown = true;
    }
    public void Hide()
    {
        // disable tilemaps renderer
        associate_renderers();
        ceiling_renderer.enabled = false;
        walls_renderer.enabled = false;
        carpet_renderer.enabled = false;
        ground_renderer.enabled = false;

        // hide capables that we have

        // hide lights

        // finally we are hidden
        shown = false;
    }
    private void associate_renderers()
    {
        if (ceiling_renderer == null) { ceiling_renderer = ceiling_tilemap.GetComponent<TilemapRenderer>(); }
        if (walls_renderer == null) { walls_renderer = walls_tilemap.GetComponent<TilemapRenderer>(); }
        if (carpet_renderer == null) { carpet_renderer = carpet_tilemap.GetComponent<TilemapRenderer>(); }
        if (ground_renderer == null) { ground_renderer = ground_tilemap.GetComponent<TilemapRenderer>(); }
    }







    // COLLIDERS EVENTS
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        Capable capable = collider.GetComponent<Capable>();
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); }
        if (capable == null) { return; }

        // check some bools
        bool in_movables = data.movables_ids.Contains(capable.data.id) || data.capables_ids.Contains(capable.data.id);
        bool in_out_movables = data.OUT_movables_ids.Contains(capable.data.id);

        if (in_movables && !in_out_movables)
        {
            // if the capable is already in the room and has not gone out of the room, it means it teleported (happens on awake)
            if (log_colliders) { Debug.Log($"(Room - {this.name}) Ignored IN - " + capable.data.id + " (should happen on a capable spawn otherwise it s weird)"); }
            return;
        }
        if (in_movables && in_out_movables)
        {
            // if the capable is in the OUT list and in the movables one it means it went out, did not find any other room to go to, and came back to main room,
            // so we simply remove both in and out for this capable
            data.OUT_movables_ids.Remove(capable.data.id);
            data.IN_movables_ids.Remove(capable.data.id);
            if (log_colliders) { Debug.Log($"(Room - {this.name}) Ignored OUT then IN - " + capable.data.id); }
            return;
        }

        // capable enters !
        data.IN_movables_ids.Add(capable.data.id);
        if (log_colliders) { Debug.Log($"(Room - {this.name}) IN - " + capable.data.id); }
    }
    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (data == null) { return; }

        Capable capable = collider.GetComponent<Capable>();
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); }
        if (capable == null) { return; }

        // check some bools
        // bool in_movables = data.movables_ids.Contains(capable.data.id);
        bool in_out_movables = data.IN_movables_ids.Contains(capable.data.id);
        if (in_out_movables)
        {
            // if the capable is in the OUT list and in the movables one it means it went out, did not find any other room to go to, and came back to main room,
            // so we simply remove both in and out for this capable
            data.IN_movables_ids.Remove(capable.data.id);
            data.OUT_movables_ids.Remove(capable.data.id);
            if (log_colliders) { Debug.Log($"(Room - {this.name}) Ignored IN then OUT - " + capable.data.id); }
            return;
        }

        // capable exits !
        data.OUT_movables_ids.Add(capable.data.id);
        if (log_colliders) { Debug.Log($"(Room - {this.name}) OUT - " + capable.data.id); }
    }

    // COLLIDER OVERLAP
    private ContactFilter2D? _contact_filter = null;
    private ContactFilter2D contact_filter
    {
        get
        {
            if (_contact_filter != null) { return _contact_filter.Value; }
            ContactFilter2D filter = new ContactFilter2D();
            filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Objects", "Feet"));
            filter.useTriggers = true;
            _contact_filter = filter;
            return filter;
        }
    }
    public void GetOverlappingCapablesIDs(out List<string> overlapping_capables, out List<string> overlapping_movables)
    {
        overlapping_capables = new List<string>();
        overlapping_movables = new List<string>();

        // we get all the colliders that are currently overlapping with the room collider
        Collider2D[] colliders = new Collider2D[30];
        int count = Physics2D.OverlapCollider(room_collider, contact_filter, colliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = colliders[i];
            Capable capable = collider.GetComponent<Capable>();
            if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); }
            if (capable == null) { continue; }
            
            // we found a capable !
            if (capable is Movable) { overlapping_movables.Add(capable.data.id); }
            else { overlapping_capables.Add(capable.data.id); }
        }
    }

}