using MvkServer.Glm;
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
            NeedsRandomTick = true;
            Particle = 65;
            Color = new vec3(.56f, .73f, .35f);
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
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[(xc + zc + xb + zb) & 3];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 colorGreen = Color;
            vec3 colorBrown = new vec3(.62f, .44f, .37f);

            boxes = new Box[4][];
            for (int i = 0; i < 4; i++)
            {
                boxes[i] = new Box[] {
                    new Box()
                    {
                        Faces = new Face[] { new Face(Pole.Up, 65, true, colorGreen) }
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
                            new Face(Pole.East, 66, true, colorGreen),
                            new Face(Pole.North, 66, true, colorGreen),
                            new Face(Pole.South, 66, true, colorGreen),
                            new Face(Pole.West, 66, true, colorGreen)
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
            if (blockStateUp.lightSky < 7 || blockUp.LightOpacity > 2)
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
                    BlockState blockState2;
                    BlockState blockState2up;
                    BlockBase blockBase2up;
                    
                    for (int i = 0; i < 4; i++)
                    {
                        blockPos2 = blockPos.Offset(random.Next(3) - 1, random.Next(5) - 3, random.Next(3) - 1);
                        blockState2up = world.GetBlockState(blockPos2.OffsetUp());
                        blockBase2up = blockState2up.GetBlock();
                        blockState2 = world.GetBlockState(blockPos2);

                        if (blockState2.GetEBlock() == EnumBlock.Dirt 
                            && blockState2up.lightSky >= 7 && blockBase2up.LightOpacity <= 2)
                        {
                            world.SetBlockState(blockPos2, new BlockState(EnumBlock.Turf), 12);
                        }
                    }
                }
            }
        }
    }
}
