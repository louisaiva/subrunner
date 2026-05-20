using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class RoomTilemap : MonoBehaviour
{
    public string RoomID { get; set; }


    [Header("Tilemap")]
    private Tilemap _tilemap;
    private Tilemap tilemap
    {
        get
        {
            if (_tilemap == null) { _tilemap = GetComponent<Tilemap>(); }
            return _tilemap;
        }
    }

    [Header("Renderer")]
    private TilemapRenderer _renderer;
    public TilemapRenderer Renderer
    {
        get
        {
            if (_renderer == null) { _renderer = GetComponent<TilemapRenderer>(); }
            return _renderer;
        }
    }





    public void BuildTilemap(List<TileBase> tilebases, int[] tiles_data, BoundsInt bounds)
    {
        TileBase[] tiles = new TileBase[tiles_data.Length];
        for (int i = 0; i < tiles_data.Length; i++)
        {
            int tile_id = tiles_data[i];
            if (tile_id == -1) { tiles[i] = null; continue; }

            // the tile_id is the index inside tilebases
            tiles[i] = tilebases[tile_id];
        }
        build_tilemap(tiles, bounds);
    }
    protected void build_tilemap(TileBase[] tiles, BoundsInt bounds)
    {
        if (ChunkEngine.Instance.log_tilemaps_loading) { Debug.Log("(RoomTilemap) Building tilemap: " + RoomID + " with bounds: " + bounds + " and tiles count: " + tiles.Length); }

        // we count how many tiles we have in the data
        string tile_count_log = "";
        int non_null_tiles = 0;
        if (ChunkEngine.Instance.log_tilemaps_loading)
        {
            tile_count_log = "\n\nTiles :";
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
        }

        tilemap.ClearAllTiles();
        tilemap.SetTilesBlock(bounds, tiles);
        tilemap.ResizeBounds();
        tilemap.CompressBounds();

        // we count how many tiles we have in the object now
        if (ChunkEngine.Instance.log_tilemaps_loading)
        {
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
        }

        if (ChunkEngine.Instance.log_tilemaps_loading) { Debug.Log("(RoomTilemap) Tilemap built: " + RoomID + " with bounds: " + tilemap.cellBounds + " and " + non_null_tiles + " non-null tiles" + tile_count_log); }
    }

}