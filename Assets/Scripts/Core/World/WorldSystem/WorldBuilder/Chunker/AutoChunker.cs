using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoChunker : MonoBehaviour
{
    [Header("Auto Chunk parameters")]
    [SerializeField] private int max_chunk_area = 10;

    [Header("References")]
    [SerializeField] private WorldRoomVisualizer chunk_prefab;
    [SerializeField] private Transform chunk_parent;

    [SerializeField] private Loggable<AutoChunker> log;

    // MAIN ENTRY POINT
    public BuiltLevelData ChunkRooms(BuiltLevelData built_data)
    {
        log.Log($"Auto chunking rooms bigger than {max_chunk_area} area");
        BuiltLevelData new_data = new BuiltLevelData()
        {
            world = built_data.world,
            level = built_data.level,
            Tilemaps = built_data.Tilemaps,
            RoomChunks = new Dictionary<string, List<string>>(),
            Chunks = new List<WorldRoomVisualizer>(),
            RoomChunksNeighbours = new List<ChunkNeighbourDataInsideRoom>()
        };

        WorldRoomVisualizer room;
        List<WorldRoomVisualizer> chunks;
        for (int i = 0; i < built_data.Chunks.Count; i++)
        {
            log.LogExtended($"Processing room {built_data.Chunks[i].name} with area {built_data.Chunks[i].PolygonCollider.bounds.size.x * built_data.Chunks[i].PolygonCollider.bounds.size.y}");
            room = built_data.Chunks[i];
            if (is_small_enough(room))
            {
                string room_name = room.name;
                log.LogSpecific($"Room {room_name} is small enough, we keep it as is");
                string chunk_name = room_name + "-0"; // we add a suffix for saying hey this is a chunk
                new_data.RoomChunks[room_name] = new List<string>() { chunk_name };
                room.name = chunk_name;
                new_data.Chunks.Add(room);
                continue;
            }

            // if the room is too big, we split it recursively until all small chunks are small enough
            log.LogSpecific($"Room {room.name} is too big, we split it in smaller chunks");
            chunks = split_room(room, out List<string> chunk_names, out ChunkNeighbourDataInsideRoom cndir);
            new_data.RoomChunksNeighbours.Add(cndir);

            // we set the names of the chunks and add them to the data
            new_data.RoomChunks[room.name] = chunk_names;
            new_data.Chunks.AddRange(chunks);
        }
        room = null;
        chunks = null;

        return new_data;
    }

    // IS SMALL ENOUGH
    private bool is_small_enough(WorldRoomVisualizer room) { return is_small_enough(room.PolygonCollider.bounds); }
    private bool is_small_enough(Bounds bounds)
    {
        float area = bounds.size.x * bounds.size.y;
        return area <= max_chunk_area;
    }
    private bool is_small_enough(Bounds2D bounds) { return bounds.area <= max_chunk_area; }


    // SPLITTING ROOM
    private List<WorldRoomVisualizer> split_room(WorldRoomVisualizer room, out List<string> chunk_names, out ChunkNeighbourDataInsideRoom cndir)
    {
        chunk_names = new List<string>();
        cndir = new ChunkNeighbourDataInsideRoom
        {
            room_name = room.name,
            chunk_neighbours = new Dictionary<string, List<string>>()
        };
        log.Log($"Splitting room {room.name} in small chunks of max area {max_chunk_area}");

        // split vertices
        List<List<Vector2>> small_chunks_vertices = new List<List<Vector2>>();
        recursive_iterations = 0;
        split_room_along_axis_recursive(room.Path.ToList(), ref small_chunks_vertices);
        log.LogExtended($"Got {small_chunks_vertices.Count} small chunks for room {room.name}");

        // create chunk visualizers
        log.LogExtended($"Creating visualizers for the chunks");
        List<WorldRoomVisualizer> chunks = new List<WorldRoomVisualizer>();
        for (int i = 0; i < small_chunks_vertices.Count; i++)
        {
            WorldRoomVisualizer chunk = Instantiate(chunk_prefab, chunk_parent);
            chunk.Path = small_chunks_vertices[i].ToArray();
            chunk.name = $"{room.name}-{i}";
            chunks.Add(chunk);
            chunk_names.Add(chunk.name);
            log.LogSpecific($"Created chunk {chunk.name}");
        }

        // split the doors & lights
        log.LogExtended($"Splitting doors and lights across the chunks");
        List<WorldDoorVisualizer> doors_left = room.Doors;
        List<WorldLightVisualizer> lights_left = room.Lights;
        foreach (WorldRoomVisualizer chunk in chunks)
        {
            for (int i = doors_left.Count - 1; i >= 0; i--)
            {
                if (!chunk.CollideWithCell(doors_left[i].Cell) && !chunk.CollideWithCell(doors_left[i].OtherCell)) { continue; }
                
                log.LogSpecific($"Assigned door {doors_left[i].name} to chunk {chunk.name}");
                chunk.Doors.Add(doors_left[i]);
                doors_left.RemoveAt(i);
            }

            for (int i = lights_left.Count - 1; i >= 0; i--)
            {
                if (!chunk.CollideWithCell(lights_left[i].Cell)) { continue; }

                log.LogSpecific($"Assigned light {lights_left[i].name} to chunk {chunk.name}");
                chunk.Lights.Add(lights_left[i]);
                lights_left.RemoveAt(i);
            }
        }
        if (doors_left.Count > 0) { log.Error($"Some DOORS were not assigned to any chunk for room {room.name} : {string.Join(", ", doors_left.Select(d => d.name))}"); }
        if (lights_left.Count > 0) { log.Error($"Some lights were not assigned to any chunk for room {room.name} : {string.Join(", ", lights_left.Select(l => l.name))}"); }


        // calculate the neighbours between the chunks inside the same room
        int max_neighbour_distance_threshold = (int)(max_chunk_area * 3f);
        for (int i = 0; i < chunks.Count; i++)
        {
            cndir.chunk_neighbours[chunks[i].name] = new List<string>();
            for (int j = 0; j < chunks.Count; j++)
            {
                if (i == j) { continue; }

                float distance = Vector2.Distance(chunks[i].PolygonCollider.bounds.center, chunks[j].PolygonCollider.bounds.center);
                if (distance <= max_neighbour_distance_threshold)
                {
                    cndir.chunk_neighbours[chunks[i].name].Add(chunks[j].name);
                    log.LogSpecific($"Chunk {chunks[i].name} is neighbour with chunk {chunks[j].name} (distance {distance})");
                }
            }
        }

        return chunks;
    }
    private static int recursive_iterations = 0;
    private void split_room_along_axis_recursive(List<Vector2> room_vertices, ref List<List<Vector2>> small_chunks)
    {
        recursive_iterations++;
        if (recursive_iterations > 100) { log.Error($"Too much recursive iterations in split_room_along_axis_recursive, something is probably wrong with the algorithm"); return; }

        // we check which axis is the longest and we split the room in 2 along this axis
        Bounds2D bounds = get_bounds(room_vertices);
        log.LogExtended($"Splitting room with area {bounds.area} and bounds {bounds.min} - {bounds.max} and center {bounds.center}");

        bool split_along_x = bounds.width >= bounds.height;
        float split_value = split_along_x ? bounds.center.x : bounds.center.y;

        log.LogExtended($"Splitting room along {(split_along_x ? "X" : "Y")} axis at value {split_value} (already gathered {small_chunks.Count} small chunks)");

        List<Vector2> chunk1_vertices = new List<Vector2>();
        List<Vector2> chunk2_vertices = new List<Vector2>();

        log.LogVerySpecific($"Room vertices are : \n - {string.Join("\n - ", room_vertices)}");

        // we split the bounds in 2
        Vector2 start_vertex;
        Vector2 end_vertex;
        for (int i = 0; i < room_vertices.Count; i++)
        {
            start_vertex = room_vertices[i];
            end_vertex = room_vertices[(i + 1) % room_vertices.Count]; // always i+1 except for the last one where == 0

            // check where both vertices are compared to the split value
            bool start_in_chunk1 = (split_along_x ? start_vertex.x : start_vertex.y) <= split_value;
            bool end_in_chunk1 = (split_along_x ? end_vertex.x : end_vertex.y) <= split_value;

            // add start to the chunk it belongs to
            if (start_in_chunk1) { if (!chunk1_vertices.Contains(start_vertex)) { chunk1_vertices.Add(start_vertex); } }
            else if (!chunk2_vertices.Contains(start_vertex)) { chunk2_vertices.Add(start_vertex); }

            // if both vertex are in the same chunk, we good !
            if (start_in_chunk1 == end_in_chunk1) { continue; }

            // else we just crossed the split line, we need to add the intersection point to both chunks
            Vector2 intersection = get_line_intersection(start_vertex, end_vertex, split_along_x, split_value);
            if (!chunk1_vertices.Contains(intersection)) { chunk1_vertices.Add(intersection); }
            if (!chunk2_vertices.Contains(intersection)) { chunk2_vertices.Add(intersection); }
        }

        // calculate the bounds
        Bounds2D bound1 = get_bounds(chunk1_vertices);
        Bounds2D bound2 = get_bounds(chunk2_vertices);

        // logging vertices
        log.LogVerySpecific($"Chunk 1 has area {bound1.area} and bounds {bound1.min} - {bound1.max} and center {bound1.center} and vertices : \n - {string.Join("\n - ", chunk1_vertices)}");
        log.LogVerySpecific($"Chunk 2 has area {bound2.area} and bounds {bound2.min} - {bound2.max} and center {bound2.center} and vertices : \n - {string.Join("\n - ", chunk2_vertices)}");

        // now we can check if the chunks are small enough, if not we split them recursively
        if (is_small_enough(bound1))
        {
            log.LogSpecific($"New chunk with area {bound1.area} created");
            small_chunks.Add(chunk1_vertices);
        }
        else
        {
            log.LogSpecific($"Chunk with area {bound1.area} is still too big, we split it recursively");
            split_room_along_axis_recursive(chunk1_vertices, ref small_chunks);
        }

        if (is_small_enough(bound2))
        {
            log.LogSpecific($"New chunk with area {bound2.area} created");
            small_chunks.Add(chunk2_vertices);
        }
        else
        {
            log.LogSpecific($"Chunk with area {bound2.area} is still too big, we split it recursively");
            split_room_along_axis_recursive(chunk2_vertices, ref small_chunks);
        }
    }

    // SPLIT LOW LEVEL METHODS
    private Bounds2D get_bounds(List<Vector2> vertices)
    {
        // get the min and max of the vertices
        float min_x = float.MaxValue;
        float max_x = float.MinValue;
        float min_y = float.MaxValue;
        float max_y = float.MinValue;
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].x < min_x) { min_x = vertices[i].x; }
            if (vertices[i].x > max_x) { max_x = vertices[i].x; }
            if (vertices[i].y < min_y) { min_y = vertices[i].y; }
            if (vertices[i].y > max_y) { max_y = vertices[i].y; }
        }
        Bounds2D bounds = new Bounds2D(min: new Vector2(min_x, min_y), max: new Vector2(max_x, max_y));
        /* {
            min = new Vector2(min_x, min_y),
            max = new Vector2(max_x, max_y)
        }; */
        return bounds;
    }
    private Vector2 get_line_intersection(Vector2 start, Vector2 end, bool split_along_x, float split_value)
    {
        // we want to find the intersection between the line (start, end) and the line x = split_value or y = split_value depending on the axis
        if (split_along_x)
        {
            // line is x = split_value
            float t = (split_value - start.x) / (end.x - start.x);
            return new Vector2(split_value, start.y + t * (end.y - start.y));
        }
        else
        {
            // line is y = split_value
            float t = (split_value - start.y) / (end.y - start.y);
            return new Vector2(start.x + t * (end.x - start.x), split_value);
        }
    }
}

public class ChunkNeighbourDataInsideRoom
{
    public string room_name;
    public Dictionary<string, List<string>> chunk_neighbours; // chunk name -> list of neighbour chunk names inside the same room
}