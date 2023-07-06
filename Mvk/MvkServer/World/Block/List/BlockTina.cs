using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World.Biome;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок тины
    /// </summary>
    public class BlockTina : BlockBase
    {
        /// <summary>
        /// Блок тины
        /// </summary>
        public BlockTina()
        {
            NeedsRandomTick = true;
            IsCollidable = false;
            SetUnique(true);
            Material = EnumMaterial.Interior;
            Particle = 208;
            Resistance = .2f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(1).SetTexture(Particle).SetSide(0, true, 0, 0, 0, 16, 0, 16),
            }};
        }

        /// <summary>
        /// Блок который замедляет сущность в перемещении на ~30%
        /// </summary>
        public override bool IsSlow(BlockState state) => true;

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public override bool IsLiquidDestruction() => true;

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 50;

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => false;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                worldIn.SetBlockToAir(blockPos, 31);
            }
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0) 
            => worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock() == EnumBlock.Water;

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met) 
            => new AxisAlignedBB[] {
                new AxisAlignedBB(new vec3(pos.X, pos.Y, pos.Z), new vec3(pos.X + 1, pos.Y + .125f, pos.Z + 1)),
            };

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        //public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        public override int QuantityDropped(Rand random) => 0;

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (!CheckBiome(world.GetBiome(blockPos)))
            {
                // Сохнет в чужом биоме
                world.SetBlockToAir(blockPos);
            }
            else
            {
                if (random.Next(8) == 0)
                {
                    EnumTimeYear timeYear = world.GetTimeYear();
                    EnumMoonPhase moonPhase = world.GetMoonPhase();

                    if (timeYear == EnumTimeYear.Summer
                        || (timeYear == EnumTimeYear.Spring && moonPhase == EnumMoonPhase.AgingMoon))
                    {
                        // Тина растёт весной в стареющей луне и всё лето
                        BlockPos blockPos2;
                        BlockState blockState2;
                        // Проверяем 4 блока рядом (8 доступных)
                        for (int i = 0; i < 4; i++)
                        {
                            blockPos2 = blockPos.Offset(random.Next(3) - 1, 0, random.Next(3) - 1);
                            if (world.GetBlockState(blockPos2.OffsetDown()).GetEBlock() == EnumBlock.Water
                                && CheckBiome(world.GetBiome(blockPos2)))
                            {
                                blockState2 = world.GetBlockState(blockPos2);
                                if (blockState2.IsAir())
                                {
                                    world.SetBlockState(blockPos2, new BlockState(EnumBlock.Tina), 12);
                                }
                            }
                        }
                    }
                    else if (timeYear == EnumTimeYear.Winter)
                    {
                        // Тина сохнет всю зиму
                        world.SetBlockToAir(blockPos);
                    }
                }
            }
        }

        /// <summary>
        /// Проверка может ли в этом биоме быть тина
        /// </summary>
        private static bool CheckBiome(EnumBiome biome) => biome == EnumBiome.Swamp 
            || biome == EnumBiome.Plain || biome == EnumBiome.BirchForest 
            || biome == EnumBiome.ConiferousForest || biome == EnumBiome.MixedForest;

        /// <summary>
        /// Может ли рости тина
        /// </summary>
        public static bool IsGrows(WorldBase world, BlockPos blockPos)
        {
            EnumTimeYear timeYear = world.GetTimeYear();
            EnumMoonPhase moonPhase = world.GetMoonPhase();

            if (timeYear == EnumTimeYear.Summer 
                || (timeYear == EnumTimeYear.Spring && moonPhase == EnumMoonPhase.AgingMoon))
            {
                // Вторая половина весны, и всё лето
                return CheckBiome(world.GetBiome(blockPos));
            }
            return false;
        }
    }
}
