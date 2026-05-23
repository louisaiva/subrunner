using Unity.VisualScripting;
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

    // CHUNKS CELLS
    public string room1_id;
    public string room2_id;
    public Vector3Int Chunk1Cell
    {
        get
        {
            if (is_vertical) { return new Vector3Int(Cell.x, Cell.y + 1, Cell.z); }
            else { return new Vector3Int(Cell.x + 1, Cell.y, Cell.z); }
        }
    }
    public Vector3Int Chunk2Cell
    {
        get
        {
            if (is_vertical) { return new Vector3Int(Cell.x, Cell.y - 1, Cell.z); }
            else { return new Vector3Int(Cell.x - 1, Cell.y, Cell.z); }
        }
    }
    public Vector2 Chunk1CellWorldPosition
    {
        get
        {
            Vector2 grid_size = grid.cellSize;
            if (is_vertical) { return WorldPosition + new Vector2(0, grid_size.y); }
            else { return WorldPosition + new Vector2(grid_size.x, 0); }
        }
    }
    public Vector2 Chunk2CellWorldPosition
    {
        get
        {
            Vector2 grid_size = grid.cellSize;
            if (is_vertical) { return WorldPosition - new Vector2(0, grid_size.y); }
            else { return WorldPosition - new Vector2(grid_size.x, 0); }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere((WorldPosition + OtherWorldPosition) /2f, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Chunk1CellWorldPosition, 0.1f);
        Gizmos.DrawSphere(Chunk2CellWorldPosition, 0.1f);
    }
}