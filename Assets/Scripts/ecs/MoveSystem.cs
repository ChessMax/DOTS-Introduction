namespace ecs
{
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [DisableAutoCreation]
    public class MoveSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(Hero))]
        private struct MoveJob : IJobForEachWithEntity<HasTarget, Translation>
        {
            public float deltaTime;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> targets;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Translation> targetsTranslations;

            public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(Entity entity, int index, ref HasTarget hasTarget, ref Translation translation)
            {
                var targetIndex = targets.IndexOf(hasTarget.target);

                if (targetIndex != -1)
                {
                    var heroPosition = translation.Value;
                    var targetPosition = targetsTranslations[targetIndex].Value;
                    var distance = math.distance(heroPosition, targetPosition);

                    if (distance > .01f)
                    {
                        var dir = math.normalize(targetPosition - heroPosition);
                        translation = new Translation { Value = heroPosition + dir * deltaTime};
                    }
                    else
                    {
                        commandBuffer.DestroyEntity(index, hasTarget.target);
                        commandBuffer.RemoveComponent<HasTarget>(index, entity);
                    }
                }
                else
                {
                    commandBuffer.RemoveComponent<HasTarget>(index, entity);
                }
            }
        }

        private EntityQuery _targets;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _targets = GetEntityQuery(ComponentType.ReadOnly<Target>(),
                ComponentType.ReadOnly<Translation>());

            _endSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var moveJob = new MoveJob
            {
                deltaTime = Time.deltaTime,
                commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                targets = _targets.ToEntityArray(Allocator.TempJob),
                targetsTranslations = _targets.ToComponentDataArray<Translation>(Allocator.TempJob)
            };

            var jobHandle = moveJob.Schedule(this, inputDeps);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}