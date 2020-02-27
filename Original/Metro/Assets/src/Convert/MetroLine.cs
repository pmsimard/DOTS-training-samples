using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

public class MetroLine
{
    public string lineName;
    public int metroLine_index;
    public Color lineColour;
    public BezierPath bezierPath;
    public List<Train> trains;
    public List<Platform> platforms;
    public int maxTrains;
    public float maxTrainSpeed;
    public Vector3[] railPath;
    public int carriagesPerTrain;
    public float train_accelerationStrength = 0.0003f;
    public float train_brakeStrength = 0.01f;
    public float train_friction = 0.95f;
    public float speedRatio;
    public float carriageLength_onRail;

    public NativeArray<MetroLinePositionElement> BakedPositionPath;
    public NativeArray<MetroLineNormalElement> BakedNormalPath;
    public NativeArray<MetroLineAccelerationStateElement> BakedAccelPath;
   
    public MetroLine(int metroLineIndex, int _maxTrains)
    {
        metroLine_index = metroLineIndex;
        maxTrains = _maxTrains;
        trains = new List<Train>();
        platforms = new List<Platform>();
    }

    public void Create_RailPath(List<RailMarker> _outboundPoints)
    {
        const float CARRIAGE_LENGTH = 5f;
        const int CARRIAGE_CAPACITY = 10;
        const float CARRIAGE_SPACING = 0.25f;

    bezierPath = new BezierPath();
        List<BezierPoint> _POINTS = bezierPath.points;
        int total_outboundPoints = _outboundPoints.Count;
        Vector3 currentLocation = Vector3.zero;

        // - - - - - - - - - - - - - - - - - - - - - - - -  OUTBOUND points
        for (int i = 0; i < total_outboundPoints; i++)
        {
            bezierPath.AddPoint(_outboundPoints[i].transform.position);
        }

        // fix the OUTBOUND handles
        for (int i = 0; i <= total_outboundPoints - 1; i++)
        {
            BezierPoint _currentPoint = _POINTS[i];
            if (i == 0)
            {
                _currentPoint.SetHandles(_POINTS[1].location - _currentPoint.location);
            }
            else if (i == total_outboundPoints - 1)
            {
                _currentPoint.SetHandles(_currentPoint.location - _POINTS[i - 1].location);
            }
            else
            {
                _currentPoint.SetHandles(_POINTS[i + 1].location - _POINTS[i - 1].location);
            }
        }

        bezierPath.MeasurePath();

        // - - - - - - - - - - - - - - - - - - - - - - - -  RETURN points
        float platformOffset = Metro.BEZIER_PLATFORM_OFFSET;
        List<BezierPoint> _RETURN_POINTS = new List<BezierPoint>();
        for (int i = total_outboundPoints - 1; i >= 0; i--)
        {
            Vector3 _targetLocation = bezierPath.GetPoint_PerpendicularOffset(bezierPath.points[i], platformOffset);
            bezierPath.AddPoint(_targetLocation);
            _RETURN_POINTS.Add(_POINTS[_POINTS.Count - 1]);
        }

        // fix the RETURN handles
        for (int i = 0; i <= total_outboundPoints - 1; i++)
        {
            BezierPoint _currentPoint = _RETURN_POINTS[i];
            if (i == 0)
            {
                _currentPoint.SetHandles(_RETURN_POINTS[1].location - _currentPoint.location);
            }
            else if (i == total_outboundPoints - 1)
            {
                _currentPoint.SetHandles(_currentPoint.location - _RETURN_POINTS[i - 1].location);
            }
            else
            {
                _currentPoint.SetHandles(_RETURN_POINTS[i + 1].location - _RETURN_POINTS[i - 1].location);
            }
        }

        bezierPath.MeasurePath();
        int bezierPointIndex = 0;
        var isPlatformPosition = new List<KeyValuePair<bool, float>>(bezierPath.points.Count);
        for (int i = 0; i <= total_outboundPoints - 1; i++)
        {
            bool isPlatform = false;
            if (_outboundPoints[i].railMarkerType == RailMarkerType.PLATFORM_END || _outboundPoints[i].railMarkerType == RailMarkerType.PLATFORM_START)
            {
                isPlatform = true;
            }

            isPlatformPosition.Add(new KeyValuePair<bool, float>(isPlatform, bezierPath.points[bezierPointIndex].distanceAlongPath));
            bezierPointIndex++;
        }

        for (int i = total_outboundPoints - 1; i >= 0; i--)
        {
            bool isPlatform = false;
            if (_outboundPoints[i].railMarkerType == RailMarkerType.PLATFORM_END || _outboundPoints[i].railMarkerType == RailMarkerType.PLATFORM_START)
            {
                isPlatform = true;
            }

            isPlatformPosition.Add(new KeyValuePair<bool, float>(isPlatform, bezierPath.points[bezierPointIndex].distanceAlongPath));
            bezierPointIndex++;
        }

        carriageLength_onRail = Get_distanceAsRailProportion(CARRIAGE_LENGTH) +
                                Get_distanceAsRailProportion(CARRIAGE_SPACING);

        /*
        // now that the rails have been laid - let's put the platforms on
        int totalPoints = bezierPath.points.Count;
        for (int i = 1; i < _outboundPoints.Count; i++)
        {
            int _plat_END = i;
            int _plat_START = i - 1;
            if (_outboundPoints[_plat_END].railMarkerType == RailMarkerType.PLATFORM_END &&
                _outboundPoints[_plat_START].railMarkerType == RailMarkerType.PLATFORM_START)
            {
                Platform _ouboundPlatform = AddPlatform(_plat_START, _plat_END);
                // now add an opposite platform!
                int opposite_START = totalPoints - (i + 1);
                int opposite_END = totalPoints - i;
                Platform _oppositePlatform = AddPlatform(opposite_START, opposite_END);
                _oppositePlatform.transform.eulerAngles =
                    _ouboundPlatform.transform.rotation.eulerAngles + new Vector3(0f, 180f, 0f);
                ;

                // pair these platforms as opposites
                _ouboundPlatform.PairWithOppositePlatform(_oppositePlatform);
                _oppositePlatform.PairWithOppositePlatform(_ouboundPlatform);
            }
        }

        var sortedPlatforms = from _PLATFORM in platforms
                              orderby _PLATFORM.point_platform_START.index
                              select _PLATFORM;
        platforms = sortedPlatforms.ToList();
        for (int i = 0; i < platforms.Count; i++)
        {
            Platform _P = platforms[i];
            _P.platformIndex = i;
            _P.nextPlatform = platforms[(i + 1) % platforms.Count];
        }
        */
        speedRatio = bezierPath.GetPathDistance() * maxTrainSpeed;
        
        // Now, let's lay the rail meshes
        float _DIST = 0f;
        Metro _M = Metro.INSTANCE;
        var pos = new List<MetroLinePositionElement>();
        var normals = new List<MetroLineNormalElement>();
        var accelState = new List<MetroLineAccelerationStateElement>();

        bool isAccel = true;
        int pointIndex = 0;

        float pointDistance = isPlatformPosition[pointIndex].Value;
        bool atStation = isPlatformPosition[pointIndex].Key;

        while (_DIST < bezierPath.GetPathDistance())
        {
            float _DIST_AS_RAIL_FACTOR = Get_distanceAsRailProportion(_DIST);
            Vector3 _RAIL_POS = Get_PositionOnRail(_DIST_AS_RAIL_FACTOR);
            Vector3 _RAIL_ROT = Get_RotationOnRail(_DIST_AS_RAIL_FACTOR);

            if (_DIST >= pointDistance)
            {
                if (isAccel && atStation)
                {
                    isAccel = false;
                }
                else
                {
                    isAccel = true;
                }

                //Next target
                pointIndex = (pointIndex + 1) % isPlatformPosition.Count;
                atStation = isPlatformPosition[pointIndex].Key;
                pointDistance = isPlatformPosition[pointIndex].Value;
            }

            //convert
            pos.Add(new MetroLinePositionElement { Value = _RAIL_POS });
            normals.Add(new MetroLineNormalElement { Value = _RAIL_ROT });
            accelState.Add(new MetroLineAccelerationStateElement { Value = isAccel });

            //GameObject _RAIL = (GameObject) Metro.Instantiate(_M.prefab_rail);
            //            _RAIL.GetComponent<Renderer>().material.color = lineColour;
            //_RAIL.transform.position = _RAIL_POS;
            //_RAIL.transform.LookAt(_RAIL_POS - _RAIL_ROT);
            _DIST += Metro.RAIL_SPACING;
        }

        BakedPositionPath = new NativeArray<MetroLinePositionElement>(pos.ToArray(), Allocator.Persistent);
        BakedNormalPath = new NativeArray<MetroLineNormalElement>(normals.ToArray(), Allocator.Persistent);
        BakedAccelPath = new NativeArray<MetroLineAccelerationStateElement>(accelState.ToArray(), Allocator.Persistent);
    }


    Platform AddPlatform(int _index_platform_START, int _index_platform_END)
    {
        BezierPoint _PT_START = bezierPath.points[_index_platform_START];
        BezierPoint _PT_END = bezierPath.points[_index_platform_END];
        GameObject platform_OBJ =
            (GameObject) Metro.Instantiate(Metro.INSTANCE.prefab_platform, _PT_END.location, Quaternion.identity);
        Platform platform = platform_OBJ.GetComponent<Platform>();
        platform.SetupPlatform(this, _PT_START, _PT_END);
        platform_OBJ.transform.LookAt(bezierPath.GetPoint_PerpendicularOffset(_PT_END, -3f));
        platforms.Add(platform);
        return platform;
    }

    public void AddTrain(int _trainIndex, float _position)
    {
        //trains.Add(new Train(_trainIndex, metroLine_index, _position, carriagesPerTrain));
    }



    public Vector3 Get_PositionOnRail(float _pos)
    {
        return bezierPath.Get_Position(_pos);
    }

    public Vector3 Get_RotationOnRail(float _pos)
    {
        return bezierPath.Get_NormalAtPosition(_pos);
    }

    public float Get_distanceAsRailProportion(float _realDistance)
    {
        return _realDistance / bezierPath.GetPathDistance();
    }

    public float Get_proportionAsDistance(float _proportion)
    {
        return bezierPath.GetPathDistance() * _proportion;
    }

    public Entity Convert(Entity parentEntity, EntityManager dstManager,
            GameObject parentGO, GameObject prefabRail)
    {
        var entity = dstManager.CreateEntity();     
        var elemCount = BakedPositionPath.Length;

        var metroLinePositions = dstManager.AddBuffer<MetroLinePositionElement>(entity);
        metroLinePositions.CopyFrom(BakedPositionPath);
        var metroLineNormals = dstManager.AddBuffer<MetroLineNormalElement>(entity);
        metroLineNormals.CopyFrom(BakedNormalPath);
        var metroLineAccelMults = dstManager.AddBuffer<MetroLineAccelerationStateElement>(entity);
        metroLineAccelMults.CopyFrom(BakedAccelPath);

        var conversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var railTrackPartPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabRail, conversionSettings);

        for (int i = 0; i < elemCount; ++i)
        {
            var railTrackPartPrefabInstanceEntity = dstManager.Instantiate(railTrackPartPrefabEntity);

            dstManager.SetComponentData(railTrackPartPrefabInstanceEntity,
                new Translation { Value = BakedPositionPath[i] });

            dstManager.SetComponentData(railTrackPartPrefabEntity,
                new Rotation { Value = quaternion.LookRotation(BakedNormalPath[i], new float3(0.0f, 1.0f, 0.0f)) });
        }

        return parentEntity;
    }
}