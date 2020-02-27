using Unity.Entities;
using Unity.Mathematics;

// Zero size internal buffer array to immediately allocate in the RAM
// as our track data is fairly large
[InternalBufferCapacity(0)]
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

// Zero size internal buffer array to immediately allocate in the RAM
// as our track data is fairly large
[InternalBufferCapacity(0)]
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

// Zero size internal buffer array to immediately allocate in the RAM
// as our track data is fairly large
[InternalBufferCapacity(0)]
public struct MetroLineAccelerationStateElement : IBufferElementData
{
    // Actual value each buffer element will store.
    public bool Value;

    // The following implicit conversions are optional, but can be convenient.
    public static implicit operator bool(MetroLineAccelerationStateElement e)
    {
        return e.Value;
    }

    public static implicit operator MetroLineAccelerationStateElement(bool e)
    {
        return new MetroLineAccelerationStateElement { Value = e };
    }
}