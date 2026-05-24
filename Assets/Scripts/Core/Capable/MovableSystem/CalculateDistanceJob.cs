using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

// We'll use Unity.Mathematics.float3 instead of Vector3,
// and we'll use Unity.Mathematics.math.distancesq instead of Vector3.sqrMagnitude.
using Unity.Mathematics;

// Include the BurstCompile attribute to Burst compile the job.
[BurstCompile]
public struct CalculateDistanceJob : IJob
{
    [ReadOnly] public NativeArray<float3> MovablePositions;
    public NativeArray<float> Distances;

    public void Execute()
    {
        int index = 0;
        for (int i = 1; i < MovablePositions.Length; i++)
        {
            for (int j = 0; j < i; j++)
            {
                float distSq = math.distancesq(MovablePositions[j], MovablePositions[i]);
                Distances[index] = distSq;
                index++;
            }
        }
    }
}