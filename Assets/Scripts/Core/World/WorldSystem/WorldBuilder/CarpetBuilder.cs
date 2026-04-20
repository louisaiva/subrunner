using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarpetBuilder : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform tilemap_parent;
    [SerializeField] private Tilemap tilemap_prefab;
    [SerializeField] private TileBase carpet_tile;
    private Dictionary<string, Tilemap> tilemap_instances = new Dictionary<string, Tilemap>();
    private Grid _grid;
    private Grid grid
    {
        get
        {
            if (_grid == null) { _grid = tilemap_parent.GetComponent<Grid>(); }
            return _grid;
        }
    }

    [Header("Logs")]
    [SerializeField] private bool log_angles = false;

    // MAIN METHODS
    public Tilemap Build(WorldRoomVisualizer room)
    {
        // we check if we already have a tilemap for this room, else we create one
        if (tilemap_instances.TryGetValue(room.name, out Tilemap tilemap_instance))
        {
            tilemap_instance.ClearAllTiles();
        }
        else
        {
            tilemap_instance = Instantiate(tilemap_prefab, tilemap_parent);
            tilemap_instance.GetComponent<TilemapCollider2D>().enabled = false;
            tilemap_instance.name = $"carpet_{room.name}";
            tilemap_instances[room.name] = tilemap_instance;
        }

        // we generate the tilemap for the room
        generate_tilemap(tilemap_instance, room);

        return tilemap_instance;
    }
    public void Clear()
    {
        foreach (var tilemap in tilemap_instances.Values)
        {
            tilemap.ClearAllTiles();
        }
    }



    // MAIN TILEMAP GENERATION
    private void generate_tilemap(Tilemap tilemap, WorldRoomVisualizer room)
    {
        // we calculate all the positions of the tiles we need to create the carpet
        List<Vector3Int> tile_positions = calculate_tiles_positions(room);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in tile_positions) { tilemap.SetTile(pos, carpet_tile); }
    }
    private List<Vector3Int> calculate_tiles_positions(WorldRoomVisualizer room)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // we go through all the links of the room
        foreach (var link in room.Links)
        {
            positions.AddRange(trace_line(link));
        }
        return positions;
    }

    // low level generation methods
    private List<float> allowed_angles = new List<float> { 0, 45, 90, 135, 180, 225, 270, 315 };
    private List<Vector3Int> trace_line(WorldLinkVisualizer link)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // we get the positions of the two cells connected by the link
        Vector3Int pos_a = WorldToCell(link.CellA.WorldPosition);
        Vector3Int pos_b = WorldToCell(link.CellB.WorldPosition);
        if (!has_good_angle(pos_a, pos_b, out float angle)) { return positions; }

        // check if the angle is 0,90,180,270 we just create a line
        if (Mathf.Approximately(angle % 90, 0)) { return trace_straight_line(pos_a, pos_b); }
        if (Mathf.Approximately(angle % 45, 0)) { return trace_diagonal_line_canard(pos_a, pos_b); }

        // we calculate the direction from a to b
        /* Vector2 direction = get_direction(pos_a, pos_b);

        // we calculate the perpendicular direction to the link
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        // we add tiles in a line between the two cells, and a few tiles on the sides to create a carpet effect
        float distance = Vector2.Distance(pos_a, pos_b);
        int tile_count = Mathf.CeilToInt(distance / 0.5f);
        for (int i = 0; i <= tile_count; i++)
        {
            Vector2 tile_position = pos_a + direction * i * 0.5f;
            positions.Add(tile_position);
            positions.Add(tile_position + perpendicular * 0.5f);
            positions.Add(tile_position - perpendicular * 0.5f);
        } */
        return positions;
    }
    private bool has_good_angle(Vector3Int from, Vector3Int to, out float angle)
    {
        // we calculate the angle of the line between the two cells
        angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        // we check if the angle is close to one of the allowed angles, else we log a warning and we return an empty list
        float closest_angle = allowed_angles[0];
        float min_angle_diff = Mathf.Abs(Mathf.DeltaAngle(angle, closest_angle));
        foreach (var a in allowed_angles)
        {
            float angle_diff = Mathf.Abs(Mathf.DeltaAngle(angle, a));
            if (angle_diff < min_angle_diff)
            {
                min_angle_diff = angle_diff;
                closest_angle = a;
            }
            if (min_angle_diff < 1f)
            {
                if (log_angles) { Debug.Log($"(CarpetBuilder) Link between {from} and {to} has an angle of {angle} which is close to the allowed angle of {closest_angle}. Carpet will be generated for this link."); }
                return true;
            }
        }
        if (log_angles) { Debug.LogWarning($"(CarpetBuilder) Link between {from} and {to} has an angle of {angle} which is not close to any of the allowed angles. No carpet will be generated for this link."); }
        return false;
    }
    private List<Vector3Int> trace_straight_line(Vector3Int from, Vector3Int to)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        Vector2Int direction = get_direction(from, to);
        int safe_guard = 1000;
        int i = 0;
        while (from != to && i < safe_guard)
        {
            positions.Add(from);
            from += (Vector3Int)direction;
            i++;
        }
        // we add the last position if we exited because we reached the target
        if (from == to) { positions.Add(to); }
        return positions;
    }
    private List<Vector3Int> trace_diagonal_line_canard(Vector3Int from, Vector3Int to)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        Vector2Int direction = get_direction(from, to);
        List<Vector2Int> intermediate_directions = new List<Vector2Int>
            {
                new Vector2Int(direction.x, 0),
                new Vector2Int(0, direction.y)
            };
        int safe_guard = 1000;
        int i = 0;
        while (from != to && i < safe_guard)
        {
            positions.Add(from);
            positions.Add(from + (Vector3Int)intermediate_directions[0]);
            positions.Add(from + (Vector3Int)intermediate_directions[1]);
            from += (Vector3Int)direction;
            i++;
        }
        // we add the last position if we exited because we reached the target
        if (from == to) { positions.Add(to); }
        return positions;
    }
    private List<Vector2Int> directions = new List<Vector2Int>
                                    {
                                        Vector2Int.right, Vector2Int.up, // R & U
                                        Vector2Int.left, Vector2Int.down, // L & D
                                        Vector2Int.right + Vector2Int.up, // RU
                                        Vector2Int.left + Vector2Int.up, // LU
                                        Vector2Int.left + Vector2Int.down, // LD
                                        Vector2Int.right + Vector2Int.down // RD
                                    };
    private Vector2Int get_direction(Vector3Int from, Vector3Int to)
    {
        Vector2 direction = new Vector2(to.x - from.x, to.y - from.y).normalized;
        Vector2Int closest_direction = directions[0];
        float min_direction_diff = Vector2.Angle(direction, directions[0]);
        foreach (var d in directions)
        {
            float direction_diff = Vector2.Angle(direction, d);
            if (direction_diff < min_direction_diff)
            {
                min_direction_diff = direction_diff;
                closest_direction = d;
            }
        }
        return closest_direction;
    }
    private Vector3Int WorldToCell(Vector2 world_position)
    {
        return grid.WorldToCell(world_position);
    }

}