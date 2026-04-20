using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CeilingBuilder : TilemapBuilder
{
    // MAIN TILEMAP GENERATION
    private Vector2Int ceiling_offset = new Vector2Int(0, 0);
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        List<Vector3Int> tile_positions = get_ceiling_outline(room);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in tile_positions) { tilemap.SetTile(pos, tile); }
    }

    protected virtual List<Vector3Int> get_ceiling_outline(WorldRoomVisualizer room, DiagonalTraceType trace_type = DiagonalTraceType.Canard)
    {
        // we calculate all the positions of the tiles we need to create the carpet
        List<Vector3Int> tile_positions = get_carpet_tiles_positions(room, trace_type);

        // we apply the ceiling offset to those positions
        tile_positions = tile_positions.Select(pos => new Vector3Int(pos.x + ceiling_offset.x, pos.y + ceiling_offset.y, pos.z)).ToList();

        return tile_positions;
    }
}