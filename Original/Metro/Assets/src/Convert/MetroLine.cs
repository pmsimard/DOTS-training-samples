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

    public NativeArray<float3> BakedPositionPath;
    public NativeArray<float3> BakedNormalPath;
   
    public MetroLine(int metroLineIndex, int _maxTrains)
    {
        metroLine_index = metroLineIndex;
        maxTrains = _maxTrains;
        trains = new List<Train>();
        platforms = new List<Platform>();
    }

    public void Create_RailPath(List<RailMarker> _outboundPoints)
    {
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
        carriageLength_onRail = Get_distanceAsRailProportion(TrainCarriage.CARRIAGE_LENGTH) +
                                Get_distanceAsRailProportion(TrainCarriage.CARRIAGE_SPACING);

        

        speedRatio = bezierPath.GetPathDistance() * maxTrainSpeed;
        
        // Now, let's lay the rail meshes
        float _DIST = 0f;
        Metro _M = Metro.INSTANCE;
        List<float3> pos = new List<float3>();
        List<float3> normals = new List<float3>();

        while (_DIST < bezierPath.GetPathDistance())
        {
            float _DIST_AS_RAIL_FACTOR = Get_distanceAsRailProportion(_DIST);
            Vector3 _RAIL_POS = Get_PositionOnRail(_DIST_AS_RAIL_FACTOR);
            Vector3 _RAIL_ROT = Get_RotationOnRail(_DIST_AS_RAIL_FACTOR);

            //convert
            pos.Add(_RAIL_POS);
            normals.Add(_RAIL_ROT);

            //GameObject _RAIL = (GameObject) Metro.Instantiate(_M.prefab_rail);
            //            _RAIL.GetComponent<Renderer>().material.color = lineColour;
            //_RAIL.transform.position = _RAIL_POS;
            //_RAIL.transform.LookAt(_RAIL_POS - _RAIL_ROT);
            _DIST += Metro.RAIL_SPACING;
        }

        BakedPositionPath = new NativeArray<float3>(pos.ToArray(), Allocator.Persistent);
        BakedNormalPath = new NativeArray<float3>(normals.ToArray(), Allocator.Persistent);
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

    public Entity Convert(Entity parentEntity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var entity = dstManager.CreateEntity();
        if (dstManager.AddComponent<MetroLineComponentData>(entity))
        {
            var comp = dstManager.GetComponentData<MetroLineComponentData>(entity);
            comp.RailPositions = BakedPositionPath;
            comp.RailNormals = BakedNormalPath;
        }


        var railPrefabEntity = dstManager.GetComponentData<RailLinePrefab>(parentEntity).Value;
        for (int index = 0; index < BakedPositionPath.Length; ++index)
        {
            var railEntity = dstManager.Instantiate(railPrefabEntity);
            var trans = dstManager.GetComponentData<Translation>(railEntity);
            var rot = dstManager.GetComponentData<Rotation>(railEntity);

            trans.Value = BakedPositionPath[index];
            rot.Value = quaternion.LookRotation(BakedNormalPath[index], math.up());

            //GameObject _RAIL = (GameObject) Metro.Instantiate(_M.prefab_rail);
            //            _RAIL.GetComponent<Renderer>().material.color = lineColour;
            //_RAIL.transform.position = _RAIL_POS;
            //_RAIL.transform.LookAt(_RAIL_POS - _RAIL_ROT);
        }

        return entity;
    }
}