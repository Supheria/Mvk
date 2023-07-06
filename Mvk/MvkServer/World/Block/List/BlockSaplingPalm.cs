using MvkServer.Util;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок саженца пальмы
    /// </summary>
    public class BlockSaplingPalm : BlockAbSapling
    {
        /// <summary>
        /// Блок саженца пальмы
        /// </summary>
        public BlockSaplingPalm() : base(161) => NeedsRandomTick = true;

        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met) 
            => worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock() == EnumBlock.Sand;

        /// <summary>
        /// Генерация дерева
        /// </summary>
        protected override void GenefateTree(WorldBase world, Rand rand, BlockPos blockPos)
            => new WorldGenTreePalm2().GenefateTree(world, rand, blockPos);
    }
}
