using MvkServer.Glm;

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
            InitBoxs();
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met == 0 ? 0 : 1];

        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override Box[] GetBoxesGui() => boxes[1];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[][] {
                new Box[] { new Box(385, false, new vec3(1)) },
                new Box[] { new Box(384, false, new vec3(1)) }
            };
        }
    }
}
