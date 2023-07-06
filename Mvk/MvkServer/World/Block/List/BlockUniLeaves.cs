using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект листвы
    /// </summary>
    public class BlockUniLeaves : BlockBase
    {
        /***
         * Met
         * 0-5 сторона
         * +6 второй вид
         */

        /// <summary>
        /// Может ли у этой листвы быть плод
        /// </summary>
        private readonly bool fetus;
        /// <summary>
        /// Блок древисины к которому принадлежит листва
        /// </summary>
        protected readonly EnumBlock eBlockLog;

        public BlockUniLeaves(int numberTexture, EnumBlock eBlockLog, bool fetus = false)
        {
            this.fetus = fetus;
            this.eBlockLog = eBlockLog;
            Material = EnumMaterial.Leaves;
            UseNeighborBrightness = true;
            IsCollidable = false;
            NeedsRandomTick = fetus;
            SetUnique(true);
            LightOpacity = 2;
            Particle = numberTexture;
            Combustibility = true;
            IgniteOddsSunbathing = 30;
            BurnOdds = 60;
            Resistance = .2f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitQuads();
        }

        /// <summary>
        /// Блок который замедляет сущность в перемещении на ~30%
        /// </summary>
        public override bool IsSlow(BlockState state) => true;

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        public override int QuantityDropped(Rand random) => 0;

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met];

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            ushort met = (ushort)side;
            if (side == Pole.Up || side == Pole.Down)
            {
                if (facing.x < .5f) met += 6;
            }
            else
            {
                if (facing.y < .5f) met += 6;
            }
            return state.NewMet(met);
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos, neighborState.met))
            {
                DropBlockAsItem(worldIn, blockPos, neighborState, 0);
                worldIn.SetBlockToAir(blockPos, 15);
            }
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            if (met > 5) met -= 6;
            return worldIn.GetBlockState(blockPos.OffsetBack(met)).GetEBlock() == eBlockLog;
        }

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (fetus)
            {
                if (world.GetMoonPhase() == EnumMoonPhase.NewMoon && random.Next(16) == 0)
                {
                    if (EBlock == EnumBlock.LeavesFruit && world.GetTimeYear() == EnumTimeYear.Summer)
                    {
                        // Яблочки растут только летом в полнолунию
                        BlockPos blockPosDown = blockPos.OffsetDown();
                        if (world.GetBlockState(blockPosDown).IsAir())
                        {
                            // появляется яблоко
                            world.SetBlockState(blockPosDown, new BlockState(EnumBlock.Apple), 12);
                        }
                    }
                    else if (EBlock == EnumBlock.LeavesPalm)
                    {
                        // Кокосы растут в полнолунию
                        BlockPos blockPosDown = blockPos.OffsetDown();
                        if (world.GetBlockState(blockPosDown).IsAir())
                        {
                            for (int i = 2; i < 6; i++)
                            {
                                if (world.GetBlockState(blockPosDown.Offset(i)).GetEBlock() == EnumBlock.LogPalm)
                                {
                                    // появляется кокос
                                    world.SetBlockState(blockPosDown, new BlockState(EnumBlock.Coconut), 12);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected virtual void InitQuads()
        {
            int texture1, texture2, index;
            quads = new QuadSide[12][];
            for (int i = 0; i < 2; i++)
            {
                texture1 = Particle + i * 3;
                texture2 = texture1 + 192;
                index = i * 6;
                // Up
                quads[index] = new QuadSide[] {
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48, 1).SetSide(Pole.East, true, 8, 0, -16, 8, 48, 32).SetRotate(-.7f, 0, -.26f).Wind(),
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48, 1).SetSide(Pole.East, true, 8, 0, -16, 8, 48, 32).SetRotate(.8f, 0, .21f).Wind(),
                };
                // Down
                quads[index + 1] = new QuadSide[] {
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48, 3).SetSide(Pole.East, true, 8, -32, -16, 8, 16, 32).SetRotate(-.13f, 0, -.16f).Wind(2),
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48, 3).SetSide(Pole.North, true, -16, -32, 8, 32, 16, 8).SetRotate(.12f, 0, .17f).Wind(2),
                };
                // East
                quads[index + 2] = new QuadSide[] {
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(-.42f, -.1f).Wind(),
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(.42f, .1f).Wind(),
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48).SetSide(Pole.Down, true, 0, 8, -16, 48, 8, 32).SetRotate(0, .3f).Wind()
                };
                // West
                quads[index + 3] = new QuadSide[] {
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(2.7f, -.08f).Wind(),
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(3.58f, .08f).Wind(),
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48).SetSide(Pole.Down, true, 0, 8, -16, 48, 8, 32).SetRotate(glm.pi, .3f).Wind()
                };
                // North
                quads[index + 4] = new QuadSide[] {
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(1.16f, -.1f).Wind(),
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(2f, .1f).Wind(),
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48).SetSide(Pole.Down, true, 0, 8, -16, 48, 8, 32).SetRotate(glm.pi90, .3f).Wind()
                };
                // South
                quads[index + 5] = new QuadSide[] {
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(4.28f, -.1f).Wind(),
                    new QuadSide(0).SetTexture(texture2, 0, 0, 48, 48).SetSide(Pole.South, true, 0, -16, 8, 48, 32, 8).SetRotate(5.12f, .1f).Wind(),
                    new QuadSide(0).SetTexture(texture1, 0, 0, 48, 48).SetSide(Pole.Down, true, 0, 8, -16, 48, 8, 32).SetRotate(glm.pi270, .3f).Wind()
                };
            }
        }
    }
}
