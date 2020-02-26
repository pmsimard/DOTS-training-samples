using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int carriageCount, platformIndex;
    public MetroLine parentMetroLine;
    public List<Walkway> walkways;
    public Walkway walkway_FRONT_CROSS, walkway_BACK_CROSS;
    public BezierPoint point_platform_START, point_platform_END;
    public Platform oppositePlatform;
    public Platform nextPlatform;
    public List<Platform> adjacentPlatforms;
    public Queue<Commuter>[] platformQueues;
    public CommuterNavPoint[] queuePoints;
    public Train currentTrainAtPlatform;

    public int temporary_routeDistance = 0;
    public Platform temporary_accessedViaPlatform;
    public CommuterState temporary_connectionType;

    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        Handles.Label(transform.position, "" + parentMetroLine.lineName + "_" + platformIndex);
    }
}
