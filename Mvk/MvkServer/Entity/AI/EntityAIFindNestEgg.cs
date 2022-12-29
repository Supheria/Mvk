using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача найти блок гнезда, подойти к нему и сесть для высадки яйца
    /// </summary>
    public class EntityAIFindNestEgg : EntityAIBase
    {
        /// <summary>
        /// Запуск задачи
        /// </summary>
        public bool run = false;
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
        /// действие кушать
        /// </summary>
        private bool actionEgg;
        /// <summary>
        /// Время в тиках на высиживания яйца в гнезде
        /// </summary>
        private int timeEgg;

        /// <summary>
        /// Задача найти блок гнезда, подойти к нему и сесть
        /// </summary>
        public EntityAIFindNestEgg(EntityLiving entity, float speed = 1f)
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
            if (!run) return false;

            run = false;
            // Поиск
            int x0 = Mth.Floor(entity.Position.x);
            int y0 = Mth.Floor(entity.Position.y);
            int z0 = Mth.Floor(entity.Position.z);

            // Проверка наличия гнезда под ногами
            if (Check(x0, y0, z0))
            {
                NestBegin();
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
            
            return false;
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
        /// Начинаем садится
        /// </summary>
        private void NestBegin()
        {
            SetMutexBits(1);
            actionMove = false;
            if (Check(new BlockPos(entity.Position)))
            {
                actionEgg = true;
                timeEgg = entity.World.Rnd.Next(300) + 300;
            }
            else
            {
                DidNotWorkOut();
            }
        }

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
                if (entity.GetNavigator().NoPath()) NestBegin();
            }
            else if (actionEgg)
            {
                entity.GetMoveHelper().SetSneak();
                if (--timeEgg <= 0)
                {
                    actionEgg = false;
                    
                    BlockPos blockPos = new BlockPos(entity.Position);
                    BlockState blockState = entity.World.GetBlockState(blockPos);
                    if (blockState.GetEBlock() == EnumBlock.Nest && blockState.met < 4)
                    {
                        entity.World.SetBlockStateMet(blockPos, (ushort)(blockState.met + 1));
                        // Звук яйца, чпок
                        entity.PlaySound(Sound.AssetsSample.MobChickenPlop, 1f, 1f);
                        if (entity is EntityChicken entityChicken) entityChicken.countEat = 0;
                    }
                }
                else if (!Check(new BlockPos(entity.Position)))
                {
                    actionEgg = false;
                    DidNotWorkOut();
                }
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => actionMove || actionEgg;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            if (!actionBegin && !actionEgg 
                && entity.GetNavigator().TryMoveToXYZ(xPosition, yPosition, zPosition, speed, false))
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
            actionEgg = false;
        }

        /// <summary>
        /// Не получилось вынести яйцо, причины, столкнули с гнезда, или не нашли гнездо
        /// </summary>
        private void DidNotWorkOut()
        {
            if (entity is EntityChicken entityChicken) entityChicken.countEat = 20;
            // Крик возмущения
            entity.PlaySoundSay();
        }
    }
}
