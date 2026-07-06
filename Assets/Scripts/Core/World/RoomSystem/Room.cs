using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [Header("Room data")]
    public RoomData data;
    public bool Loaded { get { return data != null; } }
    public string ID { get { return GetStaticID(); } }

    private Transform _lights_parent;
    public Transform LightsParent
    {
        get
        {
            if (_lights_parent == null)
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


    // STATIC DATA
    public string GetStaticID()
    {
        if (data == null) { return name; }
        if (string.IsNullOrEmpty(data.id)) { return name; }
        return data.id;
    }
    public RoomData GetStaticData()
    {
        RoomData new_data = new RoomData
        {
            // set base data things
            id = GetStaticID(),
            // set lights data
            lights_data = GetStaticLightsData(),
            chunks_ids = get_static_chunks_ids()
        };

        // set tilemaps data
        get_static_tilemaps(ref new_data);


        return new_data;
    }
    public List<LightData> GetStaticLightsData()
    {
        List<LightData> lights_data = new List<LightData>();
        if (LightsParent == null) { return lights_data; }

        // we go through our LightsTransform
        Light2D[] lights = LightsParent.GetComponentsInChildren<Light2D>(includeInactive:false);
        foreach (Light2D light in lights)
        {
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
    private List<string> get_static_chunks_ids()
    {
        // if we already have data we can just return the chunks ids from it
        if (data != null && data.chunks_ids != null && data.chunks_ids.Count > 0) { return data.chunks_ids; }

        // if we don't have data, we need to get the chunks ids from the children
        List<string> chunks_ids = new List<string>();
        Chunk[] chunks = GetComponentsInChildren<Chunk>(includeInactive: true);
        foreach (Chunk chunk in chunks) { chunks_ids.Add(chunk.ID); }
        return chunks_ids;
    }
    protected void get_static_tilemaps(ref RoomData room_data)
    {
        // get the tilemaps
        Tilemap edges_tilemap = transform.Find("edges")?.GetComponent<Tilemap>();
        Tilemap ceiling_tilemap = transform.Find("ceiling")?.GetComponent<Tilemap>();
        Tilemap walls_tilemap = transform.Find("walls")?.GetComponent<Tilemap>();
        Tilemap carpet_tilemap = transform.Find("carpet")?.GetComponent<Tilemap>();
        Tilemap ground_tilemap = transform.Find("ground")?.GetComponent<Tilemap>();

        // get the tiles & tilebases & bounds
        TileBase[] used_tilebases = new TileBase[0];
        room_data.edges_tiles = get_tilemap(edges_tilemap, out room_data.edges_bounds, ref used_tilebases);
        room_data.ceiling_tiles = get_tilemap(ceiling_tilemap, out room_data.ceiling_bounds, ref used_tilebases);
        room_data.walls_tiles = get_tilemap(walls_tilemap, out room_data.walls_bounds, ref used_tilebases);
        room_data.carpet_tiles = get_tilemap(carpet_tilemap, out room_data.carpet_bounds, ref used_tilebases);
        room_data.ground_tiles = get_tilemap(ground_tilemap, out room_data.ground_bounds, ref used_tilebases);

        // now we use AssetDatabase to get the path of the tiles bases
        string[] tilebase_paths_used = new string[used_tilebases.Length];
        for (int i = 0; i < used_tilebases.Length; i++)
        {
            TileBase tilebase = used_tilebases[i];
            tilebase_paths_used[i] = RoomEngine.Instance.TileBaseBank.GetNameFromTileBase(tilebase);
        }
        room_data.tilebases_names = tilebase_paths_used;
    }
    protected int[] get_tilemap(Tilemap tilemap, out BoundsIntData bounds, ref TileBase[] tilebases_used)
    {
        // check if tilemap is null
        if (tilemap == null) { bounds = new BoundsIntData(); return new int[0]; }

        tilemap.CompressBounds();
        BoundsInt boundsInt = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(boundsInt);
        int[] tiles_data = new int[tiles.Length];
        for (int x = 0; x < boundsInt.size.x; x++)
        {
            for (int y = 0; y < boundsInt.size.y; y++)
            {
                TileBase tile = tiles[x + y * boundsInt.size.x];
                if (tile == null) { tiles_data[x + y * boundsInt.size.x] = -1; continue; }

                // check if we have it already in the used ones
                if (!tilebases_used.Contains(tile))
                {
                    tilebases_used = tilebases_used.Append(tile).ToArray();
                }

                // the tile_id is the index inside tilebases
                int tile_id = System.Array.IndexOf(tilebases_used, tile);
                tiles_data[x + y * boundsInt.size.x] = tile_id;
            }
        }
        bounds = new BoundsIntData(boundsInt);
        return tiles_data;
    }
    public Tilemap GetStaticTilemap(string tilemap_type)
    {
        Transform tilemap_transform = transform.Find(tilemap_type);
        if (tilemap_transform == null) { return null; }
        Tilemap tilemap = tilemap_transform.GetComponent<Tilemap>();
        return tilemap;
    }
    public Chunk[] GetStaticChunks() { return GetComponentsInChildren<Chunk>(includeInactive: true); }
}