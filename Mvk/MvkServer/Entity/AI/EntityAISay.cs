namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача сказать
    /// </summary>
    public class EntityAISay : EntityAIBase
    {
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float probability;

        /// <summary>
        /// Задача сказать
        /// </summary>
        public EntityAISay(EntityLiving entity, float probability = .02f)
        {
            this.entity = entity;
            this.probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => entity.World.Rnd.NextFloat() < probability;

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => false;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            entity.PlaySoundSay();
        }
    }
}
