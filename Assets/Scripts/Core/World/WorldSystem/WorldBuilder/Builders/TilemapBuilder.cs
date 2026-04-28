using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBuilder : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] protected Transform tilemap_parent;
    [SerializeField] protected Tilemap tilemap_prefab;
    [SerializeField] protected TileBase tile;
    protected Dictionary<string, Tilemap> tilemap_instances = new Dictionary<string, Tilemap>();
    private Grid _grid;
    protected Grid grid // used to convert from default grid to final grid if they are different
    {
        get
        {
            if (_grid == null) { _grid = tilemap_parent.GetComponent<Grid>(); }
            return _grid;
        }
        set
        {
            _grid = value;
        }
    }


    [Header("Logs")]
    [SerializeField] private bool log_angles = false;

    // MAIN METHODS
    public virtual Tilemap Build(WorldRoomVisualizer room)
    {
        // we save the doors
        doors.Clear();
        doors.AddRange(room.Doors);

        // we check if we already have a tilemap for this room, else we create one
        if (tilemap_instances.TryGetValue(room.name, out Tilemap tilemap_instance))
        {
            tilemap_instance.ClearAllTiles();
        }
        else
        {
            tilemap_instance = Instantiate(tilemap_prefab, tilemap_parent);
            if (tilemap_instance.TryGetComponent(out TilemapCollider2D tilemap_collider)) { tilemap_collider.enabled = false; }
            tilemap_instance.name = $"{get_builder_name()}_{room.name}";
            tilemap_instances[room.name] = tilemap_instance;
        }

        // we generate the tilemap for the room
        GenerateTilemap(tilemap_instance, room);

        return tilemap_instance;
    }
    public virtual void Clear()
    {
        foreach (var tilemap in tilemap_instances.Values)
        {
            tilemap.ClearAllTiles();
        }
    }
    protected virtual string get_builder_name()
    {
        return GetType().Name.Replace("Builder", "").ToLower();
    }


    // MAIN TILEMAP GENERATION
    protected virtual void GenerateTilemap(Tilemap tilemap, WorldRoomVisualizer room) {}
    protected List<Vector3Int> get_carpet_tiles_positions(WorldRoomVisualizer room, DiagonalTraceType diagonal_trace_type = DiagonalTraceType.Canard)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // we go through all the links of the room
        foreach (var link in room.Links)
        {
            positions.AddRange(trace_line(link, diagonal_trace_type));
        }
        return positions;
    }
    protected List<Vector3Int> calculate_tiles_positions(WorldRoomVisualizer room)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // we go through all the cells of the room
        foreach (var cell in room.Nodes)
        {
            if (cell == null) { continue; }
            if (cell.IsPartOfRoom()) { positions.Add(WorldToCell(cell.WorldPosition)); }
        }
        return positions;
    }





    // low level generation methods




    // ANGLES & LINES TRACING
    protected List<float> _allowed_angles = new List<float> { 0, 45, 90, 135, 180, 225, 270, 315 };
    protected virtual List<float> allowed_angles { get { return _allowed_angles; } }
    protected List<Vector3Int> trace_line(WorldLinkVisualizer link, DiagonalTraceType diagonal_trace_type = DiagonalTraceType.Canard)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // we get the positions of the two cells connected by the link
        Vector3Int pos_a = WorldToCell(link.NodeA.WorldPosition);
        Vector3Int pos_b = WorldToCell(link.NodeB.WorldPosition);
        if (!has_good_angle(pos_a, pos_b, out float angle)) { return positions; }

        // check if the angle is 0,90,180,270 we just create a line
        if (Mathf.Approximately(angle % 90, 0)) { return trace_straight_line(pos_a, pos_b); }
        if (Mathf.Approximately(angle % 45, 0))
        {
            switch (diagonal_trace_type)
            {
                case DiagonalTraceType.Canard:
                    return trace_diagonal_line_canard(pos_a, pos_b);
                default:
                    return trace_straight_line(pos_a, pos_b);
            }
        }
        return positions;
    }
    private bool has_good_angle(Vector3Int from, Vector3Int to, out float angle)
    {
        return has_good_angle(new Vector2(from.x, from.y), new Vector2(to.x, to.y), out angle);
    }
    protected virtual bool has_good_angle(Vector2 from, Vector2 to, out float angle)
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
            if (min_angle_diff < 5f)
            {
                if (log_angles) { Debug.Log($"({GetType().Name}) Link between {from} and {to} has an angle of {angle} which is close to the allowed angle of {closest_angle}. Carpet will be generated for this link."); }
                return true;
            }
        }
        if (log_angles) { Debug.LogWarning($"({GetType().Name}) Link between {from} and {to} has an angle of {angle} which is not close to any of the allowed angles. No carpet will be generated for this link."); }
        return false;
    }
    private bool has_good_angle(WorldLinkVisualizer link, out float angle)
    {
        return has_good_angle(link.NodeA.WorldPosition, link.NodeB.WorldPosition, out angle);
    }
    protected List<Vector3Int> trace_straight_line(Vector3Int from, Vector3Int to)
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
    protected List<Vector3Int> trace_diagonal_line_canard(Vector3Int from, Vector3Int to)
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


    // FILL INSIDE
    protected List<Vector3Int> fill_inside(List<Vector3Int> tile_positions)
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



    // DIRECTIONS & CONVERSIONS
    private List<Vector2Int> directions = new List<Vector2Int>
                                    {
                                        Vector2Int.right, Vector2Int.up, // R & U
                                        Vector2Int.left, Vector2Int.down, // L & D
                                        Vector2Int.right + Vector2Int.up, // RU
                                        Vector2Int.left + Vector2Int.up, // LU
                                        Vector2Int.left + Vector2Int.down, // LD
                                        Vector2Int.right + Vector2Int.down // RD
                                    };
    protected Vector2Int get_direction(Vector3Int from, Vector3Int to)
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
    protected Vector3Int WorldToCell(Vector2 world_position, Grid grid = null)
    {
        Grid cell_grid = grid ?? this.grid;
        return cell_grid.WorldToCell(world_position);
    }
    protected Vector3 CellToWorld(Vector3Int cell_position, Grid grid = null)
    {
        Grid cell_grid = grid ?? this.grid;
        return cell_grid.CellToWorld(cell_position);
    }



    // DOORS
    protected readonly List<WorldDoorVisualizer> doors = new List<WorldDoorVisualizer>();
    protected virtual bool HasDoorAtPosition(Vector3Int cell_position)
    {
        Vector2 world_position = CellToWorld(cell_position) + new Vector3(0.25f, 0.25f, 0); // we check the center of the tile
        foreach (var door in doors)
        {
            if (Vector2.Distance(door.WorldPosition, world_position) < 0.1f) { return true; }
            if (Vector2.Distance(door.OtherWorldPosition, world_position) < 0.1f) { return true; }
        }
        return false;
    }
    protected virtual List<Vector3Int> GetAllDoorPositions()
    {
        List<Vector3Int> door_positions = new List<Vector3Int>();
        foreach (var door in doors)
        {
            Vector3Int pos_a = WorldToCell(door.WorldPosition);
            Vector3Int pos_b = WorldToCell(door.OtherWorldPosition);
            if (!door_positions.Contains(pos_a)) { door_positions.Add(pos_a); }
            if (!door_positions.Contains(pos_b)) { door_positions.Add(pos_b); }
        }
        return door_positions;
    }

    /* private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var door in doors)
        {
            Gizmos.DrawSphere(door.WorldPosition, 0.1f);
        }
        Gizmos.color = Color.blue;
        foreach (var tilemap in tilemap_instances.Values)
        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    Vector3 world_pos = tilemap.CellToWorld(pos) + tilemap.tileAnchor;
                    Gizmos.DrawSphere(world_pos, 0.05f);
                }
            }
        }
    } */
}

public enum DiagonalTraceType
{
    Straight, // only the diagonal tile
    Canard, // the diagonal tile + the two adjacent tiles to make it look better. trace the diagonal like this : <<<<<<<<<<<<- but rotated by 45°
}