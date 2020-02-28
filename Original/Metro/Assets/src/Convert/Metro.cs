using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class Metro : MonoBehaviour, IConvertGameObjectToEntity
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
    [Tooltip("prefabs/Rail_Accel")]
    public GameObject prefab_rail_accel;
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

    public static string GetLine_NAME_FromIndex(int _index)
    {
        string result = "";
        INSTANCE = FindObjectOfType<Metro>();
        if (INSTANCE != null)
        {
            if (INSTANCE.LineNames.Length - 1 >= _index)
            {
                result = INSTANCE.LineNames[_index];
            }
        }

        return result;
    }

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

    void SetupMetroLines()
    {
        totalLines = LineNames.Length;
        metroLines = new MetroLine[totalLines];
        for (int i = 0; i < totalLines; i++)
        {
            // Find all of the relevant RailMarkers in the scene for this line
            List<RailMarker> _relevantMarkers = FindObjectsOfType<RailMarker>().Where(m => m.metroLineID == i)
                .OrderBy(m => m.pointIndex).ToList();

            // Only continue if we have something to work with
            if (_relevantMarkers.Count > 1)
            {
                MetroLine _newLine = new MetroLine(i, maxTrains[i]);
                _newLine.Create_RailPath(_relevantMarkers);
                metroLines[i] = _newLine;
            }
            else
            {
                Debug.LogWarning("Insufficient RailMarkers found for line: " + i +
                                 ", you need to add the outbound points");
            }
        }

        // now destroy all RailMarkers
        foreach (RailMarker _RM in FindObjectsOfType<RailMarker>())
        {
            Destroy(_RM);
        }

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
                    for (int index = 1; index < _tempLine.BakedPositionPath.Length; ++index)
                    {
                        Handles.DrawLine(_tempLine.BakedPositionPath[index - 1].Value, _tempLine.BakedPositionPath[index].Value);
                    }
                }
            }
        }   
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        BEZIER_HANDLE_REACH = Bezier_HandleReach;
        BEZIER_PLATFORM_OFFSET = Bezier_PlatformOffset;
        SetupMetroLines();

        //create rail lines
        var parentGo = new GameObject("Metro");
        for (int i = 0; i < metroLines.Length; i++)
        {
            var line = metroLines[i];
            var lineEntity = line.Convert(entity, dstManager, parentGo, prefab_rail, prefab_rail_accel, prefab_platform, prefab_trainCarriage);
        }

        var conversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        //commuters
        var commuterPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab_commuter, conversionSettings);
        int skipSeats = 1; // 50%
        var seatQuery = dstManager.CreateEntityQuery(ComponentType.ReadOnly<NavPointCarriageSeatData>());
        var seatEntities = seatQuery.ToEntityArray(Allocator.Persistent);
        for (int i = 0; i < seatEntities.Length; i++)
        {
            var commuterEntity = dstManager.Instantiate(commuterPrefabEntity);

            dstManager.AddComponentData(commuterEntity, new Parent() { Value = seatEntities[i] });
            dstManager.AddComponentData(commuterEntity, new LocalToParent() { Value = float4x4.identity });

            i += skipSeats;
        }

        seatEntities.Dispose();


        int entityCountPerQueueMin = 1;
        int entityCountPerQueueMax = 20;
        float distancePerCommuter = 0.4f;
        var queueQuery = dstManager.CreateEntityQuery(ComponentType.ReadOnly<NavPointQueueData>());
        var queueEntities = queueQuery.ToEntityArray(Allocator.Persistent);
        for (int i = 0; i < queueEntities.Length; i++)
        {
            var parent = dstManager.GetComponentData<Parent>(queueEntities[i]).Value;
            var parentPos = dstManager.GetComponentData<Translation>(parent).Value;
            var parentRot = dstManager.GetComponentData<Rotation>(parent).Value;
            var localPos = dstManager.GetComponentData<Translation>(queueEntities[i]).Value;
            var localRot = dstManager.GetComponentData<Rotation>(queueEntities[i]).Value;
            var pos = math.mul(parentRot, localPos) + parentPos;
            var rot = math.mul(parentRot, localRot);
            var forward = math.mul(rot, Vector3.forward);
            var deltaQueue = forward * distancePerCommuter;
            
            var count = UnityEngine.Random.Range(entityCountPerQueueMin, entityCountPerQueueMax);
            for (int j = 0; j < count; ++j)
            {
                var commuterEntity = dstManager.Instantiate(commuterPrefabEntity);
                dstManager.SetComponentData(commuterEntity, new Translation() { Value = pos + deltaQueue * j });
            }
            
        }

        queueEntities.Dispose();
    }

    #endregion ------------------------ GIZMOS >
}