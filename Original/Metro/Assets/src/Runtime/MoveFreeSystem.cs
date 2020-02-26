using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class MoveFreeSystem : JobComponentSystem
{
    public EntityCommandBufferSystem m_endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[BurstCompile]
    [ExcludeComponent(typeof(SnapTag))]
    struct MoveFreeSystemJob : IJobForEachWithEntity<Translation, Rotation, SpeedManagementData, TargetData>
    {
        public float DeltaTime;
        public EntityCommandBuffer.Concurrent ECB;

        public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, ref SpeedManagementData speedManagementData, [ReadOnly] ref TargetData targetData)
        {
            speedManagementData.CurrentSpeed = speedManagementData.Acceleration * DeltaTime + speedManagementData.CurrentSpeed;
            speedManagementData.CurrentSpeed = min(speedManagementData.MaxSpeed, speedManagementData.CurrentSpeed);

            float3 vectorToTarget = targetData.Target - translation.Value;
            float distToTarget = math.length(targetData.Target - translation.Value);
            float3 direction = math.normalize(vectorToTarget);
            translation.Value = translation.Value + direction * DeltaTime * speedManagementData.CurrentSpeed;

            if (distToTarget < speedManagementData.CurrentSpeed * DeltaTime)
            {
                Entity entityCreated = ECB.CreateEntity(index);
                ECB.AddComponent(index, entityCreated, new TargetReached() { TargetEntity = entity });
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MoveFreeSystemJob()
        {
            DeltaTime = UnityEngine.Time.deltaTime,
            ECB = m_endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        };

        JobHandle handle = job.Schedule(this, inputDependencies);
        m_endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handle);

        // Now that the job is set up, schedule it to be run. 
        return handle;
    }
}