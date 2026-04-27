using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{

    [Header("Room data")]
    public RoomData data;
    public bool Loaded { get { return data != null; } }
    private bool _unloading = false;
    public string ID { get { return GetStaticID(); } }

    [Header("Components")]
    private PolygonCollider2D _room_collider;
    public PolygonCollider2D RoomCollider
    {
        get
        {
            if (_room_collider == null) { _room_collider = GetComponent<PolygonCollider2D>(); }
            return _room_collider;
        }
    }

    private Dictionary<Vector2,Light2D> loaded_lights = new Dictionary<Vector2, Light2D>();
    private Transform _lights_parent;
    public Transform LightsParent
    {
        get
        {
            if (_lights_parent is null)
            {
                _lights_parent = transform.Find("Lights");
                if (_lights_parent == null)
                {
                    GameObject lights_go = new GameObject("Lights");
                    lights_go.transform.SetParent(transform);
                    _lights_parent = lights_go.transform;
                }
            }
            return _lights_parent;
        }
    }

    // LOAD / UNLOAD
    public void LoadData(RoomData data)
    {
        this.data = data;
        this.name = data.id;
        this.transform.position = data.position;

        // load the colliders in the composite collider
        RoomCollider.SetPath(0, data.collider_points.ToArray());
        RoomCollider.enabled = true;

        // load the lights
        // load_lights(data.lights_data);
        RoomEngine.Instance.LightsEngine.LoadLights(data.lights_data, data.id);

        // show the tilemaps
        RoomEngine.Instance.TilemapEngine.ShowTilemaps(data);

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
        _unloading = true;

        // we hide the tilemaps
        RoomEngine.Instance.TilemapEngine.HideTilemaps(data);

        // unload the collider
        RoomCollider.enabled = false;

        // here we need to unload all the capables that we hold
        // -> interacts with CapableSystem
        if (CapableSystem.Instance != null)
        {
            CapableSystem.Instance.UnloadCapables(data.capables_ids);
            CapableSystem.Instance.UnloadCapables(data.movables_ids);
        }

        this.data = null;
        _unloading = false;
    }
    /* private void load_lights(List<LightData> lights_data)
    {
        if (lights_data == null) { return; }

        // we go through the lights data and create a new light for each of them
        foreach (LightData light_data in lights_data)
        {
            if (loaded_lights.ContainsKey(light_data.position)) { continue; }

            Light2D new_light = Instantiate(light_prefab, LightsParent);
            new_light.transform.position = light_data.position;
            new_light.color = light_data.color;
            new_light.intensity = light_data.intensity;
            new_light.pointLightInnerRadius = light_data.radius.x;
            new_light.pointLightOuterRadius = light_data.radius.y;
            new_light.falloffIntensity = light_data.falloff;
            loaded_lights[light_data.position] = new_light;
        }
    } */


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
            id = GetStaticID(),
            position = this.transform.position,

            // set collider data
            // collider_points = new List<Vector2>(RoomCollider.GetPath(0)),
            collider_points = get_static_collider_points(),

            // set neighbours data
            neighbours_ids = data.neighbours_ids ?? new List<string>(),

            // set capables data
            capables_ids = data.capables_ids ?? new List<string>(),
            movables_ids = data.movables_ids ?? new List<string>(),

            // set lights data
            lights_data = get_static_light_data(),
        };

        // set tilemaps data
        get_static_tilemaps(ref new_data);

        return new_data;
    }
    public string GetStaticID()
    {
        string id = this.name;
        if (this.data == null) { return id; }
        if (string.IsNullOrEmpty(this.data.id)) { return id; }
        return this.data.id;
    }
    protected List<LightData> get_static_light_data()
    {
        List<LightData> lights_data = new List<LightData>();

        // we go through our LightsTransform
        foreach (Transform light_transform in LightsParent)
        {
            // we get the Light2D component on it
            Light2D light = light_transform.GetComponent<Light2D>();
            if (light == null) { continue; }

            // we create a new LightData with the data of the visu and we add it to the list
            LightData light_data = new LightData()
            {
                position = light.transform.position,
                color = light.color,
                intensity = light.intensity,
                radius = new Vector2(light.pointLightInnerRadius, light.pointLightOuterRadius),
                falloff = light.falloffIntensity,
            };
            lights_data.Add(light_data);
        }
        return lights_data;
    }
    protected void get_static_tilemaps(ref RoomData room_data)
    {
        // get the tilemaps
        Tilemap ceiling_tilemap = transform.Find("ceiling")?.GetComponent<Tilemap>();
        Tilemap walls_tilemap = transform.Find("walls")?.GetComponent<Tilemap>();
        Tilemap carpet_tilemap = transform.Find("carpet")?.GetComponent<Tilemap>();
        Tilemap ground_tilemap = transform.Find("ground")?.GetComponent<Tilemap>();

        // get the tiles & tilebases & bounds
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
        // check if tilemap is null
        if (tilemap == null) { bounds = new BoundsInt(); return new int[0]; }

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
    protected List<Vector2> get_static_collider_points()
    {
        List<Vector2> points = new List<Vector2>();
        if (RoomCollider == null) { return points; }
        if (RoomCollider.pathCount == 0) { return points; }
        points = new List<Vector2>(RoomCollider.GetPath(0));
        
        // we go through all the points and apply the collider' offset to get them real pos
        for (int i = 0; i < points.Count; i++)
        {
            points[i] += RoomCollider.offset;
        }

        return points;
    }
    public void AddStaticNeighbor(Room neighbor)
    {
        // if (data == null) { return; }
        if (data.neighbours_ids == null) { data.neighbours_ids = new List<string>(); }
        if (data.neighbours_ids.Contains(neighbor.GetStaticID())) { return; }
        data.neighbours_ids.Add(neighbor.GetStaticID());
    }
    public void RemoveStaticNeighbor(Room neighbor)
    {
        if (data.neighbours_ids == null) { return; }
        if (!data.neighbours_ids.Contains(neighbor.GetStaticID())) { return; }
        data.neighbours_ids.Remove(neighbor.GetStaticID());
    }
    public void ClearStaticNeighbors()
    {
        if (data.neighbours_ids == null) { return; }
        data.neighbours_ids.Clear();
    }



    // SHOW / HIDE
    /* public void Show()
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
    } */







    // COLLIDERS EVENTS
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        Capable capable = collider.GetComponent<Capable>(); // some old objects have feet collider directly on them
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); } // some old movables have feet collider on feet -> child of the capable
        if (capable == null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); } // new obj have feet collider as child of feet -> grand child of the capable
        if (capable == null) { return; }
        if (capable.data == null) { return; } // if we don't have data, we can't do anything with it, so we ignore the trigger

        // check if capable is not the controlled one and not in the capable system
        // we just ignore the trigger
        if (Controller.Instance.Capable != capable && !CapableBank.Instance.HasCapable(capable)) { return; }
        string id = capable.data.id;

        if (RoomEngine.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) {id} - IN -"); }
        
        // check if we should ignore the trigger bcz the capable just attached to something in the room
        if (RoomEngine.Instance.ShouldIgnoreRoomTrigger(id)) { return; }

        // directly call RoomEngine.OnRoomEnter
        RoomEngine.Instance.OnRoomEnter(this.data, capable);

    }
    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (data == null) { return; }
        if (AppManager.Instance.IsQuitting) { return; }
        if (RoomEngine.Instance == null) { return; }
        if (_unloading) { return; } // if we are unloading the room we don't want any trigger event

        Capable capable = collider.GetComponent<Capable>();
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); }
        if (capable == null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); }
        if (capable == null) { return; }

        if (Controller.Instance.Capable != capable && !CapableBank.Instance.HasCapable(capable)) { return; }
        string id = capable.data.id;
        if (RoomEngine.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) {id} - OUT -"); }

        // ! is data.id null ? if we unload the capable, could be

        // check if we should ignore the trigger bcz the capable just attached/detached to something in the room
        if (RoomEngine.Instance.ShouldIgnoreRoomTrigger(id)) { return; }

        // if this is a grabbed item then we do nothing (was freed when grabbed)
        // if (capable is Item item && item.Grabbed) { return; }

        // directly call RoomEngine.OnRoomExit
        RoomEngine.Instance.OnRoomExit(this.data, capable);

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
        int count = Physics2D.OverlapCollider(RoomCollider, contact_filter, colliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = colliders[i];
            Capable capable = collider.GetComponent<Capable>();
            if (capable == null) { capable = collider.GetComponentInParent<Capable>(); }
            if (capable == null) { continue; }

            // we found a capable !
            if (capable is Movable) { overlapping_movables.Add(capable.GetStaticID()); }
            else { overlapping_capables.Add(capable.GetStaticID()); }
        }
    }
    public List<Capable> GetStaticOverlappingCapables()
    {
        List<Capable> overlapping_capables = new List<Capable>();

        // we get all the colliders that are currently overlapping with the room collider
        Collider2D[] colliders = new Collider2D[100];
        int count = Physics2D.OverlapCollider(RoomCollider, contact_filter, colliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = colliders[i];
            Capable capable = collider.GetComponent<Capable>();
            if (capable == null && collider.transform.parent != null) { capable = collider.transform.parent.GetComponent<Capable>(); }
            if (capable == null && collider.transform.parent != null && collider.transform.parent.parent != null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); }
            if (capable == null) { continue; }
            
            // we found a capable !
            overlapping_capables.Add(capable);
        }
        return overlapping_capables;
    }
    public Bounds GetStaticBounds() { return RoomCollider.bounds; }
    public Tilemap GetStaticTilemap(string tilemap_type)
    {
        Transform tilemap_transform = transform.Find(tilemap_type);
        if (tilemap_transform == null) { return null; }
        Tilemap tilemap = tilemap_transform.GetComponent<Tilemap>();
        return tilemap;
    }
}