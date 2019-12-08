namespace ecs
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [DisableAutoCreation]
    public class FindTargetSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(Hero))]
        [ExcludeComponent(typeof(HasTarget))]
        private struct TargetJob : IJobForEachWithEntity<Translation>
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> targetEntities;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Translation> targetTranslations;

            public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
            {
                var closestEntity = Entity.Null;
                var closestDistance = float.MaxValue;
                var heroPosition = translation.Value;
                var targetTranslationsCount = targetTranslations.Length;

                for (var i = 0; i < targetTranslationsCount; ++i)
                {
                    var targetEntity = targetEntities[i];
                    var targetPosition = targetTranslations[i].Value;

                    var distance = math.distance(heroPosition, targetPosition);

                    if (distance < closestDistance)
                    {
                        closestEntity = targetEntity;
                        closestDistance = distance;
                    }
                }

                if (closestEntity != Entity.Null)
                {
                    commandBuffer.AddComponent(index, entity,
                        new HasTarget {target = closestEntity});
                }
            }
        }

        private EntityQuery _targetGroup;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _targetGroup = GetEntityQuery(
                ComponentType.ReadOnly<Target>(),
                ComponentType.ReadOnly<Translation>());

            _endSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var targetJob = new TargetJob
            {
                targetEntities = _targetGroup.ToEntityArray(Allocator.TempJob),
                targetTranslations = _targetGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };

            var jobHandle = targetJob.Schedule(this, inputDeps);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}