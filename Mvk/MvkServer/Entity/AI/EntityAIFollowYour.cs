using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача следовать за своими
    /// </summary>
    public class EntityAIFollowYour : EntityAIBase
    {
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        private float xPosition;
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        private float yPosition;
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        private float zPosition;
        /// <summary>
        /// Коэффицент скорости
        /// </summary>
        private readonly float speed;

        /// <summary>
        /// Задача следовать за своими
        /// </summary>
        public EntityAIFollowYour(EntityLiving entity, float speed = 1f)
        {
            this.entity = entity;
            this.speed = speed;
            SetMutexBits(3);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (entity.EntityAge > 100 || entity.World.Rnd.NextFloat() >= .008f)
            {
                return false;
            }
            List<EntityBase> list = entity.World.GetEntitiesWithinAABB(entity.Type,
                entity.BoundingBox.Expand(new vec3(16, 8, 16)), -1);

            int count = list.Count;

            if (count == 0) return false;
            
            vec3 pos = new vec3(0);
            for (int i = 0; i < count; i++)
            {
                pos += list[i].Position;
            }
            xPosition = pos.x / count;
            yPosition = pos.y / count;
            zPosition = pos.z / count;
            return true;
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => !entity.GetNavigator().NoPath();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            entity.GetNavigator().SetAcceptanceRadius(2f);
            entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed);
        }

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask() => entity.GetNavigator().ClearPathEntity();
    }
}
