using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using System.Threading;

[BurstCompile]
public struct CollisionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    [ReadOnly] public float3 playerPos;
    [ReadOnly] public float radiusSq;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> hitCount;

    public void Execute(int i)
    {
        float3 diff = positions[i] - playerPos;
        float distSq = diff.x * diff.x + diff.y * diff.y;
        if (distSq < radiusSq)
        {
            hitCount[0] = 1;
        }
    }
}
