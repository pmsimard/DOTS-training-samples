using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct SpeedManagementData : IComponentData
{
    public float MaxSpeed;
    public float CurrentSpeed;
    public float Acceleration;
}

public struct TargetReached : IComponentData
{
    public Entity TargetEntity;
}