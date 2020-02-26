using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct MetroLinePositionElement : IBufferElementData
{
    // Actual value each buffer element will store.
    public float3 Value;

    // The following implicit conversions are optional, but can be convenient.
    public static implicit operator float3(MetroLinePositionElement e)
    {
        return e.Value;
    }

    public static implicit operator MetroLinePositionElement(float3 e)
    {
        return new MetroLinePositionElement { Value = e };
    }
}

[InternalBufferCapacity(8)]
public struct MetroLineNormalElement : IBufferElementData
{
    // Actual value each buffer element will store.
    public float3 Value;

    // The following implicit conversions are optional, but can be convenient.
    public static implicit operator float3(MetroLineNormalElement e)
    {
        return e.Value;
    }

    public static implicit operator MetroLineNormalElement(float3 e)
    {
        return new MetroLineNormalElement { Value = e };
    }
}

