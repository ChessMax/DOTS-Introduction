namespace ecs
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [DisableAutoCreation]
    public class BurstFindTargetSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Hero))]
        [ExcludeComponent(typeof(HasTarget))]
        private struct TargetJob : IJobForEachWithEntity<Translation>
        {
            [DeallocateOnJobCompletion] [ReadOnly]
            public NativeArray<Entity> targetEntities;

            [DeallocateOnJobCompletion] [ReadOnly]
            public NativeArray<Translation> targetTranslations;

            public NativeArray<Entity> closestTargets;

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
                    closestTargets[index] = closestEntity;
                }
            }
        }

        [RequireComponentTag(typeof(Hero))]
        [ExcludeComponent(typeof(HasTarget))]
        private struct AddTargetJob : IJobForEachWithEntity<Translation>
        {
            [DeallocateOnJobCompletion] [ReadOnly]
            public NativeArray<Entity> closestTargets;

            public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(Entity entity, int index, ref Translation translation)
            {
                var closestEntity = closestTargets[index];

                if (closestEntity != Entity.Null)
                    commandBuffer.AddComponent(index, entity, new HasTarget { target = closestEntity });
            }
        }

        private EntityQuery _targetGroup;
        private EntityQuery _noTargetHeroes;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _targetGroup = GetEntityQuery(
                ComponentType.ReadOnly<Target>(),
                ComponentType.ReadOnly<Translation>());

            _noTargetHeroes = GetEntityQuery(
                ComponentType.ReadOnly<Hero>(),
                ComponentType.Exclude<HasTarget>());

            _endSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var closestTargets = new NativeArray<Entity>(_noTargetHeroes.CalculateEntityCount(),
                Allocator.TempJob);
            var targetJob = new TargetJob
            {
                targetEntities = _targetGroup.ToEntityArray(Allocator.TempJob),
                targetTranslations = _targetGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                closestTargets = closestTargets
            };

            var addTargetJob = new AddTargetJob
            {
                closestTargets = closestTargets,
                commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };

            var jobHandle = targetJob.Schedule(this, inputDeps);
            var addTargetHandle = addTargetJob.Schedule(this, jobHandle);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(addTargetHandle);

            return addTargetHandle;
        }
    }
}