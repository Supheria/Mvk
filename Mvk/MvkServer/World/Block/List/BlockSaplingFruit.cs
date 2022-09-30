using MvkServer.Util;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок саженца плодовое
    /// </summary>
    public class BlockSaplingFruit : BlockAbSapling
    {
        /// <summary>
        /// Блок саженца плодовое
        /// </summary>
        public BlockSaplingFruit() : base(153) => NeedsRandomTick = true;

        /// <summary>
        /// Генерация дерева
        /// </summary>
        protected override void GenefateTree(WorldBase world, Rand rand, BlockPos blockPos)
            => new WorldGenTreeFruit().GenefateTree(world, rand, blockPos);
    }
}
