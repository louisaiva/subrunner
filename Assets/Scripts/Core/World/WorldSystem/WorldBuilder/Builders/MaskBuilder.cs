using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MaskBuilder : CeilingBuilder
{
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        List<Vector3Int> tile_positions = get_ceiling_outline(room, DiagonalTraceType.Straight);

        // we fill the inside of the room as well
        tile_positions = fill_inside(tile_positions);
        // log

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in tile_positions) { tilemap.SetTile(pos, tile); }
    }
}