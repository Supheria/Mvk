using MvkServer.Util;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок саженца дуба
    /// </summary>
    public class BlockSaplingOak : BlockAbSapling
    {
        /// <summary>
        /// Блок саженца дуба
        /// </summary>
        public BlockSaplingOak() : base(132) => NeedsRandomTick = true;

        /// <summary>
        /// Генерация дерева
        /// </summary>
        protected override void GenefateTree(WorldBase world, Rand rand, BlockPos blockPos) 
            => new WorldGenTreeOak2().GenefateTree(world, rand, blockPos);
    }
}
