using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Лампа включена
    /// </summary>
    public class BlockElLampOn : BlockAbElectricity
    {
        /// <summary>
        /// Блок Лампа включена
        /// </summary>
        public BlockElLampOn() : base(386)
        {
            LightValue = 15;
            tickRate = 50;
            InitBoxs(386, false, new vec3(1));
        }

        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            worldIn.SetBlockTick(blockPos, tickRate);
        }

        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            DischargeTick(world, blockPos);
            world.SetBlockTick(blockPos, tickRate);
        }

        /// <summary>
        /// Блок отключен от электроэнергии
        /// </summary>
        public override void UnitDisconnectedFromElectricity(WorldBase world, BlockPos blockPos)
        {
            world.SetBlockState(blockPos, new BlockState(EnumBlock.ElLampOff), 12);
        }
        /// <summary>
        /// Имеется ли у блока подключение к электроэнергии
        /// </summary>
        public override bool IsUnitConnectedToElectricity() => true;
        /// <summary>
        /// Имеется ли у блока разрядка к электроэнергии
        /// </summary>
        public override bool IsUnitDischargeToElectricity() => true;
    }
}
