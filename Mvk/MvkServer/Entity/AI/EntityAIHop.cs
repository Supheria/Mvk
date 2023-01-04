namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача прыгать при движении
    /// </summary>
    public class EntityAIHop : EntityAIBase
    {
        /// <summary>
        /// Задача прыгать при движении
        /// </summary>
        public EntityAIHop(EntityLiving entity)
        {
            this.entity = entity;
            SetMutexBits(4);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => entity.Movement.Forward;

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => !entity.Movement.Forward;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (entity.Movement.Forward) 
            {
                entity.GetMoveHelper().SetJumping();
            }
        }
    }
}
