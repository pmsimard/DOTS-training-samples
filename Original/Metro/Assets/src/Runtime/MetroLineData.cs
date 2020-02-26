using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct MetroLineComponentData : IComponentData
{
    public NativeArray<float3> RailPositions;
    public NativeArray<float3> RailNormals;
}
