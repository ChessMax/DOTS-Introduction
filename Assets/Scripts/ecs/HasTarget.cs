namespace ecs
{
    using Unity.Entities;

    public struct HasTarget : IComponentData
    {
        public Entity target;
    }
}