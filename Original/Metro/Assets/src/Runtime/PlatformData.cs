using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PlatformData : IComponentData
{
    public Entity RailEntity;
    public int PlatformIndex;
    public int RailSampleIndex;
}
