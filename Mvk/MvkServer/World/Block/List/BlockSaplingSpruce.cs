using MvkServer.Util;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок саженца ель
    /// </summary>
    public class BlockSaplingSpruce : BlockAbSapling
    {
        /// <summary>
        /// Блок саженца ель
        /// </summary>
        public BlockSaplingSpruce() : base(146) => NeedsRandomTick = true;

        /// <summary>
        /// Генерация дерева
        /// </summary>
        protected override void GenefateTree(WorldBase world, Rand rand, BlockPos blockPos)
            => new WorldGenTreeSpruce().GenefateTree(world, rand, blockPos);
    }
}
