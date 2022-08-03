using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект листвы
    /// </summary>
    public abstract class BlockAbLeaves : BlockBase
    {
        public BlockAbLeaves(int numberTexture)
        {
            Material = EnumMaterial.Leaves;
            LightOpacity = 2;
            UseNeighborBrightness = true;
            AllSideForcibly = true;
            АmbientOcclusion = false;
            Color = new vec3(.56f, .73f, .35f);
            Particle = numberTexture;
            InitBoxs();
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        {
            int met = state.Met();
            int metNew = 1;
            for (int i = 0; i < 6; i++)
            {
                if (worldIn.GetBlockState(blockPos.Offset(i)).GetEBlock() == EnumBlock.Air)
                {
                    metNew = 0;
                    break;
                }
            }
            if (metNew != met)
            {
                worldIn.SetBlockStateMet(blockPos, metNew); 
            }
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

        /// <summary>
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public override bool BlocksNotSame(int met)
        {
            return met == 0;
        }

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
    }
}
