using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class Train
{
    public int trainIndex;
    public int totalCarriages;
    public int passengerCountOnDeparture;
    private float currentPosition = 0f;
    private int currentRegion;
    public float speed = 0f;
    public float speed_on_platform_arrival = 0f;
    public float accelerationStrength, brakeStrength, railFriction;
    public float stateDelay = 0f;
    public int parentLineIndex;
    public bool isOutbound;
    public MetroLine parentLine;
    public Platform nextPlatform;
    public Train trainAheadOfMe;
    public bool trainReadyToDepart = false;
}
