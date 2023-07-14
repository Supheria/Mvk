using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача найти цветок или траву с вероятностью 30% или 70% дёрн, подойти к нему и съесть
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
        /// Ищем траву
        /// </summary>
        private bool isSapling = false;

        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float probability;

        /// <summary>
        /// Задача найти цветок или траву, подойти к нему и съесть
        /// </summary>
        public EntityAIFindSaplingEat(EntityLiving entity, float probability = .008f, float speed = 1f)
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

            // Что ищем
            isSapling = entity.World.Rnd.NextFloat() > .7f;

            bool result = FoodSearch();
            if (!result && isSapling)
            {
                isSapling = false;
                return FoodSearch();
            }
            return result;
        }

        /// <summary>
        /// Поиск еды
        /// </summary>
        private bool FoodSearch()
        {
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

            int start = entity.World.Rnd.Next(4);
            // 149 это радиус 7 включительно
            for (int i = start; i < 149; i++)
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
            => isSapling ? entity.World.GetBlockState(new BlockPos(x, y, z)).GetBlock().Material.EMaterial == EnumMaterial.Sapling
                : entity.World.GetBlockState(new BlockPos(x, y - 1, z)).GetEBlock() == EnumBlock.Turf;

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
        private void Action(BlockPos blockPos)
        {
            if (entity.World.GetBlockState(blockPos).GetBlock().Material.EMaterial == EnumMaterial.Sapling)
            {
                entity.World.SetBlockToAir(blockPos, 31);
                Eat();
            }
            else if (!isSapling)
            {
                blockPos = blockPos.OffsetDown();
                if (entity.World.GetBlockState(blockPos).GetEBlock() == EnumBlock.Turf)
                {
                    entity.World.SetBlockState(blockPos, new BlockState(EnumBlock.Dirt), 31);
                    Eat();
                }
            }
        }

        /// <summary>
        /// Кушаем
        /// </summary>
        private void Eat()
        {
            if (entity is EntityChicken entityChicken && entityChicken.countEat < 10)
            {
                entityChicken.countEat++;
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
            actionMove = false;
            actionEat = false;
        }
    }
}
