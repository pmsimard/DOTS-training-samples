﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class UpdateTrainStateSystem : JobComponentSystem
{
    //[BurstCompile]
    struct UpdateTrainStateJob : IJobForEach<TrainComponentData, LoopingData, SpeedManagementData>
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public BufferFromEntity<MetroLineAccelerationStateElement> MetroLinesAccelStateBuffers;

        public void Execute(
                   ref TrainComponentData trainData,
        [ReadOnly] ref LoopingData loopingData,
        [ReadOnly] ref SpeedManagementData speedData)
        {
            var dt = DeltaTime;
            var prevAccelStateIndex = loopingData.PreviousPathIndex;
            var accelStateIndex = loopingData.PathIndex;
            var accelStatesBuffer = MetroLinesAccelStateBuffers[trainData.RailEntity];

            bool wasAccellerating = accelStatesBuffer[prevAccelStateIndex] == 1.0f;
            bool isDescellerating = accelStatesBuffer[accelStateIndex] == -1.0f;


            switch (trainData.State)
            {
                case TrainState.InTransit:
                    {
                        if (wasAccellerating && isDescellerating)
                        {
                            Debug.Log("========== Going to Arriving");
                            trainData.State = TrainState.Arriving;
                        }
                    }
                    break;
                case TrainState.Arriving:
                    // If we're arriving (descellarating) and we need to accellarate
                    // it means we need to stop first
                    if (!wasAccellerating && !isDescellerating)
                    {
                        Debug.Log("========== Going to a Stop");
                        speedData.CurrentSpeed = 0.0f;
                        speedData.MaxSpeed = 0.0f;
                        trainData.State = TrainState.Stopped;
                    }
                    break;
                case TrainState.Stopped:
                    // Delay train arrival
                    trainData.WaitTimer -= dt;
                    if (trainData.WaitTimer <= 0.0f)
                    {
                        trainData.WaitTimer = TrainComponentData.WaitTimerInitialValue;
                        trainData.State = TrainState.OpeningDoors;
                    }
                    break;
                case TrainState.OpeningDoors:
                    // Wait for door "moving" timer to elapse
                    trainData.DoorMoveTimer -= dt;
                    if (trainData.DoorMoveTimer <= 0.0f)
                    {
                        trainData.DoorMoveTimer = TrainComponentData.DoorMoveTimerInitialValue;
                        trainData.State = TrainState.DoorsOpened;
                    }
                    break;
                case TrainState.DoorsOpened:
                    // Wait for door timer to elapse
                    trainData.DoorOpenedTimer -= dt;
                    if (trainData.DoorOpenedTimer <= 0.0f)
                    {
                        trainData.DoorOpenedTimer = TrainComponentData.DoorOpenedTimerInitialValue;
                        trainData.State = TrainState.ClosingDoors;
                    }
                    break;
                case TrainState.ClosingDoors:
                    // Wait for door "moving" timer to elapse
                    trainData.DoorMoveTimer -= dt;
                    if (trainData.DoorMoveTimer <= 0.0f)
                    {
                        trainData.DoorMoveTimer = TrainComponentData.DoorMoveTimerInitialValue;
                        trainData.State = TrainState.DoorsClosed;
                    }
                    break;
                case TrainState.DoorsClosed:
                    // Delay train departure
                    trainData.WaitTimer -= dt;
                    if (trainData.WaitTimer <= 0.0f)
                    {
                        Debug.Log($"========== Train is Departing with accellaration {speedData.Acceleration}");
                        trainData.WaitTimer = TrainComponentData.WaitTimerInitialValue;
                        speedData.MaxSpeed = SpeedManagementData.DefaultMaxSpeed;
                        trainData.State = TrainState.InTransit;
                    }
                    break;
                default:
                    Debug.LogError($"Unknown train state: {trainData.State}");
                    break;
            }
        }
    }

    struct UpdateEntireTrainSpeedJob : IJobForEach<TrainComponentData, LoopingData, SpeedManagementData>
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new UpdateTrainStateJob()
        {
            DeltaTime = UnityEngine.Time.deltaTime,
            MetroLinesAccelStateBuffers = GetBufferFromEntity<MetroLineAccelerationStateElement>()
        };

        JobHandle jobHandle = job.Schedule(this, inputDependencies);
        jobHandle.Complete();

        var allTrainsSpeedData = GetComponentDataFromEntity<SpeedManagementData>(isReadOnly: true);

        Entities
            .WithNone<TrainComponentData>()
            .WithReadOnly(allTrainsSpeedData)
            .ForEach((ref WagonComponentData wagonData, ref SpeedManagementData speedData) => {
                var trainSpeedData = allTrainsSpeedData[wagonData.TrainEntity];
                speedData.CurrentSpeed = trainSpeedData.CurrentSpeed;
                speedData.MaxSpeed = trainSpeedData.MaxSpeed;
            });

        // Needs to happen after the job
        EntityQuery targetReachedQuery = GetEntityQuery(typeof(TargetReached));
        EntityManager.RemoveComponent<TargetReached>(targetReachedQuery);

        // Now that the job is set up, schedule it to be run. 
        return jobHandle;

    }
}