using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct MetroLineBlobData
{
    public NativeArray<float3> RailPositions;
    public NativeArray<float3> RailNormals;
}

public struct MetroLineComponentData : IComponentData
{
    public float zero;
}