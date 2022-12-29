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
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float probability;
        /// <summary>
        /// Отдалённость, на которую идёт проверка группы
        /// </summary>
        private readonly int destination;

        /// <summary>
        /// Задача следовать за своими
        /// </summary>
        public EntityAIFollowYour(EntityLiving entity, float probability = .008f, float speed = 1f, int destination = 12)
        {
            this.entity = entity;
            this.speed = speed;
            this.probability = probability;
            this.destination = destination;
            SetMutexBits(3);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (entity.EntityAge > 100 || entity.World.Rnd.NextFloat() >= probability)
            {
                return false;
            }
            List<EntityBase> list = entity.World.GetEntitiesWithinAABB(entity.Type,
                entity.BoundingBox.Expand(new vec3(destination, 2, destination)), -1);

            if (list.Count == 0)
            {
                list = entity.World.GetEntitiesWithinAABB(entity.Type,
                entity.BoundingBox.Expand(new vec3(destination + 8, 4, destination + 8)), -1);
                if (list.Count == 0)
                {
                    list = entity.World.GetEntitiesWithinAABB(entity.Type,
                    entity.BoundingBox.Expand(new vec3(destination + 16, 8, destination + 16)), -1);
                }
            }

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
            => entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed, true, true, entity.World.Rnd.NextFloat() * 2f + 1f);

    }
}
