using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Выключатель
    /// </summary>
    public class BlockElSwitch : BlockAbElectricity
    {
        /***
         * Met
         * 0 - 3 bit - Power (0 - 15)
         * 4 bit - Switch
         * 
         * power = met & 0xF
         * 0 - нет сигнала к батареи
         * 1 - 15 - есть сигнал к батареи, чем цифра больше чем ближе к батареи
         * 
         * switch = met >> 4
         * 0 = выключен
         * 1 = включен
         *
         * met = switch << 4 | power & 0xF
         */

        /// <summary>
        /// Блок Выключатель
        /// </summary>
        public BlockElSwitch() : base(394)
        {
            IsAddMet = true;
            InitBoxs();
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb)
        {
            int index = (met & 0xF) == 0 ? 0 : (met >> 4) + 1;
            return boxes[index];
        }

        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override Box[] GetBoxesGui() => boxes[2];

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            int power = state.met & 0xF;
           // if (power > 0)
            {
                int button = state.met >> 4;
                int buttonNew = button == 1 ? 0 : 1;
                int met = buttonNew << 4 | power & 0xF;
                worldIn.SetBlockStateMet(pos, (ushort)met, true);
                if (!worldIn.IsRemote)
                {
                    worldIn.PlaySound(Sound.AssetsSample.Click, pos.ToVec3() + .5f, .3f, button == 1 ? .6f : .5f);
                    // worldIn.SetBlockTick(pos, 20, false);
                }
                UpdateSurroundingElectricity(worldIn, pos, state);
            }
            return true;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[][] {
                new Box[] { new Box(395, false, new vec3(1)) },
                new Box[] { new Box(394, false, new vec3(1)) },
                new Box[] { new Box(393, false, new vec3(1)) }
            };
        }

        //public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        //{
        //    world.SetBlockStateMet(blockPos, 1, true);
        //    world.PlaySound(Sound.AssetsSample.Click, blockPos.ToVec3() + .5f, .3f, .5f);
        //}
    }
}
