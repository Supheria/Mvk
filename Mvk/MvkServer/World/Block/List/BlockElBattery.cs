using MvkServer.Glm;
using MvkServer.Util;
using System.Collections.Generic;

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
            InitBoxs();
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
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb)
        {
            //int index = (met + 3) / 4; // 1-15
            int index = (met + 63) / 64; // 1-255
            return boxes[index];
        }

        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override Box[] GetBoxesGui() => boxes[4];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[][] {
                new Box[] { new Box(392, false, new vec3(1)) },
                new Box[] { new Box(391, false, new vec3(1)) },
                new Box[] { new Box(390, false, new vec3(1)) },
                new Box[] { new Box(389, false, new vec3(1)) },
                new Box[] { new Box(388, false, new vec3(1)) }
            };
        }
    }
}
