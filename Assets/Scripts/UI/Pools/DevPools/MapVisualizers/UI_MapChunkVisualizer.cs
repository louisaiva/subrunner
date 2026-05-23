using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI_MapChunkVisualizer : MonoBehaviour
{
    private UILineRenderer liner;
    private ChunkData data;
    private Color main_color;
    private Color loaded_color;

    // INIT
    public void Initialize(ChunkData chunk_data, Color loaded_color, Color main_color)
    {
        this.data = chunk_data;
        this.main_color = main_color;
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

        ChunkEngine.Instance.OnPlayerChunkChange += OnPlayerChunkChange;
    }


    // CALLBACKS
    public void OnChunkLoaded(ChunkData chunk_data)
    {
        liner.color = loaded_color;

        // set our self as last sibling so we are drawn on top of the other chunks (since we are loaded, we want to be visible)
        transform.SetSiblingIndex(Mathf.Max(0, transform.parent.childCount - 2)); // -2 so it does not go above the main player chunk visu
    }
    public void OnChunkUnloaded(ChunkData chunk_data)
    {
        liner.color = Color.white;

        // set our self as first sibling so don't hide others
        transform.SetSiblingIndex(0);
    }
    public void OnPlayerChunkChange(ChunkData player_chunk)
    {
        if (player_chunk == data)
        {
            liner.color = main_color;

            // set our self as last sibling so we are drawn on top of the other chunks (since we are loaded, we want to be visible)
            transform.SetSiblingIndex(transform.parent.childCount - 1);
        }
        else if (player_chunk != data && liner.color == main_color) { OnChunkLoaded(data); }
    }


    // ON DESTROY
    public void OnDestroy()
    {
        // unregister from data chunk events
        if (ChunkEngine.Instance != null)
        {
            ChunkEngine.Instance.OnPlayerChunkChange -= OnPlayerChunkChange;
        }
        
        if (data == null) { return; }
        data.OnChunkLoaded -= OnChunkLoaded;
        data.OnChunkUnloaded -= OnChunkUnloaded;
    }
}