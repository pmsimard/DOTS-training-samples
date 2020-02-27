using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum TrainState
{
    Departing,
    Arriving,
    Stopped,
    OpeningDoors,
    DoorsOpened,
    ClosingDoors,
    DoorsClosed,
    InTransit,
}

public struct TrainComponentData : IComponentData
{
    public const float WaitTimerInitialValue = 1.0f;
    public const float DoorMoveTimerInitialValue = 1.0f;
    public const float DoorOpenedTimerInitialValue = 5.0f;

    public Entity RailEntity;
    public TrainState State;
    public float WaitTimer;
    public float DoorMoveTimer;
    public float DoorOpenedTimer;
}

public struct WagonComponentData : IComponentData
{
    public Entity TrainEntity;
    public int Index;
    public const float k_Length = 3f;
}
