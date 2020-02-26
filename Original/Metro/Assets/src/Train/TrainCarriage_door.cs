using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCarriage_door : MonoBehaviour
{
    private const float DOOR_ACCELERATION = 0.0015f;
    private const float DOOR_FRICTION = 0.9f;
    private const float DOOR_ARRIVAL_THRESHOLD = 0.001f;
    
    public Transform door_LEFT, door_RIGHT;
    private float left_OPEN_X, left_CLOSED_X;
    private float door_SPEED = 0f;
    public CommuterNavPoint door_navPoint;
}
