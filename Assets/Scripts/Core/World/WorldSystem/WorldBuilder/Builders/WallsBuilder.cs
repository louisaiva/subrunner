using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallsBuilder : TilemapBuilder
{
    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the outline
        List<Vector3Int> outline = get_carpet_tiles_positions(room, DiagonalTraceType.Straight);

        outline = filter_vertical(outline);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in outline) { tilemap.SetTile(pos, tile); }
    }

    // FILTER VERTICAL
    private List<Vector3Int> filter_vertical(List<Vector3Int> tile_positions)
    {
        // we remove tiles that have a direct vertical neighbour below (not above)
        HashSet<Vector3Int> filtered = new HashSet<Vector3Int>();
        foreach (var pos in tile_positions)
        {
            Vector3Int below = new Vector3Int(pos.x, pos.y - 1, pos.z);
            if (tile_positions.Contains(below)) { continue; }
            filtered.Add(pos);
        }
        return new List<Vector3Int>(filtered);
    }
}