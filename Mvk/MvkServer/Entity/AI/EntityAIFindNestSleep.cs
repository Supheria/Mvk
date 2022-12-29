using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача найти блок гнезда, подойти к нему и сесть для сна, если нет гнезда спать просто
    /// </summary>
    public class EntityAIFindNestSleep : EntityAIBase
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
        /// действие начало
        /// </summary>
        private bool actionBegin;
        /// <summary>
        /// действие перемещения
        /// </summary>
        private bool actionMove;
        /// <summary>
        /// действие спать
        /// </summary>
        private bool actionSleep;

        /// <summary>
        /// Задача найти блок гнезда, подойти к нему и сесть
        /// </summary>
        public EntityAIFindNestSleep(EntityLiving entity, float speed = 1f)
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
            if (entity.World.IsDayTime()) return false;

            actionSleep = true;
            // Поиск
            int x0 = Mth.Floor(entity.Position.x);
            int y0 = Mth.Floor(entity.Position.y);
            int z0 = Mth.Floor(entity.Position.z);

            // Проверка наличия гнезда под ногами
            if (Check(x0, y0, z0))
            {
                actionMove = false;
                return true;
            }
            
            if (entity is EntityChicken entityChicken)
            {
                // Проверяем гнездо
                BlockPos blockPos = entityChicken.GetPosHome();
                if (entityChicken.IsPosHome() && Check(blockPos))
                {
                    // Есть точка дома и она не изменилась, пытаемся двигатся к ней
                    SetMutexBits(3);
                    actionBegin = true;
                    xPosition = blockPos.X;
                    yPosition = blockPos.Y;
                    zPosition = blockPos.Z;
                    return true;
                }
                else
                {
                    // Ищем новую точку дома
                    int x, y, z;
                    // 317 это радиус 10 включительно
                    for (int i = 0; i < 317; i++)
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
                                SetMutexBits(3);
                                xPosition = x;
                                yPosition = y;
                                zPosition = z;
                                entityChicken.SetPosHome(new BlockPos(x, y, z));
                                return true;
                            }
                        }
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Проверка нахождении нужного блока
        /// </summary>
        private bool Check(int x, int y, int z) => Check(new BlockPos(x, y, z));
        /// <summary>
        /// Проверка нахождении нужного блока
        /// </summary>
        private bool Check(BlockPos blockPos) => entity.World.GetBlockState(blockPos).GetEBlock() == EnumBlock.Nest;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (actionBegin)
            {
                actionBegin = false;
                float dis = glm.distance(entity.Position, new vec3(xPosition, yPosition, zPosition));
                if (entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed, dis > 15))
                {
                    actionMove = true;
                }
            }
            else if (actionMove)
            {
                if (entity.GetNavigator().NoPath()) actionMove = false;
            }
            else if(actionSleep)
            {
                entity.GetMoveHelper().SetSneak();
                if (entity.World.IsDayTime() && entity.World.Rnd.NextFloat() < .3f) actionSleep = false;
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => actionMove || actionSleep;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            if (!actionBegin && entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed, false))
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
            actionSleep = false;
        }
    }
}
