using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[Obsolete("We do not use this job for room switching anymore")]
[BurstCompile]
public struct HandleRoomTransitionsJob : IJob
{
    [ReadOnly] public NativeParallelMultiHashMap<int, int> movables_going_IN; // room, List<movable>
    [ReadOnly] public NativeParallelMultiHashMap<int, int> movables_going_OUT; // movable, List<room>
    public NativeList<MovableRoomTransition> transitions;


    public void Execute()
    {

        // 1. TROUVER LES PAIRES DE MEME MOVABLE DANS going_IN & going_OUT
        // we loop through all the movables going IN
        foreach (var pair in movables_going_IN)
        {
            int room_id = pair.Key;
            int movable_id = pair.Value;

            // we check if the same movable is going OUT
            if (movables_going_OUT.TryGetFirstValue(movable_id, out int out_room_id, out var iterator))
            {
                do
                {
                    // we ignore fake transitions where IN and OUT point to the same room
                    if (out_room_id == room_id) { continue; }

                    // if we find a pair, it means the movable is transitioning from out_room_id to room_id
                    MovableRoomTransition transition = new MovableRoomTransition
                    {
                        movable_hash = movable_id,
                        // compatibility: fields store room runtime ids even if names still mention index
                        from_room_index = out_room_id,
                        to_room_index = room_id
                    };
                    transitions.Add(transition);
                }
                while (movables_going_OUT.TryGetNextValue(out out_room_id, ref iterator));
            }
        }
    }
}


public struct MovableRoomTransition
{
    public int movable_hash;
    public int from_room_index;
    public int to_room_index;
}