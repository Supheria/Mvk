using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет высокая трава
    /// </summary>
    public class ItemTallGrass : ItemBlock
    {
        public ItemTallGrass(EnumItem enumItem, int numberTexture) : base(Blocks.GetBlockCache(EnumBlock.TallGrass))
        {
            EItem = enumItem;
            NumberTexture = numberTexture;
            UpId();
        }

        public override string GetName() => "item." + EItem;

        /// <summary>
        /// Установить дополнительные блоки
        /// </summary>
        protected override void InstallAdditionalBlocks(WorldBase worldIn, BlockPos blockPos)
        {
            if (!worldIn.IsRemote)
            {
                int height = worldIn.Rnd.Next(4) + 1;
                BlockPos blockPos2;
                BlockState blockState2;
                for (int y = 0; y < height; y++)
                {
                    blockPos2 = blockPos.Offset(0, y + 1, 0);
                    blockState2 = worldIn.GetBlockState(blockPos2);
                    if (blockState2.IsAir())
                    {
                        worldIn.SetBlockState(blockPos2, new BlockState(EnumBlock.TallGrass, 1), 12);
                        worldIn.SetBlockStateMet(blockPos2.OffsetDown(), 0);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
