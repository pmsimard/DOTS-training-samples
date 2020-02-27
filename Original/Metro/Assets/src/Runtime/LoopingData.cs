using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct LoopingData : IComponentData
{
    public int PathIndex;
    public Entity RailEntity;
}
