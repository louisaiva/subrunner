using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CeilingBuilder : TilemapBuilder
{
    [Header("Ceiling Parameters")]
    public bool remove_bottom_tiles = false; // if true, don't generate the bottom tiles
    public int bottom_to_remove = 3; // number of bottom tiles to remove (if remove_bottom_tiles is true)

    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the outline
        List<Vector3Int> outline;
        if (remove_bottom_tiles) { outline = filter_bottom_tiles(room); }
        else { outline = get_carpet_tiles_positions(room, DiagonalTraceType.Canard); }

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in outline)
        {
            if (HasDoorAtPosition(pos)) { continue; } // filter the doors
            tilemap.SetTile(pos, tile);
        }
    }


    // generation type
    protected List<Vector3Int> filter_bottom_tiles(WorldRoomVisualizer room)
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
}