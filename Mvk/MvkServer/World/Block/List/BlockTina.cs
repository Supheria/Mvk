using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;

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
            Color = new vec3(.56f, .63f, .35f);
            IsCollidable = false;
            SetUnique();
            Material = EnumMaterial.Interior;
            Particle = 208;
            Resistance = .2f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitBoxs();
        }

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
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                worldIn.SetBlockToAir(blockPos, 31);
            }
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos) 
            => worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock() == EnumBlock.Water;

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met) 
            => new AxisAlignedBB[] {
                new AxisAlignedBB(new vec3(pos.X, pos.Y, pos.Z), new vec3(pos.X + 1, pos.Y + .125f, pos.Z + 1)),
            };

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        private void InitBoxs()
        {
            boxes = new Box[1][];

            boxes[0] = new Box[]
            {
                new Box()
                {
                    From = new vec3(0),
                    To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[0], MvkStatic.Xy[16]),
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, Particle, true, Color),
                        new Face(Pole.Down, Particle, true, Color)
                    }
                }
            };
        }

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (world.GetBiome(blockPos) != Biome.EnumBiome.Swamp)
            {
                world.SetBlockToAir(blockPos);
            }
            else
            {
                if (random.Next(8) == 0 && blockState.lightSky >= 9)
                {
                    // Распространение тины
                    BlockPos blockPos2;
                    BlockState blockState2;
                    BlockState blockState2dw;

                    for (int i = 0; i < 4; i++)
                    {
                        blockPos2 = blockPos.Offset(random.Next(3) - 1, random.Next(5) - 3, random.Next(3) - 1);
                        if (world.GetBiome(blockPos2) == Biome.EnumBiome.Swamp)
                        {
                            blockState2dw = world.GetBlockState(blockPos2.OffsetDown());
                            blockState2 = world.GetBlockState(blockPos2);

                            if (blockState2dw.GetEBlock() == EnumBlock.Water && blockState2.IsAir()
                                && blockState2.lightSky >= 7)
                            {
                                world.SetBlockState(blockPos2, new BlockState(EnumBlock.Tina), 12);
                            }
                        }
                    }
                }
                else if (random.Next(64) == 0)
                {
                    // Очень редко тина может исчезнуть
                    world.SetBlockToAir(blockPos);
                }
            }
        }
    }
}
