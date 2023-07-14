using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет ведро с лавой
    /// </summary>
    public class ItemBucketLava : ItemAbBucketLiquid
    {
        public ItemBucketLava() : base(EnumItem.BucketLava, 98)
        {
            BlockLiquid = Blocks.GetBlockCache(EnumBlock.Lava);
        }
    }
}
