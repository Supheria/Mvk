using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Батарея
    /// </summary>
    public class BlockElBattery : BlockAbElectricity
    {
        /**
         * Met
         * 0 - нет мощности
         * 1 - 255 количество мощности
         */
         
        /// <summary>
        /// Блок Батарея
        /// </summary>
        public BlockElBattery() : base(392)
        {
            IsAddMet = true;
            Resistance = 50;
            InitQuads();
        }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            int met = 255;
            return state.NewMet((ushort)met);
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb)
        {
            //int index = (met + 3) / 4; // 1-15
            int index = (met + 63) / 64; // 1-255
            return quads[index];
        }

        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override QuadSide[] GetQuadsGui() => quads[4];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads()
        {
            quads = new QuadSide[5][];
            int texture;
            for (int i = 0; i < 5; i++)
            {
                texture = 392 - i;
                quads[i] = new QuadSide[6];
                for (int j = 0; j < 6; j++)
                {
                    quads[i][j] = new QuadSide(0).SetTexture(texture).SetSide((Pole)j);
                }
            }
        }
    }
}
