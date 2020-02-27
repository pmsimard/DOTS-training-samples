using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// ReSharper disable once InconsistentNaming
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class CommuterSpawner : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //Instead of performing structural changes directly, a Job can add a command to an EntityCommandBuffer to perform such changes on the main thread after the Job has finished.
        //Command buffers allow you to perform any, potentially costly, calculations on a worker thread, while queuing up the actual insertions and deletions for later.
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        // Schedule the Entities.ForEach lambda job that will add Instantiate commands to the EntityCommandBuffer.
        // Since this job only runs on the first frame, we want to ensure Burst compiles it before running to get the best performance (3rd parameter of WithBurst)
        // The actual job will be cached once it is compiled (it will only get Burst compiled once).
        JobHandle handle = Entities.ForEach((Entity entity, int entityInQueryIndex, ref TrainCarriageSpawnerData spawnerFromEntity, ref LocalToWorld location) =>
        {


            /*for (int i = 0; i < spawnerFromEntity.WagonPerLine; i++)
            {
                Entity instance = commandBuffer.Instantiate(entityInQueryIndex, spawnerFromEntity.Prefab);

                // Place the instantiated in a grid with some noise
                commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = new float3(0, 0, 0) });
                commandBuffer.AddComponent(entityInQueryIndex, instance, new WagonComponentData { Index = 0 });
            }
            commandBuffer.DestroyEntity(entityInQueryIndex, entity);*/

        }).WithBurst().Schedule(inputDeps);

        // SpawnJob runs in parallel with no sync point until the barrier system executes.
        // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
        m_EntityCommandBufferSystem.AddJobHandleForProducer(handle);

        return handle;
    }

}
