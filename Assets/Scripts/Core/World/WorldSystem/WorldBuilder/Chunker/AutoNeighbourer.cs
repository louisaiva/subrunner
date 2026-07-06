using System.Collections.Generic;
using UnityEngine;

public class AutoNeighbourer : MonoBehaviour
{
    [SerializeField] private Loggable<AutoNeighbourer> log;

    public void TraceRoomGraphNeighbours(ref List<Chunk> chunks)
    {
        // we clear all the neighbours first
        foreach (var chunk in chunks) { chunk.ClearStaticNeighbors(); }
        compute_all_chunks_neighbours(ref chunks);
    }




    // CALCULATE CHUNK NEIGHBOURS
    private float epsilon = 0.1f;
    private void compute_all_chunks_neighbours(ref List<Chunk> chunks)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            Chunk chunk = chunks[i];
            Bounds bounds1 = chunk.ChunkCollider.bounds;

            for (int j = 0; j < chunks.Count; j++)
            {
                if (i == j) { continue; }

                // we check if the bounds of the pair of chunks are touching
                Bounds bounds2 = chunks[j].ChunkCollider.bounds;
                bounds1.Expand(epsilon * 2f);
                if (!bounds1.Intersects(bounds2))
                {
                    log.LogOMGThatsVeryVerySpecific($"Chunks {chunk.name} and {chunks[j].name} are not neighbours (bounds do not touch : {bounds1.min} - {bounds1.max} vs {bounds2.min} - {bounds2.max})");
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
}