using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CeilingBuilder : TilemapBuilder
{
    [Header("Door Tile")]
    [SerializeField] private TileBase door_tile;

    [Header("Ceiling Parameters")]
    public bool remove_bottom_tiles = false; // if true, don't generate the bottom tiles
    public int bottom_to_remove = 3; // number of bottom tiles to remove (if remove_bottom_tiles is true)

    public bool generate_only_on_walls = false; // if true, only generate tiles on walls
    public int wall_bottom_to_keep = 2; // vertical distance from the wall under which we keep the tiles (if generate_only_on_walls is true)

    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldChunkVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the outline
        List<Vector3Int> outline;
        if (remove_bottom_tiles) { outline = filter_bottom_tiles(room); }
        else { outline = get_carpet_tiles_positions(room, DiagonalTraceType.Canard); }

        // filter not on walls
        if (generate_only_on_walls)
        {
            List<Vector3Int> wall_tiles = GetComponent<WallsBuilder>().GetLastOutline();
            outline = filter_not_on_walls(outline, wall_tiles);
        }

        // we add all the doors
        outline.AddRange(GetAllDoorPositions());
        HashSet<Vector3Int> unique_positions = new HashSet<Vector3Int>(outline);
        outline = unique_positions.ToList();

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in outline)
        {
            if (HasDoorAtPosition(pos)) // filter the doors
            {
                tilemap.SetTile(pos, door_tile); // we still add an empty tile so the rule tile can work properly
                continue;
            }
            tilemap.SetTile(pos, tile);
        }
    }


    // generation type
    protected List<Vector3Int> filter_bottom_tiles(WorldChunkVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the outline
        List<Vector3Int> outline = get_carpet_tiles_positions(room, DiagonalTraceType.Canard);

        // we duplicate the outline and move it by 1 up, to filter the inner walls (we want to remove the ext walls)
        List<Vector3Int> inside_mask = new List<Vector3Int>();
        foreach (var pos in outline) { inside_mask.Add(new Vector3Int(pos.x, pos.y + bottom_to_remove, pos.z)); }

        // we fill inside the outline up
        inside_mask = fill_inside(inside_mask);

        // we keep only the tiles that are in the outline and in the inside mask
        for (int i = outline.Count - 1; i >= 0; i--)
        {
            if (!inside_mask.Contains(outline[i])) { outline.RemoveAt(i); }
            // todo add an option to keep the ones that are on the bottom when there is exactly 2 carpet tiles above them
        }

        return outline;
    }

    protected List<Vector3Int> filter_not_on_walls(List<Vector3Int> tiles, List<Vector3Int> wall_tiles)
    {
        List<Vector3Int> filtered = new List<Vector3Int>();
        foreach (var pos in tiles)
        {
            // if us or x bottom tile is in the wall tiles, we keep it
            for (int i = 0; i <= wall_bottom_to_keep; i++)
            {
                if (!wall_tiles.Contains(new Vector3Int(pos.x, pos.y - i, pos.z))) { continue; }
                filtered.Add(pos);
                break;
            }
        }
        return filtered;
    }
}