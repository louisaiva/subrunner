using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarpetBuilder : TilemapBuilder
{

    [Header("Door Tile")]
    [SerializeField] private TileBase door_tile;

    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldChunkVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the carpet
        List<Vector3Int> tile_positions = get_carpet_tiles_positions(room);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in tile_positions)
        {
            if (HasDoorAtPosition(pos)) // filter the doors
            {
                tilemap.SetTile(pos, door_tile); // we still add an empty tile so the rule tile can work properly
                continue;
            }
            tilemap.SetTile(pos, tile);
        }
    }
}