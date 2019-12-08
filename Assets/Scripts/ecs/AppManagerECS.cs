namespace ecs
{
    using System;
    using System.Collections;
    using Unity.Entities;
    using Unity.Rendering;
    using Unity.Transforms;
    using UnityEngine;

    public class AppManagerECS : MonoBehaviour
    {
        public Camera mainCamera;
        public Mesh spriteMesh;
        public Material heroMaterial;
        public Material targetMaterial;

        public int numHeroes = 500;
        public int numTargets = 5000;

        public SystemType systemType;

        private void Awake()
        {
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            for (var i = 0; i < 10; ++i)
                yield return null;

            var world = World.Active;
            var entityManager = world.EntityManager;

            var heroArchetype = entityManager.CreateArchetype(
                typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation), typeof(Hero)
            );

            var targetArchetype = entityManager.CreateArchetype(
                typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation), typeof(Target)
            );

            for (var i = 0; i < numHeroes; ++i)
            {
                var heroEntity = entityManager.CreateEntity(heroArchetype);

                entityManager.SetComponentData(heroEntity, new Translation {
                    Value = mainCamera.GetRandomPosition()
                });
                entityManager.SetSharedComponentData(heroEntity, new RenderMesh {
                    mesh = spriteMesh,
                    material = heroMaterial
                });
            }

            for (var i = 0; i < numTargets; ++i)
            {
                var targetEntity = entityManager.CreateEntity(targetArchetype);

                entityManager.SetComponentData(targetEntity, new Translation {
                    Value = mainCamera.GetRandomPosition()
                });
                entityManager.SetSharedComponentData(targetEntity, new RenderMesh {
                    mesh = spriteMesh,
                    material = targetMaterial
                });
            }

            for (var i = 0; i < 10; ++i)
                yield return null;

            switch (systemType)
            {
                case SystemType.Simple:
                    CreateSimulationSystem<HeroSystem>(world);
                    break;

                case SystemType.Job:
                    CreateSimulationSystem<MoveSystem>(world);
                    CreateSimulationSystem<FindTargetSystem>(world);
                    break;

                case SystemType.Burst:
                    CreateSimulationSystem<MoveSystem>(world);
                    CreateSimulationSystem<BurstFindTargetSystem>(world);
                    break;
                case SystemType.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void CreateSimulationSystem<T>(World world) where T : ComponentSystemBase
        {
            var simulationSystemGroup = world.GetOrCreateSystem<SimulationSystemGroup>();
            var system = world.GetOrCreateSystem<T>();

            simulationSystemGroup.AddSystemToUpdateList(system);
            simulationSystemGroup.SortSystemUpdateList();
        }
    }

    public enum SystemType
    {
        Simple,

        Job,

        Burst,

        None
    }
}