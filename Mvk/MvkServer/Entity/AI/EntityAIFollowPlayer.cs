using MvkServer.Entity.List;
using MvkServer.Glm;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача следовать за игроком
    /// </summary>
    public class EntityAIFollowPlayer : EntityAIBase
    {
        /// <summary>
        /// Тип сущностей
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
        /// Задать остановку при соприкосновении коллизии, true - соприкосновении коллизии, false - центр
        /// </summary>
        private readonly bool stopOnOverlap;
        /// <summary>
        /// Задача следовать за своими
        /// </summary>
        public EntityAIFollowPlayer(EntityLiving entity, float speed = 1f, bool stopOnOverlap = true)
        {
            this.entity = entity;
            this.speed = speed;
            this.stopOnOverlap = stopOnOverlap;
            typeEntity = entity.Type;
            SetMutexBits(3);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (entity.EntityAge > 100 || entity.World.Rnd.NextFloat() >= .7008f)
            {
                return false;
            }
            return CheckEntity();
        }

        private bool CheckEntity()
        {
            EntityBase entityBase = entity.World.FindNearestEntityWithinAABB(World.Chunk.ChunkBase.EnumEntityClassAABB.EntityPlayer,
                entity.BoundingBox.Expand(new vec3(32, 12, 32)), entity);
            if (entityBase != null && entityBase is EntityLiving entityLiving)
            {
                if (entityLiving is EntityPlayer entityPlayer && entityPlayer.IsCreativeMode)
                {
                    this.entityLiving = null;
                    entity.SetAttackTarget(null);
                    return false;
                }
                this.entityLiving = entityLiving;
                entity.SetAttackTarget(entityLiving);
                return true;
            }
            this.entityLiving = null;
            entity.SetAttackTarget(null);
            return false;
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => CheckEntity();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting() => TryMoveToEntityLiving();

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask()
        {
            entityLiving = null;
            entity.SetAttackTarget(null);
        }

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
            if (entity.GetNavigator().TryMoveToEntityLiving(entityLiving, speed, true, stopOnOverlap))
            {
                timeUp = 10;
            }
        }
    }
}
