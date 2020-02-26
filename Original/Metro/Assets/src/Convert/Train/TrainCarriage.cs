using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainCarriage : MonoBehaviour
{
    public const float CARRIAGE_LENGTH = 5f;
    public const int CARRIAGE_CAPACITY = 10;
    public const float CARRIAGE_SPACING = 0.25f;

    public float positionOnRail;
    public List<Commuter> passengers;
    public List<CommuterNavPoint> seats_FREE;
    public List<CommuterNavPoint> seats_TAKEN;
    public int passengerCount;
    public TrainCarriage_door door_LEFT;
    public TrainCarriage_door door_RIGHT;
    public GameObject[] RecolouredObjects;


    private Transform t;
    private Material mat;
}