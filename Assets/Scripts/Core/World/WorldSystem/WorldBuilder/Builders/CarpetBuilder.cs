using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarpetBuilder : TilemapBuilder
{
    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the carpet
        List<Vector3Int> tile_positions = get_carpet_tiles_positions(room);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in tile_positions)
        {
            if (HasDoorAtPosition(pos)) { continue; } // filter the doors
            tilemap.SetTile(pos, tile);
        }
    }
}