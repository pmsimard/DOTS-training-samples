using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Runtime : IComponentData
{
    float3 point;
}
