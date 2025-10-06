using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FindNeighboursJob : IJob
{
    [ReadOnly] public NativeArray<float3> MovablePositions;
    [ReadOnly] public int movableIndex;
    [ReadOnly] public float maxDistanceSq;
    public NativeList<int> neighbours;

    public void Execute()
    {
        float3 currentPosition = MovablePositions[movableIndex];
        for (int i = 0; i < MovablePositions.Length; i++)
        {
            if (i == movableIndex) { continue; }

            float distSq = math.distancesq(currentPosition, MovablePositions[i]);
            if (distSq <= maxDistanceSq)
            {
                // This movable is a neighbor
                neighbours.Add(i);
            }
        }
    }
}