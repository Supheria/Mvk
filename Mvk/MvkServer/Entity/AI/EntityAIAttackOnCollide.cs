using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача атаковать в близи
    /// </summary>
    public class EntityAIAttackOnCollide : EntityAIBase
    {
        /// <summary>
        /// Тикер интервала между атаками
        /// </summary>
        private int attackTick = 0;
        /// <summary>
        /// Урон
        /// </summary>
        private readonly float damage;

        /// <summary>
        /// Задача атаковать в близи
        /// </summary>
        public EntityAIAttackOnCollide(EntityLiving entity, float damage)
        {
            this.entity = entity;
            this.damage = damage;
            SetMutexBits(0);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => entity.GetAttackTarget() != null;

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => entity.GetAttackTarget() != null;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting() => attackTick = 0;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            EntityLiving entityLiving = entity.GetAttackTarget();
            if (entityLiving != null)
            {
                if (attackTick > 0)
                {
                    attackTick--;
                }
                else
                {
                    AxisAlignedBB aabb = entity.BoundingBox.Expand(new vec3(.5f));
                    if (aabb.IntersectsWith(entityLiving.BoundingBox))
                    //float distantion = entity.GetDistanceSq(entityLiving.Position.x, entityLiving.Position.y, entityLiving.Position.z);
                    //float width = entity.Width * entity.Width * 8f + entityLiving.Width * 2f;
                    //if (distantion <= width)
                    {
                        attackTick = 20;
                        entity.SwingItem();
                        entityLiving.AttackEntityFrom(EnumDamageSource.CauseMobDamage, damage, entity);
                    }
                }
            }
        }
    }
}
