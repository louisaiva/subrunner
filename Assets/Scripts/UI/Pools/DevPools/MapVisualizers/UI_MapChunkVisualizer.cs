using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI_MapChunkVisualizer : MonoBehaviour
{
    private UILineRenderer liner;
    private ChunkData data;
    private Color loaded_color;
    
    public void Initialize(ChunkData chunk_data, Color loaded_color)
    {
        this.data = chunk_data;
        this.loaded_color = loaded_color;

        // set the line points
        liner = GetComponent<UILineRenderer>();
        List<Vector2> points = new List<Vector2>();
        foreach (Vector2 point in chunk_data.collider_points)
        {
            Vector2 world_point = point + chunk_data.position;
            Vector2 final_ui_point = UI_DevMap.GetUIPositionFromWorldPosition(world_point);
            points.Add(final_ui_point);
        }
        points.Add(points[0]); // we close the loop by adding the first point at the end
        liner.Points = points.ToArray();

        // register to data chunk events
        data.OnChunkLoaded += OnChunkLoaded;
        data.OnChunkUnloaded += OnChunkUnloaded;
    }

    public void OnChunkLoaded(ChunkData chunk_data)
    {
        liner.color = loaded_color;

        // set our self as last sibling so we are drawn on top of the other chunks (since we are loaded, we want to be visible)
        transform.SetSiblingIndex(transform.parent.childCount - 1);
    }
    public void OnChunkUnloaded(ChunkData chunk_data)
    {
        liner.color = Color.white;

        // set our self as first sibling so don't hide others
        transform.SetSiblingIndex(0);
    }

    public void OnDestroy()
    {
        // unregister from data chunk events
        if (data == null) { return; }
        data.OnChunkLoaded -= OnChunkLoaded;
        data.OnChunkUnloaded -= OnChunkUnloaded;
    }
}