using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class LoopingTarget : JobComponentSystem
{
    public EntityCommandBufferSystem m_endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[BurstCompile]
    [RequireComponentTag(typeof(TargetReached))]
    struct LoopingTargetJob : IJobForEach<Translation, SpeedManagementData, TargetData, LoopingData>
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public BufferFromEntity<MetroLinePositionElement> MetroLine;

        public void Execute([ReadOnly] ref Translation translation,
            [ReadOnly] ref SpeedManagementData speed,
            ref TargetData target,
            ref LoopingData loopingData
            )
        {
            DynamicBuffer<MetroLinePositionElement> targetPositions = MetroLine[loopingData.RailEntity];
            
            float distance = 0f;
            
            while (distance < speed.CurrentSpeed * DeltaTime)
            {
                loopingData.PathIndex = (loopingData.PathIndex + 1) % targetPositions.Length;
                target.Target = targetPositions[loopingData.PathIndex];
                distance = math.distance(target.Target, translation.Value);
            }
            
        }

    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new LoopingTargetJob()
        {
            DeltaTime = UnityEngine.Time.deltaTime,
            MetroLine = GetBufferFromEntity<MetroLinePositionElement>()
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