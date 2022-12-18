namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача паника
    /// </summary>
    public class EntityAIPanic : EntityAIBase
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
        /// Задача паника
        /// </summary>
        public EntityAIPanic(EntityLiving entity, float speed = 1f)
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
            if (!entity.InFire() && entity.GetAITarget() == null) return false;
            
            int xz = 7;
            int y = 4;
            xPosition = entity.Position.x + entity.World.Rnd.Next(xz) - entity.World.Rnd.Next(xz);
            yPosition = entity.Position.y + entity.World.Rnd.Next(y) - entity.World.Rnd.Next(y);
            zPosition = entity.Position.z + entity.World.Rnd.Next(xz) - entity.World.Rnd.Next(xz);

            return true;
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (entity.World.Rnd.NextFloat() < .4f)
            {
                entity.GetMoveHelper().SetSprinting();
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => !entity.GetNavigator().NoPath();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting() => entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed);

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask() => entity.GetNavigator().ClearPathEntity();
    }
}
