using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Network.Packets.Client;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект работы над игроком разрушения блока, итомы и прочее
    /// </summary>
    public class ItemInWorldManager
    {
        /// <summary>
        /// Позиция блока который разрушаем
        /// </summary>
        public BlockPos BlockPosDestroy { get; private set; }
        /// <summary>
        /// Истинно, если игрок уничтожает блок или ставит
        /// </summary>
        public bool IsDestroyingBlock { get; private set; } = false;
        /// <summary>
        /// Значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        private vec3 facing;
        /// <summary>
        /// Направление установки блока
        /// </summary>
        private Pole side;
        /// <summary>
        /// Тип блока для установки
        /// </summary>
        //private EnumBlock enumBlock = EnumBlock.None;
        /// <summary>
        /// Номер слота, с какого ставим блок
        /// </summary>
        private int slotPut = 0;
        /// <summary>
        /// Пауза между ударами, если удар не отключали (Update)
        /// </summary>
        private int pause = 0;
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        private WorldBase world;
        /// <summary>
        /// Объект игрока
        /// </summary>
        private EntityPlayer entityPlayer;
        /// <summary>
        /// Начальный урон блока
        /// </summary>
        private int initialDamage;
        /// <summary>
        /// Счётчик тактов нанесения урона на блок
        /// </summary>
        private int curblockDamage;
        /// <summary>
        /// Прочность, оставшаяся в блоке.
        /// -1 отмена, -2 сломан блок, -3 ставим блок
        /// </summary>
        private int durabilityRemainingOnBlock = -1;
        /// <summary>
        /// Статус действия
        /// </summary>
        private Status status = Status.None;
        /// <summary>
        /// Список мгновенной поломки
        /// </summary>
        private DoubleList<BlockPos> listDestroy = new DoubleList<BlockPos>(20); 

        public ItemInWorldManager(WorldBase world, EntityPlayer entityPlayer)
        {
            this.world = world;
            this.entityPlayer = entityPlayer;
        }

        /// <summary>
        /// Нет паузы в обновлении между ударами
        /// </summary>
        public bool NotPauseUpdate => pause == 0;

        /// <summary>
        /// Начато разрушение
        /// </summary>
        public void DestroyStart(BlockPos blockPos, int hardness = -1)
        {
            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            if (!block.IsAir && entityPlayer.Inventory.CanDestroyedBlock(block))
            {
                BlockPosDestroy = blockPos;
                IsDestroyingBlock = true;
                curblockDamage = 0;
                initialDamage = hardness == -1 ? block.GetPlayerRelativeBlockHardness(entityPlayer, blockState) : hardness;
                durabilityRemainingOnBlock = GetProcess();
                if (durabilityRemainingOnBlock < 0)
                {
                    // При старте нельзя помечать как стоп (-2) это должно быть в игровом такте
                    durabilityRemainingOnBlock = 0;
                }
                pause = entityPlayer.PauseTimeBetweenBlockDestruction();
            }
        }

        /// <summary>
        /// Отмена разрушения
        /// </summary>
        public void DestroyAbout() => status = Status.About;

        /// <summary>
        /// Окончено разрушение, блок сломан
        /// </summary>
        public void DestroyStop() => status = Status.Stop;

        /// <summary>
        /// Можно ли разрушить блок
        /// </summary>
        public bool CanDestroy(BlockBase block) => !block.IsAir && entityPlayer.Inventory.CanDestroyedBlock(block);

        /// <summary>
        /// Удар по блоку
        /// </summary>
        /// <param name="isFirst">Первый удар</param>
        public void HitOnBlock(BlockPos blockPos, BlockBase block, bool isFirst)
        {
            world.PlaySound(entityPlayer, block.SampleBreak(world), blockPos.ToVec3() + .5f, isFirst ? .4f : 1f, block.SampleBreakPitch(world.Rnd) * .8f);
            if (entityPlayer.Inventory.OnHitOnBlock(world, block, blockPos, 1))
            {
                DestroyAbout();
            }
        }

        /// <summary>
        /// Проверка на начальный урон для дополнительного удара по блоку
        /// </summary>
        public bool CheckInitialDamage(int hardness) => hardness > 3;

        /// <summary>
        /// Мгновенное разрушение блока
        /// </summary>
        public void InstantDestroy(BlockPos blockPos) => listDestroy.Add(blockPos);

        /// <summary>
        /// Разрушение блока
        /// </summary>
        public void Destroy(BlockPos blockPos)
        {
            // Уничтожение блока
            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();

            if (!entityPlayer.IsCreativeMode)
            {
                if (!world.IsRemote && entityPlayer.Inventory.CanHarvestBlock(block))
                {
                    block.DropBlockAsItem(world, blockPos, blockState, entityPlayer.Inventory.GetItemTool());
                }
                // Действия уничтожения блока для предметов
                entityPlayer.Inventory.OnBlockDestroyed(world, block, blockPos);
            }
            world.SetBlockToAir(blockPos, world.IsRemote ? 14 : 31);
            pause = entityPlayer.PauseTimeBetweenBlockDestruction();
        }

        /// <summary>
        /// Установить блок
        /// </summary>
        public void Put(BlockPos blockPos, Pole side, vec3 facing, int slot)
        {
            IsDestroyingBlock = true;
            BlockPosDestroy = blockPos;
            this.facing = facing;
            this.side = side;
            slotPut = slot;
            status = Status.Put;
        }

        /// <summary>
        /// Пауза между установками блоков, в тактах.
        /// true - между первым блоком и вторы,
        /// false - последующие, 4 успевает поставить 2 блока, когда прыгает
        /// </summary>
        public void PutPause(bool start) => pause = start ? 8 : 4;

        /// <summary>
        /// Установить блок не получилось, но мы фиксируем зажатие клавиши
        /// </summary>
        public void PutAbout()
        {
            IsDestroyingBlock = false;
            pause = 0;
        }

        /// <summary>
        /// Обновление мгновенного удаление, только на клиенте надо в тике
        /// </summary>
        public void UpdateDestroy()
        {
            listDestroy.Step();
            int count = listDestroy.CountBackward;
            for (int i = 0; i < count; i++)
            {
                Destroy(listDestroy.GetNext());
            }
        }

        /// <summary>
        /// Обновление разрушения блока
        /// </summary>
        public StatusAnimation UpdateBlock()
        {
            StatusAnimation statusUpdate = StatusAnimation.NotAnimation;

            if (status != Status.None)
            {
                durabilityRemainingOnBlock = (int)status;
                status = Status.None;
            }

            if (IsDestroyingBlock)
            {
                curblockDamage++;

                if (durabilityRemainingOnBlock >= 0)
                {
                    if (world.GetBlockState(BlockPosDestroy).GetBlock().IsAir)
                    {
                        durabilityRemainingOnBlock = (int)Status.About;
                    }
                }

                if (durabilityRemainingOnBlock < 0)
                {
                    if (durabilityRemainingOnBlock == (int)Status.About)
                    {
                        // Отмена действия блока, убираем трещину
                        world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, durabilityRemainingOnBlock);
                    }
                    else if (durabilityRemainingOnBlock == (int)Status.Stop)
                    {
                        // Уничтожение блока
                        Destroy(BlockPosDestroy);
                    }
                    else if (durabilityRemainingOnBlock == (int)Status.Put)
                    {
                        // Действие правой клавишей, ставим блок
                        if (!world.IsRemote)
                        {
                            ItemStack itemStack = entityPlayer.Inventory.GetStackInSlot(slotPut);
                            if (itemStack != null && itemStack.ItemUse(entityPlayer, world, BlockPosDestroy, side, facing))
                            {
                                // Было действие!
                            }
                        }
                    }
                    IsDestroyingBlock = false;
                }
                else
                {
                    int process = GetProcess();
                    if (curblockDamage == 1)
                    {
                        // Первый удар на разрушении блока, статус начала анимации
                        statusUpdate = StatusAnimation.Animation;
                    }
                    else
                    {
                        statusUpdate = StatusAnimation.None;
                    }

                    if (curblockDamage % 4 == 0 && CheckInitialDamage(initialDamage - curblockDamage))
                    {
                        // Удар по блоку
                        HitOnBlock(BlockPosDestroy, world.GetBlockState(BlockPosDestroy).GetBlock(), false);
                    }

                    if (process != durabilityRemainingOnBlock)
                    {
                        durabilityRemainingOnBlock = process;
                        world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, durabilityRemainingOnBlock);
                        //// Звук удара
                        //entityPlayer.PlaySound(world.GetBlockState(BlockPosDestroy).GetBlock().SampleBreak(world), 1f, .8f);
                        //this.mc.getSoundHandler().playSound(new PositionedSoundRecord(new ResourceLocation(var6.stepSound.getBreakSound()), 
                        //(var6.stepSound.getVolume() + 1.0F) / 2.0F, var6.stepSound.getFrequency() * 0.8F, 
                        // (float)p_180439_3_.getX() + 0.5F, (float)p_180439_3_.getY() + 0.5F, (float)p_180439_3_.getZ() + 0.5F));
                    }
                }
            }
            else
            {
                if (pause > 0) pause--;
            }
            return statusUpdate;
        }

        /// <summary>
        /// Проверка на блок тольо что сломался
        /// </summary>
        public bool IsDestroy() => IsDestroyingBlock && curblockDamage >= initialDamage;

        /// <summary>
        /// Получить значение процесса
        /// </summary>
        private int GetProcess()
        {
            int process = initialDamage <= 1 ? (int)Status.Stop : curblockDamage * 10 / initialDamage;
            if (process > 9) process = (int)Status.Stop;
            return process;
        }

        /// <summary>
        /// Статус анимации руки после обнавления игрового такта
        /// </summary>
        public enum StatusAnimation
        {
            /// <summary>
            /// Нет значения
            /// </summary>
            None,
            /// <summary>
            /// Есть анимация
            /// </summary>
            Animation,
            /// <summary>
            /// Нет анимации
            /// </summary>
            NotAnimation
        }

        private enum Status
        {
            None = 0,
            About = -1,
            Stop = -2,
            Put = -3
        }
    }
}
