using MvkServer.Util;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок саженца берёза
    /// </summary>
    public class BlockSaplingBirch : BlockAbSapling
    {
        /// <summary>
        /// Блок саженца берёза
        /// </summary>
        public BlockSaplingBirch() : base(139) => NeedsRandomTick = true;

        /// <summary>
        /// Генерация дерева
        /// </summary>
        protected override void GenefateTree(WorldBase world, Rand rand, BlockPos blockPos)
            => new WorldGenTreeBirch2().GenefateTree(world, rand, blockPos);
    }
}
