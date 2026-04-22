using UnityEngine;

public class WorldDoorVisualizer : WorldCellVisualizer
{
    public bool is_vertical = true;
    // not intuitif but it takes 2 horizontal cells if it is vertical
    // if horizontal, it takes 2 vertical cells
    public Vector3Int OtherCell
    {
        get
        {
            if (is_vertical) { return new Vector3Int(Cell.x + 1, Cell.y, Cell.z); }
            else { return new Vector3Int(Cell.x, Cell.y + 1, Cell.z); }
        }
    }

    public Vector2 OtherWorldPosition
    {
        get
        {
            Vector2 grid_size = grid.cellSize;
            if (is_vertical) { return WorldPosition + new Vector2(grid_size.x, 0); }
            else { return WorldPosition + new Vector2(0, grid_size.y); }
        }
    }
}