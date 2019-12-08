namespace ecs
{
    public class HeroSystem2 {}

    /*public class HeroSystem2 : JobComponentSystem
    {
        private static int _jobIndex;

        [BurstCompile]
        public struct HeroJob : IJobChunk
        {
            public int jobIndex;

            public float deltaTime;

            public ArchetypeChunkComponentType<Hero> heroes;

            public ArchetypeChunkComponentType<Translation> translations;

            [DeallocateOnJobCompletion]
            public NativeArray<Entity> targetEntities;

            [DeallocateOnJobCompletion]
            public NativeArray<Translation> targetTranslations;

            public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var heroChunks = chunk.GetNativeArray(heroes);
                var translations = chunk.GetNativeArray(this.translations);

                var chunkCount = chunk.Count;
                var targetEntitiesCount = targetEntities.Length;

                for (var i = 0; i < chunkCount; ++i)
                {
                    var hero = heroChunks[i];
                    var translation = translations[i];

                    var targetIndex = targetEntities.IndexOf(hero.target);

                    if (targetIndex > -1)
                    {
                        var heroPosition = translation.Value;
                        var targetPosition = targetTranslations[targetIndex].Value;
                        var distance = math.distance(heroPosition, targetPosition);

                        if (distance > .01f)
                        {
                            var dir = math.normalize(targetPosition - heroPosition);
                            translations[i] = new Translation { Value = heroPosition + dir * deltaTime};
                        }
                        else
                        {
                            commandBuffer.DestroyEntity(jobIndex, hero.target);
                            heroChunks[i] = new Hero {target = Entity.Null};
//                            PostUpdateCommands.DestroyEntity(hero.target);
//                            entityManager.SetComponentData(heroEntity, new Hero {target = Entity.Null});
                        }
                    }
                    else
                    {
                        var closestEntity = Entity.Null;
                        var closestDistance = float.MaxValue;
                        var heroPosition = translation.Value;

                        for (var j = 0; j < targetEntitiesCount; ++j)
                        {
                            var targetEntity = targetEntities[j];

                            var targetPosition = targetTranslations[j].Value;

                            var distance = math.distance(heroPosition, targetPosition);

                            if (distance < closestDistance)
                            {
                                closestEntity = targetEntity;
                                closestDistance = distance;
                            }
                        }

                        if (closestEntity != Entity.Null)
                        {
                            heroChunks[i] = new Hero {target = closestEntity};
                        }
                    }
                }
            }
        }

        private EntityQuery _heroGroup;
        private EntityQuery _targetGroup;
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _endSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _heroGroup = GetEntityQuery(typeof(Hero), typeof(Translation));
            _targetGroup = GetEntityQuery(ComponentType.ReadOnly<Target>(),
                ComponentType.ReadOnly<Translation>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new HeroJob
            {
                jobIndex = ++_jobIndex,
                deltaTime = Time.deltaTime,
                heroes = GetArchetypeChunkComponentType<Hero>(),
                translations = GetArchetypeChunkComponentType<Translation>(),
                commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                targetEntities = _targetGroup.ToEntityArray(Allocator.TempJob),
                targetTranslations = _targetGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
            }.Schedule(_heroGroup, inputDeps);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handle);

            return handle;
        }
    }*/
}