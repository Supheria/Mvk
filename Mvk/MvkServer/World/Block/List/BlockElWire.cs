using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок провод
    /// </summary>
    public class BlockElWire : BlockAbElectricity
    {
        /**
         * Met
         * 0 - нет сигнала к батареи
         * 1 - 15 - есть сигнал к батареи, чем цифра больше чем ближе к батареи
         */ 

        /// <summary>
        /// Блок провод
        /// </summary>
        public BlockElWire() : base(384)
        {
            InitQuads();
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met == 0 ? 0 : 1];

        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override QuadSide[] GetQuadsGui() => quads[1];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads()
        {
            quads = new QuadSide[2][];
            int texture;
            for (int i = 0; i < 2; i++)
            {
                texture = 385 - i;
                quads[i] = new QuadSide[6];
                for (int j = 0; j < 6; j++)
                {
                    quads[i][j] = new QuadSide(0).SetTexture(texture).SetSide((Pole)j);
                }
            }
        }
    }
}
