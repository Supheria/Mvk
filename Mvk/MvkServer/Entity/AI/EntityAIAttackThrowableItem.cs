using MvkServer.Entity.List;
using MvkServer.Item;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача атаковать метательным предметом
    /// </summary>
    public class EntityAIAttackThrowableItem : EntityAIBase
    {
        /// <summary>
        /// Тикер интервала между атаками
        /// </summary>
        private int attackTick = 0;
        /// <summary>
        /// Тикер для залпа атак
        /// </summary>
        private int attackVolley = 0;
        /// <summary>
        /// Залп из количество выстрелов
        /// </summary>
        private readonly int volley;
        /// <summary>
        /// Растояние на котором начинается выстрелы, квадрат
        /// </summary>
        private readonly int distantion;
        /// <summary>
        /// Предмет
        /// </summary>
        private readonly EnumItem item;

        /// <summary>
        /// Задача атаковать метательным предметом
        /// </summary>
        public EntityAIAttackThrowableItem(EntityLiving entity, EnumItem item, int volley = 5, int distantion = 400)
        {
            this.entity = entity;
            this.item = item;
            this.volley = volley;
            this.distantion = distantion;
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
                    float distantion = entity.GetDistanceSq(entityLiving.Position.x, entityLiving.Position.y, entityLiving.Position.z);
                    float width = entity.Width * entity.Width * 8f + entityLiving.Width * 2f;

                    if (distantion > 1 && distantion < this.distantion) // дистанция примерно 20
                    {
                        if (attackVolley > 1)
                        {
                            attackVolley--;
                            attackTick = 2;
                        }
                        else
                        {
                            attackTick = 50;
                            attackVolley = volley;
                        }

                        entity.GetLookHelper().SetLookPositionWithEntity(entityLiving, 30, 30);
                        entity.SwingItem();
                        EntityPiece entityPiece = new EntityPiece(entity.World, entity);
                        entityPiece.SetItem(item);
                        entity.World.SpawnEntityInWorld(entityPiece);
                    }
                }
            }
        }
    }
}
