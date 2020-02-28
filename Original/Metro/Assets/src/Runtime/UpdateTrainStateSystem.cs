using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class UpdateTrainStateSystem : JobComponentSystem
{
    //[BurstCompile]
    struct LoopingTargetJob : IJobForEach<TrainComponentData, LoopingData, SpeedManagementData>
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public BufferFromEntity<MetroLineAccelerationStateElement> MetroLinesAccelStateBuffers;

        public void Execute(ref TrainComponentData trainData,
        [ReadOnly] ref LoopingData loopingData,
        [ReadOnly] ref SpeedManagementData speedData)
        {
            var dt = DeltaTime;
            var accelStateIndex = loopingData.PathIndex;
            var accelStatesBuffer = MetroLinesAccelStateBuffers[trainData.RailEntity];
            var isAccellerating = accelStatesBuffer[accelStateIndex] == 1.0f;

            switch (trainData.State)
            {
                case TrainState.InTransit:
                    {
                        bool isAtMaxSpeed = speedData.CurrentSpeed == speedData.MaxSpeed;

                        if (isAtMaxSpeed && !isAccellerating)
                            trainData.State = TrainState.Arriving;
                        else if (isAtMaxSpeed && isAccellerating)
                            trainData.State = TrainState.Departing;
                    }
                    break;
                case TrainState.Arriving:
                    // If we're arriving (decellarating) and we need to acccellarate
                    // it means we need to stop first
                    if (isAccellerating)
                        trainData.State = TrainState.Stopped;
                    break;
                case TrainState.Stopped:
                    // Delay train arrival
                    trainData.WaitTimer -= dt;
                    if (trainData.WaitTimer <= 0.0f)
                    {
                        trainData.WaitTimer = TrainComponentData.WaitTimerInitialValue;
                        trainData.State = TrainState.InTransit;
                        //speedData.CurrentSpeed = 0;
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
                        trainData.State = TrainState.Departing;
                    }
                    break;
                case TrainState.DoorsClosed:
                    // Delay train departure
                    trainData.WaitTimer -= dt;
                    if (trainData.WaitTimer <= 0.0f)
                    {
                        trainData.WaitTimer = TrainComponentData.WaitTimerInitialValue;
                        trainData.State = TrainState.InTransit;
                    }
                    break;
                default:
                    Debug.LogError($"Unknown train state: {trainData.State}");
                    break;
            }
        }

    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new LoopingTargetJob()
        {
            DeltaTime = UnityEngine.Time.deltaTime,
            MetroLinesAccelStateBuffers = GetBufferFromEntity<MetroLineAccelerationStateElement>()
        };

        JobHandle jobHandle = job.Schedule(this, inputDependencies);
        jobHandle.Complete();

        // Needs to happen after the job
        EntityQuery targetReachedQuery = GetEntityQuery(typeof(TargetReached));
        EntityManager.RemoveComponent<TargetReached>(targetReachedQuery);

        // Now that the job is set up, schedule it to be run. 
        return jobHandle;

    }
}