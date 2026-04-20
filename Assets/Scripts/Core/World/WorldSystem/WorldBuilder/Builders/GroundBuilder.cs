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

    // FILL INSIDE
    private List<Vector3Int> fill_inside(List<Vector3Int> tile_positions)
    {
        // we get the minus x & y of the tile positions to know where to start filling
        int min_x = int.MaxValue;
        int max_x = int.MinValue;
        int min_y = int.MaxValue;
        int max_y = int.MinValue;
        foreach (var pos in tile_positions)
        {
            if (pos.x < min_x) { min_x = pos.x; }
            if (pos.y < min_y) { min_y = pos.y; }
            if (pos.x > max_x) { max_x = pos.x; }
            if (pos.y > max_y) { max_y = pos.y; }
        }
        
        List<Vector3Int> filled_positions = new List<Vector3Int>(tile_positions);

        // for each x we go vertically until maxy
        for (int y = min_y; y <= max_y; y++)
        {
            filled_positions.AddRange(fill_row(min_x, max_x, tile_positions, y));
        }

        // we remove the duplicates if there are any
        HashSet<Vector3Int> unique_positions = new HashSet<Vector3Int>(filled_positions);
        return new List<Vector3Int>(unique_positions);
    }
    private List<Vector3Int> fill_row(int minx, int maxx, List<Vector3Int> tile_positions, int y)
    {
        List<Vector3Int> filled_positions = new List<Vector3Int>();
        bool is_inside = false;

        List<Vector3Int> current_adjacent_corners = new List<Vector3Int>();

        // we start at minx miny and we go horizontally until maxx
        Vector3Int pos = new Vector3Int(minx, y, 0);
        for (int x = minx; x <= maxx; x++)
        {
            if (is_corner(x, y, tile_positions))
            {
                if (current_adjacent_corners.Contains(new Vector3Int(x-1, y, 0))) // if last tile was a corner adjacent too, we add the current corner & continue without changing inside flag
                {
                    current_adjacent_corners.Add(pos);
                    continue;
                }

                // else we arrive at a new corner. we change the flag
                is_inside = !is_inside;
                current_adjacent_corners = new List<Vector3Int>() { pos };
                continue;
            }

            // if we are not inside, we don't add the position to the filled positions
            if (!is_inside) { continue; }

            // else we add the position to the filled positions
            pos = new Vector3Int(x, y, 0);
            if (!tile_positions.Contains(pos)) { filled_positions.Add(pos); }   
        }

        return filled_positions;
    }

    private bool is_corner(int x,int y, List<Vector3Int> corners_positions)
    {
        return corners_positions.Contains(new Vector3Int(x, y, 0));
    }
}