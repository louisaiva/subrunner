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
        List<Vector3Int> default_carpet = get_carpet_tiles_positions(room, DiagonalTraceType.Straight);

        // we filter the carpet because we don't want to generate tiles that will exceed the room's bounds
        default_carpet = filter_exceeding_tiles(default_carpet, room);

        // then we want to convert those tiles positions to the ground grid
        List<Vector3> world_carpet = default_carpet.Select(pos => CellToWorld(pos, grid)).ToList(); // convert from carpet grid to world coords
        grid = null;
        List<Vector3Int> carpet = world_carpet.Select(pos => WorldToCell(pos)).ToList(); // reconvert from world into ground grid

        // we want to fill the inside of the room as well
        // carpet = fill_inside(carpet);

        // we filter to keep only one tile
        HashSet<Vector3Int> unique_carpet = new HashSet<Vector3Int>(carpet);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in unique_carpet) { tilemap.SetTile(pos, tile); }
    }

    private List<Vector3Int> filter_exceeding_tiles(List<Vector3Int> carpet, WorldRoomVisualizer room)
    {
        // we generate a filled inside WITHOUT the straight carpet
        List<Vector3Int> filled_inside = fill_inside(carpet);

        // we remove all carpet tiles from the filled inside
        HashSet<Vector3Int> carpet_set = new HashSet<Vector3Int>(carpet);
        List<Vector3Int> filled_inside_without_carpet = filled_inside.Where(pos => !carpet_set.Contains(pos)).ToList();
        return filled_inside_without_carpet;
    }
}