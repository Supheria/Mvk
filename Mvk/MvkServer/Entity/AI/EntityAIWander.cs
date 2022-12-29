namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача бродить
    /// </summary>
    public class EntityAIWander : EntityAIBase
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
        /// Параметр мгновенного выполнения этой задачи
        /// </summary>
        private bool instantExecution;

        /// <summary>
        /// Задача бродить
        /// </summary>
        public EntityAIWander(EntityLiving entity, float probability = .008f, float speed = 1f)
        {
            this.entity = entity;
            this.speed = speed;
            this.probability = probability;
            SetMutexBits(3);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (!instantExecution)
            {
                // Сущность далековато от игрока и шанс
                if (entity.EntityAge > 100 || entity.World.Rnd.NextFloat() >= probability)
                {
                    return false;
                }
            }
            /*
            EntityBase entityBase = entity.World.FindNearestEntityWithinAABB(World.Chunk.ChunkBase.EnumEntityClassAABB.EntityPlayer,
                entity.BoundingBox.Expand(new vec3(16, 12, 16)), entity);
            if (entityBase != null && entityBase is EntityPlayer entityPlayer)
            {
                xPosition = entityPlayer.Position.x;
                yPosition = entityPlayer.Position.y + entityPlayer.GetEyeHeight() / 2f;
                zPosition = entityPlayer.Position.z;
                return true;
                //return entity.GetNavigator().TryMoveToEntityLiving(entityPlayer, speed);
            }
            return false;
            */
            
            xPosition = entity.Position.x;
            yPosition = entity.Position.y;
            zPosition = entity.Position.z;
            //xPosition -= 7; // entity.Position.x + entity.World.Rnd.Next(5) - entity.World.Rnd.Next(5);
            //yPosition += 5;
            //zPosition -= 7;// entity.World.Rnd.Next(5) - entity.World.Rnd.Next(5);

            int xz = 10;
            int y = 7;
            xPosition += entity.World.Rnd.Next(xz) - entity.World.Rnd.Next(xz);
            yPosition += entity.World.Rnd.Next(y) - entity.World.Rnd.Next(y);
            zPosition += entity.World.Rnd.Next(xz) - entity.World.Rnd.Next(xz);

            instantExecution = false;
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
            => entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed, true, true);

        /// <summary>
        /// Мгновенное выполнение
        /// </summary>
        public void InstantExecution() => instantExecution = true;
    }
}
