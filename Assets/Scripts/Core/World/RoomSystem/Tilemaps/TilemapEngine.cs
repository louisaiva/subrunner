using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapEngine : MonoBehaviour
{
    
    [Header("Cache")]
    private Dictionary<string, RoomTilemaps> room_tilemaps = new Dictionary<string, RoomTilemaps>();
    private Dictionary<string, TileBase> tilebase_cache = new Dictionary<string, TileBase>();

    [Header("Tilemaps parent")]
    public Transform GroundParent;
    public Transform WallsParent; // also for ceiling & carpet & edges

    [Header("Tilemaps prefabs")]
    [SerializeField] private RoomTilemap edges_prefab;
    [SerializeField] private RoomTilemap ceiling_prefab;
    [SerializeField] private RoomTilemap walls_prefab;
    [SerializeField] private RoomTilemap carpet_prefab;
    [SerializeField] private RoomTilemap ground_prefab;
    public Dictionary<string, RoomTilemap> TilemapPrefabs
    {
        get
        {
            return new Dictionary<string, RoomTilemap>()
            {
                { "edges", edges_prefab },
                { "ceiling", ceiling_prefab },
                { "walls", walls_prefab },
                { "carpet", carpet_prefab },
                { "ground", ground_prefab }
            };
        }
    }

    [Header("Logs")]
    public bool log_building = false;
    public bool log_tilebases = false;
    public bool log_showing = false;




    ///
    //
    /// BUILD TILEMAPS
    //
    ///

    // BUILD ROOM TILEMAPS
    private List<TileBase> tilebases_used = new List<TileBase>();
    private RoomTilemaps BuildRoomTilemaps(RoomData data)
    {
        // we load the tilebases used in data
        load_tilebases(data);

        // we check if we already have the tilemaps for this room
        RoomTilemaps room_tmps;
        room_tilemaps.TryGetValue(data.id, out room_tmps);
        if (room_tmps == null)
        {
            // we don't have the tilemaps for this room, we create them
            room_tmps = new RoomTilemaps(data);
            room_tilemaps[data.id] = room_tmps;
            room_tmps.Build(data, tilebases_used);
            if (log_building) { Debug.Log($"(TilemapEngine) Created & Built tilemaps for room: {data.id}"); }
            return room_tmps;
        }
        room_tmps.Build(data, tilebases_used);
        if (log_building) { Debug.Log($"(TilemapEngine) Built tilemaps for room: {data.id}"); }
        return room_tmps;
    }
    private void load_tilebases(RoomData data)
    {
        // we load the tilebases used in data
        tilebases_used.Clear();
        if (data.tilebase_paths_used == null) { return; }
        for (int i = 0; i < data.tilebase_paths_used.Length; i++)
        {
            tilebases_used.Add(get_or_load_tile_base(data.tilebase_paths_used[i]));
        }

        if (log_tilebases) { Debug.Log($"(TilemapEngine) Loaded {tilebases_used.Count} tilebases for room: {data.id} ({string.Join(", ", tilebases_used.Select(t => t.name))})"); }
    }
    private TileBase get_or_load_tile_base(string tilebase_path)
    {
        if (tilebase_cache.TryGetValue(tilebase_path, out TileBase tilebase))
        {
            return tilebase;
        }
        
        // we don't have the tilebase in cache, we load it and add it to cache
        tilebase = Resources.Load<TileBase>(tilebase_path);
        if (tilebase == null) { Debug.LogError($"(TilemapEngine) Failed to load tilebase at path: {tilebase_path}"); }
        tilebase_cache[tilebase_path] = tilebase;
        return tilebase;
    }
    public RoomTilemaps BuildTilemapsForAIO_Room(Room room)
    {
        RoomData data = room.data;

        // we load the tilebases used in data
        load_tilebases(data);

        // we create a new RoomTilemaps with AIO Constructor, and build it
        RoomTilemaps room_tmps = new RoomTilemaps(data, room);
        room_tmps.Build(data, tilebases_used);
        if (log_building) { Debug.Log($"(TilemapEngine) Created & Built aio tilemaps for room: {data.id}"); }
        return room_tmps;
    }


    ///
    //
    /// SHOW / HIDE
    //
    ///


    // SHOW / HIDE ROOM TILEMAPS
    public void ShowTilemaps(RoomData data)
    {
        if (room_tilemaps.TryGetValue(data.id, out RoomTilemaps room_tmps))
        {
            room_tmps.Show();
            if (log_showing) { Debug.Log($"(TilemapEngine) Shown tilemaps of {data.id}"); }
            return;
        }

        // else we have no tilemaps for this room, we try to build it
        room_tmps = BuildRoomTilemaps(data);
        if (room_tmps == null) { Debug.LogError($"(TilemapEngine) Failed to build tilemaps for room: {data.id}"); return; }
        room_tmps.Show();
        if (log_showing) { Debug.Log($"(TilemapEngine) Built & Shown tilemaps of {data.id}"); }
    }
    public void HideTilemaps(RoomData data)
    {
        if (room_tilemaps.TryGetValue(data.id, out RoomTilemaps room_tmps))
        {
            room_tmps.Hide();
            if (log_showing) { Debug.Log($"(TilemapEngine) Hidden tilemaps of {data.id}"); }
            return;
        }
        if (log_showing) { Debug.LogWarning($"(TilemapEngine) No tilemaps to hide for {data.id}"); }
    }


    ///
    //
    /// CLEAR TILEMAPS
    //
    ///
    public void ClearTilemaps(bool log)
    {
        // we destroy the tilemaps
        for (int i = 0; i < room_tilemaps.Count; i++)
        {
            RoomTilemaps room_tmps = room_tilemaps.ElementAt(i).Value;
            room_tmps.Destroy();
        }

        room_tilemaps.Clear();
        tilebase_cache.Clear();
        if (log) { Debug.Log($"(TilemapEngine) Tilemaps cleared"); }
    }

}


/// <summary>
/// RUNTIME ONLY CLASS, useful only for TilemapEngine to handle the tilemaps
/// building & showing
/// </summary>
public class RoomTilemaps
{
    public string RoomID { get; set; }

    public RoomTilemap ceiling_tilemap;
    public RoomTilemap edges_tilemap;
    public RoomTilemap walls_tilemap;
    public RoomTilemap carpet_tilemap;
    public RoomTilemap ground_tilemap;

    // CONSTRUCTOR
    public RoomTilemaps(RoomData data)
    {
        Transform parent = RoomEngine.Instance.TilemapEngine.WallsParent;
        Dictionary<string, RoomTilemap> prefabs = RoomEngine.Instance.TilemapEngine.TilemapPrefabs;

        // ceiling
        if (data.HasTiles("ceiling"))
        {
            ceiling_tilemap = GameObject.Instantiate(prefabs["ceiling"], parent);
            ceiling_tilemap.RoomID = data.id;
            ceiling_tilemap.gameObject.name = $"{data.id}_ceiling";
            ceiling_tilemap.transform.position += (Vector3)data.position;
        }

        // walls
        if (data.HasTiles("walls"))
        {
            walls_tilemap = GameObject.Instantiate(prefabs["walls"], parent);
            walls_tilemap.RoomID = data.id;
            walls_tilemap.gameObject.name = $"{data.id}_walls";
            walls_tilemap.transform.position += (Vector3)data.position;
        }

        // carpet
        if (data.HasTiles("carpet"))
        {
            carpet_tilemap = GameObject.Instantiate(prefabs["carpet"], parent);
            carpet_tilemap.RoomID = data.id;
            carpet_tilemap.gameObject.name = $"{data.id}_carpet";
            carpet_tilemap.transform.position += (Vector3)data.position;
        }

        // edges
        if (data.HasTiles("edges"))
        {
            edges_tilemap = GameObject.Instantiate(prefabs["edges"], parent);
            edges_tilemap.RoomID = data.id;
            edges_tilemap.gameObject.name = $"{data.id}_edges";
            edges_tilemap.transform.position += (Vector3)data.position;
        }

        parent = RoomEngine.Instance.TilemapEngine.GroundParent;

        // ground
        if (data.HasTiles("ground"))
        {
            ground_tilemap = GameObject.Instantiate(prefabs["ground"], parent);
            ground_tilemap.RoomID = data.id;
            ground_tilemap.gameObject.name = $"{data.id}_ground";
            ground_tilemap.transform.position += (Vector3)data.position;
        }
    }
    public RoomTilemaps(RoomData data, Room room) // this is the aio version of the constructor
    {
        Dictionary<string, RoomTilemap> prefabs = RoomEngine.Instance.TilemapEngine.TilemapPrefabs;

        // ceiling
        if (data.HasTiles("ceiling"))
        {
            ceiling_tilemap = room.transform.Find("ceiling")?.GetComponent<RoomTilemap>();
        }

        // walls
        if (data.HasTiles("walls"))
        {
            walls_tilemap = room.transform.Find("walls")?.GetComponent<RoomTilemap>();
        }

        // carpet
        if (data.HasTiles("carpet"))
        {
            carpet_tilemap = room.transform.Find("carpet")?.GetComponent<RoomTilemap>();
        }

        // edges
        if (data.HasTiles("edges"))
        {
            edges_tilemap = room.transform.Find("edges")?.GetComponent<RoomTilemap>();
        }

        // ground
        if (data.HasTiles("ground"))
        {
            ground_tilemap = room.transform.Find("ground")?.GetComponent<RoomTilemap>();
        }
    }

    // BUILDER
    public void Build(RoomData data, List<TileBase> tilebases_used)
    {
        // we build the tilemaps
        ceiling_tilemap?.BuildTilemap(tilebases_used, data.ceiling_tiles, data.ceiling_bounds);
        walls_tilemap?.BuildTilemap(tilebases_used, data.walls_tiles, data.walls_bounds);
        carpet_tilemap?.BuildTilemap(tilebases_used, data.carpet_tiles, data.carpet_bounds);
        ground_tilemap?.BuildTilemap(tilebases_used, data.ground_tiles, data.ground_bounds);
        edges_tilemap?.BuildTilemap(tilebases_used, data.edges_tiles, data.edges_bounds);
    }

    // ENABLER
    public bool shown = false;
    public void Show()
    {
        if (ceiling_tilemap != null) { ceiling_tilemap.Renderer.enabled = true; }
        if (walls_tilemap != null) { walls_tilemap.Renderer.enabled = true; }
        if (ground_tilemap != null) { ground_tilemap.Renderer.enabled = true; }
        if (carpet_tilemap != null) { carpet_tilemap.Renderer.enabled = true; }
        if (edges_tilemap != null) { edges_tilemap.Renderer.enabled = true; }
        shown = true;
    }
    public void Hide()
    {
        if (ceiling_tilemap != null) { ceiling_tilemap.Renderer.enabled = false; }
        if (walls_tilemap != null) { walls_tilemap.Renderer.enabled = false; }
        if (ground_tilemap != null) { ground_tilemap.Renderer.enabled = false; }
        if (carpet_tilemap != null) { carpet_tilemap.Renderer.enabled = false; }
        if (edges_tilemap != null) { edges_tilemap.Renderer.enabled = false; }
        shown = false;
    }

    // CLEAR
    public void Destroy()
    {
        if (ceiling_tilemap != null) { GameObject.Destroy(ceiling_tilemap.gameObject); }
        if (walls_tilemap != null) { GameObject.Destroy(walls_tilemap.gameObject); }
        if (ground_tilemap != null) { GameObject.Destroy(ground_tilemap.gameObject); }
        if (carpet_tilemap != null) { GameObject.Destroy(carpet_tilemap.gameObject); }
        if (edges_tilemap != null) { GameObject.Destroy(edges_tilemap.gameObject); }
    }
}