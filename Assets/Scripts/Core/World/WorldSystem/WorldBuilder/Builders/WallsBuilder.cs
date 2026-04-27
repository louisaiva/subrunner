using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallsBuilder : TilemapBuilder
{

    // logs
    [SerializeField] private bool logs_sides_check = false;
    [SerializeField] private bool logs_walls_on_door = false;

    [Header("Walls Parameters")]
    public bool generate_only_inside = false; // if true, don't generate the exteriors walls

    [Header("Sides Tiles")]
    [SerializeField] private TileBase L_tile;
    [SerializeField] private TileBase R_tile;


    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the outline
        List<Vector3Int> outline;
        if (generate_only_inside) { outline = filter_exterior_walls(tilemap, room); }
        else { outline = get_carpet_tiles_positions(room, DiagonalTraceType.Straight); }

        List<Vector3Int> left_sides = filter_left_sides(outline);
        List<Vector3Int> right_sides = filter_right_sides(outline);
        outline = filter_vertical(outline);

        // we add the horizontal doors up positions to the outline
        outline.AddRange(GetHorizontalDoorsUpPositions());

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in outline)
        {
            if (HasDoorAtPosition(pos)) { continue; } // filter the doors

            // check if we have a door at position -1 bottom and -2 bottom it means we need to add a one tile at pos
            /* if (HasDoorAtPosition(new Vector3Int(pos.x, pos.y - 1, pos.z)) && HasDoorAtPosition(new Vector3Int(pos.x, pos.y - 2, pos.z)))
            {
                tilemap.SetTile(pos, tile);
                continue;
            } */

            // filter L and R tiles
            if (left_sides.Contains(pos)) { tilemap.SetTile(pos, L_tile); continue; }
            if (right_sides.Contains(pos)) { tilemap.SetTile(pos, R_tile); continue; }

            tilemap.SetTile(pos, tile);
        }
    }

    // generation type
    protected List<Vector3Int> filter_exterior_walls(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the outline
        List<Vector3Int> outline = get_carpet_tiles_positions(room, DiagonalTraceType.Straight);

        // we duplicate the outline and move it by 1 up, to filter the inner walls (we want to remove the ext walls)
        List<Vector3Int> inside_mask = new List<Vector3Int>();
        foreach (var pos in outline) { inside_mask.Add(new Vector3Int(pos.x, pos.y + 1, pos.z)); }

        // we fill inside the outline up
        inside_mask = fill_inside(inside_mask);

        // we keep only the tiles that are in the outline and in the inside mask
        for (int i = outline.Count - 1; i >= 0; i--)
        {
            if (!inside_mask.Contains(outline[i])) { outline.RemoveAt(i); }
        }

        return outline;
    }






    // FILTER VERTICAL
    private List<Vector3Int> filter_vertical(List<Vector3Int> tile_positions)
    {
        // we remove tiles that have a direct vertical neighbour below (not above)
        HashSet<Vector3Int> filtered = new HashSet<Vector3Int>();
        foreach (var pos in tile_positions)
        {
            Vector3Int below = new Vector3Int(pos.x, pos.y - 1, pos.z);
            if (tile_positions.Contains(below)) { continue; }


            // if we are generating only inside, we need to remove the tiles that have no right no left no LB no RB and a top
            if (generate_only_inside)
            {
                Vector3Int left = new Vector3Int(pos.x - 1, pos.y, pos.z);
                Vector3Int right = new Vector3Int(pos.x + 1, pos.y, pos.z);
                Vector3Int top = new Vector3Int(pos.x, pos.y + 1, pos.z);
                Vector3Int left_bottom = new Vector3Int(pos.x - 1, pos.y - 1, pos.z);
                Vector3Int right_bottom = new Vector3Int(pos.x + 1, pos.y - 1, pos.z);
                if (!tile_positions.Contains(left)
                 && !tile_positions.Contains(right)
                 && !tile_positions.Contains(left_bottom) 
                 && !tile_positions.Contains(right_bottom) 
                 && tile_positions.Contains(top))
                {
                    continue;
                }
            }

            filtered.Add(pos);
        }
        return new List<Vector3Int>(filtered);
    }

    // FILTER SIDES
    private List<Vector3Int> filter_left_sides(List<Vector3Int> tile_positions)
    {
        List<Vector3Int> filtered = new List<Vector3Int>();
        foreach (var pos in tile_positions)
        {
            if (IsOnLeftSide(pos, tile_positions)) { filtered.Add(pos); }
        }
        return filtered;
    }
    private List<Vector3Int> filter_right_sides(List<Vector3Int> tile_positions)
    {
        List<Vector3Int> filtered = new List<Vector3Int>();
        foreach (var pos in tile_positions)
        {
            if (IsOnRightSide(pos, tile_positions)) { filtered.Add(pos); }
        }
        return filtered;
    }

    // SIDES CHECK
    private bool IsOnLeftSide(Vector3Int pos, List<Vector3Int> outline)
    {
        // verify that we have no tile on the left
        Vector3Int left = new Vector3Int(pos.x - 1, pos.y, pos.z);
        if (outline.Contains(left)) { return false; }

        // if we have a tile on top left -> we are normal, not L
        Vector3Int top_left = new Vector3Int(pos.x - 1, pos.y + 1, pos.z);
        if (outline.Contains(top_left)) { return false; }

        // we are L if we have a tile on the right AND a tile on the left bottom
        Vector3Int right = new Vector3Int(pos.x + 1, pos.y, pos.z);
        Vector3Int left_bottom = new Vector3Int(pos.x - 1, pos.y - 1, pos.z);
        if (outline.Contains(right) && outline.Contains(left_bottom))
        {
            if (logs_sides_check) { Debug.Log("(WallsBuilder) " + pos + " is L because it has a tile on R + LB"); }
            return true;
        }

        // we are also L if we have a tile on top + LB 
        Vector3Int top = new Vector3Int(pos.x, pos.y + 1, pos.z);
        if (outline.Contains(top) && outline.Contains(left_bottom))
        {
            if (logs_sides_check) { Debug.Log("(WallsBuilder) " + pos + " is L because it has a tile on T + LB"); }
            return true;
        }

        // and RT + LB
        Vector3Int top_right = new Vector3Int(pos.x + 1, pos.y + 1, pos.z);
        if (outline.Contains(top_right) && outline.Contains(left_bottom))        {
            if (logs_sides_check) { Debug.Log("(WallsBuilder) " + pos + " is L because it has a tile on RT + LB"); }
            return true;
        }

        return false;
    }
    private bool IsOnRightSide(Vector3Int pos, List<Vector3Int> outline)
    {
        // verify that we have no tile on the right
        Vector3Int right = new Vector3Int(pos.x + 1, pos.y, pos.z);
        if (outline.Contains(right)) { return false; }

        // if we have a tile on top right -> we are normal, not R
        Vector3Int top_right = new Vector3Int(pos.x + 1, pos.y + 1, pos.z);
        if (outline.Contains(top_right)) { return false; }

        // we are R if we have a tile on the left AND a tile on the right_bottom
        Vector3Int left = new Vector3Int(pos.x - 1, pos.y, pos.z);
        Vector3Int right_bottom = new Vector3Int(pos.x + 1, pos.y - 1, pos.z);
        if (outline.Contains(left) && outline.Contains(right_bottom))
        {
            if (logs_sides_check) { Debug.Log("(WallsBuilder) " + pos + " is R because it has a tile on L + RB"); }
            return true;
        }

        // we are also R if we have a tile on top + RB
        Vector3Int top = new Vector3Int(pos.x, pos.y + 1, pos.z);
        if (outline.Contains(top) && outline.Contains(right_bottom))
        {
            if (logs_sides_check) { Debug.Log("(WallsBuilder) " + pos + " is R because it has a tile on T + RB"); }
            return true;
        }

        // and LT + RB
        Vector3Int top_left = new Vector3Int(pos.x - 1, pos.y + 1, pos.z);
        if (outline.Contains(top_left) && outline.Contains(right_bottom))
        {
            if (logs_sides_check) { Debug.Log("(WallsBuilder) " + pos + " is R because it has a tile on LT + RB"); }
            return true;
        }

        return false;
    }


    // VERTICAL DOORS
    protected virtual List<Vector3Int> GetHorizontalDoorsUpPositions()
    {
        List<Vector3Int> door_positions = new List<Vector3Int>();
        string log = "";
        foreach (var door in doors)
        {
            if (door.is_vertical) { continue; }
            Vector3Int cell_pos = WorldToCell(door.OtherWorldPosition);
            door_positions.Add(new Vector3Int(cell_pos.x, cell_pos.y + 1, cell_pos.z));
            if (logs_walls_on_door) { log += "(WallsBuilder) horizontal door up position: " + cell_pos + "\n"; }
        }
        if (logs_walls_on_door) { Debug.Log(log); }
        return door_positions;
    }
}