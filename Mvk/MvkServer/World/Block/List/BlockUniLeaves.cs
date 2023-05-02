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
        public BlockUniLeaves(int numberTexture)
        {
            Material = EnumMaterial.Leaves;
            IsCollidable = false;
            SetUnique();
            LightOpacity = 2;
            //UseNeighborBrightness = true;
            //AllSideForcibly = true;
            //АmbientOcclusion = false;
            BiomeColor = true;
            Color = new vec3(.56f, .73f, .35f);
            Particle = numberTexture;
            Combustibility = true;
            IgniteOddsSunbathing = 30;
            BurnOdds = 60;
            Resistance = .2f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitBoxs();
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        //public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        //{
        //    int met = state.Met();
        //    int metNew = 1;
        //    for (int i = 0; i < 6; i++)
        //    {
        //        if (worldIn.GetBlockState(blockPos.Offset(i)).GetEBlock() == EnumBlock.Air)
        //        {
        //            metNew = 0;
        //            break;
        //        }
        //    }
        //    if (metNew != met)
        //    {
        //        worldIn.SetBlockStateMet(blockPos, metNew); 
        //    }
        //}

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        public override int QuantityDropped(Rand random) => 0;

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

        /// <summary>
        /// Флаг отличия, для рендера прорисовки однотипных материалов, пример листва чёрная контачит с листвой прозрачной
        /// </summary>
        public override bool FlagDifference(int met)
        {
            return met == 0;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[][] {
                new Box[] {
                    new Box(Particle, true, Color)
                },
                new Box[] {
                    new Box(Particle + 1, true, Color)
                }
            };
        }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;
    }
}
