using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum TrainState
{
    Stopping,
    PreOpenDoor,
    OpeningDoors,
    LeavingTrain,
    EnteringTrain,
    ClosingDoors,
    PreLeaving,
    Moving
}

public struct TrainComponentData : IComponentData
{
    public Entity RailEntity;
    public TrainState State;
    public float WaitTimer;
    public float DoorMoveTimer;
}

public struct WagonComponentData : IComponentData
{
    public Entity TrainEntity;
    public int Index;
    public const float k_Length = 3f;
}
