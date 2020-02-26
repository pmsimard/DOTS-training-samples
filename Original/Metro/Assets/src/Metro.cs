using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Metro : MonoBehaviour
{
    public static float CUSTOMER_SATISFACTION = 1f;
    public static float BEZIER_HANDLE_REACH = 0.1f;
    public static float BEZIER_PLATFORM_OFFSET = 3f;
    public static float PLATFORM_ADJACENCY_LIMIT = 12f;
    public const int BEZIER_MEASUREMENT_SUBDIVISIONS = 2;
    public const float PLATFORM_ARRIVAL_THRESHOLD = 0.975f;
    public const float RAIL_SPACING = 0.5f;
    public static Metro INSTANCE;


    // PUBLICS
    [Tooltip("prefabs/Carriage")]
    public GameObject prefab_trainCarriage;
    [Tooltip("prefabs/Platform")]
    public GameObject prefab_platform;
    [Tooltip("prefabs/Commuter")]
    public GameObject prefab_commuter;
    [Tooltip("prefabs/Rail")]
    public GameObject prefab_rail;
    [Tooltip("Draw rail paths (eats CPU)")]
    public bool drawRailBeziers = false;
    [Tooltip("Number of commuters to spawn at the start")]
    public int maxCommuters = 2000;
    [Tooltip("Affects rail curve sharpness. 0 = no rounding, 1 = madness. Good value = 0.2 ish")]
    [Range(0f, 1f)] public float Bezier_HandleReach = 0.3f;
    [HideInInspector]
    public float Bezier_PlatformOffset = 3f;
    [Header("Trains")] public float Train_accelerationStrength = 0.001f;
    [Tooltip("How quickly trains lose speed")]
    public float Train_railFriction = 0.99f;
    [Tooltip("Once train has arrived, how long (in seconds) until doors open")]
    public float Train_delay_doors_OPEN = 2f;
    [Tooltip("Train load/unload is complete. Wait (X seconds) before closing doors")]
    public float Train_delay_doors_CLOSE = 1f;
    [Tooltip("Doors have closed, wait (X seconds) before departing")]
    public float Train_delay_departure = 1f;

    [Header("Commuters")]
    // walk speed etc
    [Header("MetroLines")]
    public string[] LineNames;
    public int[] maxTrains;
    public int[] carriagesPerTrain;
    public float[] maxTrainSpeed;
    private int totalLines = 0;
    public Color[] LineColours;

    [HideInInspector] public MetroLine[] metroLines;

    [HideInInspector] public List<Commuter> commuters;
    [HideInInspector] private Platform[] allPlatforms;

    

    public static Color GetLine_COLOUR_FromIndex(int _index)
    {
        Color result = Color.black;
        INSTANCE = FindObjectOfType<Metro>();
        if (INSTANCE != null)
        {
            if (INSTANCE.LineColours.Length - 1 >= _index)
            {
                result = INSTANCE.LineColours[_index];
            }
        }

        return result;
    }

    #region ------------------------- < GIZMOS

    private void OnDrawGizmos()
    {
        if (drawRailBeziers)
        {
            for (int i = 0; i < totalLines; i++)
            {
                MetroLine _tempLine = metroLines[i];
                if (_tempLine != null)
                {
                    BezierPath _path = _tempLine.bezierPath;
                    if (_path != null)
                    {
                        for (int pointIndex = 0; pointIndex < _path.points.Count; pointIndex++)
                        {
                            BezierPoint _CURRENT_POINT = _path.points[pointIndex];
                            BezierPoint _NEXT_POINT = _path.points[(pointIndex + 1) % _path.points.Count];
                            // Link them up
                            Handles.DrawBezier(_CURRENT_POINT.location, _NEXT_POINT.location, _CURRENT_POINT.handle_out,
                                _NEXT_POINT.handle_in, GetLine_COLOUR_FromIndex(i), null, 3f);
                        }
                    }
                }
            }
        }
    }

    #endregion ------------------------ GIZMOS >
}