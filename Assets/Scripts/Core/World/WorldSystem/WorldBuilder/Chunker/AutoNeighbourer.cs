using System.Collections.Generic;
using UnityEngine;

public class AutoNeighbourer : MonoBehaviour
{
    public static bool log = false;

    public static void TraceRoomGraphNeighbours(ref List<Room> rooms, ref List<Door> doors)
    {
        // we clear all the neighbours first
        foreach (var room in rooms) { room.ClearStaticNeighbors(); }

        // we go through all doors and we add the neighbour connections to the rooms
        foreach (var door in doors)
        {
            Room room_a = rooms.Find(r => r.ID == door.room1_id);
            Room room_b = rooms.Find(r => r.ID == door.room2_id);
            if (room_a == null || room_b == null)
            {
                if (log) { Debug.LogWarning($"(AutoNeighbourer) Door {door.name} has invalid room ids: {door.room1_id}, {door.room2_id}"); }
                continue;
            }

            // we add the neighbour connection to both rooms
            room_a.AddStaticNeighbor(room_b);
            room_b.AddStaticNeighbor(room_a);
        }
    }
}