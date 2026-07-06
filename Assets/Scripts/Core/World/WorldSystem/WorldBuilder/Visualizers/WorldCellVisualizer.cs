using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WorldCellVisualizer : MonoBehaviour
{
    [Header("Components")]
    private SpriteRenderer _sprite_renderer;
    private SpriteRenderer sprite_renderer
    {
        get
        {
            if (_sprite_renderer == null) { _sprite_renderer = GetComponent<SpriteRenderer>(); }
            return _sprite_renderer;
        }
    }
    protected Grid grid { get { return WorldBuilder.LevelBuilder.Grid; } }
    public Color Color
    {
        get { return sprite_renderer.color; }
        set { sprite_renderer.color = value; }
    }
    public void SetSize(float size)
    {
        transform.localScale = new Vector3(.2f, .2f, .2f) * size;
    }

    [Header("Data")]
    public Vector3Int Cell;

    [Header("Events")]
    public System.Action<WorldCellVisualizer> OnRemoved = delegate { };
    public System.Action<WorldCellVisualizer> OnMoved = delegate { };

    public void SetCell(Vector3Int cell)
    {
        Cell = cell;
        transform.position = grid.CellToWorld(cell) + grid.cellSize / 2;
        OnMoved?.Invoke(this);
    }

    public void SetIcon(Sprite sprite)
    {
        sprite_renderer.sprite = sprite;
    }

    private void OnDestroy()
    {
        OnRemoved?.Invoke(this);
    }

    // GETTERS
    public Vector2 WorldPosition { get { return transform.position; } }
    public override string ToString()
    {
        return $"({Cell.x}, {Cell.y})";
    }


}