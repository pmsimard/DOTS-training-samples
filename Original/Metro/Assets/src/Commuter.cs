using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


public enum CommuterState
{
    WALK,
    QUEUE,
    GET_ON_TRAIN,
    GET_OFF_TRAIN,
    WAIT_FOR_STOP,
}

public class CommuterTask
{
    public CommuterState state;
    public Vector3[] destinations;
    public int destinationIndex = 0;
    public Platform startPlatform, endPlatform;
    public Walkway walkway;

    public CommuterTask(CommuterState _state)
    {
        state = _state;
    }
}

public class Commuter : MonoBehaviour
{
    public const float ACCELERATION_STRENGTH = 0.01f;
    public const float FRICTION = 0.8f;
    public const float ARRIVAL_THRESHOLD = 0.02f;
    public const float QUEUE_PERSONAL_SPACE = 0.4f;
    public const float QUEUE_MOVEMENT_DELAY = 0.25f;
    public const float QUEUE_DECISION_RATE = 3f;

    public float satisfaction = 1f;
    public Transform body;
    private Queue<CommuterTask> route_TaskList;
    private CommuterTask currentTask;
    public Platform currentPlatform, route_START, route_END;
    public Platform nextPlatform;
    public Platform FinalDestination;
    private Vector3 speed = Vector3.zero;
    private float acceleration;
    private float stateDelay = 0f;
    public Queue<Commuter> currentQueue;
    private int myQueueIndex;
    private int carriageQueueIndex;
    private Walkway currentWalkway;
    public Train currentTrain;
    public TrainCarriage_door currentTrainDoor;
    public CommuterNavPoint currentSeat;
    private Transform t;
}