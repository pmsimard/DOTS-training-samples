using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct SpeedManagementData : IComponentData
{
    public const float DefaultMaxSpeed = 21.0f; // 21 m/s OT 75 km/h

    public float MaxSpeed;
    public float CurrentSpeed;
    public float Acceleration;
    public bool NeedsAccelleration;
}

public struct TargetReached : IComponentData
{
}