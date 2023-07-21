using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок дёрна
    /// </summary>
    public class BlockTurf : BlockBase
    {
        /// <summary>
        /// Блок дёрна
        /// </summary>
        public BlockTurf()
        {
            Resistance = .6f;
            Combustibility = true;
            IgniteOddsSunbathing = 5;
            NeedsRandomTick = true;
            Particle = 64;
            BiomeColor = true;
            Material = Materials.GetMaterialCache(EnumMaterial.Loose);
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepGrass1, AssetsSample.StepGrass2, AssetsSample.StepGrass3, AssetsSample.StepGrass4 };
            InitQuads();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 10;

        /// <summary>
        /// Стороны целого блока для рендера 0 - 5 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[(xc + zc + xb + zb) & 3];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        private void InitQuads()
        {
            quads = new QuadSide[4][];
            for (int i = 0; i < 4; i++)
            {
                quads[i] = new QuadSide[] {
                    new QuadSide(0).SetTexture(64).SetSide(Pole.Down),
                    new QuadSide(1).SetTexture(65, i).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(67).SetSide(Pole.East),
                    new QuadSide(1).SetTexture(66).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(67).SetSide(Pole.West),
                    new QuadSide(1).SetTexture(66).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(67).SetSide(Pole.North),
                    new QuadSide(1).SetTexture(66).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(67).SetSide(Pole.South),
                    new QuadSide(1).SetTexture(66).SetSide(Pole.South)
                };
            }
        }

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            BlockPos blockPosUp = blockPos.OffsetUp();
            BlockState blockStateUp = world.GetBlockState(blockPosUp);
            BlockBase blockUp = blockStateUp.GetBlock();
            if (!blockUp.Material.TurfDoesNotDry || blockStateUp.lightSky < 7 || blockUp.LightOpacity > 2)
            {
                // Засыхание дёрна
                world.SetBlockState(blockPos, new BlockState(EnumBlock.Dirt), 14);
            }
            else
            {
                if (blockStateUp.lightSky >= 9)
                {
                    EnumTimeYear enumTimeYear = world.GetTimeYear();
                    if (enumTimeYear == EnumTimeYear.Summer && blockStateUp.IsAir() 
                        && world.GetMoonPhase() == EnumMoonPhase.NewMoon && random.Next(32) == 0)
                    {
                        // Только летом в новолунее растёт трава 
                        int rnd = random.Next(20);
                        EnumBlock enumBlock = EnumBlock.Grass;
                        if (rnd == 0) enumBlock = EnumBlock.FlowerClover;
                        else if (rnd == 1) enumBlock = EnumBlock.FlowerDandelion;
                        world.SetBlockState(blockPosUp, new BlockState(enumBlock), 12);
                    }
                    if (enumTimeYear == EnumTimeYear.Summer || enumTimeYear == EnumTimeYear.Spring)
                    {
                        // Распространение дёрна
                        BlockPos blockPos2;
                        EnumBlock enumBlock2;
                        BlockState blockState2;
                        BlockState blockState2up;
                        BlockBase blockBase2up;
                        BlockPos blockPos2up;

                        for (int i = 0; i < 4; i++)
                        {
                            blockPos2 = blockPos.Offset(random.Next(3) - 1, random.Next(5) - 3, random.Next(3) - 1);
                            blockPos2up = blockPos2.OffsetUp();
                            blockState2up = world.GetBlockState(blockPos2up);
                            blockBase2up = blockState2up.GetBlock();
                            blockState2 = world.GetBlockState(blockPos2);
                            enumBlock2 = blockState2.GetEBlock();

                            if (enumBlock2 == EnumBlock.Dirt
                                && blockState2up.lightSky >= 7 && blockBase2up.LightOpacity <= 2)
                            {
                                // Дёрн
                                world.SetBlockState(blockPos2, new BlockState(EnumBlock.Turf), 12);
                            }
                            else if (BlockTina.IsGrows(world, blockPos2) && random.Next(8) == 0
                                && enumBlock2 == EnumBlock.Water && blockState2up.lightSky >= 7
                                && blockState2up.IsAir())
                            {
                                // Тина
                                world.SetBlockState(blockPos2up, new BlockState(EnumBlock.Tina), 12);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
            => Items.GetItemCache(EnumItem.PieceDirt);
    }
}
