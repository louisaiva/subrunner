using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

/// <summary>
/// this class draws the border of every room in a Level. It is
/// connected to the RoomSystem to know all rooms.
/// </summary>
public class UI_MapChunksManager : MonoBehaviour
{

    [Header("Line Visu Prefab")]
    [SerializeField] private UI_MapChunkVisualizer ui_chunk_visu; // has a line renderer
    [SerializeField] private Color main_color = Color.blue;
    [SerializeField] private Color loaded_color = Color.red;
    [SerializeField] private List<UI_MapChunkVisualizer> chunk_visualizers = new List<UI_MapChunkVisualizer>();

    [Header("Logs")]
    [SerializeField] private Loggable<UI_MapChunksManager> log;

    // CALCULATE WORLD OFFSET
    public Vector2 CalculateWorldCenter(out Vector2 extents)
    {
        // grab all the chunks in the ChunkEngine
        List<ChunkData> chunks = ChunkEngine.Instance.chunks_data.Values.ToList();

        // todo need to take the current level in account

        // compute the world bounds of all chunks to set the size of our canvas accordingly
        Bounds world_bounds = compute_world_bounds(chunks);
        extents = world_bounds.extents;
        return world_bounds.center;
    }
    private Bounds compute_world_bounds(IEnumerable<ChunkData> rooms)
    {
        bool hasPoint = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (ChunkData room in rooms)
        {
            if (room == null || room.collider_points == null) { continue; }

            for (int i = 0; i < room.collider_points.Count; i++)
            {
                Vector3 wp = (Vector3)(room.position + room.collider_points[i]);
                if (!hasPoint)
                {
                    bounds = new Bounds(wp, Vector3.zero);
                    hasPoint = true;
                }
                else
                {
                    bounds.Encapsulate(wp);
                }
            }
        }

        return bounds;
    }

    // VISUALS CREATION
    public void ClearVisuals()
    {
        foreach (UI_MapChunkVisualizer visualizer in chunk_visualizers)
        {
            if (visualizer == null) { continue; }
            Destroy(visualizer.gameObject);
        }
        chunk_visualizers.Clear();
    }
    public void CreateVisuals(string level_id = null)
    {
        // if level id is null, we get the current level id from the LevelEngine
        if (level_id == null) { level_id = LevelEngine.Instance.CurrentLevelID; }
        if (level_id == null) { log.Warning($"Can't find the current level ID"); return; }

        // grab the rooms data of the level from the LevelEngine
        List<ChunkData> chunks = LevelEngine.Instance.GetChunksDataOfLevel(level_id);

        // and build a visual for each room
        foreach (ChunkData chunk in chunks) { create_visu_for_chunk(chunk); }
    }
    private void create_visu_for_chunk(ChunkData rdata)
    {
        // 1. instanciate a visu
        UI_MapChunkVisualizer visualizer = Instantiate(ui_chunk_visu, transform);
        visualizer.name = rdata.id;

        // 2. initialize it with the room data
        visualizer.Initialize(rdata, loaded_color, main_color);

        // 3. add it to our list of visualizers
        chunk_visualizers.Add(visualizer);
    }
}