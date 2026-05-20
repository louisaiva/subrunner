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

        // we go through all doors and we add the neighbour connections to the rooms
        foreach (var door in doors)
        {
            Chunk room_a = chunks.Find(r => r.ID == door.room1_id);
            Chunk room_b = chunks.Find(r => r.ID == door.room2_id);
            if (room_a == null || room_b == null)
            {
                log.Warning($"(AutoNeighbourer) Door {door.name} has invalid room ids: {door.room1_id}, {door.room2_id}");
                continue;
            }

            // we add the neighbour connection to both rooms
            room_a.AddStaticNeighbor(room_b);
            room_b.AddStaticNeighbor(room_a);
        }
    }
}