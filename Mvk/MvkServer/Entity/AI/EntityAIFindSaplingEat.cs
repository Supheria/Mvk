using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача найти цветок или траву, подойти к нему и съесть
    /// </summary>
    public class EntityAIFindSaplingEat : EntityAIBase
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
        /// действие перемещения
        /// </summary>
        private bool actionMove;
        /// <summary>
        /// действие кушать
        /// </summary>
        private bool actionEat;
        /// <summary>
        /// Время в тиках на поедание блока
        /// </summary>
        private int timeEat;

        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float probability;

        /// <summary>
        /// Задача найти цветок или траву, подойти к нему и съесть
        /// </summary>
        public EntityAIFindSaplingEat(EntityLiving entity, float speed = 1f, float probability = .004f)
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
            if (entity.World.Rnd.NextFloat() >= probability) return false;

            // Поиск
            int x0 = Mth.Floor(entity.Position.x);
            int y0 = Mth.Floor(entity.Position.y);
            int z0 = Mth.Floor(entity.Position.z);

            // Проверка наличия растения под ногами
            if (Check(x0, y0, z0))
            {
                EatBegin();
                return true;
            }

            int x, y, z;
            
            // 149 это радиус 7 включительно
            for (int i = 0; i < 149; i++)
            {
                for (int y3 = 0; y3 < 3; y3++)
                {
                    vec2i vec = MvkStatic.DistSqrt37[i];
                    x = vec.x + x0;
                    z = vec.y + z0;
                    y = y3;
                    if (y > 1) y = -1;
                    y += y0;

                    if (Check(x, y, z))
                    {
                        xPosition = x;
                        yPosition = y;
                        zPosition = z;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Проверка нахождении нужного блока
        /// </summary>
        protected virtual bool Check(int x, int y, int z) 
            => entity.World.GetBlockState(new BlockPos(x, y, z)).GetBlock().Material == EnumMaterial.Sapling;

        /// <summary>
        /// Начинаем кушать
        /// </summary>
        private void EatBegin()
        {
            actionMove = false;
            actionEat = true;
            timeEat = 25;
            entity.GetLookHelper().SetLookPitch(-glm.pi90, glm.pi10);
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (actionMove)
            {
                if (entity.GetNavigator().NoPath()) EatBegin();
            }
            else if (actionEat)
            {
                if (--timeEat <= 0)
                {
                    actionEat = false;
                    entity.GetLookHelper().SetLookPitch(0, glm.pi10);
                    Action(new BlockPos(entity.Position));
                }
            }
        }

        /// <summary>
        /// Действие когда дошли до блока
        /// </summary>
        protected virtual void Action(BlockPos blockPos)
        {
            if (entity.World.GetBlockState(blockPos).GetBlock().Material == EnumMaterial.Sapling)
            {
                entity.World.SetBlockToAir(blockPos, 31);
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => actionMove || actionEat;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            if (!actionEat && entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed, false))
            {
                actionMove = true;
            }
        }

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask()
        {
            entity.GetNavigator().ClearPathEntity();
            actionMove = false;
            actionEat = false;
        }
    }
}
