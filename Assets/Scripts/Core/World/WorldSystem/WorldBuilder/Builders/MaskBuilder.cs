using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MaskBuilder : CeilingBuilder
{
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        List<Vector3Int> carpet = get_carpet_tiles_positions(room, DiagonalTraceType.Canard);

        // we fill the inside of the room as well
        carpet = fill_inside(carpet);
        // log

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in carpet)
        {
            if (HasDoorAtPosition(pos)) { continue; } // filter the doors
            tilemap.SetTile(pos, tile);
        }
    }
}