using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct MetroLineComponentData : IComponentData
{
    public BlobAssetReference<float3> RailPositions;
    public BlobAssetReference<float3> RailNormals;
}