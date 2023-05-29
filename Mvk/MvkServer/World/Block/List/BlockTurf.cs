using MvkServer.Glm;
using MvkServer.Item;
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
            Color = new vec3(1);
            Material = EnumMaterial.Loose;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepGrass1, AssetsSample.StepGrass2, AssetsSample.StepGrass3, AssetsSample.StepGrass4 };
            InitBoxs();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 10;

        /// <summary>
        /// Цвет блока для подмешенных для гуи
        /// </summary>
        public override vec3 ColorGui() => new vec3(.56f, .73f, .35f);

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[(xc + zc + xb + zb) & 3];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 colorGreen = new vec3(.56f, .73f, .35f);
            vec3 colorBrown = new vec3(.62f, .44f, .37f);

            boxes = new Box[4][];
            for (int i = 0; i < 4; i++)
            {
                boxes[i] = new Box[] {
                    new Box()
                    {
                        Faces = new Face[] { new Face(Pole.Up, 65, true, colorGreen).SetBiomeColor() }
                    },
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.Down, 64, colorBrown),
                            new Face(Pole.East, 67),
                            new Face(Pole.North, 67),
                            new Face(Pole.South, 67),
                            new Face(Pole.West, 67)
                        }
                    },
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.East, 66, true, colorBrown).SetBiomeColor(),
                            new Face(Pole.North, 66, true, colorBrown).SetBiomeColor(),
                            new Face(Pole.South, 66, true, colorBrown).SetBiomeColor(),
                            new Face(Pole.West, 66, true, colorBrown).SetBiomeColor()
                        }
                    }
                };
            }
            boxes[1][0].RotateYawUV = 1;
            boxes[2][0].RotateYawUV = 2;
            boxes[3][0].RotateYawUV = 3;
        }

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            BlockPos blockPosUp = blockPos.OffsetUp();
            BlockState blockStateUp = world.GetBlockState(blockPosUp);
            BlockBase blockUp = blockStateUp.GetBlock();
            if (!(blockUp.IsAir || blockUp.Material == EnumMaterial.Sapling || blockUp.Material == EnumMaterial.Piece) || blockStateUp.lightSky < 7 || blockUp.LightOpacity > 2)
            {
                // Засыхание дёрна
                world.SetBlockState(blockPos, new BlockState(EnumBlock.Dirt), 14);
            }
            else
            {
                if (blockStateUp.lightSky >= 9)
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

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        public override ItemBase GetItemDropped(BlockState state, Rand rand, int fortune)
            => Items.GetItemCache(EnumItem.PieceDirt);
    }
}
