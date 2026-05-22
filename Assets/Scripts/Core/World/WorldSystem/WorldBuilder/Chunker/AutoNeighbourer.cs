using System.Collections.Generic;
using UnityEngine;

public class AutoNeighbourer : MonoBehaviour
{
    [SerializeField] private Loggable<AutoNeighbourer> log;

    public void TraceRoomGraphNeighbours(/* List<ChunkNeighbourDataInsideRoom> cndir,  */ref List<Chunk> chunks/* , ref List<Door> doors */)
    {
        // we clear all the neighbours first
        foreach (var chunk in chunks) { chunk.ClearStaticNeighbors(); }

        // we first add all the neighbours inside the same room based on the chunk neighbour data inside room
        /* foreach (var room in cndir)
        {
            log.LogExtended($"Room '{room.room_name}' has multiple chunks ! Tracing its inside neighbour chunk graph");
            foreach (var chunk_name in room.chunk_neighbours.Keys)
            {
                Chunk chunk = chunks.Find(c => c.ID == chunk_name);
                if (chunk == null)
                {
                    log.Warning($"Chunk {chunk_name} not found in chunks list");
                    continue;
                }

                List<string> neighbour_names = room.chunk_neighbours[chunk_name];
                foreach (var neighbour_name in neighbour_names)
                {
                    Chunk neighbour_chunk = chunks.Find(c => c.ID == neighbour_name);
                    if (neighbour_chunk == null)
                    {
                        log.Warning($"Neighbour chunk {neighbour_name} not found in chunks list");
                        continue;
                    }

                    // we add the neighbour connection to both chunks
                    chunk.AddStaticNeighbor(neighbour_chunk);
                    neighbour_chunk.AddStaticNeighbor(chunk);
                }
            }
        }

        // we go through all doors and we add the neighbour connections to the chunks
        foreach (var door in doors)
        {
            if (door == null)
            {
                log.Error($"Door is null in doors list");
                continue;
            }

            Chunk chunk_a = chunks.Find(r => r.ID == door.chunk1_id);
            Chunk chunk_b = chunks.Find(r => r.ID == door.chunk2_id);
            if (chunk_a == null || chunk_b == null)
            {
                log.Warning($"Door {door.name} has invalid chunk ids: {door.chunk1_id}, {door.chunk2_id}");
                continue;
            }

            // we add the neighbour connection to both chunks
            chunk_a.AddStaticNeighbor(chunk_b);
            chunk_b.AddStaticNeighbor(chunk_a);
            log.LogExtended($"Door '{door.name}' connects chunk '{chunk_a.ID}' and chunk '{chunk_b.ID}'");
        } */
    
        compute_all_chunks_neighbours(ref chunks);
    }




    // CALCULATE CHUNK NEIGHBOURS
    private float epsilon = 0.02f;
    private void compute_all_chunks_neighbours(ref List<Chunk> chunks)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            Chunk chunk = chunks[i];
            List<Vector2> vertices = chunk.ColliderPoints;
            Bounds2D bounds1 = get_bounds(vertices);

            for (int j = 0; j < chunks.Count; j++)
            {
                if (i == j) { continue; }

                // we check if the bounds of the pair of chunks are touching
                Bounds2D bounds2 = get_bounds(chunks[j].ColliderPoints);
                if (!could_aabb_touch(bounds1, bounds2, epsilon))
                {
                    log.LogVerySpecific($"Chunks {chunk.name} and {chunks[j].name} are not neighbours (bounds do not touch : {bounds1.min} - {bounds1.max} vs {bounds2.min} - {bounds2.max})");
                    continue;
                }

                // we check the distance between the colliders
                ColliderDistance2D distance = Physics2D.Distance(chunk.ChunkCollider, chunks[j].ChunkCollider);
                if (distance.isOverlapped || distance.distance <= epsilon)
                {
                    chunk.AddStaticNeighbor(chunks[j]);
                    log.LogVerySpecific($"Chunk {chunks[i].name} is neighbour with chunk {chunks[j].name} (distance {distance.distance})");
                }
            }
        }
    }
    private bool could_aabb_touch(Bounds2D bA, Bounds2D bB, float eps, float min_overlap = 0.25f, bool allow_corner_touching = true)
    {
        // get the overlaps
        float x_overlap = Mathf.Min(bA.max.x, bB.max.x) - Mathf.Max(bA.min.x, bB.min.x);
        float y_overlap = Mathf.Min(bA.max.y, bB.max.y) - Mathf.Max(bA.min.y, bB.min.y);

        // check touching with epsilon
        bool x_touching = Mathf.Abs(bA.max.x - bB.min.x) <= eps || Mathf.Abs(bB.max.x - bA.min.x) <= eps;
        bool y_touching = Mathf.Abs(bA.max.y - bB.min.y) <= eps || Mathf.Abs(bB.max.y - bA.min.y) <= eps;

        float overlap_required = allow_corner_touching ? 0f : min_overlap;

        if (x_touching && y_overlap >= overlap_required) { return true; }
        if (y_touching && x_overlap >= overlap_required) { return true; }
        return false;
    }
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
        return bounds;
    }
}