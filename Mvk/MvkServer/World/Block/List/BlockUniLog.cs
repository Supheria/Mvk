using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект бревна
    /// </summary>
    public class BlockUniLog : BlockUniWood
    {
        /***
         * Met
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3 - вверх, игрок
         * 4/5 - бок, игрок
         * 6 - вверх, генерация, нижний блок для пня
         */
        
        /// <summary>
        /// Длинна максимальной ветки
        /// </summary>
        private readonly int stepBranchMax = 8;
        /// <summary>
        /// Объект для генерации дерева
        /// </summary>
        private readonly WorldGenTree2 worldGenTree;

        public BlockUniLog(int numberTextureButt, int numberTextureSide, WorldGenTree2 worldGenTree, int stepBranchMax = 1) 
            : base(numberTextureButt, numberTextureSide)
        {
            this.worldGenTree = worldGenTree;
            this.stepBranchMax = stepBranchMax;
            NeedsRandomTick = worldGenTree != null;
            Combustibility = true;
            IgniteOddsSunbathing = 5;
            BurnOdds = 10;
            metUp = 3;
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => state.met == 6 ? 100 : 30;

        #region Drop

        /// <summary>
        /// Получите количество выпавших на основе данного уровня удачи
        /// </summary>
        protected override int QuantityDroppedWithBonus(ItemAbTool itemTool, Rand random) 
            => itemTool != null ? itemTool.Level == 0 ? 0 : 1 : 0;

        /// <summary>
        /// Получите количество ДОПОЛНИТЕЛЬНЫХ предметов выпавших на основе данного уровня удачи
        /// </summary>
        protected override int QuantityDroppedWithBonusAdditional(ItemAbTool itemTool, Rand random) => random.Next(2);

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
        {
            // 3 = 60%, 2 = 40%, 1 = 20%, 0 = 0%
            if (rand.Next(5) < (itemTool == null ? 0 : itemTool.Level)) return Items.GetItemCache(state.id);
            return Items.GetItemCache(rand.Next(2) == 0 ? EnumItem.WoodChips : EnumItem.Stick);
        }

        /// <summary>
        /// Получите ДОПОЛНИТЕЛЬНЫЙ предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDroppedAdditional(BlockState state, Rand rand, ItemAbTool itemTool) 
            => Items.GetItemCache(rand.Next((itemTool == null ? 0 : itemTool.Level) + 5) < 4 ? EnumItem.WoodChips : EnumItem.Stick);

        #endregion

        /// <summary>
        /// Стороны целого блока для рендера 0 - 5 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb)
        {
            if (met == 6) return quads[3];
            // gen 0 up; 1 e||w; 2 s||n; 
            // play 3 up; 4 e||w; 5 s||n;
            // gen down 6 up;
            if (met > 2) met -= 3;
            //if (met > 2) met -= 3;
            return quads[met];
        }

        #region Break

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        public override void OnBreakBlock(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            int met = state.met;
            if (met == 1 || met == 2)
            {
                // Ветка!
                Branch(worldIn, blockPos, state.id, state.met);
            }
            else if (met == 0 || met == 6)
            {
                // Ствол!
                Trunk(worldIn, blockPos, state);
            }
        }

        /// <summary>
        /// Ствол со смещением
        /// </summary>
        private void Trunk(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            ushort id = state.id;
            // Удаление веток тикущего блока
            TrunkBranchBreaks(worldIn, blockPos, id);
            // Разрушение строго ровного ствола!!!
            while (true)
            {
                blockPos.Y++;
                if (!TrunkCheck(worldIn, blockPos, id))
                {
                    blockPos.X++;
                    if (!TrunkCheck(worldIn, blockPos, id))
                    {
                        blockPos.X -= 2;
                        if (!TrunkCheck(worldIn, blockPos, id))
                        {
                            blockPos.X++;
                            blockPos.Z++;
                            if (!TrunkCheck(worldIn, blockPos, id))
                            {
                                blockPos.Z -= 2;
                                if (!TrunkCheck(worldIn, blockPos, id))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                // Удаление веток
                TrunkBranchBreaks(worldIn, blockPos, id);
                // Разрушаем блок ствола
                DropBlockAsItem(worldIn, blockPos, new BlockState(EBlock));
                worldIn.SetBlockToAir(blockPos);
            }
        }

        /// <summary>
        /// Удаление веток тикущего ствола
        /// </summary>
        private void TrunkBranchBreaks(WorldBase worldIn, BlockPos blockPos, ushort id)
        {
            BranchBreakFromTrunk(worldIn, blockPos, new vec2i(1, 0), id, 1);
            BranchBreakFromTrunk(worldIn, blockPos, new vec2i(-1, 0), id, 1);
            BranchBreakFromTrunk(worldIn, blockPos, new vec2i(0, 1), id, 2);
            BranchBreakFromTrunk(worldIn, blockPos, new vec2i(0, -1), id, 2);
        }
        /// <summary>
        /// Проверка ствола
        /// </summary>
        private bool TrunkCheck(WorldBase worldIn, BlockPos blockPos, ushort id)
        {
            BlockState state = worldIn.GetBlockState(blockPos);
            if (id == state.id && state.met == 0) return true;
            return false;
        }

        /// <summary>
        /// Проверка присутствует ли в этом направлении ствол
        /// </summary>
        private bool BranchCheck(WorldBase worldIn, BlockPos blockPos, vec2i vec, ushort id, ushort met, int step)
        {
            BlockPos pos = blockPos.Offset(vec.x, 0, vec.y);
            int result = BranchCheckState(worldIn, pos, id, met);
            if (result == 2) return true;
            if (result == -1)
            {
                pos = blockPos.Offset(vec.x, 1, vec.y);
                result = BranchCheckState(worldIn, pos, id, met);
                if (result == 2) return true;
                if (result == -1)
                {
                    pos = blockPos.Offset(vec.x, -1, vec.y);
                    result = BranchCheckState(worldIn, pos, id, met);
                    if (result == 2) return true;
                    if (result == -1)
                    {
                        pos = blockPos.Offset(vec.x == 0 ? 1 : vec.x, 0, vec.y == 0 ? 1 : vec.y);
                        result = BranchCheckState(worldIn, pos, id, met);
                        if (result == 2) return true;
                        if (result == -1)
                        {
                            pos = blockPos.Offset(vec.x == 0 ? -1 : vec.x, 0, vec.y == 0 ? -1 : vec.y);
                            result = BranchCheckState(worldIn, pos, id, met);
                            if (result == 2) return true;
                            if (result == -1) return false;
                        }
                    }
                }
            }
            // продолжение ветки
            if (step >= stepBranchMax) return false;
            return BranchCheck(worldIn, pos, vec, id, met, step++);
        }

        /// <summary>
        /// Удаление ветки, если не известно откуда идёт ствол
        /// </summary>
        private void BranchBreak(WorldBase worldIn, BlockPos blockPos, vec2i vec, ushort id, ushort met, int step)
        {
            BlockPos pos = blockPos.Offset(vec.x, 0, vec.y);
            int result = BranchCheckState(worldIn, pos, id, met);
            if (result != 1)
            {
                pos = blockPos.Offset(vec.x, 1, vec.y);
                result = BranchCheckState(worldIn, pos, id, met);
                if (result != 1)
                {
                    pos = blockPos.Offset(vec.x, -1, vec.y);
                    result = BranchCheckState(worldIn, pos, id, met);
                    if (result != 1)
                    {
                        pos = blockPos.Offset(vec.x == 0 ? 1 : vec.x, 0, vec.y == 0 ? 1 : vec.y);
                        result = BranchCheckState(worldIn, pos, id, met);
                        if (result != 1)
                        {
                            pos = blockPos.Offset(vec.x == 0 ? -1 : vec.x, 0, vec.y == 0 ? -1 : vec.y);
                            result = BranchCheckState(worldIn, pos, id, met);
                            if (result != 1) return;
                        }
                    }
                }
            }
            // Разрушаем блок ветки
            DropBlockAsItem(worldIn, pos, new BlockState(EBlock));
            worldIn.SetBlockToAir(pos);
            if (step < stepBranchMax)
            {
                // Продолжаем разрушать ветку
                BranchBreak(worldIn, pos, vec, id, met, step++);
            }
        }

        /// <summary>
        /// Разрушение ветки от ствола
        /// </summary>
        private void BranchBreakFromTrunk(WorldBase worldIn, BlockPos blockPos, vec2i vec, ushort id, ushort met)
        {
            BlockPos pos = blockPos.Offset(vec.x, 0, vec.y);
            if (BranchCheckState(worldIn, pos, id, met) == 1)
            {
                // Разрушаем блок ветки
                DropBlockAsItem(worldIn, pos, new BlockState(EBlock));
                worldIn.SetBlockToAir(pos);
                // Продолжаем разрушать ветку
                BranchBreak(worldIn, pos, vec, id, met, 1);
            }
        }

        /// <summary>
        /// Проверка блока является ли тем же блоком и какой поворот (ствол или ветка)
        /// </summary>
        private int BranchCheckState(WorldBase worldIn, BlockPos blockPos, ushort id, ushort met)
        {
            BlockState state = worldIn.GetBlockState(blockPos);
            if (state.id == id)
            {
                // Ветка
                if (state.met == met) return 1;
                // Ствол
                if (state.met == 0) return 2;
            }
            // Пусто или другой блок не этого типа
            return -1;
        }

        /// <summary>
        /// Работа с ветками
        /// </summary>
        private void Branch(WorldBase worldIn, BlockPos blockPos, ushort id, ushort met)
        {
            bool b;
            if (met == 1)
            {
                b = BranchCheck(worldIn, blockPos, new vec2i(1, 0), id, met, 0);
                if (!b) BranchBreak(worldIn, blockPos, new vec2i(1, 0), id, met, 0);
                b = BranchCheck(worldIn, blockPos, new vec2i(-1, 0), id, met, 0);
                if (!b) BranchBreak(worldIn, blockPos, new vec2i(-1, 0), id, met, 0);
            }
            else
            {
                b = BranchCheck(worldIn, blockPos, new vec2i(0, 1), id, met, 0);
                if (!b) BranchBreak(worldIn, blockPos, new vec2i(0, 1), id, met, 0);
                b = BranchCheck(worldIn, blockPos, new vec2i(0, -1), id, met, 0);
                if (!b) BranchBreak(worldIn, blockPos, new vec2i(0, -1), id, met, 0);
            }
        }

        #endregion

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (random.Next(16) == 0)
            {
                EnumTimeYear enumTimeYear = world.GetTimeYear();
                if (enumTimeYear == EnumTimeYear.Spring || enumTimeYear == EnumTimeYear.Summer)
                {
                    int met = blockState.met;
                    int side = 0;
                    bool growth = false;
                    if (met == 1)
                    {
                        blockPos.X++;
                        BlockState blockStateSide = world.GetBlockState(blockPos);
                        if (blockStateSide.IsAir() && blockStateSide.lightSky > 10)
                        {
                            side = 1;
                            blockPos.X--;
                            growth = true;
                        }
                        if (!growth)
                        {
                            blockPos.X -= 2;
                            blockStateSide = world.GetBlockState(blockPos);
                            if (blockStateSide.IsAir() && blockStateSide.lightSky > 10)
                            {
                                side = 3;
                                blockPos.X++;
                                growth = true;
                            }
                        }
                    }
                    else if (met == 2)
                    {
                        blockPos.Z++;
                        BlockState blockStateSide = world.GetBlockState(blockPos);
                        if (blockStateSide.IsAir() && blockStateSide.lightSky > 10)
                        {
                            side = 0;
                            blockPos.Z--;
                            growth = true;
                        }
                        if (!growth)
                        {
                            blockPos.Z -= 2;
                            blockStateSide = world.GetBlockState(blockPos);
                            if (blockStateSide.IsAir() && blockStateSide.lightSky > 10)
                            {
                                side = 2;
                                blockPos.Z++;
                                growth = true;
                            }
                        }
                    }
                    if (growth)
                    {
                        // Рост веток
                        worldGenTree.GenerateFoliageBranch(world, blockPos, side);
                    }
                }
            }
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitBoxs()
        {
            quads = new QuadSide[][]
            {
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.South)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.South)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.South)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.East),
                    new QuadSide(1).SetTexture(66, 2).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.West),
                    new QuadSide(1).SetTexture(66, 2).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.North),
                    new QuadSide(1).SetTexture(66, 2).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.South),
                    new QuadSide(1).SetTexture(66, 2).SetSide(Pole.South)
                },
            };
        }
    }
}
