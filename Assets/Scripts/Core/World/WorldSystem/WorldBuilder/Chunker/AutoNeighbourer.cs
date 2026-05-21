using System.Collections.Generic;
using UnityEngine;

public class AutoNeighbourer : MonoBehaviour
{
    [SerializeField] private Loggable<AutoNeighbourer> log;

    public void TraceRoomGraphNeighbours(List<ChunkNeighbourDataInsideRoom> cndir, ref List<Chunk> chunks, ref List<Door> doors)
    {
        // we clear all the neighbours first
        foreach (var chunk in chunks) { chunk.ClearStaticNeighbors(); }

        // we first add all the neighbours inside the same room based on the chunk neighbour data inside room
        foreach (var room in cndir)
        {
            log.LogExtended($"(AutoNeighbourer) Room '{room.room_name}' has multiple chunks ! Tracing its inside neighbour chunk graph");
            foreach (var chunk_name in room.chunk_neighbours.Keys)
            {
                Chunk chunk = chunks.Find(c => c.ID == chunk_name);
                if (chunk == null)
                {
                    log.Warning($"(AutoNeighbourer) Chunk {chunk_name} not found in chunks list");
                    continue;
                }

                List<string> neighbour_names = room.chunk_neighbours[chunk_name];
                foreach (var neighbour_name in neighbour_names)
                {
                    Chunk neighbour_chunk = chunks.Find(c => c.ID == neighbour_name);
                    if (neighbour_chunk == null)
                    {
                        log.Warning($"(AutoNeighbourer) Neighbour chunk {neighbour_name} not found in chunks list");
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
            Chunk chunk_a = chunks.Find(r => r.ID == door.chunk1_id);
            Chunk chunk_b = chunks.Find(r => r.ID == door.chunk2_id);
            if (chunk_a == null || chunk_b == null)
            {
                log.Warning($"(AutoNeighbourer) Door {door.name} has invalid chunk ids: {door.chunk1_id}, {door.chunk2_id}");
                continue;
            }

            // we add the neighbour connection to both chunks
            chunk_a.AddStaticNeighbor(chunk_b);
            chunk_b.AddStaticNeighbor(chunk_a);
            log.LogExtended($"(AutoNeighbourer) Door '{door.name}' connects chunk '{chunk_a.ID}' and chunk '{chunk_b.ID}'");
        }
    }
}