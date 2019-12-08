namespace ecs
{
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [DisableAutoCreation]
    public class HeroSystem : ComponentSystem
    {
        private EntityQuery _heroQuery;
        private EntityQuery _targetQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            _heroQuery = GetEntityQuery(
                ComponentType.ReadOnly<Hero>(),
                typeof(Translation));
            _targetQuery = GetEntityQuery(
                ComponentType.ReadOnly<Target>(),
                ComponentType.ReadOnly<Translation>());

            RequireForUpdate(_heroQuery);
            RequireForUpdate(_targetQuery);
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            var entityManager = World.Active.EntityManager;

            using (var heroEntities = _heroQuery.ToEntityArray(Allocator.TempJob))
            {
                using (var targetEntities = _targetQuery.ToEntityArray(Allocator.TempJob))
                {
                    var numHeroEntities = heroEntities.Length;
                    var numTargetEntities = targetEntities.Length;

                    for (var j = 0; j < numHeroEntities; ++j)
                    {
                        var heroEntity = heroEntities[j];
                        var heroPosition = entityManager.GetComponentData<Translation>(heroEntity).Value;
                        var target = entityManager.HasComponent<HasTarget>(heroEntity) ?
                            entityManager.GetComponentData<HasTarget>(heroEntity).target :
                            Entity.Null;

                        if (!entityManager.Exists(target))
                        {
                            var closestTarget = Entity.Null;
                            var closestTargetDistance = float.MaxValue;

                            for (var i = 0; i < numTargetEntities; ++i)
                            {
                                var targetEntity = targetEntities[i];
                                var targetPosition = entityManager.GetComponentData<Translation>(targetEntity).Value;
                                var distance2Target = math.distance(heroPosition, targetPosition);

                                if (distance2Target < closestTargetDistance)
                                {
                                    closestTargetDistance = distance2Target;
                                    closestTarget = targetEntity;
                                }
                            }

                            if (closestTarget != Entity.Null)
                            {
                                var hasTarget = new HasTarget {
                                    target = closestTarget
                                };
                                if (entityManager.HasComponent<HasTarget>(heroEntity))
                                    entityManager.SetComponentData(heroEntity, hasTarget);
                                else
                                    entityManager.AddComponentData(heroEntity, hasTarget);
                            }
                        }

                        if (entityManager.Exists(target))
                        {
                            var targetPosition = entityManager.GetComponentData<Translation>(target).Value;
                            var distance = math.distance(heroPosition, targetPosition);

                            if (distance > .01f)
                            {
                                var dir = math.normalize(targetPosition - heroPosition);

                                entityManager.SetComponentData(heroEntity, new Translation
                                {
                                    Value = heroPosition + dir * deltaTime
                                });
                            }
                            else
                            {
                                PostUpdateCommands.DestroyEntity(target);
                                PostUpdateCommands.RemoveComponent<HasTarget>(heroEntity);
                            }
                        }
                    }
                }
            }
        }
    }
}