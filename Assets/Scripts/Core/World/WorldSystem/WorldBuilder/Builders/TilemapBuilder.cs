using System.Collections.Generic;
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
        GenerateTilemap(tilemap_instance, room);

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
        foreach (var cell in room.Cells)
        {
            if (cell == null) { continue; }
            if (cell.IsPartOfRoom()) { positions.Add(WorldToCell(cell.WorldPosition)); }
        }
        return positions;
    }

    // low level generation methods
    protected List<float> _allowed_angles = new List<float> { 0, 45, 90, 135, 180, 225, 270, 315 };
    protected virtual List<float> allowed_angles { get { return _allowed_angles; } }
    protected List<Vector3Int> trace_line(WorldLinkVisualizer link, DiagonalTraceType diagonal_trace_type = DiagonalTraceType.Canard)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // we get the positions of the two cells connected by the link
        Vector3Int pos_a = WorldToCell(link.CellA.WorldPosition);
        Vector3Int pos_b = WorldToCell(link.CellB.WorldPosition);
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
                if (log_angles) { Debug.Log($"(CarpetBuilder) Link between {from} and {to} has an angle of {angle} which is close to the allowed angle of {closest_angle}. Carpet will be generated for this link."); }
                return true;
            }
        }
        if (log_angles) { Debug.LogWarning($"(CarpetBuilder) Link between {from} and {to} has an angle of {angle} which is not close to any of the allowed angles. No carpet will be generated for this link."); }
        return false;
    }
    private bool has_good_angle(WorldLinkVisualizer link, out float angle)
    {
        return has_good_angle(link.CellA.WorldPosition, link.CellB.WorldPosition, out angle);
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
}

public enum DiagonalTraceType
{
    Straight, // only the diagonal tile
    Canard, // the diagonal tile + the two adjacent tiles to make it look better. trace the diagonal like this : <<<<<<<<<<<<- but rotated by 45°
}