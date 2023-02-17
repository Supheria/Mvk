using MvkServer.Util;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача бродить в воздухе
    /// </summary>
    public class EntityAIWanderFly : EntityAIBase
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
        /// Задача бродить в воздухе
        /// </summary>
        public EntityAIWanderFly(EntityLiving entity, float probability = .008f, float speed = 1f)
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
            
            xPosition = entity.Position.x;
            yPosition = entity.Position.y;
            zPosition = entity.Position.z;

            int xz = 10;
            int y = 7;
            xPosition += entity.World.Rnd.Next(xz) - entity.World.Rnd.Next(xz);
            int posY = entity.World.Rnd.Next(y) - entity.World.Rnd.Next(y);
            zPosition += entity.World.Rnd.Next(xz) - entity.World.Rnd.Next(xz);

            int count = 10;
            while(count > 0 && !entity.World.GetBlockState(new BlockPos(xPosition, yPosition + posY, zPosition)).IsAir())
            {
                posY++;
                count--;
            }
            if (count == 10)
            {
                while (count > 0 && entity.World.GetBlockState(new BlockPos(xPosition, yPosition + posY, zPosition)).IsAir())
                {
                    posY--;
                    count--;
                }
            }
            // 3 это примерно над такой высотой должен держатся, но по факту 1-2 блока
            yPosition = yPosition + posY + 3;

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
