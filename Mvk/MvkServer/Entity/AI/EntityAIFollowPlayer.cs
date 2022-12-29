using MvkServer.Glm;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача следовать за своими
    /// </summary>
    public class EntityAIFollowPlayer : EntityAIBase
    {
        /// <summary>
        /// Тип сущностей своих
        /// </summary>
        private readonly EnumEntities typeEntity;
        /// <summary>
        /// Коэффицент скорости
        /// </summary>
        private readonly float speed;

        /// <summary>
        /// Сущность которую преследуем
        /// </summary>
        private EntityLiving entityLiving;

        /// <summary>
        /// Время в тиках на обновление перемещения
        /// </summary>
        private int timeUp;

        /// <summary>
        /// Задача следовать за своими
        /// </summary>
        public EntityAIFollowPlayer(EntityLiving entity, float speed = 1f)
        {
            this.entity = entity;
            this.speed = speed;
            typeEntity = entity.Type;
            SetMutexBits(3);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            this.entityLiving = null;
            if (entity.EntityAge > 100 || entity.World.Rnd.NextFloat() >= .7008f)
            {
                return false;
            }
            EntityBase entityBase = entity.World.FindNearestEntityWithinAABB(World.Chunk.ChunkBase.EnumEntityClassAABB.EntityPlayer,
                entity.BoundingBox.Expand(new vec3(16, 8, 16)), entity);
            if (entityBase != null && entityBase is EntityLiving entityLiving)
            {
                this.entityLiving = entityLiving;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => entityLiving != null ||  !entity.GetNavigator().NoPath();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting() => TryMoveToEntityLiving();

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask() => entityLiving = null;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (--timeUp <= 0) TryMoveToEntityLiving();
        }

        /// <summary>
        /// Попробуйте найти и указать путь к EntityLiving. Если найдём, запустить таймер
        /// </summary>
        private void TryMoveToEntityLiving()
        {
            if (entity.GetNavigator().TryMoveToEntityLiving(entityLiving, speed, true, true)) timeUp = 10;
        }
    }
}
