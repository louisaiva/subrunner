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
        HashSet<Vector3Int> outline = new HashSet<Vector3Int>(tile_positions);
        if (outline.Count == 0) { return new List<Vector3Int>(); }

        int min_x = int.MaxValue;
        int max_x = int.MinValue;
        int min_y = int.MaxValue;
        int max_y = int.MinValue;

        foreach (var pos in outline)
        {
            if (pos.x < min_x) { min_x = pos.x; }
            if (pos.y < min_y) { min_y = pos.y; }
            if (pos.x > max_x) { max_x = pos.x; }
            if (pos.y > max_y) { max_y = pos.y; }
        }

        // Expand bounds by 1 so the flood start is guaranteed outside the outline.
        min_x -= 1;
        min_y -= 1;
        max_x += 1;
        max_y += 1;

        HashSet<Vector3Int> outside = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Vector3Int start = new Vector3Int(min_x, min_y, 0);

        outside.Add(start);
        queue.Enqueue(start);

        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            for (int i = 0; i < directions.Length; i++)
            {
                Vector3Int next = current + directions[i];

                if (next.x < min_x || next.x > max_x || next.y < min_y || next.y > max_y) { continue; }
                if (outline.Contains(next)) { continue; }
                if (outside.Contains(next)) { continue; }

                outside.Add(next);
                queue.Enqueue(next);
            }
        }

        HashSet<Vector3Int> filled = new HashSet<Vector3Int>(outline);

        for (int x = min_x + 1; x <= max_x - 1; x++)
        {
            for (int y = min_y + 1; y <= max_y - 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (outline.Contains(pos)) { continue; }
                if (outside.Contains(pos)) { continue; }
                filled.Add(pos);
            }
        }

        return new List<Vector3Int>(filled);
    }
}