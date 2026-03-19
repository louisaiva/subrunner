using System.Collections.Generic;
using System.Linq;
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
        // we load the tilebases used in data
        List<TileBase> tilebases_used = new List<TileBase>();
        if (data.tilebase_paths_used == null) { return; }
        for (int i = 0; i < data.tilebase_paths_used.Length; i++)
        {
            string tilebase_path = data.tilebase_paths_used[i];
            TileBase tilebase = Resources.Load<TileBase>(tilebase_path);
            if (tilebase == null) { Debug.LogError($"(Room) Failed to load tilebase at path: {tilebase_path}"); }
            tilebases_used.Add(tilebase);
        }

        // ceiling
        set_tilemap(ceiling_tilemap,tilebases_used, data.ceiling_tiles, data.ceiling_bounds);
        // walls
        set_tilemap(walls_tilemap,tilebases_used, data.walls_tiles, data.walls_bounds);
        // carpet
        set_tilemap(carpet_tilemap,tilebases_used, data.carpet_tiles, data.carpet_bounds);
        // ground
        set_tilemap(ground_tilemap,tilebases_used, data.ground_tiles, data.ground_bounds);
    }
    protected void set_tilemap(Tilemap tilemap, List<TileBase> tilebases, int[] tiles_data, BoundsInt bounds)
    {
        TileBase[] tiles = new TileBase[tiles_data.Length];
        for (int i = 0; i < tiles_data.Length; i++)
        {
            int tile_id = tiles_data[i];
            if (tile_id == -1) { tiles[i] = null; continue; }

            // the tile_id is the index inside tilebases
            tiles[i] = tilebases[tile_id];
        }
        set_tilemap(tilemap, tiles, bounds);
    }
    protected void set_tilemap(Tilemap tilemap, TileBase[] tiles, BoundsInt bounds)
    {
        if (RoomSystem.Instance.log_tilemaps_loading) { Debug.Log("(Room) Loading tilemap: " + tilemap.name + " with bounds: " + bounds + " and tiles count: " + tiles.Length); }

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


        if (RoomSystem.Instance.log_tilemaps_loading) { Debug.Log("(Room) Tilemap loaded: " + tilemap.name + " with bounds: " + tilemap.cellBounds + " and " + non_null_tiles + " non-null tiles" + tile_count_log); }
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
    public RoomData GetStaticData()
    {
        RoomData new_data = new RoomData
        {
            // set base data things
            id = get_static_id(),
            position = this.transform.position,

            // set collider data
            collider_points = new List<Vector2>(room_collider.GetPath(0)),

            // set neighbours data
            neighbours_ids = new List<string>(neighbours),

            // set capables data
            capables_ids = data.capables_ids ?? new List<string>(),
            movables_ids = data.movables_ids ?? new List<string>()
        };

        // set tilemaps data
        get_tilemaps(ref new_data);

        return new_data;
    }
    protected string get_static_id()
    {
        string id = this.name;
        if (this.data == null) { return id; }
        if (string.IsNullOrEmpty(this.data.id)) { return id; }
        return this.data.id;
    }
    protected void get_tilemaps(ref RoomData room_data)
    {
        TileBase[] used_tilebases = new TileBase[0];
        room_data.ceiling_tiles = get_tilemap(ceiling_tilemap, out room_data.ceiling_bounds, ref used_tilebases);
        room_data.walls_tiles = get_tilemap(walls_tilemap, out room_data.walls_bounds, ref used_tilebases);
        room_data.carpet_tiles = get_tilemap(carpet_tilemap, out room_data.carpet_bounds, ref used_tilebases);
        room_data.ground_tiles = get_tilemap(ground_tilemap, out room_data.ground_bounds, ref used_tilebases);

        // now we use AssetDatabase to get the path of the tiles bases
        string[] tilebase_paths_used = new string[used_tilebases.Length];
        #if UNITY_EDITOR
        for (int i = 0; i < used_tilebases.Length; i++)
        {
            TileBase tilebase = used_tilebases[i];
            string path = UnityEditor.AssetDatabase.GetAssetPath(tilebase);
            path = path.Replace("Assets/Resources/", "").Replace(".asset", "");
            tilebase_paths_used[i] = path;
        }
        #else
        for (int i = 0; i < used_tilebases.Length; i++) { tilebase_paths_used[i] = ""; }
        #endif
        room_data.tilebase_paths_used = tilebase_paths_used;
    }
    protected int[] get_tilemap(Tilemap tilemap, out BoundsInt bounds, ref TileBase[] tilebases_used)
    {
        tilemap.CompressBounds();
        bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);
        int[] tiles_data = new int[tiles.Length];
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = tiles[x + y * bounds.size.x];
                if (tile == null) { tiles_data[x + y * bounds.size.x] = -1; continue; }
                
                // check if we have it already in the used ones
                if (!tilebases_used.Contains(tile))
                {
                    tilebases_used = tilebases_used.Append(tile).ToArray();
                }

                // the tile_id is the index inside tilebases
                int tile_id = System.Array.IndexOf(tilebases_used, tile);
                tiles_data[x + y * bounds.size.x] = tile_id;
            }
        }
        return tiles_data;
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
        Capable capable = collider.GetComponent<Capable>(); // some old objects have feet collider directly on them
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); } // some old movables have feet collider on feet -> child of the capable
        if (capable == null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); } // new obj have feet collider as child of feet -> grand child of the capable
        if (capable == null) { return; }

        // check if capable is not the controlled one and not in the capable system
        // we just ignore the trigger
        if (Controller.Instance.Capable != capable && !CapableBank.Instance.HasCapable(capable)) { return; }
        string id = capable.data.id;

        // check if we are not already in the movables or capable + if we are not doing IN-OUT in the same room
        bool in_movables = data.movables_ids.Contains(id) || data.capables_ids.Contains(id);
        bool in_out_movables = data.OUT_movables_ids.Contains(id);
        if (in_movables && !in_out_movables)
        {
            // if the capable is already in the room and has not gone out of the room, it means it teleported (happens on awake)
            if (RoomSystem.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) Ignored IN - " + id + " (should happen on a capable spawn otherwise it s weird)"); }
            return;
        }
        if (in_movables && in_out_movables)
        {
            // if the capable is in the OUT list and in the movables one it means it went out, did not find any other room to go to, and came back to main room,
            // so we simply remove both in and out for this capable
            data.OUT_movables_ids.Remove(id);
            data.IN_movables_ids.Remove(id);
            if (RoomSystem.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) Ignored OUT then IN - " + id); }
            return;
        }

        // check if we are not already in the IN then it means we have 2 IN -> we put it directly in the movables
        if (data.IN_movables_ids.Contains(id))
        {
            /* data.IN_movables_ids.Remove(id);

            // check is capable or movable and add it to the right list
            if (capable is Movable) { data.movables_ids.Add(id); }
            else {data.capables_ids.Add(id); }
            if (RoomSystem.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) Grabbed 2x IN - " + id); } */
            return;
        }

        // capable enters !
        data.IN_movables_ids.Add(id);
        if (RoomSystem.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) IN - " + id); }
    }
    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (data == null) { return; }
        if (AppManager.Instance.IsQuitting) { return; }

        Capable capable = collider.GetComponent<Capable>();
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); }
        if (capable == null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); }
        if (capable == null) { return; }

        if (Controller.Instance.Capable != capable && !CapableBank.Instance.HasCapable(capable)) { return; }

        // check some bools
        // bool in_movables = data.movables_ids.Contains(capable.data.id);
        bool in_out_movables = data.IN_movables_ids.Contains(capable.data.id);
        if (in_out_movables)
        {
            // if the capable is in the OUT list and in the movables one it means it went out, did not find any other room to go to, and came back to main room,
            // so we simply remove both in and out for this capable
            data.IN_movables_ids.Remove(capable.data.id);
            data.OUT_movables_ids.Remove(capable.data.id);
            if (RoomSystem.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) Ignored IN then OUT - " + capable.data.id); }
            return;
        }

        // capable exits !
        data.OUT_movables_ids.Add(capable.data.id);
        if (RoomSystem.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) OUT - " + capable.data.id); }
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
            if (capable == null) { capable = collider.GetComponentInParent<Capable>(); }
            if (capable == null) { continue; }
            
            // we found a capable !
            if (capable is Movable) { overlapping_movables.Add(capable.data.id); }
            else { overlapping_capables.Add(capable.data.id); }
        }
    }

}