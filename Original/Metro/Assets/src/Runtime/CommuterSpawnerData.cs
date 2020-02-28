using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct CommuterSpawnerData : IComponentData
{
    [Range(0, 100)]
    public int WagonFillRatio;
    public int PerPlatformCount;
}
