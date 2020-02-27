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
        [ReadOnly] public DynamicBuffer<MetroLinePositionElement> RailPositions0;
        [ReadOnly] public DynamicBuffer<MetroLinePositionElement> RailPositions1;
        [ReadOnly] public DynamicBuffer<MetroLinePositionElement> RailPositions2;
        [ReadOnly] public DynamicBuffer<MetroLinePositionElement> RailPositions3;

        [ReadOnly] public float DeltaTime;

        public void Execute([ReadOnly] ref Translation translation,
            [ReadOnly] ref SpeedManagementData speed,
            ref TargetData target,
            ref LoopingData loopingData
            )
        {
            DynamicBuffer<MetroLinePositionElement> targetPositions = RailPositions0;
            switch(loopingData.RailIndex)
            {
                case 0:
                    targetPositions = RailPositions0;
                    break;
                case 1:
                    targetPositions = RailPositions1;
                    break;
                case 2:
                    targetPositions = RailPositions2;
                    break;
                case 3:
                    targetPositions = RailPositions3;
                    break;
            }

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
            DeltaTime = UnityEngine.Time.deltaTime
        };

        /*
        Entities.ForEach((Entity entity, ref MetroLine metroLine) =>
        {
            DynamicBuffer<MetroLinePositionElement> buff = EntityManager.GetBuffer<MetroLinePositionElement>(entity);
            
            switch (metroLine.RailIndex)
            {
                case 0:
                    job.RailPositions0 = metroLine.RailPositions;
                    break;
                case 1:
                    job.RailPositions1 = metroLine.RailPositions;
                    break;
                case 2:
                    job.RailPositions2 = metroLine.RailPositions;
                    break;
                case 3:
                    job.RailPositions3 = metroLine.RailPositions;
                    break;
            }
         });*/

        JobHandle jobHandle = job.Schedule(this, inputDependencies);

        jobHandle.Complete();

        // Needs to happen after the job
        EntityQuery targetReachedQuery = GetEntityQuery(typeof(TargetReached));
        EntityManager.RemoveComponent<TargetReached>(targetReachedQuery);

        // Now that the job is set up, schedule it to be run. 
        return jobHandle;
    }
}