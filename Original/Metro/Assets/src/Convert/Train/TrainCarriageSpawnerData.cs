using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct TrainCarriageSpawnerData : IComponentData
{
    public Entity Prefab;
    public int PassengerCount;
    public int WagonPerLine;

    //private Material Mat;
}

public struct MaterialColor : IComponentData
{

}
