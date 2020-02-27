using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct NavPointCarriageSeatData : IComponentData
{
    public Entity WagonEntity;
    public bool Available;
}