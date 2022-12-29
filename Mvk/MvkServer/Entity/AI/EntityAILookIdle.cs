using MvkServer.Glm;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача смотреть без дела
    /// </summary>
    public class EntityAILookIdle : EntityAIBase
    {
        private int time;
        private float lookX;
        private float lookZ;

        /// <summary>
        /// Задача смотреть без дела
        /// </summary>
        public EntityAILookIdle(EntityLiving entity)
        {
            this.entity = entity;
            SetMutexBits(2);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => entity.World.Rnd.NextFloat() < .02f;

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => time >= 0;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            float angle = glm.pi360 * entity.World.Rnd.NextFloat();
            lookX = glm.cos(angle);
            lookZ = glm.sin(angle);
            time = 20 + entity.World.Rnd.Next(20);
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            time--;
            vec3 pos = entity.Position;
            pos.x += lookX;
            pos.y += entity.GetEyeHeight();
            pos.z += lookZ;
            entity.GetLookHelper().SetLookPosition(pos, glm.pi10, glm.pi10);
        }
    }
}
