using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct NavPointQueueData : IComponentData
{
    public Entity PlatformEntity;
    public int QueueIndex;
    public int CommuterCount;
}
