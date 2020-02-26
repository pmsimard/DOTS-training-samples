using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;


public enum TrainState
{
    EN_ROUTE,
    ARRIVING,
    DOORS_OPEN,
    UNLOADING,
    LOADING,
    DOORS_CLOSE,
    DEPARTING,
    EMERGENCY_STOP
}

public class Train
{
    public int trainIndex;
    public List<TrainCarriage> carriages;
    public int totalCarriages;
    public List<Commuter> passengers;
    public List<Commuter> passengers_to_DISEMBARK;
    public List<Commuter> passengers_to_EMBARK;
    public int passengerCountOnDeparture;
    private float currentPosition = 0f;
    private int currentRegion;
    public float speed = 0f;
    public float speed_on_platform_arrival = 0f;
    public float accelerationStrength, brakeStrength, railFriction;
    public float stateDelay = 0f;
    public int parentLineIndex;
    public bool isOutbound;
    public TrainState state;
    public MetroLine parentLine;
    public Platform nextPlatform;
    public Train trainAheadOfMe;
    public bool trainReadyToDepart = false;
}