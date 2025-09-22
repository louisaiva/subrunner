using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Resizer : MonoBehaviour
{

    [Header("Resizer")]
    [SerializeField] private RectTransform target;
    [SerializeField] private GridLayoutGroup grid;

    [Header("Size")]
    [SerializeField] private Vector2 base_size;
    [SerializeField] private Vector2 cell_size;
    [SerializeField] private Vector2 min_size = new Vector2(0,0);

    [Header("Colums & Rows")]
    [SerializeField] private int columns = 1;
    [SerializeField] private int rows = 1;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // UPDATE
    public void Resize(int cells)
    {
        if (target == null) { return; }

        // we get the best size for cells count
        Vector2Int size = calculate_best_suited_size_for_cells(cells);
        columns = size.x;
        rows = size.y;

        // we resize the target
        float width = base_size.x + columns * cell_size.x;
        float height = base_size.y + rows * cell_size.y;
        target.sizeDelta = new Vector2(width, height);

        // we check if the size is lower than the min size
        if (target.sizeDelta.x < min_size.x || target.sizeDelta.y < min_size.y)
        {
            float w = Mathf.Max(target.sizeDelta.x, min_size.x);
            float h = Mathf.Max(target.sizeDelta.y, min_size.y);
            target.sizeDelta = new Vector2(w, h);
        }

        // we constraint the grid group
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        if (log) { Debug.Log($"(UI_Resizer) Resized to {cells} cells -> {columns}x{rows} -> {width}x{height}"); }
    }


    private Dictionary<int,Vector2Int> size_cache = new Dictionary<int, Vector2Int>();
    private Vector2Int calculate_best_suited_size_for_cells(int cells)
    {
        // checks the cache if we have already calculated it
        if (size_cache.ContainsKey(cells)) { return size_cache[cells]; }

        int best_columns = 1;
        int best_rows = 1;
        float best_diagonal = float.MaxValue;

        // we go from 1x1 to 1xcells then repeat from 2x1->2xcells ... cellsxcells
        // we check if we have enough space to store cells
        // and if yes we check if the diagonal is lower
        for (int c = 1; c <= cells; ++c)
        {
            for (int r = 1; r <= cells; ++r)
            {
                if (c * r < cells) { continue; }
                float diagonal = Mathf.Sqrt(c * c + r * r);
                if (diagonal < best_diagonal)
                {
                    best_diagonal = diagonal;
                    best_columns = c;
                    best_rows = r;
                }
            }
        }

        // we cache the result
        size_cache[cells] = new Vector2Int(best_columns, best_rows);

        return new Vector2Int(best_columns, best_rows);
    }

}