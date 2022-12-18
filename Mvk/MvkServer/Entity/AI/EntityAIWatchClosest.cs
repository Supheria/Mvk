using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.World.Chunk;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача смотреть на ближайшего игрока
    /// </summary>
    public class EntityAIWatchClosest : EntityAIBase
    {
        /// <summary>
        /// Сущность на которую смотрит
        /// </summary>
        private EntityPlayer closestEntity;
        /// <summary>
        /// Это максимальное расстояние, на котором ИИ будет искать сущность
        /// </summary>
        private readonly float maxDistanceForPlayer;

        /// <summary>
        /// Задача смотреть на ближайшего игрока
        /// </summary>
        public EntityAIWatchClosest(EntityLiving entity, float distance)
        {
            this.entity = entity;
            maxDistanceForPlayer = distance;
            SetMutexBits(2);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (entity.World.Rnd.NextFloat() >= .02f) // Шанс 2%
            {
                return false;
            }

            EntityBase entityBase = entity.World.FindNearestEntityWithinAABB(
                ChunkBase.EnumEntityClassAABB.EntityPlayer,
                entity.BoundingBox.Expand(new vec3(maxDistanceForPlayer, 5f, maxDistanceForPlayer)),
                entity);
            if (entityBase != null && entityBase is EntityPlayer entityPlayer)
            {
                closestEntity = entityPlayer;
            }
            else
            {
                closestEntity = null;
            }
            return closestEntity != null;
        }

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask() => closestEntity = null;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            entity.GetLookHelper().SetLookPositionWithEntity(closestEntity, glm.pi10, glm.pi10);
        }
    }
}
