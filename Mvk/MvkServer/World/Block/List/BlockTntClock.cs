using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок TNT с часами
    /// </summary>
    public class BlockTntClock : BlockTnt
    {
        /// <summary>
        /// Блок TNT с часами
        /// </summary>
        public BlockTntClock() : base()
        {
            power = 6f;
            distance = 1f;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitQuads() => InitQuads(515, 512, 513, 513, 513, 513);

        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            base.OnBlockAdded(worldIn, blockPos, state);

            if (worldIn.GetBlockState(blockPos).GetEBlock() == EnumBlock.TntClock)
            {
                // запустить мгновенный тик 1-2 сек до взрыва
                worldIn.SetBlockTick(blockPos, (uint)(worldIn.Rnd.Next(20) + 20));
            }
        }

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            Explosion(world, blockPos);
        }
    }
}
