using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundBuilder : TilemapBuilder
{
    [Header("Default 0.5f 0.5f grid")]
    [SerializeField] private Grid default_grid;

    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we set the grid as the default one so this is the one used for calculating all the tile positions in a first time
        grid = default_grid;

        // we calculate the looping polygon in grid coords.
        List<Vector3Int> carpet = get_carpet_tiles_positions(room, DiagonalTraceType.Straight);

        // then we want to convert those tiles positions to the ground grid
        List<Vector3> world_carpet = carpet.Select(pos => CellToWorld(pos, grid)).ToList(); // convert from carpet grid to world coords
        grid = null;
        carpet = world_carpet.Select(pos => WorldToCell(pos)).ToList(); // reconvert from world into ground grid

        // we want to fill the inside of the room as well
        carpet = fill_inside(carpet);

        // we filter to keep only one tile
        HashSet<Vector3Int> unique_carpet = new HashSet<Vector3Int>(carpet);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in unique_carpet) { tilemap.SetTile(pos, tile); }
    }
}