using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct TargetData : IComponentData
{
    public float3 Target;
}