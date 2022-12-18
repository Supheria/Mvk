using MvkServer.Entity.AI.PathFinding;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача всплывать если в воде
    /// </summary>
    public class EntityAISwimming : EntityAIBase
    {
        /// <summary>
        /// Задача всплывать если в воде
        /// </summary>
        public EntityAISwimming(EntityLiving entity)
        {
            this.entity = entity;
            SetMutexBits(4);
            ((PathNavigateGround)entity.GetNavigator()).CanSwim(true);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => entity.IsInWater() || entity.IsInLava() || entity.IsInOil();

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (entity.World.Rnd.NextFloat() < .8f)
            {
                entity.GetMoveHelper().SetJumping();
            }
        }
    }
}
