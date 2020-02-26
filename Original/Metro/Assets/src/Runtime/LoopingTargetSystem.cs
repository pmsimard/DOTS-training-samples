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
        [ReadOnly] public NativeArray<float3> RailPositions0;
        [ReadOnly] public NativeArray<float3> RailPositions1;
        [ReadOnly] public NativeArray<float3> RailPositions2;
        [ReadOnly] public NativeArray<float3> RailPositions3;

        [ReadOnly] public float DeltaTime;

        public void Execute([ReadOnly] ref Translation translation,
            [ReadOnly] ref SpeedManagementData speed,
            ref TargetData target,
            ref LoopingData loopingData
            )
        {
            NativeArray<float3> targetPositions = RailPositions0;
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
        int nbPoint = 1000;
        float radius = 20f;
        float3[] points = new float3[nbPoint];
        for (int i = 0; i < nbPoint; i++)
        {
            points[i].x = math.sin(2f * math.PI * i / nbPoint) * radius;
            points[i].y = 0;
            points[i].z = math.cos(2f * math.PI * i / nbPoint) * radius;
        }

        var job = new LoopingTargetJob()
        {
            RailPositions0 = new NativeArray<float3>(points, Allocator.TempJob),
            RailPositions1 = new NativeArray<float3>(points, Allocator.TempJob),
            RailPositions2 = new NativeArray<float3>(points, Allocator.TempJob),
            RailPositions3 = new NativeArray<float3>(points, Allocator.TempJob),
            DeltaTime = UnityEngine.Time.deltaTime
        };
        
        JobHandle jobHandle = job.Schedule(this, inputDependencies);

        jobHandle.Complete();

        job.RailPositions0.Dispose();
        job.RailPositions1.Dispose();
        job.RailPositions2.Dispose();
        job.RailPositions3.Dispose();

        // Needs to happen after the job
        EntityQuery targetReachedQuery = GetEntityQuery(typeof(TargetReached));
        EntityManager.RemoveComponent<TargetReached>(targetReachedQuery);

        // Now that the job is set up, schedule it to be run. 
        return jobHandle;
    }
}