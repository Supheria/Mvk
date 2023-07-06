using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Лампа выключена
    /// </summary>
    public class BlockElLampOff : BlockAbElectricity
    {
        /// <summary>
        /// Блок Лампа выключена
        /// </summary>
        public BlockElLampOff() : base(387)
        {
            InitQuads(387);
        }

        /// <summary>
        /// Блок подключен к электроэнергии
        /// </summary>
        public override void UnitConnectedToElectricity(WorldBase world, BlockPos blockPos)
        {
            world.SetBlockState(blockPos, new BlockState(EnumBlock.ElLampOn), 12);
        }
        /// <summary>
        /// Имеется ли у блока подключение к электроэнергии
        /// </summary>
        public override bool IsUnitConnectedToElectricity() => true;
    }
}
